# TASK-05 — Frontend Services: AuthService + Session Management

## Context

This task implements the **frontend service layer** that wraps the backend API endpoints. It provides a clean JavaScript API for components to call, handles HTTP requests/responses, manages session state on the client, and provides utilities for authentication checks.

This layer abstracts away HTTP details from UI components, making components testable and decoupled from the backend.

---

## Design References

| Document | Section / Anchor | Purpose |
|----------|------------------|---------|
| `docs/sprint1-authen/sprint1-design.md` | §2.2 Frontend Service Layer | AuthService control class |
| `docs/sprint1-authen/sprint1-design.md` | §5a Frontend Class Diagram | AuthService methods and properties |
| `docs/sprint1-authen/sprint1-design.md` | §3 API Contract & Payload | Request/response schemas for register and login |

---

## Sequence Diagram Reference (MANDATORY — Read Before Any Code)

**Sprint Design Doc section:** §4 — Detailed Sequence Diagrams  
**Relevant flows:** BF1 (Registration), BF2 (Login)

**From BF1 (Registration) — Frontend flow:**
```
User -> RegistrationPage: Enters username & password
RegistrationPage -> RegistrationPage: Validate input (frontend validation)
RegistrationPage -> AuthService: register(username, password)
AuthService -> AuthController: fetch('/api/auth/register', {method: 'POST', body: {...}})
AuthController -> AuthService: 201 Created + response body
AuthService -> AuthService: Store session (userId, username)
AuthService -> RegistrationPage: return {userId, username, message}
RegistrationPage -> User: Display success, redirect
```

---

## Design Rule Checklist

| # | Exact Rule (Verbatim) | Source | Owner Files | Test Name | Verification |
|---|---------------------|--------|-------------|-----------|--------------|
| 1 | "AuthService has register(username, password): Promise<{userId, username, message}>" | Sprint Design §5a Frontend Class Diagram | `frontend/js/auth.js` | `AuthServiceTests_RegisterReturnsPromise` | Unit test |
| 2 | "AuthService has login(username, password): Promise<{userId, username, message}>" | Sprint Design §5a Frontend Class Diagram | `frontend/js/auth.js` | `AuthServiceTests_LoginReturnsPromise` | Unit test |
| 3 | "AuthService stores session on successful register/login (userId, username in localStorage or in-memory)" | Design context | `frontend/js/auth.js` | `AuthServiceTests_StoresSession` | Unit test |
| 4 | "AuthService provides isAuthenticated(): bool to check if user is logged in" | Sprint Design §5a Frontend Class Diagram | `frontend/js/auth.js` | `AuthServiceTests_IsAuthenticated` | Unit test |
| 5 | "AuthService provides getSessionUserId(): int \| null to retrieve current user ID" | Implied for protected routes | `frontend/js/auth.js` | `AuthServiceTests_GetSessionUserId` | Unit test |
| 6 | "AuthService.register() throws or returns error if response is not 201" | Error handling | `frontend/js/auth.js` | `AuthServiceTests_RegisterError` | Unit test |
| 7 | "AuthService.login() throws or returns error if response is not 200" | Error handling | `frontend/js/auth.js` | `AuthServiceTests_LoginError` | Unit test |
| 8 | "Session data persists across page reloads (use localStorage or sessionStorage)" | User experience | `frontend/js/auth.js` | `AuthServiceTests_SessionPersists` | Unit test |

---

## Affected Files

| File Path | Action | Layer / Role | Must-Contain |
|-----------|--------|--------------|-------------|
| `frontend/js/auth.js` | CREATE | Service layer | `class AuthService { register(), login(), isAuthenticated(), getSessionUserId() }` |
| `frontend/js/sessionManager.js` | CREATE | Helper | `class SessionManager { set(), get(), clear(), exists() }` |
| `tests/frontend/auth.service.spec.js` | CREATE | Unit tests | Multiple test methods using a mocking library (or simple mocks) |

---

## Implementation Skeleton

### `frontend/js/sessionManager.js` (CREATE)

```javascript
/**
 * SessionManager handles client-side session storage (localStorage).
 * Rule #3, #8: Persists session data across page reloads.
 */
class SessionManager {
  constructor(storageKey = 'session') {
    this.storageKey = storageKey;
  }

  /**
   * Set session data (userId, username)
   */
  set(sessionData) {
    localStorage.setItem(this.storageKey, JSON.stringify(sessionData));
  }

  /**
   * Get session data
   */
  get() {
    const data = localStorage.getItem(this.storageKey);
    return data ? JSON.parse(data) : null;
  }

  /**
   * Check if session exists
   */
  exists() {
    return !!localStorage.getItem(this.storageKey);
  }

  /**
   * Clear session
   */
  clear() {
    localStorage.removeItem(this.storageKey);
  }

  /**
   * Get userId from session
   */
  getUserId() {
    const session = this.get();
    return session ? session.userId : null;
  }

  /**
   * Get username from session
   */
  getUsername() {
    const session = this.get();
    return session ? session.username : null;
  }
}

export default SessionManager;
```

---

### `frontend/js/auth.js` (CREATE)

```javascript
import SessionManager from './sessionManager.js';

/**
 * AuthService provides a clean API for authentication flows.
 * Rule #1, #2: register() and login() methods.
 * Rule #3, #4, #5: Session management and authentication checks.
 * Rule #6, #7: Error handling for failed requests.
 */
class AuthService {
  constructor(apiBaseUrl = 'http://localhost:5000', sessionManager = null) {
    this.apiBaseUrl = apiBaseUrl;
    this.sessionManager = sessionManager || new SessionManager();
  }

  /**
   * Rule #1: Register a new user.
   * Returns: Promise<{userId, username, message}>
   * Throws: Error with message if registration fails.
   */
  async register(username, password) {
    try {
      const response = await fetch(`${this.apiBaseUrl}/api/auth/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include', // Include cookies in request
        body: JSON.stringify({ username, password })
      });

      if (!response.ok) {
        // Rule #6: Handle error responses (400, 500, etc.)
        const errorData = await response.json();
        throw new Error(errorData.details?.message || 'Registration failed');
      }

      const data = await response.json();

      // Rule #3: Store session on success
      this.sessionManager.set({
        userId: data.userId,
        username: data.username
      });

      return {
        userId: data.userId,
        username: data.username,
        message: data.message
      };
    } catch (error) {
      throw new Error(`Registration error: ${error.message}`);
    }
  }

  /**
   * Rule #2: Authenticate user.
   * Returns: Promise<{userId, username, message}>
   * Throws: Error with message if login fails.
   */
  async login(username, password) {
    try {
      const response = await fetch(`${this.apiBaseUrl}/api/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include', // Include cookies
        body: JSON.stringify({ username, password })
      });

      if (!response.ok) {
        // Rule #7: Handle error responses (401, 400, etc.)
        const errorData = await response.json();
        throw new Error(errorData.details?.message || 'Login failed');
      }

      const data = await response.json();

      // Rule #3: Store session on success
      this.sessionManager.set({
        userId: data.userId,
        username: data.username
      });

      return {
        userId: data.userId,
        username: data.username,
        message: data.message
      };
    } catch (error) {
      throw new Error(`Login error: ${error.message}`);
    }
  }

  /**
   * Rule #4: Check if user is authenticated.
   * Returns: boolean
   */
  isAuthenticated() {
    return this.sessionManager.exists();
  }

  /**
   * Rule #5: Get current user ID.
   * Returns: int | null
   */
  getSessionUserId() {
    return this.sessionManager.getUserId();
  }

  /**
   * Rule #5: Get current username.
   * Returns: string | null
   */
  getSessionUsername() {
    return this.sessionManager.getUsername();
  }

  /**
   * Logout: clear session.
   */
  logout() {
    this.sessionManager.clear();
  }

  /**
   * Rule #8: Restore session from localStorage on app startup.
   * Returns: boolean (true if session was restored).
   */
  restoreSession() {
    return this.isAuthenticated();
  }
}

export default AuthService;
```

---

### `tests/frontend/auth.service.spec.js` (CREATE)

```javascript
import AuthService from '../../frontend/js/auth.js';
import SessionManager from '../../frontend/js/sessionManager.js';

describe('AuthService', () => {
  let authService;
  let mockSessionManager;

  beforeEach(() => {
    // Mock SessionManager
    mockSessionManager = {
      set: jest.fn(),
      get: jest.fn(),
      exists: jest.fn(),
      clear: jest.fn(),
      getUserId: jest.fn(),
      getUsername: jest.fn()
    };

    authService = new AuthService('http://localhost:5000', mockSessionManager);

    // Mock fetch
    global.fetch = jest.fn();
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  describe('register', () => {
    // Rule #1: Register happy path
    it('should call register endpoint and return user data', async () => {
      // Arrange
      const username = 'testuser';
      const password = 'SecurePass123';
      const responseData = {
        userId: 1,
        username,
        message: 'Registration successful'
      };

      global.fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => responseData
      });

      // Act
      const result = await authService.register(username, password);

      // Assert
      expect(global.fetch).toHaveBeenCalledWith(
        'http://localhost:5000/api/auth/register',
        expect.objectContaining({
          method: 'POST',
          headers: { 'Content-Type': 'application/json' }
        })
      );
      expect(result).toEqual(responseData);
      expect(mockSessionManager.set).toHaveBeenCalledWith({
        userId: 1,
        username
      });
    });

    // Rule #6: Register error handling
    it('should throw error on register failure', async () => {
      // Arrange
      global.fetch.mockResolvedValueOnce({
        ok: false,
        json: async () => ({
          error: 'ValidationError',
          details: { message: 'Username already taken' }
        })
      });

      // Act & Assert
      await expect(authService.register('existing', 'Pass123')).rejects.toThrow();
    });
  });

  describe('login', () => {
    // Rule #2: Login happy path
    it('should call login endpoint and return user data', async () => {
      // Arrange
      const username = 'testuser';
      const password = 'SecurePass123';
      const responseData = {
        userId: 1,
        username,
        message: 'Login successful'
      };

      global.fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => responseData
      });

      // Act
      const result = await authService.login(username, password);

      // Assert
      expect(global.fetch).toHaveBeenCalledWith(
        'http://localhost:5000/api/auth/login',
        expect.objectContaining({
          method: 'POST'
        })
      );
      expect(result).toEqual(responseData);
      expect(mockSessionManager.set).toHaveBeenCalledWith({
        userId: 1,
        username
      });
    });

    // Rule #7: Login error handling
    it('should throw error on login failure', async () => {
      // Arrange
      global.fetch.mockResolvedValueOnce({
        ok: false,
        json: async () => ({
          error: 'AuthenticationError',
          details: { message: 'Invalid username or password' }
        })
      });

      // Act & Assert
      await expect(authService.login('user', 'wrong')).rejects.toThrow();
    });
  });

  describe('session management', () => {
    // Rule #3, #4, #5: Session checks
    it('should check if authenticated', () => {
      // Arrange
      mockSessionManager.exists.mockReturnValueOnce(true);

      // Act
      const result = authService.isAuthenticated();

      // Assert
      expect(result).toBe(true);
    });

    // Rule #5: Get userId
    it('should return userId from session', () => {
      // Arrange
      mockSessionManager.getUserId.mockReturnValueOnce(42);

      // Act
      const userId = authService.getSessionUserId();

      // Assert
      expect(userId).toBe(42);
    });

    // Rule #8: Session persistence
    it('should restore session from storage', () => {
      // Arrange
      mockSessionManager.exists.mockReturnValueOnce(true);

      // Act
      const restored = authService.restoreSession();

      // Assert
      expect(restored).toBe(true);
    });

    it('should logout and clear session', () => {
      // Act
      authService.logout();

      // Assert
      expect(mockSessionManager.clear).toHaveBeenCalled();
    });
  });
});
```

---

## Edge Cases Handled

| # | Rule | Trigger | Expected Output |
|---|------|---------|-----------------|
| 1 | #6 | Register response is 400 | Throw error with message from response |
| 2 | #6 | Register response is 500 | Throw error "Registration failed" |
| 3 | #6 | Network error during register | Throw error "Registration error: ..." |
| 4 | #7 | Login response is 401 | Throw error "Invalid username or password" |
| 5 | #7 | Login response is 500 | Throw error "Login failed" |
| 6 | #4 | No session in storage | isAuthenticated() returns false |
| 7 | #5 | No session stored | getSessionUserId() returns null |
| 8 | #8 | Page reloads | Session restored from localStorage |

---

## Feature Flags

| Flag Name | Default Value | Controlled Behavior | When to Remove |
|-----------|---------------|---------------------|----------------|
| (none) | — | — | — |

---

## Verification Steps (for this task)

**Automated Checks:**
1. Run unit tests:
   ```bash
   npm test tests/frontend/auth.service.spec.js
   # Expected: 8+ tests pass
   ```

**Manual Checks:**
1. Open browser console; verify no JavaScript errors.
2. Test register/login flow manually in browser (to be done in TASK-06).

---

## Definition of Done

This task is complete when:

- ✅ `AuthService` class with `register()` and `login()` methods.
- ✅ `SessionManager` for localStorage-based session storage.
- ✅ Session persists across page reloads (using localStorage).
- ✅ Error handling for failed API responses.
- ✅ `isAuthenticated()`, `getSessionUserId()`, `logout()` methods implemented.
- ✅ All 8+ unit tests pass.
- ✅ No hardcoded API URLs (configurable).

**Handoff to TASK-06:**
- `AuthService` is fully implemented and tested.
- Components can now import and use AuthService for authentication.
- Next task: Frontend UI components (RegistrationPage, LoginPage) will use AuthService.

---
