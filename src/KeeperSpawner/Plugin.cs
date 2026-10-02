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
        public const string Version = "1.0.0";

        // Tasarım: son eklenenler en fazla 5 (HANDOFF.md §5)
        private const int RecentLimit = 5;
        // Uyumluluk hatası olmayan hatalar da bu kadar tekrarlanırsa log dolmasın diye mod kapanır
        private const int MaxErrors = 30;
        private const float NoticeSeconds = 6f;

        internal static ManualLogSource Log;

        /// <summary>İsteğe bağlı GK2 Mod Framework köprüsü (KeeperSpawner.GK2Framework) bu örneği kullanır.</summary>
        internal static Plugin Instance { get; private set; }

        // Köprü bu girdileri Mods menüsünde gösterir; mod devre dışı kalsa da bağlı olmaları için Awake'in başında bağlanırlar
        internal ConfigEntry<KeyboardShortcut> ToggleKey { get; private set; }
        internal ConfigEntry<bool> ShowQuestItems { get; private set; }
        internal ConfigEntry<ClickAmount> ClickAmount { get; private set; }
        internal ConfigEntry<ViewMode> View { get; private set; }
        internal ConfigEntry<float> UiScale { get; private set; }
        internal ConfigEntry<float> BackdropOpacity { get; private set; }

        internal bool IsDisabled => disabled;

        private ConfigEntry<bool> forceIncompatible;
        private ConfigEntry<string> lastCategory;
        private ConfigEntry<string> fontName;
        private ConfigEntry<string> favorites;
        private ConfigEntry<string> recent;
        private SpawnerWindow window;

        private bool disabled;
        private int errorCount;
        private readonly HashSet<string> loggedErrors = new HashSet<string>();
        private float noticeUntil;
        private GUIStyle noticeStyle;

        private void Awake()
        {
            Log = Logger;
            Instance = this;
            BindConfig();

            Log.LogInfo($"{Name} {Version} loading. Game version: {GameCompat.RunningGameVersion()} (tested: {GameCompat.TestedGameVersion})");

            bool compatible;
            try
            {
                compatible = GameCompat.Check();
            }
            catch (Exception e)
            {
                Log.LogError($"Compatibility check failed: {e}");
                compatible = false;
            }
            if (forceIncompatible.Value)
            {
                Log.LogWarning("Debug.ForceIncompatible is on, disabling the mod.");
                compatible = false;
            }
            if (!compatible)
            {
                Disable("not compatible with this game version");
                return;
            }

            try
            {
                CreateWindow();
                Strings.Validate();
            }
            catch (Exception e)
            {
                HandleError(e, "Startup");
                if (window == null)
                {
                    Disable("could not start");
                    return;
                }
            }
            Log.LogInfo($"{Name} {Version} loaded. Press {ToggleKey.Value} in game to open the spawner.");
        }

        private void BindConfig()
        {
            ToggleKey = Config.Bind("General", "ToggleKey", new KeyboardShortcut(KeyCode.O),
                "Spawner menüsünü açıp kapatan tuş / Key that toggles the spawner menu.");
            forceIncompatible = Config.Bind("Debug", "ForceIncompatible", false,
                "Test için: modu uyumsuz oyun sürümündeymiş gibi devre dışı bırakır / For testing: disables the mod as if the game version were incompatible.");
            ShowQuestItems = Config.Bind("Items", "ShowQuestItems", false,
                "Görev itemlarını listede göster (kayıtları bozabilir) / Show quest items in the list (may break quests).");
            UiScale = Config.Bind("UI", "Scale", 0f,
                "Arayüz ölçeği. 0 = otomatik (ekran yüksekliği / 1080) / UI scale. 0 = automatic (screen height / 1080).");
            View = Config.Bind("UI", "View", ViewMode.Grid,
                "Item görünümü: Grid (ikonlu ızgara) ya da List / Item view: Grid (icons) or List.");
            ClickAmount = Config.Bind("UI", "ClickAmount", KeeperSpawner.ClickAmount.Max,
                "Tıklayınca eklenen miktar: One, Ten, Max / Amount added per click: One, Ten, Max.");
            lastCategory = Config.Bind("UI", "Category", "All",
                "Son seçili kategori (menüde değişir) / Last selected category (changed from the menu).");
            fontName = Config.Bind("UI", "Font", string.Empty,
                "Arayüz fontu. Boş = otomatik (oyunun piksel fontu), \"-\" = Unity varsayılanı, ya da logdaki font adlarından biri / UI font. Empty = automatic, \"-\" = Unity default, or a font name from the log.");
            BackdropOpacity = Config.Bind("UI", "BackdropOpacity", 0.55f,
                "Pencere açıkken oyun sahnesini karartma oranı (0 - 1) / How much the game scene is dimmed behind the window (0 - 1).");
            favorites = Config.Bind("Items", "Favorites", string.Empty,
                "Favori item id'leri, virgülle ayrılmış (menüde sağ tıkla düzenlenir) / Favorite item ids, comma separated (right-click in the menu).");
            recent = Config.Bind("Items", "Recent", string.Empty,
                "Son eklenen itemlar, en yeniden eskiye (id=adet) / Recently added items, newest first (id=amount).");
        }

        private void CreateWindow()
        {
            window = new SpawnerWindow(ShowQuestItems, UiScale, View, ClickAmount, lastCategory, fontName,
                BackdropOpacity, new IdList(favorites), new RecentList(recent, RecentLimit));
        }

        private void Update()
        {
            if (disabled)
            {
                // Kullanıcı kısayola basarsa neden açılmadığını görsün
                if (ToggleKey.Value.IsDown())
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

                if (ToggleKey.Value.IsDown())
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
                Log.LogError($"{where} error: {e}");
            }

            if (GameCompat.IsCompatibilityError(e))
            {
                Disable("game API missing: " + e.Message);
            }
            else if (errorCount >= MaxErrors)
            {
                Disable($"too many errors ({errorCount})");
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
            Log.LogError($"{Name} disabled: {reason}. Game version: {GameCompat.RunningGameVersion()}, tested: {GameCompat.TestedGameVersion}.");
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
