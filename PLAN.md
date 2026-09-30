# KeeperSpawner — Graveyard Keeper 2 Item Spawner Modu

Oyun içinde bir tuşla açılan menüden itemları listeleyip tıklanan itemı envantere ekleyen BepInEx modu.

## Kararlar

| Konu | Karar |
|---|---|
| Mod adı | **KeeperSpawner** |
| Kısayol tuşu | **O** (ayar dosyasından değiştirilebilir). P oyunda başka bir eyleme atanmış. |
| Tıklama davranışı | Varsayılan olarak itemın **max stack**'i kadar ekler; v0.4.0 tasarımıyla 1 / 10 / Maks seçimi ve Shift+tık = 1 adet gelecek |
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
├── src/KeeperSpawner/SpawnerWindow.cs  # menü arayüzü (IMGUI): arama, sekmeler, bilgi satırı
├── src/KeeperSpawner/ItemView.cs       # ızgara / liste çizimi, kategori bölümleri
├── src/KeeperSpawner/IconCache.cs      # item ikonlarını çözer ve önbelleğe alır
├── src/KeeperSpawner/Categories.cs     # item → kategori kuralları
├── src/KeeperSpawner/InputBlocker.cs   # menü açıkken oyun girdisini askıya alır
├── src/KeeperSpawner/Strings.cs        # mod arayüzü metinleri (TR/EN)
├── Directory.Build.props               # Directory.Build.props.user'ı içe aktarır
├── Directory.Build.props.user          # GamePath (yerel, git'e girmez)
├── docs/game-api.md                    # Faz 1 keşif notları (oyun API'si)
├── tools/ue/                           # UnityExplorer C# konsol betikleri
├── decompiled/                         # oyun kodunun okunabilir hali (git'e girmez, ASLA yayınlanmaz)
└── workshop/                           # preview.png, açıklama, INSTALL.txt, .vdf
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
- Adet seçimi (1 / 10 / Maks, Shift+tık, bilgi panelinde miktar) → v0.4.0 arayüz tasarımıyla.
- Item adları oyunun aktif diliyle; mod arayüzü TR/EN.
- Sorunlu itemlar (görev itemları) için kara liste.
- İsteğe bağlı: oyunun stiline uygun uGUI arayüz.

### Faz 4 — Test
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

### Faz 6 — Bakım
- Oyun güncellemesi metotları bozarsa mod çökmez; loga uyarı yazıp kendini devre dışı bırakır.
- SemVer (1.0.0, 1.1.0…) ve Atölye değişiklik notları.

## Riskler

- GK2 Atölyesi resmi olarak sadece çeviriler için; kod modları "Translation" etiketiyle yükleniyor. Workshop Loader'ın Atölye sayfası kural ihlali gerekçesiyle gizlenmiş → modumuz da kaldırılabilir. Yedek plan: Nexus Mods.
- Rakipler: Graveyard Keeper II Plus, Kebo Mod All-in-One, GK2 Cheat Menu (Nexus), GK2 Power Menu (Atölye). Farkımız: sadece spawner'a odaklı, hafif, ikonlu/kategorili temiz arayüz, Atölye'de.

## Kaynaklar

- GK2 Atölyesi: https://steamcommunity.com/app/4358690/workshop/
- BepInEx sürümleri: https://github.com/BepInEx/BepInEx/releases
- GK2 Workshop Loader (mod yazarı notları `docs/FOR_MODDERS.txt`): https://github.com/Zoriten/-GK2-WorkshopLoader
