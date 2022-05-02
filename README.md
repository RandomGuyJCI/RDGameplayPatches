[![GitHub all releases](https://img.shields.io/github/downloads/RandomGuyJCI/RDGameplayPatches/total)](https://github.com/RandomGuyJCI/RDGameplayPatches/releases/latest)
[![Contributions Welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/RandomGuyJCI/RDGameplayPatches/issues)
[![Discord](https://img.shields.io/discord/296802696243970049?color=%235865F2&label=discord&logo=Discord&logoColor=%23ffffff)](https://discord.gg/rhythmdr)

<div align="center">
  <img src="https://cdn.discordapp.com/attachments/298297906509774848/970402063097954376/bgpatch.png" width=128px>
  <h1>RDGameplayPatches</h1>
  <i>A <a href="https://github.com/BepInEx/BepInEx">BepInEx</a> plugin that adds several patches to <a href="https://rhythmdr.com">Rhythm Doctor</a> gameplay.</i>
</div>

---

## Features

### Difficulty
- **Very Hard**: Adds a Very Hard difficulty to Rhythm Doctor, changing its hit margins to 25 ms. Configurable per player.

### Holds
- **Accurate Release Margins**: Changes the hold release margins to better reflect the player difficulty, including Very Hard difficulty.
- **Count Offset On Release**: Shows the millisecond offset and counts the number of offset frames on hold releases.
- **Anti-cheese Holds**: Prevents you from abusing hold auto-hit mechanics by automatically releasing late holds and not letting beats later than the release be auto-hit.
- **Fix Auto-Hit Misses**: Fixes a long-standing issue where hold auto-hits completely miss some beats.
- **Fix Hold Pseudos**: Always auto-hits beats that land at the same time as a hold's release, even when released slightly early. *Recommended with Fix Auto-Hit Misses enabled.*

### Results
- **Rank Color On Speed Change**: Implements Klyzx's suggestion and changes the rank color in the results depending on the level speed.
- **Change Rank Button Per Difficulty**: Implements lugi's suggestion and changes the small button on the bottom-right corner depending on the player's difficulty.

## TODO
- [ ] Change hold strip width based on difficulty
- [ ] Make 2P keyboard locations independent from keyboard layout

## Installation
1. Download the latest version of BepInEx 5 **x86** [here](https://github.com/BepInEx/BepInEx/releases). \
**Make sure you use the x86 version!** RD is x86 so the x64 version of BepInEx will not work.
2. Unzip the file into your RD folder. You should have a `winhttp.dll`, `doorstop_config.ini`, and `BepInEx` folder next to Rhythm Doctor.exe.
3. Launch RD once to generate BepInEx files.
4. Download the latest version of the mod from [here](https://github.com/RandomGuyJCI/RDGameplayPatches/releases). It should be named `RDGameplayPatches_1.x.x.zip`.
5. Unzip the file you downloaded into your Rhythm Doctor installation folder. You should now have a file at `BepInEx/Plugins/RDGameplayPatches/RDGameplayPatches.dll`.
6. Launch the game, and the plugin should automatically enable.
7. Configure the plugin as needed in `BepInEx/config/com.rhythmdr.gameplaypatches.cfg`.
8. **Optional:** Install the [BepInEx Configuration Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager) to configure the mod with a GUI by pressing `F1`.
9. **Optional:** To enable in-game reloading, install the [BepInEx ScriptEngine](https://github.com/BepInEx/BepInEx.Debug/releases/latest), create a `scripts` folder in the `BepInEx` folder then move the `RDGameplayPatches.dll` file to that folder. You should now be able to reload the plugin by pressing `F6` in-game.

For more information, check out the [BepInEx installation guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html).
