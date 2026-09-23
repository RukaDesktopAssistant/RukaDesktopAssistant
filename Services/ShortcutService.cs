using System.Windows.Input;

namespace RukaDesktopAssistant.Services;

public sealed class ShortcutService
{
    public Key PauseKey { get; set; } = Key.F12;
    public KeyGesture OpenChatGesture { get; set; } = new(Key.R, ModifierKeys.Control | ModifierKeys.Alt);
    public event Action? EmergencyStop;
    public void TriggerEmergencyStop() => EmergencyStop?.Invoke();
}
