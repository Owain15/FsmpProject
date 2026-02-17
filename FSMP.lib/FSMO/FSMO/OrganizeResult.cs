namespace FSMO;

public class OrganizeResult
{
    public int FilesCopied { get; set; }
    public int FilesMoved { get; set; }
    public int FilesSkipped { get; set; }
    public List<string> Errors { get; set; } = new();
}