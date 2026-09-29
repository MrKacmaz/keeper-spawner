# GK2 oyun API notları (Faz 1 keşfi)

Oyun sürümü 1.007.1 (buildid 25601286) üzerinde `Assembly-CSharp.dll` ve `LazyBearTechnology.dll` decompile edilerek çıkarıldı. Oyun kodu bu repoda **tutulmaz**; `decompiled/` klasörü yerel ve git dışında.

## Item veritabanı

- `GameBalance.Me.itemDefs : List<ItemDef>` tüm item tanımlarını tutar. `MainGame.Start()` içinde `GameBalance.LoadGameBalance()` ile yüklenir, yani ana menüde de hazırdır.
- Tek item: `GameBalance.Me.GetData<ItemDef>(id)` ya da null dönebilen `GetDataOrNull<ItemDef>(id)`.
- `ItemDef : LazyBearTechnology.BalanceBaseObject` alanları:
  - `id : string` → kimlik
  - `stackCount : int` → max stack. `1` stacklenemez demek; `0` olan tanım var mı runtime'da kontrol edilecek.
  - `type : ItemType` (enum), `itemGroupIds : List<string>` → kategoriler için
  - `isQuestItem`, `isBag`, `hasDurability`, `isTool`, `isWeapon`, `isSeed` … → filtre ve kara liste için
  - `iconId : string` → sprite adı
  - `sortOrder : int`
- Görünen ad: `ItemDef.GetHeader()`, aktif dile göre `LLBase.L(id)` döner. Aktif dil kodu: `LLBase.CurrentLang`.
- İkon: `LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(def.iconId)`. Önce `HasSprite(name)` ile kontrol edilebilir. Oyunun kendi `UIItemCell`'i de bu yolu kullanıyor.
- Sahte ya da özel id'ler: `"empty"`, `"faith"`, `"inventory"`, `"craftInventory"`. Listeden elenecek.

### Runtime dökümü (1.007.1, dil `tr`, 814 item)

| Bulgu | Adet | Faz 2'deki karşılığı |
|---|---|---|
| `stackCount <= 0` | 0 | Yine de `max(1, stackCount)` ile korunacak |
| `stackCount == 1` | 273 | Tek adet eklenir |
| `overhead` grubu: başta taşınan büyük itemlar (`wood`, `stone`, `marble`, `body_*`, `box_*`, `iron_ore_2h`) | 29 | **Gizlenecek.** Envantere girmezler; oyunun yerden toplama kodu da `ItemSize.Big` itemları reddediyor |
| `isQuestItem == true` | 35 | Varsayılan olarak gizli, ayarla açılabilir |
| `quest_bag` grubunda ama `isQuestItem == false` | 10 | Görev itemı sayılacak (ör. `intro_prison_note`, `keys_looters`, `dynamite`) |
| Test/geliştirici itemları (`test_*`, `pseudo_*`, `*_test`, `wood_OLD`, `chair`, `saw`, `fake_porter_slot_filler`) | 31+ | Gizlenecek |
| Envanter kabı sayılan id'ler (`empty`, `inventory`, `toolBeltInventory`, `craftInventory`) | 4 | Gizlenecek |
| Oyun kaynağı sayılan id'ler (`faith`, `temptation`, `fire`, `alchemy_flask`, `science`, `game_res_tech_*`, `town_happiness`) | ~9 | Gizlenecek. Bunlar envanter itemı değil, oyun kaynağı |
| Çevrilmemiş ad (ad == id) | 74 | Ad yerine id gösterilir, aramada ikisi birden aranır |
| Adda TextMeshPro etiketi var (`<sprite name="rune_r">` …) | 37 | IMGUI göstermez; etiketler `[R]`, `[G]` gibi kısa metinlere çevrilir |
| Yıldız varyantı (`id:1`, `id:2`, `id:3`, adları aynı) | 169 | Ada `★1` / `★2` / `★3` eklenir |
| İkon sprite'ı yok (`HasSprite == false`) | 73 | Yer tutucu gösterilir |

`type` dağılımı: 601 `None`, 69 `Preach` (dualar), geri kalanı alet, silah ve organ türleri. Kategori sekmeleri için `type` tek başına yetmiyor, `itemGroupIds` (`buil_bag`, `food_bag`, `alch_bag`, `tool`, `weapon`, `seed`, `fishes`, `bodypart` …) daha iyi bir ayrım veriyor.

## Envantere ekleme

Oyunun kendi senaryo düğümü `GK2.FlowCanvasNodes.Flow_AddItem` şunu yapıyor:

```csharp
MainGame.PlayerData.inventory.AddItemToInventory(new Item(itemId, count));
```

- `Inventory.AddItemToInventory(Item, Item ignoredBag = null, bool ignoreAllBags = false) : bool`
  - Başarılıysa `OnItemsAdd` olayını tetikler, envanter arayüzü kendini günceller.
  - Hiçbiri sığmazsa `false` döner ve `OnInventoryFull` tetiklenir. `UINotificator` bunu dinlediği için "inventory_is_full" bildirimi kendiliğinden çıkar.
  - **Kısmi ekleme:** sığmayan miktar kaynak `Item.Count`'ta kalır.
  - Uygun çanta (bag) varsa item çantaya da girebilir.
  - **Stacklenemeyen itemlarda (`stackCount == 1`) tek çağrı sadece 1 adet ekler.** Birden fazla eklemek için döngü gerekir.
- Kontrol metotları: `inventory.CanAddItemToInventory(id, count)` ve `inventory.Data.CanAddItemCountToInventory(item)`.
- Adet sorgusu: `inventory.Data.GetTotalCountInInventory(id)`.
- Oyunun "item eklendi" açılır bildirimi sadece yerden toplanan itemlarda çıkıyor (`PlayerData.OnDropCollected`). Doğrudan eklemede bildirim yok, geri bildirimi kendimiz vereceğiz.

## Oyun durumu

- `MainGame.Instance.gameState` → `MainGame.GameState.MainMenu` / `InGame`
- `MainGame.PlayerData` → oyuncu verisi. Ana menüde null olabilir ya da eski kayıttan kalmış olabilir, bu yüzden `gameState` ile birlikte kontrol edilmeli.
- Olaylar: `MainGame.OnGameStarted`, `MainGame.OnGoToMainMenu` (static `Action`)
- `MainGame.IsGamePaused`
- Oyuncu kontrolü: `MainGame.PlayerController.SetDisabledStateType(DisabledStateType, bool)`. Enum'da sadece `ByMainMenu`, `ByFlowScript`, `BySceneLoading` var; bunları ödünç almak oyunun kendi mantığıyla çakışabilir, Faz 2'de başka yol aranacak.

## Giriş

- Oyun **Rewired**'ı `LazyBearTechnology.LazyInput` üzerinden, soyut `GameKey` eylemleriyle kullanıyor (`Inventory`, `Map`, `QuestTree`, `CheatButton` …). Fiziksel tuş atamaları kodda değil, Rewired verisinde.
- Oyunun kendisi de doğrudan `UnityEngine.Input.GetKeyDown` kullanıyor, örneğin `Shift+F10` mod yeniden yükleme. Yani legacy Input çalışıyor.

## Mod ortamı

- `ModLoaderDetection` BepInEx'i görüyor (`BepInEx/`, `winhttp.dll`, `doorstop_config.ini`, `0Harmony` assembly), ama bunu **sadece** log'a (`loader: ...`) ve bug raporu meta verisine yazıyor. Engelleme ya da ceza yok.
- Resmi mod desteği (`ModsBootstrap`, `ModsPaths`) sadece dil paketleri ve seslendirme için. Atölye tarafı `SteamWorkshopCreatorService` / `SteamWorkshopInstalledItems`.

## UnityExplorer konsol betikleri

`tools/ue/` klasöründe:
- `01-dump-items.cs` → tüm itemları `decompiled/items.tsv` dosyasına döker
- `02-add-stick.cs` → envantere bir stack çubuk (`stick`) ekler (ara hedef)
