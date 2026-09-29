// UnityExplorer > C# Console'a yapıştır, "Compile" ile çalıştır.
// Bir kayıt yüklenmiş ve oyun içindeyken çalıştır (ana menüde çalışmaz).
// Oyunun kendi Flow_AddItem düğümünün yaptığı çağrının aynısı.
if (MainGame.Instance == null || MainGame.Instance.gameState != MainGame.GameState.InGame || MainGame.PlayerData == null)
{
    Log("KeeperSpawner: oyun içinde değilsin, önce bir kayıt yükle.");
}
else
{
    var inv = MainGame.PlayerData.inventory;
    // "wood" (Kütük) değil: o "overhead" grubunda, başta taşınan büyük item, envantere girmez
    string id = "stick";
    int stack = GameBalance.Me.GetData<ItemDef>(id).stackCount;
    int before = inv.Data.GetTotalCountInInventory(id);
    var item = new Item(id, stack);
    bool ok = inv.AddItemToInventory(item);
    int after = inv.Data.GetTotalCountInInventory(id);
    // AddItemToInventory sığmayan miktarı item.Count'ta bırakır
    Log("KeeperSpawner add: id=" + id + " ok=" + ok + " stack=" + stack + " before=" + before + " after=" + after + " leftover=" + item.Count);
}
