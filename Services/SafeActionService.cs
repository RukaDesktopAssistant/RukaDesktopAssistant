namespace RukaDesktopAssistant.Services;

public enum ActionRisk { Low, Medium, High }

public sealed record ActionRequest(string Id, string Description, ActionRisk Risk, Func<Task> Execute);

public sealed class SafeActionService
{
    private readonly PermissionManager _permissions;

    public SafeActionService(PermissionManager permissions) => _permissions = permissions;

    public async Task<bool> TryExecuteAsync(ActionRequest request, Func<string, Task<bool>> confirm)
    {
        if (request.Risk == ActionRisk.High && _permissions.RequireConfirmationForDangerousActions)
        {
            var approved = await confirm(request.Description);
            if (!approved) return false;
        }

        try
        {
            await request.Execute();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
