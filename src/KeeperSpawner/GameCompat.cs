using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LazyBearTechnology;

namespace KeeperSpawner
{
    /// <summary>
    /// Açılışta modun kullandığı oyun API'lerinin hâlâ yerinde olduğunu reflection ile doğrular.
    /// Bir oyun güncellemesi metot/alan kaldırırsa .NET o çağrıyı içeren metodu derlerken
    /// MissingMethodException/TypeLoadException fırlatır; bunu önceden yakalayıp mod çökmeden kapanır.
    /// Zorunlu API eksikse mod devre dışı kalır; isteğe bağlı olan eksikse sadece o özellik kapanır.
    /// </summary>
    internal static class GameCompat
    {
        /// <summary>Modun test edildiği oyun sürümü (ana menüde görünen).</summary>
        public const string TestedGameVersion = "1.007.1";

        /// <summary>Oyunun kendi "+N item" bildirimi (UINotificator).</summary>
        public static bool GameNotifications { get; private set; } = true;

        /// <summary>Item ikonları (EasySpritesCollection).</summary>
        public static bool Icons { get; private set; } = true;

        private static readonly List<string> MissingRequired = new List<string>();
        private static readonly List<string> MissingOptional = new List<string>();

        public static string MissingRequiredSummary => string.Join(", ", MissingRequired.ToArray());

        /// <summary>Zorunlu API'lerin hepsi varsa true.</summary>
        public static bool Check()
        {
            MissingRequired.Clear();
            MissingOptional.Clear();

            // --- Oyun durumu ---
            Required("MainGame.Instance", () => StaticProperty(typeof(MainGame), "Instance"));
            Required("MainGame.PlayerData", () => StaticProperty(typeof(MainGame), "PlayerData"));
            Required("MainGame.gameState", () => AccessTools.Property(typeof(MainGame), "gameState") != null);
            Required("MainGame.GameState.InGame", () =>
            {
                var stateEnum = AccessTools.Inner(typeof(MainGame), "GameState");
                return stateEnum != null && stateEnum.IsEnum && Array.IndexOf(Enum.GetNames(stateEnum), "InGame") >= 0;
            });

            // --- Envanter ---
            Required("PlayerData.inventory", () => FieldOf(typeof(PlayerData), "inventory", typeof(Inventory)));
            Required("Inventory.AddItemToInventory(Item, Item, bool)", () =>
                MethodReturns(typeof(Inventory), "AddItemToInventory", typeof(bool), typeof(Item), typeof(Item), typeof(bool)));
            Required("Inventory.Data", () => AccessTools.Property(typeof(Inventory), "Data")?.PropertyType == typeof(Item));
            Required("Item.GetTotalCountInInventory(string, Item, bool)", () =>
                MethodReturns(typeof(Item), "GetTotalCountInInventory", typeof(int), typeof(string), typeof(Item), typeof(bool)));
            Required("Item(string, int)", () => AccessTools.Constructor(typeof(Item), new[] { typeof(string), typeof(int) }) != null);

            // --- Item veritabanı ---
            Required("GameBalance.Me", () => StaticProperty(typeof(GameBalance), "Me"));
            Required("GameBalance.itemDefs", () => FieldOf(typeof(GameBalance), "itemDefs", typeof(List<ItemDef>)));
            Required("GameBalanceBase.GetDataOrNull<T>", () => Array.Exists(typeof(GameBalanceBase).GetMethods(),
                m => m.Name == "GetDataOrNull" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1));
            Required("ItemDef.id", () => FieldOf(typeof(ItemDef), "id", typeof(string)));
            Required("ItemDef.stackCount", () => FieldOf(typeof(ItemDef), "stackCount", typeof(int)));
            Required("ItemDef.itemGroupIds", () => FieldOf(typeof(ItemDef), "itemGroupIds", typeof(List<string>)));
            Required("ItemDef.type", () => FieldOf(typeof(ItemDef), "type", typeof(ItemType)));
            Required("ItemDef.isQuestItem", () => FieldOf(typeof(ItemDef), "isQuestItem", typeof(bool)));
            Required("ItemDef.isBag", () => FieldOf(typeof(ItemDef), "isBag", typeof(bool)));
            Required("ItemDef.iconId", () => FieldOf(typeof(ItemDef), "iconId", typeof(string)));
            Required("ItemDef.qualityType", () => AccessTools.Field(typeof(ItemDef), "qualityType") != null);
            Required("ItemDef.quality", () => FieldOf(typeof(ItemDef), "quality", typeof(int)));
            Required("ItemDef.GetHeader()", () => MethodReturns(typeof(ItemDef), "GetHeader", typeof(string)));

            // --- Dil ve girdi ---
            Required("LLBase.CurrentLang", () => StaticProperty(typeof(LLBase), "CurrentLang"));
            Required("LazyInput.SetInputActivity(bool)", () => MethodReturns(typeof(LazyInput), "SetInputActivity", typeof(void), typeof(bool)));
            Required("LazyInput.IsInputActive()", () => MethodReturns(typeof(LazyInput), "IsInputActive", typeof(bool)));

            // --- İsteğe bağlı ---
            // Menü açılırken basılı kalan tuşların temizlenmesi (yoksa yürürken açınca karakter yürüyebilir)
            foreach (var field in new[] { "instance", "holdedKeys", "pressedKeys", "direction", "direction2" })
            {
                Optional("LazyInput." + field, () => AccessTools.Field(typeof(LazyInput), field) != null);
            }
            GameNotifications = Optional("UINotificator.ShowSimpleTextWithIconNotification(string, string)", () =>
                MethodReturns(typeof(UINotificator), "ShowSimpleTextWithIconNotification", typeof(void), typeof(string), typeof(string))
                && StaticProperty(typeof(LazySingleton<UINotificator>), "Instance"));
            Icons = Optional("EasySpritesCollection.HasSprite / GetSprite", () =>
                MethodReturns(typeof(EasySpritesCollection), "HasSprite", typeof(bool), typeof(string))
                && AccessTools.Method(typeof(EasySpritesCollection), "GetSprite", new[] { typeof(string), typeof(string) }) != null
                && StaticProperty(typeof(LazySingletonSO<EasySpritesCollection>), "Instance"));

            if (MissingOptional.Count > 0)
            {
                Plugin.Log.LogWarning($"Some game APIs are missing, related features are off: {string.Join(", ", MissingOptional.ToArray())}");
            }
            if (MissingRequired.Count > 0)
            {
                Plugin.Log.LogError($"Required game APIs are missing: {MissingRequiredSummary}");
            }
            return MissingRequired.Count == 0;
        }

        /// <summary>Loga yazmak için çalışan oyun sürümü; okunamazsa "?".</summary>
        public static string RunningGameVersion()
        {
            try
            {
                var info = LazySingletonSO<GameInfo>.Instance;
                return info != null ? info.Version : "?";
            }
            catch (Exception)
            {
                return "?";
            }
        }

        /// <summary>Oyun API'si eksik olduğunda .NET'in fırlattığı hata türleri.</summary>
        public static bool IsCompatibilityError(Exception e)
        {
            for (var current = e; current != null; current = current.InnerException)
            {
                if (current is MissingMemberException || current is TypeLoadException || current is EntryPointNotFoundException)
                {
                    return true;
                }
            }
            return false;
        }

        private static void Required(string name, Func<bool> check)
        {
            if (!Safe(check))
            {
                MissingRequired.Add(name);
            }
        }

        private static bool Optional(string name, Func<bool> check)
        {
            bool ok = Safe(check);
            if (!ok)
            {
                MissingOptional.Add(name);
            }
            return ok;
        }

        /// <summary>Bir kontrol kendisi hata fırlatırsa (ör. AmbiguousMatchException, TypeLoadException) eksik say.</summary>
        private static bool Safe(Func<bool> check)
        {
            try
            {
                return check();
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool StaticProperty(Type type, string name)
        {
            var getter = AccessTools.Property(type, name)?.GetGetMethod(true);
            return getter != null && getter.IsStatic;
        }

        private static bool FieldOf(Type type, string name, Type fieldType)
        {
            return AccessTools.Field(type, name)?.FieldType == fieldType;
        }

        private static bool MethodReturns(Type type, string name, Type returnType, params Type[] parameters)
        {
            MethodInfo method = AccessTools.Method(type, name, parameters);
            return method != null && method.ReturnType == returnType;
        }
    }
}
