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
        public const string Version = "0.1.0";

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

            window = new SpawnerWindow(showQuestItems, uiScale);

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
