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

    /// <summary>
    /// Item listesini ızgara ya da liste olarak, istenirse kategori başlıklarıyla bölümlere ayırarak çizer.
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

        private struct Line
        {
            public bool IsHeader;
            public ItemCategory Category;
            public int Start;
            public int Count;
            public float Y;
            public float Height;
        }

        private readonly List<Line> lines = new List<Line>();
        private List<CatalogEntry> items = new List<CatalogEntry>();
        private ViewMode mode;
        private int columns = 1;
        private float totalHeight;
        private Vector2 scroll;

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
        public void Rebuild(List<CatalogEntry> sorted, bool grouped, ViewMode viewMode, float contentWidth)
        {
            items = sorted;
            mode = viewMode;
            columns = mode == ViewMode.Grid
                ? Math.Max(1, (int)((contentWidth + GridGap) / (GridCell + GridGap)))
                : 1;
            float rowHeight = mode == ViewMode.Grid ? GridCell + GridGap : ListRow;

            lines.Clear();
            float y = 0f;
            int i = 0;
            while (i < items.Count)
            {
                int end = items.Count;
                if (grouped)
                {
                    var category = items[i].Category;
                    end = i;
                    while (end < items.Count && items[end].Category == category)
                    {
                        end++;
                    }
                    lines.Add(new Line { IsHeader = true, Category = category, Count = end - i, Y = y, Height = HeaderHeight });
                    y += HeaderHeight;
                }
                for (int start = i; start < end; start += columns)
                {
                    lines.Add(new Line { Start = start, Count = Math.Min(columns, end - start), Y = y, Height = rowHeight });
                    y += rowHeight;
                }
                i = end;
            }
            totalHeight = y;
            scroll = Vector2.zero;
        }

        /// <summary>Tıklanan itemı döner; fare altındaki itemı <paramref name="hovered"/>'a yazar.</summary>
        public CatalogEntry Draw(float width, float height, float contentWidth, out CatalogEntry hovered)
        {
            EnsureStyles();
            hovered = null;
            CatalogEntry clicked = null;

            scroll = GUILayout.BeginScrollView(scroll, false, true, GUILayout.Width(width), GUILayout.Height(height));
            var content = GUILayoutUtility.GetRect(contentWidth, Mathf.Max(totalHeight, height - 4f),
                GUILayout.Width(contentWidth));

            if (items.Count == 0)
            {
                GUI.Label(new Rect(content.x, content.y + 4f, contentWidth, 24f), Strings.NoResults, emptyStyle);
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

                if (line.IsHeader)
                {
                    DrawHeader(new Rect(content.x, top, contentWidth, line.Height), line);
                    continue;
                }

                for (int k = 0; k < line.Count; k++)
                {
                    var entry = items[line.Start + k];
                    var rect = mode == ViewMode.Grid
                        ? new Rect(content.x + k * (GridCell + GridGap), top, GridCell, GridCell)
                        : new Rect(content.x, top + 1f, contentWidth, ListRow - 2f);

                    if (mouseInView && rect.Contains(ev.mousePosition))
                    {
                        hovered = entry;
                    }

                    bool pressed = mode == ViewMode.Grid ? DrawCell(rect, entry) : DrawRow(rect, entry);
                    if (pressed)
                    {
                        clicked = entry;
                    }
                }
            }

            GUILayout.EndScrollView();
            return clicked;
        }

        private void DrawHeader(Rect rect, Line line)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }
            GUI.Label(rect, $"{Categories.Name(line.Category)}  <color=#8a8a8a>({line.Count})</color>", headerStyle);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - 3f, rect.width, 1f), Texture2D.whiteTexture, ScaleMode.StretchToFill,
                true, 0f, new Color(1f, 1f, 1f, 0.15f), 0f, 0f);
        }

        private bool DrawCell(Rect rect, CatalogEntry entry)
        {
            bool pressed = GUI.Button(rect, GUIContent.none, cellStyle);
            if (Event.current.type != EventType.Repaint)
            {
                return pressed;
            }

            var iconArea = new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, rect.height - 10f);
            if (IconCache.TryGet(entry.Def, out var icon))
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
            if (entry.Star > 0)
            {
                DrawShadowed(new Rect(rect.x, rect.y + 1f, rect.width - 3f, rect.height), entry.Star + "*", starStyle);
            }
            if (entry.IsQuest)
            {
                DrawShadowed(new Rect(rect.x + 4f, rect.y + 1f, rect.width, rect.height), "!", questStyle);
            }
            return pressed;
        }

        private bool DrawRow(Rect rect, CatalogEntry entry)
        {
            bool pressed = GUI.Button(rect, GUIContent.none, rowStyle);
            if (Event.current.type != EventType.Repaint)
            {
                return pressed;
            }

            var iconArea = new Rect(rect.x + 4f, rect.y + 1f, rect.height - 2f, rect.height - 2f);
            if (IconCache.TryGet(entry.Def, out var icon))
            {
                IconCache.Draw(iconArea, icon);
            }

            string label = entry.DisplayName;
            if (entry.IsQuest)
            {
                label += $"  <color=#e0a040>({Strings.Quest})</color>";
            }
            label += $"  <color=#888888><size=11>{entry.Id}</size></color>";
            GUI.Label(new Rect(iconArea.xMax + 6f, rect.y, rect.width - iconArea.width - 70f, rect.height), label, rowLabelStyle);
            GUI.Label(new Rect(rect.xMax - 56f, rect.y, 50f, rect.height), "x" + entry.MaxStack, rowStackStyle);
            return pressed;
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
            emptyStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = new Color(0.75f, 0.75f, 0.75f) } };
        }
    }
}
