using LazyBearTechnology;

namespace KeeperSpawner
{
    /// <summary>Mod arayüzü metinleri: oyun Türkçeyse TR, değilse EN.</summary>
    internal static class Strings
    {
        public static bool IsTr => LLBase.CurrentLang == "tr";

        public static string Search => IsTr ? "Ara:" : "Search:";
        public static string Clear => IsTr ? "Temizle" : "Clear";
        public static string ShowQuestItems => IsTr ? "Görev itemlarını göster" : "Show quest items";
        public static string Quest => IsTr ? "görev" : "quest";
        public static string All => IsTr ? "Tümü" : "All";
        public static string Grid => IsTr ? "Izgara" : "Grid";
        public static string List => IsTr ? "Liste" : "List";
        public static string Favorites => IsTr ? "Favoriler" : "Favorites";
        public static string Recent => IsTr ? "Son" : "Recent";
        public static string RecentSection => IsTr ? "Son eklenenler" : "Recently added";
        public static string FavoriteMark => IsTr ? "favori" : "favorite";
        public static string Hint => IsTr
            ? "Tıkla: max stack ekle  •  Sağ tık: favori  •  Esc: kapat"
            : "Click: add max stack  •  Right-click: favorite  •  Esc: close";
        public static string NoResults => IsTr ? "Sonuç yok" : "No results";
        public static string NoFavorites => IsTr
            ? "Henüz favori yok. Bir itema sağ tıklayarak favorilere ekleyebilirsin."
            : "No favorites yet. Right-click an item to add it.";
        public static string NoRecent => IsTr ? "Henüz item eklenmedi." : "Nothing added yet.";

        public static string FavoriteAdded(string name) => IsTr ? $"Favorilere eklendi: {name}" : $"Added to favorites: {name}";
        public static string FavoriteRemoved(string name) => IsTr ? $"Favorilerden çıkarıldı: {name}" : $"Removed from favorites: {name}";

        public static string Star(int star)
        {
            switch (star)
            {
                case 1: return IsTr ? "Bronz yıldız" : "Bronze star";
                case 2: return IsTr ? "Gümüş yıldız" : "Silver star";
                case 3: return IsTr ? "Altın yıldız" : "Gold star";
                default: return IsTr ? $"{star} yıldız" : $"{star} stars";
            }
        }

        public static string ItemCount(int shown, int total) =>
            IsTr ? $"{shown} / {total} item" : $"{shown} / {total} items";

        public static string Added(int added, string name) =>
            $"+{added} {name}";

        public static string Partial(int added, int requested, string name) =>
            IsTr ? $"Envanter dolu: {added}/{requested} {name} eklendi"
                 : $"Inventory full: added {added}/{requested} {name}";

        public static string InventoryFull => IsTr ? "Envanter dolu" : "Inventory full";

        public static string Error => IsTr ? "Hata, ayrıntılar BepInEx logunda" : "Error, see BepInEx log";
    }
}
