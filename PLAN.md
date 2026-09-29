# KeeperSpawner — Graveyard Keeper 2 Item Spawner Modu

Oyun içinde bir tuşla açılan menüden itemları listeleyip tıklanan itemı envantere ekleyen BepInEx modu.

## Kararlar

| Konu | Karar |
|---|---|
| Mod adı | **KeeperSpawner** |
| Kısayol tuşu | **P** (ayar dosyasından değiştirilebilir) |
| Tıklama davranışı | Şimdilik itemın **max stack**'i kadar ekler |
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
├── src/KeeperSpawner/ItemCatalog.cs    # tüm itemları okur, önbelleğe alır
├── src/KeeperSpawner/Spawner.cs        # envantere ekleme
├── src/KeeperSpawner/SpawnerWindow.cs  # menü arayüzü
├── Directory.Build.props.user          # GamePath (yerel, git'e girmez)
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

### Faz 1 — Keşif (en kritik)
Oyun kodunda bulunacaklar:
- Item veritabanı: tüm item tanımları (kimlik, çeviri anahtarı / görünen ad, ikon, max stack).
- Oyuncu envanterine item ekleyen metot.
- "Kayıt yüklendi / oyun içindeyiz" durumu (menü ana menüde açılmamalı).
- Envanter dolunca oyunun davranışı.

**Ara hedef:** UnityExplorer konsolundan bulunan metodu elle çağırıp envantere 1 item eklemek.

### Faz 2 — MVP
- BepInEx eklenti iskeleti (`[BepInPlugin]`, yüklenince log).
- Config (`BepInEx\config\<guid>.cfg`): kısayol tuşu (varsayılan `P`).
- P ile açılıp kapanan pencere: arama kutusu + kaydırılabilir item listesi; tıklanınca max stack kadar envantere ekle.
- **Dikkat:** Arama kutusuna yazı yazılırken "p" harfi menüyü kapatmamalı → metin alanı odaktayken kısayol yok sayılır.
- **Dikkat:** P tuşunun oyunda başka bir işlevi var mı kontrol et (oyunun tuş ayarları).
- Menü açıkken oyun tıklamaları/hareketi engellenir.
- Item listesi bir kez okunup önbelleğe alınır.
- Stacklenemeyen itemlarda max stack = 1.

### Faz 3 — Cilalama
- İkonlu ızgara görünümü (oyunun sprite'ları).
- Kategori sekmeleri, favoriler, son spawn edilenler.
- Adet seçimi (1 / 10 / max) — tıklama davranışı ileride genişletilecek.
- Item adları oyunun aktif diliyle; mod arayüzü TR/EN.
- Sorunlu itemlar (görev itemları) için kara liste.
- İsteğe bağlı: oyunun stiline uygun uGUI arayüz.

### Faz 4 — Test
- Yeni kayıt / eski kayıt; spawn edilen itemlar kaydet-yükle sonrası duruyor mu.
- Envanter doluyken; stack sınırları; stacklenemeyen itemlar.
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
