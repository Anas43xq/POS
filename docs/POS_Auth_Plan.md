# WFP POS — Auth Optimization Plan

## Current Pain Points

| Issue | Root Cause | Time Cost |
|---|---|---|
| Login warmup 3.6s | Warmup only heats EF pool, not the raw ADO.NET pool `UserRepository` uses for `SP_LoginUser` | Startup only, user doesn't wait |
| Auth total 1404ms | bcrypt factor 11 + cold JIT on first run | User waits every login |
| User lookup 208ms | Cold ADO.NET connection on first hit | Folds into above |

---

## Phase 1 — Quick Wins
> Fix what exists, no new features

- [ ] Fix `WarmUpLoginInfrastructureAsync` to also prime the raw `SqlConnection` pool used by `SP_LoginUser`
- [ ] Pre-warm bcrypt JIT at startup with a dummy `BCrypt.Verify` call
- [ ] Drop work factor `11` → `10`, add transparent re-hash on next successful login

**Expected result:** auth `~1400ms` → `~350–600ms` with zero UX or architecture changes.

---

## Phase 2 — PIN for Override Prompts
> Build when void / discount / shift override flows are added

- [ ] Add `PinHash` column to `Users` table
- [ ] Hash with **Argon2id** at low cost (fast for UX, not instant to brute force)
- [ ] Void / override popup shows a **numpad overlay**, manager enters 4-digit PIN
- [ ] Verify against `PinHash` — no DB hit (hash cached in `IManagerSessionCache` after login)
- [ ] Wire up `IManagerSessionCache` singleton — this is when the cache earns its place

---

## Phase 3 — Windows Hello for Initial Login
> Replaces password + bcrypt for the happy path entirely

- [ ] Manager enrolls once via Windows Hello
- [ ] Login screen gets a "Sign in with Windows Hello" button
- [ ] Uses `UserConsentVerifier` WinRT API (via `Microsoft.Windows.SDK.Contracts`)
- [ ] Keep username/password as fallback if Hello unavailable or not enrolled
- [ ] bcrypt cost at login becomes irrelevant for normal operation

---

## Architecture Boundary

| Client | Auth Mechanism | Token / Session Type |
|---|---|---|
| Expo mobile | JWT access token + refresh token (existing) | DB-backed `RefreshToken` table |
| WPF POS | Windows Hello → session in `ISessionService` | In-memory only |
| WPF override prompts | PIN via `IManagerSessionCache` | In-memory hash cache |

> Mobile token flow and WPF session stay fully separate — no cross-contamination.

---

## Build Order

```
Phase 1 (now)   → Fix warmup + bcrypt JIT pre-warm + work factor 10
Phase 2 (next)  → PIN column + void override popup + ManagerSessionCache  
Phase 3 (after) → Windows Hello initial login
```
