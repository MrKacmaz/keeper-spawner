using UnityEngine;

namespace KeeperSpawner
{
    /// <summary>Orta sütunun üstü: arama + görünüm + Tık modu, altında kalite çipleri + görev kutusu + sayı.</summary>
    internal sealed partial class SpawnerWindow
    {
        private const float ToolbarH = 34f;
        private const float FilterH = 28f;
        private const float RowGap = 8f;

        /// <summary>İçerik kurulumunda hesaplanan, o an gösterilen item sayısı.</summary>
        private int shownCount;

        partial void DrawContentPanel(Rect rect);

        partial void DrawMain(Rect rect)
        {
            DrawToolbar(new Rect(rect.x, rect.y, rect.width, ToolbarH));
            DrawFilters(new Rect(rect.x, rect.y + ToolbarH + RowGap, rect.width, FilterH));
            float top = ToolbarH + RowGap + FilterH + RowGap;
            DrawContentPanel(new Rect(rect.x, rect.y + top, rect.width, rect.height - top));
        }

        private void DrawToolbar(Rect r)
        {
            // Sağdan sola: Tık segmenti, "Tık:" etiketi, görünüm segmenti
            string[] modeLabels = { "1", "10", Strings.Max };
            var modeWidths = new float[modeLabels.Length];
            float modesW = 0f;
            for (int i = 0; i < modeLabels.Length; i++)
            {
                modeWidths[i] = Mathf.Max(34f, KsTheme.Measure(modeLabels[i], 14).x + 20f);
                modesW += modeWidths[i] - (i > 0 ? 2f : 0f);
            }
            float labelW = KsTheme.Measure(Strings.Click, 13).x;
            float viewW = 34f * 2f - 2f;
            float searchW = r.width - 8f - viewW - 8f - labelW - 6f - modesW;

            DrawSearch(new Rect(r.x, r.y, searchW, r.height));

            float x = r.x + searchW + 8f;
            bool grid = viewMode.Value == ViewMode.Grid;
            var gridBtn = new Rect(x, r.y, 34f, r.height);
            var listBtn = new Rect(x + 32f, r.y, 34f, r.height);
            if (GUI.Button(gridBtn, GUIContent.none, grid ? KsTheme.ButtonOn : KsTheme.Button))
            {
                viewMode.Value = ViewMode.Grid;
            }
            if (GUI.Button(listBtn, GUIContent.none, grid ? KsTheme.Button : KsTheme.ButtonOn))
            {
                viewMode.Value = ViewMode.List;
            }
            KsTheme.GlyphGrid(Centered(gridBtn, 16f), grid ? KsTheme.TextHover : KsTheme.Text);
            KsTheme.GlyphList(Centered(listBtn, 16f), grid ? KsTheme.Text : KsTheme.TextHover);
            x += viewW + 8f;

            KsTheme.Label(new Rect(x, r.y, labelW + 2f, r.height), Strings.Click, 13, KsTheme.TextLabel);
            x += labelW + 6f;
            for (int i = 0; i < modeLabels.Length; i++)
            {
                var mode = (ClickAmount)i;
                bool on = clickAmount.Value == mode;
                var button = new Rect(x, r.y, modeWidths[i], r.height);
                if (GUI.Button(button, GUIContent.none, on ? KsTheme.ButtonOn : KsTheme.Button))
                {
                    clickAmount.Value = mode;
                }
                bool hover = button.Contains(Event.current.mousePosition);
                KsTheme.Label(button, modeLabels[i], 14, on || hover ? KsTheme.TextHover : KsTheme.Text, TextAnchor.MiddleCenter);
                x += modeWidths[i] - 2f;
            }
        }

        private void DrawSearch(Rect r)
        {
            bool focused = GUI.GetNameOfFocusedControl() == SearchControl;
            // Alttaki 2px ışık çizgisi (box-shadow 0 2px 0 #4a5264)
            KsTheme.Fill(new Rect(r.x, r.yMax, r.width, 2f), KsTheme.Hex("#4a5264"));

            GUI.SetNextControlName(SearchControl);
            string next = GUI.TextField(r, search ?? string.Empty, 64, focused ? KsTheme.InputFocused : KsTheme.Input);
            if (next != search)
            {
                search = next;
            }
            if (focusSearchRequested)
            {
                focusSearchRequested = false;
                GUI.FocusControl(SearchControl);
            }

            KsTheme.GlyphSearch(new Rect(r.x + 10f, r.y + 8f, 18f, 18f), KsTheme.TextMuted);
            if (string.IsNullOrEmpty(search) && !focused)
            {
                KsTheme.Label(new Rect(r.x + 34f, r.y, r.width - 68f, r.height), Strings.SearchPlaceholder, 15, KsTheme.Placeholder);
            }
            if (!string.IsNullOrEmpty(search))
            {
                var clear = new Rect(r.xMax - 31f, r.y + 3f, 28f, 28f);
                if (GUI.Button(clear, GUIContent.none, KsTheme.Mini))
                {
                    search = string.Empty;
                    GUIUtility.keyboardControl = 0;
                }
                KsTheme.GlyphSmallX(Centered(clear, 10f), KsTheme.Text);
            }
        }

        private void DrawFilters(Rect r)
        {
            float x = r.x;
            float labelW = KsTheme.Measure(Strings.Quality, 13).x;
            KsTheme.Label(new Rect(x, r.y, labelW + 2f, r.height), Strings.Quality, 13, KsTheme.TextLabel);
            x += labelW + 8f;

            for (int q = 0; q <= 3; q++)
            {
                string label = q == 0 ? Strings.QualityAll : Strings.StarShort(q);
                float textW = KsTheme.Measure(label, 13).x;
                float chipW = textW + 18f + (q > 0 ? 12f + 6f : 0f);
                var chip = new Rect(x, r.y, chipW, r.height);
                bool on = qualityFilter == q;
                if (GUI.Button(chip, GUIContent.none, on ? KsTheme.ChipOn : KsTheme.Chip))
                {
                    qualityFilter = q;
                }
                float cx = chip.x + 9f;
                if (q > 0)
                {
                    DrawQualityStar(new Rect(cx, chip.y + (chip.height - 12f) / 2f, 12f, 12f), q);
                    cx += 12f + 6f;
                }
                KsTheme.Label(new Rect(cx, chip.y, textW + 2f, chip.height), label, 13, on ? KsTheme.TextHover : KsTheme.TextSecondary);
                x += chipW + 4f;
            }

            // Dikey ayırıcı
            x += 4f;
            KsTheme.Fill(new Rect(x, r.y + (r.height - 20f) / 2f, 2f, 20f), KsTheme.Divider);
            x += 2f + 4f + 8f;

            // "Görev itemlarını göster" kutusu (etiket de tıklanabilir)
            float checkLabelW = KsTheme.Measure(Strings.ShowQuestItems, 13).x;
            var checkArea = new Rect(x, r.y, 18f + 8f + checkLabelW, r.height);
            bool quest = showQuestItems.Value;
            if (GUI.Button(checkArea, GUIContent.none, GUIStyle.none))
            {
                showQuestItems.Value = !quest;
            }
            var box = new Rect(x, r.y + (r.height - 18f) / 2f, 18f, 18f);
            KsTheme.Draw(quest ? KsTheme.CheckOn : KsTheme.CheckOff, box);
            if (quest)
            {
                KsTheme.Fill(new Rect(box.x + 5f, box.y + 5f, 8f, 8f), KsTheme.PrimaryText);
            }
            KsTheme.Label(new Rect(x + 26f, r.y, checkLabelW + 2f, r.height), Strings.ShowQuestItems, 13, KsTheme.TextSecondary);

            // Sağda gösterilen / toplam
            string count = Strings.ItemCount(shownCount, totalVisible);
            float countW = KsTheme.Measure(count, 13, false, true).x;
            KsTheme.Label(new Rect(r.xMax - countW - 2f, r.y, countW + 2f, r.height), count, 13, KsTheme.TextLabel,
                TextAnchor.MiddleRight, false, null, null, true);
        }

        /// <summary>Oyunun kalite yıldızı sprite'ı; yüklenemezse tasarımın piksel yıldızı.</summary>
        private static void DrawQualityStar(Rect rect, int star)
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

        private static Rect Centered(Rect outer, float size)
        {
            return new Rect(Mathf.Round(outer.x + (outer.width - size) / 2f), Mathf.Round(outer.y + (outer.height - size) / 2f), size, size);
        }
    }
}
