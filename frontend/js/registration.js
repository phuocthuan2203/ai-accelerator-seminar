import AuthService from './auth.js';

// Get the API URL from the current origin for consistency
const protocol = window.location.protocol;
const hostname = window.location.hostname;
const apiUrl = `${protocol}//${hostname}:5123`;
const authService = new AuthService(apiUrl);

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
        usernameErrorDiv.className = usernameErrors.length > 0 ? 'validation-message error' : 'validation-message success';

        passwordErrorDiv.textContent = passwordErrors.length > 0 ? passwordErrors[0] : '';
        passwordErrorDiv.className = passwordErrors.length > 0 ? 'validation-message error' : 'validation-message success';

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
