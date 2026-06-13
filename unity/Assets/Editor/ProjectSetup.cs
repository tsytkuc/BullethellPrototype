using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ProjectSetup
{
    private const string ScenesDirectory = "Assets/Scenes";
    private const string MainScenePath = ScenesDirectory + "/Main.unity";

    public static void InitializeProject()
    {
        Directory.CreateDirectory(ScenesDirectory);
        Directory.CreateDirectory("Assets/Scripts");
        Directory.CreateDirectory("Assets/Prefabs");
        Directory.CreateDirectory("Assets/Materials");

        if (!File.Exists(MainScenePath))
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "Main";

            var camera = Camera.main;
            if (camera != null)
            {
                camera.backgroundColor = new Color(0.03f, 0.07f, 0.14f);
                camera.clearFlags = CameraClearFlags.SolidColor;
            }

            EditorSceneManager.SaveScene(scene, MainScenePath);
        }

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainScenePath, true),
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorApplication.Exit(0);
    }
}
