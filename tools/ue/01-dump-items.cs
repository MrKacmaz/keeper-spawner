// UnityExplorer > C# Console'a yapıştır, "Compile" ile çalıştır.
// Ana menüde de çalışır (GameBalance oyun açılışında yüklenir).
// Tüm ItemDef'leri decompiled/items.tsv dosyasına döker (git'e girmez).
// Not: UE konsolu "using" satırlarını kodla birlikte derleyemiyor, bu yüzden tip adları tam yazıldı.
var sb = new System.Text.StringBuilder();
sb.AppendLine("id\tname\tstack\ttype\tquest\tbag\tdurability\ticonId\thasSprite\tgroups");
var sprites = LazyBearTechnology.LazySingletonSO<LazyBearTechnology.EasySpritesCollection>.Instance;
int total = 0, errors = 0;
foreach (var d in GameBalance.Me.itemDefs)
{
    total++;
    try
    {
        string name = d.GetHeader();
        bool hasSprite = !string.IsNullOrEmpty(d.iconId) && sprites.HasSprite(d.iconId);
        sb.AppendLine(string.Join("\t", new string[] {
            d.id, name, d.stackCount.ToString(), d.type.ToString(), d.isQuestItem.ToString(),
            d.isBag.ToString(), d.hasDurability.ToString(), d.iconId, hasSprite.ToString(),
            string.Join(",", d.itemGroupIds.ToArray())
        }));
    }
    catch (System.Exception e)
    {
        errors++;
        sb.AppendLine(d.id + "\tERROR: " + e.GetType().Name + " " + e.Message);
    }
}
string path = @"C:\Projects\mods\graveyardkeeper2\KeeperSpawner\decompiled\items.tsv";
System.IO.File.WriteAllText(path, sb.ToString(), new System.Text.UTF8Encoding(false));
Log("KeeperSpawner dump: " + total + " items, " + errors + " errors, lang=" + LazyBearTechnology.LLBase.CurrentLang + " -> " + path);
