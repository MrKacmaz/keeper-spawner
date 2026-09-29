using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LazyBearTechnology;
using UnityEngine;

namespace KeeperSpawner
{
    /// <summary>
    /// Menü açıkken oyunun girdisini (hareket, tıklama, kısayollar) askıya alır.
    /// Oyunun kendi UIBugReportWindow'u da metin kutusu odaktayken aynı yolu izliyor.
    /// </summary>
    internal static class InputBlocker
    {
        private static readonly FieldInfo InstanceField = AccessTools.Field(typeof(LazyInput), "instance");
        private static readonly FieldInfo HoldedKeysField = AccessTools.Field(typeof(LazyInput), "holdedKeys");
        private static readonly FieldInfo PressedKeysField = AccessTools.Field(typeof(LazyInput), "pressedKeys");
        private static readonly FieldInfo DirectionField = AccessTools.Field(typeof(LazyInput), "direction");
        private static readonly FieldInfo Direction2Field = AccessTools.Field(typeof(LazyInput), "direction2");

        private static bool suspended;
        private static bool wasActive;

        public static void Suspend()
        {
            if (suspended)
            {
                return;
            }
            wasActive = LazyInput.IsInputActive();
            LazyInput.SetInputActivity(false);
            suspended = true;
            // LazyInput.Update girdi kapalıyken erken döner ve son karenin durumunu korur.
            // Temizlemezsek menü açılırken yürüyen karakter yürümeye devam eder.
            ClearFrozenState();
        }

        public static void Restore()
        {
            if (!suspended)
            {
                return;
            }
            LazyInput.SetInputActivity(wasActive);
            suspended = false;
        }

        private static void ClearFrozenState()
        {
            try
            {
                var instance = InstanceField?.GetValue(null);
                if (instance == null)
                {
                    return;
                }
                (HoldedKeysField?.GetValue(instance) as List<GameKey>)?.Clear();
                (PressedKeysField?.GetValue(instance) as List<GameKey>)?.Clear();
                DirectionField?.SetValue(instance, Vector2.zero);
                Direction2Field?.SetValue(instance, Vector2.zero);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"Girdi durumu temizlenemedi: {e.Message}");
            }
        }
    }
}
