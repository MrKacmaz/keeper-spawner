using System;
using BepInEx;
using GK2.Framework;

namespace KeeperSpawner.GK2Framework
{
    /// <summary>
    /// KeeperSpawner'ı GK2 Mod Framework'ün "Mods" menüsünde gösterir ve ayarlarını oradan düzenletir.
    /// Ayarlar KeeperSpawner'ın kendi config girdileridir (Framework 0.1.19 girdi benimseme); kopya bağlanmaz.
    /// Framework kurulu değilse BepInEx bu eklentiyi atlar, KeeperSpawner tek başına çalışır.
    /// </summary>
    [BepInPlugin(Guid, Name, Plugin.Version)]
    [BepInDependency(Plugin.Guid, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(FrameworkPlugin.PluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public sealed class FrameworkBridgePlugin : BaseUnityPlugin
    {
        public const string Guid = "com.mrkacmaz.keeperspawner.gk2framework";
        public const string Name = "KeeperSpawner - GK2 Framework Integration";

        private void Awake()
        {
            Plugin main = Plugin.Instance;
            if (main == null)
            {
                Logger.LogError("KeeperSpawner is not loaded, skipping the Mods menu registration.");
                return;
            }
            try
            {
                FrameworkApi.RegisterMod(new Bridge(main), main.Config);
            }
            catch (Exception e)
            {
                // Framework API'si önizleme (0.1.x); değişirse KeeperSpawner'ı etkilemeden sadece menü kaydı düşer
                Logger.LogError($"GK2 Mod Framework registration failed: {e}");
            }
        }

        private sealed class Bridge : Gk2ModBase
        {
            private readonly Plugin main;
            private readonly Gk2ModMetadata metadata;

            internal Bridge(Plugin main)
            {
                this.main = main;
                string lang = FrameworkLocalization.CurrentLanguage;
                metadata = new Gk2ModMetadata(
                    Plugin.Guid,
                    Plugin.Name,
                    "MrKacmaz",
                    Plugin.Version,
                    Strings.In(lang, () => Strings.ModDescription(main.ToggleKey.Value.ToString())),
                    supportsRuntimeToggle: false,
                    // Uyumluluğu KeeperSpawner kendisi denetler (GameCompat); Framework'ün build listesine bağlı kalmasın
                    requiresKnownBuild: false,
                    frameworkManagesEnabledState: false);
            }

            public override Gk2ModMetadata Metadata => metadata;

            public override void OnRegister(Gk2ModContext context)
            {
                // Adlar kayıt anında bir kez çözülür; oyun dili değişirse sonraki açılışta güncellenir
                string lang = FrameworkLocalization.CurrentLanguage;
                string L(Func<string> text) => Strings.In(lang, text);

                Gk2Settings settings = context.Settings;
                settings.AddReadOnly("General", "Status", L(() => Strings.StatusName), L(() => Strings.StatusDescription),
                    () => main.IsDisabled ? Strings.StatusDisabled : Strings.StatusActive(main.ToggleKey.Value.ToString()), order: 0);
                settings.AddKeybind(main.ToggleKey, L(() => Strings.ToggleKeyName), L(() => Strings.ToggleKeyDescription), order: 1);
                settings.AddEnum(main.ClickAmount, L(() => Strings.ClickAmountName), L(() => Strings.ClickAmountDescription), order: 2);
                settings.AddToggle(main.ShowQuestItems, L(() => Strings.ShowQuestItems), L(() => Strings.ShowQuestItemsDescription), order: 3);
                settings.AddEnum(main.View, L(() => Strings.ViewName), L(() => Strings.ViewDescription), order: 4);
                settings.AddFloatSlider(main.UiScale, 0f, 3f, L(() => Strings.ScaleName), L(() => Strings.ScaleDescription), step: 0.05f, order: 5);
                settings.AddFloatSlider(main.BackdropOpacity, 0f, 1f, L(() => Strings.BackdropName), L(() => Strings.BackdropDescription), step: 0.05f, order: 6);
            }
        }
    }
}
