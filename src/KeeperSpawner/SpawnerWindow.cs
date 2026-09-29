using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace KeeperSpawner
{
    internal sealed class SpawnerWindow
    {
        private const int WindowId = 0x4B53_5057; // "KSPW"
        private const float Width = 620f;
        private const float ViewHeight = 440f;
        private const int TabsPerRow = 8;
        private const float StatusSeconds = 3f;

        private readonly ConfigEntry<bool> showQuestItems;
        private readonly ConfigEntry<float> uiScale;
        private readonly ConfigEntry<ViewMode> viewMode;
        private readonly ItemView view = new ItemView();

        private Rect windowRect = new Rect(0, 0, Width, 0);
        private bool positioned;

        private string search = string.Empty;
        /// <summary>null = Tümü.</summary>
        private ItemCategory? selectedCategory;

        // Son filtrelemenin girdileri; değişmedikçe yeniden kurulmaz
        private IReadOnlyList<CatalogEntry> builtSource;
        private string builtSearch;
        private bool builtShowQuest;
        private ItemCategory? builtCategory;
        private ViewMode builtMode;
        private float builtContentWidth;

        private readonly List<ItemCategory?> tabs = new List<ItemCategory?>();
        private string[] tabLabels = Array.Empty<string>();
        private readonly List<CatalogEntry> filtered = new List<CatalogEntry>();

        private CatalogEntry hovered;
        private string status;
        private float statusUntil;

        private GUIStyle infoStyle;

        public SpawnerWindow(ConfigEntry<bool> showQuestItems, ConfigEntry<float> uiScale, ConfigEntry<ViewMode> viewMode)
        {
            this.showQuestItems = showQuestItems;
            this.uiScale = uiScale;
            this.viewMode = viewMode;
        }

        public bool IsOpen { get; private set; }

        /// <summary>Bir IMGUI metin alanı klavye odağında mı (arama kutusu).</summary>
        public bool IsTyping => IsOpen && GUIUtility.keyboardControl != 0;

        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void Open()
        {
            if (IsOpen)
            {
                return;
            }
            IsOpen = true;
            builtSource = null; // katalog ya da dil değişmiş olabilir
            InputBlocker.Suspend();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }
            IsOpen = false;
            hovered = null;
            GUIUtility.keyboardControl = 0;
            InputBlocker.Restore();
        }

        public void OnGUI()
        {
            if (!IsOpen)
            {
                return;
            }

            var ev = Event.current;
            if (ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Escape)
            {
                // Önce arama kutusundan çık, ikinci Esc pencereyi kapatır
                if (GUIUtility.keyboardControl != 0)
                {
                    GUIUtility.keyboardControl = 0;
                }
                else
                {
                    Close();
                }
                ev.Use();
                return;
            }

            EnsureStyles();

            float scale = GetScale();
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            float screenW = Screen.width / scale;
            float screenH = Screen.height / scale;
            if (!positioned)
            {
                windowRect.x = (screenW - Width) / 2f;
                windowRect.y = Mathf.Max(20f, (screenH - ViewHeight - 220f) / 2f);
                positioned = true;
            }

            windowRect = GUILayout.Window(WindowId, windowRect, DrawWindow,
                $"{Plugin.Name} {Plugin.Version}", GUILayout.Width(Width));

            // Pencere ekran dışına sürüklenmesin
            windowRect.x = Mathf.Clamp(windowRect.x, 0f, Mathf.Max(0f, screenW - windowRect.width));
            windowRect.y = Mathf.Clamp(windowRect.y, 0f, Mathf.Max(0f, screenH - windowRect.height));

            GUI.matrix = previousMatrix;
        }

        private void DrawWindow(int id)
        {
            try
            {
                DrawContent();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Pencere çizilemedi: {e}");
                SetStatus(Strings.Error);
            }
            GUI.DragWindow(new Rect(0, 0, 10000, 22));
        }

        private void DrawContent()
        {
            float viewWidth = Width - GUI.skin.window.padding.horizontal - 4f;
            var scrollbar = GUI.skin.verticalScrollbar;
            float contentWidth = viewWidth - scrollbar.fixedWidth - scrollbar.margin.horizontal - 4f;

            // Arama ve görünüm satırı
            GUILayout.BeginHorizontal();
            GUILayout.Label(Strings.Search, GUILayout.ExpandWidth(false));
            search = GUILayout.TextField(search ?? string.Empty, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(Strings.Clear, GUILayout.ExpandWidth(false)))
            {
                search = string.Empty;
                GUIUtility.keyboardControl = 0;
            }
            int mode = GUILayout.Toolbar((int)viewMode.Value, new[] { Strings.Grid, Strings.List }, GUILayout.Width(140f));
            if (mode != (int)viewMode.Value)
            {
                viewMode.Value = (ViewMode)mode;
            }
            GUILayout.EndHorizontal();

            bool showQuest = GUILayout.Toggle(showQuestItems.Value, " " + Strings.ShowQuestItems);
            if (showQuest != showQuestItems.Value)
            {
                showQuestItems.Value = showQuest;
            }

            // GUILayout, Layout geçişiyle sonraki olayda aynı kontrolleri ister;
            // sekmeler ve liste sadece Layout'ta yeniden kurulur
            var all = ItemCatalog.All;
            if (Event.current.type == EventType.Layout)
            {
                RebuildIfNeeded(all, contentWidth);
            }

            int currentTab = Mathf.Max(0, tabs.IndexOf(selectedCategory));
            int newTab = GUILayout.SelectionGrid(currentTab, tabLabels, TabsPerRow);
            if (newTab != currentTab && newTab >= 0 && newTab < tabs.Count)
            {
                selectedCategory = tabs[newTab];
            }

            GUILayout.Label(Strings.ItemCount(filtered.Count, all.Count), infoStyle);

            var clicked = view.Draw(viewWidth, ViewHeight, contentWidth, out var hoveredNow);
            if (Event.current.type == EventType.Repaint)
            {
                hovered = hoveredNow;
            }
            if (clicked != null)
            {
                OnItemClicked(clicked);
            }

            GUILayout.Label(InfoText(), infoStyle);
        }

        private string InfoText()
        {
            if (hovered != null)
            {
                string quest = hovered.IsQuest ? $"  <color=#e0a040>({Strings.Quest})</color>" : string.Empty;
                return $"<b>{hovered.DisplayName}</b>{quest}   <color=#8a8a8a>{hovered.Id}  •  x{hovered.MaxStack}  •  {Categories.Name(hovered.Category)}</color>";
            }
            if (!string.IsNullOrEmpty(status) && Time.unscaledTime < statusUntil)
            {
                return status;
            }
            return Strings.Hint;
        }

        private void RebuildIfNeeded(IReadOnlyList<CatalogEntry> all, float contentWidth)
        {
            bool showQuest = showQuestItems.Value;
            var mode = viewMode.Value;
            if (ReferenceEquals(all, builtSource) && search == builtSearch && showQuest == builtShowQuest
                && selectedCategory == builtCategory && mode == builtMode && Mathf.Approximately(contentWidth, builtContentWidth))
            {
                return;
            }

            // Sekmeler: görünür itemı olan kategoriler (aramadan bağımsız, yazarken sekmeler zıplamasın)
            var present = new bool[Categories.DisplayOrder.Length];
            foreach (var entry in all)
            {
                if (!entry.IsQuest || showQuest)
                {
                    present[(int)entry.Category] = true;
                }
            }
            tabs.Clear();
            tabs.Add(null);
            foreach (var category in Categories.DisplayOrder)
            {
                if (present[(int)category])
                {
                    tabs.Add(category);
                }
            }
            if (selectedCategory.HasValue && !present[(int)selectedCategory.Value])
            {
                selectedCategory = null;
            }
            tabLabels = new string[tabs.Count];
            for (int i = 0; i < tabs.Count; i++)
            {
                tabLabels[i] = tabs[i].HasValue ? Categories.Name(tabs[i].Value) : Strings.All;
            }

            // Filtre
            string[] terms = ItemCatalog.Fold(search).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var matches = new List<CatalogEntry>();
            foreach (var entry in all)
            {
                if (entry.IsQuest && !showQuest)
                {
                    continue;
                }
                if (selectedCategory.HasValue && entry.Category != selectedCategory.Value)
                {
                    continue;
                }
                if (MatchesAll(entry.SearchKey, terms))
                {
                    matches.Add(entry);
                }
            }

            // "Tümü" sekmesinde kategori bölümlerine ayır; katalog ada göre sıralı olduğu için
            // kategori kovalarına sırayla dağıtmak bölüm içi alfabetik sırayı korur
            bool grouped = !selectedCategory.HasValue;
            filtered.Clear();
            if (grouped)
            {
                var buckets = new List<CatalogEntry>[Categories.DisplayOrder.Length];
                foreach (var entry in matches)
                {
                    int index = (int)entry.Category;
                    (buckets[index] ?? (buckets[index] = new List<CatalogEntry>())).Add(entry);
                }
                foreach (var bucket in buckets)
                {
                    if (bucket != null)
                    {
                        filtered.AddRange(bucket);
                    }
                }
            }
            else
            {
                filtered.AddRange(matches);
            }

            view.Rebuild(filtered, grouped, mode, contentWidth);

            builtSource = all;
            builtSearch = search;
            builtShowQuest = showQuest;
            builtCategory = selectedCategory;
            builtMode = mode;
            builtContentWidth = contentWidth;
        }

        private void OnItemClicked(CatalogEntry entry)
        {
            if (!GameState.IsInGame)
            {
                return;
            }
            GUIUtility.keyboardControl = 0;
            int requested = entry.MaxStack;
            try
            {
                int added = Spawner.Spawn(entry, requested);
                if (added <= 0)
                {
                    SetStatus(Strings.InventoryFull);
                }
                else if (added < requested)
                {
                    SetStatus(Strings.Partial(added, requested, entry.DisplayName));
                }
                else
                {
                    SetStatus(Strings.Added(added, entry.DisplayName));
                }
                Plugin.Log.LogInfo($"Spawn {entry.Id}: istenen {requested}, eklenen {added}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Spawn başarısız [{entry.Id}]: {e}");
                SetStatus(Strings.Error);
            }
        }

        private static bool MatchesAll(string key, string[] terms)
        {
            foreach (var term in terms)
            {
                if (key.IndexOf(term, StringComparison.Ordinal) < 0)
                {
                    return false;
                }
            }
            return true;
        }

        private void SetStatus(string text)
        {
            status = text;
            statusUntil = Time.unscaledTime + StatusSeconds;
        }

        private float GetScale()
        {
            float configured = uiScale.Value;
            if (configured > 0f)
            {
                return Mathf.Clamp(configured, 0.5f, 4f);
            }
            return Mathf.Clamp(Screen.height / 1080f, 1f, 3f);
        }

        private void EnsureStyles()
        {
            if (infoStyle != null)
            {
                return;
            }
            infoStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                richText = true,
                normal = { textColor = new Color(0.8f, 0.8f, 0.8f) },
            };
        }
    }
}
