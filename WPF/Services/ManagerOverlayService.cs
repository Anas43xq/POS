using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;
using UI.ViewModels;
using UI.Views;

namespace UI.Services
{
    public class ManagerOverlayService : IManagerOverlayService
    {
        private readonly IServiceProvider _serviceProvider;

        public ManagerOverlayService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<bool> RequestApprovalAsync(string promptTitle, bool reasonRequired = false)
        {
            var vm = _serviceProvider.GetRequiredService<ManagerPinOverlayViewModel>();
            vm.PromptTitle = promptTitle;
            vm.ReasonRequired = reasonRequired;
            await vm.InitializeAsync();

            var window = new ManagerPinOverlayView
            {
                DataContext = vm,
                Owner = Application.Current?.MainWindow
            };

            window.ShowDialog();

            return await vm.ResultTask;
        }

        public async Task<ManagerApprovalResult> RequestApprovalWithReasonAsync(string promptTitle)
        {
            var vm = _serviceProvider.GetRequiredService<ManagerPinOverlayViewModel>();
            vm.PromptTitle = promptTitle;
            vm.ReasonRequired = false;
            await vm.InitializeAsync();

            var window = new ManagerPinOverlayView
            {
                DataContext = vm,
                Owner = Application.Current?.MainWindow
            };

            window.ShowDialog();

            bool approved = await vm.ResultTask;
            string? reason = await vm.ResultWithReasonTask;
            return new ManagerApprovalResult(approved, approved ? reason : null);
        }
    }
}
