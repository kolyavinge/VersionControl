namespace VersionControl.Infrastructure;

internal interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

internal class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
