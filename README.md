# RDGameplayPatches
A BepInEx plugin that adds several patches to Rhythm Doctor gameplay.

## Features

### 2P
- **Persistent P1 And P2 Positions**: Reverts back to old game behavior and makes P1 and P2 positions persistent between level restarts.

### Difficulty
- **Very Hard**: Adds a Very Hard difficulty to Rhythm Doctor, changing its hit margins to 25 ms. Configurable per player.

### Holds
- **Accurate Release Margins**: Changes the hold release margins to better reflect the player difficulty, including Very Hard difficulty.
- **Count Offset On Release**: Shows the millisecond offset and counts the number of offset frames on hold releases.
- **Anti-cheese Holds**: Prevents you from abusing hold auto-hit mechanics by automatically releasing late holds and not letting beats later than the release be auto-hit.

## TODO
- [ ] Fix auto-hits missing/counting towards offset
- [ ] Change hold strip width based on difficulty
- [ ] Fix unmissable difficulty making hold hit timings 200ms instead of 400ms
- [ ] Auto-hit beats until hold ends even when the hold is released early
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
8. **Optional:** To enable reloading the plugin on the fly, download and extract the [BepInEx ScriptEngine](https://github.com/BepInEx/BepInEx.Debug/releases/latest) plugin into your RD folder. Create a `scripts` folder in the `BepInEx` folder then move `RDGameplayPatches.dll` to that folder. You should now be able to reload the plugin by pressing `F6` in-game. You can also configure the hotkey in `BepInEx/config/com.bepis.bepinex.scriptengine`).

For more information, check out the [BepInEx installation guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html).
