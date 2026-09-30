using System.Collections.Generic;
using UnityEngine;

namespace KeeperSpawner
{
    /// <summary>Sol panel: hızlı grup (Tümü / Favoriler / Son eklenenler), ◆ ayırıcı, kategoriler.</summary>
    internal sealed partial class SpawnerWindow
    {
        private const float CategoryRowH = 30f;
        private const float CategoryRowGap = 2f;
        private const float CategoryListPad = 6f;
        private const float SeparatorH = 22f;

        private Vector2 sidebarScroll;

        // Sayılar: katalog, görev ayarı ya da listeler değişince yeniden hesaplanır
        private IReadOnlyList<CatalogEntry> countsSource;
        private bool countsShowQuest;
        private int totalVisible;
        private readonly int[] categoryCounts = new int[Categories.DisplayOrder.Length];
        private readonly List<ItemCategory> visibleCategories = new List<ItemCategory>();

        partial void DrawSidebar(Rect rect)
        {
            KsTheme.Draw(KsTheme.Panel, rect);
            KsTheme.Strip(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, StripH), Strings.Categories);

            RefreshCounts();

            var listRect = new Rect(rect.x + 2f, rect.y + 2f + StripH, rect.width - 4f, rect.height - 4f - StripH);
            float contentH = CategoryListPad * 2f + 3f * (CategoryRowH + CategoryRowGap) + SeparatorH
                             + visibleCategories.Count * (CategoryRowH + CategoryRowGap);
            bool scrolls = contentH > listRect.height;
            float rowW = listRect.width - CategoryListPad * 2f - (scrolls ? 12f : 0f);

            sidebarScroll = GUI.BeginScrollView(listRect, sidebarScroll, new Rect(0f, 0f, listRect.width - (scrolls ? 12f : 0f), contentH),
                false, false, GUIStyle.none, KsTheme.Skin.verticalScrollbar);

            float y = CategoryListPad;
            CategoryRow(ref y, rowW, Tab.Of(TabKind.All), Strings.All, ItemIcons.AllSprite, totalVisible);
            CategoryRow(ref y, rowW, Tab.Of(TabKind.Favorites), Strings.Favorites, ItemIcons.FavoritesSprite, favorites.Count);
            CategoryRow(ref y, rowW, Tab.Of(TabKind.Recent), Strings.Recent, ItemIcons.RecentSprite, recent.Count);

            // ◆ ayırıcı
            float cy = y + SeparatorH / 2f - 1f;
            KsTheme.Fill(new Rect(CategoryListPad + 6f, cy, rowW / 2f - 14f, 2f), KsTheme.Divider);
            KsTheme.Diamond(new Vector2(CategoryListPad + rowW / 2f, cy + 1f), 6f, KsTheme.DividerDiamond, Color.clear);
            KsTheme.Fill(new Rect(CategoryListPad + rowW / 2f + 8f, cy, rowW / 2f - 14f, 2f), KsTheme.Divider);
            y += SeparatorH;

            foreach (var category in visibleCategories)
            {
                CategoryRow(ref y, rowW, Tab.Of(category), Categories.Name(category), ItemIcons.ForCategory(category), categoryCounts[(int)category]);
            }

            GUI.EndScrollView();
        }

        private void CategoryRow(ref float y, float width, Tab tab, string name, string sprite, int count)
        {
            var row = new Rect(CategoryListPad, y, width, CategoryRowH);
            bool on = tab.Equals(selectedTab);
            bool hover = row.Contains(Event.current.mousePosition);
            if (GUI.Button(row, GUIContent.none, on ? KsTheme.CategoryOn : KsTheme.Category))
            {
                SelectTab(tab);
            }

            if (KsTheme.Repaint && sprite != null && IconCache.TryGet(sprite, out var icon))
            {
                IconCache.Draw(new Rect(row.x + 8f, row.y + 3f, 24f, 24f), icon);
            }
            Color text = on || hover ? KsTheme.TextHover : KsTheme.TextSecondary;
            string countText = count.ToString();
            float countW = KsTheme.Measure(countText, 12).x;
            KsTheme.Label(new Rect(row.x + 40f, row.y, row.width - 48f - countW - 6f, row.height), name, 15, text,
                TextAnchor.MiddleLeft, false, on ? KsTheme.StripTextShadow : (Color?)null, new Vector2(0f, 1f));
            KsTheme.Label(new Rect(row.xMax - 8f - countW, row.y, countW + 2f, row.height), countText, 12,
                on ? KsTheme.StripText : KsTheme.TextMuted);

            y += CategoryRowH + CategoryRowGap;
        }

        private void RefreshCounts()
        {
            var all = ItemCatalog.All;
            bool showQuest = showQuestItems.Value;
            if (ReferenceEquals(all, countsSource) && showQuest == countsShowQuest)
            {
                return;
            }
            countsSource = all;
            countsShowQuest = showQuest;

            System.Array.Clear(categoryCounts, 0, categoryCounts.Length);
            totalVisible = 0;
            foreach (var entry in all)
            {
                // Görev kategorisi seçilince görev itemları yine gösterildiği için onun sayısı hepsini içerir
                if (entry.Category == ItemCategory.Quest || !entry.IsQuest || showQuest)
                {
                    categoryCounts[(int)entry.Category]++;
                }
                if (!entry.IsQuest || showQuest)
                {
                    totalVisible++;
                }
            }

            visibleCategories.Clear();
            foreach (var category in Categories.DisplayOrder)
            {
                if (categoryCounts[(int)category] > 0)
                {
                    visibleCategories.Add(category);
                }
            }
        }
    }
}
