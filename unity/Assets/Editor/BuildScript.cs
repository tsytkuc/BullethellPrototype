using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildScript
{
    private const int MacArchitectureIntel = 0;
    private const int MacArchitectureAppleSilicon = 1;
    private const int MacArchitectureUniversal = 2;
    private const string ExportPackagePath = "Exports/BullethellPrototype.unitypackage";

    private static readonly string[] Scenes =
    {
        "Assets/Scenes/Main.unity",
    };

    private static readonly string[] PackageRoots =
    {
        "Assets/Editor",
        "Assets/Scenes",
        "Assets/Scripts",
        "Assets/StreamingAssets",
    };

    public static void BuildWindows64()
    {
        Build(BuildTarget.StandaloneWindows64, "Builds/Windows-x64/BullethellPrototype.exe");
    }

    public static void BuildMacOS()
    {
        BuildMacOSUniversal();
    }

    public static void BuildMacOSAppleSilicon()
    {
        BuildMac(MacArchitectureAppleSilicon, "Builds/macOS-AppleSilicon/BullethellPrototype.app");
    }

    public static void BuildMacOSIntel()
    {
        BuildMac(MacArchitectureIntel, "Builds/macOS-Intel/BullethellPrototype.app");
    }

    public static void BuildMacOSUniversal()
    {
        BuildMac(MacArchitectureUniversal, "Builds/macOS-Universal/BullethellPrototype.app");
    }

    public static void ExportUnityPackage()
    {
        EnsureScenesExist();

        Directory.CreateDirectory(Path.GetDirectoryName(ExportPackagePath) ?? "Exports");

        AssetDatabase.ExportPackage(
            PackageRoots,
            ExportPackagePath,
            ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
    }

    private static void Build(BuildTarget target, string outputPath)
    {
        EnsureScenesExist();

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "Builds");

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = Scenes,
            target = target,
            locationPathName = outputPath,
            options = BuildOptions.None,
        });

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            throw new BuildFailedException($"Build failed: {report.summary.result}");
        }
    }

    private static void BuildMac(int architecture, string outputPath)
    {
        int previousArchitecture = PlayerSettings.GetArchitecture(NamedBuildTarget.Standalone);

        try
        {
            PlayerSettings.SetArchitecture(NamedBuildTarget.Standalone, architecture);
            Build(BuildTarget.StandaloneOSX, outputPath);
        }
        finally
        {
            PlayerSettings.SetArchitecture(NamedBuildTarget.Standalone, previousArchitecture);
        }
    }

    private static void EnsureScenesExist()
    {
        foreach (var scene in Scenes)
        {
            if (!File.Exists(scene))
            {
                CreateDefaultScene(scene);
            }
        }

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(Scenes[0], true),
        };
    }

    private static void CreateDefaultScene(string scenePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(scenePath) ?? "Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = Path.GetFileNameWithoutExtension(scenePath);

        var camera = Camera.main;
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.03f, 0.07f, 0.14f);
            camera.clearFlags = CameraClearFlags.SolidColor;
        }

        if (!EditorSceneManager.SaveScene(scene, scenePath))
        {
            throw new FileNotFoundException($"Scene could not be created: {scenePath}");
        }
    }
}
