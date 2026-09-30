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
        public const string Version = "0.3.0";

        private const int RecentLimit = 24;

        internal static ManualLogSource Log;

        private ConfigEntry<KeyboardShortcut> toggleKey;
        private SpawnerWindow window;

        private void Awake()
        {
            Log = Logger;

            toggleKey = Config.Bind("General", "ToggleKey", new KeyboardShortcut(KeyCode.O),
                "Spawner menüsünü açıp kapatan tuş / Key that toggles the spawner menu.");
            var showQuestItems = Config.Bind("Items", "ShowQuestItems", false,
                "Görev itemlarını listede göster (kayıtları bozabilir) / Show quest items in the list (may break quests).");
            var uiScale = Config.Bind("UI", "Scale", 0f,
                "Arayüz ölçeği. 0 = otomatik (ekran yüksekliği / 1080) / UI scale. 0 = automatic (screen height / 1080).");
            var viewMode = Config.Bind("UI", "View", ViewMode.Grid,
                "Item görünümü: Grid (ikonlu ızgara) ya da List / Item view: Grid (icons) or List.");
            var backgroundOpacity = Config.Bind("UI", "BackgroundOpacity", 0.97f,
                "Pencere arka planının opaklığı (0.2 - 1) / Window background opacity (0.2 - 1).");
            var favorites = new IdList(Config.Bind("Items", "Favorites", string.Empty,
                "Favori item id'leri, virgülle ayrılmış (menüde sağ tıkla düzenlenir) / Favorite item ids, comma separated (right-click in the menu)."));
            var recent = new IdList(Config.Bind("Items", "Recent", string.Empty,
                "Son eklenen item id'leri, en yeniden eskiye / Recently added item ids, newest first."), RecentLimit);

            window = new SpawnerWindow(showQuestItems, uiScale, viewMode, backgroundOpacity, favorites, recent);

            Log.LogInfo($"{Name} {Version} yüklendi. Kısayol: {toggleKey.Value}");
        }

        private void Update()
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

        private void OnGUI()
        {
            window.OnGUI();
        }

        private void OnDestroy()
        {
            window?.Close();
        }
    }
}
