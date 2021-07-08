var pageIndex = 1;
var URLC = "/Management/Report/Action";
var URLA = "/Management/Report";

var appWorkReportController = {
    init: function () {
        appWorkReportController.registerEvent();
    },
    registerEvent: function () {
        $('#btnSearch').off('click').on('click', function () {
            appWorkReportController.DataList(1);
        });
    },
    DataList: function (page) {
        var ddlDepartment = $('#ddlDepartment').val();
        var ddlUser = $('#ddlUser').val();
        var model = {
            Page: page,
            EmpId: ddlUser != "-1" ? ddlUser : null,
            DepartmentId: ddlDepartment != "-1" ? ddlDepartment : null,
            ReportType: 2
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

                            var rowNum = parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                            rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>                               
                                 <td>${item.Assignee}</td>                                  
                                 <td>${item.Total}</td> 
                                 <td>${item.Total_Assigned}</td>
                                 <td>${item.Total_Inprogress}</td>
                                 <td>${item.Total_Completed}</td>
                                 <td>${item.Total_Pause}</td>
                                 <td>${item.Total_OutDate}</td>
                            </tr>`;
                        });
                        $('tbody#TblData').html(rowData);
                        if (parseInt(totalPage) > 1) {
                            Paging.Pagination("#Pagination", totalPage, currentPage, appWorkReportController.DataList);
                        }
                        return;
                    }
                    else {
                        console.log('::' + result.message);
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
};
appWorkReportController.init();

