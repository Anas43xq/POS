# Reorganize Views and ViewModels into structured folders
Set-Location PointOfSale

# Create View subdirectories
$viewDirs = @("Views\Main", "Views\Auth", "Views\Products", "Views\Categories", "Views\Sales", "Views\Shifts", "Views\Reports")
foreach ($dir in $viewDirs) {
    if (-not (Test-Path $dir)) {
        New-Item -Path $dir -ItemType Directory -Force | Out-Null
    }
}

# Create ViewModel subdirectories
$vmDirs = @("ViewModels\Core", "ViewModels\Dashboards", "ViewModels\Products", "ViewModels\Categories", "ViewModels\Transactions", "ViewModels\Payments", "ViewModels\Shifts", "ViewModels\Reports", "ViewModels\Dialogs")
foreach ($dir in $vmDirs) {
    if (-not (Test-Path $dir)) {
        New-Item -Path $dir -ItemType Directory -Force | Out-Null
    }
}

# Move Views - Main
if (Test-Path "Views\MainWindow.xaml") { Move-Item "Views\MainWindow.xaml" "Views\Main\" -Force }
if (Test-Path "Views\MainWindow.xaml.cs") { Move-Item "Views\MainWindow.xaml.cs" "Views\Main\" -Force }
if (Test-Path "Views\HomeView.xaml") { Move-Item "Views\HomeView.xaml" "Views\Main\" -Force }
if (Test-Path "Views\HomeView.xaml.cs") { Move-Item "Views\HomeView.xaml.cs" "Views\Main\" -Force }
if (Test-Path "Views\CashierDashboardView.xaml") { Move-Item "Views\CashierDashboardView.xaml" "Views\Main\" -Force }
if (Test-Path "Views\CashierDashboardView.xaml.cs") { Move-Item "Views\CashierDashboardView.xaml.cs" "Views\Main\" -Force }
if (Test-Path "Views\ManagerMainView.xaml") { Move-Item "Views\ManagerMainView.xaml" "Views\Main\" -Force }
if (Test-Path "Views\ManagerMainView.xaml.cs") { Move-Item "Views\ManagerMainView.xaml.cs" "Views\Main\" -Force }

# Move Views - Auth
if (Test-Path "Views\loginView.xaml") { Move-Item "Views\loginView.xaml" "Views\Auth\" -Force }
if (Test-Path "Views\loginView.xaml.cs") { Move-Item "Views\loginView.xaml.cs" "Views\Auth\" -Force }

# Move Views - Products
if (Test-Path "Views\ProductManagementView.xaml") { Move-Item "Views\ProductManagementView.xaml" "Views\Products\" -Force }
if (Test-Path "Views\ProductManagementView.xaml.cs") { Move-Item "Views\ProductManagementView.xaml.cs" "Views\Products\" -Force }
if (Test-Path "Views\ProductFormView.xaml") { Move-Item "Views\ProductFormView.xaml" "Views\Products\" -Force }
if (Test-Path "Views\ProductFormView.xaml.cs") { Move-Item "Views\ProductFormView.xaml.cs" "Views\Products\" -Force }
if (Test-Path "Views\ProductFormWindow.xaml") { Move-Item "Views\ProductFormWindow.xaml" "Views\Products\" -Force }
if (Test-Path "Views\ProductFormWindow.xaml.cs") { Move-Item "Views\ProductFormWindow.xaml.cs" "Views\Products\" -Force }

# Move Views - Categories
if (Test-Path "Views\CategoryManagementView.xaml") { Move-Item "Views\CategoryManagementView.xaml" "Views\Categories\" -Force }
if (Test-Path "Views\CategoryManagementView.xaml.cs") { Move-Item "Views\CategoryManagementView.xaml.cs" "Views\Categories\" -Force }
if (Test-Path "Views\AddEditCategoryDialog.xaml") { Move-Item "Views\AddEditCategoryDialog.xaml" "Views\Categories\" -Force }
if (Test-Path "Views\AddEditCategoryDialog.xaml.cs") { Move-Item "Views\AddEditCategoryDialog.xaml.cs" "Views\Categories\" -Force }

# Move Views - Sales
if (Test-Path "Views\RecentTransactionView.xaml") { Move-Item "Views\RecentTransactionView.xaml" "Views\Sales\" -Force }
if (Test-Path "Views\RecentTransactionView.xaml.cs") { Move-Item "Views\RecentTransactionView.xaml.cs" "Views\Sales\" -Force }
if (Test-Path "Views\TransactionsView.xaml") { Move-Item "Views\TransactionsView.xaml" "Views\Sales\" -Force }
if (Test-Path "Views\TransactionsView.xaml.cs") { Move-Item "Views\TransactionsView.xaml.cs" "Views\Sales\" -Force }
if (Test-Path "Views\PaymentDialog.xaml") { Move-Item "Views\PaymentDialog.xaml" "Views\Sales\" -Force }
if (Test-Path "Views\PaymentDialog.xaml.cs") { Move-Item "Views\PaymentDialog.xaml.cs" "Views\Sales\" -Force }
if (Test-Path "Views\CardPaymentConfirmDialog.xaml") { Move-Item "Views\CardPaymentConfirmDialog.xaml" "Views\Sales\" -Force }
if (Test-Path "Views\CardPaymentConfirmDialog.xaml.cs") { Move-Item "Views\CardPaymentConfirmDialog.xaml.cs" "Views\Sales\" -Force }
if (Test-Path "Views\ReceiptWindow.xaml") { Move-Item "Views\ReceiptWindow.xaml" "Views\Sales\" -Force }
if (Test-Path "Views\ReceiptWindow.xaml.cs") { Move-Item "Views\ReceiptWindow.xaml.cs" "Views\Sales\" -Force }
if (Test-Path "Views\ReceiptPrintView.xaml") { Move-Item "Views\ReceiptPrintView.xaml" "Views\Sales\" -Force }
if (Test-Path "Views\ReceiptPrintView.xaml.cs") { Move-Item "Views\ReceiptPrintView.xaml.cs" "Views\Sales\" -Force }
if (Test-Path "Views\RecentSalesDialog.xaml") { Move-Item "Views\RecentSalesDialog.xaml" "Views\Sales\" -Force }
if (Test-Path "Views\RecentSalesDialog.xaml.cs") { Move-Item "Views\RecentSalesDialog.xaml.cs" "Views\Sales\" -Force }

# Move Views - Shifts
if (Test-Path "Views\StartDayDialog.xaml") { Move-Item "Views\StartDayDialog.xaml" "Views\Shifts\" -Force }
if (Test-Path "Views\StartDayDialog.xaml.cs") { Move-Item "Views\StartDayDialog.xaml.cs" "Views\Shifts\" -Force }
if (Test-Path "Views\EndDayDialog.xaml") { Move-Item "Views\EndDayDialog.xaml" "Views\Shifts\" -Force }
if (Test-Path "Views\EndDayDialog.xaml.cs") { Move-Item "Views\EndDayDialog.xaml.cs" "Views\Shifts\" -Force }
if (Test-Path "Views\ShiftManagementView.xaml") { Move-Item "Views\ShiftManagementView.xaml" "Views\Shifts\" -Force }
if (Test-Path "Views\ShiftManagementView.xaml.cs") { Move-Item "Views\ShiftManagementView.xaml.cs" "Views\Shifts\" -Force }

# Move Views - Reports
if (Test-Path "Views\ReportView.xaml") { Move-Item "Views\ReportView.xaml" "Views\Reports\" -Force }
if (Test-Path "Views\ReportView.xaml.cs") { Move-Item "Views\ReportView.xaml.cs" "Views\Reports\" -Force }

# Move ViewModels - Core
if (Test-Path "ViewModels\BaseViewModel.cs") { Move-Item "ViewModels\BaseViewModel.cs" "ViewModels\Core\" -Force }
if (Test-Path "ViewModels\MainViewModel.cs") { Move-Item "ViewModels\MainViewModel.cs" "ViewModels\Core\" -Force }
if (Test-Path "ViewModels\HomeViewModel.cs") { Move-Item "ViewModels\HomeViewModel.cs" "ViewModels\Core\" -Force }
if (Test-Path "ViewModels\LoginViewModel.cs") { Move-Item "ViewModels\LoginViewModel.cs" "ViewModels\Core\" -Force }

# Move ViewModels - Dashboards
if (Test-Path "ViewModels\CashierDashboardViewModel.cs") { Move-Item "ViewModels\CashierDashboardViewModel.cs" "ViewModels\Dashboards\" -Force }
if (Test-Path "ViewModels\ManagerMainViewModel.cs") { Move-Item "ViewModels\ManagerMainViewModel.cs" "ViewModels\Dashboards\" -Force }

# Move ViewModels - Products
if (Test-Path "ViewModels\ProductManagementViewModel.cs") { Move-Item "ViewModels\ProductManagementViewModel.cs" "ViewModels\Products\" -Force }
if (Test-Path "ViewModels\ProductViewModel.cs") { Move-Item "ViewModels\ProductViewModel.cs" "ViewModels\Products\" -Force }
if (Test-Path "ViewModels\CartItemViewModel.cs") { Move-Item "ViewModels\CartItemViewModel.cs" "ViewModels\Products\" -Force }

# Move ViewModels - Categories
if (Test-Path "ViewModels\CategoryManagementViewModel.cs") { Move-Item "ViewModels\CategoryManagementViewModel.cs" "ViewModels\Categories\" -Force }
if (Test-Path "ViewModels\AddEditCategoryViewModel.cs") { Move-Item "ViewModels\AddEditCategoryViewModel.cs" "ViewModels\Categories\" -Force }
if (Test-Path "ViewModels\CategoryCardViewModel.cs") { Move-Item "ViewModels\CategoryCardViewModel.cs" "ViewModels\Categories\" -Force }
if (Test-Path "ViewModels\SubcategoryCardViewModel.cs") { Move-Item "ViewModels\SubcategoryCardViewModel.cs" "ViewModels\Categories\" -Force }

# Move ViewModels - Transactions
if (Test-Path "ViewModels\RecentTransactionViewModel.cs") { Move-Item "ViewModels\RecentTransactionViewModel.cs" "ViewModels\Transactions\" -Force }
if (Test-Path "ViewModels\TransactionsViewModel.cs") { Move-Item "ViewModels\TransactionsViewModel.cs" "ViewModels\Transactions\" -Force }
if (Test-Path "ViewModels\ReceiptViewModel.cs") { Move-Item "ViewModels\ReceiptViewModel.cs" "ViewModels\Transactions\" -Force }

# Move ViewModels - Payments
if (Test-Path "ViewModels\PaymentDialogViewModel.cs") { Move-Item "ViewModels\PaymentDialogViewModel.cs" "ViewModels\Payments\" -Force }
if (Test-Path "ViewModels\CardPaymentConfirmDialogViewModel.cs") { Move-Item "ViewModels\CardPaymentConfirmDialogViewModel.cs" "ViewModels\Payments\" -Force }

# Move ViewModels - Shifts
if (Test-Path "ViewModels\StartDayDialogViewModel.cs") { Move-Item "ViewModels\StartDayDialogViewModel.cs" "ViewModels\Shifts\" -Force }
if (Test-Path "ViewModels\EndDayDialogViewModel.cs") { Move-Item "ViewModels\EndDayDialogViewModel.cs" "ViewModels\Shifts\" -Force }
if (Test-Path "ViewModels\ShiftManagementViewModel.cs") { Move-Item "ViewModels\ShiftManagementViewModel.cs" "ViewModels\Shifts\" -Force }

# Move ViewModels - Reports
if (Test-Path "ViewModels\ReportViewModel.cs") { Move-Item "ViewModels\ReportViewModel.cs" "ViewModels\Reports\" -Force }

# Move ViewModels - Dialogs
if (Test-Path "ViewModels\RecentSalesDialogViewModel.cs") { Move-Item "ViewModels\RecentSalesDialogViewModel.cs" "ViewModels\Dialogs\" -Force }

Write-Host "Directory reorganization complete!"