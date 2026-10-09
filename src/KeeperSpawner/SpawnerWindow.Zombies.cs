using System;
using UnityEngine;

namespace KeeperSpawner
{
    internal sealed partial class SpawnerWindow
    {
        private ZombiePreset[] zombiePresets;
        private string zombiePresetError;
        private ZombiePreset[] corpsePresets;
        private string corpsePresetError;
        private bool IsCorpseTab => selectedTab.Kind == TabKind.Corpses;
        private bool IsBodyTab => IsCorpseTab || selectedTab.Kind == TabKind.Zombies;
        private ZombiePreset[] BodyPresets => IsCorpseTab ? corpsePresets : zombiePresets;
        private string BodyHint => IsCorpseTab ? Strings.CorpseHint : Strings.ZombieHint;
        private string BodyTitle(int index) => IsCorpseTab
            ? (index == 0 ? Strings.WhiteCorpse : Strings.RedCorpse)
            : (index == 0 ? Strings.WhiteZombie : Strings.RedZombie);
        private int selectedZombie;
        private Vector2 zombieInfoScroll;

        private void EnsureZombiePresets()
        {
            if (BodyPresets != null || (IsCorpseTab ? corpsePresetError : zombiePresetError) != null) return;
            try
            {
                var presets = new[] { ZombieSpawner.Build(true, IsCorpseTab), ZombieSpawner.Build(false, IsCorpseTab) };
                if (IsCorpseTab) corpsePresets = presets;
                else zombiePresets = presets;
            }
            catch (Exception e) when (!GameCompat.IsCompatibilityError(e))
            {
                if (IsCorpseTab) corpsePresetError = e.Message;
                else zombiePresetError = e.Message;
                Plugin.Log.LogError($"Could not prepare body presets (corpse={IsCorpseTab}): {e}");
            }
        }

        private void DrawZombies(Rect r)
        {
            EnsureZombiePresets();
            KsTheme.Draw(KsTheme.ContentPanel, r);
            KsTheme.Strip(new Rect(r.x + 2f, r.y + 2f, r.width - 4f, StripH), IsCorpseTab ? Strings.Corpses : Strings.Zombies);
            var hint = new Rect(r.x + 14f, r.y + StripH + 12f, r.width - 28f, 64f);
            GUI.Label(hint, BodyHint, NameWrapStyle());
            if (BodyPresets == null)
            {
                GUI.Label(new Rect(hint.x, hint.yMax + 12f, hint.width, 80f), Strings.Error, NameWrapStyle());
                return;
            }
            for (int i = 0; i < BodyPresets.Length; i++)
            {
                var preset = BodyPresets[i];
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
                        if (IsCorpseTab) CorpseSpawner.Spawn(preset);
                        else ZombieSpawner.Spawn(preset);
                        ShowToast(IsCorpseTab ? Strings.CorpseSpawned : Strings.ZombieSpawned, KsTheme.Success);
                    }
                    catch (Exception e) when (!GameCompat.IsCompatibilityError(e))
                    {
                        Plugin.Log.LogError($"Body spawn failed (corpse={IsCorpseTab}): {e}");
                        ShowToast(Strings.Error, KsTheme.ItemName);
                    }
                }
                if (KsTheme.Repaint && IconCache.TryGet(IsCorpseTab ? "i_body" : "i_body_zombie", out var icon))
                    IconCache.Draw(new Rect(card.x + 12f, card.y + 24f, 56f, 56f), icon);
                var text = new Rect(card.x + 82f, card.y + 12f, card.width - 96f, 50f);
                GUI.Label(text, BodyTitle(i), NameWrapStyle());
                KsTheme.Label(new Rect(text.x, card.y + 68f, text.width, 24f),
                    Strings.ZombieSkulls(preset.White, preset.Red), 14, i == 0 ? KsTheme.TextBright : KsTheme.ItemName);
            }
        }

        private void DrawZombieInfo(Rect r)
        {
            EnsureZombiePresets();
            KsTheme.Draw(KsTheme.Panel, r);
            KsTheme.Strip(new Rect(r.x + 2f, r.y + 2f, r.width - 4f, StripH), Strings.ItemInfo);
            if (BodyPresets == null) return;
            var preset = BodyPresets[selectedZombie];
            var viewport = new Rect(r.x + 12f, r.y + StripH + 12f, r.width - 24f, r.height - StripH - 24f);
            float width = viewport.width - 16f;
            var style = NameWrapStyle();
            string title = BodyTitle(selectedZombie);
            string contents = title + "\n\n" + Strings.ZombieSkulls(preset.White, preset.Red) + "\n\n";
            foreach (var def in ZombieSpawner.Contents(preset))
                contents += def.GetHeader() + "\n" + Strings.ZombieSkulls(def.whiteSkulls, def.redSkulls) + "\n\n";
            if (preset.Collar != null) contents += preset.Collar.GetHeader();
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
