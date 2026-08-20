# Login Process Flowchart

Current-state login flow for the POS app after the August 17, 2026 login refactor.

## Summary

- Startup opens a single `LoginWindow`.
- Cashier login uses the default cashier lookup path.
- Manager login uses `SP_LoginUser` through raw ADO, then BCrypt verification.
- Successful shell-open happens before shift hydration finishes.
- `Session` table inserts are no longer part of the login critical path.

## Flowchart

```mermaid
flowchart TD
    A["App startup"] --> B["Read DefaultConnection<br/>with Min Pool Size=5"]
    B --> C["Configure DI and DAL"]
    C --> D["ApplicationShellService.Start()"]
    D --> E["Show LoginWindow"]

    E --> F{"User selects role"}

    F -->|Cashier| G["LoginWindowViewModel.LoginAsCashierAsync()"]
    G --> H["UserService.GetDefaultCashierAsync()"]
    H --> I{"Cashier found?"}
    I -->|No| J["Show cashier error in LoginWindow"]
    I -->|Yes| K["Set SessionService.CurrentUser"]
    K --> L["Set CurrentShift = null"]
    L --> M["Raise LoginSucceeded"]
    M --> N["ApplicationShellService.OpenMainWindow()"]
    N --> O["Show MainWindow"]
    O --> P["Close LoginWindow"]
    P --> Q["HydrateCashierShiftAfterShellOpenAsync()"]
    Q --> R["TryLoadCurrentShiftAsync()"]
    R --> S{"Open shift found?"}
    S -->|Yes| T["Set SessionService.CurrentShift"]
    S -->|No| U["Keep null and let dashboard prompt"]
    T --> V["Refresh live cashier dashboard"]
    U --> V

    F -->|Manager| W["Switch LoginWindow to manager mode"]
    W --> X["Lazy-load remembered username from registry"]
    X --> Y["User enters username + password"]
    Y --> Z["LoginWindowViewModel.LoginAsManagerAsync()"]
    Z --> AA["AuthService.LoginAsync()"]
    AA --> AB["UserRepository.GetByUsernameAsync()"]
    AB --> AC["Open SQL connection"]
    AC --> AD["Execute dbo.SP_LoginUser(@Username)"]
    AD --> AE{"User returned?"}
    AE -->|No| AF["Return invalid username/password"]
    AF --> AG["Show manager error in LoginWindow"]
    AE -->|Yes| AH["Check IsActive"]
    AH -->|Inactive| AI["Return account deactivated"]
    AI --> AG
    AH -->|Active| AJ["BCrypt.Verify(...) on background thread"]
    AJ --> AK{"Password valid?"}
    AK -->|No| AL["Return invalid username/password"]
    AL --> AG
    AK -->|Yes| AM{"Role = Manager?"}
    AM -->|No| AN["Show non-manager rejection"]
    AN --> AO["Offer switch to cashier path"]
    AO --> G
    AM -->|Yes| AP["Save remembered username"]
    AP --> AQ["Set SessionService.CurrentUser"]
    AQ --> AR["Set CurrentShift = null"]
    AR --> AS["Raise LoginSucceeded"]
    AS --> N
    AS --> AT["TryLoadCurrentShiftAsync() in background"]
    AT --> AU{"Open shift found?"}
    AU -->|Yes| AV["Set SessionService.CurrentShift"]
    AU -->|No| AW["Keep null"]

    J --> E
    AG --> E
```

## Notes

- `SP_LoginUser` returns the user record plus role name in one round trip.
- `BCrypt.Verify` still remains part of the manager critical path.
- `Min Pool Size=5` helps avoid paying full connection setup cost on the first few logins after process start.
- Logout returns the app to the same `LoginWindow` flow.
