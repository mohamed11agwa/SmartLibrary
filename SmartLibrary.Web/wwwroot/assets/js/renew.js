$(document).ready(function () {
    $('.js-renew').on('click', function () {

        var subscriberKey = $(this).data('key');
        bootbox.confirm({
            message: 'Are you Sure that you need to renew this subscription?',
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-success'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({
                        url: `/Subscribers/RenewSubscription?sKey=${subscriberKey}`,
                        data: {
                            '__RequestVerificationToken': $('input[name = "__RequestVerificationToken"]').val()

                        },
                        success: function (row) {
                            $('#SubscriptionsTable').find('tbody').append(row);
                            var ActiveIcon = $('#ActiveStatusIcon');
                            ActiveIcon.removeClass('d-none');
                            ActiveIcon.siblings('svg').remove();
                            ActiveIcon.parents('.card').removeClass('bg-warning').addClass('bg-success');

                            $('#CardStatus').text('Active Subscriber');
                            $('#BadgeStatus').removeClass('badge-light-warning').addClass('badge-light-success').text('Active Subscriber');

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
});