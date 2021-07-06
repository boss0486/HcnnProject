var pageIndex = 1;
var URLC = "/Management/Faq/Action";
var URLA = "/Management/Faq";
var FaqController = {
    init: function () {
        FaqController.registerEvent();
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
                FaqController.Create();
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
        });
        $('#btnSearch').off('click').on('click', function () {
            FaqController.DataList(1);
        });
        $('#btnUpdate').off('click').on('click', function () {
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
                FaqController.Update();
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
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
                            var action = HelperModel.RolePermission(result.role, "FaqController", id);
                            //
                            var rowNum = parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                            rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>
                                 <td>${item.Title}</td>                                  
                                 <td>${item.MenuText}</td>                                  
                                 <td class='tbcol-created'>${item.CreatedBy}</td>                                  
                                 <td class="text-center">${HelperModel.StatusIcon(item.Enabled)}</td>
                                 <td class="text-center">${item.CreatedDate}</td>
                                 <td class="tbcol-action">${action}</td>
                            </tr>`;
                        });
                        $('tbody#TblData').html(rowData);
                        if (parseInt(totalPage) > 1) {
                            Paging.Pagination("#Pagination", totalPage, currentPage, FaqController.DataList);
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
    Create: function () {
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
    Update: function () {
        var id = $('#txtID').val();
        var title = $('#txtTitle').val();
        var summary = $('#txtSummary').val();
        var menuId = $(".vetical-menu input[type='checkbox']:checked").data('id');
        var enabled = 0;
        if ($('input[name="cbxActive"]').is(":checked"))
            enabled = 1;
        //
        var model = {
            Id: id,
            MenuID: menuId,
            Title: title,
            Summary: summary,
            Enabled: enabled
        };
        AjaxFrom.POST({
            url: URLC + '/Update',
            data: model,
            success: function (response) {
                if (response != null) {
                    if (response.status == 200) {
                        Notifization.Success(response.message);
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
                        FaqController.DataList(pageIndex);
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
    Details: function () {
        var id = $('#txtID').val();
        if (id.length <= 0) {
            Notifization.Error(MessageText.NotService);
            return;
        }
        var fData = {
            Id: $('#txtID').val()
        };
        $.ajax({
            url: '/post/detail',
            data: {
                strData: JSON.stringify(fData)
            },
            type: 'POST',
            dataType: 'json',
            success: function (result) {
                if (result != null) {
                    if (result.status == 200) {
                        var item = result.data;
                        $('#LblAccount').html(item.LoginID);
                        $('#LblDate').html(item.CreatedDate);
                        var action = '';
                        if (item.Enabled)
                            action += `<i class='fa fa-toggle-on'></i> actived`;
                        else
                            action += `<i class='fa fa-toggle-off'></i>not active`;

                        $('#LblActive').html(action);
                        $('#lblLastName').html(item.FirstName + ' ' + item.LastName);
                        $('#LblEmail').html(item.Email);
                        $('#LblPhone').html(item.Phone);
                        $('#LblLanguage').html(item.LanguageID);
                        $('#LblPermission').html(item.PermissionID);

                        return;
                    }
                    else {
                        Notifization.Error(result.message);
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
    ConfirmDelete: function (id) {
        Confirm.Delete(id, FaqController.Delete, null, null);
    }
};

FaqController.init();
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
$(document).on("change", ".vetical-menu input[type='checkbox']", function () {
    var menuId = $(".vetical-menu input[type='checkbox']:checked").data("id");
    if (menuId == undefined || menuId == "") {
        $('#lblMenuID').html('Vui lòng chọn danh mục');
    }
    else {
        $('#lblMenuID').html('');
    }
});
