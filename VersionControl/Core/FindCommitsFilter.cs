namespace VersionControl.Core;

public class FindCommitsFilter
{
    public int PageIndex { get; set; }

    public int PageSize { get; set; }

    public string? Author { get; set; }

    public string? Comment { get; set; }
}
