var pageIndex = 1;
var URLC = "/Management/ArticleCategory/Action";
var URLA = "/Management/ArticleCategory";
var articleCategoryController = {
    init: function () {
        articleCategoryController.registerEvent();
    },
    registerEvent: function () {
        $('#btnCreate').off('click').on('click', function () {
            var flg = true;
            var txtTitle = $('#txtTitle').val();
            var txtSummary = $('#txtSummary').val();

            if (txtTitle == '') {
                $('#lblTitle').html('Không được để trống tiêu đề');
                flg = false;
            }
            else if (txtTitle.length < 1 || txtTitle.length > 80) {
                $('#lblTitle').html('Tiêu đề giới hạn [1-80] characters');
                flg = false;
            }
            else if (!FormatKeyword.test(txtTitle)) {
                $('#lblTitle').html('Tiêu đề không hợp lệ');
                flg = false;
            }
            else {
                $('#lblTitle').html('');
            }
            if (txtSummary != '') {
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
            //
            //var ddlOrder = $('#ddlOrder').val();
            //if (parseInt(ddlOrder) == 0) {
            //    $('#lblOrder').html('Vui lòng chọn vị trí sắp xếp');
            //    flg = false;
            //}
            //else {
            //    $('#lblOrder').html('');
            //}
            var menuId = $(".vetical-menu input[type='checkbox']:checked").data('id');
            var ddlPageTemlate = $('#ddlPageTemlate').val();
            $('#lblPageTemlate').html('');
            if (menuId == undefined || menuId == "") {
                if (ddlPageTemlate == "") {
                    $('#lblPageTemlate').html('Vui lòng chọn mẫu hiển thị');
                    flg = false;
                }
            }
            // submit
            if (flg)
                articleCategoryController.Create();
            else
                Notifization.Error(MessageText.Datamissing);
        });
        $('#btnSearch').off('click').on('click', function () {
            articleCategoryController.DataList(1);
        });
        $('#btnUpdate').off('click').on('click', function () {
            var flg = true;
            var txtTitle = $('#txtTitle').val();
            var txtSummary = $('#txtSummary').val();

            if (txtTitle == '') {
                $('#lblTitle').html('Không được để trống tiêu đề');
                flg = false;
            }
            else if (txtTitle.length < 1 || txtTitle.length > 80) {
                $('#lblTitle').html('Tiêu đề giới hạn [1-80] characters');
                flg = false;
            }
            else if (!FormatKeyword.test(txtTitle)) {
                $('#lblTitle').html('Tiêu đề không hợp lệ');
                flg = false;
            }
            else {
                $('#lblTitle').html('');
            }
            if (txtSummary != '') {
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
            //
            //var ddlOrder = $('#ddlOrder').val();
            //if (parseInt(ddlOrder) == 0) {
            //    $('#lblOrder').html('Vui lòng chọn vị trí');
            //    flg = false;
            //}
            //else {
            //    $('#lblOrder').html('');
            //}
            var menuId = $(".vetical-menu input[type='checkbox']:checked").data('id');
            var ddlPageTemlate = $('#ddlPageTemlate').val();
            $('#lblPageTemlate').html('');
            if (menuId == undefined || menuId == "") {
                if (ddlPageTemlate == "") {
                    $('#lblPageTemlate').html('Vui lòng chọn mẫu hiển thị');
                    flg = false;
                }
            }
            // submit
            if (flg)
                articleCategoryController.Update();
            else
                Notifization.Error(MessageText.Datamissing);
        });
    },
    Create: function () {
        var title = $('#txtTitle').val();
        var summary = $('#txtSummary').val();
        var parentId = "";
        var enabled = 0;
        if ($("input[name='cbxActive']").is(":checked"))
            enabled = 1;
        //
        var checked = $(".vetical-menu input[type='checkbox']:checked");
        if (checked.length > 0)
            parentId = $(checked).data('id');
        //
        var iconFont = $('#txtIconFont').val();
        var backLink = $('#lblBackLink').val();
        var ddlOrder = parseInt($('#ddlOrder').val());
        var ddlPageTemlate = $('#ddlPageTemlate').val();
        if ($(checked).data("issub") != undefined && $(checked).data("issub") == true)
            ddlPageTemlate = "";
        // 
        var model = {
            ParentID: parentId,
            Title: title,
            Summary: summary,
            IconFont: iconFont,
            BackLink: backLink,
            OrderID: ddlOrder,
            PageTemlate: ddlPageTemlate,
            ImageFile: "",
            Enabled: enabled
        };
        AjaxFrom.POST({
            url: URLC + '/create',
            data: model,
            success: function (response) {
                if (response != null) {
                    if (response.status == 200) {
                        Notifization.Success(response.message);
                        FData.ResetForm();
                        articleCategoryController.GetCategory();
                        return;
                    }
                    else {
                        Notifization.Error(response.message);
                        return;
                    }
                }
                Notifization.Error(MessageText.NotServices);
                return;
            },
            error: function (response) {
                console.log('::' + MessageText.NotServices);
            }
        });
    },
    Update: function () {
        var id = $('#txtID').val();
        var title = $('#txtTitle').val();
        var summary = $('#txtSummary').val();
        var parentId = "";
        var enabled = 0;
        if ($("input[name='cbxActive']").is(":checked"))
            enabled = 1;
        // 
        var checked = $(".vetical-menu input[type='checkbox']:checked");
        if (checked.length > 0)
            parentId = $(checked).data('id');
        //
        var iconFont = $('#txtIconFont').val();
        var backLink = $('#lblBackLink').val();
        var ddlOrder = parseInt($('#ddlOrder').val());
        var ddlPageTemlate = $('#ddlPageTemlate').val();
        if ($(checked).data("issub") != undefined && $(checked).data("issub") == true)
            ddlPageTemlate = "";
        // 
        var model = {
            ID: id,
            ParentID: parentId,
            Title: title,
            Summary: summary,
            IconFont: iconFont,
            BackLink: backLink,
            OrderID: ddlOrder,
            PageTemlate: ddlPageTemlate,
            ImageFile: "",
            Enabled: enabled
        };
        AjaxFrom.POST({
            url: URLC + '/update',
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
                Notifization.Error(MessageText.NotServices);
                return;
            },
            error: function (response) {
                console.log('::' + MessageText.NotServices);
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
                if (result != null) {
                    if (result.status == 200) {
                        $('tbody#TblData').html('');
                        $('#Pagination').html('');
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
                            var action = HelperModel.RolePermission(result.role, "articleCategoryController", id);
                            // icon sort
                            var _orderId = item.OrderID;
                            var _actionSort = `<i data-sortup='btn-sort-up' data-id='${id}' data-order'${_orderId}' class='fas fa-arrow-circle-up icon-mnsort'></i> <i data-sortdown ='btn-sort-down' data-id='${id}' data-order'${_orderId}' class='fas fa-arrow-circle-down icon-mnsort'></i>`;
                            //
                            var _title = SubStringText.SubTitle(item.Title);
                            var rowNum = parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                            rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>
                                 <td style='background:#F1F5F7'>${_title}</td>                                  
                                 <td>${item.Summary}</td>                                  
                                 <td class="text-center">${_actionSort}</td>                                                                 
                                 <td class="text-center">${HelperModel.StatusIcon(item.Enabled)}</td>
                                 <td class="text-center"><span>${item.CreatedDate}</span></td>
                                 <td class="tbcol-action">${action}</td>
                            </tr>`;
                            var subMenu = item.SubData;
                            if (subMenu != undefined && subMenu != null && subMenu.length > 0) {
                                rowData += articleCategoryController.SubData(index, subMenu, 0, result.role);
                            }
                        });
                        $('tbody#TblData').html(rowData);
                        if (parseInt(totalPage) > 1) {
                            Paging.Pagination("#Pagination", totalPage, currentPage, articleCategoryController.DataList);
                        }
                        return;
                    }
                }
                //Notifization.Error(MessageText.NotServices);
                return;
            },
            error: function (result) {
                console.log('::' + MessageText.NotServices);
            }
        });
    },
    SubData: function (_index, lstModel, _level, _role) {
        var rowData = '';
        if (lstModel.length > 0) {
            _level += 1;
            $.each(lstModel, function (index, item) {
                index = index + 1;
                var id = item.ID;
                if (id.length > 0)
                    id = id.trim();
                // 
                var _orderId = item.OrderID;
                var _actionSort = `<i data-sortup='btn-sort-up' data-id='${id}' data-order'${_orderId}' class='fas fa-arrow-circle-up icon-mnsort'></i> <i data-sortdown ='btn-sort-down' data-id='${id}' data-order'${_orderId}' class='fas fa-arrow-circle-down icon-mnsort'></i>`;
                //
                var action = HelperModel.RolePermission(_role, "articleCategoryController", id);
                var _title = SubStringText.SubTitle(item.Title);
                var rowNum = ''; //parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                _index = _level * 15;
                rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>
                                 <td><div style='padding-left:${_index}px'>${_level}.${index} ${_title}</div></td>
                                 <td>${item.Summary}</td>  
                                 <td class="text-center">${_actionSort}</td>                                                                 
                                 <td class="text-center">${HelperModel.StatusIcon(item.Enabled)}</td>
                                 <td class="text-center "><span>${item.CreatedDate}</span></td>
                                 <td class="tbcol-action">${action}</td>
                            </tr>`;

                var subMenu = item.SubData;
                if (subMenu != undefined && subMenu != null && subMenu.length > 0) {
                    rowData += articleCategoryController.SubData(_index, subMenu, _level, _role);
                }
            });
        }
        return rowData;
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
                        articleCategoryController.DataList(pageIndex);
                        return;
                    }
                    else {
                        Notifization.Error(response.message);
                        return;
                    }
                }
                Notifization.Error(MessageText.NotServices);
                return;
            },
            error: function (response) {
                console.log('::' + MessageText.NotServices);
            }
        });
    },
    ConfirmDelete: function (id) {
        Confirm.Delete(id, articleCategoryController.Delete, null, null);
    },
    GetCategory: function (_id) {
        // model 
        var _option_default = `<li>
                                    <input id="cbxItem0" type="checkbox" class="filled-in" data-id='' data-isdefault='true' />
                                    <label for="cbxItem0">Tạo mới</label>
                               </li>`;
        var model = {};
        AjaxFrom.POST({
            url: URLC + '/Option',
            data: model,
            success: function (result) {
                $('ul#CategoryOption').html(_option_default);
                if (result != null) {
                    if (result.status == 200) {
                        var rowData = '';
                        $.each(result.data, function (index, item) {
                            index = index + 1;
                            var id = item.ID;
                            if (id.length > 0)
                                id = id.trim();
                            var _title = SubStringText.SubTitle(item.Title);
                            var subMenu = item.SubData;
                            var iAngle = '';
                            if (subMenu != undefined && subMenu != null && subMenu.length > 0)
                                iAngle = "<i class='fa fa-angle-left iplus'></i>";
                            var isChecked = "";
                            if (id != null && id == _id) {
                                isChecked = "checked";
                            }
                            //
                            var _level = 0;
                            rowData += `<li>
                                        <input id="cbxItem${id}" data-issub='${item.IsSub}' type="checkbox" class="filled-in" data-id='${id}' ${isChecked} />
                                        <label for="cbxItem${id}">${_title}</label>`;
                            if (subMenu != undefined && subMenu != null && subMenu.length > 0) {
                                rowData += articleCategoryController.GetSubCategory(index, subMenu, _level, _id);
                            }
                            rowData += '</li>';


                        });
                        $('ul#CategoryOption').html(_option_default + rowData);
                        return;
                    }
                }
                //Notifization.Error(MessageText.NotService);
                return;
            },
            error: function (result) {
                console.log('::' + MessageText.NotService);
            }
        });
    },
    GetSubCategory: function (_index, lstModel, _level, _id) {
        var rowData = '';
        //
        if (lstModel.length > 0) {
            _level += 1;
            rowData += `<ul>`;
            $.each(lstModel, function (index, item) {
                index = index + 1;
                var id = item.ID;
                if (id.length > 0)
                    id = id.trim();

                var _title = SubStringText.SubTitle(item.Title);
                var subMenu = item.SubData;
                var iAngle = '';
                if (subMenu != undefined && subMenu != null && subMenu.length > 0)
                    iAngle = "<i class='fa fa-angle-left iplus'></i>";
                var isChecked = "";
                if (id != null && id == _id)
                    isChecked = "checked";
                //
                var pading = _level * 38;

                rowData += `<li>
                               <input id="cbxItem${id}" data-issub='${item.IsSub}' type="checkbox" class="filled-in" data-id='${id}' ${isChecked} />
                               <label for="cbxItem${id}">${_title}</label>`;

                if (subMenu != undefined && subMenu != null && subMenu.length > 0) {
                    rowData += articleCategoryController.GetSubCategory(_index, subMenu, _level, _id);
                }
                rowData += '</li>';
            });
            rowData += `</ul>`;
        }
        return rowData;
    }
};
articleCategoryController.init();
$(document).on('keyup', '#txtTitle', function () {
    var txtTitle = $(this).val();
    if (txtTitle == '') {
        $('#lblTitle').html('Không được để trống tiêu đề');
    }
    else if (txtTitle.length < 1 || txtTitle.length > 80) {
        $('#lblTitle').html('Tiêu đề giới hạn [1-80] characters');
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
    if (txtSummary != '') {
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
$(document).on("click", '.mn-ckeck', function () {
    if ($('i').hasClass('fa-check-square')) {
        // remove
        $('.sidebar-menu i.mn-ckeck').removeClass('fa-check-square');
        $('.sidebar-menu i.mn-ckeck').addClass('fa-square');
        $('.sidebar-menu i.actived').removeClass('actived');
    }
    if ($('i').hasClass('fa-square')) {
        $(this).removeClass('fa-square');
        $(this).addClass('fa-check-square');
        $(this).addClass('actived');
    }
});
// menu sort
$(document).on('click', '[data-sortup]', function () {
    var id = $(this).data('id');
    var model = {
        ID: id
    };
    AjaxFrom.POST({
        url: URLC + '/sortup',
        data: model,
        success: function (result) {
            if (result != null) {
                if (result.status == 200) {
                    Notifization.Success(result.message);
                    articleCategoryController.DataList(pageIndex);
                    return;
                }
                else {
                    Notifization.Error(result.message);
                    return;
                }
            }
            Notifization.Error(MessageText.NotServices);
            return;
        },
        error: function (result) {
            console.log('::' + MessageText.NotServices);
        }
    });
});
$(document).on('click', '[data-sortdown]', function () {
    var id = $(this).data('id');
    var model = {
        ID: id
    };
    AjaxFrom.POST({
        url: URLC + '/sortdown',
        data: model,
        success: function (result) {
            if (result != null) {
                if (result.status == 200) {
                    Notifization.Success(result.message);
                    articleCategoryController.DataList(pageIndex);
                    return;
                }
                else {
                    Notifization.Error(result.message);
                    return;
                }
            }
            Notifization.Error(MessageText.NotServices);
            return;
        },
        error: function (result) {
            console.log('::' + MessageText.NotServices);
        }
    });
});


