
var pageIndex = 1;
var URLC = "/Management/WorkStack/Action";
var URLA = "/Management/WorkStack";
var workStackController = {
    init: function () {
        workStackController.registerEvent();
    },
    registerEvent: function () {
        $("#btnCreate").off("click").on("click", function () {
            var flg = true;
            var txtTitle = $("#txtTitle").val();
            var ddlWork = $("#ddlWork").val();
            var txtContent = tinyMCE.editors[$("#txtContent").attr("id")].getContent();
            var txtExecuteDate = $("#txtExecuteDate").val();
            var txtDeadline = $("#txtDeadline").val();
            //
            $("#lblTitle").html("");
            if (txtTitle === "") {
                $("#lblTitle").html("Nhập tên tác vụ");
                flg = false;
            }
            else if (txtTitle.length < 1 || txtTitle.length > 80) {
                $("#lblTitle").html("Tên tác vụ giới hạn [1-80] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtTitle)) {
                $("#lblTitle").html("Tên tác vụ không hợp lệ");
                flg = false;
            }
            //

            $("#lblContent").html("");
            if (txtContent == "") {
                $("#lblContent").html("Nhập nội dung");
                flg = false;
            }
            else if (txtContent.length > 5000) {
                $("#lblContent").html("Nội dung giới hạn [1-5000] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtContent)) {
                $("#lblContent").html("Nội dung không hợp lệ");
                flg = false;
            }
            //
            $("#lblWork").html("");
            if (ddlWork === "") {
                $("#lblWork").html("Chọn công việc");
                flg = false;
            }
            //  
            $("#lblExecuteDate").html("");
            if (txtExecuteDate == "") {
                $("#lblExecuteDate").html("Nhập ngày thực hiện");
                flg = false;
            }
            else if (!ValidData.ValidDate(txtExecuteDate, "vn")) {
                $("#lblExecuteDate").html("Ngày thực hiện không hợp lệ");
                flg = false;
            }
            //
            $("#lblDeadline").html("");
            if (txtDeadline == "") {
                $("#lblDeadline").html("Nhập ngày hoàn thiện");
                flg = false;
            }
            else if (!ValidData.ValidDate(txtDeadline, "vn")) {
                $("#lblDeadline").html("Ngày hoàn thiện không hợp lệ");
                flg = false;
            }
            // submit
            if (flg) {
                workStackController.Create();
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
        });
        $("#btnSearch").off("click").on("click", function () {
            workStackController.DataList(1);
        });
        $("#btnUpdate").off("click").on("click", function () {
            var flg = true;
            var txtTitle = $("#txtTitle").val();
            var ddlWork = $("#ddlWork").val();
            var txtContent = tinyMCE.editors[$("#txtContent").attr("id")].getContent();
            var txtExecuteDate = $("#txtExecuteDate").val();
            var txtDeadline = $("#txtDeadline").val();
            //
            $("#lblTitle").html("");
            if (txtTitle === "") {
                $("#lblTitle").html("Nhập tên tác vụ");
                flg = false;
            }
            else if (txtTitle.length < 1 || txtTitle.length > 80) {
                $("#lblTitle").html("Tên tác vụ giới hạn [1-80] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtTitle)) {
                $("#lblTitle").html("Tên tác vụ không hợp lệ");
                flg = false;
            }
            //
            console.log(txtContent);
            $("#lblContent").html("");
            if (txtContent == "") {
                $("#lblContent").html("Nhập nội dung");
                flg = false;
            }
            else if (txtContent.length > 5000) {
                $("#lblContent").html("Nội dung giới hạn [1-5000] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtContent)) {
                $("#lblContent").html("Nội dung không hợp lệ");
                flg = false;
            }
            $("#lblWork").html("");
            if (ddlWork === "") {
                $("#lblWork").html("Chọn công việc");
                flg = false;
            }
            //
            $("#lblExecuteDate").html("");
            if (txtExecuteDate == "") {
                $("#lblExecuteDate").html("Nhập ngày thực hiện");
                flg = false;
            }
            else if (!ValidData.ValidDate(txtExecuteDate, "vn")) {
                $("#lblExecuteDate").html("Ngày thực hiện không hợp lệ");
                flg = false;
            }
            //
            $("#lblDeadline").html("");
            if (txtDeadline == "") {
                $("#lblDeadline").html("Nhập ngày hoàn thiện");
                flg = false;
            }
            else if (!ValidData.ValidDate(txtDeadline, "vn")) {
                $("#lblDeadline").html("Ngày hoàn thiện không hợp lệ");
                flg = false;
            }
            // submit
            if (flg) {
                workStackController.Update();
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
                            //
                            var _title = SubStringText.SubTitle(item.Title);
                            // icon sort
                            var _level = 0;
                            var _orderId = item.OrderID;
                            //var _actionSort = `<i data-sortup="btn-sort-up" data-id="${id}" data-order"${_orderId}" class="fas fa-arrow-circle-up icon-mnsort"></i> <i data-sortdown ="btn-sort-down" data-id="${id}" data-order"${_orderId}" class="fas fa-arrow-circle-down icon-mnsort"></i>`;
                            //
                            //  role 
                            var action = HelperModel.RolePermission(result.role, "workStackController", id);
                            //
                            var rowNum = parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                            rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>
                                 <td>${item.WorkName}</td>                                                                 
                                 <td>${_title}</td>                                                                 
                                 <td class="text-center">${item.ExecuteDate}</td>
                                 <td class="text-center">${item.Deadline}</td>
                                 <td class="text-center">${workStackController.StateIcon(item.State)}</td>
                                 <td class="tbcol-action">${action}</td>
                            </tr>`;
                        });
                        $("tbody#TblData").html(rowData);
                        if (parseInt(totalPage) > 1) {
                            Paging.Pagination("#Pagination", totalPage, currentPage, workStackController.DataList);
                        }
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
    Create: function () {
        var txtTitle = $("#txtTitle").val();
        var ddlWork = $("#ddlWork").val();
        var txtContent = tinyMCE.editors[$("#txtContent").attr("id")].getContent();
        var txtExecuteDate = $("#txtExecuteDate").val();
        var txtDeadline = $("#txtDeadline").val();
        var ddlState = $("#ddlState").val();
        var model = {
            WorkID: ddlWork,
            Title: txtTitle,
            HtmlText: txtContent,
            ExecuteDate: txtExecuteDate,
            Deadline: txtDeadline,
            State: ddlState
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
        var id = $("#txtID").val();
        var ddlWork = $("#ddlWork").val();
        var txtTitle = $("#txtTitle").val();
        var txtContent = tinyMCE.editors[$("#txtContent").attr("id")].getContent();
        var txtExecuteDate = $("#txtExecuteDate").val();
        var txtDeadline = $("#txtDeadline").val();
        var state = $("input[name='cbxState']").is(":checked"); 
        //
        var model = {
            ID: id,
            WorkID: ddlWork,
            Title: txtTitle,
            HtmlText: txtContent,
            ExecuteDate: txtExecuteDate,
            Deadline: txtDeadline,
            State: state
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
                        workStackController.DataList(pageIndex);
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
        Confirm.Delete(id, workStackController.Delete, null, null);
    },
    StateIcon(_status) {
        var result = `<i class="far fa-circle"></i>`;
        if (_status)  
            return `<i class="fas fa-check-circle"></i>`;
        //
        return result;
    }
};

workStackController.init();
$(document).on("keyup", "#txtTitle", function () {
    var txtTitle = $(this).val();
    if (txtTitle === "") {
        $("#lblTitle").html("Nhập tên tác vụ");
    }
    else if (txtTitle.length < 1 || txtTitle.length > 80) {
        $("#lblTitle").html("Tên tác vụ giới hạn [1-80] ký tự");
    }
    else if (!FormatKeyword.test(txtTitle)) {
        $("#lblTitle").html("Tên tác vụ không hợp lệ");
    }
    else {
        $("#lblTitle").html("");
    }
});

$(document).on("blue", "#lblContent", function () {
    var txtContent = $(this).val();
    $("#lblContent").html("");
    if (txtContent == "") {
        $("#lblContent").html("Nhập nội dung");
    }
    else if (txtContent.length > 5000) {
        $("#lblContent").html("Nội dung giới hạn [1-5000] ký tự");
    }
    else if (!FormatKeyword.test(txtContent)) {
        $("#lblContent").html("Nội dung không hợp lệ");
    }
});

$(document).on("keyup", "#txtExecuteDate", function () {
    var txtExecuteDate = $(this).val();
    $("#lblExecuteDate").html("");
    if (txtExecuteDate == "") {
        $("#lblExecuteDate").html("Nhập ngày thực hiện");
    }
    else if (!ValidData.ValidDate(txtExecuteDate, "vn")) {
        $("#lblExecuteDate").html("Ngày hiển thị không hợp lệ");
    }
});

$(document).on("keyup", "#txtDeadline", function () {
    var txtDeadline = $(this).val();
    $("#lblDeadline").html("");
    if (txtDeadline == "") {
        $("#lblDeadline").html("Nhập ngày hoàn thiện");
    }
    else if (!ValidData.ValidDate(txtDeadline, "vn")) {
        $("#lblDeadline").html("Ngày hoàn thiện không hợp lệ");
    }
});


$(document).on("change", "#ddlWork", function () {
    var ddlWork = $(this).val();
    $("#lblWork").html("");
    if (ddlWork === "") {
        $("#lblWork").html("Chọn công việc");
    }
});

// menu sort
$(document).on("click", "[data-sortup]", function () {
    var id = $(this).data("id");
    var model = {
        ID: id
    };
    AjaxFrom.POST({
        url: URLC + "/sortup",
        data: model,
        success: function (result) {
            if (result !== null) {
                if (result.status === 200) {
                    Notifization.Success(result.message);
                    workStackController.DataList(pageIndex);
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
});
$(document).on("click", "[data-sortdown]", function () {
    var id = $(this).data("id");
    var model = {
        ID: id
    };
    AjaxFrom.POST({
        url: URLC + "/sortdown",
        data: model,
        success: function (result) {
            if (result !== null) {
                if (result.status === 200) {
                    Notifization.Success(result.message);
                    workStackController.DataList(pageIndex);
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
});

