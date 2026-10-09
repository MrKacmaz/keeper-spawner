using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

namespace KeeperSpawner
{
    internal sealed class ZombiePreset
    {
        public BodyDef Template;
        public readonly List<ItemDef> Organs = new List<ItemDef>();
        public ItemDef Collar;
        public int White;
        public int Red;
    }

    internal static class ZombieSpawner
    {
        // Use the live balance, including improved organ variants, rather than inventing skull values.
        public static ZombiePreset Build(bool white)
        {
            var balance = GameBalance.Me;
            BodyDef template = null;
            foreach (var body in balance.bodyDefs)
            {
                if (body == null || string.IsNullOrEmpty(body.id) || IsTest(body.id)
                    || (body.linkedBodyItemId != "body_corpse" && body.linkedBodyItemId != "body_zombie")) continue;
                if (template == null || body.tier > template.tier
                    || (body.tier == template.tier && string.CompareOrdinal(body.id, template.id) < 0))
                    template = body;
            }
            if (template == null) throw new InvalidOperationException("No production body template found.");

            var preset = new ZombiePreset { Template = template };
            foreach (var type in LazyConsts.MAIN_ORGANS_TYPES)
            {
                ItemDef best = null;
                foreach (var def in balance.itemDefs)
                {
                    if (def == null || string.IsNullOrEmpty(def.id) || def.type != type || def.isOrganMistake
                        || IsTest(def.id) || def.itemGroupIds == null || !def.itemGroupIds.Contains("bodypart")) continue;
                    int score = white ? def.whiteSkulls : def.redSkulls;
                    int bestScore = best == null ? int.MinValue : (white ? best.whiteSkulls : best.redSkulls);
                    int other = white ? def.redSkulls : def.whiteSkulls;
                    int bestOther = best == null ? int.MaxValue : (white ? best.redSkulls : best.whiteSkulls);
                    if (best == null || score > bestScore || (score == bestScore && other < bestOther)
                        || (score == bestScore && other == bestOther && string.CompareOrdinal(def.id, best.id) < 0))
                        best = def;
                }
                if (best == null) throw new InvalidOperationException("No organ found for " + type);
                preset.Organs.Add(best);
            }

            // Include the final-tier body's pockets/rewards and starting equipment in the preview totals.
            foreach (var def in Contents(preset))
            {
                preset.White += def.whiteSkulls;
                preset.Red += def.redSkulls;
            }
            preset.White = Mathf.Clamp(preset.White, 0, 999);
            preset.Red = Mathf.Clamp(preset.Red, 0, 999);
            foreach (var def in balance.itemDefs)
            {
                if (def == null || string.IsNullOrEmpty(def.id) || def.type != ItemType.Collar || IsTest(def.id)
                    || !def.SkullsInBorders(preset.White + def.whiteSkulls, preset.Red + def.redSkulls)) continue;
                if (preset.Collar == null || def.quality > preset.Collar.quality
                    || (def.quality == preset.Collar.quality && string.CompareOrdinal(def.id, preset.Collar.id) < 0))
                    preset.Collar = def;
            }
            if (preset.Collar == null) throw new InvalidOperationException("No collar supports the maximum-skull preset.");
            preset.White = Mathf.Clamp(preset.White + preset.Collar.whiteSkulls, 0, 999);
            preset.Red = Mathf.Clamp(preset.Red + preset.Collar.redSkulls, 0, 999);
            return preset;
        }

        public static IEnumerable<ItemDef> Contents(ZombiePreset preset)
        {
            foreach (var organ in preset.Organs) yield return organ;
            foreach (var id in preset.Template.parts)
            {
                var def = GameBalance.Me.GetDataOrNull<ItemDef>(id);
                if (def != null && !LazyConsts.MAIN_ORGANS_TYPES.Contains(def.type)) yield return def;
            }
            foreach (var id in preset.Template.pocket) yield return GameBalance.Me.GetData<ItemDef>(id);
            foreach (var id in preset.Template.burialReward) yield return GameBalance.Me.GetData<ItemDef>(id);
            if (!string.IsNullOrEmpty(preset.Template.armorId)) yield return GameBalance.Me.GetData<ItemDef>(preset.Template.armorId);
            if (!string.IsNullOrEmpty(preset.Template.handsId)) yield return GameBalance.Me.GetData<ItemDef>(preset.Template.handsId);
        }

        public static void Spawn(ZombiePreset preset)
        {
            if (!GameState.IsInGame) throw new InvalidOperationException("No active game.");
            var player = MainGame.PlayerData;
            string world = player.currentGameSceneId;
            if (string.IsNullOrEmpty(world)) throw new InvalidOperationException("No player scene.");
            Vector2 facing = player.Direction;
            if (facing.sqrMagnitude < 0.01f) facing = Vector2.down;
            facing.Normalize();
            Vector3 position = player.position.Value + new Vector3(facing.x, 0f, facing.y);
            var item = new Item("body_zombie", 1);
            foreach (var def in Contents(preset))
            {
                if (!item.AddItemToInventory(new Item(def.id, 1)))
                    throw new InvalidOperationException("Zombie body cannot hold " + def.id);
            }
            if (item.InventoryFillSize >= item.InventorySize)
                throw new InvalidOperationException("Zombie body has no space for its collar.");
            // Equipment is already in the body; register it explicitly after the game's creation routine.
            ZombieWgoData zombie = null;
            try
            {
                zombie = MainGame.ZombieSystemData.CreateZombieDrop("zombie", position, world, item, preset.Collar.id);
                if (!string.IsNullOrEmpty(preset.Template.armorId))
                    zombie.equippedArmor.SetGuid(item.Inventory.Find(i => i.id == preset.Template.armorId).UniqueId);
                if (!string.IsNullOrEmpty(preset.Template.handsId))
                    zombie.equippedHand.SetGuid(item.Inventory.Find(i => i.id == preset.Template.handsId).UniqueId);
                if (!MainGame.Instance.dropSystem.DropItem(item, world, position))
                    throw new InvalidOperationException("Game rejected the zombie drop.");
            }
            catch
            {
                // CreateZombieDrop assigns the body id before registering it, so this also handles
                // a failure partway through creation (for example, while applying a skin).
                MainGame.ZombieSystemData.RemoveZombie(zombie?.UniqueId ?? item.UniqueId);
                throw;
            }
            Plugin.Log.LogInfo($"Zombie dropped: template={preset.Template.id}, white={zombie.WhiteSkulls}, red={zombie.RedSkulls}, scene={world}, position={position}");
        }

        private static bool IsTest(string id) => id.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0
            || id.StartsWith("pseudo_", StringComparison.OrdinalIgnoreCase);
    }
}
