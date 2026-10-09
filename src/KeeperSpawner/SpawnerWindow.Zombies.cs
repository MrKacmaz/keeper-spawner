using System;
using UnityEngine;

namespace KeeperSpawner
{
    internal sealed partial class SpawnerWindow
    {
        private ZombiePreset[] zombiePresets;
        private string zombiePresetError;
        private int selectedZombie;
        private Vector2 zombieInfoScroll;

        private void EnsureZombiePresets()
        {
            if (zombiePresets != null || zombiePresetError != null) return;
            try
            {
                zombiePresets = new[] { ZombieSpawner.Build(true), ZombieSpawner.Build(false) };
            }
            catch (Exception e) when (!GameCompat.IsCompatibilityError(e))
            {
                zombiePresetError = e.Message;
                Plugin.Log.LogError($"Could not prepare zombie presets: {e}");
            }
        }

        private void DrawZombies(Rect r)
        {
            EnsureZombiePresets();
            KsTheme.Draw(KsTheme.ContentPanel, r);
            KsTheme.Strip(new Rect(r.x + 2f, r.y + 2f, r.width - 4f, StripH), Strings.Zombies);
            var hint = new Rect(r.x + 14f, r.y + StripH + 12f, r.width - 28f, 64f);
            GUI.Label(hint, Strings.ZombieHint, NameWrapStyle());
            if (zombiePresets == null)
            {
                GUI.Label(new Rect(hint.x, hint.yMax + 12f, hint.width, 80f), Strings.Error, NameWrapStyle());
                return;
            }
            for (int i = 0; i < zombiePresets.Length; i++)
            {
                var preset = zombiePresets[i];
                var card = new Rect(r.x + 14f, hint.yMax + 12f + i * 132f, r.width - 28f, 120f);
                if (card.Contains(Event.current.mousePosition) && selectedZombie != i)
                {
                    selectedZombie = i;
                    zombieInfoScroll = Vector2.zero;
                }
                if (GUI.Button(card, GUIContent.none, selectedZombie == i ? KsTheme.ButtonOn : KsTheme.Button))
                {
                    selectedZombie = i;
                    zombieInfoScroll = Vector2.zero;
                    GUIUtility.keyboardControl = 0;
                    try
                    {
                        ZombieSpawner.Spawn(preset);
                        ShowToast(Strings.ZombieSpawned, KsTheme.Success);
                    }
                    catch (Exception e) when (!GameCompat.IsCompatibilityError(e))
                    {
                        Plugin.Log.LogError($"Zombie spawn failed: {e}");
                        ShowToast(Strings.Error, KsTheme.ItemName);
                    }
                }
                if (KsTheme.Repaint && IconCache.TryGet("i_body_zombie", out var icon))
                    IconCache.Draw(new Rect(card.x + 12f, card.y + 24f, 56f, 56f), icon);
                var text = new Rect(card.x + 82f, card.y + 12f, card.width - 96f, 50f);
                GUI.Label(text, i == 0 ? Strings.WhiteZombie : Strings.RedZombie, NameWrapStyle());
                KsTheme.Label(new Rect(text.x, card.y + 68f, text.width, 24f),
                    Strings.ZombieSkulls(preset.White, preset.Red), 14, i == 0 ? KsTheme.TextBright : KsTheme.ItemName);
            }
        }

        private void DrawZombieInfo(Rect r)
        {
            EnsureZombiePresets();
            KsTheme.Draw(KsTheme.Panel, r);
            KsTheme.Strip(new Rect(r.x + 2f, r.y + 2f, r.width - 4f, StripH), Strings.ItemInfo);
            if (zombiePresets == null) return;
            var preset = zombiePresets[selectedZombie];
            var viewport = new Rect(r.x + 12f, r.y + StripH + 12f, r.width - 24f, r.height - StripH - 24f);
            float width = viewport.width - 16f;
            var style = NameWrapStyle();
            string title = selectedZombie == 0 ? Strings.WhiteZombie : Strings.RedZombie;
            string contents = title + "\n\n" + Strings.ZombieSkulls(preset.White, preset.Red) + "\n\n";
            foreach (var def in ZombieSpawner.Contents(preset))
                contents += def.GetHeader() + "\n" + Strings.ZombieSkulls(def.whiteSkulls, def.redSkulls) + "\n\n";
            contents += preset.Collar.GetHeader();
            // Game headers may contain TMP sprite tags; use plain text in IMGUI.
            contents = System.Text.RegularExpressions.Regex.Replace(contents, "<[^>]+>", string.Empty);
            float height = style.CalcHeight(new GUIContent(contents), width);
            zombieInfoScroll = GUI.BeginScrollView(viewport, zombieInfoScroll, new Rect(0f, 0f, width, height),
                false, false, GUIStyle.none, KsTheme.Skin.verticalScrollbar);
            GUI.Label(new Rect(0f, 0f, width, height), contents, style);
            GUI.EndScrollView();
        }
    }
}
