using Contracts.Shifts;
using System.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;
using UI.Commands;
using UI.Views;
using UI.Services;

namespace UI.ViewModels
{
    public partial class ShiftManagementViewModel
    {
        public async Task LoadShifts(CancellationToken ct = default)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                if (CurrentFilterMode == ShiftFilterMode.Period)
                {
                    if (!FromDate.HasValue)
                    {
                        _notifications.ShowError("From date is required for a custom period.");
                        return;
                    }

                    if (ToDate.HasValue && ToDate.Value < FromDate.Value)
                    {
                        _notifications.ShowError("To date cannot be earlier than From date.");
                        return;
                    }
                }

                var request = new GetShiftsListRequest
                {
                    PeriodType = CurrentFilterMode switch
                    {
                        ShiftFilterMode.Day => "Today",
                        ShiftFilterMode.Week => "Week",
                        ShiftFilterMode.Month => "Month",
                        ShiftFilterMode.Period => "Custom",
                        _ => "Today"
                    },
                    FromDate = FromDate,
                    ToDate = ToDate,
                    StatusFilter = SelectedShiftStatusFilter?.Value,
                    PageNumber = CurrentPage,
                    PageSize = PageSize
                };

                if (request.PeriodType != "Custom")
                {
                    request.FromDate = null;
                    request.ToDate = null;
                }

                var result = await _shiftManagementService.GetShiftsListAsync(request, ct);

                if (!result.IsSuccess)
                {
                    _notifications.ShowError(result.Error ?? "Failed to load shifts.");
                    return;
                }

                var paged = result.Value!;
                TotalCount = paged.TotalCount;
                CurrentPage = paged.PageNumber;
                PageSize = paged.PageSize;

                Shifts.Clear();
                foreach (var shift in paged.Items)
                {
                    Shifts.Add(shift);
                }

                RefreshView();
            }
            catch (Exception ex)
            {
                _notifications.ShowError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LoadAsync()
        {
            if (_isLoaded)
                return;

            var stopwatch = Stopwatch.StartNew();
            await LoadShifts();
            _isLoaded = true;
            TxpTrace.WriteLine($"[TXP] - Shifts first load completed in {stopwatch.ElapsedMilliseconds} ms");
        }

        public async Task RefreshAsync()
        {
            await LoadShifts();
        }

        private void LoadDay()
        {
            CurrentFilterMode = ShiftFilterMode.Day;
            IsPeriodFilterVisible = false;
            CurrentPage = 1;
            _ = LoadShifts();
        }

        private void LoadWeek()
        {
            CurrentFilterMode = ShiftFilterMode.Week;
            IsPeriodFilterVisible = false;
            CurrentPage = 1;
            _ = LoadShifts();
        }

        private void LoadMonth()
        {
            CurrentFilterMode = ShiftFilterMode.Month;
            IsPeriodFilterVisible = false;
            CurrentPage = 1;
            _ = LoadShifts();
        }

        private void LoadPeriod()
        {
            CurrentFilterMode = ShiftFilterMode.Period;
            IsPeriodFilterVisible = true;
        }

        private void ApplyPeriod()
        {
            if (!FromDate.HasValue)
            {
                _notifications.ShowError("From date is required for a custom period.");
                return;
            }

            CurrentFilterMode = ShiftFilterMode.Period;
            IsPeriodFilterVisible = true;
            CurrentPage = 1;
            _ = LoadShifts();
        }

        private void PreviousPage()
        {
            if (!CanGoPreviousPage)
                return;

            CurrentPage--;
            _ = LoadShifts();
        }

        private void NextPage()
        {
            if (!CanGoNextPage)
                return;

            CurrentPage++;
            _ = LoadShifts();
        }

        private async Task OpenShiftDetailAsync(ShiftListItemDto? shift)
        {
            if (shift == null)
                return;

            try
            {
                var result = await _shiftManagementService.GetShiftDetailAsync(shift.ShiftId);

                if (!result.IsSuccess)
                {
                    _notifications.ShowError(result.Error ?? "Failed to load shift details.");
                    return;
                }

                var viewModel = _viewModelFactory.Create<ShiftDetailViewModel>(result.Value!);
                var dialog = new ShiftDetailView
                {
                    DataContext = viewModel
                };

                var owner = System.Windows.Application.Current?.MainWindow;
                if (owner != null && !ReferenceEquals(owner, dialog))
                    dialog.Owner = owner;

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                _notifications.ShowError(ex.Message);
            }
        }
    }
}
