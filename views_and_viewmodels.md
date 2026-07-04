# PointOfSale Application - Views and ViewModels

This document lists all Views and ViewModels in the PointOfSale WPF application.

## Views

WPF View files (.xaml and .xaml.cs) located in `PointOfSale/Views/`:

### Main Views
- `MainWindow.xaml` / `MainWindow.xaml.cs` - Main application window
- `HomeView.xaml` / `HomeView.xaml.cs` - Home dashboard view
- `CashierDashboardView.xaml` / `CashierDashboardView.xaml.cs` - Cashier dashboard
- `ManagerMainView.xaml` / `ManagerMainView.xaml.cs` - Manager main view

### Authentication
- `loginView.xaml` / `loginView.xaml.cs` - Login screen

### Product Management
- `ProductManagementView.xaml` / `ProductManagementView.xaml.cs` - Product list/management
- `ProductFormView.xaml` / `ProductFormView.xaml.cs` - Product add/edit form
- `ProductFormWindow.xaml` / `ProductFormWindow.xaml.cs` - Product form window

### Category Management
- `CategoryManagementView.xaml` / `CategoryManagementView.xaml.cs` - Category management view
- `AddEditCategoryDialog.xaml` / `AddEditCategoryDialog.xaml.cs` - Add/Edit category dialog

### Sales and Transactions
- `RecentTransactionView.xaml` / `RecentTransactionView.xaml.cs` - Recent transactions view
- `TransactionsView.xaml` / `TransactionsView.xaml.cs` - Transactions list view
- `PaymentDialog.xaml` / `PaymentDialog.xaml.cs` - Payment processing dialog
- `CardPaymentConfirmDialog.xaml` / `CardPaymentConfirmDialog.xaml.cs` - Card payment confirmation
- `ReceiptWindow.xaml` / `ReceiptWindow.xaml.cs` - Receipt display window
- `ReceiptPrintView.xaml` / `ReceiptPrintView.xaml.cs` - Receipt print preview
- `RecentSalesDialog.xaml` / `RecentSalesDialog.xaml.cs` - Recent sales dialog

### Shift Management
- `StartDayDialog.xaml` / `StartDayDialog.xaml.cs` - Start shift dialog
- `EndDayDialog.xaml` / `EndDayDialog.xaml.cs` - End shift dialog
- `ShiftManagementView.xaml` / `ShiftManagementView.xaml.cs` - Shift management view

### Reports
- `ReportView.xaml` / `ReportView.xaml.cs` - Reports view

### Total: 21 Views

---

## ViewModels

C# ViewModel classes located in `PointOfSale/ViewModels/`:

### Core ViewModels
- `BaseViewModel.cs` - Base class implementing INotifyPropertyChanged
- `MainViewModel.cs` - Main application ViewModel
- `HomeViewModel.cs` - Home dashboard ViewModel
- `LoginViewModel.cs` - Login ViewModel

### Dashboard ViewModels
- `CashierDashboardViewModel.cs` - Cashier dashboard logic
- `ManagerMainViewModel.cs` - Manager main dashboard logic

### Product Management ViewModels
- `ProductManagementViewModel.cs` - Product list management
- `ProductViewModel.cs` - Individual product data model/ViewModel
- `CartItemViewModel.cs` - Shopping cart item ViewModel

### Category Management ViewModels
- `CategoryManagementViewModel.cs` - Category management logic
- `AddEditCategoryViewModel.cs` - Add/Edit category dialog logic
- `CategoryCardViewModel.cs` - Category card display
- `SubcategoryCardViewModel.cs` - Subcategory card display

### Transaction/Sales ViewModels
- `RecentTransactionViewModel.cs` - Recent transactions logic (alias: ReceiptViewModel.cs)
- `TransactionsViewModel.cs` - Transactions list logic
- `ReceiptViewModel.cs` - Receipt display logic

### Payment ViewModels
- `PaymentDialogViewModel.cs` - Payment dialog logic
- `CardPaymentConfirmDialogViewModel.cs` - Card payment confirmation logic

### Shift Management ViewModels
- `StartDayDialogViewModel.cs` - Start day/shift dialog logic
- `EndDayDialogViewModel.cs` - End day/shift dialog logic
- `ShiftManagementViewModel.cs` - Shift management logic

### Report ViewModels
- `ReportViewModel.cs` - Reports generation/display logic

### Dialog ViewModels
- `RecentSalesDialogViewModel.cs` - Recent sales dialog logic

### Total: 22 ViewModels

---

## View-ViewModel Correspondence

| View | ViewModel |
|------|-----------|
| MainWindow | MainViewModel |
| HomeView | HomeViewModel |
| CashierDashboardView | CashierDashboardViewModel |
| ManagerMainView | ManagerMainViewModel |
| loginView | LoginViewModel |
| ProductManagementView | ProductManagementViewModel |
| ProductFormView | ProductViewModel |
| ProductFormWindow | ProductViewModel |
| CategoryManagementView | CategoryManagementViewModel |
| AddEditCategoryDialog | AddEditCategoryViewModel |
| RecentTransactionView | ReceiptViewModel |
| TransactionsView | TransactionsViewModel |
| PaymentDialog | PaymentDialogViewModel |
| CardPaymentConfirmDialog | CardPaymentConfirmDialogViewModel |
| ReceiptWindow | ReceiptViewModel |
| ReceiptPrintView | ReceiptViewModel |
| RecentSalesDialog | RecentSalesDialogViewModel |
| StartDayDialog | StartDayDialogViewModel |
| EndDayDialog | EndDayDialogViewModel |
| ShiftManagementView | ShiftManagementViewModel |
| ReportView | ReportViewModel |

---

Generated on: 2026-06-02