using System;
using LazyBearTechnology;

namespace KeeperSpawner
{
    internal static class Spawner
    {
        /// <summary>
        /// Oyuncunun envanterine item ekler ve gerçekten eklenen adedi döner.
        /// Oyunun kendi Flow_AddItem düğümüyle aynı çağrıyı kullanır.
        /// </summary>
        public static int Spawn(CatalogEntry entry, int count)
        {
            var inventory = MainGame.PlayerData.inventory;
            int before = inventory.Data.GetTotalCountInInventory(entry.Id);

            if (entry.MaxStack <= 1)
            {
                // Stacklenemeyen itemlarda AddItemToInventory tek çağrıda sadece 1 adet ekliyor
                for (int i = 0; i < count; i++)
                {
                    if (!inventory.AddItemToInventory(new Item(entry.Id, 1)))
                    {
                        break;
                    }
                }
            }
            else
            {
                // Sığmayan kısım Item.Count'ta kalır; eklenen adedi farktan hesaplıyoruz
                inventory.AddItemToInventory(new Item(entry.Id, count));
            }

            int added = inventory.Data.GetTotalCountInInventory(entry.Id) - before;
            // Notify ayrı metot: oyun bildirimi API'si değiştiyse onu derlemeye bile kalkmıyoruz
            if (added > 0 && GameCompat.GameNotifications)
            {
                Notify(entry, added);
            }
            return added;
        }

        private static void Notify(CatalogEntry entry, int added)
        {
            try
            {
                // Bilinmeyen anahtarı LLBase.L olduğu gibi döndürdüğü için kendi metnimizi verebiliyoruz
                LazySingleton<UINotificator>.Instance?.ShowSimpleTextWithIconNotification(
                    $"+{added} {entry.RichName}", entry.Def.iconId);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"Could not show notification: {e.Message}");
            }
        }
    }
}
