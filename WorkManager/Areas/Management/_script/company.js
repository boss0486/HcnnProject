﻿var pageIndex = 1;
var URLC = "/Management/Company/Action";
var URLA = "/Management/Company";
var CompanyController = {
    init: function () {
        CompanyController.registerEvent();
    },
    registerEvent: function () {
        $("#btnCreate").off("click").on("click", function () {
            var flg = true;
            var ddlAgent = $("#ddlAgent").val();
            //
            var txtCodeID = $("#txtCodeID").val();
            var txtTitle = $("#txtTitle").val();
            var txtSummary = $("#txtSummary").val();
            var txtTaxCode = $("#txtTaxCode").val();
            var txtAddress = $("#txtAddress").val();
            var txtCompanyPhone = $("#txtCompanyPhone").val();
            //
            var txtContactName = $("#txtContactName").val();
            var txtContactEmail = $("#txtContactEmail").val();
            var txtContactPhone = $("#txtContactPhone").val();
            //
            var txtAccount = $("#txtAccount").val();
            var txtPassword = $("#txtPassword").val();
            var txtEmail = $("#txtEmail").val();
            var txtPhone = $("#txtPhone").val();
            var txtTermPayment = $("#txtTermPayment").val();
            // 
            $("#lblAgent").html("");
            if (HelperModel.AccessInApplication() != RoleEnum.IsAdminCustomerLogged) {
                if (ddlAgent == "") {
                    $("#lblAgent").html("Vui lòng chọn đại lý");
                    flg = false;
                } 
            }
            //
            if (txtCodeID === "") {
                $("#lblCodeID").html("Không được để trống MKH");
                flg = false;
            }
            else if (txtCodeID.length != 3) {
                $("#lblCodeID").html("Mã đại lý bao gồm 3 ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtCodeID)) {
                $("#lblCodeID").html("Mã đại lý không hợp lệ");
                flg = false;
            }
            else {
                $("#lblCodeID").html("");
            }
            //
            if (txtTitle === "") {
                $("#lblTitle").html("Không được để trống tên đại lý");
                flg = false;
            }
            else if (txtTitle.length < 1 || txtTitle.length > 80) {
                $("#lblTitle").html("Tên đại lý giới hạn [1-80] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtTitle)) {
                $("#lblTitle").html("Tên đại lý không hợp lệ");
                flg = false;
            }
            else {
                $("#lblTitle").html("");
            }
            //
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
            // 
            if (txtTaxCode === "") {
                $("#lblTaxCode").html("Không được để trống mã số thuế");
                flg = false;
            }
            else if (!FormatKeyword.test(txtTaxCode)) {
                $("#lblTaxCode").html("Mã số thuế không hợp lệ");
                flg = false;
            }
            else {
                $("#lblTaxCode").html("");
            }
            // company phonnumber  

            if (txtCompanyPhone === "") {
                $("#lblCompanyPhone").html("Không được để trống số đ.thoại công ty");
                flg = false;
            } else if (!FormatPhone.test(txtCompanyPhone)) {
                $("#lblCompanyPhone").html("Số đ.thoại công ty không hợp lệ");
                flg = false;
            }
            else {
                $("#lblCompanyPhone").html("");
            }
            // address
            if (txtAddress === "") {
                $("#lblAddress").html("Không được để trống địa chỉ");
                flg = false;
            }
            else if (txtAddress.length < 1 || txtAddress.length > 255) {
                $("#lblAddress").html("Địa chỉ giới hạn từ 1-> 255 ký tự");
                flg = false;
            }
            else {
                $("#lblAddress").html("");
            }
            // Contact Name
            if (txtContactName === "") {
                $("#lblContactName").html("Không được để trống họ tên");
                flg = false;
            }
            else if (txtContactName.length < 2 || txtContactName.length > 30) {
                $("#lblContactName").html("Họ tên giới hạn [2-30] ký tự");
                flg = false;
            }
            else if (!FormatFName.test(txtContactName)) {
                $("#lblContactName").html("Họ tên không hợp lệ");
                flg = false;
            }
            else {
                $("#lblContactName").html("");
            }
            // Contact email  
            if (txtContactEmail === "") {
                $("#lblContactEmail").html("Không được để trống địa chỉ email");
                flg = false;
            }
            else if (!FormatEmail.test(txtContactEmail)) {
                $("#lblContactEmail").html("Địa chỉ email không hợp lệ");
                flg = false;
            }
            else {
                $("#lblContactEmail").html("");
            }
            // contact phone  
            if (txtContactPhone === "") {
                $("#lblContactPhone").html("Không được để trống số đ.thoại liên hệ");
                flg = false;
            } else if (!FormatPhone.test(txtContactPhone)) {
                $("#lblContactPhone").html("Số đ.thoại liên hệ không hợp lệ");
                flg = false;
            }
            else {
                $("#lblContactPhone").html("");
            }
            // account **********************************************************************************

            if (txtAccount === "") {
                $("#lblAccount").html("Không được để trống tài khoản");
                flg = false;
            }
            else if (!FormatUser.test(txtAccount)) {
                $("#lblAccount").html("Tài khoản không hợp lệ");
                flg = false;
            }
            else if (txtAccount.length < 4 || txtAccount.length > 16) {
                $("#lblAccount").html("Tài khoản giới hạn [6-16] ký tự");
                flg = false;
            }
            else {
                $("#lblAccount").html("");
            }
            // valid password 
            if (txtPassword === "") {
                $("#lblPassword").html("Không được để trống mật khẩu");
                flg = false;
            } else if (txtPassword.length < 4 || txtPassword.length > 16) {
                $("#lblPassword").html("Mật khẩu giới hạn [4-16] ký tự");
                flg = false;
            }
            else if (!FormatPass.test(txtPassword)) {
                $("#lblPassword").html("Yêu cầu mật khẩu bảo mật hơn");
                flg = false;
            }
            else {
                $("#lblPassword").html("");
            }
            // email account
            if (txtEmail === "") {
                $("#lblEmail").html("Không được để trống địa chỉ email");
                flg = false;
            }
            else if (!FormatEmail.test(txtEmail)) {
                $("#lblEmail").html("Địa chỉ email không hợp lệ");
                flg = false;
            }
            else {
                $("#lblEmail").html("");
            }
            // account phone number
            if (txtPhone === "") {
                $("#lblPhone").html("Không được để trống số đ.thoại nhận mã OTP");
                flg = false;
            } else if (!FormatPhone.test(txtPhone)) {
                $("#lblPhone").html("Số đ.thoại nhận mã OTP không hợp lệ");
                flg = false;
            }
            else {
                $("#lblPhone").html("");
            }
            //
            if (txtTermPayment === "") {
                $("#lblTermPayment").html("Không được để trống kỳ hạn thanh toán");
                flg = false;
            }
            else if (!FormatNumber.test(txtTermPayment)) {
                $("#lblTermPayment").html("Kỳ hạn thanh toán không hợp lệ");
                flg = false;
            }
            else if (parseInt(txtTermPayment) <= 0) {
                $("#lblTermPayment").html("Kỳ hạn thanh toán phải > 0");
                flg = false;
            }
            else {
                $("#lblTermPayment").html("");
            }
            // submit **********************************************************************************
            if (flg) {
                CompanyController.Create();
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
        });
        $("#btnSearch").off("click").on("click", function () {
            CompanyController.DataList(1);
        });
        $("#btnUpdate").off("click").on("click", function () {
            var flg = true;
            var ddlAgent = $("#ddlAgent").val();
            var txtCodeID = $("#txtCodeID").val();
            var txtTitle = $("#txtTitle").val();
            var txtSummary = $("#txtSummary").val();
            var txtTaxCode = $("#txtTaxCode").val();
            var txtAddress = $("#txtAddress").val();
            var txtCompanyPhone = $("#txtCompanyPhone").val();
            //
            var txtContactName = $("#txtContactName").val();
            var txtContactEmail = $("#txtContactEmail").val();
            var txtContactPhone = $("#txtContactPhone").val();
            //
            var txtAccount = $("#txtAccount").val();
            var txtPassword = $("#txtPassword").val();
            var txtEmail = $("#txtEmail").val();
            var txtPhone = $("#txtPhone").val();
            var txtTermPayment = $("#txtTermPayment").val();
            //
            $("#lblAgent").html("");
            if (HelperModel.AccessInApplication() != RoleEnum.IsAdminCustomerLogged) {
                if (ddlAgent == "") {
                    $("#lblAgent").html("Vui lòng chọn đại lý");
                    flg = false;
                }
            }
            
            if (txtCodeID === "") {
                $("#lblCodeID").html("Không được để trống mã khách hàng");
                flg = false;
            }
            else if (txtCodeID.length != 3) {
                $("#lblCodeID").html("Mã khách hàng bao gồm 3 ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtCodeID)) {
                $("#lblCodeID").html("Mã khách hàng không hợp lệ");
                flg = false;
            }
            else {
                $("#lblCodeID").html("");
            }
            //
            if (txtTitle === "") {
                $("#lblTitle").html("Không được để trống tên đại lý");
                flg = false;
            }
            else if (txtTitle.length < 1 || txtTitle.length > 80) {
                $("#lblTitle").html("Tên đại lý giới hạn [1-80] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtTitle)) {
                $("#lblTitle").html("Tên đại lý không hợp lệ");
                flg = false;
            }
            else {
                $("#lblTitle").html("");
            }
            //
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
            // 
            if (txtTaxCode === "") {
                $("#lblTaxCode").html("Không được để trống mã số thuế");
                flg = false;
            }
            else if (!FormatKeyword.test(txtTaxCode)) {
                $("#lblTaxCode").html("Mã số thuế không hợp lệ");
                flg = false;
            }
            else {
                $("#lblTaxCode").html("");
            }
            // company phonnumber  

            if (txtCompanyPhone === "") {
                $("#lblCompanyPhone").html("Không được để trống số đ.thoại công ty");
                flg = false;
            } else if (!FormatPhone.test(txtCompanyPhone)) {
                $("#lblCompanyPhone").html("Số đ.thoại công ty không hợp lệ");
                flg = false;
            }
            else {
                $("#lblCompanyPhone").html("");
            }
            // address
            if (txtAddress === "") {
                $("#lblAddress").html("Không được để trống địa chỉ");
                flg = false;
            }
            else if (txtAddress.length < 1 || txtAddress.length > 255) {
                $("#lblAddress").html("Địa chỉ giới hạn từ 1-> 255 ký tự");
                flg = false;
            }
            else {
                $("#lblAddress").html("");
            }
            // Contact Name
            if (txtContactName === "") {
                $("#lblContactName").html("Không được để trống họ tên");
                flg = false;
            }
            else if (txtContactName.length < 2 || txtContactName.length > 30) {
                $("#lblContactName").html("Họ tên giới hạn [2-30] ký tự");
                flg = false;
            }
            else if (!FormatFName.test(txtContactName)) {
                $("#lblContactName").html("Họ tên không hợp lệ");
                flg = false;
            }
            else {
                $("#lblContactName").html("");
            }
            // Contact email  
            if (txtContactEmail === "") {
                $("#lblContactEmail").html("Không được để trống địa chỉ email");
                flg = false;
            }
            else if (!FormatEmail.test(txtContactEmail)) {
                $("#lblContactEmail").html("Địa chỉ email không hợp lệ");
                flg = false;
            }
            else {
                $("#lblContactEmail").html("");
            }
            // contact phone  
            if (txtContactPhone === "") {
                $("#lblContactPhone").html("Không được để trống số đ.thoại liên hệ");
                flg = false;
            } else if (!FormatPhone.test(txtContactPhone)) {
                $("#lblContactPhone").html("Số đ.thoại liên hệ không hợp lệ");
                flg = false;
            }
            else {
                $("#lblContactPhone").html("");
            }
            //
            if (txtTermPayment === "") {
                $("#lblTermPayment").html("Không được để trống kỳ hạn thanh toán");
                flg = false;
            }
            else if (!FormatNumber.test(txtTermPayment)) {
                $("#lblTermPayment").html("Kỳ hạn thanh toán không hợp lệ");
                flg = false;
            }
            else if (parseInt(txtTermPayment) <= 0) {
                $("#lblTermPayment").html("Kỳ hạn thanh toán phải > 0");
                flg = false;
            }
            else {
                $("#lblTermPayment").html("");
            }

            // submit **********************************************************************************
            if (flg) {
                CompanyController.Update();
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

                            //var _id = item.ID;
                            var _customerCode = item.CodeID;
                            var _agentCode = item.AgentCode;
                            //var _parentID = item.ParentID;
                            var _title = item.Title;
                            //var _alias = item.Alias;
                            //var _summary = item.Summary;
                            //var _address = item.Address;
                            //var _phone = item.Phone;
                            //var _taxCode = item.TaxCode;
                            var _contactName = item.ContactName;
                            //var _contactEmail = item.ContactEmail;
                            //var _contactPhone = item.ContactPhone;
                            var _depositAmount = item.DepositAmount;
                            //var _termPayment = item.TermPayment;
                            //var _path = item.Path;
                            //var _accountID = item.AccountID;
                            //var _phoneOfOTP = item.PhoneOfOTP;
                            //var _typeLevel = item.TypeLevel;

                            //var strlevel = " - cấp: " + _typeLevel;
                            //if (_typeLevel < 10) {
                            //    strlevel = " - cấp: " + "0" + _typeLevel;
                            //}
                            //  role
                            var action = HelperModel.RolePermission(result.role, "CompanyController", id);
                            //
                            var rowNum = parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                            rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>
                                 <td>${_customerCode}</td>
                                 <td class="bg-danger">${_agentCode}</td>
                                 <td>${_title}</td>                    
                                 <td>${_contactName}</td>                                                                         
                                 <td class="text-center">${HelperModel.StatusIcon(item.Enabled)}</td>
                                 <td class="text-center">${item.CreatedDate}</td>
                                 <td class="tbcol-action">${action}</td>
                            </tr>`;
                        });
                        $("tbody#TblData").html(rowData);
                        if (parseInt(totalPage) > 1) {
                            Paging.Pagination("#Pagination", totalPage, currentPage, CompanyController.DataList);
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
        var ddlAgent = $("#ddlAgent").val();
        if (ddlAgent == undefined) {
            ddlAgent = "";
        }
        var txtCodeID = $("#txtCodeID").val();
        var txtTitle = $("#txtTitle").val();
        var txtSummary = $("#txtSummary").val();
        var txtTaxCode = $("#txtTaxCode").val();
        var txtAddress = $("#txtAddress").val();
        var txtCompanyPhone = $("#txtCompanyPhone").val();
        //
        var txtContactName = $("#txtContactName").val();
        var txtContactEmail = $("#txtContactEmail").val();
        var txtContactPhone = $("#txtContactPhone").val();
        //
        var txtAccount = $("#txtAccount").val();
        var txtPassword = $("#txtPassword").val();
        var txtEmail = $("#txtEmail").val();
        var txtPhone = $("#txtPhone").val();
        //
        var txtTermPayment = $("#txtTermPayment").val();
        //
        var enabled = 0;
        if ($("input[name='cbxActive']").is(":checked"))
            enabled = 1;
        //
        var model = {
            AgentID: ddlAgent,
            CodeID: txtCodeID,
            Title: txtTitle,
            Summary: txtSummary,
            TaxCode: txtTaxCode,
            Address: txtAddress,
            CompanyPhone: txtCompanyPhone,
            //
            ContactName: txtContactName,
            ContactEmail: txtContactEmail,
            ContactPhone: txtContactPhone,
            //
            AccountID: txtAccount,
            Password: txtPassword,
            Phone: txtPhone,
            Email: txtEmail,
            //
            TermPayment: txtTermPayment,
            //
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
        var id = $("#txtID").val();
        var txtCodeID = $("#txtCodeID").val();
        var txtTitle = $("#txtTitle").val();
        var txtSummary = $("#txtSummary").val();
        var txtTaxCode = $("#txtTaxCode").val();
        var txtCompanyPhone = $("#txtCompanyPhone").val();
        var txtAddress = $("#txtAddress").val();
        //
        var txtContactName = $("#txtContactName").val();
        var txtContactEmail = $("#txtContactEmail").val();
        var txtContactPhone = $("#txtContactPhone").val();
        //
        var txtTermPayment = $("#txtTermPayment").val();
        //
        var enabled = 0;

        if ($("input[name='cbxActive']").is(":checked"))
            enabled = 1;
        //
        var model = {
            ID: id,
            CodeID: txtCodeID,
            Title: txtTitle,
            Summary: txtSummary,
            Address: txtAddress,
            TaxCode: txtTaxCode,
            CompanyPhone: txtCompanyPhone,
            //
            ContactName: txtContactName,
            ContactEmail: txtContactEmail,
            ContactPhone: txtContactPhone,
            //        
            TermPayment: txtTermPayment,
            //
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
                        CompanyController.DataList(pageIndex);
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
        Confirm.Delete(id, CompanyController.Delete, null, null);
    }
};

CompanyController.init();
// *********************************************************************************************
$(document).on("change", "#ddlAgent", function () {
    var ddlAgent = $(this).val();
    $("#lblAgent").html("");
    $("#lblProviderCodeID").html("- - -");
    if (ddlAgent == "") {
        $("#lblAgent").html("Vui lòng chọn đại lý");
        flg = false;
    }
    var codeid = $(this).find(":selected").data("codeid");
    $("#lblProviderCodeID").html(codeid);
});
//
$(document).on("keyup", "#txtCodeID", function () {
    var txtCodeID = $(this).val();
    if (txtCodeID === "") {
        $("#lblCodeID").html("Không được để trống mã khách hàng");
    }
    else if (txtCodeID.length != 3) {
        $("#lblCodeID").html("Mã khách hàng bao gồm 3 ký tự");
    }
    else if (!FormatKeyword.test(txtCodeID)) {
        $("#lblCodeID").html("Mã khách hàng không hợp lệ");
    }
    else {
        $("#lblCodeID").html("");
    }
});
$(document).on("keyup", "#txtTitle", function () {
    var txtTitle = $(this).val();
    if (txtTitle === "") {
        $("#lblTitle").html("Không được để trống tên đại lý");
    }
    else if (txtTitle.length < 1 || txtTitle.length > 80) {
        $("#lblTitle").html("Tên đại lý giới hạn [1-80] ký tự");
    }
    else if (!FormatKeyword.test(txtTitle)) {
        $("#lblTitle").html("Tên đại lý không hợp lệ");
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
$(document).on("keyup", "#txtTaxCode", function () {
    var txtTaxCode = $(this).val();
    if (txtTaxCode === "") {
        $("#lblTaxCode").html("Không được để trống mã số thuế");
    }
    else if (!FormatKeyword.test(txtTaxCode)) {
        $("#lblTaxCode").html("Mã số thuế không hợp lệ");
    }
    else {
        $("#lblTaxCode").html("");
    }
});
// company phone number   
$(document).on("keyup", "#txtCompanyPhone", function () {
    var txtCompanyPhone = $(this).val();
    if (txtCompanyPhone === "") {
        $("#lblCompanyPhone").html("Không được để trống số đ.thoại công ty");
    } else if (!FormatPhone.test(txtCompanyPhone)) {
        $("#lblCompanyPhone").html("Số đ.thoại công ty không hợp lệ");
    }
    else {
        $("#lblCompanyPhone").html("");
    }
});
// company  address
$(document).on("keyup", "#txtAddress", function () {
    var txtAddress = $(this).val();
    if (txtAddress === "") {
        $("#lblAddress").html("Không được để trống địa chỉ");
    }
    else if (txtAddress.length < 1 || txtAddress.length > 255) {
        $("#lblAddress").html("Địa chỉ giới hạn từ 1-> 255 ký tự");
    }
    else {
        $("#lblAddress").html("");
    }
});
// contact name
$(document).on("keyup", "#txtContactName", function () {
    var txtContactName = $(this).val();
    if (txtContactName === "") {
        $("#lblContactName").html("Không được để trống họ tên");
    }
    else if (txtContactName.length < 2 || txtContactName.length > 30) {
        $("#lblContactName").html("Họ tên giới hạn [2-30] ký tự");
    }
    else if (!FormatFName.test(txtContactName)) {
        $("#lblContactName").html("Họ tên không hợp lệ");
    }
    else {
        $("#lblContactName").html("");
    }
});
// contact email
$(document).on("keyup", "#txtContactEmail", function () {
    var txtContactEmail = $(this).val();
    if (txtContactEmail === "") {
        $("#lblContactEmail").html("Không được để trống địa chỉ email");
    }
    else if (!FormatEmail.test(txtContactEmail)) {
        $("#lblContactEmail").html("Địa chỉ email không hợp lệ");
    }
    else {
        $("#lblContactEmail").html("");
    }
});
// contact phone
$(document).on("keyup", "#txtContactPhone", function () {
    var txtPhone = $(this).val();
    if (txtPhone === "") {
        $("#lblContactPhone").html("Không được để trống số đ.thoại liên hệ");
    } else if (!FormatPhone.test(txtPhone)) {
        $("#lblContactPhone").html("Số đ.thoại liên hệ không hợp lệ");
    }
    else {
        $("#lblContactPhone").html("");
    }
});
// account
$(document).on("keyup", "#txtAccount", function () {
    var txtAccount = $(this).val();
    if (txtAccount === "") {
        $("#lblAccount").html("Không được để trống tài khoản");
    }
    else if (!FormatUser.test(txtAccount)) {
        $("#lblAccount").html("Tài khoản không hợp lệ");
    }
    else if (txtAccount.length < 4 || txtAccount.length > 16) {
        $("#lblAccount").html("Tài khoản giới hạn [6-16] ký tự");
    }
    else {
        $("#lblAccount").html("");
    }
});
// account password
$(document).on("keyup", "#txtPassword", function () {
    var txtPassword = $(this).val();
    if (txtPassword === "") {
        $("#lblPassword").html("Không được để trống mật khẩu");
    } else if (txtPassword.length < 4 || txtPassword.length > 16) {
        $("#lblPassword").html("Mật khẩu giới hạn [4-16] ký tự");
    }
    else if (!FormatPass.test(txtPassword)) {
        $("#lblPassword").html("Yêu cầu mật khẩu bảo mật hơn");
    }
    else {
        $("#lblPassword").html("");
    }
});
// account email
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
// account/otp phone number
$(document).on("keyup", "#txtPhone", function () {
    var txtPhone = $(this).val();
    if (txtPhone === "") {
        $("#lblPhone").html("Không được để trống số đ.thoại nhận mã OTP");
    } else if (!FormatPhone.test(txtPhone)) {
        $("#lblPhone").html("Số đ.thoại nhận mã OTP không hợp lệ");
    }
    else {
        $("#lblPhone").html("");
    }
});
// TermPayment
$(document).on("keyup", "#txtTermPayment", function () {
    var txtTermPayment = $(this).val();
    if (txtTermPayment === "") {
        $("#lblTermPayment").html("Không được để trống kỳ hạn thanh toán");
    }
    else if (!FormatNumber.test(txtTermPayment)) {
        $("#lblTermPayment").html("Kỳ hạn thanh toán không hợp lệ");
    }
    else if (parseInt(txtTermPayment) <= 0) {
        $("#lblTermPayment").html("Kỳ hạn thanh toán phải > 0");
    }
    else {
        $("#lblTermPayment").html("");
    }
});
