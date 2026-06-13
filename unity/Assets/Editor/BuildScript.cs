using System.IO;
using UnityEditor;
using UnityEditor.Build;

public static class BuildScript
{
    private const int MacArchitectureIntel = 0;
    private const int MacArchitectureAppleSilicon = 1;
    private const int MacArchitectureUniversal = 2;

    private static readonly string[] Scenes =
    {
        "Assets/Scenes/Main.unity",
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
                throw new FileNotFoundException($"Scene not found: {scene}");
            }
        }
    }
}
