using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeeperSpawner
{
    /// <summary>Orta içerik paneli: filtreleme, bölümler, ızgara/liste ve boş durum.</summary>
    internal sealed partial class SpawnerWindow
    {
        private readonly ItemView view = new ItemView();

        // Son kurulumun girdileri; değişmedikçe yeniden kurulmaz
        private IReadOnlyList<CatalogEntry> builtSource;
        private string builtSearch;
        private bool builtShowQuest;
        private Tab builtTab;
        private ViewMode builtMode;
        private int builtQuality = -1;
        private int builtFavoritesVersion = -1;
        private int builtRecentVersion = -1;

        partial void InvalidateContent()
        {
            builtSource = null;
        }

        partial void RebuildContentIfNeeded()
        {
            var all = ItemCatalog.All;
            bool showQuest = showQuestItems.Value;
            var mode = viewMode.Value;
            if (ReferenceEquals(all, builtSource) && search == builtSearch && showQuest == builtShowQuest
                && selectedTab.Equals(builtTab) && mode == builtMode && qualityFilter == builtQuality
                && favorites.Version == builtFavoritesVersion && recent.Version == builtRecentVersion)
            {
                return;
            }

            string[] terms = ItemCatalog.Fold(search).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            bool questTab = selectedTab.Kind == TabKind.Category && selectedTab.Category == ItemCategory.Quest;
            int quality = qualityFilter;

            bool Passes(CatalogEntry entry)
            {
                // Görev kategorisi seçiliyken görev itemları kutudan bağımsız gösterilir
                if (entry.IsQuest && !showQuest && !questTab) return false;
                if (quality != 0 && entry.Star != quality) return false;
                return MatchesAll(entry.SearchKey, terms);
            }

            var sections = new List<ItemSection>();
            switch (selectedTab.Kind)
            {
                case TabKind.All:
                {
                    // Katalog ada göre sıralı; kategori kovalarına sırayla dağıtmak bölüm içi sırayı korur
                    var buckets = new List<CatalogEntry>[Categories.DisplayOrder.Length];
                    foreach (var entry in all)
                    {
                        if (Passes(entry))
                        {
                            int index = (int)entry.Category;
                            (buckets[index] ?? (buckets[index] = new List<CatalogEntry>())).Add(entry);
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
                    var items = new List<CatalogEntry>();
                    foreach (var entry in all)
                    {
                        if (favorites.Contains(entry.Id) && Passes(entry))
                        {
                            items.Add(entry);
                        }
                    }
                    sections.Add(new ItemSection { Title = Strings.Favorites, Items = items });
                    break;
                }
                case TabKind.Recent:
                {
                    var items = new List<CatalogEntry>();
                    foreach (var recentEntry in recent.Entries)
                    {
                        if (ItemCatalog.TryGet(recentEntry.Id, out var entry) && Passes(entry))
                        {
                            items.Add(entry);
                        }
                    }
                    sections.Add(new ItemSection { Title = Strings.Recent, Items = items });
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
                    break;
                }
            }

            shownCount = 0;
            foreach (var section in sections)
            {
                shownCount += section.Items.Count;
            }
            view.Rebuild(sections, mode);

            builtSource = all;
            builtSearch = search;
            builtShowQuest = showQuest;
            builtTab = selectedTab;
            builtMode = mode;
            builtQuality = qualityFilter;
            builtFavoritesVersion = favorites.Version;
            builtRecentVersion = recent.Version;
        }

        partial void DrawContentPanel(Rect rect)
        {
            KsTheme.Draw(KsTheme.ContentPanel, rect);
            var viewport = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);

            if (view.IsEmpty)
            {
                DrawEmptyState(viewport);
                return;
            }

            var events = view.Draw(viewport, selectedId, e => favorites.Contains(e.Id));
            if (events.Hovered != null)
            {
                MarkHovered(events.Hovered);
            }
            if (events.Clicked != null)
            {
                AddItem(events.Clicked, ClickAmountFor(events.Clicked, events.ClickedWithShift));
            }
            if (events.RightClicked != null)
            {
                ToggleFavorite(events.RightClicked);
            }
            if (events.FavoriteClicked != null)
            {
                ToggleFavorite(events.FavoriteClicked);
            }
        }

        private void DrawEmptyState(Rect r)
        {
            float cx = r.x + r.width / 2f;
            float cy = r.y + r.height / 2f;
            KsTheme.GlyphSearch(new Rect(cx - 24f, cy - 80f, 48f, 48f), KsTheme.DividerDiamond, 5f, 2.6f);
            KsTheme.Label(new Rect(r.x, cy - 20f, r.width, 26f), Strings.EmptyTitle, 18, KsTheme.SectionTitle, TextAnchor.MiddleCenter);
            KsTheme.Label(new Rect(r.x, cy + 8f, r.width, 20f), Strings.EmptyHint, 14, KsTheme.TextMuted, TextAnchor.MiddleCenter);

            float buttonW = KsTheme.Measure(Strings.ResetFilters, 14).x + 20f;
            var button = new Rect(cx - buttonW / 2f, cy + 40f, buttonW, 34f);
            if (GUI.Button(button, GUIContent.none, KsTheme.Button))
            {
                search = string.Empty;
                qualityFilter = 0;
                GUIUtility.keyboardControl = 0;
            }
            KsTheme.Label(button, Strings.ResetFilters, 14,
                button.Contains(Event.current.mousePosition) ? KsTheme.TextHover : KsTheme.Text, TextAnchor.MiddleCenter);
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
    }
}
