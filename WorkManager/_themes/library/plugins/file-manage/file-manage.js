class fmEnum {
    static fileType = {
        None: 0,
        Alone: 1,
        Multi: 2
    };
    static fileTEnum = {
        None: 0,
        Edittor: 1
    };
}
//
(function ($, w) {
    "use strict";
    var fm = {
        _pageIndex: 1,
        arrList: [],
        arrIDList: [],
        imgFile: null,
        content: "",
        fileType: 0,
        _lblTotal: 0,
        _ctrlInput: null,
        selected: []
    };
    var generateUUID = function (fileManage) {
        var d = new Date().getTime();//Timestamp
        var d2 = (performance && performance.now && (performance.now() * 1000)) || 0;//Time in microseconds since page-load or 0 if unsupported
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16;//random number between 0 and 16
            if (d > 0) {//Use timestamp until depleted
                r = (d + r) % 16 | 0;
                d = Math.floor(d / 16);
            } else {//Use microseconds since page-load if supported
                r = (d2 + r) % 16 | 0;
                d2 = Math.floor(d2 / 16);
            }
            return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });
    };
    var getCategory = async identifier => await fetch(`/Management/AttachmentCategory/Action/Option`, {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        method: "POST"
    }).then(response => {
        return (response.json());
    });
    var methods = (function () {
        var context = `<div data-bbb='${generateUUID()}' class="modal fade file-modal" id="fileManagerModal" tabindex="-1" role="dialog" aria-labelledby="exampleModalLongTitle" aria-hidden="true"><div class="modal-dialog" role="document"> <div class="modal-content"> <div class="modal-header"> <h5 class="modal-title" id="exampleModalLongTitle">Quản lý tệp tin</h5> <button type="button" class="close" data-dismiss="close" aria-label="Close"> <span aria-hidden="true">×</span> </button> </div><div class="modal-body" style="padding-top:15px;"> <div class="row"> <div class="col-md-3"> <div class="list-group-item list-group-item-info">Danh mục tệp tin</div><div class="vetical-menu"> <ul id="ddlFmnCategory" name="ddlFmnCategory" class="list-group"></ul> </div></div><div class="col-md-9"> <div class="row"> <div class="col-md-6"> <div class=" form-group input-group"> <input type="file" class="form-control" id="UploadFile" style="display:none;"/> <div class="form-control"><i id="btnOpenClientFile" data-ibtn="true" class="far fa-folder"></i> <span id="inputFileControl">&nbsp;</span></div><div id="btnUpload" class="input-group-addon"><i class="fas fa-upload" data-ibtn="true"></i></div><span id="lblFmnUpload" class="message msg-box"></span> </div></div><div class="col-md-2"> <div class="form-group"> <select id="ddlFmnType" name="ddlFmnType" class="form-control show-tick" data-live-search="true"> <option value="0">-Loại-</option> <option value="1">Hình ảnh</option> <option value="2">Tài liệu</option> <option value="3">Tệp nén</option> </select> </div></div><div class="col-md-4"> <div class="form-group input-group"> <input type="text" class="form-control" id="txtQueryFile" placeholder="Nhập từ khóa..."> <span id="btnSearchFile" data-ibtn="true" class="input-group-addon"><i class="fa fa-search -icons"></i></span> </div></div></div><hr style="margin: 0px;"/> <div class="row" id="FileManagerList"></div></div></div></div><div class="modal-footer"> <div id='FileManagerPagination' class='col-md-6'></div><div class='col-md-6 text-right'><button data-dismiss='close' class='btn btn-danger' style='margin: 20px auto' ;> &nbsp; Đóng &nbsp;</button></div></div></div></div></div>`
        var applyHandlers = function (fileManage) {
            $(document).on('click', '[data-dismiss="close"]', function () {
                //$("body").removeClass("modal-open");
                $("body .file-modal").removeClass("in");
            });
        };
        var fmnCategoryTree = {
            GetSubMenuCategoryList: function (_index, lstModel, _level, _id) {
                var rowData = '';
                //
                if (lstModel.length > 0) {
                    _level += 1;
                    rowData += `<ul>`;
                    $.each(lstModel, function (index, item) {
                        index = index + 1;
                        var id = item.ID;
                        if (id.length > 0)
                            id = id.trim();

                        var _title = SubStringText.SubTitle(item.Title);
                        var subMenu = item.SubOption;
                        var iAngle = '';
                        if (subMenu !== undefined && subMenu !== null && subMenu.length > 0)
                            iAngle = "<i class='fa fa-angle-left iplus'></i>";
                        var isChecked = "";
                        if (id !== null && id === _id)
                            isChecked = "checked";
                        //
                        var pading = _level * 38;

                        rowData += `<li>
                               <input id="cbxItem${id}" type="checkbox" class="filled-in" data-id='${id}' ${isChecked} />
                               <label for="cbxItem${id}">${_title}</label>`;

                        if (subMenu !== undefined && subMenu !== null && subMenu.length > 0) {
                            rowData += fmnCategoryTree.GetSubMenuCategoryList(_index, subMenu, _level, _id);
                        }
                        rowData += '</li>';
                    });
                    rowData += `</ul>`;
                }
                return rowData;
            }
        };



        return {
            // public methods
            init: function (op) {
                fm.fileType = op.fileType;
                fm.content = this;
                $("body").append(context);
                applyHandlers($(this));
                // load category
                (async () => {
                    'use strict';
                    //var cateId = $("#fileManagerModal #ddlFmnCategory input[type='checkbox']:checked").data('id');
                    const categoryData = await getCategory({
                        ID: ""
                    });
                    var _option_default = `<li><input id="cbxItem0" type="checkbox" class="filled-in" data-id='' /><label for="cbxItem0">Tất cả</label></li>`;

                    if (categoryData != null && categoryData.status == 200) {
                        $('ul#ddlFmnCategory').html(_option_default);
                        var rowData = '';
                        $.each(categoryData.data, function (index, item) {
                            index = index + 1;
                            var id = item.ID;
                            if (id.length > 0)
                                id = id.trim();
                            var _title = SubStringText.SubTitle(item.Title);
                            var subMenu = item.SubOption;
                            var iAngle = '';
                            if (subMenu !== undefined && subMenu !== null && subMenu.length > 0)
                                iAngle = "<i class='fa fa-angle-left iplus'></i>";
                            var isChecked = "";

                            var _id = "";
                            if (id !== null && id === _id)
                                isChecked = "checked";
                            var _level = 0;
                            rowData += `<li>
                                        <input id="cbxItem${id}" type="checkbox" class="filled-in" data-id='${id}' ${isChecked} />
                                        <label for="cbxItem${id}">${_title}</label>`;
                            if (subMenu !== undefined && subMenu !== null && subMenu.length > 0) {
                                rowData += fmnCategoryTree.GetSubMenuCategoryList(index, subMenu, _level, _id);
                            }
                            rowData += '</li>';


                        });
                        $('ul#ddlFmnCategory').html(_option_default + rowData);
                        return;
                    }
                })();
            }
        };
    })();
    $.fn.fileManage = function (method, args) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }
        else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }
        else {
            return $.error('Method ' + method + ' does not exist on file manager');
        }
    };
    $.fn.fileManage.defaults = {
        page: 1,
        lblName: null,
        lblMessage: null,
        fileType: 0
    };
    var methods_v1 = (function () {
        // private properties and methods go here
        var uploadFile = async formData => await fetch('/Management/AttachmentFile/Action/Upload', {
            method: 'POST',
            body: formData
        }).then(response => {
            return (response.json());
        });
        var fileLoad = function (page) {
            var categoryId = $("#fileManagerModal #ddlFmnCategory input[type='checkbox']:checked").data('id');
            var fileType = $('#ddlFmnType').val();
            var model = {
                Query: $('#txtQuery').val(),
                Page: page,
                CategoryID: categoryId,
                FileType: fileType,
                IsShared: -1
            };
            AjaxFrom.POST({
                url: '/Management/AttachmentFile/Action/DataList',
                data: model,
                success: function (result) {
                    $('#FileManagerList').html('');
                    $('#Pagination').html('');
                    if (result != null) {
                        if (result.status == 200) {
                            var currentPage = 1;
                            var pagination = result.paging;
                            var totalPage = 0;
                            var pageSize = 0;

                            if (pagination != null) {
                                totalPage = pagination.TotalPage;
                                currentPage = pagination.Page;
                                pageSize = pagination.PageSize;
                                fm._pageIndex = pagination.Page;
                            }
                            var rowData = '';
                            $.each(result.data, function (index, item) {
                                index = index + 1;
                                var id = item.ID;
                                if (id.length > 0)
                                    id = id.trim();
                                // 
                                var _extension = item.Extension;
                                var _name = id + item.Extension;
                                var _imgPath = item.ImagePath;
                                var _caption = SubStringText.SubFileName(_name);
                                var _summary = item.ContentLength + 'k';
                                var _state = false;
                                var _active = '';
                                var _iconCheck = 'fa-circle';
                                // ************************************************************************************************
                                if (fm._ctrlInput != undefined) {
                                    if (fm._ctrlInput.EditorInputVal != "") {
                                        var filename = fm._ctrlInput.EditorInputVal.substring(fm._ctrlInput.EditorInputVal.lastIndexOf('/') + 1);
                                        if (filename != undefined && filename == `${id + item.Extension}`)
                                            _state = true;
                                    }
                                }
                                else {
                                    if (fm.selected.find(x => x == id) != undefined)
                                        _state = true;
                                }
                                // 
                                if (_state) {
                                    _active = 'actived';
                                    _iconCheck = 'fa-check-circle';
                                }
                                // 
                                if (IFile.IsDoccumentFile(_extension) || IFile.IsCompressionFile(_extension))
                                    _imgPath = IFile.ShowIconFile(_extension, "md");
                                // 
                                rowData += `
                                    <div class="col-lg-2 col-md-3 col-sm-3 col-xs-6" id='box${id}'>
                                        <div class='item-image ${_active}' id='item${id}'>
                                            <div class='image-box'><img class="img-responsive img-file" src="${_imgPath}" /></div>
                                            <div class='note-box'><span class='item-caption'>${_caption}</span> <br /></div>
                                            <div class='action-box'><span class='size-info'>${_summary}</span> <i class="far ${_iconCheck}" data-btncheck="true" data-id='${item.ID}'></i>  <i class="far fa-trash-alt icon-trash" data-id='${item.ID}'></i></div>   
                                        </div>
                                    </div>`;
                            });
                            $('#FileManagerList').html(rowData);
                            if (parseInt(totalPage) > 1) {
                                Paging.Pagination("#FileManagerPagination", totalPage, currentPage, fileLoad);
                            }
                        }
                    }
                }
            });
        };
        var applyHandlerFinders = function (fileManage) {
            //
        };
        $(document).on('click', '.file-modal #btnOpenClientFile', function () {
            $('.file-modal #UploadFile').click();
        });
        //
        $(document).on("change", ".file-modal #UploadFile", function (elm) {
            $('#inputFileControl').html('');
            var _file = $(this)[0].files[0];
            var _fileType = $("#ddlFmnType").val();
            if (_fileType == undefined || _fileType == null) {
                Notifization.Error("Vui lòng chọn loại tệp tin");
                return;
            }
            if (_file == undefined || _file == null) {
                Notifization.Error("Vui lòng chọn tệp tin");
                return;
            }
            if (!IFile.IsFile(_file.name)) {
                Notifization.Error("Tệp tin không hợp lệ");
                $(this).val('');
                $('#inputFileControl').html('');
                return;
            }
            $('#inputFileControl').html(SubStringText.SubFileName(_file.name));
        });
        //
        $(document).on('click', '.file-modal #btnUpload', function () {
            var _file = $('#UploadFile')[0].files[0];
            var ddlFmnCategory = $("#fileManagerModal #ddlFmnCategory input[type='checkbox']:checked").data('id');
            console.log(ddlFmnCategory);
            //
            $('#lblFmnUpload').html('');
            if (ddlFmnCategory == undefined || ddlFmnCategory == "") {
                $('#lblFmnUpload').html("Vui lòng chọn danh mục");
                return;
            }
            //
            if (_file == '' || _file == undefined) {
                $('#lblFmnUpload').html("Vui lòng chọn tệp tin");
                return;
            }

            if (!IFile.IsFile(_file.name)) {
                $('#lblFmnUpload').html("Tệp tin không hợp lệ");
                $('#UploadFile').val('');
                $('#inputFileControl').html('');
                return;
            }
            // 
            (async () => {
                'use strict';
                var model = new FormData();
                model.append("DocumentFile", _file);
                model.append("CategoryID", ddlFmnCategory);
                model.append("fileType", _file);
                const uploadResult = await uploadFile(model);
                //
                if (uploadResult != null && uploadResult.status == 200) {
                    Notifization.Success(uploadResult.message);
                    $('#inputFileControl').html('');
                    fileLoad(fm._pageIndex);
                    FData.ResetForm("#FileManagerModal");
                    return;
                }
                Notifization.Error(MessageText.NotService);
                return;
            })();
        });

        $(document).on('click', '.file-modal #btnSearchFile', function () {
            fileLoad(1);
        });
        $(document).on('click', '[data-btncheck="true"]', function () {
            let _id = $(this).data('id');
            let _imgPath = $(`#item${_id} img`).attr("src");
            let _summary = $(`#item${_id} .note-box >.size-info`).html();
            let _caption = $(`#item${_id} .note-box >.item-caption`).html();
            if (_imgPath == undefined || _imgPath == "")
                _imgPath = "/files/upload/2021/4/2a750bbd-4dc2-40d3-ab19-1ce43f6c4419.jpg";
            // 
            // for ckeditor
            let _elm = $(this);
            if (fm._ctrlInput != undefined && fm._ctrlInput != null) {
                if ($(_elm).hasClass('fa-check-circle')) {
                    $('#FileManagerList .item-image').removeClass("actived");
                    $('#FileManagerList .item-image .action-box .fa-check-circle').removeClass('fa-check-circle').addClass('fa-circle');
                    //
                    fm._ctrlInput.EditorInputUrl('', { title: _summary });
                }
                else {
                    // remove all active
                    $('#FileManagerList .item-image').removeClass("actived");
                    $('#FileManagerList .item-image .action-box .fa-check-circle').removeClass('fa-check-circle').addClass('fa-circle');
                    $(_elm).removeClass('fa-circle').addClass('fa-check-circle');
                    $(`#item${_id}`).addClass("actived");
                    //   
                    if (_imgPath != '' && _imgPath != undefined) {
                        fm._ctrlInput.EditorInputUrl(_imgPath, { title: "" });
                        $('[data-dismiss="close"]').click();
                    }
                }
                return;
            }
            //  
            switch (fm.fileType) {
                case fmEnum.fileType.Multi:
                    if ($(_elm).hasClass('fa-check-circle')) {
                        console.log('remove');
                        // remove
                        $(_elm).removeClass('fa-check-circle').addClass('fa-circle');
                        $(`#item${_id}`).removeClass("actived");
                        // 
                        var itemRemove = fm.content.find(`.pre-item-box[data-id="${_id}"]`);
                        if (itemRemove != undefined) {
                            $(itemRemove).remove();
                            // update selected list
                            fm.selected = $.grep(fm.selected, function (value) {
                                return value != _id;
                            });
                        }
                    }
                    else {
                        // add
                        $(_elm).removeClass('fa-circle').addClass('fa-check-circle');
                        $(`#item${_id}`).addClass("actived");
                        // actived   
                        if (fm.selected.find(x => x == _id) == undefined) {
                            var _htmlItem = '';
                            if (fm.isList) {
                                _htmlItem = `<div class='i-list pre-item-box' data-id='${_id}'><img class="img-responsive" src="${_imgPath}" data-id='${_id}' /> <lalel>${_caption} <i class="fas fa-times icon-delete" data-ibtn="true" data-id='#ibox${_id}'></i></lalel></div>`
                            }
                            else {
                                _htmlItem = `<div class="col-lg-2 col-md-2 col-sm-2 col-xs-3 pre-item-box" id='ibox${_id}' data-id='${_id}'>
                                    <div class='item-image' data-actived='false'>
                                        <div class='image-box'>
                                            <img class="img-responsive" src="${_imgPath}" data-id='${_id}' />
                                            <i class="fas fa-times icon-delete" data-ibtn="true" data-id='#ibox${_id}'></i>
                                        </div>
                                        <div class='note-box'>
                                            ${_caption}<br />  
                                        </div>
                                    </div>
                                </div>`;
                            }
                            $(fm.content).append(_htmlItem);
                            fm.selected.push(_id);
                        }
                        else {
                            console.log('trung:' + _id);
                        }
                    }
                    break;
                case fmEnum.fileType.Alone:
                    if ($(_elm).hasClass('fa-check-circle')) {
                        $('#FileManagerList .item-image').removeClass("actived");
                        $('#FileManagerList .item-image .action-box .fa-check-circle').removeClass('fa-check-circle').addClass('fa-circle');
                    }
                    else {
                        // remove all active
                        $('#FileManagerList .item-image').removeClass("actived");
                        $('#FileManagerList .item-image .action-box .fa-check-circle').removeClass('fa-check-circle').addClass('fa-circle');
                        $(_elm).removeClass('fa-circle').addClass('fa-check-circle');
                        $(`#item${_id}`).addClass("actived");
                        //  
                    }
                    // 
                    fm.content.html(`<img class="img-responsive" src="${_imgPath}" data-id='${_id}' />`);
                    break;
                default:
            }
        });
        $(document).on('click', '.file-preview .icon-delete', function () {
            $(this).closest(".pre-item-box").remove();
            var _imgList = $('.pre-item-box');
            $("#lblFileTotal").html(`...${_imgList.length} tệp tin`);
        });

        // category change
        $(document).on("change", ".file-modal #ddlFmnCategory", function (elm) {
            $('#lblFmnUpload').html('');
            fileLoad(1);
        });
        return {
            // public methods
            hide: function (instant) {
                return this;
            },
            show: function () {
                $("body").addClass("modal-open");
                $("body .file-modal").addClass("in");
                return this;
            },
            destroy: function () {
                return this.each(function () {
                    var $this = $(this);
                });
            },
            init: function (model) {
                //   
                var isList = false;
                if (model.isList != undefined && model.isList)
                    isList = true;
                //
                applyHandlerFinders($(this));
                fm.fileType = model.fileType;
                fm.selected = model.selected;
                fm.isList = isList;
                fm.content = model.content;
                fm._ctrlInput = model.ctrlInput;
                $('.file-modal #ddlFmnCategory').change();
                methods_v1.show();

            }
        };

    })();

    $.fn.fileFinder = function (_method1, args) {
        if (methods_v1[_method1]) {
            return methods_v1[_method1].apply(this, Array.prototype.slice.call(arguments, 1));
        }
        else if (typeof _method1 === 'object' || !_method1) {
            return methods_v1.init.apply(this, arguments);
        }
        else {
            return $.error('Method ' + _method1 + ' does not exist on file manager');
        }
    };

    $.fn.fileFinder.defaults = {
        page: 1,
        lblName: null,
        lblMessage: null,
        fileType: 0
    };
})(jQuery, window);


//channelsRaw.forEach(async (data) => {
//    let dataParsed = ResponseParser.parseLine(data);

//    let method = new ChannelInfoMethod(this.query);
//    let channel = await method.run(dataParsed.cid);

//    channels.push(channel);
//}); 
$(document).on('change', '#fileManagerModal #ddlFmnCategory input[type="checkbox"]', function () {
    $('#fileManagerModal #ddlFmnCategory input[type="checkbox"]').not(this).prop('checked', false);
});