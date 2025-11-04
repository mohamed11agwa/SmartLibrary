var UpdatedRow;
var table;
var datatable;
var exportedCols = [];
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
    //console.log(message);
    Swal.fire({
        icon: "error",
        title: "Oops...",
        text: message.responseText != undefined ? message.responseText : message,
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
}

function disableSubmitButtons() {
    $('body :submit').attr('disabled', 'disabled').attr('data-kt-indicator', 'on');
}
function OnModalBegin() {
    disableSubmitButtons();
}
function OnModalSuccess(row) {
    ShowSuccessMessage();
    $('#Modal').modal('hide');

    if (UpdatedRow !== undefined) {
        datatable.row(UpdatedRow).remove().draw();
        UpdatedRow = undefined;
    }
    var newRow = $(row);
    datatable.row.add(newRow).draw(false);

    //KTMenu.init();

    setTimeout(function () {
        KTMenu.init();
        KTMenu.initHandlers();
    }, 100);

    
}

function OnModalComplete() {
    $('body :submit').removeAttr('disabled').removeAttr('data-kt-indicator');
}


function applySelect2() {
    //Select2
    $('.js-select2').select2();
    $('.js-select2').on('select2:select', function (e) {
        var select = $(this);
        $('form').not('#SignOut').validate().element('#' + select.attr('id'));
    });
}



//Datatable
var headers = $('th');
$.each(headers, function (i) {
    if (!$(this).hasClass('js-no-export')) {
        exportedCols.push(i);
    }
});

var KTDatatables = function () {

    // Private functions
    var initDatatable = function () {
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            "info": false,
            'pageLength': 10,
        });
    }

    // Hook export buttons
    var exportButtons = () => {
        const documentTitle = $('js-datatables').data('document-title');
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'copyHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'excelHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'csvHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'pdfHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                }
            ]
        }).container().appendTo($('#kt_datatable_example_buttons'));

        // Hook dropdown menu click event to datatable export buttons
        const exportButtons = document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]');
        exportButtons.forEach(exportButton => {
            exportButton.addEventListener('click', e => {
                e.preventDefault();

                // Get clicked export value
                const exportValue = e.target.getAttribute('data-kt-export');
                const target = document.querySelector('.dt-buttons .buttons-' + exportValue);

                // Trigger click event on hidden datatable export buttons
                target.click();
            });
        });
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }

    // Public methods
    return {
        init: function () {
            table = document.querySelector('.js-datatables');

            if (!table) {
                return;
            }

            initDatatable();
            exportButtons();
            handleSearchDatatable();
        }
    };
}();





$(document).ready(function () {

    //Disable submit buttons on form submit
    $('form').not('#SignOut').on('submit', function () {
        if ($('.js-tinymce').length > 0) {
            $('.js-tinymce').each(function () {
                var input = $(this);
                var content = tinymce.get(input.attr('id')).getContent();
                input.val(content);

            });

        } 

        var isValid = $(this).valid();
        if (isValid) disableSubmitButtons();

    });


    //Tinymce
    if ($('.js-tinymce').length > 0) {
        var options = { selector: ".js-tinymce", height: "421" };
        var options = { selector: ".js-tinymce", height: "421" };

        if (KTThemeMode.getMode() === "dark") {
            options["skin"] = "oxide-dark";
            options["content_css"] = "dark";
        }
        tinymce.init(options);

    } 


    //Select2
    applySelect2();

    //DateRangePIcker
    $('.js-datepicker').daterangepicker({
        singleDatePicker: true,
        autoApply: true,
        drops: 'up',
        maxDate: new Date()
    });


    //sweetAlert
    var message = $('#Message').text();
    if (message !== '') {
        ShowSuccessMessage(message);
    }

    //datatable
    KTUtil.onDOMContentLoaded(function () {
        KTDatatables.init();
    });

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
                applySelect2();
            },
            error: function () {
                ShowErrorMessage();
            }
        })
        modal.modal('show');
    });

    //Handle ToggleStatus
    $('body').delegate('.js-toggle-status', 'click', function () {
        // console.log("Button Clicked");
        var btn = $(this);
        // console.log(btn.data('id'));
        // var result = confirm("Are you Sure that you need to toggle this item status?");

        bootbox.confirm({
            message: 'Are you Sure that you need to toggle this item status?',
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({
                        url: btn.data('url'),
                        data: {
                            '__RequestVerificationToken': $('input[name = "__RequestVerificationToken"]').val()

                        },
                        success: function (LastUpdatedOn) {
                            // js-update-on
                            var row = btn.parents('tr');
                            var status = row.find('.js-status');
                            var newStatus = status.text().trim() === "Deleted" ? "Available" : "Deleted";
                            status.text(newStatus).toggleClass('badge-light-success badge-light-danger');
                            row.find('.js-update-on').html(LastUpdatedOn);
                            row.addClass('animate__animated animate__flash');

                            ShowSuccessMessage();

                        },
                        error: function () {
                            ShowErrorMessage();

                        }
                    });
                }
            }
        });





    });



    //Handle SignOut
    $('.js-signout').on('click', function () {
        $('#SignOut').submit();
    });

});