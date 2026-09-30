// UnityExplorer > C# Console'a yapıştır, "Compile" ile çalıştır.
// Oyun içindeyken (bir kayıt yüklüyken) çalıştır; oyunun pencereleri o zaman yüklü oluyor.
// Kapat (X) düğmesi gibi görünen sprite'ları decompiled/close-sprites.tsv dosyasına döker.
// Not: UE konsolu "using" satırlarını kodla birlikte derleyemiyor, bu yüzden tip adları tam yazıldı.
var sb = new System.Text.StringBuilder();
var keys = new string[] { "close", "cross", "exit", "btn_x", "button_x", "_x" };
int images = 0, sprites = 0, collection = 0;

// 1) UI görselleri: oyunun pencerelerindeki kapat düğmeleri (pasif olanlar dahil)
foreach (var img in UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.UI.Image>())
{
    if (img == null || img.sprite == null) continue;
    string path = img.gameObject.name;
    var t = img.transform.parent;
    for (int depth = 0; t != null && depth < 6; depth++, t = t.parent) path = t.name + "/" + path;
    string low = (path + " " + img.sprite.name).ToLowerInvariant();
    bool hit = false;
    foreach (var k in keys) if (low.Contains(k)) hit = true;
    if (!hit) continue;
    images++;
    sb.AppendLine("IMAGE\t" + img.sprite.name + "\t" + (img.sprite.texture != null ? img.sprite.texture.name : "-") + "\t" + img.sprite.rect + "\t" + path);
}

// 2) Yüklü tüm sprite'lar
foreach (var s in UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Sprite>())
{
    if (s == null) continue;
    string low = s.name.ToLowerInvariant();
    bool hit = false;
    foreach (var k in keys) if (low.Contains(k)) hit = true;
    if (!hit) continue;
    sprites++;
    sb.AppendLine("SPRITE\t" + s.name + "\t" + (s.texture != null ? s.texture.name : "-") + "\t" + s.rect);
}

// 3) Oyunun sprite koleksiyonu (EasySpritesCollection) isimleri
var coll = LazyBearTechnology.LazySingletonSO<LazyBearTechnology.EasySpritesCollection>.Instance;
coll.HasSprite("i_empty"); // koleksiyonu başlat
var field = typeof(LazyBearTechnology.EasySpritesCollection).GetField("hash", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
var hash = field.GetValue(coll) as System.Collections.Generic.Dictionary<string, string>;
foreach (var pair in hash)
{
    string low = pair.Key.ToLowerInvariant();
    bool hit = false;
    foreach (var k in keys) if (low.Contains(k)) hit = true;
    if (!hit) continue;
    collection++;
    sb.AppendLine("COLLECTION\t" + pair.Key + "\t" + pair.Value);
}

string outPath = @"C:\Projects\mods\graveyardkeeper2\KeeperSpawner\decompiled\close-sprites.tsv";
System.IO.File.WriteAllText(outPath, sb.ToString(), new System.Text.UTF8Encoding(false));
Log("KeeperSpawner close-sprite: image=" + images + " sprite=" + sprites + " collection=" + collection + " -> " + outPath);
