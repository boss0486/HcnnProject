var pageIndex = 1;
var URLC = "/Management/Product/Action";
var URLA = "/Management/Product";
var arrFile = [];
var ProductController = {
    init: function () {
        ProductController.registerEvent();
    },
    registerEvent: function () {
        $("#btnCreate").off("click").on("click", function () {
            var flg = true;
            var textId = $("#txtTextID").val();
            var title = $("#txtTitle").val();
            var summary = $("#txtSummary").val();
            var viewDate = $("#txtViewDate").val();
            var viewTotal = $("#txtViewTotal").val();
            var ddlCategory = $("#ddlCategoryID").val();
            var price = $("#txtPrice").val();
            var priceListed = $("#txtPriceListed").val();
            var ddlState = $("#ddlState").val();
            //
            $("#lblTextID").html("");
            if (textId != "") {
                if (textId.length < 5 || textId.length > 10) {
                    $("#lblTextID").html("Mã sản phẩm giới hạn 5-10 ký tự");
                    flg = false;
                }
                else if (!FormatKeyId.test(textId)) {
                    $("#lblTextID").html("Mã sản phẩm không hợp lệ");
                    flg = false;
                }
            }
            //
            $("#lblTitle").html("");
            if (title == "") {
                $("#lblTitle").html("Không được để trống tên sản phẩm");
                flg = false;
            }
            else if (title.length < 1 || title.length > 80) {
                $("#lblTitle").html("Tên sản phẩm giới hạn [1-80] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(title)) {
                $("#lblTitle").html("Tên sản phẩm không hợp lệ");
                flg = false;
            }
            // 
            $("#lblSummary").html("");
            if (summary != "") {
                if (summary.length < 1 || summary.length > 120) {
                    $("#lblSummary").html("Mô tả giới hạn [1-120] ký tự");
                    flg = false;
                }
                else if (!FormatKeyword.test(summary)) {
                    $("#lblSummary").html("Mô tả không hợp lệ");
                    flg = false;
                }
                else {
                    $("#lblSummary").html("");
                }
            }
            // create date display 
            $("#lblViewDate").html("");
            if (viewDate != "") {
                if (!FormatDateVN.test(viewDate)) {
                    $("#lblViewDate").html("Ngày hiển thị không hợp lệ");
                    flg = false;
                }
                else {
                    $("#lblViewDate").html("");
                }
            } else {
            }
            // ViewTotal 
            $("#lblViewTotal").html("");
            if (viewTotal != "") {
                if (!FormatNumber.test(viewTotal)) {
                    $("#lblViewTotal").html("Số lượt xem sản phẩm không hợp lệ");
                    flg = false;
                }
            }
            // menu
            var menuId = $(".vetical-menu input[type='checkbox']:checked").data('id');
            if (menuId == undefined || menuId == "") {
                $('#lblMenuID').html('Vui lòng chọn danh mục');
                flg = false;
            }
            else {
                $('#lblMenuID').html('');
            }
            // category 
            $("#lblCategory").html("");
            if (ddlCategory == "") {
                $("#lblCategory").html("Vui lòng nhóm sản phẩm");
                flg = false;
            }
            // price 
            $("#lblPrice").html("");
            if (price == "") {
                $("#lblPrice").html("Không được để trống giá sản phẩm");
                flg = false;
            }
            else if (!FormatCurrency.test(price)) {
                $("#lblPrice").html("Giá sản phẩm không hợp lệ");
                flg = false;
            }
            //  
            $("#lblPriceListed").html("");
            if (priceListed == "") {
                $("#lblPriceListed").html("Không được để trống giá khuyến mại");
                flg = false;
            }
            else if (!FormatCurrency.test(priceListed) < 0) {
                $("#lblPriceListed").html("Giá khuyến mại không hợp lệ");
                flg = false;
            }
            //// lblWarranty
            //var ddlWarranty = $("#ddlWarranty").val();
            //if (ddlWarranty == "") {
            //    $("#lblWarranty").html("Vui lòng chọn thời gian bảo hành");
            //    flg = false;
            //}
            //else {
            //    $("#lblWarranty").html("");
            //}
            //// ProductWarranty
            //var ddlProvider = $("#ddlProvider").val();
            //if (ddlProvider == "") {
            //    $("#lblProvider").html("Vui lòng chọn nhà cung cấp");
            //}
            //else {
            //    $("#lblWarranty").html("");
            //}
            // ProductState
            $("#lblState").html("");
            if (parseInt(ddlState) == -1) {
                $("#lblState").html("Vui lòng chọn tình trạng");
                flg = false;
            }
            // submit form
            if (flg)
                ProductController.Create();
            else
                Notifization.Error(MessageText.Datamissing);
        });
        $("#btnSearch").off("click").on("click", function () {
            ProductController.DataList(1);
        });
        $("#btnUpdate").off("click").on("click", function () {
            var flg = true;
            var textId = $("#txtTextID").val();
            var title = $("#txtTitle").val();
            var summary = $("#txtSummary").val();
            var viewDate = $("#txtViewDate").val();
            var viewTotal = $("#txtViewTotal").val();
            var ddlCategory = $("#ddlCategoryID").val();
            var price = $("#txtPrice").val();
            var priceListed = $("#txtPriceListed").val();
            var ddlState = $("#ddlState").val();
            //
            $("#lblTextID").html("");
            if (textId != "") {
                if (textId.length < 5 || textId.length > 10) {
                    $("#lblTextID").html("Mã sản phẩm giới hạn 5-10 ký tự");
                    flg = false;
                }
                else if (!FormatKeyId.test(textId)) {
                    $("#lblTextID").html("Mã sản phẩm không hợp lệ");
                    flg = false;
                }
            }
            //
            $("#lblTitle").html("");
            if (title == "") {
                $("#lblTitle").html("Không được để trống tên sản phẩm");
                flg = false;
            }
            else if (title.length < 1 || title.length > 80) {
                $("#lblTitle").html("Tên sản phẩm giới hạn [1-80] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(title)) {
                $("#lblTitle").html("Tên sản phẩm không hợp lệ");
                flg = false;
            }
            // 
            $("#lblSummary").html("");
            if (summary != "") {
                if (summary.length < 1 || summary.length > 120) {
                    $("#lblSummary").html("Mô tả giới hạn [1-120] ký tự");
                    flg = false;
                }
                else if (!FormatKeyword.test(summary)) {
                    $("#lblSummary").html("Mô tả không hợp lệ");
                    flg = false;
                }
                else {
                    $("#lblSummary").html("");
                }
            }
            // create date display 
            $("#lblViewDate").html("");
            if (viewDate != "") {
                if (!FormatDateVN.test(viewDate)) {
                    $("#lblViewDate").html("Ngày hiển thị không hợp lệ");
                    flg = false;
                }
                else {
                    $("#lblViewDate").html("");
                }
            } else {
            }
            // ViewTotal 
            $("#lblViewTotal").html("");
            if (viewTotal != "") {
                if (!FormatNumber.test(viewTotal)) {
                    $("#lblViewTotal").html("Số lượt xem sản phẩm không hợp lệ");
                    flg = false;
                }
            }
            // menu
            var menuId = $(".vetical-menu input[type='checkbox']:checked").data('id');
            if (menuId == undefined || menuId == "") {
                $('#lblMenuID').html('Vui lòng chọn danh mục');
                flg = false;
            }
            else {
                $('#lblMenuID').html('');
            }
            // category 
            $("#lblCategory").html("");
            if (ddlCategory == "") {
                $("#lblCategory").html("Vui lòng nhóm sản phẩm");
                flg = false;
            }
            // price 
            $("#lblPrice").html("");
            if (price == "") {
                $("#lblPrice").html("Không được để trống giá sản phẩm");
                flg = false;
            }
            else if (!FormatCurrency.test(price)) {
                $("#lblPrice").html("Giá sản phẩm không hợp lệ");
                flg = false;
            }
            //  
            $("#lblPriceListed").html("");
            if (priceListed == "") {
                $("#lblPriceListed").html("Không được để trống giá khuyến mại");
                flg = false;
            }
            else if (!FormatCurrency.test(priceListed) < 0) {
                $("#lblPriceListed").html("Giá khuyến mại không hợp lệ");
                flg = false;
            }
            //// lblWarranty
            //var ddlWarranty = $("#ddlWarranty").val();
            //if (ddlWarranty == "") {
            //    $("#lblWarranty").html("Vui lòng chọn thời gian bảo hành");
            //    flg = false;
            //}
            //else {
            //    $("#lblWarranty").html("");
            //}
            //// ProductWarranty
            //var ddlProvider = $("#ddlProvider").val();
            //if (ddlProvider == "") {
            //    $("#lblProvider").html("Vui lòng chọn nhà cung cấp");
            //}
            //else {
            //    $("#lblWarranty").html("");
            //}
            // ProductState
            $("#lblState").html("");
            if (parseInt(ddlState) == -1) {
                $("#lblState").html("Vui lòng chọn tình trạng");
                flg = false;
            }
            // submit form
            if (flg)
                ProductController.Update();
            else
                Notifization.Error(MessageText.Datamissing);
        });
    },
    Create: function () {
        var textId = $("#txtTextID").val();
        var title = $("#txtTitle").val();
        var alias = $("#txtAlias").val();
        var summary = $("#txtSummary").val();
        var tag = $("#txtTag").val();
        var viewTotal = $("#txtViewTotal").val();
        var viewDate = $("#txtViewDate").val();
        var ddlCategory = $("#ddlCategoryID").val();
        var price = $("#txtPrice").val();
        var priceListed = $("#txtPriceListed").val();
        var ddlWarranty = $("#ddlWarranty").val();
        var ddlProvider = $("#ddlProvider").val();
        var ddlState = $("#ddlState").val();
        var menuId = $(".vetical-menu input[type='checkbox']:checked").data('id');
        var htmlText = tinyMCE.editors[$("#txtHtmlText").attr("id")].getContent();
        var htmlNote = tinyMCE.editors[$("#txtNote").attr("id")].getContent();

        var enabled = 0;
        if ($("input[name='cbxActive']").is(":checked"))
            enabled = 1;
        //
        var _imgFile = "";
        var _imgFileView = $(".new-box-preview img");
        if (_imgFileView != "" && _imgFileView != undefined)
            _imgFile = $(_imgFileView).data("id");

        // photo
        var arrPhoto = [];
        var _imgList = $(".pre-view .pre-item-box");
        if (_imgList.length > 0) {
            $.each(_imgList, function (index, preItem) {
                var idFile = $(this).data("id");
                if (idFile != undefined && idFile.length > 0)
                    arrPhoto.push(idFile);
            });
        }
        var model = {
            Title: title,
            Alias: alias,
            TextID: textId,
            Summary: summary,
            HtmlNote: htmlNote,
            HtmlText: htmlText,
            Tag: tag,
            ImageFile: _imgFile,
            Price: LibCurrencies.ConvertToCurrency(price),
            PriceListed: LibCurrencies.ConvertToCurrency(priceListed),
            PriceText: "",
            MenuID: menuId,
            CategoryID: ddlCategory,
            MadeInID: ddlProvider,
            WarrantyID: ddlWarranty,
            ViewTotal: viewTotal,
            ViewDate: viewDate,
            Photos: arrPhoto,
            State: ddlState,
            Enabled: enabled
        };
        AjaxFrom.POST({
            url: URLC + "/create",
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
                console.log("::" + MessageText.NotService);
            }
        });

    },
    Update: function () {
        var id = $("#txtID").val();
        var textId = $("#txtTextID").val();
        var title = $("#txtTitle").val();
        var alias = $("#txtAlias").val();
        var summary = $("#txtSummary").val();
        var tag = $("#txtTag").val();
        var viewTotal = $("#txtViewTotal").val();
        var viewDate = $("#txtViewDate").val();
        var ddlCategory = $("#ddlCategoryID").val();
        var price = $("#txtPrice").val();
        var priceListed = $("#txtPriceListed").val();
        var ddlWarranty = $("#ddlWarranty").val();
        var ddlProvider = $("#ddlProvider").val();
        var ddlState = $("#ddlState").val();
        var menuId = $(".vetical-menu input[type='checkbox']:checked").data('id');
        var htmlText = tinyMCE.editors[$("#txtHtmlText").attr("id")].getContent();
        var htmlNote = tinyMCE.editors[$("#txtNote").attr("id")].getContent();
        var enabled = 0;
        if ($("input[name='cbxActive']").is(":checked"))
            enabled = 1;
        //
        var _imgFile = "";
        var _imgFileView = $(".new-box-preview img");
        if ($(_imgFileView).data("id") != undefined)
            _imgFile = $(_imgFileView).data("id");

        // photo
        var arrPhoto = [];
        var _imgList = $(".pre-view .pre-item-box");
        if (_imgList.length > 0) {
            $.each(_imgList, function (index, preItem) {
                var idFile = $(this).data("id");
                if (idFile != undefined && idFile.length > 0)  
                    arrPhoto.push(idFile);
            });
        }
        var model = {
            ID: id,
            TextID: textId,
            Title: title,
            Alias: alias,
            Summary: summary,
            HtmlNote: htmlNote,
            HtmlText: htmlText,
            Tag: tag,
            ImageFile: _imgFile,
            Price: LibCurrencies.ConvertToCurrency(price),
            PriceListed: LibCurrencies.ConvertToCurrency(priceListed),
            MenuID: menuId,
            CategoryID: ddlCategory,
            MadeInID: ddlProvider,
            WarrantyID: ddlWarranty,
            ViewTotal: viewTotal,
            ViewDate: viewDate,
            Photos: arrPhoto,
            State: ddlState,
            Enabled: enabled
        };
        AjaxFrom.POST({
            url: URLC + "/Update",
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
                console.log("::" + MessageText.NotService);
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
        AjaxFrom.POST({
            url: URLC + "/DataList",
            data: model,
            success: function (result) {
                $("tbody#TblData").html("");
                $("#Pagination").html("");
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
                        var rowData = "";
                        $.each(result.data, function (index, item) {
                            index = index + 1;
                            var id = item.ID;
                            if (id.length > 0)
                                id = id.trim();
                            //  role
                            var action = HelperModel.RolePermission(result.role, "ProductController", id);
                            var rowNum = parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                            var _unit = "đ";
                            var _title = SubStringText.SubTitle(item.Title);
                            rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>  
                                 <td class="bg-photo"><img src="${item.ImagePath}" class="img-responsive" /></td>  
                                 <td class="text-left"><span class='code'>${item.TextID}</span>: <span class="title">${_title}</div></td>      
                                 <td class="text-left">${item.CategoryText}</td>                                                                                      
                                 <td class="text-left"><span class="date"><i class="far fa-clock"></i> ${item.ViewDate} | <i class="fas fa-check-double"></i> ${item.ViewTotal}</span></td>                                                                                      
                                 <td class="text-right"><span class="price">${LibCurrencies.FormatToCurrency(item.Price)}</span><span> ${_unit}</span></td>                                                                                            
                                 <td class="text-right"><span class="price-listed">${LibCurrencies.FormatToCurrency(item.PriceListed)}</span> <span> ${_unit}</span></td>                                                                                            
                                 <td class="text-center">${ProductState(item.State)}</td>                                                                                            
                                 <td class="text-center">${HelperModel.StatusIcon(item.Enabled)}</td>
                                 <td class="tbcol-action">${action}</td>
                            </tr > `;
                        });
                        $("tbody#TblData").html(rowData);
                        if (parseInt(totalPage) > 1) {
                            Paging.Pagination("#Pagination", totalPage, currentPage, ProductController.DataList);
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
            Id: id
        };
        AjaxFrom.POST({
            url: URLC + "/Delete",
            data: model,
            success: function (response) {
                if (response != null) {
                    if (response.status == 200) {
                        Notifization.Success(response.message);
                        ProductController.DataList(pageIndex);
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
        Confirm.Delete(id, ProductController.Delete, null, null);
    },
};
ProductController.init();
$(document).on("keyup", "#txtTitle", function () {
    var title = $(this).val();
    if (title == "") {
        $("#lblTitle").html("Không được để trống tên sản phẩm");
    }
    else if (title.length < 1 || title.length > 80) {
        $("#lblTitle").html("Tên sản phẩm giới hạn [1-80] ký tự");
    }
    else if (!FormatKeyword.test(title)) {
        $("#lblTitle").html("Tên sản phẩm không hợp lệ");
    }
    else {
        $("#lblTitle").html("");
    }
});

$(document).on("keyup", "#txtTextID", function () {
    var textId = $(this).val();
    $("#lblTextID").html("");
    if (textId.length < 5 || textId.length > 10) {
        $("#lblTextID").html("Mã sản phẩm giới hạn 5-10 ký tự");
        flg = false;
    }
    else if (!FormatKeyId.test(textId)) {
        $("#lblTextID").html("Mã sản phẩm không hợp lệ");
        flg = false;
    }
});
$(document).on("keyup", "#txtSummary", function () {
    var summary = $(this).val();
    if (summary != "") {
        if (summary.length < 1 || summary.length > 120) {
            $("#lblSummary").html("Tên sản phẩm giới hạn [1-80] ký tự");
        }
        else if (!FormatKeyword.test(summary)) {
            $("#lblSummary").html("Mô tả không hợp lệ");
        }
        else {
            $("#lblSummary").html("");
        }
    }
    else {
        $("#lblSummary").html("");
    }
});
$(document).on("keyup", "#txtAlias", function () {
    var alias = $("#txtAlias").val();
    if (alias != "") {
        if (alias.length > 80) {
            $("#lblAlias").html("Đường dẫn giới hạn từ 0-> 80 ký tự");
        }
        else if (!FormatUnicode.test(alias)) {
            $("#lblAlias").html("Đường dẫn không hợp lệ");
        }
        else {
            $("#lblAlias").html("");
        }
    } else {
        $("#lblAlias").html("");
    }
});
// ViewTotal
$(document).on("keyup", "#txtViewTotal", function () {
    var viewTotal = $(this).val();
    if (viewTotal != "") {
        if (!FormatNumber.test(viewTotal)) {
            $("#lblViewTotal").html("Số lượt xem sản phẩm không hợp lệ");
        }
        else {
            $("#lblViewTotal").html("");
        }
    }
});
// view date
$(document).on("keyup", "#txtViewDate", function () {
    var viewDate = $(this).val();
    if (viewDate != "") {
        if (!FormatDateVN.test(viewDate)) {
            $("#lblViewDate").html("Ngày hiển thị không hợp lệ");
        }
        else {
            $("#lblViewDate").html("");
        }
    } else {
        $("#lblViewDate").html("");
    }
});
// price
$(document).on("keyup", "#txtPrice", function () {
    var price = $(this).val();

    if (price == "") {
        $("#lblPrice").html("Không được để trống giá sản phẩm");
    }
    else if (!FormatCurrency.test(price)) {
        $("#lblPrice").html("Giá sản phẩm không hợp lệ");
    }
    else {
        $("#lblPrice").html("");
    }
});
//PriceListed
$(document).on("keyup", "#txtPriceListed", function () {
    var priceListed = $(this).val();
    if (priceListed == "") {
        $("#lblPriceListed").html("Không được để trống giá khuyến mại");
    }
    else if (!FormatCurrency.test(priceListed)) {
        $("#lblPriceListed").html("Giá khuyến mại không hợp lệ");
    }
    else {
        $("#lblPriceListed").html("");
    }
});
$(document).on("change", "#ddlCategoryID", function () {
    var txtCtl = $(this).val();
    if (txtCtl == "") {
        $("#lblCategory").html("Vui lòng nhóm sản phẩm");
    }
    else {
        $("#lblCategory").html("");
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
//$(document).on("change", "#ddlWarranty", function () {
//    var txtCtl = $(this).val();
//    if (txtCtl == "") {
//        $("#lblWarranty").html("Vui lòng chọn thời gian bảo hành");
//    }
//    else {
//        $("#lblWarranty").html("");
//    }
//});
//$(document).on("change", "#ddlProvider", function () {
//    var txtCtl = $(this).val();
//    if (txtCtl == "") {
//        $("#lblProvider").html("Vui lòng chọn nhà cung cấp");
//    }
//    else {
//        $("#lblProvider").html("");
//    }
//});
//$(document).on("change", "#ddlState", function () {
//    var txtCtl = $(this).val();
//    if (txtCtl == "") {
//        $("#lblState").html("Vui lòng chọn tình trạng");
//    }
//    else {
//        $("#lblState").html("");
//    }
//});

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
//$(document).on("click", "#btnFile", function () {
//    $("#ImageFile").click();
//});
//$(document).on("change", "#ImageFile", function (elm) {
//    $("#inputFileControl").html("");
//    $("#lblFile").html("");
//    var _file = $(this)[0].files[0];
//    if (_file != "") {
//        if (!IFile.IsImageFile(_file.name)) {
//            $("#lblFile").html("Tệp tin ảnh không hợp lệ");
//            $(this).val("");
//            $("#inputFileControl").html("");
//        }
//        else {
//            $("#inputFileControl").html(SubStringText.SubFileName(_file.name));
//            ImgPreview(this, ".new-box-preview");
//        }
//    }
//});
//function ImgPreview(inputFile, imgView) {
//    if (inputFile.files && inputFile.files[0]) {
//        var reader = new FileReader();
//        reader.onload = function (e) {
//            //$(imgView).attr("src", e.target.result);
//            $(imgView).css("background-image", "url(" + e.target.result + ")");
//        };
//        reader.readAsDataURL(inputFile.files[0]);
//    }
//}
$(document).on("click", ".img-caption-text", function () {
    $(".new-box-preview img").click();
});

function ProductState(_status) {
    var result = '';
    switch (_status) {
        case 1:
            result = `<i class="fas fa-check-circle"></i>`;
            break;
        case 2:
            result = `<i class="fas fa-times-circle"></i>`;
            break;
        default:
            result = "";
            break;
    }
    return result;
}