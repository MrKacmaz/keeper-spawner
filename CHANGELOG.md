# Changelog

Sürümler [SemVer](https://semver.org/) kurallarına göre: düzeltmeler `1.0.x`, yeni özellikler `1.x.0`.
Atölye değişiklik notları bu dosyadan alınır.

## 1.3.0 — 2026-10-09 — Ceset spawn desteği

- O menüsünde Zombiler'in yanına ayrı **Cesetler** bölümü eklendi: en yüksek beyaz veya kırmızı kafatası için iki hazır organ seti. Her tıklama karakterin önüne bir ceset bırakır. Toplamlar ve içerik menüde görülebilir. Cesetler zombi olarak kaydedilmez; canlandırma için oyunun doğal beden verileri korunur. Yeni metinler 17 dili destekler.

### English

- Added a separate **Corpses** section next to Zombies in the O menu, with maximum-white and maximum-red skull organ presets. Each click drops one corpse in front of the player, with skull totals and contents shown in the menu. Corpses are not registered as zombies; native body metadata is retained for resurrection. New text supports all 17 game languages.

### Validation / Doğrulama

Release build: 0 warnings, 0 errors. Isolated zombie and corpse checks passed: organ maxima, native corpse templates, collar independence, contents and preview totals, resurrection metadata, placement, unique bodies, zombie registration, failed drops and invalid game state. In-game testing has not been performed for this release.

Release derlemesi: 0 uyarı, 0 hata. İzole zombi ve ceset kontrolleri geçti: organ seçimleri, doğal ceset şablonu, tasma bağımsızlığı, içerik ve toplamlar, canlandırma verileri, konum, benzersiz bedenler, zombi kaydı, başarısız bırakma ve geçersiz oyun durumu. Bu sürüm için oyun içi test yapılmadı.

## 1.2.1 — 2026-10-09 — Faith desteği

- Faith'in item listesinde görünmemesi düzeltildi. Artık **Tümü** ve **Kilise** kategorilerinde görünür; `faith` ID'si veya oyundaki adıyla aranabilir ve mevcut miktar seçenekleriyle envantere eklenebilir.

### English

- Fixed Faith being missing from the item list. It now appears under **All** and **Church**, can be found by its `faith` ID or localized name, and can be added to inventory using the existing amount controls.

### Validation / Doğrulama

Release build: 0 warnings, 0 errors. Catalog filtering, category assignment and the existing inventory path were reviewed against the game code. In-game testing has not been performed for this release.

Release derlemesi: 0 uyarı, 0 hata. Katalog filtresi, kategori ataması ve mevcut envantere ekleme akışı oyun koduyla karşılaştırıldı. Bu sürüm için oyun içi test yapılmadı.

## 1.2.0 — 2026-10-09 — Envanter kapasitesi ve zombi spawn

- O menüsüne **Zombiler** sekmesi eklendi: en yüksek beyaz veya kırmızı kafatası için iki hazır zombi. Oyunun her ana organ türündeki en yüksek kafatası değerine sahip organlar, son seviye beden içeriği ve uyumlu tasma kullanılır. Her tıklama karakterin önüne bir zombi bırakır; oyuncu envanterine eklenmez. Yeni metinler 17 dili destekler.
- Envanter kapasitesi ayarı GK2 Mod Framework Mods menüsüne eklendi. Ayarın adı ve açıklaması oyunun 17 dilini destekler (es-mx, İspanyolca çevirisini kullanır); config açıklaması da tüm çevirileri içerir.
- Config dosyasına `Inventory.Size` eklendi: oyuncunun temel envanter kapasitesi 40–1000 slot olarak belirlenebilir; 0 mevcut kapasiteyi korur. Düşürürken dolu slotlar korunur. Kapasite oyun kaydına yazılır.
- Kurulum ZIP'i changelog dosyasını da içerir.

### English

- Added a **Zombies** tab to the O menu with maximum-white and maximum-red skull presets, using the best available organ of each main type, final-tier body contents and a compatible collar. Each click drops one registered zombie in front of the player. New text supports all 17 game languages.
- Added the inventory capacity setting to the GK2 Mod Framework Mods menu, with its name and description in all 17 game languages (es-mx reuses Spanish). The config description includes all translations too.
- Added `Inventory.Size` to configure player inventory capacity (40–1000 slots, 0 leaves it unchanged), preserving occupied slots when reducing capacity. Capacity is saved with the game.
- The installation ZIP now includes the changelog.

### Validation / Doğrulama

Release build: 0 warnings, 0 errors. Isolated zombie logic checks passed (organ selection, final-tier template, compatible collar, contents, placement, unique spawns and failed-drop cleanup). In-game testing has not been performed for this release.

Release derlemesi: 0 uyarı, 0 hata. İzole zombi mantık kontrolleri geçti (organ seçimi, son seviye şablon, uyumlu tasma, içerik, konum, benzersiz zombiler ve başarısız bırakmada temizlik). Bu sürüm için oyun içi test yapılmadı.

## 1.1.0 — GK2 Mod Framework desteği

Graveyard Keeper 2 1.007.1 (Steam build 25601286) ve GK2 Mod Framework 0.1.19 ile test edildi.

- [GK2 Mod Framework](https://www.nexusmods.com/graveyardkeeper2/mods/42) kuruluysa KeeperSpawner oyunun **Mods** menüsünde görünür: sürüm, durum ("Etkin, oyunda O tuşuna bas" / "Devre dışı"), açıklama ve ayarlar (açma tuşu, tık başına miktar, görev itemları, görünüm, arayüz ölçeği, arka plan karartma). Ayarlar KeeperSpawner'ın kendi config dosyasına yazılır.
- Framework isteğe bağlı: entegrasyon ayrı bir DLL'de (`KeeperSpawner.GK2Framework.dll`). Framework yoksa BepInEx sadece onu atlar, KeeperSpawner eskisi gibi çalışır.
- Mods menüsü metinleri oyunun 17 dilinde.
- Log mesajları İngilizce (hata raporlarını herkes okuyabilsin diye).

### English

Tested with Graveyard Keeper 2 1.007.1 (Steam build 25601286) and GK2 Mod Framework 0.1.19.

- With [GK2 Mod Framework](https://www.nexusmods.com/graveyardkeeper2/mods/42) installed, KeeperSpawner shows up in the game's **Mods** menu: version, status ("Active, press O in game" / "Disabled"), description and settings (hotkey, amount per click, quest items, view, UI scale, background dimming). Settings are saved to KeeperSpawner's own config file.
- The Framework is optional: the integration lives in a separate DLL (`KeeperSpawner.GK2Framework.dll`). Without the Framework, BepInEx skips only that DLL and KeeperSpawner works as before.
- Mods menu texts in all 17 languages of the game.
- Log messages are now in English, so anyone can read bug reports.

## 1.0.0 — ilk yayın

Graveyard Keeper 2 1.007.1 ile test edildi.

- O tuşuyla açılan item spawner penceresi (tuş ayardan değiştirilebilir).
- 726 normal item; oyunun ikonları, oyunun dilindeki adlar, bronz / gümüş / altın kalite yıldızları.
- 15 kategori, isim ya da id ile Türkçe harflere duyarsız arama, kalite filtresi, ızgara / liste görünümü.
- Tık 1 / 10 / Maks ekler, Shift+Tık 1 adet ekler; bilgi panelinden 1–999 arası miktar.
- Favoriler (sağ tık) ve son eklenen 5 item; ayar dosyasına kaydedilir.
- Görev itemları varsayılan olarak gizli.
- Arayüz oyunun 17 dilinde.
- Oyun güncellemesinden sonra kullanılan oyun API'leri açılışta kontrol edilir; bir şey değiştiyse mod çökmek yerine kendini kapatır ve sebebini loga yazar.

### English

First release, tested with Graveyard Keeper 2 1.007.1.

- Item spawner window opened with O (configurable).
- 726 regular items with the game's icons, names in the game language and bronze / silver / gold quality stars.
- 15 categories, accent-insensitive search by name or id, quality filter, grid / list view.
- Click adds 1 / 10 / Max, Shift+Click adds 1; the info panel adds any amount from 1 to 999.
- Favorites (right-click) and the last 5 added items, saved in the config.
- Quest items hidden by default.
- UI in all 17 languages of the game.
- After a game update the mod checks the game API it uses at startup and switches itself off with a log message instead of breaking.
