$(document).ready(function () {
    // Show Modal via Fetch
    window.showModal = function (url, title) {
        $('#actionModalLabel').text(title);
        $('#modalBodyContent').html('<div class="text-center p-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');
        $('#actionModal').modal('show');

        fetch(url)
            .then(response => response.text())
            .then(html => {
                $('#modalBodyContent').html(html);
            })
            .catch(error => {
                $('#modalBodyContent').html('<div class="alert alert-danger">Error loading content. Please try again.</div>');
                console.error('Error:', error);
            });
    };

    // Auto-hide alerts after 5 seconds
    setTimeout(function () {
        $(".alert").fadeOut("slow");
    }, 5000);

    // Auto-open modal if URL has openModal=true
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get('openModal') === 'true') {
        const currentPath = window.location.pathname.toLowerCase();
        if (currentPath.includes('transactions')) {
            showModal('/Transactions/Create', 'New Transaction');
        } else if (currentPath.includes('employees')) {
            showModal('/Employees/Create', 'Add New Employee');
        }
    }
});
