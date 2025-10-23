var UpdatedRow;
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

function OnModalSuccess(item) {
    ShowSuccessMessage();
    $('#Modal').modal('hide');
    if (UpdatedRow === undefined) {
        $('tbody').append(item);
    } else {
        $(UpdatedRow).replaceWith(item);
        UpdatedRow = undefined;
    }
    KTMenu.init();
    KTMenu.initHandlers();
}
$(document).ready(function () {
    var message = $('#Message').text();
    if (message !== '') {
        ShowSuccessMessage(message);
    }


    //handle bootstrap modal
    $('body').delegate('.js-render-modal' ,'click', function () {
        var btn = $(this);
        var modal = $('#Modal');
        modal.find('#ModalLabel').text(btn.data('title'));
        if (btn.data('update') !== undefined) {
            UpdatedRow = btn.parents('tr');
            //console.log(UpdatedRow);
        }
        $.get({
            url: btn.data('url'),
            success: function (form) {
                //console.log(form);
                modal.find('.modal-body').html(form);
                $.validator.unobtrusive.parse(modal);
            },
            error: function () {
                ShowErrorMessage();
            }
        })
        modal.modal('show');
    });




});