using System;
using System.Linq;
using UnityEngine;

namespace KeeperSpawner
{
    /// <summary>
    /// Arayüz fontunu seçer. Oyunun yüklü fontları bir kez loglanır; UI.Font ayarıyla isim verilebilir.
    /// Ayar boşsa adında "pixel" geçen ve Türkçe karakterleri içeren dinamik bir font aranır;
    /// bulunamazsa Unity'nin varsayılan fontu kullanılır. "-" varsayılan fonta zorlar.
    /// </summary>
    internal static class FontFinder
    {
        private static bool logged;

        public static Font Resolve(string setting)
        {
            Font[] fonts;
            try
            {
                fonts = Resources.FindObjectsOfTypeAll<Font>();
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"Fontlar listelenemedi: {e.Message}");
                return null;
            }

            if (!logged)
            {
                logged = true;
                string list = string.Join(", ", fonts.Select(f => $"{f.name}{(f.dynamic ? "" : " (statik)")}{(SupportsTurkish(f) ? "" : " (TR yok)")}").ToArray());
                Plugin.Log.LogInfo($"Yüklü fontlar ({fonts.Length}): {list}");
            }

            if (setting == "-")
            {
                return null;
            }

            if (!string.IsNullOrEmpty(setting))
            {
                var named = fonts.FirstOrDefault(f => string.Equals(f.name, setting, StringComparison.OrdinalIgnoreCase));
                if (named != null)
                {
                    Plugin.Log.LogInfo($"Arayüz fontu: {named.name} (ayardan)");
                    return named;
                }
                Plugin.Log.LogWarning($"UI.Font = \"{setting}\" bulunamadı, otomatik seçime geçiliyor.");
            }

            var auto = fonts.FirstOrDefault(f => f.dynamic
                && f.name.IndexOf("pixel", StringComparison.OrdinalIgnoreCase) >= 0
                && SupportsTurkish(f));
            Plugin.Log.LogInfo(auto != null
                ? $"Arayüz fontu: {auto.name} (otomatik)"
                : "Arayüz fontu: Unity varsayılanı (uygun piksel font bulunamadı)");
            return auto;
        }

        private static bool SupportsTurkish(Font font)
        {
            try
            {
                return font.HasCharacter('ş') && font.HasCharacter('İ') && font.HasCharacter('ğ');
            }
            catch
            {
                return false;
            }
        }
    }
}
