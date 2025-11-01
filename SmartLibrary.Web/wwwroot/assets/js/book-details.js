function OnAddCopySuccess(row) {
    ShowSuccessMessage();
    $('#Modal').modal('hide');
    $('tbody').prepend(row);
    KTMenu.createInstances();

    var count = $('#CopiesCount');
    var newCount = parseInt(count.text()) + 1;
    count.text(newCount);

    $('.js-alert').addClass('d-none');
    $('table').removeClass('d-none');
}

function OnEditCopySuccess(row) {
    ShowSuccessMessage();
    $('#Modal').modal('hide');

    $(UpdatedRow).replaceWith(row);
    KTMenu.createInstances();

}