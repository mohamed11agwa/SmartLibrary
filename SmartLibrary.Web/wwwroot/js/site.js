function ShowSuccessMessage(message = "Saved Successfully") {
    Swal.fire({
        icon: "success",
        title: "success",
        text: message,
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
}
function ShowErrorMessage(message = "Something went wrong!") {
    Swal.fire({
        icon: "error",
        title: "Oops...",
        text: message,
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
}

$(document).ready(function () {
    var message = $('#Message').text();
    if (message !== '') {
        ShowSuccessMessage(message);
    }


    //handle bootstrap modal
    $('.js-render-modal').on('click', function () {
        var btn = $(this);
        var modal = $('#Modal');
        modal.find('#ModalLabel').text(btn.data('title'));

        $.get({
            url: btn.data('url'),
            success: function (form) {
                console.log(form);
                modal.find('.modal-body').html(form);
            },
            error: function () {
                ShowErrorMessage();
            }
        })
        modal.modal('show');
    });




});