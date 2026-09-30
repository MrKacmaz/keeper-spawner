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

    /// <summary>
    /// Item listesini ızgara ya da liste olarak, istenirse başlıklı bölümlere ayırarak çizer.
    /// Scroll alanı GUILayout'a tek bir dikdörtgen olarak bildirilir, içerik mutlak GUI çağrılarıyla ve
    /// sadece görünen satırlar için çizilir. Böylece 700+ item her karede ucuz kalır ve Layout/Repaint
    /// geçişleri arasında kontrol sayısı değişmez.
    /// </summary>
    internal sealed class ItemView
    {
        public const float GridCell = 52f;
        public const float GridGap = 4f;
        public const float ListRow = 28f;
        public const float HeaderHeight = 26f;

        private static readonly Color FavoriteColor = new Color(0.95f, 0.75f, 0.25f);

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
        private int columns = 1;
        private float totalHeight;
        private Vector2 scroll;
        private string emptyText = string.Empty;

        private GUIStyle cellStyle;
        private GUIStyle rowStyle;
        private GUIStyle rowLabelStyle;
        private GUIStyle rowStackStyle;
        private GUIStyle headerStyle;
        private GUIStyle placeholderStyle;
        private GUIStyle badgeStyle;
        private GUIStyle starStyle;
        private GUIStyle questStyle;
        private GUIStyle emptyStyle;

        /// <summary>Filtre, sekme ya da görünüm değişince çağrılır (sadece Layout olayında).</summary>
        public void Rebuild(IList<ItemSection> sections, bool showHeaders, ViewMode viewMode, float contentWidth, string whenEmpty)
        {
            mode = viewMode;
            emptyText = whenEmpty ?? string.Empty;
            columns = mode == ViewMode.Grid
                ? Math.Max(1, (int)((contentWidth + GridGap) / (GridCell + GridGap)))
                : 1;
            float rowHeight = mode == ViewMode.Grid ? GridCell + GridGap : ListRow;

            lines.Clear();
            float y = 0f;
            foreach (var section in sections)
            {
                if (section.Items.Count == 0)
                {
                    continue;
                }
                if (showHeaders)
                {
                    lines.Add(new Line { Header = section, Y = y, Height = HeaderHeight });
                    y += HeaderHeight;
                }
                for (int start = 0; start < section.Items.Count; start += columns)
                {
                    lines.Add(new Line
                    {
                        Section = section,
                        Start = start,
                        Count = Math.Min(columns, section.Items.Count - start),
                        Y = y,
                        Height = rowHeight,
                    });
                    y += rowHeight;
                }
            }
            totalHeight = y;
            scroll = Vector2.zero;
        }

        /// <summary>
        /// Sol tıklanan itemı döner. Sağ tıklananı <paramref name="rightClicked"/>'a, fare altındakini
        /// <paramref name="hovered"/>'a yazar. <paramref name="isFavorite"/> hücre işaretlemesi için.
        /// </summary>
        public CatalogEntry Draw(float width, float height, float contentWidth, Func<CatalogEntry, bool> isFavorite,
            out CatalogEntry hovered, out CatalogEntry rightClicked)
        {
            EnsureStyles();
            hovered = null;
            rightClicked = null;
            CatalogEntry clicked = null;

            scroll = GUILayout.BeginScrollView(scroll, false, true, GUILayout.Width(width), GUILayout.Height(height));
            var content = GUILayoutUtility.GetRect(contentWidth, Mathf.Max(totalHeight, height - 4f),
                GUILayout.Width(contentWidth));

            if (lines.Count == 0)
            {
                GUI.Label(new Rect(content.x, content.y + 4f, contentWidth, 40f), emptyText, emptyStyle);
            }

            var ev = Event.current;
            // Scroll içinde koordinatlar içerik uzayında; görünen bölge scroll.y'den başlar
            var viewport = new Rect(content.x, content.y + scroll.y, contentWidth, height);
            bool mouseInView = viewport.Contains(ev.mousePosition);

            foreach (var line in lines)
            {
                float top = content.y + line.Y;
                if (top + line.Height < viewport.yMin || top > viewport.yMax)
                {
                    continue;
                }

                if (line.Header != null)
                {
                    DrawHeader(new Rect(content.x, top, contentWidth, line.Height), line.Header);
                    continue;
                }

                for (int k = 0; k < line.Count; k++)
                {
                    var entry = line.Section.Items[line.Start + k];
                    var rect = mode == ViewMode.Grid
                        ? new Rect(content.x + k * (GridCell + GridGap), top, GridCell, GridCell)
                        : new Rect(content.x, top + 1f, contentWidth, ListRow - 2f);

                    bool underMouse = mouseInView && rect.Contains(ev.mousePosition);
                    if (underMouse)
                    {
                        hovered = entry;
                        // Sağ tık favori; olayı tüketiyoruz ki buton sağ tıkla item eklemesin
                        if (ev.type == EventType.MouseDown && ev.button == 1)
                        {
                            rightClicked = entry;
                            ev.Use();
                        }
                    }

                    bool favorite = isFavorite(entry);
                    bool pressed = mode == ViewMode.Grid ? DrawCell(rect, entry, favorite) : DrawRow(rect, entry, favorite);
                    if (pressed && Event.current.button == 0)
                    {
                        clicked = entry;
                    }
                }
            }

            GUILayout.EndScrollView();
            return clicked;
        }

        private void DrawHeader(Rect rect, ItemSection section)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }
            GUI.Label(rect, $"{section.Title}  <color=#8a8a8a>({section.Items.Count})</color>", headerStyle);
            DrawFill(new Rect(rect.x, rect.yMax - 3f, rect.width, 1f), new Color(1f, 1f, 1f, 0.15f));
        }

        private bool DrawCell(Rect rect, CatalogEntry entry, bool favorite)
        {
            bool pressed = GUI.Button(rect, GUIContent.none, cellStyle);
            if (Event.current.type != EventType.Repaint)
            {
                return pressed;
            }

            var iconArea = new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, rect.height - 10f);
            if (IconCache.TryGet(entry.Def.iconId, out var icon))
            {
                IconCache.Draw(iconArea, icon);
            }
            else
            {
                GUI.Label(iconArea, Initials(entry), placeholderStyle);
            }

            if (entry.MaxStack > 1)
            {
                DrawShadowed(new Rect(rect.x, rect.y, rect.width - 3f, rect.height - 1f), entry.MaxStack.ToString(), badgeStyle);
            }
            DrawStar(new Rect(rect.xMax - 19f, rect.y + 2f, 17f, 17f), entry);
            if (entry.IsQuest)
            {
                DrawShadowed(new Rect(rect.x + 4f, rect.y + 1f, rect.width, rect.height), "!", questStyle);
            }
            if (favorite)
            {
                DrawOutline(rect, FavoriteColor, 2f);
            }
            return pressed;
        }

        private bool DrawRow(Rect rect, CatalogEntry entry, bool favorite)
        {
            bool pressed = GUI.Button(rect, GUIContent.none, rowStyle);
            if (Event.current.type != EventType.Repaint)
            {
                return pressed;
            }

            if (favorite)
            {
                DrawFill(new Rect(rect.x, rect.y, 3f, rect.height), FavoriteColor);
            }

            var iconArea = new Rect(rect.x + 6f, rect.y + 1f, rect.height - 2f, rect.height - 2f);
            if (IconCache.TryGet(entry.Def.iconId, out var icon))
            {
                IconCache.Draw(iconArea, icon);
            }
            DrawStar(new Rect(iconArea.xMax - 8f, iconArea.y - 1f, 11f, 11f), entry);

            string label = entry.DisplayName;
            if (entry.IsQuest)
            {
                label += $"  <color=#e0a040>({Strings.Quest})</color>";
            }
            label += $"  <color=#888888><size=11>{entry.Id}</size></color>";
            GUI.Label(new Rect(iconArea.xMax + 6f, rect.y, rect.width - iconArea.width - 72f, rect.height), label, rowLabelStyle);
            GUI.Label(new Rect(rect.xMax - 56f, rect.y, 50f, rect.height), "x" + entry.MaxStack, rowStackStyle);
            return pressed;
        }

        /// <summary>Oyunun kendi yıldız sprite'ı: item_star_1 bronz, _2 gümüş, _3 altın.</summary>
        private void DrawStar(Rect area, CatalogEntry entry)
        {
            if (entry.StarIconId == null)
            {
                return;
            }
            if (IconCache.TryGet(entry.StarIconId, out var star))
            {
                IconCache.Draw(area, star);
            }
            else
            {
                // Sprite yoksa (ya da bu karenin bütçesi dolduysa) metinle göster
                DrawShadowed(area, entry.Star + "*", starStyle);
            }
        }

        private static void DrawOutline(Rect rect, Color color, float thickness)
        {
            DrawFill(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawFill(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            DrawFill(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawFill(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private static void DrawFill(Rect rect, Color color)
        {
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, color, 0f, 0f);
        }

        private static void DrawShadowed(Rect rect, string text, GUIStyle style)
        {
            var color = style.normal.textColor;
            style.normal.textColor = new Color(0f, 0f, 0f, 0.85f);
            GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), text, style);
            style.normal.textColor = color;
            GUI.Label(rect, text, style);
        }

        private static string Initials(CatalogEntry entry)
        {
            string name = entry.DisplayName;
            return name.Length <= 2 ? name : name.Substring(0, 2);
        }

        private void EnsureStyles()
        {
            if (cellStyle != null)
            {
                return;
            }
            cellStyle = new GUIStyle(GUI.skin.button) { padding = new RectOffset(0, 0, 0, 0) };
            rowStyle = new GUIStyle(GUI.skin.button);
            rowLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                richText = true,
                clipping = TextClipping.Clip,
                wordWrap = false,
            };
            rowStackStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
            headerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerLeft,
                fontStyle = FontStyle.Bold,
                richText = true,
                fontSize = 13,
            };
            placeholderStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 1f, 1f, 0.45f) },
            };
            badgeStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerRight,
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
            };
            starStyle = new GUIStyle(badgeStyle)
            {
                alignment = TextAnchor.UpperRight,
                normal = { textColor = new Color(1f, 0.85f, 0.3f) },
            };
            questStyle = new GUIStyle(badgeStyle)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 13,
                normal = { textColor = new Color(0.95f, 0.6f, 0.2f) },
            };
            emptyStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true,
                normal = { textColor = new Color(0.75f, 0.75f, 0.75f) },
            };
        }
    }
}
