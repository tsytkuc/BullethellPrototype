# BullethellPrototype

BullethellPrototype is a bullet hell prototype project for validating danmaku algorithms in the browser while keeping a Unity migration path in parallel.

This repository is being developed with `Codex + GPT-5.4`.

## Overview

- Web prototype for fast iteration of bullet patterns
- Unity-side scaffold for future in-engine object-based implementation
- C#-first structure so the pattern logic can be moved into Unity more easily

## Using `JohnVonDrashek/benedict-bullethell-patterns`

This project uses the bullet pattern library from:

- `JohnVonDrashek/benedict-bullethell-patterns`
- NuGet package: `BenedictBulletHell.Patterns`

Current Web-side pattern generation is built around this library, and the Unity side is being prepared so the same style of pattern definition can be reused there.

## Repository Structure

- `BullethellPrototype.csproj`
  - ASP.NET Core entry project for the Web prototype
- `Program.cs`
  - Minimal API and static file hosting
- `Models/GameStageDto.cs`
  - Shared stage / dialogue / battle DTOs intended for both Web and Unity migration
- `Services/PatternCatalog.cs`
  - Pattern definitions and sample spawn generation
- `Services/GameStageCatalog.cs`
  - C# source of truth for stage flow, dialogue, and gameplay tuning
- `wwwroot/`
  - Browser UI, canvas rendering, and runtime controls
- `unity/`
  - Unity-side project scaffold, editor scripts, and runtime prototype scripts
- `scripts/build-unity-macos.sh`
  - Helper script for Unity CLI builds on macOS

## Web Preview

The Web prototype is the main working verification environment right now.
There are now two separate preview lines:

- `Danmaku Motion`
  - route: `/danmaku/index.html`
  - keeps the existing bullet pattern verification workflow
- `Game Motion`
  - route: `/game/index.html`
  - focuses on player movement, shooting, and core game loop behavior

### Unity Migration-Oriented Structure

The project is now being reorganized so the Web preview is a thin client over C# stage definitions:

- `Services/GameStageCatalog.cs`
  - owns the `Stage 1` prototype flow, dialogue, and battle tuning in C#
- `Program.cs`
  - exposes `/api/game/stages` and `/api/game/stages/{id}`
- `wwwroot/game/app.js`
  - fetches stage definitions from the API instead of hardcoding the flow
- `unity/Assets/StreamingAssets/Stages/`
  - JSON export target for the same stage definitions so Unity can read the same data later

This makes it easier to migrate the current Web prototype into Unity by reusing the same stage data shape instead of rewriting stage progression from scratch.

### Required Environment

- `.NET SDK 10` or later

Check your installation:

```bash
dotnet --info
```

### How to Run

```bash
dotnet restore
dotnet run --project BullethellPrototype.csproj --urls http://127.0.0.1:5000
```

Then open:

```text
http://127.0.0.1:5000
```

From the preview hub, choose either:

```text
/danmaku/index.html
/game/index.html
```

### What You Can Verify on Web

- Bullet pattern switching
- Bullet density adjustment
- Bullet speed adjustment
- Playback speed adjustment
- Visual feel of generated bullet timelines before Unity implementation

## Unity Build

Unity support is currently in a hybrid preparation state.

- The Unity project scaffold exists under `unity/`
- Runtime prototype scripts for `Bullet`, `BulletSpawner`, and `PatternRunner` are included
- Shared stage definition JSON is exported to `unity/Assets/StreamingAssets/Stages/`
- Editor build scripts are included
- Full end-to-end Unity build flow is not fully stabilized yet

### Required Environment

- Unity Hub
- Unity Editor `6000.4.10f1`
- macOS environment for the current helper script

### Included Unity Build Files

- `unity/Assets/Editor/BuildScript.cs`
- `unity/Assets/Editor/ProjectSetup.cs`
- `unity/Assets/Scripts/`
- `unity/Packages/manifest.json`
- `unity/ProjectSettings/ProjectVersion.txt`

### Current Unity Build Method

Project initialization attempt:

```bash
"/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode \
  -quit \
  -projectPath "$PWD/unity" \
  -executeMethod ProjectSetup.InitializeProject \
  -logFile "$PWD/unity-setup.log"
```

Build helper script:

```bash
chmod +x scripts/build-unity-macos.sh

UNITY_APP="/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app" \
PROJECT_PATH="$PWD/unity" \
BUILD_METHOD="BuildScript.BuildWindows64" \
scripts/build-unity-macos.sh
```

### Important Note

Unity build support is still incomplete. The repository already contains the Unity-side structure, but project initialization and batchmode build flow may still require manual editor setup depending on local licensing and editor state.

## License

This project is intended to be managed under the MIT License.

The external bullet pattern library `JohnVonDrashek/benedict-bullethell-patterns` is also distributed under the MIT License.
