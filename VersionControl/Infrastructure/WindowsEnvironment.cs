namespace VersionControl.Infrastructure;

internal interface IWindowsEnvironment
{
    string UserName { get; }
}

internal class WindowsEnvironment : IWindowsEnvironment
{
    public string UserName => Environment.UserName;
}
