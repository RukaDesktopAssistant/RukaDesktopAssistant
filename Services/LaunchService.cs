using System.Diagnostics;

namespace RukaDesktopAssistant.Services;

public sealed class LaunchService
{
    private readonly PermissionManager _permissions;
    public LaunchService(PermissionManager permissions) => _permissions = permissions;
    public bool TryLaunch(string target)
    {
        if (!_permissions.AllowLaunchApps) return false;
        Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
        return true;
    }
}
