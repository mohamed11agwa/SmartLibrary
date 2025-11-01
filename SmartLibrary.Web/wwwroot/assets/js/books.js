$(document).ready(function () {
    $('[data-kt-filter="search"]').on('keyup', function () {
        var input = $(this);
        datatable.search(this.value).draw();
    });



    datatable = $('#Books').DataTable({
        serverSide: true,
        processing: true,
        stateSave: true,
        language: {
            processing: '<div class="d-flex justify-content-center text-primary align-items-center dt-spinner"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div><span class="text-muted ps-2">Loading...</span></div>'
        },
        ajax: {
            url: '/Books/GetBooks',
            type: 'POST'
        },
        'drawCallback': function () {
            KTMenu.createInstances();
        },
        order: [[1, 'asc']],
        columnDefs: [{
            targets: [0],
            visible: false,
            searchable: false
        }],
        columns: [
            { "data": "id", "name": "Id", "className": "d-none" },
            {
                "name": "Title",
                "className": "d-flex align-items-center",
                "render": function (data, type, row) {
                    return `<div class="symbol symbol-50px overflow-hidden me-3">
                                            <a href="/Books/Details/${row.id}">
                                                <div class="symbol-label h-70px">
                                                    <img src="${(row.imageThumbnailUrl === null ? '/images/books/no-book.jpg' : row.imageThumbnailUrl)}" alt="cover" class="w-100">
                                                </div>
                                            </a>
                                        </div>
                                        <div class="d-flex flex-column">
                                            <a href="/Books/Details/${row.id}" class="text-primary fw-bolder mb-1">${row.title}</a>
                                            <span>${row.author}</span>
                                        </div>`;
                }
            },
            { "data": "publisher", "name": "Publisher" },
            {
                "name": "PublishingDate",
                "render": function (data, type, row) {
                    return moment(row.publishingDate).format('ll')
                }
            },

            { "data": "hall", "name": "Hall" },
            { "data": "categories", "name": "Categories", "orderable": false },
            {
                "name": "IsAvailableForRental",
                "render": function (data, type, row) {
                    return `<span class="badge badge-light-${(row.isAvailableForRental ? 'success' : 'warning')}">
                                                    ${(row.isAvailableForRental ? 'Available' : 'Not Available')}
                                                </span>`;
                }
            },
            {
                "name": "IsDeleted",
                "render": function (data, type, row) {
                    return `<span class="badge badge-light-${(row.isDeleted ? 'danger' : 'success')} js-status">
                                                    ${(row.isDeleted ? 'Deleted' : 'Available')}
                                                </span>`;
                }
            },
            {
                "orderable": false,
                "render": function (data, type, row) {
                    return `
                        <button type="button" class="btn btn-sm btn-icon btn-color-primary btn-active-light-primary" data-kt-menu-trigger="click" data-kt-menu-placement="bottom-end">
                            <!--begin::Svg Icon | path: icons/duotune/general/gen024.svg-->
                            <span class="svg-icon svg-icon-2">
                                <svg xmlns="http://www.w3.org/2000/svg" width="24px" height="24px" viewBox="0 0 24 24">
                                    <g stroke="none" stroke-width="1" fill="none" fill-rule="evenodd">
                                        <rect x="5" y="5" width="5" height="5" rx="1" fill="currentColor"></rect>
                                        <rect x="14" y="5" width="5" height="5" rx="1" fill="currentColor" opacity="0.3"></rect>
                                        <rect x="5" y="14" width="5" height="5" rx="1" fill="currentColor" opacity="0.3"></rect>
                                        <rect x="14" y="14" width="5" height="5" rx="1" fill="currentColor" opacity="0.3"></rect>
                                    </g>
                                </svg>
                            </span>
                        <!--end::Svg Icon-->
                        </button>
                        <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-800 menu-state-bg-light-primary fw-semibold w-200px py-3" data-kt-menu="true" style="z-index: 107;">


                            <!--begin::Menu item-->
                            <div class="menu-item px-3">
                                    <a href="/Books/Edit/${row.id}"class="menu-link px-3" \>Edit</a>
                            </div>
                            <!--end::Menu item-->
                            <!--begin::Menu item-->
                            <div class="menu-item px-3">
                                <a href="javascript:;" id="toggle-status" class="menu-link flex-stack px-3 js-toggle-status"
                                            data-id="${row.id}" data-url="/Books/ToggleStatus/${row.id}">
                                    Toggle Status
                                </a>
                            </div>
                            <!--end::Menu item-->

                        </div> `;


                }
            },
        ]

    });





});

