using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace KeeperSpawner
{
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "com.mrkacmaz.keeperspawner";
        public const string Name = "KeeperSpawner";
        public const string Version = "0.4.0";

        // Tasarım: son eklenenler en fazla 5 (HANDOFF.md §5)
        private const int RecentLimit = 5;
        // Uyumluluk hatası olmayan hatalar da bu kadar tekrarlanırsa log dolmasın diye mod kapanır
        private const int MaxErrors = 30;
        private const float NoticeSeconds = 6f;

        internal static ManualLogSource Log;

        private ConfigEntry<KeyboardShortcut> toggleKey;
        private SpawnerWindow window;

        private bool disabled;
        private int errorCount;
        private readonly HashSet<string> loggedErrors = new HashSet<string>();
        private float noticeUntil;
        private GUIStyle noticeStyle;

        private void Awake()
        {
            Log = Logger;

            toggleKey = Config.Bind("General", "ToggleKey", new KeyboardShortcut(KeyCode.O),
                "Spawner menüsünü açıp kapatan tuş / Key that toggles the spawner menu.");
            var forceIncompatible = Config.Bind("Debug", "ForceIncompatible", false,
                "Test için: modu uyumsuz oyun sürümündeymiş gibi devre dışı bırakır / For testing: disables the mod as if the game version were incompatible.");

            Log.LogInfo($"{Name} {Version} yükleniyor. Oyun sürümü: {GameCompat.RunningGameVersion()} (test edilen: {GameCompat.TestedGameVersion})");

            bool compatible;
            try
            {
                compatible = GameCompat.Check();
            }
            catch (Exception e)
            {
                Log.LogError($"Uyumluluk kontrolü başarısız: {e}");
                compatible = false;
            }
            if (forceIncompatible.Value)
            {
                Log.LogWarning("Debug.ForceIncompatible açık, mod devre dışı bırakılıyor.");
                compatible = false;
            }
            if (!compatible)
            {
                Disable("oyun sürümüyle uyumsuz");
                return;
            }

            try
            {
                CreateWindow();
                Strings.Validate();
            }
            catch (Exception e)
            {
                HandleError(e, "başlatma");
                if (window == null)
                {
                    Disable("başlatılamadı");
                    return;
                }
            }
            Log.LogInfo($"{Name} {Version} yüklendi. Kısayol: {toggleKey.Value}");
        }

        private void CreateWindow()
        {
            var showQuestItems = Config.Bind("Items", "ShowQuestItems", false,
                "Görev itemlarını listede göster (kayıtları bozabilir) / Show quest items in the list (may break quests).");
            var uiScale = Config.Bind("UI", "Scale", 0f,
                "Arayüz ölçeği. 0 = otomatik (ekran yüksekliği / 1080) / UI scale. 0 = automatic (screen height / 1080).");
            var viewMode = Config.Bind("UI", "View", ViewMode.Grid,
                "Item görünümü: Grid (ikonlu ızgara) ya da List / Item view: Grid (icons) or List.");
            var clickAmount = Config.Bind("UI", "ClickAmount", ClickAmount.Max,
                "Tıklayınca eklenen miktar: One, Ten, Max / Amount added per click: One, Ten, Max.");
            var lastCategory = Config.Bind("UI", "Category", "All",
                "Son seçili kategori (menüde değişir) / Last selected category (changed from the menu).");
            var fontName = Config.Bind("UI", "Font", string.Empty,
                "Arayüz fontu. Boş = otomatik (oyunun piksel fontu), \"-\" = Unity varsayılanı, ya da logdaki font adlarından biri / UI font. Empty = automatic, \"-\" = Unity default, or a font name from the log.");
            var backdropOpacity = Config.Bind("UI", "BackdropOpacity", 0.55f,
                "Pencere açıkken oyun sahnesini karartma oranı (0 - 1) / How much the game scene is dimmed behind the window (0 - 1).");
            var favorites = new IdList(Config.Bind("Items", "Favorites", string.Empty,
                "Favori item id'leri, virgülle ayrılmış (menüde sağ tıkla düzenlenir) / Favorite item ids, comma separated (right-click in the menu)."));
            var recent = new RecentList(Config.Bind("Items", "Recent", string.Empty,
                "Son eklenen itemlar, en yeniden eskiye (id=adet) / Recently added items, newest first (id=amount)."), RecentLimit);

            window = new SpawnerWindow(showQuestItems, uiScale, viewMode, clickAmount, lastCategory, fontName,
                backdropOpacity, favorites, recent);
        }

        private void Update()
        {
            if (disabled)
            {
                // Kullanıcı kısayola basarsa neden açılmadığını görsün
                if (toggleKey.Value.IsDown())
                {
                    noticeUntil = Time.unscaledTime + NoticeSeconds;
                }
                return;
            }

            try
            {
                if (!GameState.IsInGame)
                {
                    // Ana menüye dönüldüyse ya da kayıt kapandıysa pencereyi kapat, girdiyi geri ver
                    window.Close();
                    return;
                }

                // Arama kutusuna yazarken kısayol harfi menüyü kapatmasın
                if (window.IsTyping)
                {
                    return;
                }

                if (toggleKey.Value.IsDown())
                {
                    window.Toggle();
                }
            }
            catch (Exception e)
            {
                HandleError(e, "Update");
            }
        }

        private void OnGUI()
        {
            if (disabled)
            {
                DrawDisabledNotice();
                return;
            }
            try
            {
                window.OnGUI();
            }
            catch (Exception e)
            {
                HandleError(e, "OnGUI");
            }
        }

        private void OnDestroy()
        {
            try
            {
                window?.Close();
            }
            catch (Exception)
            {
                // kapanışta sessiz
            }
        }

        /// <summary>
        /// Oyun API'si eksik hatası (güncelleme sonrası) gelirse modu hemen kapatır; diğer hataları
        /// her biri bir kez loglar ve çok tekrarlanırsa yine kapatır.
        /// </summary>
        private void HandleError(Exception e, string where)
        {
            errorCount++;
            string key = where + "|" + e.GetType().Name + "|" + e.Message;
            if (loggedErrors.Add(key))
            {
                Log.LogError($"{where} hatası: {e}");
            }

            if (GameCompat.IsCompatibilityError(e))
            {
                Disable("oyun API'si eksik: " + e.Message);
            }
            else if (errorCount >= MaxErrors)
            {
                Disable($"çok fazla hata ({errorCount})");
            }
        }

        private void Disable(string reason)
        {
            if (disabled)
            {
                return;
            }
            disabled = true;
            try
            {
                window?.Close(); // girdiyi oyuna geri ver
            }
            catch (Exception)
            {
                // girdi sistemi de bozuksa yapacak bir şey yok
            }
            Log.LogError($"{Name} devre dışı: {reason}. Oyun sürümü: {GameCompat.RunningGameVersion()}, test edilen: {GameCompat.TestedGameVersion}.");
        }

        private void DrawDisabledNotice()
        {
            if (Time.unscaledTime >= noticeUntil || Event.current.type != EventType.Repaint)
            {
                return;
            }
            string text;
            try
            {
                text = Strings.DisabledNotice;
            }
            catch (Exception)
            {
                text = Strings.DisabledNoticeFallback;
            }
            if (noticeStyle == null)
            {
                noticeStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 16,
                    wordWrap = true,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(16, 16, 10, 10),
                };
            }
            float width = Mathf.Min(720f, Screen.width - 40f);
            float height = noticeStyle.CalcHeight(new GUIContent(text), width);
            GUI.Box(new Rect((Screen.width - width) / 2f, 40f, width, height), text, noticeStyle);
        }
    }
}
