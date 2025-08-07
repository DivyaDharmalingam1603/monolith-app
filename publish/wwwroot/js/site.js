// ANTI-PATTERN: Global variables and functions
var globalAppData = {};

// ANTI-PATTERN: jQuery mixed with vanilla JavaScript
$(document).ready(function () {
    // Initialize tooltips using jQuery selector
    $('[data-bs-toggle="tooltip"]').each(function () {
        new bootstrap.Tooltip(this);
    });

    // ANTI-PATTERN: Global event handlers
    setupGlobalEventHandlers();
});

// ANTI-PATTERN: Monolithic function with multiple responsibilities
function setupGlobalEventHandlers() {
    // Auto-dismiss alerts after 5 seconds
    setTimeout(function () {
        $('.alert').fadeOut('slow');
    }, 5000);

    // ANTI-PATTERN: Inline event handlers
    $('.btn-danger').click(function (e) {
        if (!confirm('Are you sure you want to perform this action?')) {
            e.preventDefault();
            return false;
        }
    });

    // ANTI-PATTERN: Generic error handling
    $(document).ajaxError(function (event, xhr, settings, error) {
        console.error('AJAX Error:', error);
        showNotification('An error occurred. Please try again.', 'error');
    });
}

// ANTI-PATTERN: Inconsistent function naming and no error handling
function showNotification(message, type) {
    var alertClass = 'alert-info';
    var icon = 'fas fa-info-circle';

    switch (type) {
        case 'success':
            alertClass = 'alert-success';
            icon = 'fas fa-check-circle';
            break;
        case 'error':
            alertClass = 'alert-danger';
            icon = 'fas fa-exclamation-triangle';
            break;
        case 'warning':
            alertClass = 'alert-warning';
            icon = 'fas fa-exclamation-triangle';
            break;
    }

    var alertHtml = `
<div class="alert ${alertClass} alert-dismissible fade show" role="alert">
<i class="${icon} me-2"></i>${message}
<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
</div>
    `;

    // ANTI-PATTERN: DOM manipulation without checking if element exists
    $('main').prepend(alertHtml);

    // Auto-dismiss after 3 seconds
    setTimeout(function () {
        $('.alert').first().fadeOut('slow');
    }, 3000);
}

// ANTI-PATTERN: Utility functions mixed with business logic
function formatNumber(num) {
    return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

function formatCurrency(amount) {
    return '$' + formatNumber(amount.toFixed(2));
}

// ANTI-PATTERN: No validation or error handling
function exportTableToCSV(tableId, filename) {
    var table = document.getElementById(tableId);
    if (!table) return;
    var csv = [];
    var rows = table.rows;

    for (var i = 0; i < rows.length; i++) {
        var row = [];
        var cells = rows[i].cells;

        for (var j = 0; j < cells.length; j++) {
            row.push(cells[j].innerText);
        }

        csv.push(row.join(','));
    }

    downloadCSV(csv.join('\n'), filename);
}

function downloadCSV(csv, filename) {
    var csvFile;
    var downloadLink;

    csvFile = new Blob([csv], { type: "text/csv" });
    downloadLink = document.createElement("a");
    downloadLink.download = filename;
    downloadLink.href = window.URL.createObjectURL(csvFile);
    downloadLink.style.display = "none";
    document.body.appendChild(downloadLink);
    downloadLink.click();
}

// ANTI-PATTERN: Hardcoded configuration values
var APP_CONFIG = {
    REFRESH_INTERVAL: 30000, // 30 seconds
    MAX_RETRY_ATTEMPTS: 3,
    API_TIMEOUT: 10000,
    DEBUG_MODE: true
};

// ANTI-PATTERN: Global state management
function updateGlobalState(key, value) {
    globalAppData[key] = value;

    if (APP_CONFIG.DEBUG_MODE) {
        console.log('Global state updated:', key, value);
    }
}

// ANTI-PATTERN: Synchronous operations that could block UI
function processLargeDataSet(data) {
    var processed = [];

    for (var i = 0; i < data.length; i++) {
        // Simulate heavy processing
        var item = data[i];
        item.processed = true;
        item.timestamp = new Date().toISOString();
        processed.push(item);
    }

    return processed;
}