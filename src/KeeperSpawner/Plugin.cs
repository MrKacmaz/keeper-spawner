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
