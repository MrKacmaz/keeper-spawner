# Nexus Mods page fields

Mod page: https://www.nexusmods.com/graveyardkeeper2/mods/275
Full description: [description.bbcode](description.bbcode) (paste into "Detailed description").

## Mod details

- **Name:** KeeperSpawner
- **Version:** 1.1.0
- **Category:** Cheats and God items (or Utilities)
- **Brief overview (summary, max 350 characters):**

  > Lightweight item spawner: press O in game, find any of 726 items by name, category or quality and click to add it to your inventory. Game icons, 17 languages, favorites. Optional GK2 Mod Framework support adds it to the in-game Mods menu with its settings.

## Requirements

**Nexus requirements**

| Mod | Notes |
|---|---|
| GK2 Mod Framework (mods/42) | Optional. Adds KeeperSpawner to the in-game Mods menu (status and settings). 0.1.19 or newer. |

**Off-site requirements**

| Name | Link | Notes |
|---|---|---|
| BepInEx 5.4.23.5 (win_x64) | https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5 | Required mod loader. Extract next to GraveyardKeeper2.exe and start the game once. |

## Files

**Main file**

- **File name:** KeeperSpawner 1.1.0
- **Version:** 1.1.0
- **Category:** Main files
- **Upload:** `artifacts/1.1.0/KeeperSpawner-1.1.0.zip`
- **Description:**

  > Extract into the Graveyard Keeper 2 folder (contains BepInEx/plugins/KeeperSpawner). Requires BepInEx 5.4.23.5. Includes the optional GK2 Mod Framework bridge; without the Framework it is skipped and the mod works as before. Updating: extract over 1.0.0, settings are kept.

**Old file**

- Move **KeeperSpawner 1.0.0** to **Old versions** and add to its description: `Superseded by 1.1.0.`

## Changelog (Nexus "Logs" tab, version 1.1.0)

- Optional GK2 Mod Framework support: KeeperSpawner shows up in the Mods menu with its status and settings (hotkey, amount per click, quest items, view, UI scale, background dimming).
- Mods menu texts in all 17 languages of the game.
- Log messages are now in English.

## Reply to Joe43k

> 1.1.0 is out. With GK2 Mod Framework 0.1.19 installed, KeeperSpawner now shows up in the Mods menu, and its Status line tells you whether it is running and which key opens it. Two things to check: the key is the letter O (not zero), and it only works after loading a save, not in the main menu. If it still does not open, please post BepInEx\LogOutput.log (or the lines that contain "KeeperSpawner") and I will take a look.
