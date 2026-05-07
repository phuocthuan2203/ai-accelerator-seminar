import SessionManager from './sessionManager.js';

class AuthService {
  constructor(apiBaseUrl = 'http://localhost:5000', sessionManager = null) {
    this.apiBaseUrl = apiBaseUrl;
    this.sessionManager = sessionManager || new SessionManager();
  }

  async register(username, password) {
    try {
      const response = await fetch(`${this.apiBaseUrl}/api/auth/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify({ username, password })
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.details?.message || 'Registration failed');
      }

      const data = await response.json();

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

  async login(username, password) {
    try {
      const response = await fetch(`${this.apiBaseUrl}/api/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify({ username, password })
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.details?.message || 'Login failed');
      }

      const data = await response.json();

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

  isAuthenticated() {
    return this.sessionManager.exists();
  }

  getSessionUserId() {
    return this.sessionManager.getUserId();
  }

  getSessionUsername() {
    return this.sessionManager.getUsername();
  }

  logout() {
    this.sessionManager.clear();
  }

  restoreSession() {
    return this.isAuthenticated();
  }
}

export default AuthService;
