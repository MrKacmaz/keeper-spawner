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
        /// <summary>Yıldız kalite varyantıysa 1-3, değilse 0.</summary>
        public int Star;
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
        private static readonly Regex StarSuffix = new Regex(":(\\d+)$", RegexOptions.Compiled);

        private static List<CatalogEntry> entries;
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
                    Plugin.Log.LogWarning($"Item atlandı [{def.id}]: {e.Message}");
                }
            }
            list.Sort((a, b) =>
            {
                int byName = string.Compare(a.DisplayName, b.DisplayName, StringComparison.InvariantCultureIgnoreCase);
                return byName != 0 ? byName : string.CompareOrdinal(a.Id, b.Id);
            });

            entries = list;
            builtForLang = LLBase.CurrentLang;
            Plugin.Log.LogInfo($"Katalog kuruldu: {list.Count} item, {skipped} gizli (dil: {builtForLang})");
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

            // Yıldız kalite varyantları (id:1, id:2, id:3) aynı adı taşıyor, ayırt edilebilsinler
            var starMatch = StarSuffix.Match(def.id);
            int star = 0;
            if (starMatch.Success)
            {
                display += " [" + starMatch.Groups[1].Value + "*]";
                int.TryParse(starMatch.Groups[1].Value, out star);
            }

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
