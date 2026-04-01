namespace FSMO;

public class OrganizePreview
{
    public List<FileMapping> FileMappings { get; set; } = new();
    public int TotalFiles => FileMappings.Count;
    public int WouldCopyOrMove => FileMappings.Count(f => f.Action != FileAction.Skip);
    public int WouldSkip => FileMappings.Count(f => f.Action == FileAction.Skip);
    public List<string> Errors { get; set; } = new();
}

public class FileMapping
{
    public required string SourcePath { get; set; }
    public required string TargetPath { get; set; }
    public required FileAction Action { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? Title { get; set; }
}

public enum FileAction
{
    Copy,
    Move,
    Skip,
    Overwrite,
    Rename
}
