using BLL.Interfaces;
using Contracts.Shifts;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.Commands;

namespace UI.ViewModels
{
    public class ShiftManagementViewModel : BaseViewModel
    {
        private readonly IShiftManagementService _shiftManagementService;

        private readonly ObservableCollection<ShiftManagementDto> _shifts = new();
        private bool _isLoaded;

        public ShiftManagementViewModel(IShiftManagementService shiftManagementService)
        {
            _shiftManagementService = shiftManagementService;
            Shifts = _shifts;
            RefreshCommand = new AsyncRelayCommand(()=>RefreshAsync());
        }

        public ObservableCollection<ShiftManagementDto> Shifts { get; }
        public bool HasData => Shifts.Count > 0;
        public ICommand RefreshCommand { get; }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value)
                    return;

                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public async Task LoadAsync()
        {
            if (_isLoaded)
                return;

            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                var items = await _shiftManagementService.GetAllShiftManagementAsync();

                Shifts.Clear();
                foreach (var item in items)
                {
                    Shifts.Add(item);
                }
                OnPropertyChanged(nameof(HasData));
                _isLoaded = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task RefreshAsync() =>  await LoadAsync();
    }
}
