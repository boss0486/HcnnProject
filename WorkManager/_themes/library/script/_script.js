$('form').submit(function (e) {
    e.preventDefault();
    //do something
});
//# LOGOUT ##################################################################################################################################################################
function Logout() {
    AjaxFrom.POST({
        url: '/Authentication/Action/Logout',
        data: '',
        success: function (response) {
            if (response !== null) {
                if (response.status === 200) {
                    Notifization.Success(response.message);
                    setTimeout(function () {
                        location.href = "/Authentication";
                    }, 2000)
                    return;
                }
                else {
                    Notifization.Error(MessageText.NotService);
                    return;
                }
            }
            Notifization.Error(MessageText.NotService);
            return;
        },
        error: function (response) {
            console.log('::' + MessageText.NotService);
        }
    });

}
//###################################################################################################################################################################

function LoadDefaultOption() {
    var dfOption = "<option value=''>-Lựa chọn-</option>";
    $('#ddlDepartmentPart').html(dfOption);
    $('select').selectpicker('refresh');
}
 