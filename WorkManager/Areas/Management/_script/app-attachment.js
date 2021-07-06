var pageIndex = 1;
var URLC = "/Management/Attachment/Action";
var URLA = "/Management/Attachment";
var attachmentController = {
    init: function () {
        attachmentController.registerEvent();
    },
    registerEvent: function () {
        $("#btnUpload").off("click").on("click", function () {
            var flg = true;
            var txtTitle = $("#txtTitle").val();
            var _file = $("#inputUploadFile")[0].files[0];
            console.log(_file);
            var ddlCategory = $("#ddlCategory input[type='checkbox']:checked").data('id');
            //
            $("#lblTitle").html("");
            if (txtTitle != "") {
                if (txtTitle.length < 1 || txtTitle.length > 80) {
                    $("#lblTitle").html("Tên tài liệu giới hạn [1-80] ký tự");
                    flg = false;
                }
                else if (!FormatKeyword.test(txtTitle)) {
                    $("#lblTitle").html("Tên tài liệu không hợp lệ");
                    flg = false;
                }
            }
            //
            $("#lblCategory").html("");
            if (ddlCategory == undefined || ddlCategory == "") {
                $("#lblCategory").html("Vui lòng chọn nhóm tệp tin");
                flg = false;
            }
            //
            $("#lblFile").html("");
            if (_file == undefined || _file == null) {
                $("#lblFile").html("Vui lòng chọn tệp tin");
                flg = false;
            }
            else if (!IFile.IsFile(_file.name)) {
                $("#lblFile").html("Tệp tin không hợp lệ");
                $("#inputUploadFile").val("");
                $("#inputFileControl").html("");
                flg = false;
            }
            // submit
            if (flg) {
                attachmentController.Create();
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
        });
        $("#btnSearch").off("click").on("click", function () {
            attachmentController.DataList(1);
        });
        $("#btnUpdate").off("click").on("click", function () {
            var flg = true;
            var txtTitle = $("#txtTitle").val();
            var _file = $("#inputUploadFile")[0].files[0];
            var ddlCategory = $("#ddlCategory input[type='checkbox']:checked").data('id');
            //
            $("#lblTitle").html("");
            if (txtTitle != "") {
                if (txtTitle.length < 1 || txtTitle.length > 80) {
                    $("#lblTitle").html("Tên tài liệu giới hạn [1-80] ký tự");
                    flg = false;
                }
                else if (!FormatKeyword.test(txtTitle)) {
                    $("#lblTitle").html("Tên tài liệu không hợp lệ");
                    flg = false;
                }
            }
            //
            $("#lblCategory").html("");
            if (ddlCategory == undefined || ddlCategory == "") {
                $("#lblCategory").html("Vui lòng chọn nhóm tệp tin");
                flg = false;
            }
            //
            $("#lblFile").html("");
            if (_file != null && _file != undefined) {
                console.log(_file);
                if (!IFile.IsFile(_file.name)) {
                    $("#lblFile").html("Tệp tin không hợp lệ");
                    $("#inputUploadFile").val("");
                    $("#inputFileControl").html("");
                    flg = false;
                }
            }
            // submit
            if (flg) {
                attachmentController.Update();
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
        });
        //
        $(document).on("keyup", "#txtTitle", function () {
            var txtTitle = $(this).val();
            $("#lblTitle").html("");
            if (txtTitle != "") {
                if (txtTitle.length < 1 || txtTitle.length > 80) {
                    $("#lblTitle").html("Tên tài liệu giới hạn [1-80] ký tự");
                }
                else if (!FormatKeyword.test(txtTitle)) {
                    $("#lblTitle").html("Tên tài liệu không hợp lệ");
                }
            }

        });
        //
        $(document).on("click", "#btnOpenFile", function () {
            $("#inputUploadFile").click();
        });
        //
        $(document).on("change", "#inputUploadFile", function (elm) {
            var _file = $(this)[0].files[0];
            $("#inputFileControl").html("");
            $("#lblFile").html("");
            if (_file != null && _file != undefined) {
                if (!IFile.IsFile(_file.name)) {
                    $("#lblFile").html("Tệp tin không hợp lệ");
                    $(this).val("");
                    $("#inputFileControl").html("");
                    return;
                }
                //
                var reName = _file.name.replace(/[^a-zA-Z0-9_\-. ]/g, "");
                $("#txtTitle").val(reName);
                $("#inputFileControl").html(SubStringText.SubFileName(reName));
            }
        });

        $(document).on("change", "#ddlCategory input[type='checkbox']", function () {
            var ddlCategory = $("#ddlCategory input[type='checkbox']:checked").data("id");
            if (ddlCategory == undefined || ddlCategory == "") {
                $('#lblCategory').html('Vui lòng chọn nhóm tệp tin');
            }
            else {
                $('#lblCategory').html('');
            }
        });

        $(document).on("change", "#cbxSharedAll", function () {
            var cbxSharedAll = $(this).is(":checked");
            if (cbxSharedAll) {
                $('#ddlUser').prop({
                    disabled: true
                });
                $("select#ddlUser")[0].selectedIndex = -1;
                $('select#ddlUser').selectpicker('refresh');
            }
            else {
                $('#ddlUser').prop({
                    disabled: false
                }).selectpicker('refresh');
            }
        });
    },
    DataList: function (page) {
        //
        var ddlTimeExpress = $("#ddlTimeExpress").val();
        var txtStartDate = $("#txtStartDate").val();
        var txtEndDate = $("#txtEndDate").val();
        var ddlShared = $("#ddlShared").val();
        var ddlType = $("#ddlType").val();
        var model = {
            Query: $("#txtQuery").val(),
            Page: page,
            //TimeExpress: parseInt(ddlTimeExpress),
            StartDate: txtStartDate,
            EndDate: txtEndDate,
            TimeZoneLocal: LibDateTime.GetTimeZoneByLocal(),
            FileType: parseInt(ddlType),
            IsShared: parseInt(ddlShared),
        };
        //
        AjaxFrom.POST({
            url: URLC + "/DataList",
            data: model,
            success: function (result) {
                $("tbody#TblData").html("");
                $("#Pagination").html("");
                if (result !== null) {
                    if (result.status == 200) {
                        var currentPage = 1;
                        var pagination = result.paging;
                        if (pagination !== null) {
                            totalPage = pagination.TotalPage;
                            currentPage = pagination.Page;
                            pageSize = pagination.PageSize;
                            pageIndex = pagination.Page;
                        }
                        var rowData = "";
                        $.each(result.data, function (index, item) {
                            index = index + 1;
                            var id = item.ID;
                            if (id.length > 0)
                                id = id.trim();
                            //  role
                            var action = HelperModel.RolePermission(result.role, "attachmentController", id);
                            //
                            var rowNum = parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                            //`<i class="fas fa-check-circle"></i>` <i class="far fa-circle"></i>
                            rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>
                                 <td>${item.CategoryName}</td>                                                            
                                 <td>${item.Title}</td>                                                            
                                 <td class="text-center">${HelperModel.ShareFileIcon(item.IsShared)}</td>                                                            
                                 <td>${item.CreatedBy}</td>                                  
                                 <td class="text-center">${item.CreatedDate}</td>
                                 <td class="tbcol-action">${action}</td>
                            </tr>`;
                        });
                        $("tbody#TblData").html(rowData);
                        if (parseInt(totalPage) > 1) {
                            Paging.Pagination("#Pagination", totalPage, currentPage, attachmentController.DataList);
                        }
                        return;
                    }
                    else {
                        //Notifization.Error(result.message);
                        console.log("::" + result.message);
                        return;
                    }
                }
                Notifization.Error(MessageText.NotService);
                return;
            },
            error: function (result) {
                console.log("::" + MessageText.NotService);
            }
        });
    },
    Create: function () {
        var name = $("#txtTitle").val();
        var _file = $("#inputUploadFile")[0].files[0];
        var ddlCategory = $("#ddlCategory input[type='checkbox']:checked").data("id");
        var ddlUser = $("#ddlUser").val();
        var ddlPermission = $("#ddlPermission").val(); 
        var sharedAll = $("input[id='cbxSharedAll']").is(":checked");
        if (sharedAll) {
            ddlUser = null;
        } 
        var model = new FormData();
        model.append("DocumentFile", _file);
        model.append("CategoryID", ddlCategory);
        model.append("FileName", name);
        model.append("UserList", ddlUser);
        model.append("Permissions", ddlPermission);
        model.append("SharedAll", sharedAll);
        //
        AjaxFrom.POSTFILE({
            url: URLC + "/Create",
            data: model,
            success: function (response) {
                if (response !== null) {
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
                console.log("::" + MessageText.NotService);
            }
        });

    },
    Update: function () {
        var id = $("#txtID").val();
        var name = $("#txtTitle").val();
        var _file = $("#inputUploadFile")[0].files[0];
        var ddlCategory = $("#ddlCategory input[type='checkbox']:checked").data("id");
        var ddlUser = $("#ddlUser").val();
        var ddlPermission = $("#ddlPermission").val();
        var sharedAll = $("input[id='cbxSharedAll']").is(":checked");
        if (sharedAll) {
            ddlUser = null; 
        } 
        var model = new FormData();
        model.append("ID", id);
        model.append("DocumentFile", _file);
        model.append("CategoryID", ddlCategory);
        model.append("FileName", name);
        model.append("UserList", ddlUser);
        model.append("Permissions", ddlPermission);
        model.append("SharedAll", sharedAll);
        //
        AjaxFrom.POSTFILE({
            url: URLC + "/Update",
            data: model,
            success: function (response) {
                if (response !== null) {
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
                console.log("::" + MessageText.NotService);
            }
        });
    },
    Delete: function (id) {
        var model = {
            Id: id
        };
        AjaxFrom.POST({
            url: URLC + "/Delete",
            data: model,
            success: function (response) {
                if (response !== null) {
                    if (response.status == 200) {
                        Notifization.Success(response.message);
                        attachmentController.DataList(pageIndex);
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
                console.log("::" + MessageText.NotService);
            }
        });
    },
    Details: function () {
        var id = $("#txtID").val();
        if (id.length <= 0) {
            Notifization.Error(MessageText.NotService);
            return;
        }
        var fData = {
            Id: $("#txtID").val()
        };
        $.ajax({
            url: "/post/detail",
            data: {
                strData: JSON.stringify(fData)
            },
            type: "POST",
            dataType: "json",
            success: function (result) {
                if (result !== null) {
                    if (result.status == 200) {
                        var item = result.data;
                        $("#LblAccount").html(item.LoginID);
                        $("#LblDate").html(item.CreatedDate);
                        var action = "";
                        if (item.Enabled)
                            action += `<i class="fa fa-toggle-on"></i> actived`;
                        else
                            action += `<i class="fa fa-toggle-off"></i>not active`;

                        $("#LblActive").html(action);
                        $("#lblLastName").html(item.FirstName + " " + item.LastName);
                        $("#LblEmail").html(item.Email);
                        $("#LblPhone").html(item.Phone);
                        $("#LblLanguage").html(item.LanguageID);
                        $("#LblPermission").html(item.PermissionID);

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
                console.log("::" + MessageText.NotService);
            }
        });
    },
    ConfirmDelete: function (id) {
        Confirm.Delete(id, attachmentController.Delete, null, null);
    }
};

attachmentController.init();


