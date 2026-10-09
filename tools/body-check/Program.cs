using System;
using System.Collections.Generic;
using System.Linq;
using KeeperSpawner;
using UnityEngine;

var balance = GameBalance.Me;
balance.bodyDefs.Add(new BodyDef { id = "final", tier = 3, linkedBodyItemId = "body_corpse", pocket = new() { "coin" } });
balance.bodyDefs.Add(new BodyDef { id = "old", tier = 1, linkedBodyItemId = "body_corpse" });
balance.bodyDefs.Add(new BodyDef { id = "body_test", tier = 999, linkedBodyItemId = "body_zombie" });
foreach (var type in LazyConsts.MAIN_ORGANS_TYPES)
{
 balance.itemDefs.Add(new ItemDef { id = type + "_white", type = type, whiteSkulls = 3 });
 balance.itemDefs.Add(new ItemDef { id = type + "_red", type = type, redSkulls = 3 });
 balance.itemDefs.Add(new ItemDef { id = type + "_mixed", type = type, whiteSkulls = 3, redSkulls = 2 });
 balance.itemDefs.Add(new ItemDef { id = type + "_test", type = type, whiteSkulls = 99, redSkulls = 99 });
 balance.itemDefs.Add(new ItemDef { id = type + "_mistake", type = type, whiteSkulls = 99, redSkulls = 99, isOrganMistake = true });
}
balance.itemDefs.Add(new ItemDef { id = "coin" });
balance.itemDefs.Add(new ItemDef { id = "body_zombie" });
balance.itemDefs.Add(new ItemDef { id = "collar", type = ItemType.Collar, quality = 2, limit = 20 });
balance.itemDefs.Add(new ItemDef { id = "small_collar", type = ItemType.Collar, quality = 3, limit = 5 });
void Check(bool result, string message) { if (!result) throw new Exception(message); }
var white = ZombieSpawner.Build(true);
var red = ZombieSpawner.Build(false);
Check(white.White == 18 && white.Red == 0, "White optimum or tie-break failed");
Check(red.Red == 18 && red.White == 0, "Red optimum failed");
Check(white.Template.id == "final" && white.Collar.id == "collar", "Template/collar filtering failed");
Check(ZombieSpawner.Contents(white).Count() == 7, "Missing organs or final-tier pocket");
MainGame.PlayerData.Direction = new Vector2(1, 0);
ZombieSpawner.Spawn(white);
var drop = MainGame.Instance.dropSystem;
Check(drop.Last.Item.id == "body_zombie" && drop.Last.Position.x == 1 && drop.Last.Position.z == 0, "World drop placement failed");
Check(MainGame.ZombieSystemData.Items.Count == 1 && drop.Last.Item.Inventory.Count == 8, "Registration/contents failed");
ZombieSpawner.Spawn(red);
Check(MainGame.ZombieSystemData.Items.Count == 2 && !ReferenceEquals(drop.Last.Item, MainGame.ZombieSystemData.Items[0].ZombieItem), "Repeated spawn reused body");
drop.Accept = false;
try { ZombieSpawner.Spawn(white); throw new Exception("Rejected drop succeeded"); }
catch (InvalidOperationException) { }
Check(MainGame.ZombieSystemData.Items.Count == 2, "Rejected drop left orphan zombie");
MainGame.PlayerData.Direction = new Vector2(0, 0);
drop.Accept = true;
ZombieSpawner.Spawn(white);
Check(drop.Last.Position.z == -1, "Zero direction fallback failed");
Console.WriteLine("PASS: optimal organs, tie-breaks, final tier, collar limits, contents, world placement, fresh bodies, rollback, direction fallback.");

// Corpse presets must work independently of zombie collar restrictions and templates.
balance.itemDefs.RemoveAll(x => x.type == ItemType.Collar);
balance.itemDefs.Add(new ItemDef { id = "body_corpse" });
balance.bodyDefs.Add(new BodyDef { id = "higher_zombie", tier = 999, linkedBodyItemId = "body_zombie" });
balance.bodyDefs[0].armorId = "starting_armor";
var whiteCorpse = ZombieSpawner.Build(true, true);
var redCorpse = ZombieSpawner.Build(false, true);
Check(whiteCorpse.White == 18 && whiteCorpse.Red == 0 && redCorpse.Red == 18, "Corpse skull optimization failed");
Check(whiteCorpse.Template.id == "final" && whiteCorpse.Collar == null, "Corpse used zombie template or required collar");
Check(!ZombieSpawner.Contents(whiteCorpse).Any(x => x.id == "starting_armor"), "Corpse incorrectly equipped zombie armor");
int zombieCount = MainGame.ZombieSystemData.Items.Count;
MainGame.PlayerData.Direction = new Vector2(1,0);
CorpseSpawner.Spawn(whiteCorpse);
var firstCorpse = drop.Last.Item;
Check(firstCorpse.id == "body_corpse" && firstCorpse.Inventory.Count == 7, "Incorrect corpse contents");
Check(firstCorpse.Inventory.Sum(x => x.Definition.whiteSkulls) == whiteCorpse.White, "Preview differs from corpse stats");
Check(drop.Last.Position.x == 1 && drop.Last.Position.z == 0, "Corpse not in front of player");
Check(firstCorpse.TryGetProperty<BodyZombieSkinSerializedItemProperty>(out var skin) && skin.head == 1052, "Missing skin metadata");
Check(firstCorpse.TryGetProperty<BodyZombieStartItemsSerializedItemProperty>(out var equipment) && equipment.armorId == "starting_armor", "Missing resurrection metadata");
CorpseSpawner.Spawn(redCorpse);
Check(!ReferenceEquals(firstCorpse, drop.Last.Item) && firstCorpse.UniqueId != drop.Last.Item.UniqueId, "Corpse id reused");
Check(drop.Last.Item.Inventory.Sum(x => x.Definition.redSkulls) == redCorpse.Red, "Incorrect red corpse stats");
Check(MainGame.ZombieSystemData.Items.Count == zombieCount, "Corpse was registered as zombie");
drop.Accept = false;
try { CorpseSpawner.Spawn(whiteCorpse); throw new Exception("Rejected corpse drop succeeded"); } catch (InvalidOperationException) { }
Check(MainGame.ZombieSystemData.Items.Count == zombieCount, "Rejected corpse changed zombie system");
GameState.IsInGame = false;
try { CorpseSpawner.Spawn(whiteCorpse); throw new Exception("Spawn outside game succeeded"); } catch (InvalidOperationException) { }
GameState.IsInGame = true;
MainGame.PlayerData.currentGameSceneId = "";
try { CorpseSpawner.Spawn(whiteCorpse); throw new Exception("Spawn without scene succeeded"); } catch (InvalidOperationException) { }
Console.WriteLine("PASS: corpse organ maxima, native template, no collar dependency, contents, metadata, placement, unique bodies, no zombie registration, rejected drops and game-state guards.");
public enum ItemType { Bones, Brain, Heart, Guts, Skin, Skull, Collar, BodyArmor }
public class ItemDef {
 public string id; public ItemType type; public int whiteSkulls, redSkulls, quality, limit = 999; public bool isOrganMistake;
 public List<string> itemGroupIds = new() { "bodypart" };
 public bool SkullsInBorders(int w, int r) => w <= limit && r <= limit;
}
public class BodyZombieSkinSerializedItemProperty { public int head = 1052; }
public class BodyZombieStartItemsSerializedItemProperty { public string armorId, handsId; }
public class BodyDef { public Item GenerateItem() { var item = new Item(linkedBodyItemId); item.AddProperty(new BodyZombieSkinSerializedItemProperty()); item.AddProperty(new BodyZombieStartItemsSerializedItemProperty { armorId = armorId, handsId = handsId }); return item; } public string id, linkedBodyItemId, armorId, handsId; public int tier; public List<string> parts = new(), pocket = new(), burialReward = new(); }
public class GameBalance {
 public static GameBalance Me = new(); public List<BodyDef> bodyDefs = new(); public List<ItemDef> itemDefs = new();
 public T GetDataOrNull<T>(string id) where T : class => itemDefs.FirstOrDefault(x => x.id == id) as T;
 public T GetData<T>(string id) where T : class => GetDataOrNull<T>(id) ?? throw new Exception(id);
}
public class SGuid { public Guid Id = Guid.NewGuid(); public void SetGuid(SGuid other) { Id = other.Id; } }
public class Item { private readonly Dictionary<Type, object> properties = new(); public void AddProperty<T>(T property) { properties[typeof(T)] = property; } public bool TryGetProperty<T>(out T property) { if (properties.TryGetValue(typeof(T), out var value)) { property = (T)value; return true; } property = default; return false; }
 public string id; public int Count; public SGuid UniqueId = new(); public List<Item> Inventory = new();
 public int InventorySize => 40; public int InventoryFillSize => Inventory.Count; public ItemDef Definition => GameBalance.Me.GetData<ItemDef>(id);
 public Item(string id, int count = 1) { this.id = id; Count = count; }
 public bool AddItemToInventory(Item item) { Inventory.Add(item); return true; }
 public Item GetItemByType(ItemType type) => Inventory.First(x => x.Definition.type == type);
}
public static class LazyConsts { public static List<ItemType> MAIN_ORGANS_TYPES = new() { ItemType.Bones, ItemType.Brain, ItemType.Heart, ItemType.Guts, ItemType.Skin, ItemType.Skull }; }
public class ZombieWgoData {
 public Item ZombieItem; public SGuid UniqueId = new(), equippedArmor = new(), equippedHand = new();
 public int WhiteSkulls => ZombieItem.Inventory.Sum(x => x.Definition.whiteSkulls);
 public int RedSkulls => ZombieItem.Inventory.Sum(x => x.Definition.redSkulls);
}
public class ZombieSystemData {
 public List<ZombieWgoData> Items = new();
 public ZombieWgoData CreateZombieDrop(string id, Vector3 pos, string world, Item item, string collar) {
  var zombie = new ZombieWgoData { ZombieItem = item }; item.AddItemToInventory(new Item(collar)); Items.Add(zombie); return zombie;
 }
 public void RemoveZombie(SGuid id) { Items.RemoveAll(x => x.UniqueId == id); }
}
public class PlayerData { public string currentGameSceneId = "world"; public Vector2 Direction; public PositionData position = new(); }
public class PositionData { public Vector3 Value; }
public class MainGame {
 public static MainGame Instance = new(); public static PlayerData PlayerData = new(); public static ZombieSystemData ZombieSystemData = new(); public DropSystem dropSystem = new();
}
public class DropSystem { public bool Accept = true; public (Item Item, Vector3 Position) Last; public bool DropItem(Item item, string world, Vector3 pos) { if (Accept) Last = (item, pos); return Accept; } }
namespace LazyBearTechnology { }
namespace KeeperSpawner {
 internal static class GameState { public static bool IsInGame = true; }
 internal static class Plugin { public static Logger Log = new(); }
 internal class Logger { public void LogInfo(string message) { } }
}
namespace UnityEngine {
 public struct Vector2 {
  public float x, y; public Vector2(float x, float y) { this.x = x; this.y = y; }
  public float sqrMagnitude => x*x+y*y; public static Vector2 down => new(0,-1);
  public void Normalize() { var length = MathF.Sqrt(sqrMagnitude); x /= length; y /= length; }
 }
 public struct Vector3 {
  public float x,y,z; public Vector3(float x, float y, float z) { this.x=x; this.y=y; this.z=z; }
  public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.x+b.x,a.y+b.y,a.z+b.z);
 }
 public static class Mathf { public static int Clamp(int x, int min, int max) => Math.Clamp(x,min,max); }
}
