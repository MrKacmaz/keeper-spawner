using System.Collections.Generic;
using UnityEngine;

namespace KeeperSpawner
{
    /// <summary>
    /// Oyunun UI prefab'larına bağlı, EasySpritesCollection'da olmayan sprite'ları isimle bulur
    /// (ör. pencerelerin kapat düğmesi "comm-btn_close-*"). Oyun pencerelerini açılışta önceden yüklediği
    /// için bu sprite'lar bellekte hazır. Tarama pahalı; sonuç önbelleğe alınır, bulunamazsa ara ara denenir.
    /// </summary>
    internal static class GameSprites
    {
        public const string CloseNormal = "comm-btn_close-active";
        public const string CloseHover = "comm-btn_close-over";
        public const string ClosePressed = "comm-btn_close-press";

        private const float RetrySeconds = 5f;
        // Tarama binlerce sprite'ı geziyor; hiç bulunamazsa sınırsız tekrarlayıp takılma yapmasın
        private const int MaxScans = 4;
        private static int scans;

        private static readonly Dictionary<string, IconCache.Icon> Found = new Dictionary<string, IconCache.Icon>();
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        private static float nextScan;

        public static bool TryGet(string name, out IconCache.Icon icon)
        {
            if (Found.TryGetValue(name, out icon) && Sprites.TryGetValue(name, out var sprite) && sprite != null && icon.Texture != null)
            {
                return true;
            }
            if (Time.unscaledTime < nextScan || scans >= MaxScans)
            {
                return false;
            }
            scans++;
            Scan();
            return Found.TryGetValue(name, out icon);
        }

        /// <summary>Aranan tüm isimleri tek taramada toplar.</summary>
        private static void Scan()
        {
            nextScan = Time.unscaledTime + RetrySeconds;
            Found.Clear();
            Sprites.Clear();
            var wanted = new HashSet<string> { CloseNormal, CloseHover, ClosePressed };
            foreach (var sprite in Resources.FindObjectsOfTypeAll<Sprite>())
            {
                if (sprite != null && sprite.texture != null && wanted.Remove(sprite.name))
                {
                    Sprites[sprite.name] = sprite;
                    Found[sprite.name] = IconCache.FromSprite(sprite);
                    if (wanted.Count == 0)
                    {
                        break;
                    }
                }
            }
            if (wanted.Count > 0)
            {
                Plugin.Log.LogDebug($"Oyun sprite'ları bulunamadı: {string.Join(", ", new List<string>(wanted).ToArray())}");
            }
        }
    }
}
