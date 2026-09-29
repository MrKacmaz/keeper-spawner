using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace KeeperSpawner
{
    internal sealed class SpawnerWindow
    {
        private const int WindowId = 0x4B53_5057; // "KSPW"
        private const float Width = 460f;
        private const float ListHeight = 440f;
        private const float RowHeight = 26f;
        private const float StatusSeconds = 3f;

        private readonly ConfigEntry<bool> showQuestItems;
        private readonly ConfigEntry<float> uiScale;

        private Rect windowRect = new Rect(0, 0, Width, 0);
        private bool positioned;

        private string search = string.Empty;
        private string filteredSearch;
        private bool filteredShowQuest;
        private IReadOnlyList<CatalogEntry> filteredSource;
        private readonly List<CatalogEntry> filtered = new List<CatalogEntry>();
        private Vector2 scroll;
        private int visibleFirst;
        private int visibleLast;

        private string status;
        private float statusUntil;

        private GUIStyle rowStyle;
        private GUIStyle rightLabelStyle;
        private GUIStyle hintStyle;

        public SpawnerWindow(ConfigEntry<bool> showQuestItems, ConfigEntry<float> uiScale)
        {
            this.showQuestItems = showQuestItems;
            this.uiScale = uiScale;
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
            filteredSource = null; // katalog ya da dil değişmiş olabilir
            InputBlocker.Suspend();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }
            IsOpen = false;
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
                windowRect.y = Mathf.Max(20f, (screenH - ListHeight - 160f) / 2f);
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
            // Arama satırı
            GUILayout.BeginHorizontal();
            GUILayout.Label(Strings.Search, GUILayout.ExpandWidth(false));
            search = GUILayout.TextField(search ?? string.Empty, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(Strings.Clear, GUILayout.ExpandWidth(false)))
            {
                search = string.Empty;
                GUIUtility.keyboardControl = 0;
            }
            GUILayout.EndHorizontal();

            bool showQuest = GUILayout.Toggle(showQuestItems.Value, " " + Strings.ShowQuestItems);
            if (showQuest != showQuestItems.Value)
            {
                showQuestItems.Value = showQuest;
            }

            // GUILayout, Layout geçişiyle sonraki olayda aynı kontrolleri ister;
            // liste yapısını sadece Layout'ta değiştiriyoruz
            var all = ItemCatalog.All;
            if (Event.current.type == EventType.Layout)
            {
                RefreshFilter(all);
            }

            GUILayout.Label(Strings.ItemCount(filtered.Count, all.Count), hintStyle);

            DrawList();

            // Durum ve ipucu satırı
            bool hasStatus = !string.IsNullOrEmpty(status) && Time.unscaledTime < statusUntil;
            GUILayout.Label(hasStatus ? status : Strings.Hint, hintStyle);
        }

        private void DrawList()
        {
            scroll = GUILayout.BeginScrollView(scroll, false, true, GUILayout.Height(ListHeight));

            if (filtered.Count == 0)
            {
                GUILayout.Label(Strings.NoResults, hintStyle);
            }
            else
            {
                // Sadece görünen satırları çiz, geri kalanı boşlukla doldur (800+ buton her karede pahalı).
                // Aralık sadece Layout'ta hesaplanır; kaydırma olayı ortasında satır sayısı değişmesin.
                if (Event.current.type == EventType.Layout)
                {
                    visibleFirst = Mathf.Clamp((int)(scroll.y / RowHeight), 0, filtered.Count - 1);
                    int visible = Mathf.CeilToInt(ListHeight / RowHeight) + 2;
                    visibleLast = Mathf.Min(filtered.Count, visibleFirst + visible);
                }
                int first = Mathf.Min(visibleFirst, filtered.Count);
                int last = Mathf.Min(visibleLast, filtered.Count);

                GUILayout.Space(first * RowHeight);
                for (int i = first; i < last; i++)
                {
                    DrawRow(filtered[i]);
                }
                GUILayout.Space((filtered.Count - last) * RowHeight);
            }

            GUILayout.EndScrollView();
        }

        private void DrawRow(CatalogEntry entry)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(RowHeight));
            string label = entry.IsQuest ? $"{entry.DisplayName}  <color=#e0a040>({Strings.Quest})</color>" : entry.DisplayName;
            label += $"  <color=#888888><size=11>{entry.Id}</size></color>";
            if (GUILayout.Button(label, rowStyle, GUILayout.Height(RowHeight - 2), GUILayout.ExpandWidth(true)))
            {
                OnItemClicked(entry);
            }
            GUILayout.Label("x" + entry.MaxStack, rightLabelStyle, GUILayout.Width(44), GUILayout.Height(RowHeight - 2));
            GUILayout.EndHorizontal();
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

        private void RefreshFilter(IReadOnlyList<CatalogEntry> all)
        {
            bool showQuest = showQuestItems.Value;
            if (ReferenceEquals(all, filteredSource) && search == filteredSearch && showQuest == filteredShowQuest)
            {
                return;
            }
            filteredSource = all;
            filteredSearch = search;
            filteredShowQuest = showQuest;

            string[] terms = ItemCatalog.Fold(search).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            filtered.Clear();
            foreach (var entry in all)
            {
                if (entry.IsQuest && !showQuest)
                {
                    continue;
                }
                if (MatchesAll(entry.SearchKey, terms))
                {
                    filtered.Add(entry);
                }
            }
            scroll = Vector2.zero;
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
            if (rowStyle != null)
            {
                return;
            }
            rowStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                richText = true,
                padding = new RectOffset(8, 8, 2, 2),
            };
            rightLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
            };
            hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                richText = true,
                normal = { textColor = new Color(0.75f, 0.75f, 0.75f) },
            };
        }
    }
}
