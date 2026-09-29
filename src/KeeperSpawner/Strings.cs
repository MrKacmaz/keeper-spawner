using LazyBearTechnology;

namespace KeeperSpawner
{
    /// <summary>Mod arayüzü metinleri: oyun Türkçeyse TR, değilse EN.</summary>
    internal static class Strings
    {
        private static bool Tr => LLBase.CurrentLang == "tr";

        public static string Search => Tr ? "Ara:" : "Search:";
        public static string Clear => Tr ? "Temizle" : "Clear";
        public static string ShowQuestItems => Tr ? "Görev itemlarını göster" : "Show quest items";
        public static string Quest => Tr ? "görev" : "quest";
        public static string Hint => Tr ? "Tıkla: max stack ekle  •  Esc: kapat" : "Click: add max stack  •  Esc: close";
        public static string NoResults => Tr ? "Sonuç yok" : "No results";

        public static string ItemCount(int shown, int total) =>
            Tr ? $"{shown} / {total} item" : $"{shown} / {total} items";

        public static string Added(int added, string name) =>
            $"+{added} {name}";

        public static string Partial(int added, int requested, string name) =>
            Tr ? $"Envanter dolu: {added}/{requested} {name} eklendi"
               : $"Inventory full: added {added}/{requested} {name}";

        public static string InventoryFull => Tr ? "Envanter dolu" : "Inventory full";

        public static string Error => Tr ? "Hata, ayrıntılar BepInEx logunda" : "Error, see BepInEx log";
    }
}
