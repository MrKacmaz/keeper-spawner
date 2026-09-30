# Changelog

Sürümler [SemVer](https://semver.org/) kurallarına göre: düzeltmeler `1.0.x`, yeni özellikler `1.x.0`.
Atölye değişiklik notları bu dosyadan alınır.

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
