using UnityEngine;

namespace KeeperSpawner
{
    /// <summary>Alt çubuk: solda bildirim ya da fare altındaki itemın özeti, sağda tuş ipuçları.</summary>
    internal sealed partial class SpawnerWindow
    {
        partial void DrawFooter(Rect r)
        {
            KsTheme.Fill(r, KsTheme.FooterBg);
            KsTheme.Fill(new Rect(r.x, r.y, r.width, 2f), KsTheme.FrameDark);
            var inner = new Rect(r.x + 14f, r.y + 2f, r.width - 28f, r.height - 2f);

            // Sağdan sola tuş ipuçları
            var hints = new[]
            {
                (Strings.KeyClick, Strings.ClickHint(clickAmount.Value)),
                (Strings.KeyShiftClick, Strings.OneItem),
                (Strings.KeyRightClick, Strings.FavoriteHint),
                (Strings.KeyCtrlF, Strings.SearchHint),
                (Strings.KeyEsc, Strings.CloseHint),
            };
            float x = inner.xMax;
            for (int i = hints.Length - 1; i >= 0; i--)
            {
                var (key, text) = hints[i];
                float textW = KsTheme.Measure(text, 13).x;
                float keyW = KsTheme.Measure(key, 12).x + 12f;
                x -= textW;
                KsTheme.Label(new Rect(x, inner.y, textW + 2f, inner.height), text, 13, KsTheme.TextLabel);
                x -= 6f + keyW;
                var cap = new Rect(x, inner.y + (inner.height - 20f) / 2f, keyW, 20f);
                KsTheme.Draw(KsTheme.Key, cap);
                KsTheme.Label(new Rect(cap.x, cap.y, cap.width, cap.height - 2f), key, 12, KsTheme.StripText, TextAnchor.MiddleCenter);
                x -= 12f;
            }

            // Solda bildirim ya da özet (sığmazsa kırpılır). Döngü son ipucundan sonra da 12 düştü;
            // onu geri alıp ipuçlarıyla arasına 14px bırakıyoruz.
            float hintsLeft = x + 12f;
            var left = new Rect(inner.x, inner.y, hintsLeft - 14f - inner.x, inner.height);
            if (HasToast)
            {
                KsTheme.Label(left, toast, 13, toastColor);
            }
            else if (hovered != null)
            {
                KsTheme.Label(left, HoverLine(hovered), 13, KsTheme.TextMuted, TextAnchor.MiddleLeft, false, null, null, true);
            }
        }

        /// <summary>Ad · Kalite · id · xN · Kategori</summary>
        private static string HoverLine(CatalogEntry e)
        {
            string name = $"<b><color=#eaa640>{e.DisplayName}</color></b>";
            string quality = e.Star > 0
                ? $"<color=#{ColorUtility.ToHtmlStringRGB(KsTheme.StarColor(e.Star))}>  {Strings.Star(e.Star)}</color>"
                : string.Empty;
            string quest = e.IsQuest ? $"<color=#eaa640>  ({Strings.Quest})</color>" : string.Empty;
            return $"{name}{quality}{quest}<color=#a9aeb8>  ·  {e.Id}  ·  x{e.MaxStack}  ·  {Categories.Name(e.Category)}</color>";
        }
    }
}
