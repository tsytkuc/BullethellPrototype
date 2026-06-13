using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace BullethellPrototype.Unity
{
    public static class StageLoader
    {
        public static IEnumerator LoadStage(string stageId, Action<StageDefinition> onLoaded, Action<string> onError)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Stages", $"{stageId}.json");

            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("jar:", StringComparison.OrdinalIgnoreCase))
            {
                using UnityWebRequest request = UnityWebRequest.Get(path);
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    yield break;
                }

                TryParse(request.downloadHandler.text, onLoaded, onError);
                yield break;
            }

            if (!File.Exists(path))
            {
                onError?.Invoke($"Stage json not found: {path}");
                yield break;
            }

            string json = File.ReadAllText(path);
            TryParse(json, onLoaded, onError);
        }

        private static void TryParse(string json, Action<StageDefinition> onLoaded, Action<string> onError)
        {
            StageDefinition stage = JsonUtility.FromJson<StageDefinition>(json);
            if (stage == null || string.IsNullOrWhiteSpace(stage.id))
            {
                onError?.Invoke("Failed to parse stage json.");
                return;
            }

            onLoaded?.Invoke(stage);
        }
    }
}
