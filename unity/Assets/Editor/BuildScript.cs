using System.IO;
using UnityEditor;

public static class BuildScript
{
    private static readonly string[] Scenes =
    {
        "Assets/Scenes/Main.unity",
    };

    public static void BuildWindows64()
    {
        Build(BuildTarget.StandaloneWindows64, "Builds/Windows/BullethellPrototype.exe");
    }

    public static void BuildMacOS()
    {
        Build(BuildTarget.StandaloneOSX, "Builds/macOS/BullethellPrototype.app");
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

    private static void EnsureScenesExist()
    {
        foreach (var scene in Scenes)
        {
            if (!File.Exists(scene))
            {
                throw new FileNotFoundException($"Scene not found: {scene}");
            }
        }
    }
}
