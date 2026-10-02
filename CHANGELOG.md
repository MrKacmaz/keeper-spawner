# Changelog

Sürümler [SemVer](https://semver.org/) kurallarına göre: düzeltmeler `1.0.x`, yeni özellikler `1.x.0`.
Atölye değişiklik notları bu dosyadan alınır.

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
