# Unity Build Prep

This folder contains the preparation work for starting Unity CLI builds after Unity has been installed.

## Prerequisites

- Unity Editor is installed
- Run `ProjectSetup.InitializeProject()` once first to create the initial scene

## Included Files

- `Assets/Editor/BuildScript.cs`
  - `BuildScript.BuildWindows64()`
  - `BuildScript.BuildMacOS()`
  - `BuildScript.BuildMacOSAppleSilicon()`
  - `BuildScript.BuildMacOSIntel()`
  - `BuildScript.BuildMacOSUniversal()`
- `Assets/Editor/ProjectSetup.cs`
  - `ProjectSetup.InitializeProject()`
- `Assets/Scripts/`
  - `Bullet.cs`
  - `BulletSpawner.cs`
  - `PatternRunner.cs`
  - `BulletPatternAsset.cs`
  - `RingPatternAsset.cs`
  - `SpiralPatternAsset.cs`
  - `StageDefinition.cs`
  - `StageLoader.cs`
  - `GameFlowController.cs`
- `Assets/StreamingAssets/Stages/`
  - Stage JSON exported from C#
- `../scripts/build-unity-macos.sh`
  - Helper script for running Unity CLI builds from macOS
- `Packages/manifest.json`
- `ProjectSettings/ProjectVersion.txt`

## Initial Setup

```bash
"/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode \
  -quit \
  -projectPath "$PWD/unity" \
  -executeMethod ProjectSetup.InitializeProject \
  -logFile "$PWD/unity-setup.log"
```

If successful, this creates `Assets/Scenes/Main.unity` and the initial build settings.

## Usage

Build for Windows:

```bash
chmod +x scripts/build-unity-macos.sh
UNITY_APP="/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app" \
PROJECT_PATH="$PWD/unity" \
BUILD_METHOD="BuildScript.BuildWindows64" \
scripts/build-unity-macos.sh
```

Build for macOS Apple Silicon:

```bash
UNITY_APP="/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app" \
PROJECT_PATH="$PWD/unity" \
BUILD_METHOD="BuildScript.BuildMacOSAppleSilicon" \
scripts/build-unity-macos.sh
```

Build for macOS Intel:

```bash
UNITY_APP="/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app" \
PROJECT_PATH="$PWD/unity" \
BUILD_METHOD="BuildScript.BuildMacOSIntel" \
scripts/build-unity-macos.sh
```

Build for macOS Universal:

```bash
UNITY_APP="/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app" \
PROJECT_PATH="$PWD/unity" \
BUILD_METHOD="BuildScript.BuildMacOSUniversal" \
scripts/build-unity-macos.sh
```

## Output Paths

- Windows x64: `unity/Builds/Windows-x64/BullethellPrototype.exe`
- macOS Apple Silicon: `unity/Builds/macOS-AppleSilicon/BullethellPrototype.app`
- macOS Intel: `unity/Builds/macOS-Intel/BullethellPrototype.app`
- macOS Universal: `unity/Builds/macOS-Universal/BullethellPrototype.app`

## Next Steps

- Create a `Bullet` prefab and assign it to `BulletSpawner`
- Attach `PatternRunner` to an empty GameObject and assign a `BulletPatternAsset`
- Place `GameFlowController` in the scene and load either `stage-1-prototype` or `stage-2-prototype`
- Connect dialogue UI and enemy spawning to `StageDefinition`
- Add any required prefabs and materials
