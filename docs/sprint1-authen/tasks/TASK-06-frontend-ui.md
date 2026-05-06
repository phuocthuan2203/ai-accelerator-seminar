# TASK-06 — Frontend UI: Registration & Login Pages + Form Handling

## Context

This task implements the **user-facing UI pages and form handling** for registration and login. It includes:
- `registration.html` page with form and real-time validation
- `login.html` page with form and error display
- Form submission handling that calls AuthService
- Input validation with immediate feedback
- Success/error state management and user feedback
- Navigation and redirect logic

This is the final user-facing layer that ties together the entire Sprint 1 authentication flow.

---

## Design References

| Document | Section / Anchor | Purpose |
|----------|------------------|---------|
| `docs/sprint1-authen/sprint1-design.md` | §6 Wireframe Design | Page layout and states (default, loading, error, success) |
| `docs/sprint1-authen/sprint1-design.md` | §4 Sequence Diagrams BF1, BF2 | Frontend form flow and error handling |
| `docs/sprint1-authen/sprint1-design.md` | §5a Frontend Class Diagram | UI components using AuthService |

---

## Sequence Diagram Reference (MANDATORY — Read Before Any Code)

**Sprint Design Doc section:** §4 — Detailed Sequence Diagrams  
**Relevant flows:** BF1 (Registration), BF2 (Login)

**BF1 Frontend flow:**
```
User -> RegistrationPage: Enters username & password
RegistrationPage -> RegistrationPage: Validate input (frontend)
alt Validation Fails
  RegistrationPage -> User: Display error message (red)
else Validation Passes
  User -> RegistrationPage: Clicks "Register"
  RegistrationPage -> RegistrationPage: Disable form, show spinner
  RegistrationPage -> AuthService: register(username, password)
  AuthService -> API: HTTP POST
  alt Error response (400, 500, etc.)
    AuthService -> RegistrationPage: throw Error
    RegistrationPage -> User: Display error message
    RegistrationPage -> RegistrationPage: Re-enable form
  else Success (201 Created)
    RegistrationPage -> User: Display "Registration successful"
    RegistrationPage -> RegistrationPage: Redirect to dashboard (or login page)
```

---

## Design Rule Checklist

| # | Exact Rule (Verbatim) | Source | Owner Files | Test Name | Verification |
|---|---------------------|--------|-------------|-----------|--------------|
| 1 | "RegistrationPage with form fields: username, password" | Sprint Design §6 Wireframe | `registration.html` | (manual test) | Manual verification |
| 2 | "LoginPage with form fields: username, password" | Sprint Design §6 Wireframe | `login.html` | (manual test) | Manual verification |
| 3 | "Frontend input validation before submit: username 3-50 chars alphanumeric+underscore, password 6+ chars with complexity" | Sprint Design §3 API Contract, SRS §3.1 Purpose | `registration.html`, `registration.js` | (manual test) | Manual verification |
| 4 | "Validation messages displayed in real-time as user types" | Sprint Design §6 Wireframe | Registration/Login page JS | (manual test) | Manual verification |
| 5 | "Submit button disabled until form is valid" | Sprint Design §6 Wireframe | Registration/Login page JS | (manual test) | Manual verification |
| 6 | "On form submit, show loading spinner, disable inputs" | Sprint Design §6 Wireframe (Loading state) | Form submit handler | (manual test) | Manual verification |
| 7 | "On success (201/200), display success message, redirect to dashboard" | Sprint Design §4 Sequence Diagrams | Success state handler | (manual test) | Manual verification |
| 8 | "On error (400/401), display error message, re-enable form" | Sprint Design §4 Sequence Diagrams | Error state handler | (manual test) | Manual verification |
| 9 | "Password input field uses type='password' to hide input" | UI security best practice | `registration.html`, `login.html` | (code review) | Manual verification |
| 10 | "Forms use HTTPS and secure session cookies (HTTP-only)" | Sprint Design §3 Security Notes | (backend, but verify in frontend) | (infrastructure test) | Integration test with backend |

---

## Affected Files

| File Path | Action | Layer / Role | Must-Contain |
|-----------|--------|--------------|-------------|
| `frontend/pages/register.html` | CREATE/MODIFY | HTML page | Form with username, password inputs; validation messages; error/success divs |
| `frontend/pages/login.html` | CREATE/MODIFY | HTML page | Form with username, password inputs; error div; success message |
| `frontend/js/registration.js` | CREATE | Form handler | Input validation, form submission, error/success handling |
| `frontend/js/login.js` | CREATE | Form handler | Input validation, form submission, error/success handling |
| `frontend/pages/dashboard.html` (placeholder) | CREATE | Placeholder page | Redirect target after successful registration/login |

---

## Implementation Skeleton

### `frontend/pages/register.html` (CREATE)

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Register - Tool Lending Platform</title>
    <link rel="stylesheet" href="../css/styles.css">
</head>
<body>
    <div id="app">
        <header>
            <nav class="navbar">
                <h1>Tool Lending Platform</h1>
            </nav>
        </header>

        <main id="main-content">
            <div id="register-page">
                <h2>Create Your Account</h2>

                <!-- Rule #1: Registration form -->
                <form id="register-form" class="auth-form">
                    <!-- Username field -->
                    <div class="form-group">
                        <label for="username">Username *</label>
                        <input 
                            type="text" 
                            id="username" 
                            name="username" 
                            placeholder="3-50 characters, letters, numbers, underscore"
                            required
                        >
                        <!-- Rule #3, #4: Validation message shown in real-time -->
                        <div class="validation-message" id="username-error"></div>
                    </div>

                    <!-- Password field -->
                    <div class="form-group">
                        <label for="password">Password *</label>
                        <!-- Rule #9: Use type="password" to hide input -->
                        <input 
                            type="password" 
                            id="password" 
                            name="password" 
                            placeholder="6+ chars: 1 upper, 1 lower, 1 digit"
                            required
                        >
                        <!-- Rule #4: Real-time password strength feedback -->
                        <div id="password-strength" class="password-strength"></div>
                        <div class="validation-message" id="password-error"></div>
                    </div>

                    <!-- Rule #5: Submit button disabled until form is valid -->
                    <button type="submit" id="register-btn" class="btn btn-primary" disabled>
                        Create Account
                    </button>

                    <!-- Rule #8: Error message display -->
                    <div id="error-message" class="error-message" style="display: none;"></div>

                    <!-- Rule #7: Success message display -->
                    <div id="success-message" class="success-message" style="display: none;"></div>
                </form>

                <p>
                    Already have an account? <a href="login.html">Log in here</a>
                </p>
            </div>
        </main>
    </div>

    <script src="../js/sessionManager.js"></script>
    <script src="../js/auth.js"></script>
    <script src="../js/registration.js"></script>
</body>
</html>
```

---

### `frontend/pages/login.html` (CREATE)

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Login - Tool Lending Platform</title>
    <link rel="stylesheet" href="../css/styles.css">
</head>
<body>
    <div id="app">
        <header>
            <nav class="navbar">
                <h1>Tool Lending Platform</h1>
            </nav>
        </header>

        <main id="main-content">
            <div id="login-page">
                <h2>Log In to Your Account</h2>

                <!-- Rule #2: Login form -->
                <form id="login-form" class="auth-form">
                    <!-- Username field -->
                    <div class="form-group">
                        <label for="username">Username *</label>
                        <input 
                            type="text" 
                            id="username" 
                            name="username" 
                            placeholder="Enter your username"
                            required
                        >
                        <div class="validation-message" id="username-error"></div>
                    </div>

                    <!-- Password field -->
                    <div class="form-group">
                        <label for="password">Password *</label>
                        <!-- Rule #9: Use type="password" -->
                        <input 
                            type="password" 
                            id="password" 
                            name="password" 
                            placeholder="Enter your password"
                            required
                        >
                        <div class="validation-message" id="password-error"></div>
                    </div>

                    <!-- Submit button -->
                    <button type="submit" id="login-btn" class="btn btn-primary">
                        Log In
                    </button>

                    <!-- Rule #8: Error message display -->
                    <div id="error-message" class="error-message" style="display: none;"></div>

                    <!-- Rule #7: Success message -->
                    <div id="success-message" class="success-message" style="display: none;"></div>
                </form>

                <p>
                    Don't have an account? <a href="register.html">Register here</a>
                </p>
            </div>
        </main>
    </div>

    <script src="../js/sessionManager.js"></script>
    <script src="../js/auth.js"></script>
    <script src="../js/login.js"></script>
</body>
</html>
```

---

### `frontend/js/registration.js` (CREATE)

```javascript
import AuthService from './auth.js';

const authService = new AuthService('http://localhost:5000');

document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('register-form');
    const usernameInput = document.getElementById('username');
    const passwordInput = document.getElementById('password');
    const registerBtn = document.getElementById('register-btn');
    const errorDiv = document.getElementById('error-message');
    const successDiv = document.getElementById('success-message');

    /**
     * Rule #3: Username validation
     * - 3-50 characters
     * - Only alphanumeric and underscore
     */
    function validateUsername(username) {
        const errors = [];
        if (!username || username.length < 3) {
            errors.push('Username must be at least 3 characters');
        }
        if (username.length > 50) {
            errors.push('Username must not exceed 50 characters');
        }
        if (!/^[a-zA-Z0-9_]+$/.test(username)) {
            errors.push('Username can only contain letters, numbers, and underscores');
        }
        return errors;
    }

    /**
     * Rule #3: Password validation
     * - 6+ characters
     * - At least 1 uppercase
     * - At least 1 lowercase
     * - At least 1 digit
     */
    function validatePassword(password) {
        const errors = [];
        if (!password || password.length < 6) {
            errors.push('Password must be at least 6 characters');
        }
        if (!/[A-Z]/.test(password)) {
            errors.push('Password must contain at least 1 uppercase letter');
        }
        if (!/[a-z]/.test(password)) {
            errors.push('Password must contain at least 1 lowercase letter');
        }
        if (!/\d/.test(password)) {
            errors.push('Password must contain at least 1 digit');
        }
        return errors;
    }

    /**
     * Rule #4: Display validation messages in real-time
     */
    function updateValidationMessages() {
        const usernameErrors = validateUsername(usernameInput.value);
        const passwordErrors = validatePassword(passwordInput.value);

        const usernameErrorDiv = document.getElementById('username-error');
        const passwordErrorDiv = document.getElementById('password-error');

        usernameErrorDiv.textContent = usernameErrors.length > 0 ? usernameErrors[0] : '';
        usernameErrorDiv.className = usernameErrors.length > 0 ? 'error' : 'success';

        passwordErrorDiv.textContent = passwordErrors.length > 0 ? passwordErrors[0] : '';
        passwordErrorDiv.className = passwordErrors.length > 0 ? 'error' : 'success';

        // Rule #5: Disable submit button if form invalid
        const isFormValid = usernameErrors.length === 0 && passwordErrors.length === 0;
        registerBtn.disabled = !isFormValid;
    }

    /**
     * Rule #4: Listen to input changes for real-time validation
     */
    usernameInput.addEventListener('input', updateValidationMessages);
    passwordInput.addEventListener('input', updateValidationMessages);

    /**
     * Rule #6, #7, #8: Handle form submission
     */
    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        // Rule #6: Show loading state
        registerBtn.disabled = true;
        registerBtn.textContent = 'Creating Account...';
        errorDiv.style.display = 'none';
        successDiv.style.display = 'none';

        try {
            const result = await authService.register(
                usernameInput.value,
                passwordInput.value
            );

            // Rule #7: Show success message
            successDiv.textContent = result.message;
            successDiv.style.display = 'block';

            // Redirect to dashboard after 1.5 seconds
            setTimeout(() => {
                window.location.href = 'dashboard.html';
            }, 1500);
        } catch (error) {
            // Rule #8: Show error message and re-enable form
            errorDiv.textContent = error.message;
            errorDiv.style.display = 'block';

            registerBtn.disabled = false;
            registerBtn.textContent = 'Create Account';
        }
    });

    // Initial validation check
    updateValidationMessages();
});
```

---

### `frontend/js/login.js` (CREATE)

```javascript
import AuthService from './auth.js';

const authService = new AuthService('http://localhost:5000');

document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('login-form');
    const usernameInput = document.getElementById('username');
    const passwordInput = document.getElementById('password');
    const loginBtn = document.getElementById('login-btn');
    const errorDiv = document.getElementById('error-message');
    const successDiv = document.getElementById('success-message');

    /**
     * Rule #4: Real-time validation feedback
     */
    function validateForm() {
        const usernameValid = usernameInput.value.length > 0;
        const passwordValid = passwordInput.value.length > 0;
        loginBtn.disabled = !(usernameValid && passwordValid);
    }

    usernameInput.addEventListener('input', validateForm);
    passwordInput.addEventListener('input', validateForm);

    /**
     * Rule #6, #7, #8: Handle form submission
     */
    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        // Rule #6: Show loading state
        loginBtn.disabled = true;
        loginBtn.textContent = 'Logging in...';
        errorDiv.style.display = 'none';
        successDiv.style.display = 'none';

        try {
            const result = await authService.login(
                usernameInput.value,
                passwordInput.value
            );

            // Rule #7: Show success message
            successDiv.textContent = result.message;
            successDiv.style.display = 'block';

            // Redirect to dashboard
            setTimeout(() => {
                window.location.href = 'dashboard.html';
            }, 1500);
        } catch (error) {
            // Rule #8: Show error and re-enable form
            errorDiv.textContent = error.message;
            errorDiv.style.display = 'block';

            loginBtn.disabled = false;
            loginBtn.textContent = 'Log In';
        }
    });

    // Initial validation check
    validateForm();
});
```

---

### `frontend/pages/dashboard.html` (CREATE, placeholder)

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Dashboard - Tool Lending Platform</title>
    <link rel="stylesheet" href="../css/styles.css">
</head>
<body>
    <div id="app">
        <header>
            <nav class="navbar">
                <h1>Tool Lending Platform</h1>
                <div id="nav-menu">
                    <span id="welcome-message">Welcome, <strong id="username"></strong>!</span>
                    <a href="#" id="logout-link">Logout</a>
                </div>
            </nav>
        </header>

        <main id="main-content">
            <h2>Dashboard</h2>
            <p>Welcome to your Tool Lending Dashboard! (Content to be filled in future sprints)</p>
        </main>
    </div>

    <script src="../js/sessionManager.js"></script>
    <script src="../js/auth.js"></script>
    <script>
        // Placeholder: Check authentication and display username
        const authService = new AuthService('http://localhost:5000');
        if (!authService.isAuthenticated()) {
            window.location.href = 'login.html';
        } else {
            document.getElementById('username').textContent = authService.getSessionUsername() || 'User';
            
            document.getElementById('logout-link').addEventListener('click', (e) => {
                e.preventDefault();
                authService.logout();
                window.location.href = 'login.html';
            });
        }
    </script>
</body>
</html>
```

---

### `frontend/css/styles.css` (MODIFY - add auth form styles)

Add to existing styles:

```css
/* Auth form styles */
.auth-form {
    max-width: 500px;
    margin: 0 auto;
    background: white;
    padding: 2rem;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.form-group {
    margin-bottom: 1.5rem;
}

.form-group label {
    display: block;
    margin-bottom: 0.5rem;
    font-weight: 500;
    color: #2c3e50;
}

.form-group input {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ccc;
    border-radius: 4px;
    font-size: 1rem;
    transition: border-color 0.2s;
}

.form-group input:focus {
    outline: none;
    border-color: #3498db;
    box-shadow: 0 0 0 2px rgba(52, 152, 219, 0.1);
}

.validation-message {
    font-size: 0.875rem;
    margin-top: 0.25rem;
    min-height: 1.25rem;
}

.validation-message.error {
    color: #e74c3c;
}

.validation-message.success {
    color: #27ae60;
}

.password-strength {
    font-size: 0.875rem;
    margin-top: 0.25rem;
    min-height: 1.25rem;
}

.btn {
    padding: 0.75rem 1.5rem;
    border: none;
    border-radius: 4px;
    font-size: 1rem;
    cursor: pointer;
    transition: background 0.2s;
    width: 100%;
}

.btn-primary {
    background: #3498db;
    color: white;
}

.btn-primary:hover:not(:disabled) {
    background: #2980b9;
}

.btn-primary:disabled {
    background: #bdc3c7;
    cursor: not-allowed;
    opacity: 0.6;
}

.error-message {
    background: #fee;
    color: #c33;
    padding: 1rem;
    border-radius: 4px;
    margin-top: 1rem;
    border-left: 4px solid #e74c3c;
}

.success-message {
    background: #efe;
    color: #3c3;
    padding: 1rem;
    border-radius: 4px;
    margin-top: 1rem;
    border-left: 4px solid #27ae60;
}

#register-form p,
#login-form p {
    text-align: center;
    margin-top: 1rem;
}

#register-form a,
#login-form a {
    color: #3498db;
    text-decoration: none;
}

#register-form a:hover,
#login-form a:hover {
    text-decoration: underline;
}
```

---

## Edge Cases Handled

| # | Rule | Trigger | Expected Output |
|---|------|---------|-----------------|
| 1 | #3 | User types "ab" in username field | Error message: "Username must be at least 3 characters" |
| 2 | #3 | User types "pass" in password field | Error message: "Password must be at least 6 characters" |
| 3 | #4 | User clears form completely | All error messages disappear |
| 4 | #5 | Form becomes invalid | Submit button disabled (greyed out) |
| 5 | #5 | Form becomes valid | Submit button enabled |
| 6 | #6 | User clicks submit | Button shows "Creating Account..." and is disabled |
| 7 | #7 | Server returns 201 (registration success) | Success message displayed, redirect after 1.5s |
| 8 | #8 | Server returns 400 (duplicate username) | Error message displayed, form re-enabled |
| 9 | #8 | Network error during submit | Error message "Registration error: ..." displayed |
| 10 | #9 | Password input field | Text is masked (dots/asterisks shown, not plaintext) |

---

## Feature Flags

| Flag Name | Default Value | Controlled Behavior | When to Remove |
|-----------|---------------|---------------------|----------------|
| (none) | — | — | — |

---

## Verification Steps (for this task)

**Automated Checks:**
1. Code validates (no syntax errors):
   ```bash
   # No build step needed for vanilla JS; just verify files exist and are syntactically correct
   ```

**Manual/Integration Checks:**
1. Start backend: `cd backend && dotnet run`
2. Open browser to `http://localhost:5000/pages/register.html`
3. **Test Rule #3:** Type "ab" in username; verify error message appears
4. **Test Rule #4:** Type "password" in password field; verify messages appear in real-time
5. **Test Rule #5:** Leave form empty; verify submit button is disabled
6. **Test Rule #6:** Fill form and submit; verify loading spinner appears
7. **Test Rule #7:** After successful registration, verify redirect to dashboard happens
8. **Test Rule #8:** Try registering with duplicate username; verify error message displays
9. **Test Rule #9:** Click password field; verify input is masked (dots/asterisks)
10. **Test full flow:** Register → Redirect to dashboard → See welcome message with username

---

## Definition of Done

This task is complete when:

- ✅ `register.html` page with form fields and validation display.
- ✅ `login.html` page with form fields and error display.
- ✅ Real-time input validation with immediate feedback (Rules #3, #4).
- ✅ Submit button disabled until form is valid (Rule #5).
- ✅ Loading state shown during form submission (Rule #6).
- ✅ Success messages displayed and redirect happens (Rule #7).
- ✅ Error messages displayed with form re-enabled (Rule #8).
- ✅ Password fields use `type="password"` (Rule #9).
- ✅ All manual verification checks pass.
- ✅ Dashboard page placeholder created.

**Handoff to Sprint 2:**
- Complete authentication flow is implemented.
- Users can register and log in.
- Session persists across page reloads.
- Next sprint: Tool upload and management features.

---

## Final Checklist for Sprint 1 Completion

When all 7 tasks (TASK-00 through TASK-06) are completed:

- ✅ TASK-00: Project setup (ASP.NET Core, SQLite, frontend scaffold)
- ✅ TASK-01: Domain entities (User, exceptions)
- ✅ TASK-02: Repositories (UserRepository, migration)
- ✅ TASK-03: Application services (AuthenticationService, PasswordHasher)
- ✅ TASK-04: API layer (AuthController, DTOs, DI)
- ✅ TASK-05: Frontend services (AuthService, SessionManager)
- ✅ TASK-06: Frontend UI (Registration & Login pages)

**Full Sprint 1 verification:**
1. Backend tests pass: `dotnet test`
2. Frontend tests pass (if using test framework)
3. Manual E2E flow works: Register → Login → Dashboard
4. No console errors or warnings
5. All design rules from Design Rule Checklist implemented and verified

---
