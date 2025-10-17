# DisableAimFOV

A BepInEx mod for MycoPunk that removes FOV zoom changes when aiming to reduce claustrophobia.

## Description

This client-side mod eliminates the field of view (FOV) zoom that occurs when aiming weapons in MycoPunk. Many players experience discomfort or claustrophobia from the narrowing of the field of view during aiming, and this mod maintains the default FOV at all times, creating a more comfortable and less disorienting gameplay experience.

The mod uses Harmony to patch the PlayerLook aiming system methods (UpdateAiming and UpdateCameraFOV), replacing the aiming FOV transitions with static default FOV values. This creates smooth aiming without the jarring zoom effects.

## Getting Started

### Dependencies

* MycoPunk (base game)
* [BepInEx](https://github.com/BepInEx/BepInEx) - Version 5.4.2403 or compatible
* .NET Framework 4.8

### Building/Compiling

1. Clone this repository
2. Open the solution file in Visual Studio, Rider, or your preferred C# IDE
3. Build the project in Release mode

Alternatively, use dotnet CLI:
```bash
dotnet build --configuration Release
```

### Installing

**Option 1: Via Thunderstore (Recommended)**
1. Download and install using the Thunderstore Mod Manager
2. Search for "DisableAimFOV" under MycoPunk community
3. Install and enable the mod

**Option 2: Manual Installation**
1. Ensure BepInEx is installed for MycoPunk
2. Copy `DisableAimFOV.dll` from the build folder
3. Place it in `<MycoPunk Game Directory>/BepInEx/plugins/`
4. Launch the game

### Executing program

Once the mod is loaded, FOV changes when aiming are automatically disabled. The game will maintain your default field of view at all times.

### Configuration

The mod can be configured through BepInEx Configuration Manager:

**General Settings:**
- `DisableFOVChange`: (Default: true) If enabled, FOV remains constant when aiming instead of zooming in

Changes require a game restart to take effect.

## Help

* **Aiming feels different?** This mod intentionally removes the FOV zoom effect to reduce claustrophobia
* **Performance issues?** The mod only patches aiming logic and shouldn't impact game performance
* **Conflicts with camera mods?** If you have other camera or FOV modifications, they may interfere with this mod
* **Configuration not working?** Ensure you restart the game after changing BepInEx config settings
* **Only applies to weapons?** The mod specifically targets weapon aiming FOV changes in PlayerLook class
* **Not working with certain weapons?** All weapons that normally zoom should be affected by this mod

## Authors

* Sparroh
* funlennysub (original mod template)
* [@DomPizzie](https://twitter.com/dompizzie) (README template)

## License

* This project is licensed under the MIT License - see the LICENSE.md file for details
