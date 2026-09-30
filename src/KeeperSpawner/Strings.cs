using LazyBearTechnology;

namespace KeeperSpawner
{
    /// <summary>Mod arayüzü metinleri: oyun Türkçeyse TR, değilse EN.</summary>
    internal static class Strings
    {
        public static bool IsTr => LLBase.CurrentLang == "tr";

        // Sol panel
        public static string Categories => IsTr ? "Kategoriler" : "Categories";
        public static string All => IsTr ? "Tümü" : "All";
        public static string Favorites => IsTr ? "Favoriler" : "Favorites";
        public static string Recent => IsTr ? "Son eklenenler" : "Recently added";

        // Araç çubuğu ve filtreler
        public static string SearchPlaceholder => IsTr ? "İsim veya id ara  (ör. kalas, ingot_iron)" : "Search name or id  (e.g. plank, ingot_iron)";
        public static string Click => IsTr ? "Tık:" : "Click:";
        public static string Max => IsTr ? "Maks" : "Max";
        public static string Quality => IsTr ? "Kalite" : "Quality";
        public static string QualityAll => IsTr ? "Hepsi" : "All";
        public static string ShowQuestItems => IsTr ? "Görev itemlarını göster" : "Show quest items";
        public static string ItemCount(int shown, int total) =>
            $"<color=#f2c94c>{shown}</color> / {total} " + (IsTr ? "item" : "items");

        // İçerik
        public static string ColumnItem => IsTr ? "Item" : "Item";
        public static string ColumnId => "ID";
        public static string ColumnCategory => IsTr ? "Kategori" : "Category";
        public static string ColumnStack => IsTr ? "Yığın" : "Stack";
        public static string EmptyTitle => IsTr ? "Bu filtrelerle item bulunamadı" : "No items match these filters";
        public static string EmptyHint => IsTr ? "Aramayı veya kalite filtresini değiştirmeyi dene." : "Try changing the search or quality filter.";
        public static string ResetFilters => IsTr ? "Filtreleri sıfırla" : "Reset filters";
        public static string Quest => IsTr ? "görev" : "quest";

        // Bilgi paneli
        public static string ItemInfo => IsTr ? "Item Bilgisi" : "Item Info";
        public static string MaxStack => IsTr ? "Maks yığın" : "Max stack";
        public static string IdCopied => IsTr ? "ID panoya kopyalandı" : "ID copied to clipboard";
        public static string Amount => IsTr ? "Miktar" : "Amount";
        public static string AddToInventory(int amount) => (IsTr ? "Envantere ekle" : "Add to inventory") + "  ×" + amount;
        public static string AddFavorite => IsTr ? "Favorilere ekle" : "Add to favorites";
        public static string InFavorites => IsTr ? "Favorilerde" : "In favorites";
        public static string RecentTitle => IsTr ? "Son Eklenenler" : "Recently Added";
        public static string NoRecent => IsTr ? "Henüz bir şey eklemedin." : "Nothing added yet.";

        public static string StarShort(int star)
        {
            switch (star)
            {
                case 1: return IsTr ? "Bronz" : "Bronze";
                case 2: return IsTr ? "Gümüş" : "Silver";
                case 3: return IsTr ? "Altın" : "Gold";
                default: return string.Empty;
            }
        }

        public static string Star(int star)
        {
            switch (star)
            {
                case 1: return IsTr ? "Bronz yıldız" : "Bronze star";
                case 2: return IsTr ? "Gümüş yıldız" : "Silver star";
                case 3: return IsTr ? "Altın yıldız" : "Gold star";
                default: return string.Empty;
            }
        }

        // Alt çubuk
        public static string Added(int added, string name) =>
            IsTr ? $"+{added}  {name}  envantere eklendi" : $"+{added}  {name}  added to inventory";
        public static string AddedPartial(int added, int requested, string name) =>
            IsTr ? $"+{added}/{requested}  {name}  eklendi, envanter doldu" : $"+{added}/{requested}  {name}  added, inventory full";
        public static string InventoryFull => IsTr ? "Envanter dolu" : "Inventory full";
        public static string Error => IsTr ? "Hata, ayrıntılar BepInEx logunda" : "Error, see BepInEx log";

        public static string KeyClick => IsTr ? "Tık" : "Click";
        public static string KeyShiftClick => IsTr ? "Shift+Tık" : "Shift+Click";
        public static string KeyRightClick => IsTr ? "Sağ tık" : "Right-click";
        public static string ClickHint(ClickAmount mode)
        {
            switch (mode)
            {
                case ClickAmount.One: return IsTr ? "1 adet ekle" : "add 1";
                case ClickAmount.Ten: return IsTr ? "10 adet ekle" : "add 10";
                default: return IsTr ? "maks yığın ekle" : "add max stack";
            }
        }
        public static string OneItem => IsTr ? "1 adet" : "1 item";
        public static string FavoriteHint => IsTr ? "favori" : "favorite";
        public static string SearchHint => IsTr ? "ara" : "search";
        public static string CloseHint => IsTr ? "kapat" : "close";
    }
}
