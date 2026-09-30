using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace KeeperSpawner
{
    /// <summary>
    /// Son eklenen itemlar ve eklendikleri miktar, en yeniden eskiye. Config'te "id=adet,id=adet" olarak
    /// tutulur; eski sürümlerin adetsiz "id,id" biçimi de okunur (adet 0 = maks yığın).
    /// </summary>
    internal sealed class RecentList
    {
        public struct Entry
        {
            public string Id;
            public int Amount;
        }

        private readonly ConfigEntry<string> config;
        private readonly int maxCount;
        private readonly List<Entry> entries = new List<Entry>();
        private bool saving;

        public RecentList(ConfigEntry<string> config, int maxCount)
        {
            this.config = config;
            this.maxCount = maxCount;
            Load();
            config.SettingChanged += (sender, args) =>
            {
                if (!saving)
                {
                    Load();
                }
            };
        }

        public int Version { get; private set; }

        public IReadOnlyList<Entry> Entries => entries;

        public int Count => entries.Count;

        /// <summary>Başa taşır (aynı item tekrar eklenince de); sınır aşılırsa en eskiyi atar.</summary>
        public void Push(string id, int amount)
        {
            entries.RemoveAll(e => e.Id == id);
            entries.Insert(0, new Entry { Id = id, Amount = amount });
            if (entries.Count > maxCount)
            {
                entries.RemoveRange(maxCount, entries.Count - maxCount);
            }
            Save();
        }

        private void Load()
        {
            entries.Clear();
            foreach (var raw in (config.Value ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string part = raw.Trim();
                int eq = part.LastIndexOf('=');
                string id = eq >= 0 ? part.Substring(0, eq) : part;
                int amount = 0;
                if (eq >= 0)
                {
                    int.TryParse(part.Substring(eq + 1), out amount);
                }
                if (id.Length > 0 && !entries.Exists(e => e.Id == id))
                {
                    entries.Add(new Entry { Id = id, Amount = Math.Max(0, amount) });
                    if (entries.Count >= maxCount)
                    {
                        break;
                    }
                }
            }
            Version++;
        }

        private void Save()
        {
            saving = true;
            try
            {
                var parts = new string[entries.Count];
                for (int i = 0; i < entries.Count; i++)
                {
                    parts[i] = entries[i].Id + "=" + entries[i].Amount;
                }
                config.Value = string.Join(",", parts);
            }
            finally
            {
                saving = false;
            }
            Version++;
        }
    }
}
