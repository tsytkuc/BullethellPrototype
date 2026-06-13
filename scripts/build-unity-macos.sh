#!/usr/bin/env bash
set -euo pipefail

UNITY_APP="${UNITY_APP:-/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app}"
PROJECT_PATH="${PROJECT_PATH:-$PWD/unity}"
BUILD_METHOD="${BUILD_METHOD:-BuildScript.BuildWindows64}"
LOG_FILE="${LOG_FILE:-$PWD/unity-build.log}"

UNITY_BIN="$UNITY_APP/Contents/MacOS/Unity"

if [[ ! -x "$UNITY_BIN" ]]; then
  echo "Unity executable not found: $UNITY_BIN"
  echo "Set UNITY_APP to your installed Unity.app path."
  exit 1
fi

if [[ ! -d "$PROJECT_PATH" ]]; then
  echo "Unity project path not found: $PROJECT_PATH"
  exit 1
fi

echo "Unity: $UNITY_APP"
echo "Project: $PROJECT_PATH"
echo "Method: $BUILD_METHOD"
echo "Log: $LOG_FILE"

"$UNITY_BIN" \
  -batchmode \
  -quit \
  -projectPath "$PROJECT_PATH" \
  -executeMethod "$BUILD_METHOD" \
  -logFile "$LOG_FILE"

echo "Build completed. See $LOG_FILE"
