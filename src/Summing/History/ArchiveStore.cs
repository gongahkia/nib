using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Summing.History;

public sealed record ArchiveEntry(
    string Id,
    string WorldHistoryId,
    long Seed,
    string Epoch,
    string Culture,
    string Title,
    string Fragment,
    string Trace,
    DateTimeOffset CollectedAtUtc);

public sealed class ArchiveDocument
{
    public int Version { get; set; } = 1;
    public List<ArchiveEntry> Discoveries { get; set; } = [];
}

public sealed class ArchiveStore
{
    private readonly string _path;
    private readonly ArchiveDocument _document;

    public ArchiveStore(string path)
    {
        _path = path;
        _document = Load(path);
    }

    public IReadOnlyList<ArchiveEntry> Discoveries => _document.Discoveries;

    public bool Add(ArchiveEntry entry)
    {
        if (_document.Discoveries.Any(existing => existing.Id == entry.Id)) return false;
        _document.Discoveries.Add(entry);
        Save();
        return true;
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");
        var temporary = _path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(_document, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, _path, true);
    }

    private static ArchiveDocument Load(string path)
    {
        if (!File.Exists(path)) return new ArchiveDocument();
        try
        {
            return JsonSerializer.Deserialize<ArchiveDocument>(File.ReadAllText(path)) ?? new ArchiveDocument();
        }
        catch (JsonException)
        {
            var backup = path + $".invalid-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}";
            File.Copy(path, backup, false);
            return new ArchiveDocument();
        }
    }
}
