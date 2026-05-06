document.addEventListener('DOMContentLoaded', () => {
    console.log('Tool Lending Platform initialized');

    // Health check on app startup
    fetch('/api/health')
        .then(res => res.json())
        .then(data => console.log('Backend health:', data))
        .catch(err => console.error('Backend unreachable:', err));
});
