// UnityExplorer > C# Console'a yapıştır, "Compile" ile çalıştır.
// Oyun içindeyken (bir kayıt yüklüyken) çalıştır; öncesinde envanteri bir kez açıp kapatmak iyi olur.
// Oyunun ikonlardaki mavi kenar pikselerini boyama mekanizmasını inceler ve
// decompiled/blue-replace.txt dosyasına yazar:
//  1) UIItemCell'in kenar renkleri (ImageColors normal / fare üstü) ve ikon materyali + shader'ı
//  2) Projenin renk uzayı
//  3) Birkaç ikondaki mavi(msı) renklerin dağılımı (doku okunamıyorsa RenderTexture üzerinden)
// Not: UE konsolu "using" satırlarını kodla birlikte derleyemiyor, bu yüzden tip adları tam yazıldı.
var sb = new System.Text.StringBuilder();
var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
sb.AppendLine("colorSpace\t" + UnityEngine.QualitySettings.activeColorSpace);

// 1) Item hücreleri: renkler ve materyal
int cells = 0;
var seenMaterials = new System.Collections.Generic.HashSet<string>();
foreach (var cell in UnityEngine.Resources.FindObjectsOfTypeAll<UIItemCell>())
{
    if (cell == null) continue;
    cells++;
    var colors = typeof(UIItemCell).GetField("colors", flags).GetValue(cell) as ImageColors;
    var icon = typeof(UIItemCell).GetField("icon", flags).GetValue(cell) as UnityEngine.UI.Image;
    if (cells <= 3 && colors != null)
    {
        sb.AppendLine("colors\t" + colors.name + "\tnormal=" + colors.NormalColor + " #" + UnityEngine.ColorUtility.ToHtmlStringRGBA(colors.NormalColor)
            + "\thover=" + colors.HighlightedColorMouse + " #" + UnityEngine.ColorUtility.ToHtmlStringRGBA(colors.HighlightedColorMouse));
    }
    if (icon == null) continue;
    var mat = icon.material;
    if (mat == null || !seenMaterials.Add(mat.name + "|" + (mat.shader != null ? mat.shader.name : "-"))) continue;
    var shader = mat.shader;
    sb.AppendLine("material\t" + mat.name + "\tshader=" + (shader != null ? shader.name : "-") + "\tkeywords=" + string.Join(",", mat.shaderKeywords));
    if (shader != null)
    {
        for (int i = 0; i < shader.GetPropertyCount(); i++)
        {
            string pname = shader.GetPropertyName(i);
            var ptype = shader.GetPropertyType(i);
            string value = "";
            if (ptype == UnityEngine.Rendering.ShaderPropertyType.Color) value = mat.GetColor(pname).ToString();
            else if (ptype == UnityEngine.Rendering.ShaderPropertyType.Float || ptype == UnityEngine.Rendering.ShaderPropertyType.Range) value = mat.GetFloat(pname).ToString();
            else if (ptype == UnityEngine.Rendering.ShaderPropertyType.Texture) { var tex = mat.GetTexture(pname); value = tex != null ? tex.name : "-"; }
            sb.AppendLine("  prop\t" + pname + "\t" + ptype + "\t" + value);
        }
    }
}
sb.AppendLine("uiItemCells\t" + cells);

// Oyundaki tüm ImageColors varlıkları
foreach (var ic in UnityEngine.Resources.FindObjectsOfTypeAll<ImageColors>())
{
    sb.AppendLine("imageColors\t" + ic.name + "\tnormal=#" + UnityEngine.ColorUtility.ToHtmlStringRGBA(ic.NormalColor) + "\thover=#" + UnityEngine.ColorUtility.ToHtmlStringRGBA(ic.HighlightedColorMouse));
}

// 3) İkon piksellerinde mavi(msı) renkler
var coll = LazyBearTechnology.LazySingletonSO<LazyBearTechnology.EasySpritesCollection>.Instance;
var ids = new string[] { "i_box_empty", "i_stick", "i_wooden_plank", "i_bread", "i_sword_t3", "i_heal_potion_1" };
foreach (var id in ids)
{
    if (!coll.HasSprite(id)) { sb.AppendLine("sprite\t" + id + "\tYOK"); continue; }
    var sprite = coll.GetSprite(id);
    var src = sprite.texture;
    var r = sprite.textureRect;
    int w = (int)r.width, h = (int)r.height;
    var rt = UnityEngine.RenderTexture.GetTemporary(w, h, 0, UnityEngine.RenderTextureFormat.ARGB32, UnityEngine.RenderTextureReadWrite.sRGB);
    var scale = new UnityEngine.Vector2(r.width / src.width, r.height / src.height);
    var offset = new UnityEngine.Vector2(r.x / src.width, r.y / src.height);
    UnityEngine.Graphics.Blit(src, rt, scale, offset);
    var prev = UnityEngine.RenderTexture.active;
    UnityEngine.RenderTexture.active = rt;
    var read = new UnityEngine.Texture2D(w, h, UnityEngine.TextureFormat.RGBA32, false);
    read.ReadPixels(new UnityEngine.Rect(0, 0, w, h), 0, 0);
    read.Apply();
    UnityEngine.RenderTexture.active = prev;
    UnityEngine.RenderTexture.ReleaseTemporary(rt);
    var counts = new System.Collections.Generic.Dictionary<string, int>();
    foreach (var c in read.GetPixels32())
    {
        if (c.a < 8) continue;
        // mavimsi: mavi kanal belirgin şekilde baskın
        if (c.b > 120 && c.b > c.r + 60 && c.b > c.g + 60)
        {
            string key = c.r + "," + c.g + "," + c.b + "," + c.a;
            counts[key] = counts.ContainsKey(key) ? counts[key] + 1 : 1;
        }
    }
    var parts = new System.Collections.Generic.List<string>();
    foreach (var kv in counts) parts.Add(kv.Key + " x" + kv.Value);
    sb.AppendLine("sprite\t" + id + "\t" + w + "x" + h + "\treadable=" + src.isReadable + "\tformat=" + src.format + "\tfilter=" + src.filterMode + "\tblue: " + string.Join("  ", parts.ToArray()));
    UnityEngine.Object.Destroy(read);
}

string outPath = @"C:\Projects\mods\graveyardkeeper2\KeeperSpawner\decompiled\blue-replace.txt";
System.IO.File.WriteAllText(outPath, sb.ToString(), new System.Text.UTF8Encoding(false));
Log("KeeperSpawner blue-replace: cells=" + cells + " -> " + outPath);
