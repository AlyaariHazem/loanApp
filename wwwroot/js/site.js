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
                bindAjaxForm();
            })
            .catch(error => {
                $('#modalBodyContent').html('<div class="alert alert-danger">خطأ في تحميل المحتوى. يرجى المحاولة مرة أخرى.</div>');
                console.error('Error:', error);
            });
    };

    function bindAjaxForm() {
        $('#modalBodyContent form').on('submit', function (e) {
            e.preventDefault();
            const form = $(this);
            const url = form.attr('action');
            const data = form.serialize();

            $.ajax({
                type: "POST",
                url: url,
                data: data,
                success: function (response) {
                    if (response.success) {
                        $('#actionModal').modal('hide');
                        window.location.reload();
                    } else {
                        // If it's a validation error, partial view is returned
                        $('#modalBodyContent').html(response);
                        bindAjaxForm(); // Re-bind for the new form
                    }
                },
                error: function () {
                    alert("حدث خطأ أثناء حفظ البيانات.");
                }
            });
        });
    }

    // Auto-hide alerts after 5 seconds
    setTimeout(function () {
        $(".alert").fadeOut("slow");
    }, 5000);

    // Auto-open modal if URL has openModal=true
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get('openModal') === 'true') {
        const currentPath = window.location.pathname.toLowerCase();
        if (currentPath.includes('transactions')) {
            showModal('/Transactions/Create', 'عملية تحويل جديدة');
        } else if (currentPath.includes('employees')) {
            showModal('/Employees/Create', 'إضافة موظف جديد');
        }
    }
});
