using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Input;

namespace RukaDesktopAssistant.Services;

public sealed class ShortcutService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModNoRepeat = 0x4000;
    private const int PauseId = 1001;
    private const int ChatId = 1002;
    private const int EmergencyId = 1003;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(nint hWnd, int id, uint modifiers, uint key);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(nint hWnd, int id);

    private readonly Window _window;
    private HwndSource? _source;
    private bool _registered;

    public Key PauseKey { get; set; } = Key.F12;
    public event Action? Pause;
    public event Action? OpenChat;
    public event Action? EmergencyStop;

    public ShortcutService(Window window) => _window = window;

    public void Start()
    {
        if (_registered) return;

        var helper = new WindowInteropHelper(_window);
        _source = HwndSource.FromHwnd(helper.Handle);
        _source?.AddHook(WndProc);

        if (_source is null) return;

        _registered =
            RegisterHotKey(helper.Handle, PauseId, ModNoRepeat, (uint)KeyInterop.VirtualKeyFromKey(PauseKey)) &&
            RegisterHotKey(helper.Handle, ChatId, ModControl | ModAlt | ModNoRepeat, (uint)KeyInterop.VirtualKeyFromKey(Key.R)) &&
            RegisterHotKey(helper.Handle, EmergencyId, ModControl | ModAlt | ModNoRepeat, (uint)KeyInterop.VirtualKeyFromKey(Key.Escape));
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == WmHotkey)
        {
            switch (wParam.ToInt32())
            {
                case PauseId: Pause?.Invoke(); handled = true; break;
                case ChatId: OpenChat?.Invoke(); handled = true; break;
                case EmergencyId: EmergencyStop?.Invoke(); handled = true; break;
            }
        }

        return nint.Zero;
    }

    public void Dispose()
    {
        if (_source is null) return;
        var hwnd = _source.Handle;
        UnregisterHotKey(hwnd, PauseId);
        UnregisterHotKey(hwnd, ChatId);
        UnregisterHotKey(hwnd, EmergencyId);
        _source.RemoveHook(WndProc);
        _source = null;
        _registered = false;
    }
}
