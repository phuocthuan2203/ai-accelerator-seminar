import { describe, it, expect, beforeEach, afterEach, jest } from '@jest/globals';
import AuthService from '../../frontend/js/auth.js';
import SessionManager from '../../frontend/js/sessionManager.js';

describe('AuthService', () => {
  let authService;
  let mockSessionManager;

  beforeEach(() => {
    mockSessionManager = {
      set: jest.fn(),
      get: jest.fn(),
      exists: jest.fn(),
      clear: jest.fn(),
      getUserId: jest.fn(),
      getUsername: jest.fn()
    };

    authService = new AuthService('http://localhost:5000', mockSessionManager);

    global.fetch = jest.fn();
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  describe('register', () => {
    it('should call register endpoint and return user data', async () => {
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

      const result = await authService.register(username, password);

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

    it('should throw error on register failure', async () => {
      global.fetch.mockResolvedValueOnce({
        ok: false,
        json: async () => ({
          error: 'ValidationError',
          details: { message: 'Username already taken' }
        })
      });

      await expect(authService.register('existing', 'Pass123')).rejects.toThrow();
    });

    it('should handle network error during register', async () => {
      global.fetch.mockRejectedValueOnce(new Error('Network error'));

      await expect(authService.register('user', 'pass')).rejects.toThrow();
    });

    it('should handle register 500 error', async () => {
      global.fetch.mockResolvedValueOnce({
        ok: false,
        json: async () => ({})
      });

      await expect(authService.register('user', 'pass')).rejects.toThrow('Registration failed');
    });
  });

  describe('login', () => {
    it('should call login endpoint and return user data', async () => {
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

      const result = await authService.login(username, password);

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

    it('should throw error on login failure', async () => {
      global.fetch.mockResolvedValueOnce({
        ok: false,
        json: async () => ({
          error: 'AuthenticationError',
          details: { message: 'Invalid username or password' }
        })
      });

      await expect(authService.login('user', 'wrong')).rejects.toThrow();
    });

    it('should handle login 401 error with generic message', async () => {
      global.fetch.mockResolvedValueOnce({
        ok: false,
        json: async () => ({
          error: 'Unauthorized',
          details: { message: 'Invalid username or password' }
        })
      });

      await expect(authService.login('user', 'wrong')).rejects.toThrow('Invalid username or password');
    });

    it('should handle login 500 error', async () => {
      global.fetch.mockResolvedValueOnce({
        ok: false,
        json: async () => ({})
      });

      await expect(authService.login('user', 'pass')).rejects.toThrow('Login failed');
    });
  });

  describe('session management', () => {
    it('should check if authenticated', () => {
      mockSessionManager.exists.mockReturnValueOnce(true);

      const result = authService.isAuthenticated();

      expect(result).toBe(true);
    });

    it('should return false when not authenticated', () => {
      mockSessionManager.exists.mockReturnValueOnce(false);

      const result = authService.isAuthenticated();

      expect(result).toBe(false);
    });

    it('should return userId from session', () => {
      mockSessionManager.getUserId.mockReturnValueOnce(42);

      const userId = authService.getSessionUserId();

      expect(userId).toBe(42);
    });

    it('should return null when no userId in session', () => {
      mockSessionManager.getUserId.mockReturnValueOnce(null);

      const userId = authService.getSessionUserId();

      expect(userId).toBeNull();
    });

    it('should restore session from storage', () => {
      mockSessionManager.exists.mockReturnValueOnce(true);

      const restored = authService.restoreSession();

      expect(restored).toBe(true);
    });

    it('should logout and clear session', () => {
      authService.logout();

      expect(mockSessionManager.clear).toHaveBeenCalled();
    });

    it('should get session username', () => {
      mockSessionManager.getUsername.mockReturnValueOnce('testuser');

      const username = authService.getSessionUsername();

      expect(username).toBe('testuser');
    });
  });
});
