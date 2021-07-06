var pageIndex = 1;
var URLC = "/Management/Quotation/Action";
var URLA = "/Management/Quotation";
var arrFile = [];
var QuotationController = {
    init: function () {
        QuotationController.registerEvent();
    },
    registerEvent: function () {
        $(document).ready(function () {
            // $('[data-date="true"]').val(DateTime.Get_ClientDate(lg = 'en'));
        });
        $('#btnCreate').off('click').on('click', function () {
            var flg = true;
            //
            var title = $('#txtTitle').val();
            if (title === '') {
                $('#lblTitle').html('Không được để trống tiêu đề');
                flg = false;
            }
            else if (title.length < 1 || title.length > 80) {
                $('#lblTitle').html('Tiêu đề giới hạn [1-80] characters');
                flg = false;
            }
            else if (!FormatKeyword.test(title)) {
                $('#lblTitle').html('Tiêu đề không hợp lệ');
                flg = false;
            }
            else {
                $('#lblTitle').html('');
            }
            //  
            var summary = $('#txtSummary').val();
            if (summary !== '') {
                if (summary.length < 1 || summary.length > 120) {
                    $('#lblSummary').html('Mô tả giới hạn [1-120] ký tự');
                    flg = false;
                }
                else if (!FormatKeyword.test(summary)) {
                    $('#lblSummary').html('Mô tả không hợp lệ');
                    flg = false;
                }
                else {
                    $('#lblSummary').html('');
                }
            }
            else {
                $('#lblSummary').html('');
            }
             
            // submit form
            if (flg)
                QuotationController.Create();
            else
                Notifization.Error(MessageText.Datamissing);
        });
        $('#btnSearch').off('click').on('click', function () {
            QuotationController.DataList(1);
        });
        $('#btnUpdate').off('click').on('click', function () {
            var flg = true;
            //
            var title = $('#txtTitle').val();
            if (title === '') {
                $('#lblTitle').html('Không được để trống tiêu đề');
                flg = false;
            }
            else if (title.length < 1 || title.length > 80) {
                $('#lblTitle').html('Tiêu đề giới hạn [1-80] characters');
                flg = false;
            }
            else if (!FormatKeyword.test(title)) {
                $('#lblTitle').html('Tiêu đề không hợp lệ');
                flg = false;
            }
            else {
                $('#lblTitle').html('');
            }
            // 
            var summary = $('#txtSummary').val();
            if (summary !== '') {
                if (summary.length < 1 || summary.length > 120) {
                    $('#lblSummary').html('Mô tả giới hạn [1-120] ký tự');
                    flg = false;
                }
                else if (!FormatKeyword.test(summary)) {
                    $('#lblSummary').html('Mô tả không hợp lệ');
                    flg = false;
                }
                else {
                    $('#lblSummary').html('');
                }
            }
            else {
                $('#lblSummary').html('');
            }
             
            // submit form
            if (flg) {
                QuotationController.Update();
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
        });
    },
    Create: function () {
        var title = $('#txtTitle').val();
        var alias = $('#txtAlias').val();
        var summary = $('#txtSummary').val();
        var viewTotal = $('#txtViewTotal').val();
        var viewDate = $('#txtViewDate').val();
        var menuId = $(".vetical-menu input[type='checkbox']:checked").data('id');
        var htmlText = tinyMCE.editors[$('#txtHtmlText').attr('id')].getContent();
        var isShow = 0;
        var enabled = 0;
        if ($('input[name="cbxIsShow"]').is(":checked"))
            isShow = 1;
        //
        if ($('input[name="cbxActive"]').is(":checked"))
            enabled = 1;
        // 

        var _imgFile = '';
        var _imgFileView = $('.new-box-preview img');
        if (_imgFileView !== '' && _imgFileView !== undefined)
            _imgFile = $(_imgFileView).data('id');
        // file
        var arrFile = [];
        var _fileList = $('.pre-view .pre-item-box');
        if (_fileList.length > 0) {
            $.each(_fileList, function (index, preItem) {
                var idFile = $(this).data("id");
                if (idFile != undefined && idFile.length > 0)
                    arrFile.push(idFile);
            });
        }
        var model = {
            MenuID: menuId,
            Title: title,
            Alias: alias,
            Summary: summary,
            HtmlNote: "",
            HtmlText: htmlText,
            Tag: "",
            ImageFile: _imgFile,
            ViewTotal: viewTotal,
            ViewDate: viewDate,
            Files: arrFile,
            IsShow: isShow,
            Enabled: enabled
        };
        AjaxFrom.POST({
            url: URLC + '/Create',
            data: model,
            success: function (response) {
                if (response !== null) {
                    if (response.status === 200) {
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
        var menuId = $(".vetical-menu input[type='checkbox']:checked").data('id');
        var title = $('#txtTitle').val();
        var alias = $('#txtAlias').val();
        var summary = $('#txtSummary').val();
        var viewTotal = $('#txtViewTotal').val();
        var viewDate = $('#txtViewDate').val();
        var htmlText = tinyMCE.editors[$('#txtHtmlText').attr('id')].getContent();
        var enabled = 0;
        var isShow = 0;
        if ($('input[name="cbxActive"]').is(":checked"))
            enabled = 1;
        //
        if ($('input[name="cbxIsShow"]').is(":checked"))
            isShow = 1;
        //
        var _imgFile = '';
        if ($('.new-box-preview img') != undefined)
            _imgFile = AttachmentFile.FileName($('.new-box-preview img').attr('src'));//$('#imgFile').attr('src').FileName();
        //
        // file
        var arrFile = [];
        var _fileList = $('.pre-view .pre-item-box');
        if (_fileList.length > 0) {
            $.each(_fileList, function (index, preItem) {
                var idFile = $(this).data("id");
                if (idFile != undefined && idFile.length > 0)
                    arrFile.push(idFile);
            });
        }
        //
        var model = {
            ID: id,
            MenuID: menuId,
            Title: title,
            Alias: alias,
            Summary: summary,
            HtmlNote: "",
            HtmlText: htmlText,
            Tag: "",
            ImageFile: _imgFile,
            ViewTotal: viewTotal,
            ViewDate: viewDate,
            Files: arrFile,
            IsShow: isShow,
            Enabled: enabled
        };
        AjaxFrom.POST({
            url: URLC + '/Update',
            data: model,
            success: function (response) {
                if (response !== null) {
                    if (response.status === 200) {
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
    DataList: function (page) {
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
                if (result !== null) {
                    if (result.status === 200) {
                        var currentPage = 1;
                        var pagination = result.paging;
                        if (pagination !== null) {
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
                            var action = HelperModel.RolePermission(result.role, "QuotationController", id);
                            // 
                            var rowNum = parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                            var _title = SubStringText.SubTitle(item.Title);
                            rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>   
                                 <td class='tbcol-photo'><img src='${item.ImagePath}' /></td>  
                                 <td class='text-left'>${_title}</td>
                                 <td class="text-left"><span class="date"><i class="far fa-clock"></i> ${item.ViewDate} | <i class="fas fa-check-double"></i> ${item.ViewTotal}</span></td>
                                 <td class='tbcol-created'>${item.CreatedBy}</td>                                  
                                 <td class="text-center">${HelperModel.StatusIcon(item.Enabled)}</td>
                                 <td class="text-center">${item.CreatedDate}</td>
                                 <td class="tbcol-action">${action}</td>
                            </tr>`;
                        });
                        $('tbody#TblData').html(rowData);
                        if (parseInt(totalPage) > 1) {
                            Paging.Pagination("#Pagination", totalPage, currentPage, QuotationController.DataList);
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
    Delete: function (id) {
        var model = {
            ID: id
        };
        AjaxFrom.POST({
            url: URLC + '/Delete',
            data: model,
            success: function (response) {
                if (response !== null) {
                    if (response.status === 200) {
                        Notifization.Success(response.message);
                        QuotationController.DataList(pageIndex);
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
        Confirm.Delete(id, QuotationController.Delete, null, null);
    }
};
QuotationController.init();
$(document).on('keyup', '#txtTitle', function () {
    var txtTitle = $(this).val();
    if (txtTitle === '') {
        $('#lblTitle').html('Không được để trống tiêu đề');
    }
    else if (txtTitle.length < 1 || txtTitle.length > 80) {
        $('#lblTitle').html('Tiêu đề giới hạn [1-80] ký tự');
    }
    else if (!FormatKeyword.test(txtTitle)) {
        $('#lblTitle').html('Tiêu đề không hợp lệ');
    }
    else {
        $('#lblTitle').html('');
    }
});
$(document).on('keyup', '#txtSummary', function () {
    var txtSummary = $(this).val();
    if (txtSummary !== '') {
        if (txtSummary.length > 120) {
            $('#lblSummary').html('Mô tả giới hạn [1-120] ký tự');
            flg = false;
        }
        else if (!FormatKeyword.test(txtSummary)) {
            $('#lblSummary').html('Mô tả không hợp lệ');
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
$(document).on('keyup', '#txtAlias', function () {
    var alias = $('#txtAlias').val();
    if (alias !== '') {
        if (alias.length > 80) {
            $('#lblAlias').html('Đường dẫn giới hạn từ 0-> 80 ký tự');
        }
        else if (!FormatUnicode.test(alias)) {
            $('#lblAlias').html('Đường dẫn không hợp lệ');
        }
        else {
            $('#lblAlias').html('');
        }
    } else {
        $('#lblAlias').html('');
    }
});
// ViewTotal
$(document).on('keyup', '#txtViewTotal', function () {
    var viewTotal = $(this).val();
    if (viewTotal !== "") {
        if (!FormatNumber.test(viewTotal)) {
            $('#lblViewTotal').html('Số lượt xem sản phẩm không hợp lệ');
        }
        else {
            $('#lblViewTotal').html('');
        }
    }
});
// view date
$(document).on('keyup', '#txtViewDate', function () {
    var viewDate = $(this).val();
    if (viewDate !== '') {
        if (!FormatDateVN.test(viewDate)) {
            $('#lblViewDate').html('Ngày hiển thị không hợp lệ');
        }
        else {
            $('#lblViewDate').html('');
        }
    } else {
        $('#lblViewDate').html('');
    }
});
$(document).on("change", "#ddlCategoryID", function () {
    var txtCtl = $(this).val();
    if (txtCtl === "") {
        $('#lblCategory').html('Vui lòng chọn danh mục');
    }
    else {
        $('#lblCategory').html('');
    }
});
$(document).on('click', '#cbxActive', function () {
    if ($(this).hasClass('actived')) {
        // remove
        $(this).children('i').removeClass('fa-check-square');
        $(this).children('i').addClass('fa-square');
        $(this).removeClass('actived');
    }
    else {
        $(this).children('i').addClass('fa-check-square');
        $(this).children('i').removeClass('fa-square');
        $(this).addClass('actived');
    }
});
$(document).on('', '.img-caption-text', function () {
    $('.new-box-preview img').click();
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