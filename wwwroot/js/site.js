// Wait for DOM to be ready
$(document).ready(function() {
    // Back to top button functionality
    var backToTopButton = $('#backToTop');

    // Show/hide button based on scroll position
    $(window).scroll(function() {
        if ($(this).scrollTop() > 100) {
            backToTopButton.addClass('show');
        } else {
            backToTopButton.removeClass('show');
        }
    });

    // Smooth scroll to top
    backToTopButton.click(function(e) {
        e.preventDefault();
        $('html, body').animate({
            scrollTop: 0
        }, 300);
    });

    // Initialize lazy loading for images
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('lazy');
                    img.classList.add('loaded');
                    imageObserver.unobserve(img);
                }
            });
        });

        document.querySelectorAll('img[data-src]').forEach(img => {
            imageObserver.observe(img);
        });
    }

    // Add loading state to buttons
    $('.btn[type="submit"]').click(function() {
        var $btn = $(this);
        if (!$btn.hasClass('loading')) {
            $btn.addClass('loading');
            // Remove loading state after form submission
            setTimeout(() => {
                $btn.removeClass('loading');
            }, 3000);
        }
    });

    // Auto-hide alerts
    $('.alert').each(function() {
        var $alert = $(this);
        setTimeout(function() {
            $alert.fadeOut();
        }, 5000);
    });

	// Form validation enhancement
    const forms = document.querySelectorAll('.needs-validation');
    forms.forEach(function(form) {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });

     // Search functionality (if search input exists)
    const searchInput = document.querySelector('#search-input');
    if (searchInput) {
        let searchTimeout;
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function() {
                // Implement search functionality here
                console.log('Searching for:', searchInput.value);
            }, 300);
        });
    }
});

// Utility functions
function formatDate(date) {
    const options = { 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric',
        locale: 'pt-BR'
    };
    return new Date(date).toLocaleDateString('pt-BR', options);
}

function showAlert(message, type = 'info') {
    const alertContainer = document.querySelector('#alert-container');
    if (!alertContainer) return;

    const alertElement = document.createElement('div');
    alertElement.className = `alert alert-${type} alert-dismissible fade show`;
    alertElement.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    alertContainer.appendChild(alertElement);

    // Auto-hide after 5 seconds
    setTimeout(function() {
        if (alertElement.parentNode) {
            alertElement.remove();
        }
    }, 5000);
}

// Function to copy text to clipboard
function copyToClipboard(text) {
    if (navigator.clipboard) {
        navigator.clipboard.writeText(text).then(() => {
            showAlert('Texto copiado para a área de transferência!', 'success');
        });
    } else {
        // Fallback for older browsers
        const textArea = document.createElement('textarea');
        textArea.value = text;
        document.body.appendChild(textArea);
        textArea.select();
        document.execCommand('copy');
        document.body.removeChild(textArea);
        showAlert('Texto copiado para a área de transferência!', 'success');
    }
}

// Smooth scrolling for anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            e.preventDefault();
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// AJAX helper function
async function makeRequest(url, options = {}) {
    const defaultOptions = {
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    };

    const mergedOptions = { ...defaultOptions, ...options };

    try {
        const response = await fetch(url, mergedOptions);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const contentType = response.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
            return await response.json();
        } else {
            return await response.text();
        }
    } catch (error) {
        console.error('Request failed:', error);
        showAlert('Erro na requisição. Tente novamente.', 'danger');
        throw error;
    }
}

// Export functions for use in other scripts
window.BlogUtils = {
    formatDate,
    showAlert,
    copyToClipboard,
    makeRequest
};