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

        private static int Index
        {
            get
            {
                string lang = LLBase.CurrentLang;
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

        /// <summary>Tüm satırların dil sayısı kadar eleman içerdiğini kontrol eder (açılışta bir kez).</summary>
        public static void Validate()
        {
            foreach (var field in typeof(Strings).GetFields(BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (field.FieldType == typeof(string[]) && field.Name != nameof(LanguageIds)
                    && field.GetValue(null) is string[] row && row.Length != LanguageIds.Length)
                {
                    Plugin.Log.LogWarning($"Çeviri satırı eksik: {field.Name} ({row.Length}/{LanguageIds.Length})");
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
