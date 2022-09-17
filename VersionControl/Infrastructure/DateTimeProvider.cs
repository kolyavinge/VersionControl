namespace VersionControl.Infrastructure;

internal interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

internal class DateTimeProvider : IDateTimeProvider
{
    public static readonly DateTimeProvider Instance = new();

    public DateTime UtcNow => DateTime.UtcNow;

    private DateTimeProvider() { }
}
