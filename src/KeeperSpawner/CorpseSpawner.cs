using System;
using UnityEngine;

namespace KeeperSpawner
{
    internal static class CorpseSpawner
    {
        public static void Spawn(ZombiePreset preset)
        {
            if (!GameState.IsInGame || !preset.IsCorpse)
                throw new InvalidOperationException("A corpse preset and an active game are required.");
            var player = MainGame.PlayerData;
            string world = player.currentGameSceneId;
            if (string.IsNullOrEmpty(world)) throw new InvalidOperationException("No player scene.");
            Vector2 facing = player.Direction;
            if (facing.sqrMagnitude < 0.01f) facing = Vector2.down;
            facing.Normalize();
            Vector3 position = player.position.Value + new Vector3(facing.x, 0f, facing.y);

            // Preserve the native body's skin and starting-equipment metadata for later resurrection.
            Item generated = preset.Template.GenerateItem();
            var item = new Item("body_corpse", 1);
            if (generated.TryGetProperty<BodyZombieSkinSerializedItemProperty>(out var skin)) item.AddProperty(skin);
            if (generated.TryGetProperty<BodyZombieStartItemsSerializedItemProperty>(out var equipment)) item.AddProperty(equipment);
            foreach (var def in ZombieSpawner.Contents(preset))
            {
                if (!item.AddItemToInventory(new Item(def.id, 1)))
                    throw new InvalidOperationException("Corpse cannot hold " + def.id);
            }
            // Corpses are world drops. They must not be registered as living zombies.
            if (!MainGame.Instance.dropSystem.DropItem(item, world, position))
                throw new InvalidOperationException("Game rejected the corpse drop.");
            Plugin.Log.LogInfo($"Corpse dropped: template={preset.Template.id}, white={preset.White}, red={preset.Red}, scene={world}, position={position}");
        }
    }
}
