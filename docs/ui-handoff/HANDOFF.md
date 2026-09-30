# KeeperSpawner 0.4 — UI Handoff

Bu klasör, KeeperSpawner'ın yeni oyun-içi pencere tasarımını Claude Code'a aktarmak için hazırlandı.

- `HANDOFF.md` — bu dosya: Claude Code'a verilecek görev + tasarım spesifikasyonu
- `reference-ui.html` — tasarımın kaynak kodu (HTML/CSS + davranış mantığı). Tek başına tarayıcıda çalışmaz (tasarım aracının runtime'ına bağlı); **ölçü, renk ve davranış referansı** olarak okunmalı. Tüm değerlerin kesin kaynağı burası.
- (önerilen) `reference.png` — tasarım tuvalinden Share › Export ile alınan PNG; görsel karşılaştırma için bu klasöre koy.

---

## 1. Claude Code'a verilecek görev

> KeeperSpawner modunun spawner penceresini `docs/ui-handoff/` altındaki tasarıma göre yeniden yaz. `HANDOFF.md` spesifikasyon, `reference-ui.html` ölçü/renk/davranış için kesin kaynak, `reference.png` görsel hedef.
>
> Kurallar:
> 1. Önce mevcut kodu incele: pencere hangi UI sistemiyle çiziliyor (IMGUI `OnGUI`/`GUI.Window` mi, uGUI mi), item listesi, kategoriler, favoriler, item ikonları ve envantere ekleme nereden geliyor. Mevcut UI sistemini koru, yeni bağımlılık ekleme.
> 2. Item verisi, ikonlar ve ekleme mantığı zaten modda var — tasarımdaki 55 örnek item ve piksel ikonlar **sadece yer tutucu**, onları taşıma. Gerçek item sprite'larını ve gerçek kategori/kalite verisini kullan.
> 3. Renkleri, ölçüleri ve durumları (hover/seçili/basılı) aşağıdaki tablolardan birebir al. Stilleri tek bir yerde topla (ör. `KsTheme` sınıfı: renkler, GUIStyle'lar, runtime'da üretilen Texture2D'ler).
> 4. Font: oyunun kendi piksel fontunu kullan (`Resources.FindObjectsOfTypeAll<Font>()` ile bul, adını logla). Bulunamazsa mevcut fontla devam et.
> 5. Pencere 1920×1080 referansında 1120×720; `GUI.matrix` (veya CanvasScaler) ile `Screen.height / 1080f` oranında ölçekle, ekranda ortala.
> 6. Aşamalı çalış ve her aşamadan sonra derle: (a) tema + çerçeve + başlık, (b) sol kategori listesi, (c) araç çubuğu + filtreler, (d) ızgara + liste görünümü, (e) sağ bilgi paneli, (f) alt çubuk + kısayollar. Her aşamanın sonunda ne değiştiğini özetle.
> 7. Oyun içinde test edemeyeceğin şeyleri (görsel hizalama vb.) bana bir kontrol listesi olarak bırak.

---

## 2. Yerleşim (1920×1080 referans, px)

```
┌─────────────────────────── Pencere 1120 × 720 ───────────────────────────┐
│ Başlık çubuğu (ahşap) 54px: ◆ [logo32] KeeperSpawner [v0.4.0] ──◆── [X] │
├──────────────┬──────────────────────────────────────────┬────────────────┤
│ Kategoriler  │ [🔍 Arama............][▦][☰]  Tık:[1][10][Maks] │ Item Bilgisi   │
│ 200px        │ Kalite [Hepsi][★Bronz][★Gümüş][★Altın] |  │ 272px          │
│              │ [☐ Görev itemlarını göster]     12 / 53 item │ büyük slot 84  │
│ Tümü     53  │ ┌──────────────────────────────────────┐ │ ad / kalite    │
│ Favoriler 3  │ │ İnşaat [14] ─────────────────────── │ │ ID  [kopyala]  │
│ Son ekl.  3  │ │ ▢▢▢▢▢▢▢▢▢▢   (10 sütun, 50px, gap 5) │ │ Maks yığın     │
│ ──◆──        │ │ Yiyecek [4] ─────────────────────── │ │ Miktar [-][n][+][Maks] │
│ İnşaat   14  │ │ ▢▢▢▢                                 │ │ [Envantere ekle ×n] │
│ ...          │ └──────────────────────────────────────┘ │ [☆ Favorilere ekle] │
│              │                                          │ Son Eklenenler │
│              │                                          │ ▢▢▢▢▢ (46px)   │
├──────────────┴──────────────────────────────────────────┴────────────────┤
│ Alt çubuk 38px: [toast / hover özeti]        [Tık] … [Shift+Tık] 1 adet … │
└──────────────────────────────────────────────────────────────────────────┘
```

- Gövde: padding 12, sütun arası 12. Orta sütun = kalan genişlik (~600).
- Orta sütun dikey: araç çubuğu 34 → 8 → filtre satırı 28 → 8 → içerik paneli (kalan, dikey kaydırmalı).
- İçerik paneli padding: üst 8, sağ 8, alt 14, sol 12. Bölüm başlığı: ad (16px) + sayı rozeti + 2px yatay çizgi; altında ızgara, bölüm sonu 12px boşluk.
- Kategori satırı: yükseklik 30, ikon 24, satır arası 2, liste padding 6. Hızlı grup (Tümü/Favoriler/Son eklenenler) ile kategoriler arasında ◆ ayırıcı.
- Liste görünümü satırı: yükseklik 44, sütunlar `40 | esnek | 150 (ID) | 92 (Kategori) | 44 (Yığın) | 36 (favori yıldızı)`, gap 10, satır arası 2px çizgi `#262a32`.

## 3. Renkler

| Token | Hex | Kullanım |
|---|---|---|
| backdrop | `#08080a` | pencere arkası (oyun sahnesini karartma, ~%70 opak siyah da olur) |
| frame-dark | `#15171c` | çerçeve dış/iç koyu halka |
| frame-steel | `#5b6272` | çerçeve çelik halka (4px) |
| rivet | `#8a92a2` / gölge `#4b515d` | köşe perçinleri 10px |
| window-bg | `#3a3f4a` | pencere zemini |
| panel-bg | `#2f343e` | sol/orta/sağ paneller, kenar 2px `#1b1e24` |
| wood | `#76583a` | başlık & şerit zemini; üst ışık 2px `#9c7a52`, alt gölge 3px `#4a3421`; 7px'de bir 2px %8 siyah yatay çizgi (ahşap damarı) |
| wood-on | `#6f5236` | seçili kategori / aktif buton; üst `#92704a`, alt `#4a3421` |
| strip-text | `#f3e2b8` + gölge `0 2px #2b1b0f` | şerit başlıkları |
| title-text | `#f6e6bd` | pencere başlığı 24px bold |
| accent | `#e0b050` | ◆ süsler, hover kenarı, seçili slot kenarı |
| slot-bg | `#262a33` | slot; kenar 2px `#16181d`; iç bevel üst-sol `#353a47`, alt-sağ `#1d2027` |
| slot-hover | `#30353f` + kenar accent | |
| slot-selected | `#3a3527` + kenar accent + iç 2px `#6e5222` | |
| count | `#f2c94c` + 1px siyah kontur | slot sağ-alt adet, 12px |
| btn-bg | `#2a2f3b`, kenar `#151820`, üst ışık `#4a5264` | ikincil butonlar/çipler; hover `#363c4a` |
| input-bg | `#1d2028`, kenar `#121419`, placeholder `#8a909e` | arama, miktar |
| primary | `#9a6424`, kenar `#2a1a0a`, üst `#d49a50`, alt `#6a3f14`, yazı `#fff2d0` | "Envantere ekle"; hover `#ab7230` |
| close | `#8c1f1f`, kenar `#2a0a0a`, üst `#c84a3a` | X butonu; hover `#a52a24` |
| item-name | `#eaa640` | bilgi paneli ve alt çubukta item adı |
| text | `#e6dcc8` / ikincil `#d9ccb4` / soluk `#a9aeb8` | |
| success | `#a6db6e` | "+20 … envantere eklendi" bildirimi |
| footer-bg | `#23272f`, üst kenar 2px `#15171c` | |
| star-bronze / silver / gold | `#d99555` / `#cfd8e2` / `#f2c94c` | kalite yıldızları (siyah konturlu) |

## 4. Bileşen durumları

- **Slot (50×50)**: normal / hover (accent kenar) / seçili (son tıklanan) / klavye odağı (`#f6dc7a` dış çizgi). Sağ-üstte kalite yıldızı 12px, sol-üstte favori ise 6px accent kare, sağ-altta maks yığın.
- **Kategori satırı**: normal (şeffaf) / hover `#434956` / seçili `wood-on` + açık yazı. Sağda item sayısı (12px soluk).
- **Segment butonlar** (görünüm, Tık miktarı, kalite çipleri): aynı anda tek seçili, seçili = `wood-on`.
- **Checkbox**: 18px kare, işaretli = `#9a6424` zemin + ortada 8px `#fff2d0` kare.

## 5. Davranış

| Etkileşim | Sonuç |
|---|---|
| Slot'a sol tık | "Tık" moduna göre ekle: `1`, `10` (yığından büyükse yığın kadar), `Maks` = maks yığın. Item seçili olur. |
| Shift + sol tık | 1 adet ekle |
| Sağ tık | Favoriye ekle/çıkar |
| Hover | Sağ panel o itemı gösterir; alt çubukta `Ad · Kalite · id · xN · Kategori`. Fare ızgaradan çıkınca panel seçili iteme döner. |
| Arama | Ad **veya** id içinde, büyük/küçük harf duyarsız, Türkçe kültürle (`ToLower(new CultureInfo("tr-TR"))`). Doluyken sağda temizle (×) butonu. |
| Kalite filtresi | Hepsi / Bronz / Gümüş / Altın |
| "Görev itemlarını göster" | Kapalıyken görev itemları gizli ("Görev" kategorisi seçiliyken yine de gösterilir). Toplam sayıya da yansır. |
| Kategori "Tümü" | Her kategori ayrı başlıklı bölüm olarak listelenir; boş bölümler gizlenir. |
| Favoriler / Son eklenenler | Tek bölüm. Son eklenenler en yeni başta, en fazla 5, aynı item tekrar eklenince başa taşınır. |
| Bilgi paneli miktar | Varsayılan = maks yığın; `-`/`+` 1..999; `Maks` yığına eşitler; seçim değişince sıfırlanır. "Envantere ekle ×n" bu miktarı ekler. |
| ID kopyala | `GUIUtility.systemCopyBuffer = id`; 1.8 sn "ID panoya kopyalandı". |
| Ekleme sonrası | Alt çubukta 2.6 sn yeşil bildirim: `+20  Ahşap Aksam (bronz)  envantere eklendi` |
| Son Eklenenler slotu | Tıklayınca aynı miktarı tekrar ekler |
| Sonuç yok | Ortada "Bu filtrelerle item bulunamadı" + "Filtreleri sıfırla" (arama + kalite sıfırlanır) |
| Kısayollar | Ctrl+F arama kutusuna odak, Esc kapat |
| Kalıcılık | Favoriler, görünüm (ızgara/liste), Tık modu ve son seçili kategori config'e (ör. BepInEx `ConfigEntry`) kaydedilsin. |

## 6. Uygulama ipuçları (Unity)

- Düz renk + bevel görünümü için 4×4 veya 8×8 `Texture2D` üret, `filterMode = Point`, `GUIStyle.border` ile 9-slice yap (ör. slot: kenar 2px koyu, içte 2px bevel). Her durum (normal/hover/active/onNormal) için ayrı texture.
- Ahşap damarı: 1×7 dikey texture'ı `wrapMode = Repeat` ile döşe, ya da düz `#76583a` ile başla — damar isteğe bağlı.
- Kaydırma alanında yalnızca görünen satırları çiz (726 item × çok bölüm IMGUI'de ağırlaşabilir): satır yüksekliği sabit olduğu için görünür aralığı `scrollPosition.y`'den hesapla.
- Metin gölgesi/kontur: aynı metni 1px kaydırıp koyu renkte önce çiz.
- Oyunun item sprite'ları farklı boyutlardaysa slot içinde 32px kutuya en-boy oranını koruyarak sığdır.
