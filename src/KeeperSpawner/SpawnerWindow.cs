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
        private const int TabsPerRow = 9;
        private const float StatusSeconds = 3f;

        private enum TabKind
        {
            All,
            Favorites,
            Recent,
            Category,
        }

        private struct Tab : IEquatable<Tab>
        {
            public TabKind Kind;
            public ItemCategory Category;

            public static Tab Of(TabKind kind) => new Tab { Kind = kind };
            public static Tab Of(ItemCategory category) => new Tab { Kind = TabKind.Category, Category = category };

            public bool Equals(Tab other) => Kind == other.Kind && (Kind != TabKind.Category || Category == other.Category);
        }

        private readonly ConfigEntry<bool> showQuestItems;
        private readonly ConfigEntry<float> uiScale;
        private readonly ConfigEntry<ViewMode> viewMode;
        private readonly ConfigEntry<float> backgroundOpacity;
        private readonly IdList favorites;
        private readonly IdList recent;
        private readonly ItemView view = new ItemView();

        private Rect windowRect = new Rect(0, 0, Width, 0);
        private bool positioned;

        private string search = string.Empty;
        private Tab selectedTab = Tab.Of(TabKind.All);

        // Son kurulumun girdileri; değişmedikçe yeniden kurulmaz
        private IReadOnlyList<CatalogEntry> builtSource;
        private string builtSearch;
        private bool builtShowQuest;
        private Tab builtTab;
        private ViewMode builtMode;
        private float builtContentWidth;
        private int builtFavoritesVersion = -1;
        private int builtRecentVersion = -1;

        private readonly Dictionary<string, CatalogEntry> byId = new Dictionary<string, CatalogEntry>();
        private readonly List<Tab> tabs = new List<Tab>();
        private string[] tabLabels = Array.Empty<string>();
        private int shownCount;

        private CatalogEntry hovered;
        private string status;
        private float statusUntil;

        private GUIStyle infoStyle;
        private GUIStyle tabStyle;

        public SpawnerWindow(ConfigEntry<bool> showQuestItems, ConfigEntry<float> uiScale, ConfigEntry<ViewMode> viewMode,
            ConfigEntry<float> backgroundOpacity, IdList favorites, IdList recent)
        {
            this.showQuestItems = showQuestItems;
            this.uiScale = uiScale;
            this.viewMode = viewMode;
            this.backgroundOpacity = backgroundOpacity;
            this.favorites = favorites;
            this.recent = recent;
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
            var windowStyle = Theme.Window(backgroundOpacity.Value);

            float scale = GetScale();
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            float screenW = Screen.width / scale;
            float screenH = Screen.height / scale;
            if (!positioned)
            {
                windowRect.x = (screenW - Width) / 2f;
                windowRect.y = Mathf.Max(20f, (screenH - ViewHeight - 240f) / 2f);
                positioned = true;
            }

            windowRect = GUILayout.Window(WindowId, windowRect, DrawWindow, GUIContent.none, windowStyle, GUILayout.Width(Width));

            // Pencere ekran dışına sürüklenmesin
            windowRect.x = Mathf.Clamp(windowRect.x, 0f, Mathf.Max(0f, screenW - windowRect.width));
            windowRect.y = Mathf.Clamp(windowRect.y, 0f, Mathf.Max(0f, screenH - windowRect.height));

            GUI.matrix = previousMatrix;
        }

        private void DrawWindow(int id)
        {
            Theme.DrawTitle(windowRect.width, $"{Plugin.Name} {Plugin.Version}");
            try
            {
                DrawContent();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Pencere çizilemedi: {e}");
                SetStatus(Strings.Error);
            }
            GUI.DragWindow(new Rect(0, 0, 10000, Theme.TitleHeight + 4f));
        }

        private void DrawContent()
        {
            float viewWidth = Width - Theme.Window(backgroundOpacity.Value).padding.horizontal;
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

            int currentTab = Mathf.Max(0, tabs.FindIndex(t => t.Equals(selectedTab)));
            int newTab = GUILayout.SelectionGrid(currentTab, tabLabels, TabsPerRow, tabStyle);
            if (newTab != currentTab && newTab >= 0 && newTab < tabs.Count)
            {
                selectedTab = tabs[newTab];
            }

            GUILayout.Label(Strings.ItemCount(shownCount, all.Count), infoStyle);

            var clicked = view.Draw(viewWidth, ViewHeight, contentWidth, e => favorites.Contains(e.Id),
                out var hoveredNow, out var rightClicked);
            if (Event.current.type == EventType.Repaint)
            {
                hovered = hoveredNow;
            }
            if (clicked != null)
            {
                OnItemClicked(clicked);
            }
            if (rightClicked != null)
            {
                ToggleFavorite(rightClicked);
            }

            GUILayout.Label(InfoText(), infoStyle);
        }

        private string InfoText()
        {
            if (hovered != null)
            {
                string favorite = favorites.Contains(hovered.Id) ? $"  <color=#f2c040>{Strings.FavoriteMark}</color>" : string.Empty;
                string quest = hovered.IsQuest ? $"  <color=#e0a040>({Strings.Quest})</color>" : string.Empty;
                string star = hovered.Star > 0 ? $"  <color=#e8c060>{Strings.Star(hovered.Star)}</color>" : string.Empty;
                return $"<b>{hovered.DisplayName}</b>{star}{favorite}{quest}   <color=#8a8a8a>{hovered.Id}  •  x{hovered.MaxStack}  •  {Categories.Name(hovered.Category)}</color>";
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
                && selectedTab.Equals(builtTab) && mode == builtMode && Mathf.Approximately(contentWidth, builtContentWidth)
                && favorites.Version == builtFavoritesVersion && recent.Version == builtRecentVersion)
            {
                return;
            }

            if (!ReferenceEquals(all, builtSource))
            {
                byId.Clear();
                foreach (var entry in all)
                {
                    byId[entry.Id] = entry;
                }
            }

            RebuildTabs(all, showQuest);

            string[] terms = ItemCatalog.Fold(search).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            bool Passes(CatalogEntry entry) => (!entry.IsQuest || showQuest) && MatchesAll(entry.SearchKey, terms);

            var sections = new List<ItemSection>();
            bool showHeaders = false;
            string whenEmpty = Strings.NoResults;

            switch (selectedTab.Kind)
            {
                case TabKind.All:
                {
                    showHeaders = true;
                    sections.Add(new ItemSection { Title = Strings.Favorites, Items = FavoriteEntries(all, Passes) });
                    sections.Add(new ItemSection { Title = Strings.RecentSection, Items = RecentEntries(Passes) });
                    // Katalog ada göre sıralı; kategori kovalarına sırayla dağıtmak bölüm içi sırayı korur
                    var buckets = new List<CatalogEntry>[Categories.DisplayOrder.Length];
                    shownCount = 0;
                    foreach (var entry in all)
                    {
                        if (Passes(entry))
                        {
                            int index = (int)entry.Category;
                            (buckets[index] ?? (buckets[index] = new List<CatalogEntry>())).Add(entry);
                            shownCount++;
                        }
                    }
                    foreach (var category in Categories.DisplayOrder)
                    {
                        var bucket = buckets[(int)category];
                        if (bucket != null)
                        {
                            sections.Add(new ItemSection { Title = Categories.Name(category), Items = bucket });
                        }
                    }
                    break;
                }
                case TabKind.Favorites:
                {
                    var items = FavoriteEntries(all, Passes);
                    sections.Add(new ItemSection { Title = Strings.Favorites, Items = items });
                    shownCount = items.Count;
                    whenEmpty = favorites.Count == 0 ? Strings.NoFavorites : Strings.NoResults;
                    break;
                }
                case TabKind.Recent:
                {
                    var items = RecentEntries(Passes);
                    sections.Add(new ItemSection { Title = Strings.RecentSection, Items = items });
                    shownCount = items.Count;
                    whenEmpty = recent.Count == 0 ? Strings.NoRecent : Strings.NoResults;
                    break;
                }
                default:
                {
                    var items = new List<CatalogEntry>();
                    foreach (var entry in all)
                    {
                        if (entry.Category == selectedTab.Category && Passes(entry))
                        {
                            items.Add(entry);
                        }
                    }
                    sections.Add(new ItemSection { Title = Categories.Name(selectedTab.Category), Items = items });
                    shownCount = items.Count;
                    break;
                }
            }

            view.Rebuild(sections, showHeaders, mode, contentWidth, whenEmpty);

            builtSource = all;
            builtSearch = search;
            builtShowQuest = showQuest;
            builtTab = selectedTab;
            builtMode = mode;
            builtContentWidth = contentWidth;
            builtFavoritesVersion = favorites.Version;
            builtRecentVersion = recent.Version;
        }

        private void RebuildTabs(IReadOnlyList<CatalogEntry> all, bool showQuest)
        {
            // Kategori sekmeleri: görünür itemı olan kategoriler (aramadan bağımsız, yazarken sekmeler zıplamasın)
            var present = new bool[Categories.DisplayOrder.Length];
            foreach (var entry in all)
            {
                if (!entry.IsQuest || showQuest)
                {
                    present[(int)entry.Category] = true;
                }
            }

            tabs.Clear();
            tabs.Add(Tab.Of(TabKind.All));
            tabs.Add(Tab.Of(TabKind.Favorites));
            tabs.Add(Tab.Of(TabKind.Recent));
            foreach (var category in Categories.DisplayOrder)
            {
                if (present[(int)category])
                {
                    tabs.Add(Tab.Of(category));
                }
            }
            if (!tabs.Exists(t => t.Equals(selectedTab)))
            {
                selectedTab = Tab.Of(TabKind.All);
            }

            tabLabels = new string[tabs.Count];
            for (int i = 0; i < tabs.Count; i++)
            {
                tabLabels[i] = TabLabel(tabs[i]);
            }
        }

        private static string TabLabel(Tab tab)
        {
            switch (tab.Kind)
            {
                case TabKind.All: return Strings.All;
                case TabKind.Favorites: return Strings.Favorites;
                case TabKind.Recent: return Strings.Recent;
                default: return Categories.Name(tab.Category);
            }
        }

        /// <summary>Favoriler katalog sırasıyla (ada göre).</summary>
        private List<CatalogEntry> FavoriteEntries(IReadOnlyList<CatalogEntry> all, Func<CatalogEntry, bool> passes)
        {
            var items = new List<CatalogEntry>();
            if (favorites.Count == 0)
            {
                return items;
            }
            foreach (var entry in all)
            {
                if (favorites.Contains(entry.Id) && passes(entry))
                {
                    items.Add(entry);
                }
            }
            return items;
        }

        /// <summary>Son eklenenler en yeniden eskiye; katalogda olmayan (gizli/kaldırılmış) id'ler atlanır.</summary>
        private List<CatalogEntry> RecentEntries(Func<CatalogEntry, bool> passes)
        {
            var items = new List<CatalogEntry>();
            foreach (var id in recent.Ids)
            {
                if (byId.TryGetValue(id, out var entry) && passes(entry))
                {
                    items.Add(entry);
                }
            }
            return items;
        }

        private void ToggleFavorite(CatalogEntry entry)
        {
            favorites.Toggle(entry.Id);
            SetStatus(favorites.Contains(entry.Id)
                ? Strings.FavoriteAdded(entry.DisplayName)
                : Strings.FavoriteRemoved(entry.DisplayName));
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
                else
                {
                    recent.PushFront(entry.Id);
                    SetStatus(added < requested
                        ? Strings.Partial(added, requested, entry.DisplayName)
                        : Strings.Added(added, entry.DisplayName));
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
            tabStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                clipping = TextClipping.Clip,
                padding = new RectOffset(2, 2, 3, 3),
            };
        }
    }
}
