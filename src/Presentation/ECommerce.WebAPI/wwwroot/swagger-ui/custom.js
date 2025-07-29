// Custom Swagger UI JavaScript
window.addEventListener('DOMContentLoaded', function() {
    // Add custom functionality to Swagger UI
    console.log('ECommerce API Documentation loaded');
    
    // Add version info to the page
    const versionInfo = document.createElement('div');
    versionInfo.innerHTML = '<small>API Version: 1.0.0 | Environment: ' + 
        (window.location.hostname === 'localhost' ? 'Development' : 'Production') + 
        '</small>';
    versionInfo.style.cssText = 'text-align: center; padding: 10px; color: #666; border-top: 1px solid #eee; margin-top: 20px;';
    
    // Wait for Swagger UI to load and then append version info
    setTimeout(function() {
        const swaggerContainer = document.querySelector('.swagger-ui');
        if (swaggerContainer) {
            swaggerContainer.appendChild(versionInfo);
        }
    }, 1000);
    
    // Add keyboard shortcuts
    document.addEventListener('keydown', function(e) {
        // Ctrl+/ to focus search
        if (e.ctrlKey && e.key === '/') {
            e.preventDefault();
            const searchInput = document.querySelector('.swagger-ui .filter input');
            if (searchInput) {
                searchInput.focus();
            }
        }
    });
});