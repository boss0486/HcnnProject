var pageIndex = 1;
var URLC = "/Management/Work/Action";
var URLA = "/Management/Work";
var workController = {
    init: function () {
        workController.registerEvent();
    },
    registerEvent: function () {
        $("#btnCreate").off("click").on("click", function () {
            var flg = true;
            var txtTitle = $("#txtTitle").val();
            var txtContent = tinyMCE.editors[$("#txtContent").attr("id")].getContent();
            var txtExecuteDate = $("#txtExecuteDate").val();
            var txtDeadline = $("#txtDeadline").val();
            //  
            $("#lblTitle").html("");
            if (txtTitle === "") {
                $("#lblTitle").html("Nhập tên công việc");
                flg = false;
            }
            else if (txtTitle.length < 1 || txtTitle.length > 80) {
                $("#lblTitle").html("Tên công việc giới hạn [1-80] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtTitle)) {
                $("#lblTitle").html("Tên công việc không hợp lệ");
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
            $("#lblExecuteDate").html("");
            if (txtExecuteDate == "") {
                $("#lblExecuteDate").html("Nhập ngày thực hiện");
                flg = false;
            }
            else if (!ValidData.ValidDate(txtExecuteDate, "vn")) {
                $("#lblExecuteDate").html("Nhập ngày thực hiện");
                flg = false;
            }
            //
            $("#lblDeadline").html("");
            if (txtDeadline == "") {
                $("#lblDeadline").html("Nhập ngày kết thúc");
                flg = false;
            }
            else if (!ValidData.ValidDate(txtDeadline, "vn")) {
                $("#lblDeadline").html("Ngày kết thúc không hợp lệ");
                flg = false;
            }
            // submit
            if (flg) {
                workController.Create();
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
        });
        $("#btnSearch").off("click").on("click", function () {
            workController.DataList(1);
        });
        $("#btnUpdate").off("click").on("click", function () {
            var flg = true;
            var txtTitle = $("#txtTitle").val();
            var txtContent = tinyMCE.editors[$("#txtContent").attr("id")].getContent();
            var txtExecuteDate = $("#txtExecuteDate").val();
            var txtDeadline = $("#txtDeadline").val();
            //
            $("#lblTitle").html("");
            if (txtTitle === "") {
                $("#lblTitle").html("Nhập tên công việc");
                flg = false;
            }
            else if (txtTitle.length < 1 || txtTitle.length > 80) {
                $("#lblTitle").html("Tên công việc giới hạn [1-80] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtTitle)) {
                $("#lblTitle").html("Tên công việc không hợp lệ");
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
            $("#lblExecuteDate").html("");
            if (txtExecuteDate == "") {
                $("#lblExecuteDate").html("Nhập ngày thực hiện");
                flg = false;
            }
            else if (!ValidData.ValidDate(txtExecuteDate, "vn")) {
                $("#lblExecuteDate").html("Nhập ngày thực hiện");
                flg = false;
            }
            //
            $("#lblDeadline").html("");
            if (txtDeadline == "") {
                $("#lblDeadline").html("Nhập ngày kết thúc");
                flg = false;
            }
            else if (!ValidData.ValidDate(txtDeadline, "vn")) {
                $("#lblDeadline").html("Ngày kết thúc không hợp lệ");
                flg = false;
            }
            // submit
            if (flg) {
                workController.Update();
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
        });
        // assign
        $("#btnWorkAddModal").off("click").on("click", function () {
            var receptionType = $(this).data("receptiontype");
            $("#assignModalLabel").html("Thêm công việc và giao việc");
            if (parseInt(receptionType) == 2 || parseInt(receptionType) == 3) {
                // process
                $("#p_btnProcessSave").data("type", 1);
                
                $("#processModal .file-preview .pre-view").html("");
                $("#processModal .message").html("");
                var dpm = $("#ddlAssignTo").val();
                var workId = $("#p_txtId").val();
                DdlWorkUserExecutes("#processModal select#p_userExecute", dpm, workId);
                DdlWorkUserFollows("#processModal select#p_userFollow", dpm, workId);
                FData.ResetForm("#processModal");
                $("#processModal").modal("show");
                return;
            }
            if (parseInt(receptionType) == 0 || parseInt(receptionType) == 1) {
                // assign
                $("#a_btnAssignSave").data("type", 1);
                $("#assignModal .file-preview .pre-view").html("");
                DdlReception("#assignModal #a_ddlAssignTo", "");
                DdlState("#assignModal #a_ddlState", 1);
                $("#assignModal").modal("show");
                return;
            }
        });
        $(document).on("click", "[data-asgedit='true']", function () {
            var receptionType = $(this).data("receptiontype");
            if (parseInt(receptionType) == 2 || parseInt(receptionType) == 3) {
                // process
                $("#processModal .file-preview .pre-view").html("");
                $("#processModalLabel").html("Cập nhật công việc");
                $("#p_btnProcessSave").data("type", 2);
                var id = $(this).data("id");
                AjaxFrom.POST({
                    url: URLC + "/GetWorkByID",
                    data: { ID: id },
                    success: function (response) {
                        if (response !== null && response.status === 200) {
                            // $("#txtWork").val(); 
                            //DdlReception("#assignModal #a_ddlAssignTo", assignto);
                            var data = response.data;
                            if (data == null)
                                return;
                            //
                            var id = data.ID;
                            var title = data.Title;
                            var executeDate = data.ExecuteDate;
                            var deadline = data.Deadline;
                            var htmlText = data.HtmlText;
                            var htmlNote = data.HtmlNote;
                            $("#processModal #p_txtId").val(id);
                            $("#processModal #p_txtTitle").val(title);
                            tinymce.get("p_txtContent").setContent(htmlText);
                            $("#processModal #p_txtExecuteDate").val(executeDate);
                            $("#processModal #p_txtDeadline").val(deadline);
                            $("#processModal #p_txtNote").val(htmlNote);
                            DdlWorkUserExecutes("#processModal select#p_userExecute", data.AssignTo, id);
                            DdlWorkUserFollows("#processModal select#p_userFollow", data.AssignTo, id);
                            if (data.Attachments != undefined) {
                                var _htmlItem = "";
                                $.each(data.Attachments, function (idxFile, file) {
                                    _htmlItem += `<div class='i-list pre-item-box' data-id='${file.ID}'><img class="img-responsive" src="${file.ImagePath}" data-id='${file.ID}' /> <lalel> ${SubStringText.SubText(80, file.Title)} <i class="fas fa-times icon-delete" data-ibtn="true" data-id='#ibox${file.ID}'></i></lalel></div>`
                                });
                                $("#processModal .file-preview .pre-view").html(_htmlItem);
                            }
                            return;
                        }
                        //
                        Notifization.Error(MessageText.NotService);
                        return;
                    },
                    error: function (response) {
                        console.log("::" + MessageText.NotService);
                    }
                });
                $("#processModal").modal("show");
                return;
            }
            if (parseInt(receptionType) == 1) {
                // assign
                $("#assignModal .file-preview .pre-view").html("");
                $("#assignModalLabel").html("Cập nhật công việc");
                $("#a_btnAssignSave").data("type", 2);
                //
                var id = $(this).data("id");
                var assignto = $(this).data("assignto");
                AjaxFrom.POST({
                    url: URLC + "/GetWorkByID",
                    data: { ID: id },
                    success: function (response) {
                        if (response !== null && response.status === 200) {
                            // $("#txtWork").val(); 
                            DdlReception("#assignModal #a_ddlAssignTo", assignto);

                            var data = response.data;
                            var title = data.Title;
                            var executeDate = data.ExecuteDate;
                            var deadline = data.Deadline;
                            var htmlText = data.HtmlText;
                            DdlState("#assignModal #a_ddlState", data.State);
                            $("#assignModal #a_txtID").val(id);
                            $("#assignModal #a_txtTitle").val(title);
                            tinymce.get("p_txtContent").setContent(htmlText);
                            $("#assignModal #a_txtExecuteDate").val(executeDate);
                            $("#assignModal #a_txtDeadline").val(deadline);
                            if (data.Attachments != undefined) {
                                var _htmlItem = "";
                                $.each(data.Attachments, function (idxFile, file) {
                                    _htmlItem += `<div class='i-list pre-item-box' data-id='${file.ID}'><img class="img-responsive" src="${file.ImagePath}" data-id='${file.ID}' /> <lalel> ${SubStringText.SubText(80, file.Title)} <i class="fas fa-times icon-delete" data-ibtn="true" data-id='#ibox${file.ID}'></i></lalel></div>`


                                });
                                $("#assignModal .file-preview .pre-view").html(_htmlItem);
                            }
                            return;
                        }

                        Notifization.Error(MessageText.NotService);
                        return;
                    },
                    error: function (response) {
                        console.log("::" + MessageText.NotService);
                    }
                });
                $("#assignModal").modal("show");
                return;
            }
        });
        $(document).on("click", "[data-asgdelete='true']", function () {
            var id = $(this).data("id");
            Confirm.Delete(id, workController.AssignDelete, null, null);
        });
        $("#btnAssign").off("click").on("click", function () {
            var flg = true;
            var ddlAssignTo = $("#ddlAssignTo").val();
            var receptionType = $("#ddlAssignTo").find(':selected').data('type');
            if (ddlAssignTo == "" || parseInt(receptionType) == 1) {
                workController.Assign();
                return;
            }
            if (ddlAssignTo != "" || parseInt(receptionType) == 2) {
                var id = $(this).data("id");
                AjaxFrom.POST({
                    url: URLC + "/GetWorkByID",
                    data: { ID: id },
                    success: function (response) {
                        if (response !== null && response.status === 200) {
                            var data = response.data;
                            if (data == null)
                                return;
                            // 
                            var id = data.ID;
                            var executeDate = data.ExecuteDate;
                            var deadline = data.Deadline;
                            var htmlNote = data.HtmlNote;
                            //
                            $("#processFastModal #pf_txtId").val(id);
                            $("#processFastModal #pf_txtExecuteDate").val(executeDate);
                            $("#processFastModal #pf_txtDeadline").val(deadline);
                            $("#processFastModal #pf_txtNote").val(htmlNote);
                            DdlWorkUserExecutes("#processFastModal select#pf_userExecute", data.AssignTo, id);
                            DdlWorkUserFollows("#processFastModal select#pf_userFollow", data.AssignTo, id);
                            //
                            $("#processFastModal").modal("show");
                            return;
                        }
                        //
                        Notifization.Error(MessageText.NotService);
                        return;
                    },
                    error: function (response) {
                        console.log("::" + MessageText.NotService);
                    }
                });
                return;
            }

        });
        $("#a_btnAssignSave").off("click").on("click", function () {
            var flg = true;
            var ddlAssignTo = $("#a_ddlAssignTo").val();
            var txtTitle = $("#a_txtTitle").val();
            var txtContent = $("#a_txtContent").val();
            var txtExecuteDate = $("#a_txtExecuteDate").val();
            var txtDeadline = $("#a_txtDeadline").val();
            var ddlState = $("#a_ddlState").val();
            //
            $("#a_lblAssignTo").html("");
            if (ddlAssignTo == "") {
                $("#a_lblAssignTo").html("Chọn bộ phận tiếp nhận");
                flg = false;
            }
            //
            $("#a_lblTitle").html("");
            if (txtTitle === "") {
                $("#a_lblTitle").html("Nhập tên công việc");
                flg = false;
            }
            else if (txtTitle.length < 1 || txtTitle.length > 80) {
                $("#a_lblTitle").html("Tên công việc giới hạn [1-80] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtTitle)) {
                $("#a_lblTitle").html("Tên công việc không hợp lệ");
                flg = false;
            }
            //
            $("#a_lblContent").html("");
            if (txtContent == "") {
                $("#a_lblContent").html("Nhập nội dung");
                flg = false;
            }
            else if (txtContent.length > 5000) {
                $("#a_lblContent").html("Nội dung giới hạn [1-5000] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtContent)) {
                $("#a_lblContent").html("Nội dung không hợp lệ");
                flg = false;
            }
            // 
            $("#a_lblExecuteDate").html("");
            if (txtExecuteDate == "") {
                $("#a_lblExecuteDate").html("Nhập ngày thực hiện");
                flg = false;
            }
            else if (!ValidData.ValidDate(txtExecuteDate, "vn")) {
                $("#a_lblExecuteDate").html("Nhập ngày thực hiện");
                flg = false;
            }
            //
            $("#a_lblDeadline").html("");
            if (txtDeadline == "") {
                $("#a_lblDeadline").html("Nhập ngày kết thúc");
                flg = false;
            }
            else if (!ValidData.ValidDate(txtDeadline, "vn")) {
                $("#a_lblDeadline").html("Ngày kết thúc không hợp lệ");
                flg = false;
            }
            //
            $("#a_lblState").html("");
            if (ddlState == "") {
                $("#a_lblState").html("Chọn trạng thái công việc");
                flg = false;
            }
            // submit
            if (flg) {
                if ($(this).data("type") == 1) {
                    workController.WorkAdd();
                }
                else if ($(this).data("type") == 2) {
                    workController.WorkUpdate();
                }
                else
                    Notifization.Error(MessageText.Datamissing);
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
        });
        // process
        $("#p_btnProcessSave").off("click").on("click", function () {
            var flg = true;
            var txtTitle = $("#p_txtTitle").val();
            var txtContent = tinyMCE.editors[$("#p_txtContent").attr("id")].getContent();
            var txtExecuteDate = $("#p_txtExecuteDate").val();
            var txtDeadline = $("#p_txtDeadline").val();
            var txtNote = $("#p_txtNote").val();
            // 
            var userExecute = $("#p_userExecute").val();
            var userFollow = $("#p_userFollow").val();
            //$("#p_userExecute").find("option:selected").each(function () {
            //    userExecute.push($(this).val());
            //});
            //$("#p_userFollow").find("option:selected").each(function () {
            //    userFollow.push($(this).val());
            //});
            //
            $("#p_lblTitle").html("");
            if (txtTitle === "") {
                $("#p_lblTitle").html("Nhập tên công việc");
                flg = false;
            }
            else if (txtTitle.length < 1 || txtTitle.length > 80) {
                $("#p_lblTitle").html("Tên công việc giới hạn [1-80] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtTitle)) {
                $("#p_lblTitle").html("Tên công việc không hợp lệ");
                flg = false;
            }
            //
            $("#p_lblContent").html("");
            if (txtContent == "") {
                $("#p_lblContent").html("Nhập nội dung");
                flg = false;
            }
            else if (txtContent.length > 5000) {
                $("#p_lblContent").html("Nội dung giới hạn [1-5000] ký tự");
                flg = false;
            }
            else if (!FormatKeyword.test(txtContent)) {
                $("#p_lblContent").html("Nội dung không hợp lệ");
                flg = false;
            }
            // 
            $("#p_lblExecuteDate").html("");
            if (txtExecuteDate == "") {
                $("#p_lblExecuteDate").html("Nhập ngày thực hiện");
                flg = false;
            }
            else if (!ValidData.ValidDate(txtExecuteDate, "vn")) {
                $("#p_lblExecuteDate").html("Nhập ngày thực hiện");
                flg = false;
            }
            //
            $("#p_lblDeadline").html("");
            if (txtDeadline == "") {
                $("#p_lblDeadline").html("Nhập ngày kết thúc");
                flg = false;
            }
            else if (!ValidData.ValidDate(txtDeadline, "vn")) {
                $("#p_lblDeadline").html("Ngày kết thúc không hợp lệ");
                flg = false;
            }
            // 
            $("#p_lblUserExecute").html("");
            if (userExecute == "") {
                $("#p_lblUserExecute").html("Chọn nhóm/người thực hiện");
                flg = false;
            }
            //
            $("#p_lblUserFollow").html("");
            if (userFollow == "") {
                $("#p_lblUserFollow").html("Chọn nhóm/người theo dõi:");
                flg = false;
            }
            //
            $("#p_lblNote").html("");
            if (txtNote !== "") {
                if (txtNote.length > 120) {
                    $("#p_lblNote").html("Mô tả giới hạn [1-120] ký tự");
                    flg = false;
                }
                else if (!FormatKeyword.test(txtNote)) {
                    $('#p_lblNote').html("Mô tả không hợp lệ");
                    flg = false;
                }
            }
            // submit
            if (flg) {
                if ($(this).data("type") == 1) {
                    workController.ProcessAdd();
                }
                else if ($(this).data("type") == 2) {
                    workController.ProcessUpdate();
                }
                else
                    Notifization.Error(MessageText.Datamissing);
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
        });

        $("#pf_btnProcessSave").off("click").on("click", function () {
            var flg = true;
            var txtExecuteDate = $("#pf_txtExecuteDate").val();
            var txtDeadline = $("#pf_txtDeadline").val();
            var htmlNote = $("#pf_txtNote").val();
            // 
            var userExecute = $("#pf_userExecute").val();
            var userFollow = $("#pf_userFollow").val();
            //$("#pf_userExecute").find("option:selected").each(function () {
            //    userExecute.push($(this).val());
            //});
            //$("#pf_userFollow").find("option:selected").each(function () {
            //    userFollow.push($(this).val());
            //});
            // 
            $("#pf_lblExecuteDate").html("");
            if (txtExecuteDate == "") {
                $("#pf_lblExecuteDate").html("Nhập ngày thực hiện");
                flg = false;
            }
            else if (!ValidData.ValidDate(txtExecuteDate, "vn")) {
                $("#pf_lblExecuteDate").html("Nhập ngày thực hiện");
                flg = false;
            }
            //
            $("#pf_lblDeadline").html("");
            if (txtDeadline == "") {
                $("#pf_lblDeadline").html("Nhập ngày kết thúc");
                flg = false;
            }
            else if (!ValidData.ValidDate(txtDeadline, "vn")) {
                $("#pf_lblDeadline").html("Ngày kết thúc không hợp lệ");
                flg = false;
            }
            //
            $("#pf_lblUserExecute").html("");
            if (userExecute == "") {
                $("#pf_lblUserExecute").html("Chọn nhóm/người thực hiện");
                flg = false;
            }
            //
            $("#pf_lblUserFollow").html("");
            if (userFollow == "") {
                $("#pf_lblUserFollow").html("Chọn nhóm/người theo dõi:");
                flg = false;
            }
            //
            $("#pf_lblNote").html("");
            if (htmlNote !== "") {
                if (htmlNote.length > 120) {
                    $("#pf_lblNote").html("Mô tả giới hạn [1-120] ký tự");
                    flg = false;
                }
                else if (!FormatKeyword.test(htmlNote)) {
                    $('#pf_lblNote').html("Mô tả không hợp lệ");
                    flg = false;
                }
            }
            // submit
            if (flg) {
                workController.ProcessAssign();
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
                            var _receptionName = SubStringText.SubTitle(item.ReceptionName);
                            var _progressClass = WorProgress(item.Progress);
                            var _level = 0;
                            //var _orderId = item.OrderID;
                            //var _actionSort = `<i data-sortup="btn-sort-up" data-id="${id}" data-order"${_orderId}" class="fas fa-arrow-circle-up icon-mnsort"></i> <i data-sortdown ="btn-sort-down" data-id="${id}" data-order"${_orderId}" class="fas fa-arrow-circle-down icon-mnsort"></i>`;
                            //
                            //  role 
                            var action = HelperModel.RolePermission(result.role, "workController", id);
                            var rowNum = parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                            rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>
                                 <td>${_title}</td>                                                                                            
                                 <td>${_receptionName}</td>                                                                               
                                 <td class="text-center">${item.ExecuteDate}</td>
                                 <td class="text-center">${item.Deadline}</td>
                                 <td class="text-center"><div class="bg-state ${_progressClass}">${item.Progress}%</div></td>
                                 <td class="text-center">${HelperModel.StatusIcon(item.Enabled)}</td>
                                 <td class="tbcol-action">${action}</td>
                            </tr>`;
                            var subData = item.SubData;
                            if (subData != undefined && subData != null && subData.length > 0) {
                                rowData += workController.SubDataList(index, subData, _level, result.role);
                            }
                        });
                        $("tbody#TblData").html(rowData);
                        if (parseInt(totalPage) > 1) {
                            Paging.Pagination("#Pagination", totalPage, currentPage, workController.DataList);
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
    SubDataList: function (_index, lstModel, _level, _role) {
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
                // var _actionSort = `<i data-sortup='btn-sort-up' data-id='${id}' data-order'${_orderId}' class='fas fa-arrow-circle-up icon-mnsort'></i> <i data-sortdown ='btn-sort-down' data-id='${id}' data-order'${_orderId}' class='fas fa-arrow-circle-down icon-mnsort'></i>`;
                //
                var action = HelperModel.RolePermission(_role, "workController", id);
                var rowNum = '';//parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                var _title = SubStringText.SubTitle(item.Title);
                var _receptionName = SubStringText.SubTitle(item.ReceptionName);
                var _progressClass = WorProgress(item.Progress);
                _index = _level * 15;
                rowData += `<tr><td class="text-right">${rowNum}&nbsp;</td>
                  <td><div style='padding-left:${_index}px'>${index}. ${_title}</div></td> 
                  <td>${_receptionName}</td>       
                  <td class="text-center">${item.ExecuteDate}</td>
                  <td class="text-center">${item.Deadline}</td>
                  <td class="text-center"><div class="bg-state ${_progressClass}">${item.Progress}%</div></td>
                  <td class="text-center">${HelperModel.StatusIcon(item.Enabled)}</td>
                  <td class="tbcol-action">${action}</td></tr>`;
                var subData = item.SubData;
                if (subData != undefined && subData != null && subData.length > 0) {
                    rowData += workController.SubDataList(_index, subData, _level, _role);
                }
            });
        }
        return rowData;
    },
    Create: function () {
        var txtTitle = $("#txtTitle").val();
        var txtContent = tinyMCE.editors[$("#txtContent").attr("id")].getContent();
        var txtExecuteDate = $("#txtExecuteDate").val();
        var txtDeadline = $("#txtDeadline").val();
        var ddlState = $("#ddlState").val();
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
            Title: txtTitle,
            HtmlText: txtContent,
            ExecuteDate: txtExecuteDate,
            Deadline: txtDeadline,
            Files: arrFile,
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
        var txtTitle = $("#txtTitle").val();
        var txtContent = tinyMCE.editors[$("#txtContent").attr("id")].getContent();
        var txtExecuteDate = $("#txtExecuteDate").val();
        var txtDeadline = $("#txtDeadline").val();
        var ddlState = $("#ddlState").val();
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
            ID: id,
            Title: txtTitle,
            HtmlText: txtContent,
            ExecuteDate: txtExecuteDate,
            Deadline: txtDeadline,
            Files: arrFile,
            State: ddlState
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
            ID: id
        };
        AjaxFrom.POST({
            url: URLC + "/Delete",
            data: model,
            success: function (response) {
                if (response !== null) {
                    if (response.status === 200) {
                        Notifization.Success(response.message);
                        workController.DataList(pageIndex);
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
        Confirm.Delete(id, workController.Delete, null, null);
    },
    // assign 
    WorkAdd: function () {
        var ddlWork = $("#txtWork").val();
        var ddlAssign = $("#a_ddlAssign").val();
        var receptionType = $("#a_ddlAssign").find(':selected').data('type');
        var txtTitle = $("#a_txtTitle").val();
        var txtContent = tinyMCE.editors[$("#a_txtContent").attr("id")].getContent();
        var txtExecuteDate = $("#a_txtExecuteDate").val();
        var txtDeadline = $("#a_txtDeadline").val();
        var ddlState = $("#a_ddlState").val();
        // file
        var arrFile = [];
        var _fileList = $('#assignModal .pre-view .pre-item-box');
        if (_fileList.length > 0) {
            $.each(_fileList, function (index, preItem) {
                var idFile = $(this).data("id");
                if (idFile != undefined && idFile.length > 0)
                    arrFile.push(idFile);
            });
        }
        //  
        var model = {
            CategoryID: ddlWork,
            AssignTo: ddlAssign,
            ReceptionType: receptionType,
            Title: txtTitle,
            HtmlText: txtContent,
            ExecuteDate: txtExecuteDate,
            Deadline: txtDeadline,
            Files: arrFile,
            State: ddlState
        };
        AjaxFrom.POST({
            url: URLC + "/WorkAdd",
            data: model,
            success: function (response) {
                if (response !== null) {
                    if (response.status === 200) {
                        Notifization.Success(response.message);
                        FData.ResetForm();
                        workController.AssignList(pageIndex);
                        $("select#a_ddlAssignTo").prop('disabled', true);
                        $("select#a_ddlAssignTo")[0].selectedIndex = 0;
                        $("select#a_ddlAssignTo").selectpicker('refresh');
                        $("#a_btnAssign").addClass('disabled');
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
    WorkUpdate: function () {
        var id = $("#txtID").val();
        var ddlAssign = $("#a_ddlAssign").val();
        var receptionType = $("#a_ddlAssign").find(':selected').data('type');
        var txtTitle = $("#a_txtTitle").val();
        var txtContent = $("#a_txtContent").val();
        var txtExecuteDate = $("#a_txtExecuteDate").val();
        var txtDeadline = $("#a_txtDeadline").val();
        var ddlState = $("#a_ddlState").val();
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
            AssignTo: ddlAssign,
            ReceptionType: receptionType,
            Title: txtTitle,
            HtmlText: txtContent,
            ExecuteDate: txtExecuteDate,
            Deadline: txtDeadline,
            Files: arrFile,
            State: ddlState
        };
        AjaxFrom.POST({
            url: URLC + "/WorkUpdate",
            data: model,
            success: function (response) {
                if (response !== null) {
                    if (response.status === 200) {
                        Notifization.Success(response.message);
                        workController.AssignList(pageIndex);
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
    Assign: function () {
        var txtWork = $("#txtWork").val();
        var ddlAssign = $("#ddlAssignTo").val();
        var receptionType = $("#ddlAssignTo").find(':selected').data('type');
        //  
        var model = {
            WorkID: txtWork,
            AssignTo: ddlAssign,
            ReceptionType: receptionType,
        };
        AjaxFrom.POST({
            url: URLC + "/Assign",
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
    AssignList: function (page) {
        var ddlTimeExpress = "";
        var txtStartDate = "";
        var txtEndDate = "";
        var model = {
            Query: "",
            Page: page,
            TimeExpress: "",
            StartDate: txtStartDate,
            EndDate: txtEndDate,
            TimeZoneLocal: LibDateTime.GetTimeZoneByLocal(),
            Status: -1,
            WorkID: $("#txtWork").val()
        };
        //
        AjaxFrom.POST({
            url: URLC + "/AssignList",
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
                            var _receptionName = SubStringText.SubTitle(item.ReceptionName);
                            // icon sort
                            var _level = 0;
                            //var _orderId = item.OrderID;
                            //var _actionSort = `<i data-sortup="btn-sort-up" data-id="${id}" data-order"${_orderId}" class="fas fa-arrow-circle-up icon-mnsort"></i> <i data-sortdown ="btn-sort-down" data-id="${id}" data-order"${_orderId}" class="fas fa-arrow-circle-down icon-mnsort"></i>`;
                            //
                            //  role 
                            var action = `<i class="fas fa-edit btn-" data-id="${id}" data-assignto="${item.AssignTo}" data-receptiontype="${item.ReceptionType}" data-asgedit="true"></i> &nbsp; <i class="fas fa-times-circle btn-" data-id="${id}" data-asgdelete="true"></i>`;
                            var rowNum = parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                            rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>
                                 <td>${_title}</td>                                                                                            
                                 <td>${_receptionName}</td>                                                                                            
                                 <td class="text-center">${item.ExecuteDate}</td>
                                 <td class="text-center">${item.Deadline}</td>
                                 <td class="tbcol-action">${action}</td>
                            </tr>`;
                        });
                        $("tbody#TblData").html(rowData);
                        if (parseInt(totalPage) > 1) {
                            Paging.Pagination("#Pagination", totalPage, currentPage, workController.DataList);
                        }
                        return;
                    } else if (result.status == 404) {
                        $("select#ddlAssignTo").prop('disabled', false);
                        $("select#ddlAssignTo").selectpicker('refresh');
                        $("#btnAssign").removeClass('disabled');
                    }
                    return;
                }
            },
            error: function (result) {
                console.log("::" + MessageText.NotService);
            }
        });
    },
    AssignDelete: function (id) {
        var model = {
            ID: id
        };
        AjaxFrom.POST({
            url: URLC + "/Delete",
            data: model,
            success: function (response) {
                if (response !== null) {
                    if (response.status === 200) {
                        Notifization.Success(response.message);
                        workController.AssignList(pageIndex);
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
    // process  
    ProcessAdd: function () {
        var cateId = $("#p_txtCateId").val();
        var txtTitle = $("#p_txtTitle").val();
        var txtContent = tinyMCE.editors[$("#p_txtContent").attr("id")].getContent();
        var txtExecuteDate = $("#p_txtExecuteDate").val();
        var txtDeadline = $("#p_txtDeadline").val();
        //
        var userExecute = $("#p_userExecute").val();
        var userFollow = $("#p_userFollow").val();
        var htmlNote = $("#p_txtNote").val();
        // file
        var arrFile = [];
        var _fileList = $('#processModal .pre-view .pre-item-box');
        if (_fileList.length > 0) {
            $.each(_fileList, function (index, preItem) {
                var idFile = $(this).data("id");
                if (idFile != undefined && idFile.length > 0)
                    arrFile.push(idFile);
            });
        }
        //
        var model = {
            CategoryID: cateId,
            Title: txtTitle,
            HtmlText: txtContent,
            ExecuteDate: txtExecuteDate,
            Deadline: txtDeadline,
            Files: arrFile,
            //
            UserExecutes: userExecute,
            UserFollows: userFollow,
            HtmlNote: htmlNote
        };
        AjaxFrom.POST({
            url: URLC + "/ProcessAdd",
            data: model,
            success: function (response) {
                if (response !== null) {
                    if (response.status === 200) {
                        Notifization.Success(response.message);
                        workController.AssignList(pageIndex);
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
    ProcessUpdate: function () {
        var id = $("#p_txtId").val();
        var workId = $("#p_txtWorkID").val();
        var txtTitle = $("#p_txtTitle").val();
        var txtContent = tinyMCE.editors[$("#p_txtContent").attr("id")].getContent();
        var txtExecuteDate = $("#p_txtExecuteDate").val();
        var txtDeadline = $("#p_txtDeadline").val();
        //
        var userExecute = $("#p_userExecute").val();
        var userFollow = $("#p_userFollow").val();
        var txtNote = $("#p_txtNote").val();
        // file
        var arrFile = [];
        var _fileList = $('#processModal .pre-view .pre-item-box');
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
            WorkID: workId,
            Title: txtTitle,
            HtmlText: txtContent,
            ExecuteDate: txtExecuteDate,
            Deadline: txtDeadline,
            Files: arrFile,
            //
            UserExecutes: userExecute,
            UserFollows: userFollow,
            HtmlNote: txtNote
        };
        AjaxFrom.POST({
            url: URLC + "/ProcessUpdate",
            data: model,
            success: function (response) {
                if (response !== null) {
                    if (response.status === 200) {
                        Notifization.Success(response.message);
                        workController.AssignList(pageIndex);
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
    ProcessAssign: function () {
        var id = $("#pf_txtId").val();
        var txtExecuteDate = $("#pf_txtExecuteDate").val();
        var txtDeadline = $("#pf_txtDeadline").val();
        //
        var userExecute = $("#pf_userExecute").val();
        var userFollow = $("#pf_userFollow").val();
        var txtNote = $("#pf_txtNote").val();
        // file
        var model = {
            ID: id,
            ExecuteDate: txtExecuteDate,
            Deadline: txtDeadline,
            //
            UserExecutes: userExecute,
            UserFollows: userFollow,
            HtmlNote: txtNote
        };
        AjaxFrom.POST({
            url: URLC + "/ProcessAssign",
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
};

workController.init();
// 
$(document).on("keyup", "#txtTitle", function () {
    var txtTitle = $(this).val();
    if (txtTitle === "") {
        $("#lblTitle").html("Nhập tên công việc");
    }
    else if (txtTitle.length < 1 || txtTitle.length > 80) {
        $("#lblTitle").html("Tên công việc giới hạn [1-80] ký tự");
    }
    else if (!FormatKeyword.test(txtTitle)) {
        $("#lblTitle").html("Tên công việc không hợp lệ");
    }
    else {
        $("#lblTitle").html("");
    }
});

$(document).on("blue", "#txtContent", function () {
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
        $("#lblExecuteDate").html("Nhập ngày thực hiện");
    }
});

$(document).on("keyup", "#txtDeadline", function () {
    var txtDeadline = $(this).val();
    $("#lblDeadline").html("");
    if (txtDeadline == "") {
        $("#lblDeadline").html("Nhập ngày kết thúc");
    }
    else if (!ValidData.ValidDate(txtDeadline, "vn")) {
        $("#lblDeadline").html("Ngày kết thúc không hợp lệ");
    }
});

//asign modal**************************************************************************************
$(document).on("keyup", "#a_txtTitle", function () {
    var txtTitle = $(this).val();
    $("#a_lblTitle").html("");
    if (txtTitle === "") {
        $("#a_lblTitle").html("Nhập tên công việc");
    }
    else if (txtTitle.length < 1 || txtTitle.length > 80) {
        $("#a_lblTitle").html("Tên công việc giới hạn [1-80] ký tự");
    }
    else if (!FormatKeyword.test(txtTitle)) {
        $("#a_lblTitle").html("Tên công việc không hợp lệ");
    }
});
$(document).on("blue", "#a_txtContent", function () {
    var txtContent = $(this).val();
    $("#a_lblContent").html("");
    if (txtContent == "") {
        $("#a_lblContent").html("Nhập nội dung");
    }
    else if (txtContent.length > 5000) {
        $("#a_lblContent").html("Nội dung giới hạn [1-5000] ký tự");
    }
    else if (!FormatKeyword.test(txtContent)) {
        $("#a_lblContent").html("Nội dung không hợp lệ");
    }
});
$(document).on("keyup", "#a_txtExecuteDate", function () {
    var txtExecuteDate = $(this).val();
    $("#a_lblExecuteDate").html("");
    if (txtExecuteDate == "") {
        $("#a_lblExecuteDate").html("Nhập ngày thực hiện");
    }
    else if (!ValidData.ValidDate(txtExecuteDate, "vn")) {
        $("#a_lblExecuteDate").html("Nhập ngày thực hiện");
    }
});
$(document).on("keyup", "#a_txtDeadline", function () {
    var txtDeadline = $(this).val();
    $("#a_lblDeadline").html("");
    if (txtDeadline == "") {
        $("#a_lblDeadline").html("Nhập ngày kết thúc");
    }
    else if (!ValidData.ValidDate(txtDeadline, "vn")) {
        $("#a_lblDeadline").html("Ngày kết thúc không hợp lệ");
    }
});
$(document).on("change", "#a_ddlAssignTo", function () {
    var ddlAssign = $(this).val();
    $("#a_lblAssignTo").html("");
    if (ddlAssign == "") {
        $("#a_lblAssignTo").html("Chọn bộ phận tiếp nhận");
    }
});

//process modal **************************************************************************************
$(document).on("keyup", "#p_txtTitle", function () {
    var txtTitle = $(this).val();
    $("#p_lblTitle").html("");
    if (txtTitle === "") {
        $("#p_lblTitle").html("Nhập tên công việc");
    }
    else if (txtTitle.length < 1 || txtTitle.length > 80) {
        $("#p_lblTitle").html("Tên công việc giới hạn [1-80] ký tự");
    }
    else if (!FormatKeyword.test(txtTitle)) {
        $("#p_lblTitle").html("Tên công việc không hợp lệ");
    }

});
$(document).on("blue", "#p_txtContent", function () {
    var txtContent = $(this).val();
    $("#p_lblContent").html("");
    if (txtContent == "") {
        $("#p_lblContent").html("Nhập nội dung");
    }
    else if (txtContent.length > 5000) {
        $("#p_lblContent").html("Nội dung giới hạn [1-5000] ký tự");
    }
    else if (!FormatKeyword.test(txtContent)) {
        $("#p_lblContent").html("Nội dung không hợp lệ");
    }
});
$(document).on("keyup", "#p_txtExecuteDate", function () {
    var txtExecuteDate = $(this).val();
    $("#p_lblExecuteDate").html("");
    if (txtExecuteDate == "") {
        $("#p_lblExecuteDate").html("Nhập ngày thực hiện");
    }
    else if (!ValidData.ValidDate(txtExecuteDate, "vn")) {
        $("#p_lblExecuteDate").html("Nhập ngày thực hiện");
    }
});
$(document).on("keyup", "#p_txtDeadline", function () {
    var txtDeadline = $(this).val();
    $("#p_lblDeadline").html("");
    if (txtDeadline == "") {
        $("#p_lblDeadline").html("Nhập ngày kết thúc");
    }
    else if (!ValidData.ValidDate(txtDeadline, "vn")) {
        $("#p_lblDeadline").html("Ngày kết thúc không hợp lệ");
    }
});
$(document).on("change", "#p_ddlAssignTo", function () {
    var ddlAssign = $(this).val();
    $("#p_lblAssignTo").html("");
    if (ddlAssign == "") {
        $("#p_lblAssignTo").html("Chọn bộ phận tiếp nhận");
    }
});

$(document).on("change", "#p_userExecute", function () {
    var userExecute = $(this).val();
    $("#p_lblUserExecute").html("");
    if (userExecute == "") {
        $("#p_lblUserExecute").html("Chọn nhóm/người thực hiện")
    }
});
$(document).on("change", "#p_userFollow", function () {
    var userExecute = $(this).val();
    $("#p_lblUserFollow").html("");
    if (userExecute == "") {
        $("#p_lblUserFollow").html("Chọn nhóm/người theo dõi:");
    }
});
$(document).on('keyup', '#p_txtNote', function () {
    var txtNote = $(this).val();
    $("#p_lblNote").html("");
    if (txtNote !== "") {
        if (txtNote.length > 120) {
            $("#p_lblNote").html("Lưu ý giới hạn [1-120] ký tự");
        }
        else if (!FormatKeyword.test(txtNote)) {
            $("#p_lblNote").html("Lưu ý không hợp lệ");
        }
    }
});
//
function DdlReception(_control, _id) {
    if (_control == undefined)
        return;
    //
    var option = `<option value="">-lựa chọn-</option>`;
    $(_control).html(option);
    $(_control).selectpicker('refresh');
    var model = {

    };
    //GetTicketing
    AjaxFrom.POST({
        url: '/Management/Work/Action/AssignOption',
        data: model,
        success: function (response) {
            if (response !== null && response.status === 200) {
                var _option = "";
                $.each(response.data.OutSite, function (index, item) {
                    var id = item.ID;
                    var selectId = "";
                    if (id == _id)
                        selectId = "selected";
                    //
                    var title = item.Title;
                    _option += `<option value='${id}' data-type='${item.Cate}' ${selectId}>${title}</option>`;
                });
                if (_option != "") {
                    _option += `<option data-divider='true'></option>`;
                }
                $.each(response.data.Internal, function (index, item) {
                    var id = item.ID;
                    var selectId = "";
                    if (id == _id)
                        selectId = "selected";
                    //
                    var title = item.Title;
                    _option += `<option value='${id}' data-type='${item.Cate}' ${selectId}>${title}</option>`;
                });
                $(_control).html(option + _option);
                $(_control).selectpicker('refresh');
                return;
            }
        },
        error: function (result) {
            console.log('::' + MessageText.NotService);
        }
    });
}
function DdlAssignInternal(_control, _id) {
    if (_control == undefined)
        return;
    //
    var option = `<option value="">-lựa chọn-</option>`;
    $(_control).html(option);
    $(_control).selectpicker('refresh');
    var model = {

    };
    //GetTicketing
    AjaxFrom.POST({
        url: '/Management/Work/Action/AssignInOption',
        data: model,
        success: function (response) {
            if (response !== null) {
                if (response.status === 200) {
                    var _option = "";
                    $.each(response.data, function (index, item) {
                        var id = item.ID;
                        var selectId = "";
                        if (id == _id)
                            selectId = "selected";
                        //
                        var title = item.Title;
                        _option += `<option value='${id}' data-type='${item.Cate}' ${selectId}>${title}</option>`;
                    });
                    $(_control).html(option + _option);
                    $(_control).selectpicker('refresh');
                    return;
                }
            }
            return;
        },
        error: function (result) {
            console.log('::' + MessageText.NotService);
        }
    });
}
function DdlState(_control, _id) {
    if (_control == undefined)
        return;
    //
    var option = `<option value="">-lựa chọn-</option>`;
    $(_control).html(option);
    $(_control).selectpicker('refresh');
    var model = {

    };
    //GetTicketing
    AjaxFrom.POST({
        url: '/Management/Work/Action/StateOption',
        data: model,
        success: function (response) {
            if (response !== null) {
                if (response.status === 200) {
                    //
                    var _option = "";
                    $.each(response.data, function (index, item) {
                        var id = item.ID;
                        var selectId = "";
                        if (id == _id)
                            selectId = "selected";
                        //
                        var title = item.Title;
                        _option += `<option value='${id}' data-type='${item.Cate}' ${selectId}>${title}</option>`;
                    });
                    $(_control).html(option + _option);
                    $(_control).selectpicker('refresh');
                    return;
                }
            }
            return;
        },
        error: function (result) {
            console.log('::' + MessageText.NotService);
        }
    });
}

async function DdlWorkUserExecutes(_control, _departmentId, _workId) {
    if (_control == undefined)
        return;
    //
    var model = {
        DepartmentID: _departmentId,
        WorkID: _workId
    };
    //GetTicketing
    await AjaxFrom.POST({
        url: '/Management/Work/Action/WorkUserExecutes',
        data: model,
        success: function (response) {
            if (response !== null && response.status === 200) {
                var _option = "";
                $.each(response.data, function (index, item) {
                    var id = item.ID;
                    var selectId = "";
                    if (item.IsSelected)
                        selectId = "selected";
                    // 
                    _option += `<option value='${id}' ${selectId}>${item.Title}</option>`;
                });
                $(_control).html(_option);
                $(_control).selectpicker('refresh');
                return;
            }
        },
        error: function (result) {
            console.log('::' + MessageText.NotService);
        }
    });
}
async function DdlWorkUserFollows(_control, _departmentId, workId) {
    if (_control == undefined)
        return;
    //
    var model = {
        DepartmentID: _departmentId,
        WorkID: workId
    };
    //GetTicketing
    await AjaxFrom.POST({
        url: '/Management/Work/Action/WorkUserFollows',
        data: model,
        success: function (response) {
            if (response !== null && response.status === 200) {
                var _option = "";
                $.each(response.data, function (index, item) {

                    var id = item.ID;
                    var selectId = "";
                    if (item.IsSelected)
                        selectId = "selected";
                    // 
                    _option += `<option value='${id}' ${selectId}>${item.Title}</option>`;
                });
                $(_control).html(_option);
                $(_control).selectpicker('refresh');
                return;
            }
        },
        error: function (result) {
            console.log('::' + MessageText.NotService);
        }
    });
}
function WorProgress(_val) {
    var result = "";
    if (_val < 40) {
        result = "bg-state-1";
    }
    else if (_val >= 40 && _val <= 60) {
        result = "bg-state-2";
    }
    else if (_val >= 60 && _val <= 80) {
        result = "bg-state-3";
    }
    else if (_val >= 80) {
        result = "bg-state-4";
    }
    return result;
}
//**************************************************************************************
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
                    workController.DataList(pageIndex);
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
                    workController.DataList(pageIndex);
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


