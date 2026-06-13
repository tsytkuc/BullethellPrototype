using System.Text.Json;
using BullethellPrototype.Models;

namespace BullethellPrototype.Services;

public sealed class GameContentExporter
{
    private readonly IWebHostEnvironment _environment;
    private readonly JsonSerializerOptions _jsonOptions;

    public GameContentExporter(IWebHostEnvironment environment)
    {
        _environment = environment;
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
        };
    }

    public void ExportSharedContent()
    {
        string sourceRoot = Path.Combine(_environment.ContentRootPath, "GameContent");
        string webRoot = Path.Combine(_environment.WebRootPath, "game", "data");
        string unityRoot = Path.Combine(_environment.ContentRootPath, "unity", "Assets", "StreamingAssets", "game-data");

        CopyDirectory(Path.Combine(sourceRoot, "characters"), Path.Combine(webRoot, "characters"));
        CopyDirectory(Path.Combine(sourceRoot, "dialogue"), Path.Combine(webRoot, "dialogue"));
        CopyDirectory(Path.Combine(sourceRoot, "characters"), Path.Combine(unityRoot, "characters"));
        CopyDirectory(Path.Combine(sourceRoot, "dialogue"), Path.Combine(unityRoot, "dialogue"));
    }

    public CharacterProfileDto LoadCharacterProfile(string fileName)
    {
        string path = Path.Combine(_environment.ContentRootPath, "GameContent", "characters", fileName);
        return LoadJson<CharacterProfileDto>(path);
    }

    public DialogueScriptDto LoadDialogueScript(string fileName)
    {
        string path = Path.Combine(_environment.ContentRootPath, "GameContent", "dialogue", fileName);
        return LoadJson<DialogueScriptDto>(path);
    }

    private T LoadJson<T>(string path)
    {
        string json = File.ReadAllText(path);
        T? value = JsonSerializer.Deserialize<T>(json, _jsonOptions);
        return value ?? throw new InvalidOperationException($"Failed to deserialize: {path}");
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);

        foreach (string file in Directory.GetFiles(source))
        {
            string fileName = Path.GetFileName(file);
            File.Copy(file, Path.Combine(destination, fileName), overwrite: true);
        }
    }
}
