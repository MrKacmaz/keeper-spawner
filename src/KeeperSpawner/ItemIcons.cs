using System.Collections.Generic;

namespace KeeperSpawner
{
    /// <summary>
    /// Logo ve kategori ikonları için oyunun gerçek item sprite'ları. Tasarımdaki çizilmiş ikonlar yer
    /// tutucuydu; her kategori onu en iyi temsil eden bir itemın ikonunu kullanıyor.
    /// </summary>
    internal static class ItemIcons
    {
        private static readonly Dictionary<string, string> SpriteByItem = new Dictionary<string, string>();

        public static string LogoSprite => SpriteOf("box_empty");
        public static string AllSprite => SpriteOf("box_empty");
        public static string FavoritesSprite => SpriteOf("gem");
        public static string RecentSprite => SpriteOf("book");

        public static string ForCategory(ItemCategory category)
        {
            switch (category)
            {
                case ItemCategory.Building: return SpriteOf("wooden_plank");
                case ItemCategory.Food: return SpriteOf("bread");
                case ItemCategory.Farming: return SpriteOf("wheat");
                case ItemCategory.Alchemy: return SpriteOf("alchemy_bottle_1");
                case ItemCategory.Potions: return SpriteOf("heal_potion");
                case ItemCategory.Tools: return SpriteOf("hammer_2");
                case ItemCategory.Equipment: return SpriteOf("sword_2");
                case ItemCategory.Fishing: return SpriteOf("fish_carp");
                case ItemCategory.BodyParts: return SpriteOf("heart_0_1:1");
                case ItemCategory.Graves: return SpriteOf("grave_top_stn_1");
                case ItemCategory.Church: return SpriteOf("candle_basic");
                case ItemCategory.Papers: return SpriteOf("clean_paper");
                case ItemCategory.Bags: return SpriteOf("bag_universal");
                case ItemCategory.Quest: return SpriteOf("quest_bell");
                default: return SpriteOf("flag_blue");
            }
        }

        /// <summary>Item id'sinden sprite adı; item yoksa (oyun güncellemesiyle kaldırıldıysa) null.</summary>
        public static string SpriteOf(string itemId)
        {
            if (SpriteByItem.TryGetValue(itemId, out var sprite))
            {
                return sprite;
            }
            sprite = GameBalance.Me?.GetDataOrNull<ItemDef>(itemId)?.iconId;
            SpriteByItem[itemId] = sprite;
            return sprite;
        }
    }
}
