var pageIndex = 1;
var URLC = "/Management/Contact/Action";
var URLA = "/Management/Contact";
var ContactController = {
    init: function () {
        ContactController.registerEvent();
    },
    registerEvent: function () {
        $('#btnCreate').off('click').on('click', function () {
            var flg = true;
            var txtTitle = $('#txtTitle').val();
            var txtSummary = $('#txtSummary').val();
            var menuId = $(".vetical-menu input[type='checkbox']:checked").data('id');
            //
            $('#lblTitle').html('');
            if (txtTitle == '') {
                $('#lblTitle').html('Không được để trống câu hỏi');
                flg = false;
            }
            else if (txtTitle.length < 1 || txtTitle.length > 80) {
                $('#lblTitle').html('Câu hỏi giới hạn [1-80] ký tự');
                flg = false;
            }
            else if (!FormatKeyword.test(txtTitle)) {
                $('#lblTitle').html('Câu hỏi không hợp lệ');
                flg = false;
            }
            //
            $('#lblSummary').html('');
            if (txtSummary != '') {
                if (txtSummary.length > 500) {
                    $('#lblSummary').html('Giải đáp giới hạn [1-500] ký tự');
                    flg = false;
                }
                else if (!FormatKeyword.test(txtSummary)) {
                    $('#lblSummary').html('Giải đáp không hợp lệ');
                    flg = false;
                } 
            } 
            // menu
            $('#lblMenuID').html('');
            var menuId = $(".vetical-menu input[type='checkbox']:checked").data('id');
            if (menuId == undefined || menuId == "") {
                $('#lblMenuID').html('Vui lòng chọn danh mục');
                flg = false;
            }
            // submit
            if (flg) {
                ContactController.Create();
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
        });
        $('#btnSearch').off('click').on('click', function () {
            ContactController.DataList(1);
        });
    },
    DataList: function (page) {
        //
        var ddlTimeExpress = $('#ddlTimeExpress').val();
        var txtStartDate = $('#txtStartDate').val();
        var txtEndDate = $('#txtEndDate').val();
        var model = {
            Query: $('#txtQuery').val(),
            Page: page,
            TimeExpress: parseInt(ddlTimeExpress),
            StartDate: txtStartDate,
            EndDate: txtEndDate,
            TimeZoneLocal: LibDateTime.GetTimeZoneByLocal(),
            Status: parseInt($('#ddlStatus').val())
        };
        //
        AjaxFrom.POST({
            url: URLC + '/DataList',
            data: model,
            success: function (result) {
                $('tbody#TblData').html('');
                $('#Pagination').html('');
                if (result != null) {
                    if (result.status == 200) {
                        var currentPage = 1;
                        var pagination = result.paging;
                        if (pagination != null) {
                            totalPage = pagination.TotalPage;
                            currentPage = pagination.Page;
                            pageSize = pagination.PageSize;
                            pageIndex = pagination.Page;
                        }
                        var rowData = '';
                        $.each(result.data, function (index, item) {
                            index = index + 1;
                            var id = item.ID;
                            if (id.length > 0)
                                id = id.trim();
                            //  role
                            var action = HelperModel.RolePermission(result.role, "ContactController", id);
                            //
                            var rowNum = parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                            rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>
                                 <td>${item.Title}</td>                                  
                                 <td>${item.Phone}</td>                                                               
                                 <td>${item.Email}</td>                                                                                                                           
                                 <td class="text-center">${HelperModel.StateIcon(item.State)}</td>
                                 <td class="text-center">${item.CreatedDate}</td>
                                 <td class="tbcol-action">${action}</td>
                            </tr>`;
                        });
                        $('tbody#TblData').html(rowData);
                        if (parseInt(totalPage) > 1) {
                            Paging.Pagination("#Pagination", totalPage, currentPage, ContactController.DataList);
                        }
                        return;
                    }
                    else {
                        //Notifization.Error(result.message);
                        console.log('::' + result.message);
                        return;
                    }
                }
                Notifization.Error(MessageText.NotService);
                return;
            },
            error: function (result) {
                console.log('::' + MessageText.NotService);
            }
        });
    },
    Send: function () {
        var title = $('#txtTitle').val();
        var summary = $('#txtSummary').val();
        var menuId = $(".vetical-menu input[type='checkbox']:checked").data('id');
        var enabled = 0;
        if ($('input[name="cbxActive"]').is(":checked"))
            enabled = 1;
        //
        var model = {
            MenuID: menuId,
            Title: title,
            Summary: summary,
            Enabled: enabled
        };
        AjaxFrom.POST({
            url: URLC + '/Create',
            data: model,
            success: function (response) {
                if (response != null) {
                    if (response.status == 200) {
                        Notifization.Success(response.message);
                        FData.ResetForm();
                        return;
                    }
                    else {
                        Notifization.Error(response.message);
                        return;
                    }
                }
                Notifization.Error(MessageText.NotService);
                return;
            },
            error: function (response) {
                console.log('::' + MessageText.NotService);
            }
        });

    },
    Delete: function (id) {
        var model = {
            Id: id
        };
        AjaxFrom.POST({
            url: URLC + '/Delete',
            data: model,
            success: function (response) {
                if (response != null) {
                    if (response.status == 200) {
                        Notifization.Success(response.message);
                        ContactController.DataList(pageIndex);
                        return;
                    }
                    else {
                        Notifization.Error(response.message);
                        return;
                    }
                }
                Notifization.Error(MessageText.NotService);
                return;
            },
            error: function (response) {
                console.log('::' + MessageText.NotService);
            }
        });
    },
    ConfirmDelete: function (id) {
        Confirm.Delete(id, ContactController.Delete, null, null);
    }
};

ContactController.init();
$(document).on('keyup', '#txtTitle', function () {
    var txtTitle = $(this).val();
    if (txtTitle == '') {
        $('#lblTitle').html('Không được để trống câu hỏi');
    }
    else if (txtTitle.length < 1 || txtTitle.length > 80) {
        $('#lblTitle').html('Câu hỏi giới hạn [1-80] ký tự');
    }
    else if (!FormatKeyword.test(txtTitle)) {
        $('#lblTitle').html('Câu hỏi không hợp lệ');
    }
    else {
        $('#lblTitle').html('');
    }
});
$(document).on('keyup', '#txtSummary', function () {
    var txtSummary = $(this).val();
    if (txtSummary != '') {
        if (txtSummary.length > 500) {
            $('#lblSummary').html('Giải đáp giới hạn [1-500] ký tự');
            flg = false;
        }
        else if (!FormatKeyword.test(txtSummary)) {
            $('#lblSummary').html('Giải đáp không hợp lệ');
            flg = false;
        }
        else {
            $('#lblSummary').html('');
        }
    }
    else {
        $('#lblSummary').html('');
    }
});
