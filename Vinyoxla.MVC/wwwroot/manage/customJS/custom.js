toastr.options = {
    hideDuration: 300,
    timeOut: 2500,
    positionClass: "toast-bottom-right",
    "closeButton": false,
    "debug": false,
    "newestOnTop": true,
    "progressBar": false,
    "preventDuplicates": false,
    "onclick": null,
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
}

$(document).ready(function () {

    // -------------------------- Relation page Admin Area

    //#region Vin code input uppercase

    $(document).on('input keyup', '#findvin', function () {
        let value = $(this).val();
        $(this).val(value.toUpperCase());
    });

    //#endregion Vin code input uppercase

    //#region Prevent non numeric in phone input

    $(document).on('input keyup', '#finduser', function () {
        if (!/^[0-9]+$/.test($(this).val())) {
            $(this).val($(this).val().slice(0, -1))
        }
    });

    //#endregion Prevent non numeric in phone input

    //#region Clear form and url, reload

    $(document).on('click', '#clearform', function () {
        $("#finduser").val("");
        $("#findvin").val("");

        let url = window.location.href
        url = url.split("?")
        window.location.href = url[0];
    });

    //#endregion Clear form and url, reload

    //#region Delete button

    $(document).on('click', '.deleteBtn', function (e) {
        e.preventDefault();

        Swal.fire({
            title: 'Are you sure?',
            text: "You won't be able to revert this!",
            padding: '3em',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {

                fetch($(this).attr('href'))
                    .then(res => res.text())
                    .then(data => { $('.tblContent').html(data) });

                Swal.fire(
                    'Deleted!',
                    '',
                    'success'
                )
            }
        });
    });

    //#endregion Delete button

    // -------------------------- Relation page Admin Area

});