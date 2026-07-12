using System.Windows.Input;
using Contracts.Enum;
using Contracts.Shortcuts;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels;

public class EditShortcutDialogViewModel : BaseViewModel
{
    private readonly ShortcutBinding _originalBinding;
    private readonly ShortcutProfileType _profileType;
    private readonly IKeyboardShortcutService _shortcutService;

    private string _currentGesture = string.Empty;
    private string _newGesture = string.Empty;
    private bool _isCapturing;
    private string? _errorMessage;
    private ShortcutConflict? _conflict;

    public EditShortcutDialogViewModel(
        ShortcutBinding binding,
        ShortcutProfileType profileType,
        IKeyboardShortcutService shortcutService)
    {
        _originalBinding = binding;
        _profileType = profileType;
        _shortcutService = shortcutService;

        ActionName = binding.Action.ToString();
        Description = binding.Description;
        _currentGesture = binding.KeyGesture;

        StartCaptureCommand = new RelayCommand(_ => StartCapture(), _ => !IsCapturing);
        CancelCaptureCommand = new RelayCommand(_ => CancelCapture(), _ => IsCapturing);
        SaveCommand = new RelayCommand(_ => OnSave(), _ => CanSave());
        CloseCommand = new RelayCommand(_ => RequestClose?.Invoke(false));
    }

    public string ActionName { get; }
    public string Description { get; }

    public string CurrentGesture
    {
        get => _currentGesture;
        set { _currentGesture = value; OnPropertyChanged(); }
    }

    public string NewGesture
    {
        get => _newGesture;
        set { _newGesture = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNewGesture)); }
    }

    public bool HasNewGesture => !string.IsNullOrWhiteSpace(NewGesture);

    public bool IsCapturing
    {
        get => _isCapturing;
        set { _isCapturing = value; OnPropertyChanged(); OnPropertyChanged(nameof(CapturePrompt)); }
    }

    public string CapturePrompt => IsCapturing ? "Press keys now..." : "Press to start capturing";

    public string? ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public ShortcutConflict? Conflict
    {
        get => _conflict;
        set { _conflict = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasConflict)); }
    }

    public bool HasConflict => Conflict != null;

    public ICommand StartCaptureCommand { get; }
    public ICommand CancelCaptureCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }

    public Action<bool>? RequestClose { get; set; }

    public void HandleKeyDown(Key key, ModifierKeys modifiers)
    {
        if (!IsCapturing)
            return;

        if (key == Key.Escape)
        {
            CancelCapture();
            return;
        }

        if (modifiers == ModifierKeys.None && (key == Key.System || key == Key.LeftAlt || key == Key.RightAlt))
            return;

        var gesture = KeyGestureParser.ToDisplayString(key, modifiers);
        NewGesture = gesture;
        IsCapturing = false;
        ValidateGesture();
    }

    private void StartCapture()
    {
        ErrorMessage = null;
        Conflict = null;
        NewGesture = string.Empty;
        IsCapturing = true;
    }

    private void CancelCapture()
    {
        IsCapturing = false;
        NewGesture = string.Empty;
        ErrorMessage = null;
        Conflict = null;
    }

    private bool CanSave()
    {
        return HasNewGesture && !HasError && !HasConflict;
    }

    private void ValidateGesture()
    {
        ErrorMessage = null;
        Conflict = null;

        if (string.IsNullOrWhiteSpace(NewGesture))
            return;

        if (!KeyGestureParser.TryParse(NewGesture, out _, out _))
        {
            ErrorMessage = "Invalid key combination.";
            return;
        }

        var bindings = _shortcutService.GetBindings(_profileType);
        var conflict = bindings.FirstOrDefault(b =>
            b.Action != _originalBinding.Action
            && string.Equals(b.KeyGesture, NewGesture, StringComparison.OrdinalIgnoreCase));

        if (conflict != null)
        {
            Conflict = new ShortcutConflict
            {
                ExistingAction = conflict.Action,
                ExistingGesture = conflict.KeyGesture,
                ConflictingAction = _originalBinding.Action,
                ConflictingGesture = NewGesture
            };
        }
    }

    private void OnSave()
    {
        if (string.IsNullOrWhiteSpace(NewGesture))
            return;

        var updated = new ShortcutBinding
        {
            Action = _originalBinding.Action,
            KeyGesture = NewGesture,
            Description = _originalBinding.Description,
            IsEnabled = _originalBinding.IsEnabled
        };

        _shortcutService.RegisterBinding(_profileType, updated);
        RequestClose?.Invoke(true);
    }
}
