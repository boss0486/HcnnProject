var pageIndex = 1;
var URLC = "/HomePage/Contact/Action";
var URLA = "/HomePage/Contact";
var ContactController = {
    init: function () {
        ContactController.registerEvent();
    },
    registerEvent: function () {
        $('#btnSend').off('click').on('click', function () {
            var flg = true;
            var txtName = $('#txtName').val();
            var txtEmail = $('#txtEmail').val();
            var txtPhone = $('#txtPhone').val();
            var txtContent = $('#txtContent').val();
            var subject = $(".contact-subject input[type='checkbox']:checked").data('id'); 
            //
            $('#lblName').html('');
            if (txtName == '') {
                $('#lblName').html('Không được để trống họ tên');
                flg = false;
            }
            else if (txtName.length < 1 || txtName.length > 50) {
                $('#lblName').html('Họ tên giới hạn [1-50] ký tự');
                flg = false;
            }
            else if (!FormatKeyword.test(txtName)) {
                $('#lblName').html('Họ tê không hợp lệ');
                flg = false;
            }
            //
            $('#lblPhone').html('');
            if (txtPhone != '') {
                if (txtPhone.length > 20) {
                    $('#lblPhone').html('Số đ.thoại giới hạn [1-20] ký tự');
                    flg = false;
                }
                else if (!FormatPhone.test(txtPhone)) {
                    $('#lblPhone').html('Số đ.thoại không hợp lệ');
                    flg = false;
                }
            }
            // 
            $('#lblEmail').html('');
            if (txtEmail != '') {
                if (txtEmail.length > 120) {
                    $('#lblEmail').html('E-mail giới hạn [1-120] ký tự');
                    flg = false;
                }
                else if (!FormatEmail.test(txtEmail)) {
                    $('#lblEmail').html('Địa chỉ e-mail không hợp lệ');
                    flg = false;
                }
            }
            // 
            $('#lblContent').html('');
            if (txtContent == '') {
                $('#lblContent').html('Không được để trống nội dung');
                flg = false;
            }
            else if (txtContent.length > 120) {
                $('#lblContent').html('Nội dung giới hạn [1-120] ký tự');
                flg = false;
            }
            else if (!FormatKeyword.test(txtContent)) {
                $('#lblContent').html('Nội dung không hợp lệ');
                flg = false;
            }
            //  
            $('#lblSubject').html('');
            console.log("::::::" + subject);
            if (subject == undefined || subject == "") {
                $('#lblSubject').html('Vui lòng chọn chủ đề tư vấn');
                flg = false;
            }
            // submit 
            if (flg) {
                ContactController.Send();
            }
            else {
                Notifization.Error(MessageText.Datamissing);
            }
        });
    },
    Send: function () {
        var txtName = $('#txtName').val();
        var txtEmail = $('#txtEmail').val();
        var txtPhone = $('#txtPhone').val();
        var txtContent = $('#txtContent').val();
        var subject = $(".contact-subject input[type='checkbox']:checked").data('id');
        //
        var model = {
            Title: txtName,
            Email: txtEmail,
            Phone: txtPhone,
            Content: txtContent,
            Subject: subject
        };
        AjaxFrom.POST({
            url: URLC + '/Send',
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
                console.log('::' + MessageText.NotService);
            }
        });

    }
};

ContactController.init();
$(document).on('keyup', '#txtName', function () {
    var txtName = $(this).val();
    $('#lblName').html('');
    if (txtName == '') {
        $('#lblName').html('Không được để trống họ tên');
    }
    else if (txtName.length < 1 || txtName.length > 50) {
        $('#lblName').html('Họ tên giới hạn [1-50] ký tự');
    }
    else if (!FormatKeyword.test(txtName)) {
        $('#lblName').html('Họ tê không hợp lệ');
    }
});
$(document).on('keyup', '#txtPhone', function () {
    var txtPhone = $(this).val();
    $('#lblPhone').html('');
    if (txtPhone != '') {
        if (txtPhone.length > 20) {
            $('#lblPhone').html('Số đ.thoại giới hạn [1-20] ký tự');
        }
        else if (!FormatPhone.test(txtPhone)) {
            $('#lblPhone').html('Số đ.thoại không hợp lệ');
        }
    }
});
$(document).on('keyup', '#txtEmail', function () {
    var txtEmail = $(this).val();
    $('#lblEmail').html('');
    if (txtEmail != '') {
        if (txtEmail.length > 120) {
            $('#lblEmail').html('E-mail giới hạn [1-120] ký tự');
        }
        else if (!FormatEmail.test(txtEmail)) {
            $('#lblEmail').html('Địa chỉ e-mail không hợp lệ');
        }
    }
});

$(document).on('keyup', '#txtContent', function () {
    var txtContent = $(this).val();
    $('#lblContent').html('');
    if (txtContent == '') {
        $('#lblContent').html('Không được để trống nội dung');
    }
    else if (txtContent.length > 120) {
        $('#lblContent').html('Nội dung giới hạn [1-120] ký tự');
    }
    else if (!FormatKeyword.test(txtContent)) {
        $('#lblContent').html('Nội dung không hợp lệ');
    }
});

$(document).on("change", ".contact-subject input[type='checkbox']", function () {
    $('#lblSubject').html('');
    var subject = $(".contact-subject input[type='checkbox']:checked").data("id");
    if (subject == undefined || subject == "") {
        $('#lblSubject').html('Vui lòng chọn chủ đề tư vấn');
    }
});