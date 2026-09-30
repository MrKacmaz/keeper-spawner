using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace KeeperSpawner
{
    internal enum ClickAmount
    {
        One,
        Ten,
        Max,
    }

    /// <summary>
    /// v0.4 spawner penceresi (docs/ui-handoff). 1920×1080 referansında 1120×720 sabit yerleşim;
    /// her şey mutlak GUI çağrılarıyla çiziliyor. Paneller partial dosyalarda:
    /// Sidebar (kategoriler), Toolbar (arama/filtreler), Content (ızgara/liste), Info (sağ panel), Footer.
    /// </summary>
    internal sealed partial class SpawnerWindow
    {
        // --- Yerleşim (HANDOFF.md §2) ---
        private const float W = 1120f;
        private const float H = 720f;
        private const float HeaderH = 54f;
        private const float FooterH = 38f;
        private const float Pad = 12f;
        private const float SidebarW = 200f;
        private const float InfoW = 272f;
        private const float StripH = 32f;
        private const float ReferenceHeight = 1080f;

        private const float ToastSeconds = 2.6f;
        private const float CopiedSeconds = 1.8f;
        private const string SearchControl = "ks-search";

        private static readonly Rect SidebarRect = new Rect(Pad, HeaderH + Pad, SidebarW, H - HeaderH - FooterH - Pad * 2f);
        private static readonly Rect MainRect = new Rect(Pad + SidebarW + Pad, HeaderH + Pad, W - Pad * 4f - SidebarW - InfoW, H - HeaderH - FooterH - Pad * 2f);
        private static readonly Rect InfoRect = new Rect(W - Pad - InfoW, HeaderH + Pad, InfoW, H - HeaderH - FooterH - Pad * 2f);
        private static readonly Rect FooterRect = new Rect(0f, H - FooterH, W, FooterH);

        private enum TabKind
        {
            All,
            Favorites,
            Recent,
            Category,
        }

        private struct Tab : IEquatable<Tab>
        {
            public TabKind Kind;
            public ItemCategory Category;

            public static Tab Of(TabKind kind) => new Tab { Kind = kind };
            public static Tab Of(ItemCategory category) => new Tab { Kind = TabKind.Category, Category = category };

            public bool Equals(Tab other) => Kind == other.Kind && (Kind != TabKind.Category || Category == other.Category);

            public string Key => Kind == TabKind.Category ? Category.ToString() : Kind.ToString();

            public static Tab Parse(string key)
            {
                if (Enum.TryParse(key, out TabKind kind) && kind != TabKind.Category)
                {
                    return Of(kind);
                }
                if (Enum.TryParse(key, out ItemCategory category))
                {
                    return Of(category);
                }
                return Of(TabKind.All);
            }
        }

        // --- Ayarlar ---
        private readonly ConfigEntry<bool> showQuestItems;
        private readonly ConfigEntry<float> uiScale;
        private readonly ConfigEntry<ViewMode> viewMode;
        private readonly ConfigEntry<ClickAmount> clickAmount;
        private readonly ConfigEntry<string> lastCategory;
        private readonly ConfigEntry<string> fontName;
        private readonly ConfigEntry<float> backdropOpacity;
        private readonly IdList favorites;
        private readonly RecentList recent;

        // --- Durum ---
        private Rect windowRect = new Rect(0f, 0f, W, H);
        private bool positioned;
        private bool dragging;
        private Vector2 dragOffset;

        private string search = string.Empty;
        /// <summary>0 = Hepsi, 1 bronz, 2 gümüş, 3 altın.</summary>
        private int qualityFilter;
        private Tab selectedTab;
        private string selectedId;
        private CatalogEntry hovered;
        private CatalogEntry hoveredNextFrame;
        private int? amountOverride;
        private bool focusSearchRequested;

        private string toast;
        private Color toastColor;
        private float toastUntil;
        private float copiedUntil;

        public SpawnerWindow(ConfigEntry<bool> showQuestItems, ConfigEntry<float> uiScale, ConfigEntry<ViewMode> viewMode,
            ConfigEntry<ClickAmount> clickAmount, ConfigEntry<string> lastCategory, ConfigEntry<string> fontName,
            ConfigEntry<float> backdropOpacity, IdList favorites, RecentList recent)
        {
            this.showQuestItems = showQuestItems;
            this.uiScale = uiScale;
            this.viewMode = viewMode;
            this.clickAmount = clickAmount;
            this.lastCategory = lastCategory;
            this.fontName = fontName;
            this.backdropOpacity = backdropOpacity;
            this.favorites = favorites;
            this.recent = recent;
            selectedTab = Tab.Parse(lastCategory.Value);
        }

        public bool IsOpen { get; private set; }

        /// <summary>Bir IMGUI metin alanı klavye odağında mı (arama ya da miktar kutusu).</summary>
        public bool IsTyping => IsOpen && GUIUtility.keyboardControl != 0;

        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void Open()
        {
            if (IsOpen)
            {
                return;
            }
            IsOpen = true;
            InvalidateContent(); // katalog ya da dil değişmiş olabilir
            InputBlocker.Suspend();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }
            IsOpen = false;
            dragging = false;
            hovered = null;
            GUIUtility.keyboardControl = 0;
            InputBlocker.Restore();
        }

        public void OnGUI()
        {
            if (!IsOpen)
            {
                return;
            }

            KsTheme.EnsureBuilt(fontName.Value);
            if (HandleShortcuts())
            {
                return;
            }

            var previousSkin = GUI.skin;
            var previousMatrix = GUI.matrix;
            GUI.skin = KsTheme.Skin;
            try
            {
                // Oyun sahnesini karart
                KsTheme.Fill(new Rect(0f, 0f, Screen.width, Screen.height),
                    new Color(KsTheme.Backdrop.r, KsTheme.Backdrop.g, KsTheme.Backdrop.b, Mathf.Clamp01(backdropOpacity.Value)));

                float scale = GetScale();
                GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
                float screenW = Screen.width / scale;
                float screenH = Screen.height / scale;
                if (!positioned)
                {
                    windowRect.x = Mathf.Round((screenW - W) / 2f);
                    windowRect.y = Mathf.Round((screenH - H) / 2f);
                    positioned = true;
                }
                HandleDrag();
                windowRect.x = Mathf.Clamp(windowRect.x, 10f, Mathf.Max(10f, screenW - W - 10f));
                windowRect.y = Mathf.Clamp(windowRect.y, 10f, Mathf.Max(10f, screenH - H - 10f));

                DrawFrame(windowRect);

                if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
                {
                    RebuildContentIfNeeded();
                }

                hoveredNextFrame = null;
                GUI.BeginGroup(windowRect);
                KsTheme.Fill(new Rect(0f, 0f, W, H), KsTheme.WindowBg);
                DrawHeader(new Rect(0f, 0f, W, HeaderH));
                DrawSidebar(SidebarRect);
                DrawMain(MainRect);
                DrawInfo(InfoRect);
                DrawFooter(FooterRect);
                GUI.EndGroup();

                if (Event.current.type == EventType.Repaint)
                {
                    hovered = hoveredNextFrame;
                }

                // Pencere üstündeki tıklamalar arkaya geçmesin
                var ev = Event.current;
                if ((ev.type == EventType.MouseDown || ev.type == EventType.MouseUp || ev.type == EventType.ScrollWheel)
                    && windowRect.Contains(ev.mousePosition))
                {
                    ev.Use();
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Pencere çizilemedi: {e}");
                ShowToast(Strings.Error, KsTheme.ItemName);
            }
            finally
            {
                GUI.matrix = previousMatrix;
                GUI.skin = previousSkin;
            }
        }

        // Panel çizimleri partial dosyalarda; henüz yazılmamış olanlar derleyici tarafından atlanır
        partial void DrawSidebar(Rect rect);
        partial void DrawMain(Rect rect);
        partial void DrawInfo(Rect rect);
        partial void DrawFooter(Rect rect);
        partial void RebuildContentIfNeeded();
        partial void InvalidateContent();

        /// <summary>Esc ve Ctrl+F. true dönerse bu olay tüketildi, çizime gerek yok.</summary>
        private bool HandleShortcuts()
        {
            var ev = Event.current;
            if (ev.type != EventType.KeyDown)
            {
                return false;
            }
            if (ev.keyCode == KeyCode.Escape)
            {
                // Önce metin alanından çık, ikinci Esc pencereyi kapatır
                if (GUIUtility.keyboardControl != 0)
                {
                    GUIUtility.keyboardControl = 0;
                }
                else
                {
                    Close();
                }
                ev.Use();
                return true;
            }
            if (ev.keyCode == KeyCode.F && (ev.control || ev.command))
            {
                focusSearchRequested = true;
                ev.Use();
                return false;
            }
            return false;
        }

        private void HandleDrag()
        {
            var ev = Event.current;
            var titleBar = new Rect(windowRect.x, windowRect.y, W - 60f, HeaderH);
            switch (ev.type)
            {
                case EventType.MouseDown when ev.button == 0 && titleBar.Contains(ev.mousePosition):
                    dragging = true;
                    dragOffset = ev.mousePosition - windowRect.position;
                    ev.Use();
                    break;
                case EventType.MouseDrag when dragging:
                    windowRect.position = ev.mousePosition - dragOffset;
                    ev.Use();
                    break;
                case EventType.MouseUp when dragging:
                    dragging = false;
                    ev.Use();
                    break;
            }
        }

        /// <summary>Çelik çerçeve halkaları (3px koyu, 4px çelik, 2px koyu) ve köşe perçinleri.</summary>
        private static void DrawFrame(Rect r)
        {
            if (!KsTheme.Repaint)
            {
                return;
            }
            KsTheme.Fill(new Rect(r.x - 9f, r.y - 9f, r.width + 18f, r.height + 18f), KsTheme.FrameDark);
            KsTheme.Fill(new Rect(r.x - 7f, r.y - 7f, r.width + 14f, r.height + 14f), KsTheme.FrameSteel);
            KsTheme.Fill(new Rect(r.x - 3f, r.y - 3f, r.width + 6f, r.height + 6f), KsTheme.FrameDark);
            KsTheme.RivetAt(r.x - 8f, r.y - 8f);
            KsTheme.RivetAt(r.xMax - 2f, r.y - 8f);
            KsTheme.RivetAt(r.x - 8f, r.yMax - 2f);
            KsTheme.RivetAt(r.xMax - 2f, r.yMax - 2f);
        }

        /// <summary>Başlık: ◆ [logo] KeeperSpawner [v0.4.0] ──◆── [X]</summary>
        private void DrawHeader(Rect r)
        {
            KsTheme.WoodStrip(new Rect(r.x, r.y, r.width, r.height - 2f));
            KsTheme.Fill(new Rect(r.x, r.yMax - 2f, r.width, 2f), KsTheme.PanelEdge);

            float cy = r.y + (r.height - 2f) / 2f;
            float x = r.x + 18f;
            KsTheme.Diamond(new Vector2(x + 4f, cy), 8f, KsTheme.Accent, KsTheme.Hex("#3a2410"));
            x += 8f + 14f;

            DrawLogo(new Rect(x, cy - 16f, 32f, 32f));
            x += 32f + 14f;

            const string title = Plugin.Name;
            var titleSize = KsTheme.Measure(title, 24, true);
            KsTheme.Label(new Rect(x, r.y, titleSize.x + 4f, r.height - 2f), title, 24, KsTheme.TitleText, TextAnchor.MiddleLeft, true,
                KsTheme.StripTextShadow, new Vector2(1f, 2f));
            x += titleSize.x + 14f;

            string version = "v" + Plugin.Version;
            float badgeW = KsTheme.Measure(version, 13).x + 16f;
            var badge = new Rect(x, cy - 11f, badgeW, 22f);
            KsTheme.Draw(KsTheme.VersionBadge, badge);
            KsTheme.Label(badge, version, 13, KsTheme.Hex("#e8cf9a"), TextAnchor.MiddleCenter);
            x += badgeW + 14f;

            var close = new Rect(r.xMax - 10f - 36f, cy - 18f, 36f, 36f);
            DrawCloseButton(close);

            // Süs: uzayan çizgi ◆ 90px çizgi
            float left = x + 12f, right = close.x - 14f - 12f;
            var line = new Color(KsTheme.Hex("#a8865c").r, KsTheme.Hex("#a8865c").g, KsTheme.Hex("#a8865c").b, 0.45f);
            float diaX = right - 90f - 10f - 8f;
            if (diaX - 10f > left)
            {
                KsTheme.Fill(new Rect(left, cy - 1f, diaX - 10f - left, 2f), line);
                KsTheme.Diamond(new Vector2(diaX + 4f, cy), 8f, KsTheme.Accent, KsTheme.Hex("#3a2410"));
                KsTheme.Fill(new Rect(right - 90f, cy - 1f, 90f, 2f), line);
            }
        }

        /// <summary>
        /// Kapat düğmesi: oyunun kendi pencerelerindeki sprite'lar (normal / üstünde / basılı).
        /// Bulunamazlarsa tasarımın kırmızı kare + piksel X çizimi.
        /// </summary>
        private void DrawCloseButton(Rect rect)
        {
            if (GameSprites.TryGet(GameSprites.CloseNormal, out var normal))
            {
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                {
                    Close();
                }
                if (KsTheme.Repaint)
                {
                    bool hover = rect.Contains(Event.current.mousePosition);
                    bool pressed = hover && UnityEngine.Input.GetMouseButton(0);
                    var icon = normal;
                    if (pressed && GameSprites.TryGet(GameSprites.ClosePressed, out var down))
                    {
                        icon = down;
                    }
                    else if (hover && GameSprites.TryGet(GameSprites.CloseHover, out var over))
                    {
                        icon = over;
                    }
                    IconCache.DrawStretched(rect, icon);
                }
                return;
            }

            if (GUI.Button(rect, GUIContent.none, KsTheme.Close))
            {
                Close();
            }
            KsTheme.GlyphClose(new Rect(rect.x + 10f, rect.y + 10f, 16f, 16f), KsTheme.Hex("#fbe3d6"));
        }

        /// <summary>Logo: oyunun sandık ikonu (tasarımdaki altın sandık yer tutucusunun yerine).</summary>
        private static void DrawLogo(Rect rect)
        {
            if (KsTheme.Repaint && IconCache.TryGet(ItemIcons.LogoSprite, out var icon))
            {
                IconCache.Draw(rect, icon);
            }
        }

        private float GetScale()
        {
            float configured = uiScale.Value;
            if (configured > 0f)
            {
                return Mathf.Clamp(configured, 0.5f, 4f);
            }
            // 1080p'de 1:1; dar ekranlarda pencere sığsın
            float byHeight = Screen.height / ReferenceHeight;
            float byWidth = Screen.width / (W + 40f);
            return Mathf.Clamp(Mathf.Min(byHeight, byWidth), 0.5f, 4f);
        }

        // --- Ortak eylemler ---

        private void ShowToast(string text, Color color)
        {
            toast = text;
            toastColor = color;
            toastUntil = Time.unscaledTime + ToastSeconds;
        }

        private bool HasToast => !string.IsNullOrEmpty(toast) && Time.unscaledTime < toastUntil;

        /// <summary>Tık moduna göre eklenecek miktar; Shift basılıysa 1.</summary>
        private int ClickAmountFor(CatalogEntry entry, bool shift)
        {
            if (shift)
            {
                return 1;
            }
            switch (clickAmount.Value)
            {
                case ClickAmount.One: return 1;
                case ClickAmount.Ten: return Math.Min(10, entry.MaxStack);
                default: return entry.MaxStack;
            }
        }

        private void AddItem(CatalogEntry entry, int amount)
        {
            if (!GameState.IsInGame || entry == null)
            {
                return;
            }
            GUIUtility.keyboardControl = 0;
            amount = Mathf.Clamp(amount, 1, 999);
            selectedId = entry.Id;
            amountOverride = null;
            try
            {
                int added = Spawner.Spawn(entry, amount);
                string quality = entry.Star > 0 ? $" ({Strings.StarShort(entry.Star).ToLowerInvariant()})" : string.Empty;
                if (added <= 0)
                {
                    ShowToast(Strings.InventoryFull, KsTheme.ItemName);
                }
                else
                {
                    recent.Push(entry.Id, amount);
                    ShowToast(added < amount
                        ? Strings.AddedPartial(added, amount, entry.DisplayName + quality)
                        : Strings.Added(added, entry.DisplayName + quality), added < amount ? KsTheme.ItemName : KsTheme.Success);
                }
                Plugin.Log.LogInfo($"Spawn {entry.Id}: istenen {amount}, eklenen {added}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Spawn başarısız [{entry.Id}]: {e}");
                ShowToast(Strings.Error, KsTheme.ItemName);
            }
        }

        private void ToggleFavorite(CatalogEntry entry)
        {
            if (entry != null)
            {
                favorites.Toggle(entry.Id);
            }
        }

        private void SelectTab(Tab tab)
        {
            if (tab.Equals(selectedTab))
            {
                return;
            }
            selectedTab = tab;
            lastCategory.Value = tab.Key;
        }

        /// <summary>Sağ panelin gösterdiği item: fare altındaki, yoksa seçili (son tıklanan).</summary>
        private CatalogEntry FocusedEntry
        {
            get
            {
                if (hovered != null)
                {
                    return hovered;
                }
                if (selectedId != null && ItemCatalog.TryGet(selectedId, out var selected))
                {
                    return selected;
                }
                return null;
            }
        }

        /// <summary>Izgara/liste ya da son eklenenler slotu üzerinde fare varsa çizim sırasında çağrılır.</summary>
        private void MarkHovered(CatalogEntry entry)
        {
            hoveredNextFrame = entry;
        }
    }
}
