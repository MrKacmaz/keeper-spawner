# KeeperSpawner — Graveyard Keeper 2 Item Spawner Modu

Oyun içinde bir tuşla açılan menüden itemları listeleyip tıklanan itemı envantere ekleyen BepInEx modu.

## Kararlar

| Konu | Karar |
|---|---|
| Mod adı | **KeeperSpawner** |
| Kısayol tuşu | **O** (ayar dosyasından değiştirilebilir). P oyunda başka bir eyleme atanmış. |
| Tıklama davranışı | Araç çubuğundaki **Tık** moduna göre 1 / 10 / Maks (varsayılan Maks); Shift+tık = 1 adet; bilgi panelinden 1-999 (v0.4.0) |
| Geliştirme ortamı | Windows (kodlama + derleme + test aynı makinede) |
| Versiyon kontrol | Git (Windows cihazda başlatılacak) |
| Yayın yeri | Steam Atölyesi (ileride Nexus) |

## Teknik altyapı

- **Oyun:** Graveyard Keeper 2, Steam appid `4358690`, sadece Windows, **Unity 6000.3.9 (Unity 6.3), Mono** (Faz 0'da doğrulandı). Oyun sürümü 1.007.1, Steam buildid 25601286. Giriş sistemi: **Rewired**.
- **Dil:** C#, hedef framework `net472`.
- **Mod yükleyici:** BepInEx **5.4.23.5 win_x64** (topluluk standardı, Workshop Loader da bunu bekliyor).
- **Kod kancaları:** HarmonyX (BepInEx ile geliyor).
- **Arayüz:** Başlangıçta Unity IMGUI (`OnGUI`), sonra istenirse uGUI.
- **Araçlar:** .NET 8 SDK, Git, ILSpy + `ilspycmd` 9.1 (oyun kodunu okumak için; en yeni ilspycmd .NET 10 SDK istiyor), UnityExplorer (çalışırken canlı inceleme).
  - **UnityExplorer notu:** yukieiji sürümü (4.13.6) Unity 6.3'te bozuk (`Scene.handle` artık `SceneHandle`; geliştirici düzeltmiyor, [#104](https://github.com/yukieiji/UnityExplorer/issues/104)). Bunun yerine Thunderstore'daki **AtlyssModding-Atlyss_UnityExplorer 4.13.100** kullanılıyor.

## Proje yapısı

```
KeeperSpawner/
├── src/KeeperSpawner/KeeperSpawner.csproj
├── src/KeeperSpawner/Plugin.cs         # giriş noktası, config, kısayol
├── src/KeeperSpawner/GameState.cs      # "oyun içinde miyiz" kontrolü
├── src/KeeperSpawner/ItemCatalog.cs    # tüm itemları okur, filtreler, önbelleğe alır
├── src/KeeperSpawner/Spawner.cs        # envantere ekleme + oyun bildirimi
├── src/KeeperSpawner/SpawnerWindow.cs  # pencere çekirdeği (IMGUI): durum, çerçeve, başlık, kısayollar
├── src/KeeperSpawner/SpawnerWindow.*.cs# paneller: Sidebar, Toolbar, Content, Info, Footer
├── src/KeeperSpawner/ItemView.cs       # ızgara / liste çizimi, bölümler
├── src/KeeperSpawner/KsTheme.cs        # v0.4 tema: renkler, 9-dilim dokular, GUIStyle'lar
├── src/KeeperSpawner/FontFinder.cs     # oyunun piksel fontunu bulur (UI.Font)
├── src/KeeperSpawner/ItemIcons.cs      # logo ve kategori ikonları (gerçek item sprite'ları)
├── src/KeeperSpawner/IdList.cs         # favoriler (config)
├── src/KeeperSpawner/RecentList.cs     # son eklenenler + adet (config)
├── src/KeeperSpawner/IconCache.cs      # item ikonlarını çözer ve önbelleğe alır
├── src/KeeperSpawner/Categories.cs     # item → kategori kuralları
├── src/KeeperSpawner/InputBlocker.cs   # menü açıkken oyun girdisini askıya alır
├── src/KeeperSpawner/Strings.cs        # mod arayüzü metinleri (17 dil)
├── src/KeeperSpawner.GK2Framework/     # isteğe bağlı GK2 Mod Framework köprüsü (netstandard2.1, v1.1.0)
├── Directory.Build.props               # Directory.Build.props.user'ı içe aktarır
├── Directory.Build.props.user          # GamePath (yerel, git'e girmez)
├── docs/game-api.md                    # Faz 1 keşif notları (oyun API'si)
├── tools/ue/                           # UnityExplorer C# konsol betikleri
├── decompiled/                         # oyun kodunun okunabilir hali (git'e girmez, ASLA yayınlanmaz)
├── workshop/                           # preview.png, açıklama, INSTALL.txt, .vdf
└── nexus/                              # Nexus sayfa metinleri (açıklama, özet, dosyalar, gereksinimler)
```

- Oyun DLL'lerine (`Assembly-CSharp.dll`, `UnityEngine*.dll`) doğrudan oyun klasöründen referans verilir (`$(GamePath)\GraveyardKeeper2_Data\Managed\`); kopyalanmaz.
- Build sonrası DLL otomatik olarak `$(GamePath)\BepInEx\plugins\KeeperSpawner\` klasörüne kopyalanır.

## Fazlar

### Faz 0 — Windows kurulumu ✅ (2026-09-29; git kimliği ve ilk commit kullanıcıda)
1. Git, .NET SDK, IDE, dnSpyEx/ILSpy kur.
2. BepInEx 5.4.23.5 win_x64'ü oyunun `.exe`'sinin yanına çıkar, oyunu bir kez aç/kapat.
3. **Motor kontrolü:** `GraveyardKeeper2_Data\Managed\` varsa Mono (plan geçerli). `GameAssembly.dll` varsa IL2CPP → BepInEx 6 + Il2CppInterop'a geçilir, plan güncellenir.
4. UnityExplorer (BepInEx5 Mono) kur.
5. `git init`, `.gitignore` (bin/, obj/, decompiled/, *.user).

### Faz 1 — Keşif (en kritik) ✅ (2026-09-29; bulgular: `docs/game-api.md`)
Oyun kodunda bulunacaklar:
- Item veritabanı: tüm item tanımları (kimlik, çeviri anahtarı / görünen ad, ikon, max stack).
- Oyuncu envanterine item ekleyen metot.
- "Kayıt yüklendi / oyun içindeyiz" durumu (menü ana menüde açılmamalı).
- Envanter dolunca oyunun davranışı.

**Ara hedef:** UnityExplorer konsolundan bulunan metodu elle çağırıp envantere 1 item eklemek. ✅ `tools/ue/02-add-stick.cs` ile 50 `stick` eklendi (`ok=True before=0 after=50 leftover=0`).

### Faz 2 — MVP ✅ (2026-09-29; v0.1.0 oyunda test edildi: 726 item listelendi, spawn'lar tam adetle eklendi, log temiz)
- BepInEx eklenti iskeleti (`[BepInPlugin]`, yüklenince log).
- Config (`BepInEx\config\<guid>.cfg`): kısayol tuşu (varsayılan `O`).
- O ile açılıp kapanan pencere: arama kutusu + kaydırılabilir item listesi; tıklanınca max stack kadar envantere ekle.
- **Dikkat:** Arama kutusuna yazı yazılırken "o" harfi menüyü kapatmamalı → metin alanı odaktayken kısayol yok sayılır.
- ~~P tuşunun oyunda başka bir işlevi var mı kontrol et~~ → P dolu, O seçildi.
- Liste filtreleri (bkz. `docs/game-api.md`): `overhead`, test/pseudo, envanter kabı ve oyun kaynağı sayılan id'ler gizli; görev itemları varsayılan olarak gizli.
- Menü açıkken oyun tıklamaları/hareketi engellenir.
- Item listesi bir kez okunup önbelleğe alınır.
- Stacklenemeyen itemlarda max stack = 1.

### Faz 3 — Cilalama
- ✅ İkonlu ızgara görünümü (oyunun sprite'ları); Izgara/Liste seçimi config'e kaydediliyor. (v0.2.0)
- ✅ Kategori sekmeleri; "Tümü" sekmesinde ızgara kategori başlıklarıyla bölümlere ayrılıyor. 15 kategori, kurallar `Categories.cs`. (v0.2.0)
- Favoriler (sağ tık) ve son eklenenler (en fazla 24); ayrı sekmeler + "Tümü"nün başında bölümler. Config'te `Items.Favorites` / `Items.Recent`. (v0.3.0)
- Opak pencere arka planı (`UI.BackgroundOpacity`, varsayılan 0.97). (v0.3.0)
- v0.4.0 arayüz yeniden tasarımı (`docs/ui-handoff/`): çelik çerçeveli 1120×720 pencere, sol dikey kategori listesi, arama + Tık modu (1/10/Maks) + kalite filtresi, ızgara/liste, sağda item bilgi paneli (miktar, ID kopyala, favori, son 5), alt çubukta bildirim ve tuş ipuçları. Son eklenenler 24'ten 5'e indi; "Tümü" artık favori/son bölümlerini içermiyor (tasarım). **Oyunda test bekliyor.**
- Item adları oyunun aktif diliyle; mod arayüzü oyunun 17 dilinin hepsinde (en, tr, de, fr, es/es-mx, pt-br, ru, uk-ua, it, pl, ja, zh_cn, zh_cht, ko, th, vn; bilinmeyen dil → en). Tablo `Strings.cs`. **Oyunda test bekliyor** (CJK/Tay harfleri varsayılan fontla, uzun Almanca metinler).
- Sorunlu itemlar (görev itemları) için kara liste.
- İsteğe bağlı: oyunun stiline uygun uGUI arayüz.

### Faz 4 — Test ✅ (2026-09-30; kullanıcı tüm maddeleri oyunda geçti. Kalan: `Debug.ForceIncompatible` ile devre dışı yolu)
- Yeni kayıt / eski kayıt; spawn edilen itemlar kaydet-yükle sonrası duruyor mu.
- Envanter doluyken; stack sınırları; stacklenemeyen itemlar.
- Menü açıkken: arkadaki oyun arayüzüne (uGUI, ör. açık envanter) tıklama geçiyor mu; fare tekerleği kamerayı yakınlaştırıyor mu. `InputBlocker` sadece `LazyInput`'u askıya alıyor.
- Menü açıkken yürüme tuşu basılıyken O'ya basınca karakter duruyor mu.
- Diğer modlarla birlikte (ör. GK2 Power Menu).
- Hem Workshop Loader ile hem elle kurulumla.
- Hatalar için `BepInEx\LogOutput.log`.

### Faz 5 — Atölye yayını
- Paket:
  ```
  BepInEx/plugins/KeeperSpawner/KeeperSpawner.dll
  INSTALL.txt
  ```
  BepInEx / Harmony / UnityEngine / Assembly-CSharp DLL'leri pakete **konmaz**.
- Ağ, dosya silme, `Process`, `DllImport`, `Assembly.Load`, registry, `Environment.GetFolderPath` kullanılmaz (Workshop Loader güvenlik uyarısı çıkmasın).
- Asset'ler `Path.GetDirectoryName(Info.Location)`'a göre yüklenir; plugin klasörüne yazılmaz.
- Yükleme: SteamCMD `workshop_build_item` + `.vdf` (appid 4358690). Önce gizli yükle, test et, sonra herkese açık. Oyunun kendi yükleyicisi var mı kontrol et.
- Önizleme görseli (<1 MB), EN + TR açıklama, BepInEx kurulum linki, kısayol ve ayarlar.
- **Durum (2026-09-30):**
  - ✅ `tools/package.ps1`: derler, DLL'i Workshop Loader güvenlik listesine göre tarar, `artifacts/<sürüm>/content` (Atölye), `KeeperSpawner-<sürüm>.zip` (elle kurulum / Nexus, `/` ayırıcılı) ve `workshop_item.vdf` üretir. Tarama temiz.
  - ✅ `workshop/`: `INSTALL.txt` (EN+TR), `description.en.bbcode`, `description.tr.bbcode`, `keeperspawner.vdf.template`. `CHANGELOG.md`.
  - ✅ Sürüm 1.0.0.
  - ✅ `workshop/preview.png`: 1024×1024 kare kapak (`docs/images/banner.png` ile aynı). Atölye listeleri önizlemeyi kare gösterir; 16:9 `docs/images/cover.png` siyah bantlı görünüyor, o yüzden kullanılmıyor.
  - ✅ Atölye'ye yüklendi (oyunun yükleyicisiyle): [3810736028](https://steamcommunity.com/sharedfiles/filedetails/?id=3810736028). `tools/package.ps1` varsayılan olarak bu id'yi `.vdf`'e yazar. İlk kontrolde anonim Steam API'si itemı göremedi (Liste dışı / henüz herkese açık değil).
  - ✅ Repo herkese açık, MIT lisansı, README (EN+TR), issue formları.
  - ✅ GitHub release [v1.0.0](https://github.com/MrKacmaz/keeper-spawner/releases/tag/v1.0.0) (`KeeperSpawner-1.0.0.zip`, SHA-256 `0ac5ec56…2660`). Sonraki sürümler: `tools/package.ps1` → `gh release create vX.Y.Z artifacts\X.Y.Z\KeeperSpawner-X.Y.Z.zip --notes-file …`.
- **Oyunun Atölye yükleyicisi** (`SteamWorkshopCreatorService`, `UISteamWorkshopCreatorWindow`):
  - `…\AppData\LocalLow\Lazy Bear Games\Graveyard Keeper 2\Mods\workshop.json` içinde `"workshopCreatorMode": true` → oyunda **Shift+F11**.
  - Tek etiket seçeneği `Translation`. Dosya türü filtresi yok; sadece `_`/`~` ile başlayanları atlar. Önizleme: klasördeki `Thumbnail.png|jpg|jpeg` (içeriğe kopyalanmaz).
  - **Her yüklemede** görünürlüğü `Unlisted` yapar ve açıklamayı başlıkla değiştirir → güncellemeden sonra açıklamayı ve (herkese açıksa) görünürlüğü Steam sayfasından yeniden ayarlamak gerekir. Güncellemeler için SteamCMD + `workshop_item.vdf` daha rahat olabilir.
- ✅ **Atölye → Workshop Loader → oyun zinciri test edildi (2026-09-30):** loader itemı tanıdı, güvenlik taraması uyarısız, onaydan sonra `BepInEx\plugins\_Workshop\3810736028\` altına kurdu, KeeperSpawner 1.0.0 oradan yüklenip çalıştı. Loader bir **preloader patcher**: `BepInEx\patchers\` klasörüne kurulur (README, açıklamalar ve `INSTALL.txt` buna göre düzeltildi; `INSTALL.txt` Atölye içeriğinde olduğu için sonraki sürümle gider).
- **Workshop Loader** (`docs/FOR_MODDERS.txt`): normal BepInEx 5 eklentisi değişiklik gerektirmez; öğeyi `BepInEx\plugins\_Workshop\<ItemID>\` altına kopyalar; framework DLL'lerini kopyalamaz; her güncellemede oyuncu yeniden onaylar; plugin klasörüne yazılan dosyaları siler.

### Faz 6 — Bakım
- Oyun güncellemesi metotları bozarsa mod çökmez; loga uyarı yazıp kendini devre dışı bırakır. ✅ (kod hazır, **oyunda test bekliyor**)
  - `GameCompat.Check()` açılışta kullanılan oyun API'lerini reflection ile doğrular. Zorunlu biri eksikse mod kapanır, isteğe bağlı olan eksikse (oyun bildirimi, ikonlar, girdi donma temizliği) sadece o özellik kapanır.
  - `Update`/`OnGUI` sarmalı: `MissingMemberException`/`TypeLoadException` gelirse mod hemen kapanır; diğer hatalar birer kez loglanır, 30 hatadan sonra mod kapanır. Kapalıyken kısayola basınca 17 dilde uyarı çıkar.
  - Test için `Debug.ForceIncompatible = true`.
  - Açılışta oyun sürümü ve test edilen sürüm (`GameCompat.TestedGameVersion`) loglanır; her oyun güncellemesinden sonra güncellenmeli.
- SemVer (1.0.0, 1.1.0…) ve Atölye değişiklik notları.

### v1.1.0 — GK2 Mod Framework entegrasyonu (2026-10-02)
- Sebep: Nexus'ta ([mods/275](https://www.nexusmods.com/graveyardkeeper2/mods/275)) bir kullanıcı modun Framework'ün Mods menüsünde görünmediğini ve O ile açılmadığını yazdı (muhtemelen sıfıra bastı ya da ana menüde denedi).
- [GK2 Mod Framework](https://www.nexusmods.com/graveyardkeeper2/mods/42) 0.1.19 "optional integration" deseni: ana DLL Framework'e referans vermez; `KeeperSpawner.GK2Framework.dll` köprüsü hem KeeperSpawner'a hem Framework'e hard dependency verir. Framework yoksa BepInEx sadece köprüyü atlar.
- Köprü `frameworkManagesEnabledState: false`, `requiresKnownBuild: false` (uyumluluğu `GameCompat` denetler). Ayarlar KeeperSpawner'ın kendi `ConfigEntry`'leri (0.1.19 girdi benimseme): ToggleKey, ClickAmount, ShowQuestItems, View, Scale, BackdropOpacity + salt okunur Durum satırı. Font (pencere bir kez kurulur) ve iç durum girdileri gösterilmez.
- Config girdileri artık uyumluluk kontrolünden önce bağlanıyor (mod kapalıyken de menüde görünsünler). Köprü `InternalsVisibleTo` ile erişir.
- Metinler kayıt anında `FrameworkLocalization.CurrentLanguage` ile çözülür (`Strings.In`); o an `LLBase.CurrentLang` henüz ayarlı olmayabilir. Bölüm başlıkları (General/Items/UI) İngilizce kalıyor: Framework onları sadece kendi klasöründeki JSON'dan çeviriyor, Atölye oraya dosya koyamaz.
- Framework `netstandard2.1` → köprü de `netstandard2.1`; net472 ana DLL'e doğrudan `Reference` ile bağlanır (ProjectReference sadece derleme sırası için).
- Log mesajları İngilizceye çevrildi.
- ✅ Oyunda test edildi (Framework 0.1.19, build 25601286): kayıt `status=Compatible`, Mods menüsünde açıklama/ayarlar Türkçe, Durum satırı doğru.
- Workshop Loader DLL atlama listesi (`0Harmony`, `BepInEx`, `Unity.`…) köprüyü etkilemiyor.

## Riskler

- GK2 Atölyesi resmi olarak sadece çeviriler için; kod modları "Translation" etiketiyle yükleniyor. Workshop Loader'ın Atölye sayfası kural ihlali gerekçesiyle gizlenmiş → modumuz da kaldırılabilir. Yedek plan: Nexus Mods.
- Rakipler: Graveyard Keeper II Plus, Kebo Mod All-in-One, GK2 Cheat Menu (Nexus), GK2 Power Menu (Atölye). Farkımız: sadece spawner'a odaklı, hafif, ikonlu/kategorili temiz arayüz, Atölye'de.

## Kaynaklar

- GK2 Atölyesi: https://steamcommunity.com/app/4358690/workshop/
- BepInEx sürümleri: https://github.com/BepInEx/BepInEx/releases
- GK2 Workshop Loader (mod yazarı notları `docs/FOR_MODDERS.txt`): https://github.com/Zoriten/-GK2-WorkshopLoader
