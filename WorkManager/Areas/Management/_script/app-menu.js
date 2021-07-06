var pageIndex = 1;
var URLC = "/Management/Menu/Action";
var URLA = "/Management/Menu";
var appMenuController = {
    init: function () {
        appMenuController.registerEvent();
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
            if (ddlPageTemlate == undefined || ddlPageTemlate == "") {
                if (ddlPageTemlate == "") {
                    $('#lblPageTemlate').html('Chọn mẫu hiển thị');
                    flg = false;
                } 
            }
            // submit
            if (flg)
                appMenuController.Create();
            else
                Notifization.Error(MessageText.Datamissing);
        });
        $('#btnSearch').off('click').on('click', function () {
            appMenuController.DataList(1);
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
            if (ddlPageTemlate == undefined || ddlPageTemlate == "") {
                if (ddlPageTemlate == "") {
                    $('#lblPageTemlate').html('Vui lòng chọn mẫu hiển thị');
                    flg = false;
                } 
            }
            // submit
            if (flg)
                appMenuController.Update();
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
                        appMenuController.GetCategory();
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
                            var action = HelperModel.RolePermission(result.role, "appMenuController", id);
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
                                 <td>${item.PageTemlate}</td>                                  
                                 <td class="text-center">${_actionSort}</td>                                                                 
                                 <td class="text-center">${HelperModel.StatusIcon(item.Enabled)}</td>
                                 <td class="text-center"><span>${item.CreatedDate}</span></td>
                                 <td class="tbcol-action">${action}</td>
                            </tr>`;
                            var subMenu = item.SubMenu;
                            if (subMenu != undefined && subMenu != null && subMenu.length > 0) {
                                rowData += appMenuController.MenuSubData(index, subMenu, 0, result.role);
                            }
                        });
                        $('tbody#TblData').html(rowData);
                        if (parseInt(totalPage) > 1) {
                            Paging.Pagination("#Pagination", totalPage, currentPage, appMenuController.DataList);
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
    MenuSubData: function (_index, lstModel, _level, _role) {
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
                var action = HelperModel.RolePermission(_role, "appMenuController", id);
                var _title = SubStringText.SubTitle(item.Title);
                var rowNum = ''; //parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                _index = _level * 15;
                rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>
                                 <td><div style='padding-left:${_index}px'>${_level}.${index} ${_title}</div></td>
                                 <td>${item.PageTemlate}</td>  
                                 <td class="text-center">${_actionSort}</td>                                                                 
                                 <td class="text-center">${HelperModel.StatusIcon(item.Enabled)}</td>
                                 <td class="text-center "><span>${item.CreatedDate}</span></td>
                                 <td class="tbcol-action">${action}</td>
                            </tr>`;

                var subMenu = item.SubMenu;
                if (subMenu != undefined && subMenu != null && subMenu.length > 0) {
                    rowData += appMenuController.MenuSubData(_index, subMenu, _level, _role);
                }
            });
        }
        return rowData;
    },
    InLineList: function (page) {
        var model = {
            Query: $('#txtQuery').val(),
            Page: page
        };
        AjaxFrom.POST({
            url: URLC + '/datalist',
            data: model,
            success: function (result) {
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
                            var rowNum = parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                            rowData += `
                            <tr>
                                <td class="tbcol-title"><input id='Title'          type='text' data-column='Title'         data-id='${id}' value='${item.Title}'        data-oldval='${item.Title}'           max-length='100' style='width:100%' /></td>
                                <td class="tbcol-none" ><input id='PathAction'     type='text' data-column='PathAction'    data-id='${id}' value='${item.PathAction}'   data-oldval='${item.PathAction}'      max-length='100' style='width:100%' /></td>
                                <td class="tbcol-none" ><input id='MvcController'  type='text' data-column='Controller' data-id='${id}' value='${item.MvcController}'data-oldval='${item.MvcController}'   max-length='100' style='width:80px' /></td>
                                <td class="tbcol-none" ><input id='MvcAction'      type='text' data-column='Action'     data-id='${id}' value='${item.MvcAction}'    data-oldval='${item.MvcAction}'       max-length='100' style='width:80px' /></td>
                                <td class="tbcol-none" ><input id='IsPermission'   type='text' data-column='IsPermission'  data-id='${id}' value='${item.IsPermission}' data-oldval='${item.IsPermission}'    max-length='100' style='width:50px' /></td>
                                <td class="tbcol-none" ><input id='Enabled'        type='text' data-column='Enabled'       data-id='${id}' value='${item.Enabled}'      data-oldval='${item.Enabled}'         max-length='100' style='width:50px' /></td>
                                <td class="tbcol-none" ><input id='OrderID'        type='text' data-column='OrderID'       data-id='${id}' value='${item.OrderID}'      data-oldval='${item.OrderID}'         max-length='100' style='width:50px' /></td>
                            </tr>`;
                        });
                        $('tbody#TblData').html(rowData);
                        if (parseInt(totalPage) > 1) {
                            Paging.Pagination("#Pagination", totalPage, currentPage, appMenuController.InLineList);
                        }
                        return;
                    }
                    else {
                        //Notifization.Error(result.message);
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
                        appMenuController.DataList(pageIndex);
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
        appMenuController.Delete(id);
    },
    Active: function (id) {
        var fData = {
            Id: id
        };
        $.ajax({
            url: '/PostCategory/Active',
            data: {
                strData: JSON.stringify(fData)
            },
            type: 'POST',
            dataType: 'json',
            success: function (response) {
                if (response != null) {
                    if (response.status == 200) {
                        Notifization.Success(response.message);
                        appMenuController.DataList(pageIndex);
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
    UnActive: function (id) {
        var fData = {
            Id: id
        };
        $.ajax({
            url: '/PostCategory/UnActive',
            data: {
                strData: JSON.stringify(fData)
            },
            type: 'POST',
            dataType: 'json',
            success: function (response) {
                if (response != null) {
                    if (response.status == 200) {
                        Notifization.Success(response.message);
                        appMenuController.DataList(pageIndex);
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
    GetMenu: function () {
        var model = {};
        AjaxFrom.POST({
            url: URLC + '/MenuOption',
            data: model,
            success: function (result) {
                if (result != null) {
                    if (result.status == 200) {
                        $('#SidebarMenu').html(result.data);
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
    },
    GetCategory: function (_id) {
        // model 
        var _option_default = `<li>
                                    <input id="cbxItem0" type="checkbox" class="filled-in" data-id='' data-isdefault='true' />
                                    <label for="cbxItem0">Tạo mới</label>
                               </li>`;
        var model = {};
        AjaxFrom.POST({
            url: URLC + '/MenuOption',
            data: model,
            success: function (result) {
                $('ul#SidebarMenu').html(_option_default);
                if (result != null) {
                    if (result.status == 200) {
                        var rowData = '';
                        $.each(result.data, function (index, item) {
                            index = index + 1;
                            var id = item.ID;
                            if (id.length > 0)
                                id = id.trim();
                            var _title = SubStringText.SubTitle(item.Title);
                            var subMenu = item.SubMenu;
                            var iAngle = '';
                            if (subMenu != undefined && subMenu != null && subMenu.length > 0)
                                iAngle = "<i class='fa fa-angle-left iplus'></i>";
                            var isChecked = "";
                            if (id != null && id == _id) {
                                isChecked = "checked";
                                $("select#ddlPageTemlate").attr('disabled', true).closest(".bootstrap-select").find("button").attr('disabled', true);
                                $('select#ddlPageTemlate').selectpicker('refresh');
                            }
                            //
                            var _level = 0;
                            rowData += `<li>
                                        <input id="cbxItem${id}" data-issub='${item.IsSub}' type="checkbox" class="filled-in" data-id='${id}' ${isChecked} />
                                        <label for="cbxItem${id}">${_title}</label>`;
                            if (subMenu != undefined && subMenu != null && subMenu.length > 0) {
                                rowData += appMenuController.GetSubCategory(index, subMenu, _level, _id);
                            }
                            rowData += '</li>';


                        });
                        $('ul#SidebarMenu').html(_option_default + rowData);
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
                var subMenu = item.SubMenu;
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
                    rowData += appMenuController.GetSubCategory(_index, subMenu, _level, _id);
                }
                rowData += '</li>';
            });
            rowData += `</ul>`;
        }
        return rowData;
    }
};
appMenuController.init();
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
$(document).on('click', '#cbxPermission', function () {
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
$(document).on("click", "#btnFile", function () {
    $('#ImageFile').click();
});
$(document).on("change", "#ImageFile", function (elm) {
    $('#fake-file-input-name').val($(this).val());
});
//$(document).on("change", "#ddlOrder", function () {
//    var ddlOrder = $(this).val();
//    if (parseInt(ddlOrder) == 0)
//        $('#lblOrder').html('Vui lòng chọn vị trí sắp xếp');
//    else
//        $('#lblOrder').html('');
//});
$(document).on("change", "#ddlPageTemlate", function () {
    var ddlPageTemlate = $(this).val();
    if (ddlPageTemlate == "")
        $('#lblPageTemlate').html('Vui lòng chọn mẫu hiển thị');
    else
        $('#lblPageTemlate').html('');
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
                    appMenuController.DataList(pageIndex);
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
                    appMenuController.DataList(pageIndex);
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

//$(document).on("change", ".vetical-menu input[type='checkbox']", function () {
//    var menuId = $(this).data('id');
//    $('#lblPageTemlate').html('');
//    if (menuId != undefined && menuId != "") {
//        $("select#ddlPageTemlate")[0].selectedIndex = 0;
//        $("select#ddlPageTemlate").attr('disabled', true).closest(".bootstrap-select").find("button").attr('disabled', true);
//        $('select#ddlPageTemlate').selectpicker('refresh');
//    }
//    else {
//        $("select#ddlPageTemlate").removeAttr('disabled').closest(".bootstrap-select").find("button").removeAttr('disabled');
//        $('select#ddlPageTemlate').selectpicker('refresh');
//    }
//});


