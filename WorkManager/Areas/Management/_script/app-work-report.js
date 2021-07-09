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

        $('#btnExport').off('click').on('click', function () {
            appWorkReportController.Export();
        });
    },
    DataList: function (page) {

        var sumTask = {
            Sum_Total: 0,
            Sum_Assigned: 0,
            Sum_Inprogress: 0,
            Sum_Completed: 0,
            Sum_Pause: 0,
            Sum_OutDate: 0
        }

        AjaxFrom.POST({
            url: URLC + '/DataList',
            data: appWorkReportController.GetInputData(page),
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
                            sumTask.Sum_Total += item.Total;
                            sumTask.Sum_Assigned += item.Total_Assigned;
                            sumTask.Sum_Inprogress += item.Total_Inprogress;
                            sumTask.Sum_Completed += item.Total_Completed;
                            sumTask.Sum_Pause += item.Total_Pause;
                            sumTask.Sum_OutDate += item.Total_OutDate;
                            var rowNum = parseInt(index) + (parseInt(currentPage) - 1) * parseInt(pageSize);
                            rowData += `
                            <tr>
                                 <td class="text-right">${rowNum}&nbsp;</td>                               
                                 <td class="text-right">${item.Assignee}</td>                                  
                                 <td class="text-right">${item.Total}</td> 
                                 <td class="text-right">${item.Total_Assigned}</td>
                                 <td class="text-right">${item.Total_Inprogress}</td>
                                 <td class="text-right">${item.Total_Completed}</td>
                                 <td class="text-right">${item.Total_Pause}</td>
                                 <td class="text-right">${item.Total_OutDate}</td>
                            </tr>`;
                        });
                        rowData += `
                            <tr>
                                 <td colspan="2">Tổng</td>                               
                                 <td class="text-right">${sumTask.Sum_Total}</td>                                  
                                 <td class="text-right">${sumTask.Sum_Assigned}</td> 
                                 <td class="text-right">${sumTask.Sum_Inprogress}</td>
                                 <td class="text-right">${sumTask.Sum_Completed}</td>
                                 <td class="text-right">${sumTask.Sum_Pause}</td>
                                 <td class="text-right">${sumTask.Sum_OutDate}</td>
                            </tr>`;
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
    },
    Export: function () {
        AjaxFrom.POST({
            url: `${URLC}/DataList`,
            data: appWorkReportController.GetInputData(null, true),
            success: function (result) {
                if (result !== null) {
                    if (result.status === 200) {
                        HelperModel.Download(result.path);
                    }
                    else {
                        Notifization.Error(result.message);
                        return;
                    }
                }
                return;
            },
            error: function (result) {
                console.log('::' + MessageText.NotService);
            }
        });
    },
    GetInputData: function (page, isExport = false) {
        var ddlDepartment = $('#ddlDepartment').val();
        var ddlUser = $('#ddlUser').val();

        //get report type via query string (1: department | 2: user)
        var urlParams = new URLSearchParams(window.location.search);
        var reportType = urlParams.get('type');

        var model = {
            Page: page,
            EmpId: ddlUser != "-1" ? ddlUser : null,
            DepartmentId: ddlDepartment != "-1" ? ddlDepartment : null,
            ReportType: reportType ? reportType : 1,
            IsExportExcel: isExport
        };
        return model;
    }
};
appWorkReportController.init();

