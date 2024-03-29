[![GitHub all releases](https://img.shields.io/github/downloads/RandomGuyJCI/RDGameplayPatches/total)](https://github.com/RandomGuyJCI/RDGameplayPatches/releases/latest)
[![Contributions Welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/RandomGuyJCI/RDGameplayPatches/issues)
[![Discord](https://img.shields.io/discord/296802696243970049?color=%235865F2&label=discord&logo=Discord&logoColor=%23ffffff)](https://discord.gg/rhythmdr)

<div align="center">
  <img title="Thanks to WolfPlay013 for making this logo!" src="https://cdn.discordapp.com/attachments/298297906509774848/970402063097954376/bgpatch.png" width=128px>
  <h1>RDGameplayPatches</h1>
  <i>A <a href="https://github.com/BepInEx/BepInEx">BepInEx</a> plugin that adds several patches to <a href="https://rhythmdr.com">Rhythm Doctor</a> gameplay.</i>
</div>

---

## Features

### Hits
- **Very Hard**: Adds a Very Hard difficulty to Rhythm Doctor, changing its hit margins to 25 ms. Configurable per player.
- **2P Keyboard Layout**: Changes the keyboard layout for 2P hit keybinds. Includes Dvorak, Colemak, and Workman layouts.

### Holds
- **Accurate Release Margins**: Changes the hold release margins to better reflect the player difficulty, including Very Hard difficulty.
- **Count Offset On Release**: Shows the millisecond offset and counts the number of offset frames on hold releases.
- **Anti-cheese Holds**: Prevents you from abusing hold auto-hit mechanics by automatically releasing late holds and not letting beats later than the release be auto-hit.
- **Fix Auto-Hit Misses**: Fixes a long-standing issue where hold auto-hits completely miss some beats.
- **Fix Hold Pseudos**: Always auto-hits beats that land at the same time as a hold's release, even when released slightly early. *Recommended with Fix Auto-Hit Misses enabled.*

### HUD
- **Rank Color On Speed Change**: Implements Klyzx's suggestion and changes the rank color in the results depending on the level speed.
- **Change Rank Button Per Difficulty**: Implements lugi's suggestion and changes the button on the bottom-right corner of the rank screen depending on the player's difficulty.
- **Legacy Hit Judgment**: Reverts back to old game behavior which rounds the hit judgment millisecond offset to 3 decimal points.
- **Status Sign Transparency**: Sets the transparency of the status sign. 1 is fully opaque while 0 is fully transparent.

## TODO
- [ ] Change hold strip width based on difficulty
- [ ] Make multi-beat hits count as a single hit when it comes to offset
- [ ] Fix Smart Judgment not working for hold hits

## Installation
1. Download the latest version of **BepInEx 5 x86** [here](https://github.com/BepInEx/BepInEx/releases/latest). \
**Make sure you use the x86 version of BepInEx 5!** RD is x86 so the x64 version of BepInEx will not work, and BepInEx 6 is currently not yet compatible with BepInEx 5 mods.
2. Unzip the file into your RD folder. You should have a `winhttp.dll`, `doorstop_config.ini`, and `BepInEx` folder next to Rhythm Doctor.exe.
3. Launch RD once to generate BepInEx files.
4. Download the latest version of the mod from [here](https://github.com/RandomGuyJCI/RDGameplayPatches/releases). It should be named `RDGameplayPatches_1.x.x.zip`.
5. Unzip the file you downloaded into your Rhythm Doctor installation folder. You should now have a file at `BepInEx/plugins/RDGameplayPatches/RDGameplayPatches.dll`.
6. Launch the game, and the plugin should automatically enable.
7. Configure the plugin as needed in `BepInEx/config/com.rhythmdr.gameplaypatches.cfg`.
8. **Optional:** Install the [BepInEx Configuration Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager) to configure the mod with a GUI by pressing `F1`.
9. **Optional:** To enable in-game reloading, install the [BepInEx ScriptEngine](https://github.com/BepInEx/BepInEx.Debug/releases/latest), create a `scripts` folder in the `BepInEx` folder then move the `RDGameplayPatches.dll` file to that folder. You should now be able to reload the plugin by pressing `F6` in-game.

For more information, check out the [BepInEx installation guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html).
