using System;
using System.Reflection;
using LazyBearTechnology;

namespace KeeperSpawner
{
    /// <summary>
    /// Mod arayüzü metinleri, oyunun desteklediği 17 dilde (LLBase.languages; es-mx İspanyolcayı kullanır).
    /// Bilinmeyen diller (topluluk dil modları) İngilizceye düşer.
    /// Her satır <see cref="LanguageIds"/> sırasıyla: en, tr, de, fr, es, pt-br, ru, uk-ua, it, pl, ja, zh_cn, zh_cht, ko, th, vn.
    /// </summary>
    internal static class Strings
    {
        private static readonly string[] LanguageIds =
        {
            "en", "tr", "de", "fr", "es", "pt-br", "ru", "uk-ua", "it", "pl", "ja", "zh_cn", "zh_cht", "ko", "th", "vn",
        };

        private static string cachedLang;
        private static int cachedIndex;
        private static string languageOverride;

        private static int Index
        {
            get
            {
                string lang = languageOverride ?? LLBase.CurrentLang;
                if (lang != cachedLang)
                {
                    cachedLang = lang;
                    int index = Array.IndexOf(LanguageIds, lang == "es-mx" ? "es" : lang);
                    cachedIndex = index < 0 ? 0 : index;
                }
                return cachedIndex;
            }
        }

        private static string T(string[] row) => row[Index] ?? row[0];

        /// <summary>
        /// Metni verilen dilde üretir. GK2 Mod Framework köprüsü Mods menüsü metinlerini kayıt anında bir kez alır;
        /// o an <c>LLBase.CurrentLang</c> henüz kayıtlı dile ayarlanmamış olabilir, bu yüzden Framework'ün okuduğu dili verir.
        /// Framework dil kodlarında "-" yerine "_" kullanır (pt_br, uk_ua, es_mx).
        /// </summary>
        public static string In(string lang, Func<string> text)
        {
            string previous = languageOverride;
            if (!string.IsNullOrEmpty(lang))
            {
                lang = lang.ToLowerInvariant();
                languageOverride = lang.StartsWith("zh") ? lang : lang.Replace('_', '-');
            }
            try
            {
                return text();
            }
            finally
            {
                languageOverride = previous;
            }
        }

        /// <summary>Tüm satırların dil sayısı kadar eleman içerdiğini kontrol eder (açılışta bir kez).</summary>
        public static void Validate()
        {
            foreach (var field in typeof(Strings).GetFields(BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (field.FieldType == typeof(string[]) && field.Name != nameof(LanguageIds)
                    && field.GetValue(null) is string[] row && row.Length != LanguageIds.Length)
                {
                    Plugin.Log.LogWarning($"Incomplete translation row: {field.Name} ({row.Length}/{LanguageIds.Length})");
                }
            }
        }

        // ------------------------------------------------------------------
        // Sol panel
        // ------------------------------------------------------------------

        private static readonly string[] categoriesRow = { "Categories", "Kategoriler", "Kategorien", "Catégories", "Categorías", "Categorias", "Категории", "Категорії", "Categorie", "Kategorie", "カテゴリー", "分类", "分類", "카테고리", "หมวดหมู่", "Danh mục" };
        private static readonly string[] allRow = { "All", "Tümü", "Alle", "Tout", "Todo", "Tudo", "Все", "Усі", "Tutto", "Wszystko", "すべて", "全部", "全部", "전체", "ทั้งหมด", "Tất cả" };
        private static readonly string[] favoritesRow = { "Favorites", "Favoriler", "Favoriten", "Favoris", "Favoritos", "Favoritos", "Избранное", "Обране", "Preferiti", "Ulubione", "お気に入り", "收藏", "收藏", "즐겨찾기", "รายการโปรด", "Yêu thích" };
        private static readonly string[] recentRow = { "Recently added", "Son eklenenler", "Zuletzt hinzugefügt", "Ajoutés récemment", "Añadidos recientemente", "Adicionados recentemente", "Недавно добавленные", "Нещодавно додані", "Aggiunti di recente", "Ostatnio dodane", "最近追加", "最近添加", "最近新增", "최근 추가", "เพิ่มล่าสุด", "Vừa thêm" };

        public static string Categories => T(categoriesRow);
        public static string All => T(allRow);
        public static string Favorites => T(favoritesRow);
        public static string Recent => T(recentRow);

        // ------------------------------------------------------------------
        // Araç çubuğu ve filtreler
        // ------------------------------------------------------------------

        private static readonly string[] searchPlaceholderRow =
        {
            "Search name or id  (e.g. ingot_iron)", "İsim veya id ara  (ör. kalas, ingot_iron)", "Name oder ID suchen  (z. B. ingot_iron)",
            "Rechercher un nom ou un id  (ex. ingot_iron)", "Buscar nombre o id  (p. ej. ingot_iron)", "Buscar nome ou id  (ex.: ingot_iron)",
            "Поиск по названию или id  (напр. ingot_iron)", "Пошук за назвою або id  (напр. ingot_iron)", "Cerca nome o id  (es. ingot_iron)",
            "Szukaj nazwy lub id  (np. ingot_iron)", "名前またはIDで検索  (例: ingot_iron)", "搜索名称或 ID（例：ingot_iron）",
            "搜尋名稱或 ID（例：ingot_iron）", "이름 또는 ID 검색  (예: ingot_iron)", "ค้นหาชื่อหรือ id  (เช่น ingot_iron)", "Tìm theo tên hoặc id  (vd: ingot_iron)",
        };
        private static readonly string[] clickRow = { "Click:", "Tık:", "Klick:", "Clic :", "Clic:", "Clique:", "Клик:", "Клік:", "Clic:", "Klik:", "クリック:", "点击:", "點擊:", "클릭:", "คลิก:", "Nhấp:" };
        private static readonly string[] maxRow = { "Max", "Maks", "Max", "Max", "Máx.", "Máx.", "Макс", "Макс", "Max", "Maks", "最大", "最大", "最大", "최대", "สูงสุด", "Tối đa" };
        private static readonly string[] qualityRow = { "Quality", "Kalite", "Qualität", "Qualité", "Calidad", "Qualidade", "Качество", "Якість", "Qualità", "Jakość", "品質", "品质", "品質", "품질", "คุณภาพ", "Chất lượng" };
        private static readonly string[] qualityAllRow = { "All", "Hepsi", "Alle", "Toutes", "Todas", "Todas", "Все", "Усі", "Tutte", "Wszystkie", "すべて", "全部", "全部", "전체", "ทั้งหมด", "Tất cả" };
        private static readonly string[] showQuestRow = { "Show quest items", "Görev itemlarını göster", "Questgegenstände anzeigen", "Afficher les objets de quête", "Mostrar objetos de misión", "Mostrar itens de missão", "Показывать квестовые предметы", "Показувати квестові предмети", "Mostra oggetti delle missioni", "Pokaż przedmioty z zadań", "クエストアイテムを表示", "显示任务物品", "顯示任務物品", "퀘스트 아이템 표시", "แสดงไอเท็มเควสต์", "Hiện vật phẩm nhiệm vụ" };
        private static readonly string[] itemsWordRow = { "items", "item", "Gegenstände", "objets", "objetos", "itens", "предм.", "предм.", "oggetti", "przedm.", "個", "件", "件", "개", "ชิ้น", "vật phẩm" };

        public static string SearchPlaceholder => T(searchPlaceholderRow);
        public static string Click => T(clickRow);
        public static string Max => T(maxRow);
        public static string Quality => T(qualityRow);
        public static string QualityAll => T(qualityAllRow);
        public static string ShowQuestItems => T(showQuestRow);
        public static string ItemCount(int shown, int total) => $"<color=#f2c94c>{shown}</color> / {total} {T(itemsWordRow)}";

        // ------------------------------------------------------------------
        // İçerik
        // ------------------------------------------------------------------

        private static readonly string[] columnItemRow = { "Item", "Item", "Gegenstand", "Objet", "Objeto", "Item", "Предмет", "Предмет", "Oggetto", "Przedmiot", "アイテム", "物品", "物品", "아이템", "ไอเท็ม", "Vật phẩm" };
        private static readonly string[] columnCategoryRow = { "Category", "Kategori", "Kategorie", "Catégorie", "Categoría", "Categoria", "Категория", "Категорія", "Categoria", "Kategoria", "カテゴリー", "分类", "分類", "카테고리", "หมวดหมู่", "Danh mục" };
        private static readonly string[] columnStackRow = { "Stack", "Yığın", "Stapel", "Pile", "Pila", "Pilha", "Стак", "Стос", "Pila", "Stos", "スタック", "堆叠", "堆疊", "스택", "กอง", "Chồng" };
        private static readonly string[] emptyTitleRow = { "No items match these filters", "Bu filtrelerle item bulunamadı", "Keine Gegenstände für diese Filter", "Aucun objet ne correspond à ces filtres", "Ningún objeto coincide con estos filtros", "Nenhum item corresponde a esses filtros", "Нет предметов по этим фильтрам", "Немає предметів за цими фільтрами", "Nessun oggetto corrisponde ai filtri", "Brak przedmiotów dla tych filtrów", "条件に一致するアイテムがありません", "没有符合筛选条件的物品", "沒有符合篩選條件的物品", "조건에 맞는 아이템이 없습니다", "ไม่พบไอเท็มที่ตรงกับตัวกรองนี้", "Không có vật phẩm phù hợp bộ lọc" };
        private static readonly string[] emptyHintRow = { "Try changing the search or quality filter.", "Aramayı veya kalite filtresini değiştirmeyi dene.", "Ändere die Suche oder den Qualitätsfilter.", "Modifiez la recherche ou le filtre de qualité.", "Prueba a cambiar la búsqueda o el filtro de calidad.", "Tente mudar a busca ou o filtro de qualidade.", "Измените поиск или фильтр качества.", "Змініть пошук або фільтр якості.", "Prova a cambiare la ricerca o il filtro qualità.", "Zmień wyszukiwanie lub filtr jakości.", "検索または品質フィルターを変更してください。", "请尝试更改搜索或品质筛选。", "請嘗試變更搜尋或品質篩選。", "검색어나 품질 필터를 바꿔 보세요.", "ลองเปลี่ยนคำค้นหาหรือตัวกรองคุณภาพ", "Hãy thử đổi từ khóa hoặc bộ lọc chất lượng." };
        private static readonly string[] resetFiltersRow = { "Reset filters", "Filtreleri sıfırla", "Filter zurücksetzen", "Réinitialiser les filtres", "Restablecer filtros", "Redefinir filtros", "Сбросить фильтры", "Скинути фільтри", "Azzera filtri", "Resetuj filtry", "フィルターをリセット", "重置筛选", "重設篩選", "필터 초기화", "รีเซ็ตตัวกรอง", "Đặt lại bộ lọc" };
        private static readonly string[] questRow = { "quest", "görev", "Quest", "quête", "misión", "missão", "квест", "квест", "missione", "zadanie", "クエスト", "任务", "任務", "퀘스트", "เควสต์", "nhiệm vụ" };

        public static string ColumnItem => T(columnItemRow);
        public static string ColumnId => "ID";
        public static string ColumnCategory => T(columnCategoryRow);
        public static string ColumnStack => T(columnStackRow);
        public static string EmptyTitle => T(emptyTitleRow);
        public static string EmptyHint => T(emptyHintRow);
        public static string ResetFilters => T(resetFiltersRow);
        public static string Quest => T(questRow);

        // ------------------------------------------------------------------
        // Bilgi paneli
        // ------------------------------------------------------------------

        private static readonly string[] itemInfoRow = { "Item Info", "Item Bilgisi", "Gegenstand", "Infos objet", "Info del objeto", "Info do item", "Сведения", "Відомості", "Info oggetto", "Informacje", "アイテム情報", "物品信息", "物品資訊", "아이템 정보", "ข้อมูลไอเท็ม", "Thông tin" };
        private static readonly string[] maxStackRow = { "Max stack", "Maks yığın", "Max. Stapel", "Pile max", "Pila máx.", "Pilha máx.", "Макс. стак", "Макс. стос", "Pila max", "Maks. stos", "最大スタック", "最大堆叠", "最大堆疊", "최대 스택", "กองสูงสุด", "Chồng tối đa" };
        private static readonly string[] idCopiedRow = { "ID copied to clipboard", "ID panoya kopyalandı", "ID kopiert", "ID copié", "ID copiado", "ID copiado", "ID скопирован", "ID скопійовано", "ID copiato", "Skopiowano ID", "IDをコピーしました", "已复制 ID", "已複製 ID", "ID를 복사했습니다", "คัดลอก ID แล้ว", "Đã sao chép ID" };
        private static readonly string[] amountRow = { "Amount", "Miktar", "Menge", "Quantité", "Cantidad", "Quantidade", "Количество", "Кількість", "Quantità", "Ilość", "数量", "数量", "數量", "수량", "จำนวน", "Số lượng" };
        private static readonly string[] addToInventoryRow = { "Add to inventory", "Envantere ekle", "Ins Inventar", "Ajouter à l'inventaire", "Añadir al inventario", "Adicionar ao inventário", "В инвентарь", "До інвентаря", "Aggiungi all'inventario", "Dodaj do ekwipunku", "インベントリに追加", "添加到背包", "加入背包", "인벤토리에 추가", "เพิ่มเข้าคลัง", "Thêm vào túi" };
        private static readonly string[] addFavoriteRow = { "Add to favorites", "Favorilere ekle", "Zu Favoriten", "Ajouter aux favoris", "Añadir a favoritos", "Adicionar aos favoritos", "В избранное", "До обраного", "Aggiungi ai preferiti", "Dodaj do ulubionych", "お気に入りに追加", "加入收藏", "加入收藏", "즐겨찾기 추가", "เพิ่มในรายการโปรด", "Thêm vào yêu thích" };
        private static readonly string[] inFavoritesRow = { "In favorites", "Favorilerde", "In Favoriten", "Dans les favoris", "En favoritos", "Nos favoritos", "В избранном", "В обраному", "Nei preferiti", "W ulubionych", "お気に入り済み", "已收藏", "已收藏", "즐겨찾기됨", "อยู่ในรายการโปรด", "Đã yêu thích" };
        private static readonly string[] recentTitleRow = { "Recently Added", "Son Eklenenler", "Zuletzt hinzugefügt", "Ajoutés récemment", "Añadidos recientes", "Adicionados recentes", "Недавние", "Нещодавні", "Aggiunti di recente", "Ostatnio dodane", "最近追加", "最近添加", "最近新增", "최근 추가", "เพิ่มล่าสุด", "Vừa thêm" };
        private static readonly string[] noRecentRow = { "Nothing added yet.", "Henüz bir şey eklemedin.", "Noch nichts hinzugefügt.", "Rien ajouté pour l'instant.", "Aún no has añadido nada.", "Nada adicionado ainda.", "Пока ничего не добавлено.", "Поки нічого не додано.", "Ancora niente aggiunto.", "Nic jeszcze nie dodano.", "まだ何も追加していません。", "尚未添加任何物品。", "尚未新增任何物品。", "아직 추가한 것이 없습니다.", "ยังไม่ได้เพิ่มอะไร", "Chưa thêm gì." };

        public static string ItemInfo => T(itemInfoRow);
        public static string MaxStack => T(maxStackRow);
        public static string IdCopied => T(idCopiedRow);
        public static string Amount => T(amountRow);
        public static string AddToInventory(int amount) => T(addToInventoryRow) + "  ×" + amount;
        public static string AddFavorite => T(addFavoriteRow);
        public static string InFavorites => T(inFavoritesRow);
        public static string RecentTitle => T(recentTitleRow);
        public static string NoRecent => T(noRecentRow);

        // ------------------------------------------------------------------
        // Kalite
        // ------------------------------------------------------------------

        private static readonly string[] bronzeRow = { "Bronze", "Bronz", "Bronze", "Bronze", "Bronce", "Bronze", "Бронза", "Бронза", "Bronzo", "Brąz", "銅", "铜", "銅", "동", "ทองแดง", "Đồng" };
        private static readonly string[] silverRow = { "Silver", "Gümüş", "Silber", "Argent", "Plata", "Prata", "Серебро", "Срібло", "Argento", "Srebro", "銀", "银", "銀", "은", "เงิน", "Bạc" };
        private static readonly string[] goldRow = { "Gold", "Altın", "Gold", "Or", "Oro", "Ouro", "Золото", "Золото", "Oro", "Złoto", "金", "金", "金", "금", "ทอง", "Vàng" };
        private static readonly string[] starBronzeRow = { "Bronze star", "Bronz yıldız", "Bronzestern", "Étoile de bronze", "Estrella de bronce", "Estrela de bronze", "Бронзовая звезда", "Бронзова зірка", "Stella di bronzo", "Brązowa gwiazda", "銅の星", "铜星", "銅星", "동 별", "ดาวทองแดง", "Sao đồng" };
        private static readonly string[] starSilverRow = { "Silver star", "Gümüş yıldız", "Silberstern", "Étoile d'argent", "Estrella de plata", "Estrela de prata", "Серебряная звезда", "Срібна зірка", "Stella d'argento", "Srebrna gwiazda", "銀の星", "银星", "銀星", "은 별", "ดาวเงิน", "Sao bạc" };
        private static readonly string[] starGoldRow = { "Gold star", "Altın yıldız", "Goldstern", "Étoile d'or", "Estrella de oro", "Estrela de ouro", "Золотая звезда", "Золота зірка", "Stella d'oro", "Złota gwiazda", "金の星", "金星", "金星", "금 별", "ดาวทอง", "Sao vàng" };

        public static string StarShort(int star)
        {
            switch (star)
            {
                case 1: return T(bronzeRow);
                case 2: return T(silverRow);
                case 3: return T(goldRow);
                default: return string.Empty;
            }
        }

        public static string Star(int star)
        {
            switch (star)
            {
                case 1: return T(starBronzeRow);
                case 2: return T(starSilverRow);
                case 3: return T(starGoldRow);
                default: return string.Empty;
            }
        }

        // ------------------------------------------------------------------
        // Alt çubuk
        // ------------------------------------------------------------------

        private static readonly string[] addedRow = { "added to inventory", "envantere eklendi", "zum Inventar hinzugefügt", "ajouté à l'inventaire", "añadido al inventario", "adicionado ao inventário", "добавлено в инвентарь", "додано до інвентаря", "aggiunto all'inventario", "dodano do ekwipunku", "インベントリに追加しました", "已添加到背包", "已加入背包", "인벤토리에 추가됨", "เพิ่มเข้าคลังแล้ว", "đã thêm vào túi" };
        private static readonly string[] addedPartialRow = { "added, inventory full", "eklendi, envanter doldu", "hinzugefügt, Inventar voll", "ajouté, inventaire plein", "añadido, inventario lleno", "adicionado, inventário cheio", "добавлено, инвентарь полон", "додано, інвентар заповнений", "aggiunto, inventario pieno", "dodano, ekwipunek pełny", "追加しました（インベントリ満杯）", "已添加，背包已满", "已加入，背包已滿", "추가됨, 인벤토리 가득 참", "เพิ่มแล้ว คลังเต็ม", "đã thêm, túi đầy" };
        private static readonly string[] inventoryFullRow = { "Inventory full", "Envanter dolu", "Inventar voll", "Inventaire plein", "Inventario lleno", "Inventário cheio", "Инвентарь полон", "Інвентар заповнений", "Inventario pieno", "Ekwipunek pełny", "インベントリがいっぱいです", "背包已满", "背包已滿", "인벤토리가 가득 참", "คลังเต็ม", "Túi đầy" };
        private static readonly string[] errorRow = { "Error, see BepInEx log", "Hata, ayrıntılar BepInEx logunda", "Fehler, siehe BepInEx-Log", "Erreur, voir le journal BepInEx", "Error, ver el registro de BepInEx", "Erro, veja o log do BepInEx", "Ошибка, см. журнал BepInEx", "Помилка, див. журнал BepInEx", "Errore, vedi il log di BepInEx", "Błąd, zobacz log BepInEx", "エラー: BepInEx のログを確認", "错误，请查看 BepInEx 日志", "錯誤，請查看 BepInEx 日誌", "오류, BepInEx 로그 확인", "ข้อผิดพลาด ดูบันทึก BepInEx", "Lỗi, xem log BepInEx" };
        private static readonly string[] keyClickRow = { "Click", "Tık", "Klick", "Clic", "Clic", "Clique", "Клик", "Клік", "Clic", "Klik", "クリック", "点击", "點擊", "클릭", "คลิก", "Nhấp" };
        private static readonly string[] keyShiftClickRow = { "Shift+Click", "Shift+Tık", "Umschalt+Klick", "Maj+Clic", "Mayús+Clic", "Shift+Clique", "Shift+Клик", "Shift+Клік", "Maiusc+Clic", "Shift+Klik", "Shift+クリック", "Shift+点击", "Shift+點擊", "Shift+클릭", "Shift+คลิก", "Shift+Nhấp" };
        private static readonly string[] keyRightClickRow = { "Right-click", "Sağ tık", "Rechtsklick", "Clic droit", "Clic derecho", "Clique direito", "ПКМ", "ПКМ", "Clic destro", "PPM", "右クリック", "右键", "右鍵", "우클릭", "คลิกขวา", "Nhấp phải" };
        private static readonly string[] keyCtrlFRow = { "Ctrl+F", "Ctrl+F", "Strg+F", "Ctrl+F", "Ctrl+F", "Ctrl+F", "Ctrl+F", "Ctrl+F", "Ctrl+F", "Ctrl+F", "Ctrl+F", "Ctrl+F", "Ctrl+F", "Ctrl+F", "Ctrl+F", "Ctrl+F" };
        private static readonly string[] addNRow = { "add {0}", "{0} adet ekle", "{0} hinzufügen", "ajouter {0}", "añadir {0}", "adicionar {0}", "добавить {0}", "додати {0}", "aggiungi {0}", "dodaj {0}", "{0}個追加", "添加 {0} 个", "加入 {0} 個", "{0}개 추가", "เพิ่ม {0}", "thêm {0}" };
        private static readonly string[] addMaxRow = { "add max stack", "maks yığın ekle", "max. Stapel hinzufügen", "ajouter une pile max", "añadir pila máx.", "adicionar pilha máx.", "добавить макс. стак", "додати макс. стос", "aggiungi pila max", "dodaj maks. stos", "最大スタックを追加", "添加最大堆叠", "加入最大堆疊", "최대 스택 추가", "เพิ่มกองสูงสุด", "thêm chồng tối đa" };
        private static readonly string[] oneItemRow = { "1 item", "1 adet", "1 Stück", "1 objet", "1 unidad", "1 unidade", "1 шт.", "1 шт.", "1 pezzo", "1 szt.", "1個", "1 个", "1 個", "1개", "1 ชิ้น", "1 cái" };
        private static readonly string[] favoriteHintRow = { "favorite", "favori", "Favorit", "favori", "favorito", "favorito", "избранное", "обране", "preferito", "ulubione", "お気に入り", "收藏", "收藏", "즐겨찾기", "โปรด", "yêu thích" };
        private static readonly string[] searchHintRow = { "search", "ara", "suchen", "rechercher", "buscar", "buscar", "поиск", "пошук", "cerca", "szukaj", "検索", "搜索", "搜尋", "검색", "ค้นหา", "tìm" };
        private static readonly string[] closeHintRow = { "close", "kapat", "schließen", "fermer", "cerrar", "fechar", "закрыть", "закрити", "chiudi", "zamknij", "閉じる", "关闭", "關閉", "닫기", "ปิด", "đóng" };

        private static readonly string[] disabledNoticeRow =
        {
            "KeeperSpawner is disabled: not compatible with this game version. See the BepInEx log.",
            "KeeperSpawner devre dışı: bu oyun sürümüyle uyumsuz. Ayrıntılar BepInEx logunda.",
            "KeeperSpawner ist deaktiviert: nicht kompatibel mit dieser Spielversion. Siehe BepInEx-Log.",
            "KeeperSpawner est désactivé : incompatible avec cette version du jeu. Voir le journal BepInEx.",
            "KeeperSpawner está desactivado: no es compatible con esta versión del juego. Consulta el registro de BepInEx.",
            "KeeperSpawner está desativado: incompatível com esta versão do jogo. Veja o log do BepInEx.",
            "KeeperSpawner отключён: несовместим с этой версией игры. См. журнал BepInEx.",
            "KeeperSpawner вимкнено: несумісний з цією версією гри. Див. журнал BepInEx.",
            "KeeperSpawner è disattivato: non compatibile con questa versione del gioco. Vedi il log di BepInEx.",
            "KeeperSpawner jest wyłączony: niezgodny z tą wersją gry. Zobacz log BepInEx.",
            "KeeperSpawner は無効です: このゲームバージョンと互換性がありません。BepInEx のログを確認してください。",
            "KeeperSpawner 已禁用：与当前游戏版本不兼容。请查看 BepInEx 日志。",
            "KeeperSpawner 已停用：與目前遊戲版本不相容。請查看 BepInEx 日誌。",
            "KeeperSpawner 비활성화됨: 이 게임 버전과 호환되지 않습니다. BepInEx 로그를 확인하세요.",
            "KeeperSpawner ถูกปิดใช้งาน: ไม่รองรับเกมเวอร์ชันนี้ ดูบันทึก BepInEx",
            "KeeperSpawner đã tắt: không tương thích với phiên bản game này. Xem log BepInEx.",
        };

        public static string DisabledNotice => T(disabledNoticeRow);

        /// <summary>Dil sistemi de bozuksa kullanılır.</summary>
        public static string DisabledNoticeFallback => disabledNoticeRow[0];

        public static string Added(int added, string name) => $"+{added}  {name}  {T(addedRow)}";
        public static string AddedPartial(int added, int requested, string name) => $"+{added}/{requested}  {name}  {T(addedPartialRow)}";
        public static string InventoryFull => T(inventoryFullRow);
        public static string Error => T(errorRow);

        public static string KeyClick => T(keyClickRow);
        public static string KeyShiftClick => T(keyShiftClickRow);
        public static string KeyRightClick => T(keyRightClickRow);
        public static string KeyCtrlF => T(keyCtrlFRow);
        public static string KeyEsc => "Esc";

        public static string ClickHint(ClickAmount mode)
        {
            switch (mode)
            {
                case ClickAmount.One: return string.Format(T(addNRow), 1);
                case ClickAmount.Ten: return string.Format(T(addNRow), 10);
                default: return T(addMaxRow);
            }
        }

        public static string OneItem => T(oneItemRow);
        public static string FavoriteHint => T(favoriteHintRow);
        public static string SearchHint => T(searchHintRow);
        public static string CloseHint => T(closeHintRow);

        // ------------------------------------------------------------------
        // GK2 Mod Framework "Mods" menüsü (KeeperSpawner.GK2Framework köprüsü)
        // ------------------------------------------------------------------

        private static readonly string[] modDescriptionRow =
        {
            "Item spawner. Press {0} in game, find an item and click it to add it to your inventory.",
            "Item spawner. Oyunda {0} tuşuna bas, itemı bul ve tıklayarak envanterine ekle.",
            "Item-Spawner. Drücke im Spiel {0}, suche einen Gegenstand und klicke ihn an, um ihn deinem Inventar hinzuzufügen.",
            "Générateur d'objets. Appuyez sur {0} en jeu, trouvez un objet et cliquez dessus pour l'ajouter à votre inventaire.",
            "Generador de objetos. Pulsa {0} en el juego, busca un objeto y haz clic para añadirlo a tu inventario.",
            "Gerador de itens. Pressione {0} no jogo, encontre um item e clique nele para adicioná-lo ao inventário.",
            "Спавнер предметов. Нажмите {0} в игре, найдите предмет и щёлкните по нему, чтобы добавить его в инвентарь.",
            "Спавнер предметів. Натисніть {0} у грі, знайдіть предмет і клацніть по ньому, щоб додати його до інвентаря.",
            "Generatore di oggetti. Premi {0} in gioco, trova un oggetto e cliccalo per aggiungerlo all'inventario.",
            "Spawner przedmiotów. Naciśnij {0} w grze, znajdź przedmiot i kliknij go, aby dodać go do ekwipunku.",
            "アイテムスポナー。ゲーム中に {0} を押し、アイテムを探してクリックするとインベントリに追加されます。",
            "物品生成器。在游戏中按 {0}，找到物品并点击即可添加到背包。",
            "物品生成器。在遊戲中按 {0}，找到物品並點擊即可加入背包。",
            "아이템 스포너. 게임 중 {0} 키를 눌러 아이템을 찾고 클릭하면 인벤토리에 추가됩니다.",
            "ตัวเสกไอเท็ม กด {0} ในเกม ค้นหาไอเท็มแล้วคลิกเพื่อเพิ่มลงในกระเป๋า",
            "Công cụ tạo vật phẩm. Nhấn {0} trong game, tìm vật phẩm và nhấp để thêm vào túi đồ.",
        };
        private static readonly string[] toggleKeyNameRow = { "Open / close key", "Açma / kapama tuşu", "Taste zum Öffnen / Schließen", "Touche d'ouverture / fermeture", "Tecla para abrir / cerrar", "Tecla para abrir / fechar", "Клавиша открытия / закрытия", "Клавіша відкриття / закриття", "Tasto apri / chiudi", "Klawisz otwierania / zamykania", "開く / 閉じるキー", "打开 / 关闭按键", "開啟 / 關閉按鍵", "열기 / 닫기 키", "ปุ่มเปิด / ปิด", "Phím mở / đóng" };
        private static readonly string[] toggleKeyDescRow = { "Opens the spawner window while you are in game.", "Oyundayken spawner penceresini açar.", "Öffnet das Spawner-Fenster im Spiel.", "Ouvre la fenêtre du générateur en jeu.", "Abre la ventana del generador durante la partida.", "Abre a janela do gerador durante o jogo.", "Открывает окно спавнера в игре.", "Відкриває вікно спавнера у грі.", "Apre la finestra del generatore durante il gioco.", "Otwiera okno spawnera w grze.", "ゲーム中にスポナーウィンドウを開きます。", "在游戏中打开生成器窗口。", "在遊戲中開啟生成器視窗。", "게임 중 스포너 창을 엽니다.", "เปิดหน้าต่างตัวเสกระหว่างเล่นเกม", "Mở cửa sổ tạo vật phẩm khi đang chơi." };
        private static readonly string[] showQuestDescRow = { "Spawning quest items can break quests.", "Görev itemları eklemek görevleri bozabilir.", "Questgegenstände zu erzeugen kann Quests kaputt machen.", "Générer des objets de quête peut bloquer des quêtes.", "Generar objetos de misión puede romper misiones.", "Gerar itens de missão pode quebrar missões.", "Квестовые предметы могут сломать квесты.", "Квестові предмети можуть зламати квести.", "Generare oggetti delle missioni può rompere le missioni.", "Przedmioty z zadań mogą zepsuć zadania.", "クエストアイテムを追加するとクエストが進行不能になる場合があります。", "生成任务物品可能会破坏任务。", "生成任務物品可能會破壞任務。", "퀘스트 아이템을 추가하면 퀘스트가 망가질 수 있습니다.", "การเสกไอเท็มเควสต์อาจทำให้เควสต์พัง", "Tạo vật phẩm nhiệm vụ có thể làm hỏng nhiệm vụ." };
        private static readonly string[] clickAmountNameRow = { "Amount per click", "Tık başına miktar", "Menge pro Klick", "Quantité par clic", "Cantidad por clic", "Quantidade por clique", "Количество за клик", "Кількість за клік", "Quantità per clic", "Ilość na kliknięcie", "クリックごとの数量", "每次点击数量", "每次點擊數量", "클릭당 수량", "จำนวนต่อคลิก", "Số lượng mỗi lần nhấp" };
        private static readonly string[] clickAmountDescRow = { "One = 1, Ten = 10, Max = a full stack. Shift+Click always adds 1.", "One = 1, Ten = 10, Max = tam yığın. Shift+Tık her zaman 1 ekler.", "One = 1, Ten = 10, Max = voller Stapel. Umschalt+Klick fügt immer 1 hinzu.", "One = 1, Ten = 10, Max = une pile complète. Maj+Clic ajoute toujours 1.", "One = 1, Ten = 10, Max = una pila completa. Mayús+Clic siempre añade 1.", "One = 1, Ten = 10, Max = uma pilha completa. Shift+Clique sempre adiciona 1.", "One = 1, Ten = 10, Max = полный стак. Shift+Клик всегда добавляет 1.", "One = 1, Ten = 10, Max = повний стос. Shift+Клік завжди додає 1.", "One = 1, Ten = 10, Max = una pila intera. Maiusc+Clic aggiunge sempre 1.", "One = 1, Ten = 10, Max = pełny stos. Shift+Klik zawsze dodaje 1.", "One = 1、Ten = 10、Max = 最大スタック。Shift+クリックは常に1個追加します。", "One = 1，Ten = 10，Max = 满堆叠。Shift+点击始终添加 1 个。", "One = 1，Ten = 10，Max = 滿堆疊。Shift+點擊始終加入 1 個。", "One = 1, Ten = 10, Max = 최대 스택. Shift+클릭은 항상 1개를 추가합니다.", "One = 1, Ten = 10, Max = เต็มกอง Shift+คลิกจะเพิ่ม 1 ชิ้นเสมอ", "One = 1, Ten = 10, Max = đầy chồng. Shift+Nhấp luôn thêm 1." };
        private static readonly string[] viewNameRow = { "Item view", "Item görünümü", "Ansicht", "Affichage des objets", "Vista de objetos", "Visualização de itens", "Вид списка", "Вигляд списку", "Vista oggetti", "Widok przedmiotów", "表示形式", "物品视图", "物品檢視", "아이템 보기", "มุมมองไอเท็ม", "Kiểu hiển thị" };
        private static readonly string[] viewDescRow = { "Grid with icons or a list.", "İkonlu ızgara ya da liste.", "Raster mit Symbolen oder Liste.", "Grille avec icônes ou liste.", "Cuadrícula con iconos o lista.", "Grade com ícones ou lista.", "Сетка с иконками или список.", "Сітка з іконками або список.", "Griglia con icone o elenco.", "Siatka z ikonami lub lista.", "アイコンのグリッドまたはリスト。", "图标网格或列表。", "圖示網格或清單。", "아이콘 격자 또는 목록.", "ตารางไอคอนหรือรายการ", "Lưới biểu tượng hoặc danh sách." };
        private static readonly string[] scaleNameRow = { "UI scale", "Arayüz ölçeği", "UI-Skalierung", "Échelle de l'interface", "Escala de la interfaz", "Escala da interface", "Масштаб интерфейса", "Масштаб інтерфейсу", "Scala interfaccia", "Skala interfejsu", "UIの大きさ", "界面缩放", "介面縮放", "UI 크기", "ขนาด UI", "Tỉ lệ giao diện" };
        private static readonly string[] scaleDescRow = { "0 = automatic (screen height / 1080).", "0 = otomatik (ekran yüksekliği / 1080).", "0 = automatisch (Bildschirmhöhe / 1080).", "0 = automatique (hauteur d'écran / 1080).", "0 = automático (altura de pantalla / 1080).", "0 = automático (altura da tela / 1080).", "0 = автоматически (высота экрана / 1080).", "0 = автоматично (висота екрана / 1080).", "0 = automatico (altezza schermo / 1080).", "0 = automatycznie (wysokość ekranu / 1080).", "0 = 自動（画面の高さ / 1080）。", "0 = 自动（屏幕高度 / 1080）。", "0 = 自動（螢幕高度 / 1080）。", "0 = 자동 (화면 높이 / 1080).", "0 = อัตโนมัติ (ความสูงหน้าจอ / 1080)", "0 = tự động (chiều cao màn hình / 1080)." };
        private static readonly string[] backdropNameRow = { "Background dimming", "Arka plan karartma", "Hintergrund abdunkeln", "Assombrissement du fond", "Oscurecer el fondo", "Escurecer o fundo", "Затемнение фона", "Затемнення фону", "Oscuramento sfondo", "Przyciemnienie tła", "背景の暗さ", "背景变暗", "背景變暗", "배경 어둡게", "ความมืดของพื้นหลัง", "Làm tối nền" };
        private static readonly string[] backdropDescRow = { "How much the game is dimmed behind the window.", "Pencere açıkken oyunun ne kadar karartılacağı.", "Wie stark das Spiel hinter dem Fenster abgedunkelt wird.", "À quel point le jeu est assombri derrière la fenêtre.", "Cuánto se oscurece el juego detrás de la ventana.", "Quanto o jogo escurece atrás da janela.", "Насколько затемняется игра за окном.", "Наскільки затемнюється гра за вікном.", "Quanto si oscura il gioco dietro la finestra.", "Jak bardzo gra jest przyciemniona za oknem.", "ウィンドウの後ろのゲーム画面を暗くする度合い。", "窗口后方游戏画面的变暗程度。", "視窗後方遊戲畫面的變暗程度。", "창 뒤의 게임 화면을 어둡게 하는 정도.", "ระดับความมืดของเกมด้านหลังหน้าต่าง", "Mức làm tối game phía sau cửa sổ." };
        private static readonly string[] statusNameRow = { "Status", "Durum", "Status", "État", "Estado", "Status", "Состояние", "Стан", "Stato", "Stan", "状態", "状态", "狀態", "상태", "สถานะ", "Trạng thái" };
        private static readonly string[] statusDescRow = { "Problems are written to BepInEx\\LogOutput.log.", "Sorunlar BepInEx\\LogOutput.log dosyasına yazılır.", "Probleme werden in BepInEx\\LogOutput.log geschrieben.", "Les problèmes sont écrits dans BepInEx\\LogOutput.log.", "Los problemas se escriben en BepInEx\\LogOutput.log.", "Os problemas são gravados em BepInEx\\LogOutput.log.", "Проблемы записываются в BepInEx\\LogOutput.log.", "Проблеми записуються в BepInEx\\LogOutput.log.", "I problemi vengono scritti in BepInEx\\LogOutput.log.", "Problemy są zapisywane w BepInEx\\LogOutput.log.", "問題は BepInEx\\LogOutput.log に記録されます。", "问题会写入 BepInEx\\LogOutput.log。", "問題會寫入 BepInEx\\LogOutput.log。", "문제는 BepInEx\\LogOutput.log에 기록됩니다.", "ปัญหาจะถูกบันทึกใน BepInEx\\LogOutput.log", "Lỗi được ghi vào BepInEx\\LogOutput.log." };
        private static readonly string[] statusActiveRow = { "Active, press {0} in game", "Etkin, oyunda {0} tuşuna bas", "Aktiv, im Spiel {0} drücken", "Actif, appuyez sur {0} en jeu", "Activo, pulsa {0} en el juego", "Ativo, pressione {0} no jogo", "Активен, нажмите {0} в игре", "Активний, натисніть {0} у грі", "Attivo, premi {0} in gioco", "Aktywny, naciśnij {0} w grze", "有効、ゲーム中に {0} を押す", "已启用，在游戏中按 {0}", "已啟用，在遊戲中按 {0}", "활성, 게임 중 {0} 키", "ทำงานอยู่ กด {0} ในเกม", "Đang bật, nhấn {0} trong game" };
        private static readonly string[] statusDisabledRow = { "Disabled, see the log", "Devre dışı, loga bak", "Deaktiviert, siehe Log", "Désactivé, voir le journal", "Desactivado, consulta el registro", "Desativado, veja o log", "Отключён, см. журнал", "Вимкнено, див. журнал", "Disattivato, vedi il log", "Wyłączony, zobacz log", "無効、ログを確認", "已禁用，请查看日志", "已停用，請查看日誌", "비활성, 로그 확인", "ปิดใช้งาน ดูบันทึก", "Đã tắt, xem log" };

        public static string ModDescription(string key) => string.Format(T(modDescriptionRow), key);
        public static string ToggleKeyName => T(toggleKeyNameRow);
        public static string ToggleKeyDescription => T(toggleKeyDescRow);
        public static string ShowQuestItemsDescription => T(showQuestDescRow);
        public static string ClickAmountName => T(clickAmountNameRow);
        public static string ClickAmountDescription => T(clickAmountDescRow);
        public static string ViewName => T(viewNameRow);
        public static string ViewDescription => T(viewDescRow);
        public static string ScaleName => T(scaleNameRow);
        public static string ScaleDescription => T(scaleDescRow);
        public static string BackdropName => T(backdropNameRow);
        public static string BackdropDescription => T(backdropDescRow);
        public static string StatusName => T(statusNameRow);
        public static string StatusDescription => T(statusDescRow);
        public static string StatusActive(string key) => string.Format(T(statusActiveRow), key);
        public static string StatusDisabled => T(statusDisabledRow);

        // ------------------------------------------------------------------
        // Kategoriler
        // ------------------------------------------------------------------

        private static readonly string[] catBuildingRow = { "Building", "İnşaat", "Bau", "Construction", "Construcción", "Construção", "Строительство", "Будівництво", "Costruzione", "Budowa", "建築", "建筑", "建築", "건설", "ก่อสร้าง", "Xây dựng" };
        private static readonly string[] catFoodRow = { "Food", "Yiyecek", "Nahrung", "Nourriture", "Comida", "Comida", "Еда", "Їжа", "Cibo", "Jedzenie", "食べ物", "食物", "食物", "음식", "อาหาร", "Thức ăn" };
        private static readonly string[] catFarmingRow = { "Farming", "Tarım", "Landwirtschaft", "Agriculture", "Cultivo", "Cultivo", "Фермерство", "Фермерство", "Agricoltura", "Rolnictwo", "農業", "农业", "農業", "농사", "การเกษตร", "Trồng trọt" };
        private static readonly string[] catAlchemyRow = { "Alchemy", "Simya", "Alchemie", "Alchimie", "Alquimia", "Alquimia", "Алхимия", "Алхімія", "Alchimia", "Alchemia", "錬金術", "炼金", "煉金", "연금술", "เล่นแร่แปรธาตุ", "Giả kim" };
        private static readonly string[] catPotionsRow = { "Potions", "İksirler", "Tränke", "Potions", "Pociones", "Poções", "Зелья", "Зілля", "Pozioni", "Mikstury", "ポーション", "药水", "藥水", "물약", "ยา", "Thuốc" };
        private static readonly string[] catToolsRow = { "Tools", "Aletler", "Werkzeuge", "Outils", "Herramientas", "Ferramentas", "Инструменты", "Інструменти", "Attrezzi", "Narzędzia", "道具", "工具", "工具", "도구", "เครื่องมือ", "Công cụ" };
        private static readonly string[] catEquipmentRow = { "Equipment", "Ekipman", "Ausrüstung", "Équipement", "Equipo", "Equipamento", "Снаряжение", "Спорядження", "Equipaggiamento", "Wyposażenie", "装備", "装备", "裝備", "장비", "อุปกรณ์", "Trang bị" };
        private static readonly string[] catFishingRow = { "Fishing", "Balıkçılık", "Angeln", "Pêche", "Pesca", "Pesca", "Рыбалка", "Риболовля", "Pesca", "Wędkarstwo", "釣り", "钓鱼", "釣魚", "낚시", "ตกปลา", "Câu cá" };
        private static readonly string[] catBodyPartsRow = { "Body parts", "Organlar", "Körperteile", "Organes", "Órganos", "Órgãos", "Органы", "Органи", "Organi", "Narządy", "臓器", "器官", "器官", "장기", "อวัยวะ", "Nội tạng" };
        private static readonly string[] catGravesRow = { "Graves", "Mezar", "Gräber", "Tombes", "Tumbas", "Túmulos", "Могилы", "Могили", "Tombe", "Groby", "墓", "坟墓", "墳墓", "무덤", "หลุมศพ", "Mộ" };
        private static readonly string[] catChurchRow = { "Church", "Kilise", "Kirche", "Église", "Iglesia", "Igreja", "Церковь", "Церква", "Chiesa", "Kościół", "教会", "教堂", "教堂", "교회", "โบสถ์", "Nhà thờ" };
        private static readonly string[] catPapersRow = { "Papers", "Kâğıt", "Papier", "Papiers", "Papeles", "Papéis", "Бумаги", "Папери", "Carte", "Papiery", "紙", "纸张", "紙張", "종이", "กระดาษ", "Giấy" };
        private static readonly string[] catBagsRow = { "Bags", "Çantalar", "Taschen", "Sacs", "Bolsas", "Bolsas", "Сумки", "Сумки", "Borse", "Torby", "バッグ", "包", "包", "가방", "กระเป๋า", "Túi" };
        private static readonly string[] catQuestRow = { "Quest", "Görev", "Quest", "Quête", "Misión", "Missão", "Квест", "Квест", "Missione", "Zadanie", "クエスト", "任务", "任務", "퀘스트", "เควสต์", "Nhiệm vụ" };
        private static readonly string[] catOtherRow = { "Other", "Diğer", "Sonstiges", "Autre", "Otros", "Outros", "Прочее", "Інше", "Altro", "Inne", "その他", "其他", "其他", "기타", "อื่นๆ", "Khác" };

        public static string Category(ItemCategory category)
        {
            switch (category)
            {
                case ItemCategory.Building: return T(catBuildingRow);
                case ItemCategory.Food: return T(catFoodRow);
                case ItemCategory.Farming: return T(catFarmingRow);
                case ItemCategory.Alchemy: return T(catAlchemyRow);
                case ItemCategory.Potions: return T(catPotionsRow);
                case ItemCategory.Tools: return T(catToolsRow);
                case ItemCategory.Equipment: return T(catEquipmentRow);
                case ItemCategory.Fishing: return T(catFishingRow);
                case ItemCategory.BodyParts: return T(catBodyPartsRow);
                case ItemCategory.Graves: return T(catGravesRow);
                case ItemCategory.Church: return T(catChurchRow);
                case ItemCategory.Papers: return T(catPapersRow);
                case ItemCategory.Bags: return T(catBagsRow);
                case ItemCategory.Quest: return T(catQuestRow);
                default: return T(catOtherRow);
            }
        }
    }
}
