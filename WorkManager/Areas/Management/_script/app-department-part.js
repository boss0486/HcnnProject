var pageIndex = 1;
var URLC = "/Management/DepartmentPart/Action";
var URLA = "/Management/DepartmentPart";
var departmentPartController = {
    init: function () {
        departmentPartController.registerEvent();
    },
    registerEvent: function () {
        $("#btnCreate").off("click").on("click", function () {
            var flg = true;
            var ddlDepartment = $("#ddlDepartment").val();
            var txtTitle = $("#txtTitle").val();
            var txtSummary = $("#txtSummary").val();
            //
            $("#lblDepartment").html("");
            if (ddlDepartment === "") {
                $("#lblDepartment").html("Vui lòng chọn phòng ban");
                flg = false;
            }
            //
            if (txtTitle === "") {
                $("#lblTitle").html("Không được để trống chức vụ");
                flg = false;
            }
            else if (txtTitle.length < 1 || txtTitle.length > 80) {
                $("#lblTitle").html("Chức vụ giới hạn [1-80] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtTitle)) {
                $("#lblTitle").html("Chức vụ không hợp lệ");
                flg = false;
            }
            else {
                $("#lblTitle").html("");
            }

            if (txtSummary !== "") {
                if (txtSummary.length > 120) {
                    $("#lblSummary").html("Mô tả giới hạn [1-120] ký tự");
                    flg = false;
                }
                else if (!FormatKeyword.test(txtSummary)) {
                    $("#lblSummary").html("Mô tả không hợp lệ");
                    flg = false;
                }
                else {
                    $("#lblSummary").html("");
                }
            }
            else {
                $("#lblSummary").html("");
            }
            // submit

            if (flg) {
                departmentPartController.Create();
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
        });
        $("#btnSearch").off("click").on("click", function () {
            departmentPartController.DataList(1);
        });
        $("#btnUpdate").off("click").on("click", function () {
            var flg = true;
            var ddlDepartment = $("#ddlDepartment").val();
            var txtTitle = $("#txtTitle").val();
            var txtSummary = $("#txtSummary").val();
            //
            $("#lblDepartment").html("");
            if (ddlDepartment === "") {
                $("#lblDepartment").html("Vui lòng chọn phòng ban");
                flg = false;
            }
            //
            if (txtTitle === "") {
                $("#lblTitle").html("Không được để trống chức vụ");
                flg = false;
            }
            else if (txtTitle.length < 1 || txtTitle.length > 80) {
                $("#lblTitle").html("Chức vụ giới hạn [1-80] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtTitle)) {
                $("#lblTitle").html("Chức vụ không hợp lệ");
                flg = false;
            }
            else {
                $("#lblTitle").html("");
            }

            if (txtSummary !== "") {
                if (txtSummary.length > 120) {
                    $("#lblSummary").html("Mô tả giới hạn [1-120] ký tự");
                    flg = false;
                }
                else if (!FormatKeyword.test(txtSummary)) {
                    $("#lblSummary").html("Mô tả không hợp lệ");
                    flg = false;
                }
                else {
                    $("#lblSummary").html("");
                }
            }
            else {
                $("#lblSummary").html("");
            }
            // submit
            if (flg) {
                departmentPartController.Update();
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
        });
    },
    DataList: function (page) {
        //
        var ddlTimeExpress = $("#ddlTimeExpress").val();
        var txtStartDate = $("#txtStartDate").val();
        var txtEndDate = $("#txtEndDate").val();
        var model = {
            Query: $("#txtQuery").val(),
            Page: page,
            TimeExpress: parseInt(ddlTimeExpress),
            StartDate: txtStartDate,
            EndDate: txtEndDate,
            TimeZoneLocal: LibDateTime.GetTimeZoneByLocal(),
            Status: parseInt($("#ddlStatus").val())
        };
        //
        AjaxFrom.POST({
            url: URLC + "/DataList",
            data: model,
            success: function (result) {
                $("tbody#TblData").html("");
                $("#Pagination").html("");
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
                        var rowData = "";
                        $.each(result.data, function (index, item) {
                            index = index + 1;
                            var id = item.ID;
                            if (id.length > 0)
                                id = id.trim();
                            //  role
                            var action = HelperModel.RolePermission(result.role, "departmentPartController", id);
                            //
                            var rowNum = parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                            rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>
                                 <td>${item.Title}</td>                                  
                                 <td>${item.DepartmentName}</td>                                  
                                 <td>${item.Summary}</td>                                  
                                 <td class="tbcol-created">${item.CreatedBy}</td>                                  
                                 <td class="text-center">${HelperModel.StatusIcon(item.Enabled)}</td>
                                 <td class="text-center">${item.CreatedDate}</td>
                                 <td class="tbcol-action">${action}</td>
                            </tr>`;
                        });
                        $("tbody#TblData").html(rowData);
                        if (parseInt(totalPage) > 1) {
                            Paging.Pagination("#Pagination", totalPage, currentPage, departmentPartController.DataList);
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
        var ddlDepartment = $("#ddlDepartment").val();
        var title = $("#txtTitle").val();
        var summary = $("#txtSummary").val();
        var enabled = 0;
        if ($("input[name='cbxActive']").is(":checked"))
            enabled = 1;
        //
        var model = {
            DepartmentID: ddlDepartment,
            Title: title,
            Summary: summary,
            Enabled: enabled
        };
        AjaxFrom.POST({
            url: URLC + "/Create",
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
                console.log("::" + MessageText.NotService);
            }
        });

    },
    Update: function () {
        var ddlDepartment = $("#ddlDepartment").val();
        var title = $("#txtTitle").val();
        var summary = $("#txtSummary").val();
        var id = $("#txtID").val();
        var enabled = 0;
        if ($("input[name='cbxActive']").is(":checked"))
            enabled = 1;
        //
        var model = {
            Id: id,
            DepartmentID: ddlDepartment,
            Title: title,
            Summary: summary,
            Enabled: enabled
        };
        AjaxFrom.POST({
            url: URLC + "/Update",
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
                    if (response.status === 200) {
                        Notifization.Success(response.message);
                        departmentPartController.DataList(pageIndex);
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
                    if (result.status === 200) {
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
        Confirm.Delete(id, departmentPartController.Delete, null, null);
    }
};

departmentPartController.init();
$(document).on("change", "#ddlDepartment", function () {
    var ddlDepartment = $(this).val();
    $("#lblDepartment").html("");
    if (ddlDepartment == undefined || ddlDepartment == "") {
        $("#lblDepartment").html("Vui lòng chọn phòng ban");
    } 
});
$(document).on("keyup", "#txtTitle", function () {
    var txtTitle = $(this).val();
    if (txtTitle === "") {
        $("#lblTitle").html("Không được để trống chức vụ");
    }
    else if (txtTitle.length < 1 || txtTitle.length > 80) {
        $("#lblTitle").html("Chức vụ giới hạn [1-80] ký tự");
    }
    else if (!FormatKeyword.test(txtTitle)) {
        $("#lblTitle").html("Chức vụ không hợp lệ");
    }
    else {
        $("#lblTitle").html("");
    }
});
$(document).on("keyup", "#txtSummary", function () {
    var txtSummary = $(this).val();
    if (txtSummary !== "") {
        if (txtSummary.length > 120) {
            $("#lblSummary").html("Mô tả giới hạn [1-120] ký tự");
            flg = false;
        }
        else if (!FormatKeyword.test(txtSummary)) {
            $("#lblSummary").html("Mô tả không hợp lệ");
            flg = false;
        }
        else {
            $("#lblSummary").html("");
        }
    }
    else {
        $("#lblSummary").html("");
    }
});

