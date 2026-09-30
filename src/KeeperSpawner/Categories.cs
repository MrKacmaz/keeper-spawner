using System;
using System.Collections.Generic;

namespace KeeperSpawner
{
    internal enum ItemCategory
    {
        Building,
        Food,
        Farming,
        Alchemy,
        Potions,
        Tools,
        Equipment,
        Fishing,
        BodyParts,
        Graves,
        Church,
        Papers,
        Bags,
        Quest,
        Other,
    }

    /// <summary>
    /// Itemları tek bir kategoriye yerleştirir. Oyunun grup etiketleri (itemGroupIds) birçok itemda
    /// eksik olduğu için id kalıplarıyla tamamlanıyor. Kurallar items.tsv dökümü üzerinde denendi:
    /// 726 görünür itemdan sadece 8'i "Diğer"e düşüyor.
    /// </summary>
    internal static class Categories
    {
        /// <summary>Izgarada bölümlerin ve sekmelerin sırası (enum sırası).</summary>
        public static readonly ItemCategory[] DisplayOrder = (ItemCategory[])Enum.GetValues(typeof(ItemCategory));

        private static readonly HashSet<string> FarmingGroups = new HashSet<string> { "seed", "seedable", "crop", "farm_bag", "fertilizer" };

        private static readonly HashSet<string> ChurchIds = new HashSet<string> { "carpet", "prosperity_item", "music_notes" };
        private static readonly HashSet<string> FoodIds = new HashSet<string> { "fried_egg", "boiled_egg", "moonshine", "spices", "mash" };
        private static readonly HashSet<string> PaperIds = new HashSet<string> { "clean_paper", "ink", "green_scroll" };
        private static readonly HashSet<string> BuildingIds = new HashSet<string>
        {
            "bottle_empty", "glass_bottle_empty", "polishing_paste", "frill", "imported_components", "ev_needles",
        };
        private static readonly HashSet<string> AlchemyIds = new HashSet<string>
        {
            "quality_items_silver", "quality_items_gold", "dust_explosive", "catalyst", "powder_gun", "acid", "glue",
            "black_oil", "resin", "dye", "aqua_vitae", "quicksilver", "brimstone", "cinnabar", "alum", "camphor",
            "homunculus", "slug", "mold_green", "zombie_power",
        };

        private static readonly string[] ChurchPrefixes = { "candle_", "incense_" };
        private static readonly string[] FoodPrefixes = { "cook_", "cognac" };
        private static readonly string[] PaperPrefixes =
        {
            "book", "notebook", "techbook_", "story", "newspaper", "science_kit_", "skroll_", "paper_",
            "body_certificate", "exhume_certificate",
        };
        private static readonly string[] BuildingPrefixes =
        {
            "copper_", "ingot_", "nails_", "details_", "detail_", "wooden_", "wood_", "thread_", "fabric_",
            "ceramic_", "barricade_", "tower_",
        };

        public static ItemCategory Classify(ItemDef def)
        {
            string id = def.id;
            var groups = def.itemGroupIds ?? new List<string>();
            var type = def.type;

            if (def.isQuestItem) return ItemCategory.Quest;
            if (type == ItemType.Preach || HasPrefix(id, ChurchPrefixes) || ChurchIds.Contains(id)) return ItemCategory.Church;
            if (def.isBag || type == ItemType.Bag) return ItemCategory.Bags;
            if (groups.Contains("tool")) return ItemCategory.Tools;
            if (groups.Contains("weapon") || type == ItemType.BodyArmor || type == ItemType.Collar
                || id.StartsWith("hero_clothes_", StringComparison.Ordinal)) return ItemCategory.Equipment;
            if (groups.Contains("bodypart")) return ItemCategory.BodyParts;
            if (groups.Contains("gravetop") || groups.Contains("gravebot")
                || id.StartsWith("urn_ashes", StringComparison.Ordinal)) return ItemCategory.Graves;
            if (groups.Contains("battle_potion") || groups.Contains("pot_bag") || type == ItemType.Embalm
                || id.Contains("potion")) return ItemCategory.Potions;
            if (groups.Exists(FarmingGroups.Contains)) return ItemCategory.Farming;
            if (groups.Contains("fishes") || groups.Contains("fish_bag") || type == ItemType.Bait) return ItemCategory.Fishing;
            if (groups.Contains("food_bag") || HasPrefix(id, FoodPrefixes) || FoodIds.Contains(id)) return ItemCategory.Food;
            if (HasPrefix(id, PaperPrefixes) || PaperIds.Contains(id)) return ItemCategory.Papers;
            if (groups.Contains("buil_bag") || HasPrefix(id, BuildingPrefixes) || BuildingIds.Contains(id)) return ItemCategory.Building;
            if (groups.Contains("alch_bag") || AlchemyIds.Contains(id)) return ItemCategory.Alchemy;
            // quest_bag'de kakao gibi sıradan malzemeler de var; onlar yukarıda yakalandı
            if (groups.Contains("quest_bag")) return ItemCategory.Quest;
            return ItemCategory.Other;
        }

        /// <summary>Kategorinin oyunun aktif dilindeki adı (çeviriler Strings'te).</summary>
        public static string Name(ItemCategory category) => Strings.Category(category);

        private static bool HasPrefix(string id, string[] prefixes)
        {
            foreach (var prefix in prefixes)
            {
                if (id.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
