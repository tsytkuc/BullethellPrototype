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
            yield return LoadJson(path, onLoaded, onError);
        }

        public static IEnumerator LoadDialogue(string fileName, Action<DialogueScript> onLoaded, Action<string> onError)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "game-data", "dialogue", fileName);
            yield return LoadJson(path, onLoaded, onError);
        }

        public static IEnumerator LoadCharacter(string fileName, Action<CharacterProfile> onLoaded, Action<string> onError)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "game-data", "characters", fileName);
            yield return LoadJson(path, onLoaded, onError);
        }

        private static IEnumerator LoadJson<T>(string path, Action<T> onLoaded, Action<string> onError)
        {
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

        private static void TryParse<T>(string json, Action<T> onLoaded, Action<string> onError)
        {
            T value = JsonUtility.FromJson<T>(json);
            if (value == null)
            {
                onError?.Invoke("Failed to parse json.");
                return;
            }

            onLoaded?.Invoke(value);
        }
    }
}
