using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace KeeperSpawner
{
    /// <summary>
    /// Virgülle ayrılmış bir config değerinde tutulan sıralı item id listesi (favoriler, son eklenenler).
    /// Plugin klasörüne dosya yazmamak için BepInEx config'i kullanılıyor.
    /// </summary>
    internal sealed class IdList
    {
        private readonly ConfigEntry<string> entry;
        private readonly int maxCount;
        private readonly List<string> ids = new List<string>();
        private readonly HashSet<string> set = new HashSet<string>();
        private bool saving;

        public IdList(ConfigEntry<string> entry, int maxCount = int.MaxValue)
        {
            this.entry = entry;
            this.maxCount = maxCount;
            Load();
            // Config dosyası dışarıdan (ör. ConfigurationManager) değişirse yeniden oku
            entry.SettingChanged += (sender, args) =>
            {
                if (!saving)
                {
                    Load();
                }
            };
        }

        /// <summary>Liste her değiştiğinde artar; arayüz yeniden kurulup kurulmayacağına buna bakıyor.</summary>
        public int Version { get; private set; }

        public IReadOnlyList<string> Ids => ids;

        public int Count => ids.Count;

        public bool Contains(string id) => set.Contains(id);

        public void Toggle(string id)
        {
            if (set.Remove(id))
            {
                ids.Remove(id);
            }
            else
            {
                set.Add(id);
                ids.Add(id);
            }
            Save();
        }

        /// <summary>Listenin başına taşır; sınır aşılırsa en eskiyi atar.</summary>
        public void PushFront(string id)
        {
            ids.Remove(id);
            ids.Insert(0, id);
            set.Add(id);
            while (ids.Count > maxCount)
            {
                set.Remove(ids[ids.Count - 1]);
                ids.RemoveAt(ids.Count - 1);
            }
            Save();
        }

        private void Load()
        {
            ids.Clear();
            set.Clear();
            foreach (var raw in (entry.Value ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string id = raw.Trim();
                if (id.Length > 0 && set.Add(id))
                {
                    ids.Add(id);
                    if (ids.Count >= maxCount)
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
                entry.Value = string.Join(",", ids.ToArray());
            }
            finally
            {
                saving = false;
            }
            Version++;
        }
    }
}
