using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeeperSpawner
{
    internal enum ViewMode
    {
        Grid,
        List,
    }

    /// <summary>Başlıklı bir item grubu (bir kategori, favoriler ya da son eklenenler).</summary>
    internal sealed class ItemSection
    {
        public string Title;
        public List<CatalogEntry> Items;
    }

    /// <summary>Bir karede görünümde olan etkileşimler.</summary>
    internal struct ViewEvents
    {
        public CatalogEntry Clicked;
        public bool ClickedWithShift;
        public CatalogEntry RightClicked;
        public CatalogEntry FavoriteClicked;
        public CatalogEntry Hovered;
    }

    /// <summary>
    /// İçerik paneli: ızgara (10 sütun, 50px yuva) ya da liste (44px satır), başlıklı bölümlerle.
    /// Ölçüler reference-ui.html'den. Sadece görünen satırlar çizilir (700+ item).
    /// </summary>
    internal sealed class ItemView
    {
        // Izgara
        public const float Cell = 50f;
        public const float Gap = 5f;
        public const int Columns = 10;
        private const float GridHeaderH = 34f;   // 6 üst + başlık + 8 alt
        private const float SectionEndGap = 12f;

        // Liste
        private const float RowH = 44f;
        private const float RowLineH = 2f;
        private const float ListColumnsH = 26f;
        private const float ListHeaderH = 35f;   // 12 üst + başlık + 4 alt
        private const float IdColumnW = 150f;
        private const float CategoryColumnW = 92f;
        private const float StackColumnW = 44f;
        private const float FavColumnW = 36f;
        private const float ColumnGap = 10f;

        // Panel iç boşluğu: üst 8, sağ 8, alt 14, sol 12
        private const float PadTop = 8f;
        private const float PadRight = 8f;
        private const float PadBottom = 14f;
        private const float PadLeft = 12f;
        private const float ScrollbarW = 12f;

        private struct Line
        {
            public ItemSection Header;
            public ItemSection Section;
            public int Start;
            public int Count;
            public float Y;
            public float Height;
        }

        private readonly List<Line> lines = new List<Line>();
        private ViewMode mode;
        private float totalHeight;
        private Vector2 scroll;

        /// <summary>Filtre, sekme ya da görünüm değişince çağrılır.</summary>
        public void Rebuild(IList<ItemSection> sections, ViewMode viewMode)
        {
            mode = viewMode;
            lines.Clear();
            float y = PadTop;
            if (mode == ViewMode.List)
            {
                y += ListColumnsH;
            }

            foreach (var section in sections)
            {
                int count = section.Items.Count;
                if (count == 0)
                {
                    continue;
                }
                float headerH = mode == ViewMode.Grid ? GridHeaderH : ListHeaderH;
                lines.Add(new Line { Header = section, Y = y, Height = headerH });
                y += headerH;

                if (mode == ViewMode.Grid)
                {
                    int rows = (count + Columns - 1) / Columns;
                    for (int row = 0; row < rows; row++)
                    {
                        lines.Add(new Line { Section = section, Start = row * Columns, Count = Math.Min(Columns, count - row * Columns), Y = y, Height = Cell });
                        y += Cell + (row < rows - 1 ? Gap : 0f);
                    }
                    y += SectionEndGap;
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        lines.Add(new Line { Section = section, Start = i, Count = 1, Y = y, Height = RowH + RowLineH });
                        y += RowH + RowLineH;
                    }
                }
            }
            totalHeight = y + PadBottom;
            scroll = Vector2.zero;
        }

        public bool IsEmpty => lines.Count == 0;

        public ViewEvents Draw(Rect viewport, string selectedId, Func<CatalogEntry, bool> isFavorite)
        {
            var events = new ViewEvents();
            var ev = Event.current;
            float contentW = viewport.width - ScrollbarW;
            scroll = GUI.BeginScrollView(viewport, scroll, new Rect(0f, 0f, contentW, Mathf.Max(totalHeight, viewport.height)),
                false, true, GUIStyle.none, KsTheme.Skin.verticalScrollbar);

            var visible = new Rect(0f, scroll.y, contentW, viewport.height);
            bool mouseInView = visible.Contains(ev.mousePosition);
            float innerW = contentW - PadLeft - PadRight;

            if (mode == ViewMode.List && visible.yMin < PadTop + ListColumnsH)
            {
                DrawListColumns(new Rect(PadLeft - 8f, PadTop, innerW + 8f, ListColumnsH));
            }

            foreach (var line in lines)
            {
                if (line.Y + line.Height < visible.yMin || line.Y > visible.yMax)
                {
                    continue;
                }
                if (line.Header != null)
                {
                    DrawSectionHeader(new Rect(PadLeft, line.Y, innerW, line.Height), line.Header);
                    continue;
                }

                for (int k = 0; k < line.Count; k++)
                {
                    var entry = line.Section.Items[line.Start + k];
                    Rect rect = mode == ViewMode.Grid
                        ? new Rect(PadLeft + k * (Cell + Gap), line.Y, Cell, Cell)
                        : new Rect(PadLeft - 8f, line.Y, innerW + 8f, RowH);

                    bool underMouse = mouseInView && rect.Contains(ev.mousePosition);
                    if (underMouse)
                    {
                        events.Hovered = entry;
                        if (ev.type == EventType.MouseDown && ev.button == 1)
                        {
                            events.RightClicked = entry;
                            ev.Use();
                        }
                    }

                    bool selected = entry.Id == selectedId;
                    bool favorite = isFavorite(entry);
                    bool pressed = mode == ViewMode.Grid
                        ? DrawSlot(rect, entry, selected, favorite)
                        : DrawRow(rect, entry, selected, favorite, ref events);
                    if (pressed && ev.button == 0)
                    {
                        events.Clicked = entry;
                        events.ClickedWithShift = ev.shift;
                    }
                }
            }

            GUI.EndScrollView();
            return events;
        }

        private static void DrawSectionHeader(Rect r, ItemSection section)
        {
            bool grid = r.height == GridHeaderH;
            int size = grid ? 16 : 15;
            float top = grid ? 6f : 12f;
            float bottom = grid ? 8f : 4f;
            var band = new Rect(r.x, r.y + top, r.width, r.height - top - bottom);

            float titleW = KsTheme.Measure(section.Title, size, true).x;
            KsTheme.Label(new Rect(band.x, band.y, titleW + 2f, band.height), section.Title, size, KsTheme.SectionTitle, TextAnchor.MiddleLeft, true);
            float x = band.x + titleW + 10f;

            string count = section.Items.Count.ToString();
            float badgeW = KsTheme.Measure(count, 12).x + 12f;
            var badge = new Rect(x, band.y + (band.height - 18f) / 2f, badgeW, 18f);
            KsTheme.Fill(badge, KsTheme.BadgeBg);
            KsTheme.Label(badge, count, 12, KsTheme.TextLabel, TextAnchor.MiddleCenter);
            x += badgeW + 10f;

            if (grid && x < r.xMax)
            {
                KsTheme.Fill(new Rect(x, band.y + band.height / 2f - 1f, r.xMax - x, 2f), KsTheme.SectionLine);
            }
        }

        private static bool DrawSlot(Rect r, CatalogEntry entry, bool selected, bool favorite)
        {
            bool pressed = GUI.Button(r, GUIContent.none, selected ? KsTheme.SlotSelected : KsTheme.Slot);
            if (!KsTheme.Repaint)
            {
                return pressed;
            }

            var iconBox = new Rect(r.x + (r.width - 32f) / 2f, r.y + (r.height - 32f) / 2f, 32f, 32f);
            DrawItemIcon(iconBox, entry);
            if (entry.Star > 0)
            {
                DrawStar(new Rect(r.xMax - 2f - 12f, r.y + 2f, 12f, 12f), entry.Star);
            }
            if (favorite)
            {
                KsTheme.Fill(new Rect(r.x + 2f, r.y + 2f, 8f, 8f), KsTheme.StarOutline);
                KsTheme.Fill(new Rect(r.x + 3f, r.y + 3f, 6f, 6f), KsTheme.Accent);
            }
            if (entry.IsQuest)
            {
                KsTheme.Outlined(new Rect(r.x + 4f, r.yMax - 17f, 10f, 16f), "!", 12, KsTheme.ItemName, TextAnchor.LowerLeft);
            }
            KsTheme.Outlined(new Rect(r.x, r.y, r.width - 3f, r.height), entry.MaxStack.ToString(), 12, KsTheme.Count, TextAnchor.LowerRight);
            return pressed;
        }

        private static void DrawListColumns(Rect r)
        {
            float x = r.x + 4f;
            float nameW = NameColumnWidth(r.width);
            KsTheme.Label(new Rect(x + 40f + ColumnGap, r.y, nameW, r.height - 10f), Strings.ColumnItem, 12, KsTheme.TextMuted);
            float idX = x + 40f + ColumnGap + nameW + ColumnGap;
            KsTheme.Label(new Rect(idX, r.y, IdColumnW, r.height - 10f), Strings.ColumnId, 12, KsTheme.TextMuted);
            KsTheme.Label(new Rect(idX + IdColumnW + ColumnGap, r.y, CategoryColumnW, r.height - 10f), Strings.ColumnCategory, 12, KsTheme.TextMuted);
            KsTheme.Label(new Rect(idX + IdColumnW + CategoryColumnW + ColumnGap * 2f, r.y, StackColumnW, r.height - 10f), Strings.ColumnStack, 12, KsTheme.TextMuted);
            KsTheme.Fill(new Rect(r.x, r.yMax - 2f, r.width, 2f), KsTheme.SectionLine);
        }

        private static float NameColumnWidth(float rowW)
        {
            // 4 sol + 40 ikon + ad + 150 ID + 92 kategori + 44 yığın, aralar 10, sağ 8; favori 36 ayrı
            return rowW - FavColumnW - 4f - 8f - 40f - IdColumnW - CategoryColumnW - StackColumnW - ColumnGap * 4f;
        }

        private static bool DrawRow(Rect r, CatalogEntry entry, bool selected, bool favorite, ref ViewEvents events)
        {
            if (selected)
            {
                KsTheme.Fill(new Rect(r.x, r.y, r.width, r.height), KsTheme.RowSelected);
            }
            var rowButton = new Rect(r.x, r.y, r.width - FavColumnW, r.height);
            bool pressed = GUI.Button(rowButton, GUIContent.none, KsTheme.Row);
            var fav = new Rect(r.xMax - FavColumnW, r.y + (r.height - 36f) / 2f, 36f, 36f);
            if (GUI.Button(fav, GUIContent.none, KsTheme.FavButton) && Event.current.button == 0)
            {
                events.FavoriteClicked = entry;
            }
            KsTheme.Fill(new Rect(r.x, r.yMax, r.width, RowLineH), KsTheme.RowLine);
            if (!KsTheme.Repaint)
            {
                return pressed;
            }

            float x = r.x + 4f;
            var iconBox = new Rect(x, r.y + (r.height - 38f) / 2f, 38f, 38f);
            KsTheme.Fill(iconBox, KsTheme.Hex("#16181d"));
            KsTheme.Fill(new Rect(iconBox.x + 2f, iconBox.y + 2f, 34f, 34f), KsTheme.Hex("#262a33"));
            DrawItemIcon(new Rect(iconBox.x + 3f, iconBox.y + 3f, 32f, 32f), entry);
            x += 40f + ColumnGap;

            float nameW = NameColumnWidth(r.width);
            string quality = entry.Star > 0 ? Strings.StarShort(entry.Star) : null;
            float qualityW = quality != null ? 11f + 4f + KsTheme.Measure(quality, 12).x + 8f : 0f;
            string name = KsTheme.Ellipsize(entry.DisplayName, 15, nameW - qualityW);
            float drawnNameW = KsTheme.Measure(name, 15).x;
            KsTheme.Label(new Rect(x, r.y, drawnNameW + 2f, r.height), name, 15, KsTheme.TextBright);
            if (quality != null)
            {
                float qx = x + drawnNameW + 8f;
                DrawStar(new Rect(qx, r.y + (r.height - 11f) / 2f, 11f, 11f), entry.Star);
                KsTheme.Label(new Rect(qx + 15f, r.y, qualityW, r.height), quality, 12, KsTheme.StarColor(entry.Star));
            }
            x += nameW + ColumnGap;

            KsTheme.Label(new Rect(x, r.y, IdColumnW, r.height), KsTheme.Ellipsize(entry.Id, 12, IdColumnW), 12, KsTheme.TextMuted);
            x += IdColumnW + ColumnGap;
            KsTheme.Label(new Rect(x, r.y, CategoryColumnW, r.height), KsTheme.Ellipsize(Categories.Name(entry.Category), 13, CategoryColumnW), 13, KsTheme.CategoryText);
            x += CategoryColumnW + ColumnGap;
            KsTheme.Label(new Rect(x, r.y, StackColumnW, r.height), "x" + entry.MaxStack, 13, KsTheme.Count);

            var star = new Rect(fav.x + 10f, fav.y + 10f, 16f, 16f);
            if (favorite)
            {
                KsTheme.Star(star, KsTheme.Count);
            }
            else
            {
                KsTheme.StarOutlineOnly(star);
            }
            return pressed;
        }

        /// <summary>Oyunun item sprite'ı 32px kutuya en-boy oranı korunarak; yoksa adın baş harfleri.</summary>
        public static void DrawItemIcon(Rect box, CatalogEntry entry)
        {
            if (!KsTheme.Repaint)
            {
                return;
            }
            if (IconCache.TryGet(entry.Def.iconId, out var icon))
            {
                IconCache.Draw(box, icon);
            }
            else
            {
                string initials = entry.DisplayName.Length <= 2 ? entry.DisplayName : entry.DisplayName.Substring(0, 2);
                KsTheme.Label(box, initials, 13, new Color(1f, 1f, 1f, 0.45f), TextAnchor.MiddleCenter, true);
            }
        }

        /// <summary>Kalite yıldızı: oyunun sprite'ı, yoksa tasarımın piksel yıldızı.</summary>
        public static void DrawStar(Rect rect, int star)
        {
            if (!KsTheme.Repaint)
            {
                return;
            }
            if (IconCache.TryGet("item_star_" + star, out var icon))
            {
                IconCache.Draw(rect, icon);
            }
            else
            {
                KsTheme.Star(rect, KsTheme.StarColor(star));
            }
        }
    }
}
