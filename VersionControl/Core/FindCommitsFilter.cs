namespace VersionControl.Core;

public class FindCommitsFilter
{
    public int PageIndex { get; set; } = 0;

    public int PageSize { get; set; } = 10;

    public string? Author { get; set; }

    public string? Comment { get; set; }
}
