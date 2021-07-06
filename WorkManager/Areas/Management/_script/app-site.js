 var pageIndex = 1;
var URLC = "/Management/Site/Action";
var URLA = "/Management/Site";
var arrFile = [];
var siteController = {
    init: function () {
        siteController.registerEvent();
    },
    registerEvent: function () {
        $(document).ready(function () {
            // $("[data-date="true"]").val(DateTime.Get_ClientDate(lg = "en"));
        });
        $("#btnCreate").off("click").on("click", function () {
            var flg = true;
            var title = $("#txtTitle").val();
            var summary = $("#txtSummary").val();
            var txtEmail = $("#txtEmail").val();
            var txtTel = $("#txtTel").val();
            var txtPhone = $("#txtPhone").val();
            var txtAddress = $("#txtAddress").val();
            // 
            $("#lblTitle").html("");
            if (title === "") {
                $("#lblTitle").html("Không được để trống tiêu đề");
                flg = false;
            }
            else if (title.length < 1 || title.length > 80) {
                $("#lblTitle").html("Tiêu đề giới hạn [1-80] characters");
                flg = false;
            }
            else if (!FormatKeyword.test(title)) {
                $("#lblTitle").html("Tiêu đề không hợp lệ");
                flg = false;
            }
            //  
            $("#lblSummary").html("");
            if (summary !== "") {
                if (summary.length < 1 || summary.length > 120) {
                    $("#lblSummary").html("Mô tả giới hạn [1-120] ký tự");
                    flg = false;
                }
                else if (!FormatKeyword.test(summary)) {
                    $("#lblSummary").html("Mô tả không hợp lệ");
                    flg = false;
                }
            }
            // email validate 
            $("#lblEmail").html("");
            if (txtEmail === "") {
                $("#lblEmail").html("Không được để trống địa chỉ email");
                flg = false;
            }
            else if (!FormatEmail.test(txtEmail)) {
                $("#lblEmail").html("Địa chỉ email không hợp lệ");
                flg = false;
            }
            // phonnumber validate 
            $("#lblTel").html("");
            if (txtTel === "") {
                $("#lblTel").html("Không được để trống số điện thoại");
                flg = false;
            }
            else if (!FormatPhone.test(txtTel)) {
                $("#lblTel").html("Số điện thoại không hợp lệ");
                flg = false;
            }

            // phonnumber validate 
            $("#lblPhone").html("");
            if (txtPhone !== "") {
                if (!FormatPhone.test(txtPhone)) {
                    $("#lblPhone").html("Hotline không hợp lệ");
                    flg = false;
                }
            }
            // address validate
            $("#lblAddress").html("");
            if (txtAddress === "") {
                $("#lblAddress").html("Không được để trống địa chỉ");
                flg = false;
            }
            else if (txtAddress.length < 1 || txtAddress.length > 80) {
                $("#lblAddress").html("Địa chỉ giới hạn [1-80] ký tự");
                flg = false;
            }
            else if (txtAddress !== "") {
                if (!FormatKeyword.test(txtAddress)) {
                    $("#lblAddress").html("Địa chỉ không hợp lệ");
                    flg = false;
                }
            }
            // submit form
            if (flg)
                siteController.Create();
            else
                Notifization.Error(MessageText.Datamissing + "1");
        });
        $("#btnSearch").off("click").on("click", function () {
            siteController.DataList(1);
        });
        $("#btnUpdate").off("click").on("click", function () {
            var flg = true;
            var title = $("#txtTitle").val();
            var summary = $("#txtSummary").val();
            var txtEmail = $("#txtEmail").val();
            var txtTel = $("#txtTel").val();
            var txtPhone = $("#txtPhone").val();
            var txtAddress = $("#txtAddress").val();
            // 
            $("#lblTitle").html("");
            if (title === "") {
                $("#lblTitle").html("Không được để trống tiêu đề");
                flg = false;
            }
            else if (title.length < 1 || title.length > 80) {
                $("#lblTitle").html("Tiêu đề giới hạn [1-80] characters");
                flg = false;
            }
            else if (!FormatKeyword.test(title)) {
                $("#lblTitle").html("Tiêu đề không hợp lệ");
                flg = false;
            }
            //  
            $("#lblSummary").html("");
            if (summary !== "") {
                if (summary.length < 1 || summary.length > 120) {
                    $("#lblSummary").html("Mô tả giới hạn [1-120] ký tự");
                    flg = false;
                }
                else if (!FormatKeyword.test(summary)) {
                    $("#lblSummary").html("Mô tả không hợp lệ");
                    flg = false;
                }
            }
            // email validate 
            $("#lblEmail").html("");
            if (txtEmail === "") {
                $("#lblEmail").html("Không được để trống địa chỉ email");
                flg = false;
            }
            else if (!FormatEmail.test(txtEmail)) {
                $("#lblEmail").html("Địa chỉ email không hợp lệ");
                flg = false;
            }
            // phonnumber validate 
            $("#lblTel").html("");
            if (txtTel === "") {
                $("#lblTel").html("Không được để trống số điện thoại");
                flg = false;
            }
            else if (!FormatPhone.test(txtTel)) {
                $("#lblTel").html("Số điện thoại không hợp lệ");
                flg = false;
            }

            // phonnumber validate 
            $("#lblPhone").html("");
            if (txtPhone !== "") {
                if (!FormatPhone.test(txtPhone)) {
                    $("#lblPhone").html("Hotline không hợp lệ");
                    flg = false;
                }
            }
            // address validate
            $("#lblAddress").html("");
            if (txtAddress === "") {
                $("#lblAddress").html("Không được để trống địa chỉ");
                flg = false;
            }
            else if (txtAddress.length < 1 || txtAddress.length > 80) {
                $("#lblAddress").html("Địa chỉ giới hạn [1-80] ký tự");
                flg = false;
            }
            else if (txtAddress !== "") {
                if (!FormatKeyword.test(txtAddress)) {
                    $("#lblAddress").html("Địa chỉ không hợp lệ");
                    flg = false;
                }
            }
            // submit form
            if (flg) {
                siteController.Update();
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
        });
    },
    Create: function () {
        var id = $("#txtID").val();
        var title = $("#txtTitle").val();
        var summary = $("#txtSummary").val();
        var enabled = 0;
        if ($("input[name='cbxActive']").is(":checked"))
            enabled = 1;
        //
        var _logoFile = "";
        if ($("#logoFile img") != undefined)
            _logoFile = AttachmentFile.FileName($("#logoFile img").attr("src"));
        //
        var _iconFile = "";
        if ($("#iconFile img") != undefined)
            _iconFile = AttachmentFile.FileName($("#iconFile img").attr("src"));
        //
        var email = $("#txtEmail").val();
        var fax = $("#txtFax").val();
        var tel = $("#txtTel").val();
        var hotline = $("#txtHotline").val();
        var workTime = $("#txtWorkTime").val();
        var address = $("#txtAddress").val();
        var gmap = $("#txtGmap").val();
        var googleAnalytic = $("#txtGoogleAnalytic").val();
        var fanpage = $("#txtFanpage").val();
        //
        var model = {
            Title: title,
            Summary: summary,
            IconFile: _iconFile,
            ImageFile: _logoFile,
            Email: email,
            Fax: fax,
            Phone: hotline,
            Tel: tel,
            WorkTime: workTime,
            Address: address,
            Gmaps: gmap,
            GoogleAnalytic: googleAnalytic,
            Fanpage: fanpage,
            Enabled: enabled
        };
        AjaxFrom.POST({
            url: URLC + "/Create",
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
    Update: function () {
        var id = $("#txtID").val();
        var title = $("#txtTitle").val();
        var summary = $("#txtSummary").val();
        var enabled = 0;
        if ($("input[name='cbxActive']").is(":checked"))
            enabled = 1;
        //
        var _logoFile = "";
        if ($("#logoFile img") != undefined)
            _logoFile = AttachmentFile.FileName($("#logoFile img").attr("src"));
        //
        var _iconFile = "";
        if ($("#iconFile img") != undefined)
            _iconFile = AttachmentFile.FileName($("#iconFile img").attr("src"));
        //
        var email = $("#txtEmail").val();
        var fax = $("#txtFax").val();
        var tel = $("#txtTel").val();
        var hotline = $("#txtHotline").val();
        var workTime = $("#txtWorkTime").val();
        var address = $("#txtAddress").val();
        var gmap = $("#txtGmap").val();
        var googleAnalytic = $("#txtGoogleAnalytic").val();
        var fanpage = $("#txtFanpage").val();
        //
        var model = {
            ID: id,
            Title: title,
            Summary: summary,
            IconFile: _iconFile,
            ImageFile: _logoFile,
            Email: email,
            Fax: fax,
            Phone: hotline,
            Tel: tel,
            WorkTime: workTime,
            Address: address,
            Gmaps: gmap,
            GoogleAnalytic: googleAnalytic,
            Fanpage: fanpage,
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
    DataList: function (page) {
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
                            var action = HelperModel.RolePermission(result.role, "siteController", id);
                            // 
                            var rowNum = parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                            var _title = SubStringText.SubTitle(item.Title);
                            rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>   
                                 <td class="text-left">${_title}</td>
                                 <td class="text-left">${item.Tel}</td>
                                 <td class="text-left">${item.Phone}</td>
                                 <td class="text-left">${item.Email}</td>                               
                                 <td class="text-center">${HelperModel.StatusIcon(item.Enabled)}</td>
                                 <td class="text-center">${item.CreatedDate}</td>
                                 <td class="tbcol-action">${action}</td>
                            </tr>`;
                        });
                        $("tbody#TblData").html(rowData);
                        if (parseInt(totalPage) > 1) {
                            Paging.Pagination("#Pagination", totalPage, currentPage, siteController.DataList);
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
    Delete: function (id) {
        var model = {
            ID: id
        };
        AjaxFrom.POST({
            url: URLC + "/Delete",
            data: model,
            success: function (response) {
                if (response != null) {
                    if (response.status == 200) {
                        Notifization.Success(response.message);
                        siteController.DataList(pageIndex);
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
    ConfirmDelete: function (id) {
        Confirm.Delete(id, siteController.Delete, null, null);
    },
};
siteController.init();
$(document).on("keyup", "#txtTitle", function () {
    var txtTitle = $(this).val();
    if (txtTitle === "") {
        $("#lblTitle").html("Không được để trống tiêu đề");
    }
    else if (txtTitle.length < 1 || txtTitle.length > 80) {
        $("#lblTitle").html("Tiêu đề giới hạn [1-80] ký tự");
    }
    else if (!FormatKeyword.test(txtTitle)) {
        $("#lblTitle").html("Tiêu đề không hợp lệ");
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
$(document).on("keyup", "#txtEmail", function () {
    var txtEmail = $(this).val();
    if (txtEmail === "") {
        $("#lblEmail").html("Không được để trống địa chỉ email");
    }
    else {
        if (!FormatEmail.test(txtEmail)) {
            $("#lblEmail").html("Địa chỉ email không hợp lệ");
        }
        else {
            $("#lblEmail").html("");
        }
    }
});

$(document).on("keyup", "#txtTel", function () {
    var txtTel = $(this).val();
    $("#lblTel").html("");
    if (txtTel === "") {
        $("#lblTel").html("Không được để trống số điện thoại");
    }
    else if (!FormatPhone.test(txtTel)) {
        $("#lblTel").html("Số điện thoại không hợp lệ");
    }
});

$(document).on("keyup", "#txtPhone", function () {
    var txtPhone = $(this).val();
    $("#lblPhone").html("");
    if (txtPhone !== "") {
        if (!FormatPhone.test(txtPhone)) {
            $("#lblPhone").html("Hotline không hợp lệ");
        }
    }
});
$(document).on("keyup", "#txtAddress", function () {
    var txtAddress = $(this).val();
    $("#lblAddress").html("");
    if (txtAddress === "") {
        $("#lblAddress").html("Không được để trống địa chỉ");
    }
    else if (txtAddress.length < 1 || txtAddress.length > 80) {
        $("#lblAddress").html("Địa chỉ giới hạn [1-80] ký tự");
    }
    else if (txtAddress !== "") {
        if (!FormatKeyword.test(txtAddress)) {
            $("#lblAddress").html("Địa chỉ không hợp lệ");
        }
    }
});

$(document).on("click", "#cbxActive", function () {
    if ($(this).hasClass("actived")) {
        // remove
        $(this).children("i").removeClass("fa-check-square");
        $(this).children("i").addClass("fa-square");
        $(this).removeClass("actived");
    }
    else {
        $(this).children("i").addClass("fa-check-square");
        $(this).children("i").removeClass("fa-square");
        $(this).addClass("actived");
    }
});