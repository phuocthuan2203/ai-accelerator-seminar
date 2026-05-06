# Sprint 1 — Verification Plan

## Sprint: User Registration & Login (UC-1, UC-2)
## Flows under test: BF1 (Register), BF2 (Login)

---

## Prerequisites

- [ ] Backend running at `http://localhost:5000` (health check OK: `curl http://localhost:5000/api/health`)
- [ ] Frontend accessible at `http://localhost:5000/pages/register.html` and `http://localhost:5000/pages/login.html`
- [ ] SQLite database created (`backend/tool_lending.db` exists)
- [ ] All backend tests passing: `dotnet test`
- [ ] All frontend files in place (no 404 errors)

---

## Critical Rule Coverage

Rules that must not drift or fail (flagged for high-risk verification):

| Rule ID | Exact Rule | Why High Risk | Covered in |
|---------|-----------|---------------|-----------|
| R-01 | Passwords stored using bcrypt, not plaintext | Security vulnerability if failed | Agent + Human |
| R-02 | Use constant-time password comparison to prevent timing attacks | Timing attack vulnerability | Agent |
| R-03 | Username must be unique; reject duplicates with 400 | Data integrity / business logic | Agent + Human |
| R-04 | Successful registration auto-logs in user (201 Created, session cookie set) | Core flow requirement | Human |
| R-05 | Generic error "Invalid username or password" on login failure | Security: prevent username enumeration | Agent + Human |
| R-06 | Frontend input validation before submit (immediate feedback) | UX + data quality | Human |
| R-07 | Session cookie is HTTP-only, secure, 30-min timeout | Session security | Agent |
| R-08 | All domain/application exceptions mapped to HTTP responses (400, 401) | API contract enforcement | Agent |

---

# Part 1 — Agent-Executed Verification

Automated checks that validate cross-task integration and correctness.

### A-BUILD-01 — Project builds without errors

- **Action:** `cd backend && dotnet build`
- **Expected:** Exit code 0, no errors or warnings related to code
- **What this catches:** Compilation errors, missing dependencies, configuration issues

### A-TEST-BACKEND-01 — Backend unit + integration tests pass

- **Action:** `dotnet test`
- **Expected:** All tests pass (≥35 tests across 6 test files)
  - AuthenticationServiceTests: 10+ tests
  - PasswordHasherTests: 5+ tests
  - UserRepositoryTests: 7+ tests
  - UserTests: 5+ tests
  - AuthControllerTests: 7+ tests
- **What this catches:** Business logic errors, persistence bugs, API contract violations

### A-TEST-AUTH-RULES-01 — Password hashing uses bcrypt

- **Action:** 
  ```bash
  grep -r "BCrypt" backend/Application/Services/PasswordHasher.cs
  dotnet test tests/Application/PasswordHasherTests.cs
  ```
- **Expected:** "BCrypt" mentioned in code, tests verify hash format (starts with `$2a$`)
- **What this catches:** Plaintext password storage, weak hashing algorithm (Rule R-01)

### A-TEST-AUTH-RULES-02 — Constant-time password verification

- **Action:**
  ```bash
  grep -r "bcrypt.Verify" backend/
  dotnet test tests/Application/PasswordHasherTests.cs -k "VerifyAsync"
  ```
- **Expected:** Code uses `BCrypt.Verify()`, tests verify both correct and incorrect passwords return expected results
- **What this catches:** Timing attack vulnerability (Rule R-02)

### A-TEST-UNIQUENESS-01 — Username uniqueness enforced

- **Action:** `dotnet test tests/Infrastructure/UserRepositoryTests.cs -k "Duplicate"`
- **Expected:** Test "SaveAsync_DuplicateUsername_ThrowsException" passes
- **What this catches:** Duplicate usernames allowed in database (Rule R-03)

### A-TEST-EXCEPTIONS-01 — Exceptions mapped to HTTP codes

- **Action:**
  ```bash
  grep -A 5 "DuplicateUsernameException" backend/Api/Filters/AuthExceptionFilter.cs
  grep -A 5 "AuthenticationException" backend/Api/Filters/AuthExceptionFilter.cs
  dotnet test tests/Api/AuthControllerTests.cs
  ```
- **Expected:** Exception filter maps exceptions to 400/401, controller tests verify HTTP codes
- **What this catches:** Exceptions leaking to client as 500, incorrect HTTP codes (Rule R-08)

### A-INT-REGISTER-FLOW-01 — Registration endpoint 201 on success

- **Action:** `dotnet test tests/Api/AuthControllerTests.cs -k "RegisterAsync_ValidInput_Returns201Created"`
- **Expected:** Test passes, endpoint returns 201 Created with userId, username, message
- **What this catches:** Wrong HTTP codes, missing response fields (Rule R-04)

### A-INT-LOGIN-FLOW-01 — Login endpoint 200 on success, 401 on failure

- **Action:**
  ```bash
  dotnet test tests/Api/AuthControllerTests.cs -k "LoginAsync"
  ```
- **Expected:** Happy path returns 200, wrong password returns 401 with generic message
- **What this catches:** Wrong HTTP codes, non-generic error messages (Rule R-05)

### A-SESSION-01 — Session middleware configured with security flags

- **Action:**
  ```bash
  grep -A 5 "AddSession" backend/Program.cs
  grep "HttpOnly" backend/Program.cs
  grep "SecurePolicy" backend/Program.cs
  ```
- **Expected:** Session middleware configured with `HttpOnly = true`, `SecurePolicy = Always`, 30-min timeout
- **What this catches:** Insecure session cookies (Rule R-07)

### A-DB-SCHEMA-01 — Users table has correct schema

- **Action:**
  ```bash
  cd backend && dotnet ef database update
  sqlite3 tool_lending.db ".schema Users"
  ```
- **Expected:** Table has columns: id (PK), username (UNIQUE NOT NULL), password_hash (NOT NULL), created_at, updated_at
- **What this catches:** Missing constraints, schema misalignment (Rule R-03)

### A-CORS-01 — CORS configured for local development

- **Action:**
  ```bash
  curl -H "Origin: http://localhost:3000" -H "Access-Control-Request-Method: POST" \
       -H "Access-Control-Request-Headers: Content-Type" \
       -X OPTIONS http://localhost:5000/api/auth/register -v
  ```
- **Expected:** Response includes `Access-Control-Allow-Origin: *` (for dev)
- **What this catches:** Frontend cannot reach API due to CORS

---

## Agent Verification Checklist

| Test ID | Description | Result | Notes |
|---------|-------------|--------|-------|
| A-BUILD-01 | Backend builds without errors | ⬜ | |
| A-TEST-BACKEND-01 | All backend tests pass (35+) | ⬜ | |
| A-TEST-AUTH-RULES-01 | Password hashing uses bcrypt | ⬜ | |
| A-TEST-AUTH-RULES-02 | Constant-time password verification | ⬜ | |
| A-TEST-UNIQUENESS-01 | Username uniqueness enforced | ⬜ | |
| A-TEST-EXCEPTIONS-01 | Exceptions mapped to HTTP codes | ⬜ | |
| A-INT-REGISTER-FLOW-01 | Register endpoint 201 on success | ⬜ | |
| A-INT-LOGIN-FLOW-01 | Login endpoint 200/401 | ⬜ | |
| A-SESSION-01 | Session middleware secure | ⬜ | |
| A-DB-SCHEMA-01 | Users table correct schema | ⬜ | |
| A-CORS-01 | CORS configured | ⬜ | |

---

# Part 2 — Human-Executed Verification

Manual testing of user-facing flows and UI/UX.

### H-FLOW-REGISTER-01: User Registration Happy Path

**Precondition:** 
- Backend running at `http://localhost:5000`
- Browser open to `http://localhost:5000/pages/register.html`

**Steps:**
1. Enter username `testuser_2026` in username field
2. Enter password `SecurePass123` in password field
3. Observe validation messages (should be green checkmarks or success indicators)
4. Observe submit button is **enabled** (not greyed out)
5. Click "Create Account" button
6. Observe spinner/loading indicator shows
7. Wait for redirect to dashboard page
8. Verify browser URL is now `/pages/dashboard.html`
9. Verify welcome message shows "Welcome, testuser_2026!"

**Expected:**
- Form validates correctly
- Button enabled when valid
- Successful registration → redirect
- Dashboard shows username

**Not expected:**
- Any console errors (F12 → Console)
- Plaintext password visible in network requests (F12 → Network)
- Redirect to login page (should go to dashboard)

**Sign-off:**
- [ ] Registration successful, redirect works
- [ ] No console errors
- [ ] Username displayed on dashboard

---

### H-FLOW-REGISTER-DUPLICATE: Registration with Duplicate Username

**Precondition:**
- User `testuser_2026` already registered (from H-FLOW-REGISTER-01)
- Browser at `http://localhost:5000/pages/register.html`

**Steps:**
1. Enter username `testuser_2026` (same as first registration)
2. Enter password `AnotherPass123` (different password)
3. Click "Create Account"
4. Observe error message appears
5. Verify error message contains "already taken" or similar (not generic "error")
6. Verify form remains enabled (can retry)
7. Verify no redirect happens

**Expected:**
- Error message displayed
- Form re-enabled for retry
- No redirect

**Not expected:**
- 500 Internal Server Error
- Form disabled
- Successful registration

**Sign-off:**
- [ ] Duplicate username rejected with clear message
- [ ] Form remains usable

---

### H-FLOW-REGISTER-INVALID-INPUTS: Validation Messages

**Precondition:**
- Browser at `http://localhost:5000/pages/register.html`
- Form is fresh/empty

**Steps:**
1. Type "ab" in username field (too short)
2. Observe error message appears in real-time (Rule R-06)
3. Type "securepass" in password field (no uppercase, no digit)
4. Observe error message appears for password (Rule R-06)
5. Continue typing to fix password: "SecurePass1"
6. Observe error message disappears, success indicator shows (green)
7. Verify submit button becomes **enabled** once all errors cleared

**Expected:**
- Real-time validation feedback
- Error messages clear when input fixed
- Submit button disabled → enabled as form becomes valid

**Not expected:**
- Errors only shown on submit attempt
- Submit button always enabled
- Vague error messages (should say "at least X characters", etc.)

**Sign-off:**
- [ ] Real-time validation works
- [ ] Button state follows form validity

---

### H-FLOW-LOGIN-01: User Login Happy Path

**Precondition:**
- User `testuser_2026` registered (from H-FLOW-REGISTER-01)
- Browser at `http://localhost:5000/pages/login.html`
- Session cleared (or open in new incognito window)

**Steps:**
1. Enter username `testuser_2026`
2. Enter password `SecurePass123`
3. Click "Log In"
4. Observe spinner/loading indicator
5. Wait for redirect
6. Verify URL is now `/pages/dashboard.html`
7. Verify welcome message shows "Welcome, testuser_2026!"

**Expected:**
- Successful login → redirect to dashboard
- Username displayed on dashboard

**Not expected:**
- 401 Unauthorized error
- Redirect to register page
- Error message

**Sign-off:**
- [ ] Login successful, redirect works
- [ ] Username displayed

---

### H-FLOW-LOGIN-WRONG-PASSWORD: Login with Wrong Password

**Precondition:**
- User `testuser_2026` registered
- Browser at `http://localhost:5000/pages/login.html`

**Steps:**
1. Enter username `testuser_2026`
2. Enter password `WrongPass123` (incorrect)
3. Click "Log In"
4. Observe error message appears
5. Verify error message says **generic** "Invalid username or password" (NOT "Password incorrect" or "User not found")
6. Verify form remains enabled
7. Verify no redirect

**Expected:**
- Generic error message (prevents username enumeration)
- Form re-enabled for retry
- No redirect

**Not expected:**
- Specific error like "Password incorrect"
- Error like "User not found"
- Form disabled
- 500 error

**Sign-off:**
- [ ] Generic error message shown
- [ ] Form re-enabled

---

### H-FLOW-LOGIN-NONEXISTENT: Login with Nonexistent User

**Precondition:**
- Browser at `http://localhost:5000/pages/login.html`

**Steps:**
1. Enter username `nonexistentuser99`
2. Enter password `SomePass123`
3. Click "Log In"
4. Observe error message
5. Verify error is same generic message as H-FLOW-LOGIN-WRONG-PASSWORD

**Expected:**
- Same generic error message (prevents username enumeration)

**Not expected:**
- Different error message than wrong-password case

**Sign-off:**
- [ ] Generic error, no user enumeration

---

### H-FLOW-SESSION-PERSISTENCE: Session Persists After Page Reload

**Precondition:**
- User logged in (from H-FLOW-LOGIN-01)
- Browser at dashboard page

**Steps:**
1. Verify username is displayed
2. Press F5 or Cmd+R to reload page
3. Observe page reloads and **still shows username** (no redirect to login)
4. Session persisted across reload

**Expected:**
- Page reloads without logout
- Username still displayed
- Session data loaded from localStorage

**Not expected:**
- Redirect to login after refresh
- Session lost

**Sign-off:**
- [ ] Session persists across page reload

---

### H-FLOW-LOGOUT: User Can Logout

**Precondition:**
- User logged in (dashboard page)

**Steps:**
1. Click "Logout" link in navbar
2. Observe redirect to login page
3. Reload page (F5)
4. Verify page does NOT redirect back to dashboard
5. Verify login form is shown

**Expected:**
- Logout clears session
- Subsequent navigations require login

**Not expected:**
- Redirect back to dashboard

**Sign-off:**
- [ ] Logout clears session
- [ ] User must re-login to access dashboard

---

### H-SECURITY-01: Password Input Masked

**Precondition:**
- Browser at register or login page

**Steps:**
1. Click on password input field
2. Type a password (e.g., "SecurePass123")
3. Observe password is **masked** (shows dots/asterisks, not plaintext)
4. Verify F12 → Elements shows `<input type="password" ...>`

**Expected:**
- Password masked in UI
- HTML shows `type="password"`

**Not expected:**
- Plaintext password visible
- `type="text"` in HTML

**Sign-off:**
- [ ] Password properly masked

---

### H-SECURITY-02: No Plaintext Passwords in Network Traffic

**Precondition:**
- Browser F12 Developer Tools open (Network tab)
- Register or login about to be performed

**Steps:**
1. Open F12 → Network tab
2. Register or login
3. Find the POST request to `/api/auth/register` or `/api/auth/login`
4. Click on it and view the request body (Request tab)
5. Verify request body shows `password: "SecurePass123"` (plaintext is expected here)
6. Click on response body
7. Verify response does NOT contain plaintext password or hash
8. Verify response contains only: userId, username, message (no password/hash)

**Expected:**
- Request body has plaintext (normal for HTTPS POST)
- Response does NOT expose password/hash

**Not expected:**
- Response contains plaintext password
- Response contains bcrypt hash (should only be server-side)

**Sign-off:**
- [ ] Response payloads secure (no password exposure)

---

## Human Verification Checklist

| Test ID | Description | Result | Notes |
|---------|-------------|--------|-------|
| H-FLOW-REGISTER-01 | Register happy path → dashboard | ⬜ | |
| H-FLOW-REGISTER-DUPLICATE | Duplicate username error | ⬜ | |
| H-FLOW-REGISTER-INVALID-INPUTS | Validation messages real-time | ⬜ | |
| H-FLOW-LOGIN-01 | Login happy path → dashboard | ⬜ | |
| H-FLOW-LOGIN-WRONG-PASSWORD | Wrong password → generic error | ⬜ | |
| H-FLOW-LOGIN-NONEXISTENT | Nonexistent user → generic error | ⬜ | |
| H-FLOW-SESSION-PERSISTENCE | Session persists after reload | ⬜ | |
| H-FLOW-LOGOUT | Logout clears session | ⬜ | |
| H-SECURITY-01 | Password input masked | ⬜ | |
| H-SECURITY-02 | No plaintext in response | ⬜ | |

---

## Sprint Sign-Off

Sprint 1 is shippable when:

- [ ] **Part 1: Agent-Executed Verification**
  - [ ] All 11 agent checks pass (green ✅)
  - [ ] Backend tests: 35+ tests passing
  - [ ] No compilation errors or warnings
  - [ ] Database schema correct
  
- [ ] **Part 2: Human-Executed Verification**
  - [ ] All 10 human flows tested and pass (green ✅)
  - [ ] No console errors (F12)
  - [ ] No security issues (plaintext passwords exposed, etc.)
  - [ ] Session persistence works
  
- [ ] **No open items**
  - [ ] PROGRESS.md Issues Log is empty or all issues resolved
  - [ ] All 7 tasks (TASK-00 through TASK-06) marked ✅ Done
  - [ ] No deviations from design unresolved
  
- [ ] **Every critical rule verified**
  - [ ] R-01: Bcrypt hashing implemented and tested
  - [ ] R-02: Constant-time comparison verified
  - [ ] R-03: Username uniqueness enforced
  - [ ] R-04: Registration returns 201 with session
  - [ ] R-05: Generic error messages on auth failure
  - [ ] R-06: Frontend validation real-time
  - [ ] R-07: Session cookies secure
  - [ ] R-08: Exceptions mapped to HTTP codes

---

## Sign-Off by Role

**Development Lead / QA:**
- [ ] Reviewed agent verification results
- [ ] Reviewed human verification checklist
- [ ] Confirmed all tests pass
- [ ] Approved for merge to main branch

**Product Owner / Stakeholder:**
- [ ] User registration flow works end-to-end
- [ ] User login flow works end-to-end
- [ ] Errors are clear and actionable
- [ ] No obvious UX issues
- [ ] Approved for demo/release

---

## Rollback Plan

If Sprint 1 verification fails and cannot be fixed quickly:

1. **Revert all changes** to last known good state (or previous sprint branch)
2. **Review failures** in PROGRESS.md Issues Log
3. **Log issues** with specific evidence (test output, step-by-step reproduction)
4. **Schedule fix sprint** to address critical issues before next attempt
5. **Do not merge** incomplete/broken code to main branch

---
