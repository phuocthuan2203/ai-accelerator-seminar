using System;
using System.Threading.Tasks;
using ToolLendingPlatform.Domain;
using ToolLendingPlatform.Domain.Exceptions;
using ToolLendingPlatform.Application.Interfaces;
using ToolLendingPlatform.Application.Validators;

namespace ToolLendingPlatform.Application.Services
{
    /// <summary>
    /// AuthenticationService orchestrates registration and login flows.
    /// Rule #8: Register: validate input, hash password, create User entity, save.
    /// Rule #9: Login: validate input, query user, verify password with constant-time compare.
    /// Rule #5, #6: Throw appropriate exceptions for errors.
    /// </summary>
    public class AuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public AuthenticationService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        /// <summary>
        /// Rule #8: Register a new user.
        /// Steps:
        ///   1. Validate username format (Rule #4)
        ///   2. Validate password strength (Rule #3)
        ///   3. Check if username already exists (Rule #5)
        ///   4. Hash password using bcrypt (Rule #1)
        ///   5. Create User entity
        ///   6. Save to repository
        /// Throws: InvalidUsernameException, InvalidPasswordException, DuplicateUsernameException
        /// </summary>
        public async Task<User> RegisterAsync(string username, string password)
        {
            // Validate username format (Rule #4)
            var (usernameValid, usernameErrors) = UsernameValidator.ValidateUsername(username);
            if (!usernameValid)
                throw new InvalidUsernameException(string.Join("; ", usernameErrors));

            // Validate password strength (Rule #3)
            var (passwordValid, passwordErrors) = PasswordValidator.ValidatePassword(password);
            if (!passwordValid)
                throw new InvalidPasswordException(string.Join("; ", passwordErrors));

            // Check username uniqueness (Rule #5)
            bool userExists = await _userRepository.ExistsAsync(username);
            if (userExists)
                throw new DuplicateUsernameException(username);

            // Hash password (Rule #1, #2)
            string passwordHash = await _passwordHasher.HashAsync(password);

            // Create User entity (domain validation happens here)
            var user = new User(username, passwordHash);

            // Persist to repository
            await _userRepository.SaveAsync(user);

            return user;
        }

        /// <summary>
        /// Rule #9: Authenticate user with username and password.
        /// Steps:
        ///   1. Validate input (username and password required)
        ///   2. Query user by username
        ///   3. If user not found, throw AuthenticationException (Rule #6, #7)
        ///   4. Verify password using constant-time comparison (Rule #2)
        ///   5. If password incorrect, throw AuthenticationException (Rule #6, #7)
        ///   6. Return authenticated user
        /// Throws: AuthenticationException (with generic message to prevent username enumeration)
        /// </summary>
        public async Task<User> LoginAsync(string username, string password)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new AuthenticationException("Invalid username or password"); // Rule #7: Generic message

            // Query user by username
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
                throw new AuthenticationException("Invalid username or password"); // Rule #7: Generic message

            // Verify password using constant-time comparison (Rule #2)
            bool passwordValid = await _passwordHasher.VerifyAsync(password, user.PasswordHash);
            if (!passwordValid)
                throw new AuthenticationException("Invalid username or password"); // Rule #7: Generic message

            return user;
        }
    }
}
