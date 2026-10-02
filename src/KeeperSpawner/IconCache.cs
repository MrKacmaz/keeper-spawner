using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

namespace KeeperSpawner
{
    /// <summary>
    /// Item ikonlarını oyunun EasySpritesCollection'ından çözüp önbelleğe alır.
    /// - SpriteAtlas.GetSprite her çağrıda yeni bir Sprite kopyası üretir; her ikon bir kez çözülür.
    /// - Atlaslar eşzamanlı yüklenir (takılma yapabilir); karede sınırlı sayıda ikon çözülür.
    /// - Oyun sahne değişiminde atlasları boşaltabilir; yok olmuş sprite fark edilip yeniden çözülür.
    /// </summary>
    internal static class IconCache
    {
        public struct Icon
        {
            public Texture Texture;
            public Rect Uv;
            public Vector2 Size;
        }

        private sealed class Entry
        {
            public Sprite Sprite;
            public Icon Icon;
        }

        private const int ResolvesPerFrame = 24;

        private static readonly Dictionary<string, Entry> Cache = new Dictionary<string, Entry>();
        private static readonly HashSet<string> Missing = new HashSet<string>();
        private static int budgetFrame = -1;
        private static int budget;

        /// <summary>Sadece Repaint olayında çağrılmalı (bütçe kare başına sayılır).</summary>
        public static bool TryGet(string name, out Icon icon)
        {
            icon = default;
            // Resolve ayrı metot: sprite API'si değiştiyse onu derlemeye kalkmadan ikonsuz devam
            if (!GameCompat.Icons || string.IsNullOrEmpty(name) || Missing.Contains(name))
            {
                return false;
            }

            if (Cache.TryGetValue(name, out var cached))
            {
                // Unity nesnesi yok edildiyse (atlas boşaltıldı) == null true döner
                if (cached.Sprite != null && cached.Icon.Texture != null)
                {
                    icon = cached.Icon;
                    return true;
                }
                Cache.Remove(name);
            }

            if (Time.frameCount != budgetFrame)
            {
                budgetFrame = Time.frameCount;
                budget = ResolvesPerFrame;
            }
            if (budget <= 0)
            {
                return false; // sonraki karede
            }
            budget--;

            var sprite = Resolve(name);
            if (sprite == null || sprite.texture == null)
            {
                Missing.Add(name);
                return false;
            }

            var entry = new Entry { Sprite = sprite, Icon = FromSprite(sprite) };
            Cache[name] = entry;
            icon = entry.Icon;
            return true;
        }

        private static Sprite Resolve(string name)
        {
            try
            {
                var collection = LazySingletonSO<EasySpritesCollection>.Instance;
                // HasSprite önce: GetSprite eksik isimde oyunun loguna uyarı basıyor
                if (collection == null || !collection.HasSprite(name))
                {
                    return null;
                }
                return collection.GetSprite(name);
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogWarning($"Could not load icon [{name}]: {e.Message}");
                return null;
            }
        }

        public static Icon FromSprite(Sprite sprite)
        {
            // Atlas sprite'larında textureRect "tight" paketlemede erişilemez; uv sınırlarını kullan
            var uvs = sprite.uv;
            float minX = 1f, minY = 1f, maxX = 0f, maxY = 0f;
            foreach (var uv in uvs)
            {
                minX = Mathf.Min(minX, uv.x);
                minY = Mathf.Min(minY, uv.y);
                maxX = Mathf.Max(maxX, uv.x);
                maxY = Mathf.Max(maxY, uv.y);
            }
            return new Icon
            {
                Texture = sprite.texture,
                Uv = Rect.MinMaxRect(minX, minY, maxX, maxY),
                Size = sprite.rect.size,
            };
        }

        /// <summary>
        /// İkonu hedef alana en-boy oranını koruyarak çizer. Piksel sanatı bulanıklaşmasın diye
        /// sığıyorsa tam sayı katıyla büyütür.
        /// </summary>
        public static void Draw(Rect area, Icon icon)
        {
            float w = Mathf.Max(1f, icon.Size.x);
            float h = Mathf.Max(1f, icon.Size.y);
            float fit = Mathf.Min(area.width / w, area.height / h);
            float scale = fit >= 1f ? Mathf.Floor(fit) : fit;
            float dw = w * scale;
            float dh = h * scale;
            var rect = new Rect(
                Mathf.Round(area.x + (area.width - dw) / 2f),
                Mathf.Round(area.y + (area.height - dh) / 2f),
                dw, dh);
            GUI.DrawTextureWithTexCoords(rect, icon.Texture, icon.Uv);
        }

        /// <summary>İkonu alanı tam dolduracak şekilde çizer (oyunun kendi UI ölçeklemesi gibi).</summary>
        public static void DrawStretched(Rect area, Icon icon)
        {
            GUI.DrawTextureWithTexCoords(area, icon.Texture, icon.Uv);
        }
    }
}
