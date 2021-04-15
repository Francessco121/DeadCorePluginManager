# DeadCore Plugin Manager

A "decompiled fork" of [Standalone](https://github.com/MrStandalone)'s plugin manager for the game [DeadCore](https://store.steampowered.com/app/284460/DeadCore/).

Standalone's plugin manager used to exist at https://github.com/MrStandalone/DeadCorePluginManager, but has since been deleted. This repository's code was created by decompiling a build provided by Standalone [in this Steam forum thread](https://steamcommunity.com/app/284460/discussions/0/619568794056639879/?ctp=2#c458607518211812791) using [dnSpy](https://github.com/dnSpy/dnSpy) and afterwards cleaned up manually.

## Contents
- [Plugins](#plugins)
    - [SmoothCamera](#smoothcamera)
- [Manual Installation](#manual-installation)
- [Custom Plugins](#custom-plugins)

## Plugins

### SmoothCamera

Smooths camera movement to the game's render framerate to fix the camera being locked to the game's 50 FPS physics framerate.

> Important Note: This plugin smoothly moves the camera between player positions meaning that the camera will be (at most) 1 frame behind during gameplay. This **will** affect shooting as the gun will fire from the position the camera would have been at with this plugin off. **However**, this is really not noticeable in practice due to it being at most a 0.02s offset. 

The SmoothCamera plugin was also included with the [build provided by Standalone on the steam forums](https://steamcommunity.com/app/284460/discussions/0/619568794056639879/?ctp=2#c458607518211812791). The version here has been modified slightly. Originally, SmoothCamera had a "smooth factor" option. It turned out that this option isn't needed and the perfect smooth factor can be automatically determined. Also includes some minor fixes like working correctly with player teleports.

#### Usage
Once in a level, you can press the <kbd>~</kbd> key to bring up the plugin manager's console. Then, you can enter the `sc_toggle` command to toggle on/off the smooth camera plugin (by default, this is on).

#### "The game still doesn't feel like it's above 60 FPS"
Unfortunately, this plugin only addresses camera smoothing with player movement. DeadCore's physics and animations will still simulate at 50 FPS.

## Manual Installation

> Note: If you'd like to easily install the plugin manager and the smooth camera plugin, please head to the [releases page](https://github.com/Francessco121/DeadCorePluginManager/releases).

The plugin manager lives as a set of .NET DLLs next to DeadCore's main .NET gameplay code DLL (`Assembly-CSharp.dll`). In order for the plugin manager to even be initialized, DeadCore's `Assembly-CSharp.dll` file must be modified to call the plugin manager's `DCPM.Initializer.Initialize`.

1. First, build the plugin manager solution.
2. Go to DeadCore's installation directory (normally at `C:\Program Files (x86)\Steam\steamapps\common\DeadCore`).
3. Place the built `DCPM.dll`, `DCPM.Common.dll`, and `DCPM.PluginBase.dll` files inside of `DeadCore_Data\Managed`.
4. Make a backup copy of `Assembly-CSharp.dll` (found in `DeadCore_Data\Managed`).
5. Modify `Assembly-CSharp.dll` using a tool capable of modifying .NET assemblies (such as [dnSpy](https://github.com/dnSpy/dnSpy)) to have `GameManager.Awake` call `DCPM.Initializer.Initialize`.
    - Example `GameManager.Awake` code:
      ```csharp
      private void Awake()
      {
          if (!GameManager.ApplicationQuit)
          {
              if (GameManager._instance != null)
              {
                  UnityEngine.Object.DestroyImmediate(this);
                  return;
              }
              SaveManager.Instance.Init();
              this.CurrentGameState = GameManager.GameState.MainMenu;
              this.CurrentGameMode = GameManager.GameMode.None;
              UnityEngine.Object.DontDestroyOnLoad(this);

              DCPM.Initializer.Initialize(); // Add this!
          }
      }
      ```
    - You will also need to add an assembly reference to `DCPM.dll` from `Assembly-CSharp.dll`.
        - This can be done using dnSpy inside the "Edit Method" dialog.
6. Place any built plugin DLLs (such as `SmoothCamera.dll`) in `DeadCore_Data\Managed\dcpm-plugins`.

## Custom Plugins

Making custom plugins is pretty straight forward. You just need to create a .NET 3.5 DLL containing a class (or multiple classes) that extend `DCPM.PluginBase.DeadCorePlugin`. Once you're done or ready to test, build your plugin and place (only) it's DLL into `<DeadCore Installation Dir>\DeadCore_Data\Managed\dcpm-plugins`. You can also reference any game DLLs you may need, including other plugins, just make sure to only copy your plugin's DLL into `dcpm-plugins`.

For more information, please use the SmoothCamera plugin as a reference.
