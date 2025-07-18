// Main site JavaScript functionality

document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(function(alert) {
        setTimeout(function() {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });

    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href === '#') {
                e.preventDefault(); // Prevent default behavior for href="#"
                return;
            }

            e.preventDefault();
            const target = document.querySelector(href);
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
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

    // Loading state for buttons
    document.querySelectorAll('button[type="submit"]').forEach(function(button) {
        button.addEventListener('click', function() {
            const form = this.closest('form');
            if (form && form.checkValidity()) {
                this.classList.add('loading');
                this.disabled = true;

                // Re-enable after 3 seconds as fallback
                setTimeout(() => {
                    this.classList.remove('loading');
                    this.disabled = false;
                }, 3000);
            }
        });
    });

    // Image lazy loading
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver(function(entries, observer) {
            entries.forEach(function(entry) {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('lazy');
                    imageObserver.unobserve(img);
                }
            });
        });

        document.querySelectorAll('img[data-src]').forEach(function(img) {
            imageObserver.observe(img);
        });
    }

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

    // Back to top button
    const backToTopButton = document.querySelector('#back-to-top');
    if (backToTopButton) {
        window.addEventListener('scroll', function() {
            if (window.pageYOffset > 300) {
                backToTopButton.classList.remove('d-none');
                backToTopButton.classList.add('show');
            } else {
                backToTopButton.classList.add('d-none');
                backToTopButton.classList.remove('show');
            }
        });

        backToTopButton.addEventListener('click', function(e) {
            e.preventDefault();
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
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
        const bsAlert = new bootstrap.Alert(alertElement);
        bsAlert.close();
    }, 5000);
}

function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(function() {
        showAlert('Texto copiado para a área de transferência!', 'success');
    }).catch(function() {
        showAlert('Erro ao copiar texto.', 'danger');
    });
}

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

// Site.js - ModernBlog JavaScript

// Check if jQuery is loaded
if (typeof jQuery !== 'undefined') {
    console.log('✅ jQuery carregado com sucesso!');
    console.log('🔍 Versão do jQuery:', jQuery.fn.jquery);
} else {
    console.error('❌ jQuery não foi carregado!');
}

// Use jQuery safely
$(document).ready(function() {
    // Track link clicks for debugging (only for valid hrefs)
    $('a').on('click', function(e) {
        const href = $(this).attr('href');
        if (href && href !== '#' && href !== '') {
            console.log('🔗 Link clicado:', href);
        }
    });

    // Fix like button functionality
    $('.like-btn').on('click', function(e) {
        e.preventDefault();
        const postId = $(this).data('post-id');

        if (!postId) {
            console.warn('Post ID não encontrado');
            return;
        }

        // Only proceed if user is authenticated
        if (!$(this).hasClass('authenticated')) {
            window.location.href = '/Identity/Account/Login';
            return;
        }

        // Make AJAX request to toggle like
        $.ajax({
            url: '/api/posts/' + postId + '/like',
            method: 'POST',
            success: function(response) {
                // Update like count and button state
                $('.like-count[data-post-id="' + postId + '"]').text(response.likeCount);
                const button = $('.like-btn[data-post-id="' + postId + '"]');
                button.toggleClass('liked', response.isLiked);
            },
            error: function(xhr, status, error) {
                console.error('Error toggling like:', error);
            }
        });
    });

    // Back to top button
    const backToTopButton = $('#back-to-top');
    if (backToTopButton.length) {
        $(window).scroll(function() {
            if ($(this).scrollTop() > 300) {
                backToTopButton.addClass('show');
            } else {
                backToTopButton.removeClass('show');
            }
        });

        backToTopButton.on('click', function(e) {
            e.preventDefault();
            $('html, body').animate({scrollTop: 0}, 600);
        });
    }
});