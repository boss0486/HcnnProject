var chatService = null;



$(function () {
    // Reference the auto-generated proxy for the hub.
    chatService = $.connection.chatHub;
    chatService.client.connect = function (model) {
        console.log(model);
    };
    chatService.client.joinRoom = function (model) {
        console.log(model);
    };
    // resutt server
    chatService.client.chatLog = function (model) {

        var roomId = model.RoomID;
        if (roomId != undefined && roomId != "")
            ChatApplication.chatLog(roomId);
    };
    // Set initial focus to message input box.
    $('#chatInput').focus();
    // Start the connection.
    $.connection.hub.start().done(function () {
        // connect to room **********************************************************
        $("select#ddlChatRoom")[0].selectedIndex = 1;
        $("select#ddlChatRoom").selectpicker('refresh');
        $("select#ddlChatRoom").change();
        // end connect to room ******************************************************

        var idLog = $("body").data("id");
        if (idLog == null || idLog == "") {
            console.log("id: invalid!");
            return;
        }
        //
        chatService.server.connect(idLog);
        //
        $('#chatSend').click(function () {
            var roomId = $("#ddlChatRoom").val();
            var input = $("#chatInput").val();
            if (roomId == "" || input == "" || input == null) {
                console.log("Loi gui tin nhan");
                return;
            }
            //
            chatService.server.sendMessage({
                RoomID: roomId,
                UserID: idLog,
                Message: input
            });
            $('#chatInput').val('').focus();
        });


    });

});

$(document).on("change", "select#ddlChatRoom", function () {
    ChatApplication.joinRoom($(this).val());
    ChatApplication.chatLog($(this).val());
});

class ChatApplication {

    static chatLog = function (roomId) {
        Loading.HideLoading();
        var model = {
            RoomID: roomId
        };
        AjaxFrom.POST({
            url: '/Management/Chat/Action/ChatLog',
            data: model,
            success: function (result) {
                if (result == null || result.status != 200) {
                    return;
                }
                // 
                if (result.data.length == 0) {
                    $("#chatLog").html("Chua co tin nhan nao");
                    return;
                }
                //
                $("#chatLog").html("");
                var idLog = $("body").data("id");
                $.each(result.data, function (index, item) {
                    var _html = `<div class='form-group text-center time-line'><span>${item.DataDate}</span></div>`;
                    if (item.Items.length > 0) {
                        var dtTime = item.Items[0].DataTime;
                        var logId = item.Items[0].CreatedBy;
                        var name = item.Items[0].Name;
                        var strName = `<div class='name'>${formatName(name)} <span class='time'>${dtTime}</span></div>`;
                        var msgType = 'msg-left';
                        if (logId == idLog) {
                            msgType = 'msg-right';
                            strName = `<div class='name'><span class='time'>${dtTime}</span> ${formatName(name)}</div>`;
                        }
                        _html += `<div class='msg-group ${msgType}'>${strName}<div class='msg-content'>`;
                        $.each(item.Items, function (_index, _item) {

                            logId = _item.CreatedBy;
                            name = _item.Name;

                            msgType = 'msg-left';
                            strName = `<div class='name'>${formatName(name)} <span class='time'>${_item.DataTime}</span></div>`;
                            if (logId == idLog) {
                                msgType = 'msg-right';
                                strName = `<div class='name'><span class='time'>${_item.DataTime}</span> ${formatName(name)}</div>`;
                            }
                            if (dtTime === _item.DataTime) {
                                _html += `<span>${_item.Message}</span><br />`;
                            }
                            else {
                                dtTime = _item.DataTime;
                                _html += `</div></div>
                                    <div class='msg-group ${msgType}'>${strName}<div class='msg-content'><span> ${_item.Message}</span><br />`;
                            }
                        });
                    }
                    _html += `</div></div>`;
                    $("#chatLog").append(_html);
                });
                // show html 
                $('#chatLog').scrollTop($('#chatLog')[0].scrollHeight);
                return;
            },
            error: function (result) {
                Notifization.Error(MessageText.NotService);
                return;
            }
        }, false);
    }
    static chatNotify = function (roomId) {
        var model = {
        };
        AjaxFrom.POST({
            url: '/Management/Chat/Action/Notify',
            data: model,
            success: function (result) {
                if (result == null || result.status != 200) {
                    return;
                }
                // 
                if (result.data.length == 0) {
                    $("#boxChatNotify").html('');
                    return;
                }
                //
                $("#boxChatNotify").html("");
                $.each(result.data, function (index, item) {
                    if (item.Items.length > 0) {
                        var dtTime = item.DataTime;
                        var name = item.Name;
                        var summary = item.Summary;
                        var _html = `<li>
                                <a href="javascript:void(0);">
                                    <div class="icon-circle bg-light-green">
                                        <i class="material-icons">person_add</i>
                                    </div>
                                    <div class="menu-info">
                                        <h4>12 new members joined</h4>
                                        <p><i class="material-icons">access_time</i> 14 mins ago</p>
                                    </div>
                                </a>;
                            </li>`;

                        $("#boxChatNotify").append(_html);
                    }
                });
                // show html  
                return;
            },
            error: function (result) {
                Notifization.Error(MessageText.NotService);
                return;
            }
        }, false);
    }

    static joinRoom = function (roomId) {
        chatService.server.joinRoom(roomId);
    }

}
function formatName(_name) {
    var firstName = _name.split(' ').shift().slice(0, 2);
    var lastName = _name.split(' ').slice(-1).join(' ');
    return `${firstName}.${lastName}`;
}
