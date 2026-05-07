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
