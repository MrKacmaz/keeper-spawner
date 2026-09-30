using System.Collections.Generic;
using UnityEngine;

namespace KeeperSpawner
{
    /// <summary>
    /// v0.4 arayüz teması: renk token'ları, runtime'da üretilen 9-dilim dokular, GUIStyle'lar ve
    /// çizim yardımcıları. Değerler docs/ui-handoff/HANDOFF.md ve reference-ui.html'den birebir alındı.
    /// </summary>
    internal static class KsTheme
    {
        // --- Renkler (HANDOFF.md §3) ---
        public static readonly Color Backdrop = Hex("#08080a");
        public static readonly Color FrameDark = Hex("#15171c");
        public static readonly Color FrameSteel = Hex("#5b6272");
        public static readonly Color Rivet = Hex("#8a92a2");
        public static readonly Color RivetShadow = Hex("#4b515d");
        public static readonly Color WindowBg = Hex("#3a3f4a");
        public static readonly Color PanelBg = Hex("#2f343e");
        public static readonly Color PanelEdge = Hex("#1b1e24");
        public static readonly Color Wood = Hex("#76583a");
        public static readonly Color WoodLight = Hex("#9c7a52");
        public static readonly Color WoodShadow = Hex("#4a3421");
        public static readonly Color StripText = Hex("#f3e2b8");
        public static readonly Color StripTextShadow = Hex("#2b1b0f");
        public static readonly Color TitleText = Hex("#f6e6bd");
        public static readonly Color Accent = Hex("#e0b050");
        public static readonly Color FocusRing = Hex("#f6dc7a");
        public static readonly Color Count = Hex("#f2c94c");
        public static readonly Color ItemName = Hex("#eaa640");
        public static readonly Color Text = Hex("#e6dcc8");
        public static readonly Color TextSecondary = Hex("#d9ccb4");
        public static readonly Color TextMuted = Hex("#a9aeb8");
        public static readonly Color TextLabel = Hex("#b4b9c3");
        public static readonly Color TextBright = Hex("#f3ead6");
        public static readonly Color TextHover = Hex("#fff1cf");
        public static readonly Color SectionTitle = Hex("#efe3c6");
        public static readonly Color CategoryText = Hex("#cfc4ad");
        public static readonly Color Success = Hex("#a6db6e");
        public static readonly Color Copied = Hex("#9fd26a");
        public static readonly Color FooterBg = Hex("#23272f");
        public static readonly Color Divider = Hex("#4a505c");
        public static readonly Color DividerDiamond = Hex("#6b7282");
        public static readonly Color SectionLine = Hex("#454b57");
        public static readonly Color BadgeBg = Hex("#1f232a");
        public static readonly Color RowLine = Hex("#262a32");
        public static readonly Color RowHover = Hex("#383d49");
        public static readonly Color RowSelected = Hex("#3a3527");
        public static readonly Color Placeholder = Hex("#8a909e");
        public static readonly Color StarBronze = Hex("#d99555");
        public static readonly Color StarSilver = Hex("#cfd8e2");
        public static readonly Color StarGold = Hex("#f2c94c");
        public static readonly Color StarOutline = Hex("#1a1208");
        public static readonly Color PrimaryText = Hex("#fff2d0");
        public static readonly Color Black = new Color(0f, 0f, 0f, 1f);

        public static Color StarColor(int star)
        {
            switch (star)
            {
                case 1: return StarBronze;
                case 2: return StarSilver;
                case 3: return StarGold;
                default: return TextMuted;
            }
        }

        // --- Kutu stilleri (9-dilim dokular) ---
        public static GUIStyle Panel;
        public static GUIStyle ContentPanel;
        public static GUIStyle Slot;
        public static GUIStyle SlotSelected;
        public static GUIStyle BigSlot;
        public static GUIStyle Button;
        public static GUIStyle ButtonOn;
        public static GUIStyle Chip;
        public static GUIStyle ChipOn;
        public static GUIStyle Primary;
        public static GUIStyle Close;
        public static GUIStyle Mini;
        public static GUIStyle Category;
        public static GUIStyle CategoryOn;
        public static GUIStyle Row;
        public static GUIStyle FavButton;
        public static GUIStyle Key;
        public static GUIStyle CheckOff;
        public static GUIStyle CheckOn;
        public static GUIStyle Input;
        public static GUIStyle InputFocused;
        public static GUIStyle NumberInput;
        public static GUIStyle Badge;
        public static GUIStyle VersionBadge;

        /// <summary>Özel kaydırma çubuğu ve metin alanları için kopyalanmış skin.</summary>
        public static GUISkin Skin;
        public static Font Font;

        private static Texture2D starFilled;
        private static Texture2D starEmpty;
        private static bool built;
        private static readonly List<Object> Owned = new List<Object>();
        private static readonly Dictionary<long, GUIStyle> TextStyles = new Dictionary<long, GUIStyle>();

        /// <summary>OnGUI içinde, GUI.skin hazırken çağrılır.</summary>
        public static void EnsureBuilt(string fontSetting)
        {
            if (built && Skin != null)
            {
                return;
            }
            Font = FontFinder.Resolve(fontSetting);
            BuildSkin();
            BuildStyles();
            BuildStars();
            built = true;
        }

        // ------------------------------------------------------------------
        // Kurulum
        // ------------------------------------------------------------------

        private static void BuildSkin()
        {
            Skin = Object.Instantiate(GUI.skin);
            Skin.hideFlags = HideFlags.HideAndDontSave;
            Owned.Add(Skin);
            if (Font != null)
            {
                Skin.font = Font;
            }

            // Kaydırma çubuğu: 12px iz #1f232a (sol kenar 2px #16181d), tutamaç #5b6272
            var track = new GUIStyle(Skin.verticalScrollbar)
            {
                normal = { background = Box(BadgeBg, Hex("#16181d"), 0, left: Band(Hex("#16181d"), 2)) },
                border = new RectOffset(2, 0, 0, 0),
                fixedWidth = 12f,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
            };
            var thumb = new GUIStyle(Skin.verticalScrollbarThumb)
            {
                normal = { background = Box(FrameSteel, BadgeBg, 2, top: Band(Hex("#7c8494"), 2)) },
                border = new RectOffset(2, 2, 4, 2),
                fixedWidth = 12f,
            };
            Skin.verticalScrollbar = track;
            Skin.verticalScrollbarThumb = thumb;
            Skin.verticalScrollbarUpButton = new GUIStyle { fixedHeight = 0f, fixedWidth = 0f };
            Skin.verticalScrollbarDownButton = new GUIStyle { fixedHeight = 0f, fixedWidth = 0f };
            Skin.scrollView = new GUIStyle();
        }

        private static void BuildStyles()
        {
            Panel = BoxStyle(Box(PanelBg, PanelEdge, 2), 2);
            ContentPanel = BoxStyle(Box(PanelBg, PanelEdge, 2, top: Band(Hex("#23272e"), 2)), 2, top: 4);

            var slotTopLeft = Band(Hex("#353a47"), 2);
            var slotBottomRight = Band(Hex("#1d2027"), 2);
            Slot = BoxStyle(Box(Hex("#262a33"), Hex("#16181d"), 2, slotTopLeft, slotBottomRight, slotTopLeft, slotBottomRight), 4);
            Slot.hover.background = Box(Hex("#30353f"), Accent, 2, slotTopLeft, slotBottomRight, slotTopLeft, slotBottomRight);
            Slot.active.background = Slot.hover.background;
            var selectedBand = Band(Hex("#6e5222"), 2);
            SlotSelected = BoxStyle(Box(Hex("#3a3527"), Accent, 2, selectedBand, selectedBand, selectedBand, selectedBand), 4);
            SlotSelected.hover.background = SlotSelected.normal.background;
            BigSlot = BoxStyle(Box(Hex("#262a33"), Hex("#16181d"), 2,
                Band(Hex("#353a47"), 3), Band(Hex("#1d2027"), 3), Band(Hex("#353a47"), 3), Band(Hex("#1d2027"), 3)), 5);

            Button = ButtonStyle(Hex("#2a2f3b"), Hex("#363c4a"), Hex("#151820"), Band(Hex("#4a5264"), 2), Band(Hex("#1e222b"), 2), Text, TextHover, 14);
            ButtonOn = ButtonStyle(Hex("#6f5236"), Hex("#6f5236"), Hex("#2a1a0e"), Band(Hex("#92704a"), 2), Band(WoodShadow, 2), TextHover, TextHover, 14);
            Chip = ButtonStyle(Hex("#2a2f3b"), Hex("#363c4a"), Hex("#151820"), Band(Hex("#434b5c"), 2), default, TextSecondary, TextSecondary, 13);
            ChipOn = ButtonStyle(Hex("#6f5236"), Hex("#6f5236"), Hex("#2a1a0e"), Band(Hex("#92704a"), 2), default, TextHover, TextHover, 13);
            Primary = ButtonStyle(Hex("#9a6424"), Hex("#ab7230"), Hex("#2a1a0a"), Band(Hex("#d49a50"), 2), Band(Hex("#6a3f14"), 3), PrimaryText, PrimaryText, 17);
            Primary.active.background = Box(Hex("#9a6424"), Hex("#2a1a0a"), 2, top: Band(Hex("#6a3f14"), 3));
            Close = ButtonStyle(Hex("#8c1f1f"), Hex("#a52a24"), Hex("#2a0a0a"), Band(Hex("#c84a3a"), 2), Band(Hex("#5a1010"), 2), Text, Text, 14);
            Mini = ButtonStyle(Hex("#2a2f3b"), Hex("#363c4a"), Hex("#151820"), Band(Hex("#4a5264"), 2), default, Text, Text, 13);

            Category = ButtonStyle(Color.clear, Hex("#434956"), Color.clear, default, default, TextSecondary, TextHover, 15, edgeWidth: 0);
            CategoryOn = ButtonStyle(Hex("#6f5236"), Hex("#6f5236"), Color.clear, Band(Hex("#92704a"), 2), Band(WoodShadow, 2), TextHover, TextHover, 15, edgeWidth: 0);
            Row = ButtonStyle(Color.clear, RowHover, Color.clear, default, default, Text, Text, 15, edgeWidth: 0);
            FavButton = ButtonStyle(Color.clear, Hex("#2a2f3b"), Color.clear, default, default, Text, Text, 13, edgeWidth: 0);

            Key = BoxStyle(Box(Hex("#2f3440"), Hex("#4a5264"), 1, bottom: Band(Hex("#4a5264"), 2)), 3);
            CheckOff = BoxStyle(Box(Hex("#1d2028"), Hex("#121419"), 2, Band(Hex("#3a404c"), 1), Band(Hex("#3a404c"), 1), Band(Hex("#3a404c"), 1), Band(Hex("#3a404c"), 1)), 3);
            CheckOn = BoxStyle(Box(Hex("#9a6424"), Hex("#2a1a0a"), 2), 2);

            var inputShade = Band(Hex("#0e1014"), 2);
            Input = TextFieldStyle(Box(Hex("#1d2028"), Hex("#121419"), 2, inputShade, default, inputShade, default), 15, TextBright, TextAnchor.MiddleLeft);
            Input.padding = new RectOffset(34, 34, 0, 0);
            InputFocused = TextFieldStyle(Box(Hex("#1d2028"), Accent, 2, inputShade, default, inputShade, default), 15, TextBright, TextAnchor.MiddleLeft);
            InputFocused.padding = Input.padding;
            NumberInput = TextFieldStyle(Box(Hex("#1d2028"), Hex("#121419"), 2, inputShade, default, inputShade, default), 16, Count, TextAnchor.MiddleCenter);
            NumberInput.fontStyle = FontStyle.Bold;

            Badge = BoxStyle(Box(BadgeBg, BadgeBg, 0), 0);
            VersionBadge = BoxStyle(Box(WoodShadow, StripTextShadow, 2), 2);
        }

        private static void BuildStars()
        {
            // 12x12 piksel yıldız (tasarımdaki SVG yıldızının piksel karşılığı)
            string[] mask =
            {
                ".....##.....",
                ".....##.....",
                "....####....",
                "############",
                ".##########.",
                "..########..",
                "...######...",
                "..########..",
                "..###..###..",
                ".###....###.",
                ".##......##.",
                "............",
            };
            starFilled = StarTexture(mask, Color.white, StarOutline, true);
            starEmpty = StarTexture(mask, Color.clear, Placeholder, false);
        }

        // ------------------------------------------------------------------
        // Çizim yardımcıları (hepsi Repaint dışında çağrılırsa sessizce çizmez)
        // ------------------------------------------------------------------

        public static bool Repaint => Event.current.type == EventType.Repaint;

        public static void Fill(Rect rect, Color color)
        {
            if (Repaint && color.a > 0f)
            {
                GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, color, 0f, 0f);
            }
        }

        public static void Draw(GUIStyle style, Rect rect, bool hover = false)
        {
            if (Repaint)
            {
                style.Draw(rect, GUIContent.none, hover, false, false, false);
            }
        }

        /// <summary>Ahşap şerit: düz zemin + 7px'de bir 2px %8 siyah damar + üst ışık 2px + alt gölge 3px.</summary>
        public static void WoodStrip(Rect rect)
        {
            if (!Repaint)
            {
                return;
            }
            Fill(rect, Wood);
            var grain = new Color(0f, 0f, 0f, 0.08f);
            for (float y = rect.yMax - 2f; y > rect.y; y -= 7f)
            {
                Fill(new Rect(rect.x, y, rect.width, 2f), grain);
            }
            Fill(new Rect(rect.x, rect.y, rect.width, 2f), WoodLight);
            Fill(new Rect(rect.x, rect.yMax - 3f, rect.width, 3f), WoodShadow);
        }

        /// <summary>Bölüm şeridi (32px): ahşap + alt kenar + ortada "— Başlık —".</summary>
        public static void Strip(Rect rect, string title)
        {
            WoodStrip(new Rect(rect.x, rect.y, rect.width, rect.height - 2f));
            Fill(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), PanelEdge);
            var size = Measure(title, 17, true);
            float total = 22f + 10f + size.x + 10f + 22f;
            float x = rect.x + (rect.width - total) / 2f;
            float cy = rect.y + (rect.height - 2f) / 2f;
            var dash = new Color(Hex("#c9a870").r, Hex("#c9a870").g, Hex("#c9a870").b, 0.55f);
            Fill(new Rect(x, cy - 1f, 22f, 2f), dash);
            Label(new Rect(x + 32f, rect.y, size.x + 2f, rect.height - 2f), title, 17, StripText, TextAnchor.MiddleLeft, true, StripTextShadow, new Vector2(0f, 2f));
            Fill(new Rect(x + 32f + size.x + 10f, cy - 1f, 22f, 2f), dash);
        }

        /// <summary>45° döndürülmüş kare süs; piksel basamaklarla çiziliyor.</summary>
        public static void Diamond(Vector2 center, float size, Color color, Color ring)
        {
            if (!Repaint)
            {
                return;
            }
            float half = Mathf.Round(size * 0.7f);
            if (ring.a > 0f)
            {
                DiamondShape(center, half + 2f, ring);
            }
            DiamondShape(center, half, color);
        }

        private static void DiamondShape(Vector2 center, float half, Color color)
        {
            for (float dy = -half; dy < half; dy += 1f)
            {
                float w = (half - Mathf.Abs(dy + 0.5f)) * 2f;
                if (w > 0f)
                {
                    Fill(new Rect(center.x - w / 2f, center.y + dy, w, 1f), color);
                }
            }
        }

        /// <summary>Pencere köşesindeki 10px perçin.</summary>
        public static void RivetAt(float x, float y)
        {
            Fill(new Rect(x - 2f, y - 2f, 14f, 14f), FrameDark);
            Fill(new Rect(x, y, 10f, 10f), Rivet);
            Fill(new Rect(x + 8f, y, 2f, 10f), RivetShadow);
            Fill(new Rect(x, y + 8f, 10f, 2f), RivetShadow);
        }

        /// <summary>Tasarımın SVG yıldızının piksel karşılığı; <paramref name="color"/> dolgu rengi.</summary>
        public static void Star(Rect rect, Color color)
        {
            if (Repaint)
            {
                // Beyaz dolgulu doku renkle çarpılıyor; koyu kontur koyu kalıyor
                GUI.DrawTexture(rect, starFilled, ScaleMode.ScaleToFit, true, 0f, color, 0f, 0f);
            }
        }

        public static void StarOutlineOnly(Rect rect)
        {
            if (Repaint)
            {
                GUI.DrawTexture(rect, starEmpty, ScaleMode.ScaleToFit, true);
            }
        }

        /// <summary>Metin; istenirse gölgeyle (önce kaydırılmış koyu kopya çizilir).</summary>
        public static void Label(Rect rect, string text, int size, Color color, TextAnchor align = TextAnchor.MiddleLeft,
            bool bold = false, Color? shadow = null, Vector2? shadowOffset = null, bool rich = false, bool clip = true)
        {
            if (!Repaint || string.IsNullOrEmpty(text))
            {
                return;
            }
            var style = TextStyle(size, bold, align, rich, clip);
            if (shadow.HasValue)
            {
                var offset = shadowOffset ?? new Vector2(1f, 1f);
                style.normal.textColor = shadow.Value;
                GUI.Label(new Rect(rect.x + offset.x, rect.y + offset.y, rect.width, rect.height), text, style);
            }
            style.normal.textColor = color;
            GUI.Label(rect, text, style);
        }

        /// <summary>1px siyah konturlu metin (slot adetleri).</summary>
        public static void Outlined(Rect rect, string text, int size, Color color, TextAnchor align, bool bold = true)
        {
            if (!Repaint)
            {
                return;
            }
            var style = TextStyle(size, bold, align, false, false);
            style.normal.textColor = Black;
            GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), text, style);
            GUI.Label(new Rect(rect.x - 1f, rect.y, rect.width, rect.height), text, style);
            GUI.Label(new Rect(rect.x, rect.y - 1f, rect.width, rect.height), text, style);
            GUI.Label(new Rect(rect.x, rect.y + 1f, rect.width, rect.height), text, style);
            style.normal.textColor = color;
            GUI.Label(rect, text, style);
        }

        public static Vector2 Measure(string text, int size, bool bold = false, bool rich = false)
        {
            return TextStyle(size, bold, TextAnchor.MiddleLeft, rich, false).CalcSize(new GUIContent(text ?? string.Empty));
        }

        public static GUIStyle TextStyle(int size, bool bold, TextAnchor align, bool rich, bool clip)
        {
            long key = (long)(size & 0xFF) | ((bold ? 1L : 0L) << 8) | ((long)(int)align << 9) | ((rich ? 1L : 0L) << 14) | ((clip ? 1L : 0L) << 15);
            if (!TextStyles.TryGetValue(key, out var style))
            {
                style = new GUIStyle(Skin.label)
                {
                    fontSize = size,
                    fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                    alignment = align,
                    richText = rich,
                    wordWrap = false,
                    clipping = clip ? TextClipping.Clip : TextClipping.Overflow,
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                };
                if (Font != null)
                {
                    style.font = Font;
                }
                TextStyles[key] = style;
            }
            return style;
        }

        /// <summary>Metni genişliğe sığmazsa "…" ile kısaltır.</summary>
        public static string Ellipsize(string text, int size, float width, bool bold = false)
        {
            if (string.IsNullOrEmpty(text) || Measure(text, size, bold).x <= width)
            {
                return text;
            }
            for (int len = text.Length - 1; len > 0; len--)
            {
                string candidate = text.Substring(0, len).TrimEnd() + "…";
                if (Measure(candidate, size, bold).x <= width)
                {
                    return candidate;
                }
            }
            return "…";
        }

        // --- Piksel glifler (tasarımdaki küçük SVG'lerin karşılıkları) ---

        public static void GlyphClose(Rect r, Color color)
        {
            for (int i = 0; i < 10; i++)
            {
                Fill(new Rect(r.x + 3f + i, r.y + 3f + i, 3f, 3f), color);
                Fill(new Rect(r.xMax - 6f - i, r.y + 3f + i, 3f, 3f), color);
            }
        }

        public static void GlyphSmallX(Rect r, Color color)
        {
            for (int i = 0; i < 8; i++)
            {
                Fill(new Rect(r.x + 1f + i, r.y + 1f + i, 2f, 2f), color);
                Fill(new Rect(r.xMax - 3f - i, r.y + 1f + i, 2f, 2f), color);
            }
        }

        public static void GlyphSearch(Rect r, Color color, float thickness = 2f, float scale = 1f)
        {
            float s = scale;
            float x = r.x + 2f * s, y = r.y + 2f * s, box = 9f * s;
            Fill(new Rect(x, y, box, thickness), color);
            Fill(new Rect(x, y + box - thickness, box, thickness), color);
            Fill(new Rect(x, y, thickness, box), color);
            Fill(new Rect(x + box - thickness, y, thickness, box), color);
            for (int i = 0; i < 5 * s; i++)
            {
                Fill(new Rect(x + box - 1f + i, y + box - 1f + i, 3f * s, 3f * s), color);
            }
        }

        public static void GlyphGrid(Rect r, Color color)
        {
            Fill(new Rect(r.x + 1f, r.y + 1f, 6f, 6f), color);
            Fill(new Rect(r.x + 9f, r.y + 1f, 6f, 6f), color);
            Fill(new Rect(r.x + 1f, r.y + 9f, 6f, 6f), color);
            Fill(new Rect(r.x + 9f, r.y + 9f, 6f, 6f), color);
        }

        public static void GlyphList(Rect r, Color color)
        {
            for (int i = 0; i < 3; i++)
            {
                Fill(new Rect(r.x + 1f, r.y + 2f + i * 5f, 3f, 3f), color);
                Fill(new Rect(r.x + 6f, r.y + 2f + i * 5f, 9f, 3f), color);
            }
        }

        public static void GlyphPlus(Rect r, Color color, float size = 12f, float t = 2f)
        {
            float cx = r.x + (r.width - size) / 2f, cy = r.y + (r.height - size) / 2f;
            Fill(new Rect(cx, cy + (size - t) / 2f, size, t), color);
            Fill(new Rect(cx + (size - t) / 2f, cy, t, size), color);
        }

        public static void GlyphMinus(Rect r, Color color, float size = 10f, float t = 2f)
        {
            float cx = r.x + (r.width - size) / 2f, cy = r.y + (r.height - t) / 2f;
            Fill(new Rect(cx, cy, size, t), color);
        }

        public static void GlyphCopy(Rect r, Color color)
        {
            float x = r.x + (r.width - 14f) / 2f, y = r.y + (r.height - 14f) / 2f;
            Outline(new Rect(x + 4f, y + 4f, 8f, 8f), color, 2f);
            Fill(new Rect(x + 2f, y + 2f, 2f, 7f), color);
            Fill(new Rect(x + 2f, y + 2f, 7f, 2f), color);
        }

        public static void Outline(Rect r, Color color, float t)
        {
            Fill(new Rect(r.x, r.y, r.width, t), color);
            Fill(new Rect(r.x, r.yMax - t, r.width, t), color);
            Fill(new Rect(r.x, r.y, t, r.height), color);
            Fill(new Rect(r.xMax - t, r.y, t, r.height), color);
        }

        // ------------------------------------------------------------------
        // Doku üretimi
        // ------------------------------------------------------------------

        public struct Bevel
        {
            public Color Color;
            public int Width;
        }

        public static Bevel Band(Color color, int width) => new Bevel { Color = color, Width = width };

        /// <summary>
        /// Kenar + iç bevel bantlı kutu dokusu. CSS'teki "border + inset box-shadow" karşılığı.
        /// Dokunun ortası dolgu; GUIStyle.border ile 9-dilim olarak geriliyor.
        /// </summary>
        public static Texture2D Box(Color fill, Color edge, int edgeWidth, Bevel top = default, Bevel bottom = default,
            Bevel left = default, Bevel right = default)
        {
            int inset = Mathf.Max(Mathf.Max(top.Width, bottom.Width), Mathf.Max(left.Width, right.Width));
            int n = edgeWidth * 2 + inset * 2 + 2;
            var texture = NewTexture(n, n);
            var pixels = new Color[n * n];
            for (int row = 0; row < n; row++)
            {
                for (int x = 0; x < n; x++)
                {
                    // row 0 = üst satır; Texture2D'de y=0 alt satır
                    Color c = fill;
                    int fromLeft = x, fromRight = n - 1 - x, fromTop = row, fromBottom = n - 1 - row;
                    if (bottom.Width > 0 && fromBottom >= edgeWidth && fromBottom < edgeWidth + bottom.Width) c = bottom.Color;
                    if (right.Width > 0 && fromRight >= edgeWidth && fromRight < edgeWidth + right.Width) c = right.Color;
                    if (top.Width > 0 && fromTop >= edgeWidth && fromTop < edgeWidth + top.Width) c = top.Color;
                    if (left.Width > 0 && fromLeft >= edgeWidth && fromLeft < edgeWidth + left.Width) c = left.Color;
                    if (fromLeft < edgeWidth || fromRight < edgeWidth || fromTop < edgeWidth || fromBottom < edgeWidth) c = edge;
                    pixels[(n - 1 - row) * n + x] = c;
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private static GUIStyle BoxStyle(Texture2D texture, int border, int top = -1)
        {
            var style = new GUIStyle
            {
                border = new RectOffset(border, border, top < 0 ? border : top, border),
                normal = { background = texture },
            };
            return style;
        }

        private static GUIStyle ButtonStyle(Color fill, Color hoverFill, Color edge, Bevel top, Bevel bottom, Color text, Color hoverText, int fontSize, int edgeWidth = 2)
        {
            int e = edgeWidth;
            var normal = Box(fill, EdgeOr(edge, fill), e, top, bottom);
            var hover = Box(hoverFill, EdgeOr(edge, hoverFill), e, top, bottom);
            int border = e + Mathf.Max(top.Width, bottom.Width);
            var style = new GUIStyle
            {
                border = new RectOffset(border, border, border, border),
                normal = { background = normal, textColor = text },
                hover = { background = hover, textColor = hoverText },
                active = { background = hover, textColor = hoverText },
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                font = Font,
            };
            return style;

            // Kenar rengi saydamsa (kenarsız stil) dolgu rengini kullan
            Color EdgeOr(Color edgeColor, Color fillColor) => edgeColor.a > 0f ? edgeColor : fillColor;
        }

        private static GUIStyle TextFieldStyle(Texture2D texture, int fontSize, Color color, TextAnchor align)
        {
            var style = new GUIStyle(Skin.textField)
            {
                border = new RectOffset(4, 4, 4, 4),
                fontSize = fontSize,
                alignment = align,
                font = Font,
                clipping = TextClipping.Clip,
                wordWrap = false,
            };
            foreach (var state in new[] { style.normal, style.hover, style.focused, style.active, style.onNormal, style.onHover, style.onFocused, style.onActive })
            {
                state.background = texture;
                state.textColor = color;
            }
            return style;
        }

        private static Texture2D StarTexture(string[] mask, Color fill, Color outline, bool filled)
        {
            int h = mask.Length, w = mask[0].Length;
            var texture = NewTexture(w, h);
            var pixels = new Color[w * h];
            bool On(int x, int y) => x >= 0 && y >= 0 && x < w && y < h && mask[y][x] == '#';
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = Color.clear;
                    if (On(x, y))
                    {
                        bool edge = !On(x - 1, y) || !On(x + 1, y) || !On(x, y - 1) || !On(x, y + 1);
                        c = edge ? outline : (filled ? fill : Color.clear);
                    }
                    pixels[(h - 1 - y) * w + x] = c;
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private static Texture2D NewTexture(int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            Owned.Add(texture);
            return texture;
        }

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}
