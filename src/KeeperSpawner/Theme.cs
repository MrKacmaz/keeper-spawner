using UnityEngine;

namespace KeeperSpawner
{
    /// <summary>
    /// Opak pencere teması. IMGUI'nin varsayılan penceresi yarı saydam ve arkadaki oyun görünüyor;
    /// oyunun ahşap/koyu tonlarına yakın düz bir arka plan ve ince bir çerçeve çiziyoruz.
    /// </summary>
    internal static class Theme
    {
        public const float TitleHeight = 22f;

        private static readonly Color Fill = new Color(0.12f, 0.105f, 0.09f);
        private static readonly Color Border = new Color(0.52f, 0.42f, 0.28f);
        private static readonly Color TitleFill = new Color(0.20f, 0.16f, 0.12f);
        private static readonly Color TitleText = new Color(0.93f, 0.86f, 0.72f);

        private static GUIStyle window;
        private static GUIStyle title;
        private static Texture2D windowTexture;
        private static Texture2D titleTexture;
        private static float builtOpacity = -1f;

        public static GUIStyle Window(float opacity)
        {
            opacity = Mathf.Clamp(opacity, 0.2f, 1f);
            if (window == null || windowTexture == null || !Mathf.Approximately(opacity, builtOpacity))
            {
                Build(opacity);
            }
            return window;
        }

        /// <summary>Pencere fonksiyonunun başında çağrılır (koordinatlar pencereye göre).</summary>
        public static void DrawTitle(float width, string text)
        {
            var bar = new Rect(2f, 2f, width - 4f, TitleHeight);
            if (Event.current.type == EventType.Repaint)
            {
                GUI.DrawTexture(bar, titleTexture);
            }
            GUI.Label(bar, text, title);
        }

        private static void Build(float opacity)
        {
            Destroy(windowTexture);
            Destroy(titleTexture);

            var fill = Fill;
            fill.a = opacity;
            windowTexture = MakeFrame(fill, Border);
            titleTexture = MakeSolid(TitleFill);

            window = new GUIStyle(GUI.skin.window)
            {
                border = new RectOffset(1, 1, 1, 1),
                padding = new RectOffset(10, 10, (int)TitleHeight + 8, 10),
                contentOffset = Vector2.zero,
            };
            // Pencere odakta olsa da olmasa da aynı görünsün
            foreach (var state in new[] { window.normal, window.onNormal, window.focused, window.onFocused, window.hover, window.onHover, window.active, window.onActive })
            {
                state.background = windowTexture;
                state.textColor = TitleText;
            }

            title = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = TitleText },
            };

            builtOpacity = opacity;
        }

        /// <summary>1 piksel çerçeveli 3x3 doku; GUIStyle.border ile 9-dilim olarak geriliyor.</summary>
        private static Texture2D MakeFrame(Color fill, Color border)
        {
            var texture = NewTexture(3, 3);
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    texture.SetPixel(x, y, x == 1 && y == 1 ? fill : border);
                }
            }
            texture.Apply();
            return texture;
        }

        private static Texture2D MakeSolid(Color color)
        {
            var texture = NewTexture(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static Texture2D NewTexture(int width, int height)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
        }

        private static void Destroy(Texture2D texture)
        {
            if (texture != null)
            {
                Object.Destroy(texture);
            }
        }
    }
}
