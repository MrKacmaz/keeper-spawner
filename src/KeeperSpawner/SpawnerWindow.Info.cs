using UnityEngine;

namespace KeeperSpawner
{
    /// <summary>Sağ panel: odaktaki itemın bilgisi, miktar, ekleme/favori düğmeleri ve son eklenenler.</summary>
    internal sealed partial class SpawnerWindow
    {
        private const string AmountControl = "ks-amount";
        private const float InfoPad = 14f;
        private const float InfoGap = 14f;
        private const float RecentSlot = 46f;
        private const float RecentGap = 3f; // tasarımda 5; 5 yuvanın 244px'e sığması için
        private const float RecentAreaH = 70f;

        private string amountText = string.Empty;
        private GUIStyle nameWrapStyle;

        partial void DrawInfo(Rect r)
        {
            KsTheme.Draw(KsTheme.Panel, r);
            KsTheme.Strip(new Rect(r.x + 2f, r.y + 2f, r.width - 4f, StripH), Strings.ItemInfo);

            var focused = FocusedEntry ?? FirstEntry();
            if (focused != null)
            {
                DrawItemDetails(new Rect(r.x + 2f + InfoPad, r.y + 2f + StripH + InfoPad, r.width - 4f - InfoPad * 2f, 0f), focused);
            }
            DrawRecent(r);
        }

        private static CatalogEntry FirstEntry()
        {
            var all = ItemCatalog.All;
            return all.Count > 0 ? all[0] : null;
        }

        private void DrawItemDetails(Rect body, CatalogEntry f)
        {
            float x = body.x, y = body.y, w = body.width;

            // Büyük yuva (84) + ad / kalite / kategori
            var slot = new Rect(x, y, 84f, 84f);
            KsTheme.Fill(new Rect(slot.x - 2f, slot.y - 2f, 88f, 88f), KsTheme.Divider);
            KsTheme.Draw(KsTheme.BigSlot, slot);
            if (KsTheme.Repaint && IconCache.TryGet(f.Def.iconId, out var icon))
            {
                IconCache.Draw(new Rect(slot.x + 10f, slot.y + 10f, 64f, 64f), icon);
            }
            if (f.Star > 0)
            {
                ItemView.DrawStar(new Rect(slot.xMax - 3f - 18f, slot.y + 3f, 18f, 18f), f.Star);
            }

            float tx = slot.xMax + 12f, tw = w - 84f - 12f;
            var nameStyle = NameWrapStyle();
            float nameH = Mathf.Min(nameStyle.CalcHeight(new GUIContent(f.DisplayName), tw), 48f);
            float blockH = nameH + 4f + (f.Star > 0 ? 17f : 0f) + 17f;
            float ty = slot.y + Mathf.Max(0f, (84f - blockH) / 2f);
            if (KsTheme.Repaint)
            {
                nameStyle.normal.textColor = KsTheme.PanelEdge;
                GUI.Label(new Rect(tx, ty + 2f, tw, nameH), f.DisplayName, nameStyle);
                nameStyle.normal.textColor = KsTheme.ItemName;
                GUI.Label(new Rect(tx, ty, tw, nameH), f.DisplayName, nameStyle);
            }
            ty += nameH + 4f;
            if (f.Star > 0)
            {
                KsTheme.Label(new Rect(tx, ty, tw, 17f), Strings.Star(f.Star), 13, KsTheme.StarColor(f.Star));
                ty += 17f;
            }
            KsTheme.Label(new Rect(tx, ty, tw, 17f), Categories.Name(f.Category), 13, KsTheme.CategoryText);
            y += 84f + InfoGap;

            // ID + kopyala
            KsTheme.Label(new Rect(x, y, 78f, 28f), "ID", 13, KsTheme.TextMuted);
            var copy = new Rect(x + w - 28f, y, 28f, 28f);
            KsTheme.Label(new Rect(x + 86f, y, w - 86f - 36f, 28f), KsTheme.Ellipsize(f.Id, 13, w - 86f - 36f), 13, KsTheme.TextBright);
            if (GUI.Button(copy, GUIContent.none, KsTheme.Mini))
            {
                GUIUtility.systemCopyBuffer = f.Id;
                copiedUntil = Time.unscaledTime + CopiedSeconds;
            }
            KsTheme.GlyphCopy(copy, KsTheme.Text);
            y += 28f + 6f;

            KsTheme.Label(new Rect(x, y, 78f, 18f), Strings.MaxStack, 13, KsTheme.TextMuted);
            KsTheme.Label(new Rect(x + 86f, y, w - 86f, 18f), f.MaxStack.ToString(), 13, KsTheme.Count);
            y += 18f;
            if (Time.unscaledTime < copiedUntil)
            {
                KsTheme.Label(new Rect(x, y + 6f, w, 16f), Strings.IdCopied, 12, KsTheme.Copied);
                y += 22f;
            }
            y += InfoGap;

            // ◆ ayırıcı
            KsTheme.Fill(new Rect(x, y + 2f, w / 2f - 8f, 2f), KsTheme.Divider);
            KsTheme.Diamond(new Vector2(x + w / 2f, y + 3f), 6f, KsTheme.Accent, KsTheme.Hex("#3a2410"));
            KsTheme.Fill(new Rect(x + w / 2f + 8f, y + 2f, w / 2f - 8f, 2f), KsTheme.Divider);
            y += 6f + InfoGap;

            // Miktar
            int amount = Mathf.Clamp(amountOverride ?? f.MaxStack, 1, 999);
            KsTheme.Label(new Rect(x, y, w, 16f), Strings.Amount, 13, KsTheme.TextMuted);
            y += 16f + 8f;
            var dec = new Rect(x, y, 34f, 34f);
            var num = new Rect(dec.xMax + 6f, y, 64f, 34f);
            var inc = new Rect(num.xMax + 6f, y, 34f, 34f);
            var max = new Rect(inc.xMax + 6f, y, x + w - inc.xMax - 6f, 34f);
            if (GUI.Button(dec, GUIContent.none, KsTheme.Button))
            {
                amountOverride = Mathf.Clamp(amount - 1, 1, 999);
            }
            KsTheme.GlyphMinus(dec, dec.Contains(Event.current.mousePosition) ? KsTheme.TextHover : KsTheme.Text);
            DrawAmountField(num, amount);
            if (GUI.Button(inc, GUIContent.none, KsTheme.Button))
            {
                amountOverride = Mathf.Clamp(amount + 1, 1, 999);
            }
            KsTheme.GlyphPlus(inc, inc.Contains(Event.current.mousePosition) ? KsTheme.TextHover : KsTheme.Text);
            if (GUI.Button(max, GUIContent.none, KsTheme.Button))
            {
                amountOverride = f.MaxStack;
            }
            KsTheme.Label(max, Strings.Max, 14, max.Contains(Event.current.mousePosition) ? KsTheme.TextHover : KsTheme.Text, TextAnchor.MiddleCenter);
            y += 34f + InfoGap;

            // Envantere ekle ×n
            amount = Mathf.Clamp(amountOverride ?? f.MaxStack, 1, 999);
            var add = new Rect(x, y, w, 44f);
            if (GUI.Button(add, GUIContent.none, KsTheme.Primary))
            {
                AddItem(f, amount);
            }
            string addText = Strings.AddToInventory(amount);
            float addTextW = KsTheme.Measure(addText, 17, true).x;
            float ax = add.x + (add.width - (16f + 10f + addTextW)) / 2f;
            KsTheme.GlyphPlus(new Rect(ax, add.y + 14f, 16f, 16f), KsTheme.PrimaryText, 12f, 2f);
            KsTheme.Label(new Rect(ax + 26f, add.y, addTextW + 4f, add.height - 2f), addText, 17, KsTheme.PrimaryText, TextAnchor.MiddleLeft, true,
                KsTheme.Hex("#3a220a"), new Vector2(0f, 2f));
            y += 44f + InfoGap;

            // Favori düğmesi
            bool fav = favorites.Contains(f.Id);
            var favButton = new Rect(x, y, w, 36f);
            if (GUI.Button(favButton, GUIContent.none, fav ? KsTheme.ButtonOn : KsTheme.Button))
            {
                ToggleFavorite(f);
            }
            string favText = fav ? Strings.InFavorites : Strings.AddFavorite;
            float favTextW = KsTheme.Measure(favText, 14).x;
            float fx = favButton.x + (favButton.width - (14f + 6f + favTextW)) / 2f;
            var starRect = new Rect(fx, favButton.y + 11f, 14f, 14f);
            if (fav)
            {
                KsTheme.Star(starRect, KsTheme.Count);
            }
            else if (KsTheme.Repaint)
            {
                KsTheme.StarOutlineOnly(starRect);
            }
            KsTheme.Label(new Rect(fx + 20f, favButton.y, favTextW + 2f, favButton.height), favText, 14,
                fav || favButton.Contains(Event.current.mousePosition) ? KsTheme.TextHover : KsTheme.Text);
        }

        /// <summary>Sayı kutusu; yazarken geçici olarak boş kalabilsin diye ayrı metin tamponu tutulur.</summary>
        private void DrawAmountField(Rect rect, int amount)
        {
            bool editing = GUI.GetNameOfFocusedControl() == AmountControl;
            if (!editing)
            {
                amountText = amount.ToString();
            }
            GUI.SetNextControlName(AmountControl);
            string next = GUI.TextField(rect, amountText, 3, KsTheme.NumberInput);
            if (next != amountText)
            {
                amountText = FilterDigits(next);
                if (int.TryParse(amountText, out int typed))
                {
                    amountOverride = Mathf.Clamp(typed, 1, 999);
                }
            }
        }

        private static string FilterDigits(string text)
        {
            var chars = new System.Text.StringBuilder(text.Length);
            foreach (char c in text)
            {
                if (c >= '0' && c <= '9')
                {
                    chars.Append(c);
                }
            }
            return chars.ToString();
        }

        private void DrawRecent(Rect r)
        {
            float areaY = r.yMax - 2f - RecentAreaH;
            var strip = new Rect(r.x + 2f, areaY - StripH, r.width - 4f, StripH);
            KsTheme.Fill(new Rect(strip.x, strip.y - 2f, strip.width, 2f), KsTheme.PanelEdge);
            KsTheme.Strip(strip, Strings.RecentTitle);

            float x = r.x + 2f + 12f;
            float y = areaY + 10f;
            if (recent.Count == 0)
            {
                KsTheme.Label(new Rect(x, areaY, r.width - 28f, RecentAreaH), Strings.NoRecent, 13, KsTheme.TextMuted);
                return;
            }

            foreach (var item in recent.Entries)
            {
                if (!ItemCatalog.TryGet(item.Id, out var entry))
                {
                    continue;
                }
                int amount = item.Amount > 0 ? item.Amount : entry.MaxStack;
                var slot = new Rect(x, y, RecentSlot, RecentSlot);
                if (slot.Contains(Event.current.mousePosition))
                {
                    MarkHovered(entry);
                }
                // Tıklayınca aynı miktarı tekrar ekler
                if (GUI.Button(slot, GUIContent.none, KsTheme.Slot) && Event.current.button == 0)
                {
                    AddItem(entry, amount);
                }
                ItemView.DrawItemIcon(new Rect(slot.x + 7f, slot.y + 7f, 32f, 32f), entry);
                if (entry.Star > 0)
                {
                    ItemView.DrawStar(new Rect(slot.xMax - 13f, slot.y + 2f, 11f, 11f), entry.Star);
                }
                KsTheme.Outlined(new Rect(slot.x, slot.y, slot.width - 3f, slot.height), amount.ToString(), 12, KsTheme.Count, TextAnchor.LowerRight);
                x += RecentSlot + RecentGap;
            }
        }

        private GUIStyle NameWrapStyle()
        {
            if (nameWrapStyle == null)
            {
                nameWrapStyle = new GUIStyle(KsTheme.TextStyle(20, true, TextAnchor.UpperLeft, false, true))
                {
                    wordWrap = true,
                };
            }
            return nameWrapStyle;
        }
    }
}
