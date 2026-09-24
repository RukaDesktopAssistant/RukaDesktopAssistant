namespace RukaDesktopAssistant.Services;

public interface IRukaExtension
{
    string Id { get; }
    string Name { get; }
    bool CanHandle(string text);
    Task<string?> HandleAsync(string text, CancellationToken cancellationToken = default);
}

public sealed class ExtensionManager
{
    private readonly List<IRukaExtension> _extensions = new();

    public ExtensionManager() => Register(new TimeExtension());

    public void Register(IRukaExtension extension)
    {
        if (!_extensions.Any(x => x.Id.Equals(extension.Id, StringComparison.OrdinalIgnoreCase)))
            _extensions.Add(extension);
    }

    public async Task<string?> TryHandleAsync(string text, CancellationToken cancellationToken = default)
    {
        foreach (var extension in _extensions)
            if (extension.CanHandle(text))
                return await extension.HandleAsync(text, cancellationToken);
        return null;
    }
}

internal sealed class TimeExtension : IRukaExtension
{
    public string Id => "builtin.time";
    public string Name => "時刻";
    public bool CanHandle(string text) =>
        text.Contains("何時", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("いま何時", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("現在時刻", StringComparison.OrdinalIgnoreCase);

    public Task<string?> HandleAsync(string text, CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>($"今は{DateTime.Now:HH時mm分}だよ。");
}