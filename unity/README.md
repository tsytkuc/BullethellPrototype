# Unity Build Prep

このフォルダは Unity 導入後に CLI ビルドを始めるための下準備です。

## 前提

- Unity Editor がインストール済み
- 最初に一度 `ProjectSetup.InitializeProject()` を実行して初期シーンを作ること

## 追加済みファイル

- `Assets/Editor/BuildScript.cs`
  - `BuildScript.BuildWindows64()`
  - `BuildScript.BuildMacOS()`
- `Assets/Editor/ProjectSetup.cs`
  - `ProjectSetup.InitializeProject()`
- `Assets/Scripts/`
  - `Bullet.cs`
  - `BulletSpawner.cs`
  - `PatternRunner.cs`
  - `BulletPatternAsset.cs`
  - `RingPatternAsset.cs`
  - `SpiralPatternAsset.cs`
- `../scripts/build-unity-macos.sh`
  - macOS から Unity CLI ビルドを叩く補助スクリプト
- `Packages/manifest.json`
- `ProjectSettings/ProjectVersion.txt`

## 最初の初期化

```bash
"/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode \
  -quit \
  -projectPath "$PWD/unity" \
  -executeMethod ProjectSetup.InitializeProject \
  -logFile "$PWD/unity-setup.log"
```

成功すると `Assets/Scenes/Main.unity` とビルド設定が作られます。

## 使い方

Windows 向けビルド:

```bash
chmod +x scripts/build-unity-macos.sh
UNITY_APP="/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app" \
PROJECT_PATH="$PWD/unity" \
BUILD_METHOD="BuildScript.BuildWindows64" \
scripts/build-unity-macos.sh
```

macOS 向けビルド:

```bash
UNITY_APP="/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app" \
PROJECT_PATH="$PWD/unity" \
BUILD_METHOD="BuildScript.BuildMacOS" \
scripts/build-unity-macos.sh
```

## 出力先

- Windows: `unity/Builds/Windows/BullethellPrototype.exe`
- macOS: `unity/Builds/macOS/BullethellPrototype.app`

## 次に必要なこと

- `Bullet` プレハブを作って `BulletSpawner` に割り当てる
- 空の GameObject に `PatternRunner` を付けて `BulletPatternAsset` を指定する
- 必要な prefab や material を追加する
