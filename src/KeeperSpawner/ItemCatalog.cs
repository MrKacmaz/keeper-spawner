using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using LazyBearTechnology;

namespace KeeperSpawner
{
    internal sealed class CatalogEntry
    {
        public ItemDef Def;
        public string Id;
        /// <summary>IMGUI'de gösterilecek temiz ad (TextMeshPro etiketleri çıkarılmış).</summary>
        public string DisplayName;
        /// <summary>Oyunun kendi adı; oyun bildirimleri TextMeshPro kullandığı için etiketlerle birlikte.</summary>
        public string RichName;
        public string SearchKey;
        public int MaxStack;
        public bool IsQuest;
        public ItemCategory Category;
        /// <summary>Yıldızlı kalite (1 bronz, 2 gümüş, 3 altın); yıldızsız itemlarda 0.</summary>
        public int Star;
        /// <summary>Oyunun yıldız sprite'ı ("item_star_N"), yıldızsızsa null.</summary>
        public string StarIconId;
    }

    /// <summary>
    /// GameBalance.Me.itemDefs listesini bir kez okuyup filtreler ve önbelleğe alır.
    /// Oyunun dili değişirse adlar yeniden kurulur. Kurallar: docs/game-api.md
    /// </summary>
    internal static class ItemCatalog
    {
        private static readonly HashSet<string> HiddenIds = new HashSet<string>
        {
            // envanter kapları
            "empty", "inventory", "toolBeltInventory", "craftInventory",
            // envanter itemı değil, oyun kaynağı
            "faith", "temptation", "fire", "alchemy_flask", "science", "town_happiness",
            // geliştirici artıkları ve iç kullanım
            "wood_OLD", "chair", "saw", "fake_porter_slot_filler", "hand_tool", "mouth_spit", "no_bait",
        };

        private static readonly string[] HiddenPrefixes = { "test_", "pseudo_", "game_res_", "npc_" };
        private static readonly string[] HiddenSuffixes = { "_test" };

        // "overhead": başta taşınan büyük itemlar (kütük, taş, ceset, tedarik sandığı), envantere girmez
        private static readonly HashSet<string> HiddenGroups = new HashSet<string> { "overhead" };

        private static readonly Regex SpriteTag = new Regex("<sprite name=\"([^\"]+)\">", RegexOptions.Compiled);
        private static readonly Regex AnyTag = new Regex("<[^>]+>", RegexOptions.Compiled);

        private static List<CatalogEntry> entries;
        private static readonly Dictionary<string, CatalogEntry> ById = new Dictionary<string, CatalogEntry>();

        /// <summary>Katalogdaki (gizli olmayan) bir itemı id ile bulur.</summary>
        public static bool TryGet(string id, out CatalogEntry entry)
        {
            entry = null;
            return id != null && All != null && ById.TryGetValue(id, out entry);
        }
        private static string builtForLang;

        public static IReadOnlyList<CatalogEntry> All
        {
            get
            {
                if (entries == null || builtForLang != LLBase.CurrentLang)
                {
                    Build();
                }
                return entries;
            }
        }

        private static void Build()
        {
            var list = new List<CatalogEntry>();
            int skipped = 0;
            foreach (var def in GameBalance.Me.itemDefs)
            {
                if (def == null || string.IsNullOrEmpty(def.id) || IsHidden(def))
                {
                    skipped++;
                    continue;
                }
                try
                {
                    list.Add(CreateEntry(def));
                }
                catch (Exception e)
                {
                    skipped++;
                    Plugin.Log.LogWarning($"Skipped item [{def.id}]: {e.Message}");
                }
            }
            list.Sort((a, b) =>
            {
                int byName = string.Compare(a.DisplayName, b.DisplayName, StringComparison.InvariantCultureIgnoreCase);
                return byName != 0 ? byName : string.CompareOrdinal(a.Id, b.Id);
            });

            entries = list;
            ById.Clear();
            foreach (var entry in list)
            {
                ById[entry.Id] = entry;
            }
            builtForLang = LLBase.CurrentLang;
            Plugin.Log.LogInfo($"Catalog built: {list.Count} items, {skipped} hidden (language: {builtForLang})");
        }

        private static bool IsHidden(ItemDef def)
        {
            if (HiddenIds.Contains(def.id))
            {
                return true;
            }
            foreach (var prefix in HiddenPrefixes)
            {
                if (def.id.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            foreach (var suffix in HiddenSuffixes)
            {
                if (def.id.EndsWith(suffix, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            if (def.itemGroupIds != null)
            {
                foreach (var group in def.itemGroupIds)
                {
                    if (HiddenGroups.Contains(group))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static CatalogEntry CreateEntry(ItemDef def)
        {
            string rich = def.GetHeader();
            if (string.IsNullOrEmpty(rich))
            {
                rich = def.id;
            }

            string display = SpriteTag.Replace(rich, m => "[" + ShortSpriteName(m.Groups[1].Value) + "]");
            display = AnyTag.Replace(display, "").Trim();

            // Yıldız kalite varyantları (id:1, id:2, id:3) aynı adı taşıyor; oyun gibi yıldız ikonuyla
            // ayırt ediliyorlar (UIItemCell: qualityType == Star ise "item_star_" + quality)
            int star = def.qualityType == ItemDef.QualityType.Star ? Math.Max(0, def.quality) : 0;

            return new CatalogEntry
            {
                Def = def,
                Id = def.id,
                DisplayName = display,
                RichName = rich,
                SearchKey = Fold(display + " " + def.id),
                MaxStack = Math.Max(1, def.stackCount),
                IsQuest = def.isQuestItem,
                Category = Categories.Classify(def),
                Star = star,
                StarIconId = star > 0 ? "item_star_" + star : null,
            };
        }

        /// <summary>"rune_r" → "R", "tech_red" → "RED", "rskull" → "RSKULL".</summary>
        private static string ShortSpriteName(string sprite)
        {
            int underscore = sprite.IndexOf('_');
            string tail = underscore >= 0 ? sprite.Substring(underscore + 1) : sprite;
            return tail.ToUpperInvariant();
        }

        /// <summary>
        /// Arama için küçük harfe çevirir ve Türkçe/aksanlı harfleri sadeleştirir:
        /// "Kılıç" → "kilic", "Şarap" → "sarap". Böylece Türkçe klavyesi olmayan da bulabilir.
        /// </summary>
        public static string Fold(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            var sb = new StringBuilder(text.Length);
            foreach (char c in text.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }
                // 'ı' ayrıştırılamayan tek Türkçe harf; 'İ' FormD'de 'I' + nokta olur
                sb.Append(c == 'ı' ? 'i' : char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }
    }
}
