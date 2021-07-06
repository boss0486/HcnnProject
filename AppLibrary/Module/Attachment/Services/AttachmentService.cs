using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using Helper;
using Helper.File;
using Helper.Page;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WebCore.ENM;
using WebCore.Entities;
using WebCore.Model.Entities;

namespace WebCore.Services
{
    public interface IAttachmentService : IEntityService<Attachment> { }
    public partial class AttachmentService : EntityService<Attachment>, IAttachmentService
    {
        public AttachmentService() : base() { }
        public AttachmentService(System.Data.IDbConnection db) : base(db) { }


        //##############################################################################################################################################################################################################################################################
        public ActionResult DataList(AttachmentSearchModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            int page = model.Page;
            string query = model.Query;
            string categoryId = model.CategoryID;
            int isShared = model.IsShared;

            int fileType = model.FileType;
            if (string.IsNullOrWhiteSpace(query))
                query = "";
            //
            string whereCondition = string.Empty;
            SearchResult searchResult = WebCore.Model.Services.ModelService.SearchDefault(new SearchModel
            {
                Query = model.Query,
                TimeExpress = model.TimeExpress,
                Status = -1,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Page = model.Page,
                TimeZoneLocal = model.TimeZoneLocal
            }, ectColumn: "a.");
            if (searchResult != null)
            {
                if (searchResult.Status == 1)
                    whereCondition = searchResult.Message;
                else
                    return Notifization.Invalid(searchResult.Message);
            }

            if (!string.IsNullOrWhiteSpace(categoryId))
                whereCondition += " AND a.CategoryID = @CategoryID";
            //

            if (model.IsShared >= 0 && model.IsShared <= 3)
                whereCondition += " AND a.IsShared = @IsShared ";

            // 
            switch (fileType)
            {
                case 1:
                    whereCondition += $" AND a.Extension IN ('.gif','.png','.bmp','.jpeg','.jpg','.ico')";
                    break;
                case 2:
                    whereCondition += $" AND a.Extension IN ('.doc','.docx','.xls','.xlsx','.pdf')";
                    break;
                case 3:
                    whereCondition += $" AND a.Extension IN ('.rar','.zip')";
                    break;
                default:
                    break;
            }
            //gif, png, bmp, jpeg, jpg, ico | doc, docx, xls, xlsx, pdf | rar, zip

            string langID = Helper.Current.UserLogin.LanguageID;
            whereCondition += $@" AND a.SiteID = @SiteID AND a.CreatedBy IN
            (CASE WHEN a.IsShared = @IsShared THEN
              (select ash.UserID from AttachmentShared as ash where ash.UserID = a.CreatedBy)
             ELSE
                @CreatedBy
            END) ";

            string sqlQuery = $@"SELECT a.*, c.Title as 'CategoryName' FROM Attachment as a LEFT JOIN AttachmentCategory as c ON a.CategoryID = c.ID WHERE dbo.Uni2NONE(a.Title) LIKE N'%'+ @Query +'%' {whereCondition} ORDER BY c.Title, a.CreatedDate DESC";
            // 
            var dtList = _connection.Query<ViewAttachment>(sqlQuery, new
            {
                Query = Helper.Page.Library.FormatNameToUni2NONE(query),
                CategoryID = categoryId,
                SiteID = Helper.Current.UserLogin.SiteID,
                IsShared = model.IsShared,
                CreatedBy = Helper.Current.UserLogin.IdentifierID
            }).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound + sqlQuery);
            //
            var result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
            if (result.Count <= 0 && page > 1)
            {
                page -= 1;
                result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
            }
            if (result.Count <= 0)
                return Notifization.NotFound(MessageText.NotFound);

            Helper.Pagination.PagingModel pagingModel = new Helper.Pagination.PagingModel
            {
                PageSize = Helper.Pagination.Paging.PAGESIZE,
                Total = dtList.Count,
                Page = page
            };
            //
            return Notifization.Data(MessageText.Success, data: result, role: RoleActionSettingService.RoleListForUser(), paging: pagingModel);
        }

        public ActionResult Delete(string id)
        {
            _connection.Open();
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.NotFound();
            //
            id = id.ToLower();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    AttachmentService attachmentService = new AttachmentService(_connection);
                    Attachment attachment = attachmentService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    if (attachment == null)
                        return Notifization.Invalid(MessageText.Invalid);
                    //
                    attachmentService.Execute("DELETE AttachmentShared WHERE FileID = @FileID", new { FileID = id }, transaction: _transaction);
                    attachmentService.Remove(id, transaction: _transaction);
                    DeleteFile($"{attachment.ID}{attachment.Extension}", attachment.CreatedDate);
                    // remover seo
                    _transaction.Commit();
                    return Notifization.Success(MessageText.DeleteSuccess);
                }
                catch
                {
                    _transaction.Rollback();
                    return Notifization.NotService;
                }
            }
        }

        public List<ViewAttachment> AttachmentrListByForID(string _forId)
        {
            try
            {
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT  att.* FROM AttachmentIngredient atti LEFT JOIN  Attachment as att ON  att. ID = atti.FileID WHERE atti.ForID = @ForID";
                var dtList = _connection.Query<ViewAttachment>(sqlQuery, new { ForID = _forId }).ToList();
                if (dtList.Count == 0)
                    return new List<ViewAttachment>();
                return dtList;
            }
            catch
            {
                return new List<ViewAttachment>();
            }
        }

        public ActionResult Create(AttachmentUploadModel model)
        {
            HttpPostedFileBase file = model.DocumentFile;
            string categoryId = model.CategoryID;
            string fileName = model.FileName;
            string userList = model.UserList;
            string permissions = model.Permissions;
            bool sharedAll = model.sharedAll;
            int isShared = (int)AttachmentEnum.IsShared.None;
            if (file == null)
                return Notifization.Invalid(MessageText.Invalid);
            // 
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = file.FileName;
            //
            string extension = System.IO.Path.GetExtension(fileName);
            DateTime dateTime = Helper.TimeData.TimeHelper.UtcDateTime;
            //
            if (string.IsNullOrWhiteSpace(categoryId))
                return Notifization.Invalid("Vui lòng chọn nhóm tệp tin");
            //
            if (file == null || file.ContentLength == 0)
                return Notifization.Invalid(MessageText.Invalid);
            //
            if (string.IsNullOrEmpty(fileName))
                return Notifization.Invalid(MessageText.Invalid);
            //Check allowed file
            if (!Validate.TestImageFile(extension))
                return Notifization.Invalid("Lỗi định dạng tệp tin");
            //
            if (file.ContentLength / 1024000 > 1)
                return Notifization.Invalid("Tệp tin phải < 1M" + file.ContentLength);
            //
            if (sharedAll)
                isShared = (int)AttachmentEnum.IsShared.Everyone;
            else if (!string.IsNullOrWhiteSpace(userList) && userList.Split(',').Count() > 0)
                isShared = (int)AttachmentEnum.IsShared.SomeOne;
            //
            //Save file info to database
            AttachmentService attachmentService = new AttachmentService(_connection);
            int imgWidth = 0;
            int imgHeight = 0;
            var year = dateTime.Year;
            var month = dateTime.Month;
            var guid = attachmentService.Create<string>(new Attachment()
            {
                CategoryID = categoryId,
                Title = fileName.Replace(extension, ""),
                Permissions = permissions,
                Extension = extension,
                ContentLength = file.ContentLength,
                ContentType = file.ContentType.ToLower(),
                CreatedDate = dateTime,
                IsShared = isShared
            });
            // 
            AttachmentSharedService _attachmentSharedService = new AttachmentSharedService(_connection);
            if (!string.IsNullOrWhiteSpace(userList))
            {
                string[] arrUser = userList.Split(',');
                if (arrUser.Count() > 0)
                {
                    foreach (string item in arrUser)
                    {
                        if (string.IsNullOrWhiteSpace(item))
                            continue;
                        //
                        _attachmentSharedService.Create<string>(new AttachmentShared
                        {
                            FileID = guid,
                            UserID = item
                        });
                    }
                }
            }
            //Save file to system 
            string fileFolderPath = HttpContext.Current.Server.MapPath("~/Files/Upload/" + year + "/" + month + "/");
            if (!System.IO.Directory.Exists(fileFolderPath))
                System.IO.Directory.CreateDirectory(fileFolderPath);
            //
            if (imgWidth > 0)
            {
                WebImage webImage = new WebImage(file.InputStream);
                double imageWidth = webImage.Width;

                if (imgHeight == 0)
                {
                    int imageHeight = webImage.Height;
                    imgHeight = (int)(imgWidth * 1.0 * imageHeight / imageWidth);
                }

                webImage.Resize(imgWidth, imgHeight);
                webImage.Save(fileFolderPath + guid + extension);
            }
            else
                file.SaveAs(fileFolderPath + guid + extension);
            //
            if (string.IsNullOrWhiteSpace(guid))
                return Notifization.Invalid(MessageText.Invalid);
            //
            return Notifization.Success(MessageText.Success);
        }

        public ActionResult Update(AttachmentUpdateModel model)
        {
            using (var _connectDb = DbConnect.Connection.CMS)
            {
                _connectDb.Open();
                using (var _transaction = _connectDb.BeginTransaction())
                {
                    try
                    {
                        HttpPostedFileBase file = model.DocumentFile;
                        string id = model.ID;
                        string categoryId = model.CategoryID;
                        string fileName = model.FileName;
                        string userList = model.UserList;
                        string permissions = model.Permissions;
                        float contentLength = 0;
                        string contentType = string.Empty;
                        bool sharedAll = model.sharedAll;
                        int isShared = (int)AttachmentEnum.IsShared.None;
                        //
                        if (!string.IsNullOrWhiteSpace(fileName))
                        {
                            if (!Helper.Page.Validate.TestText(fileName))
                                return Notifization.Invalid("Tên tệp tin không hợp lệ");
                        }

                        DateTime dateTime = Helper.TimeData.TimeHelper.UtcDateTime;
                        //
                        if (string.IsNullOrWhiteSpace(categoryId))
                            return Notifization.Invalid("Vui lòng chọn nhóm tệp tin");
                        //
                        AttachmentService attachmentService = new AttachmentService(_connectDb);
                        Attachment attachment = attachmentService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                        if (attachment == null)
                            return Notifization.Invalid(MessageText.Invalid);
                        //
                        if (string.IsNullOrWhiteSpace(fileName))
                            fileName = attachment.Title;
                        //
                        contentLength = attachment.ContentLength;
                        contentType = attachment.ContentType;
                        isShared = attachment.IsShared;
                        string extension = attachment.Extension;
                        if (file != null)
                        {
                            extension = System.IO.Path.GetExtension(file.FileName);
                            contentLength = file.ContentLength;
                            contentType = file.ContentType;
                            if (string.IsNullOrWhiteSpace(fileName))
                                fileName = file.FileName;
                            // 
                            if (!Validate.TestImageFile(extension))
                                return Notifization.Invalid("Lỗi định dạng tệp tin");
                            //
                            if (file.ContentLength / 1024000 > 1)
                                return Notifization.Invalid("Tệp tin phải < 1M" + file.ContentLength);
                            //
                        }
                        if (sharedAll)
                            isShared = (int)AttachmentEnum.IsShared.Everyone;
                        else if (!string.IsNullOrWhiteSpace(userList) && userList.Split(',').Count() > 0)
                            isShared = (int)AttachmentEnum.IsShared.SomeOne;
                        //
                        //Save file info to database
                        int imgWidth = 0;
                        int imgHeight = 0;
                        var year = dateTime.Year;
                        var month = dateTime.Month;
                        //
                        attachment.CategoryID = categoryId;
                        attachment.Title = fileName.Replace(extension, "");
                        attachment.Permissions = permissions;
                        attachment.Extension = extension;
                        attachment.ContentLength = contentLength;
                        attachment.ContentType = contentType;
                        attachment.IsShared = isShared;
                        attachmentService.Update(attachment, transaction: _transaction);
                        AttachmentSharedService _attachmentSharedService = new AttachmentSharedService(_connectDb);
                        if (sharedAll)
                        {
                            _attachmentSharedService.Execute("DELETE AttachmentShared WHERE FileID = @FileID", new { FileID = id }, transaction: _transaction);
                        }
                        else
                        {
                            List<string> fileModels = new List<string>();
                            if (!string.IsNullOrWhiteSpace(userList))
                                fileModels = userList.Split(',').ToList();
                            //
                            List<string> attachmentDb = _attachmentSharedService.GetAlls(m => m.FileID == id, transaction: _transaction).Select(m => m.UserID).ToList();
                            List<string> userNews = fileModels.Except<string>(attachmentDb).ToList();
                            List<string> userDeletes = attachmentDb.Except<string>(fileModels).ToList();
                            // add   
                            if (userNews.Count > 0)
                            {
                                foreach (var item in userNews)
                                {
                                    if (string.IsNullOrWhiteSpace(item))
                                        continue;
                                    //
                                    _attachmentSharedService.Create<string>(new AttachmentShared
                                    {
                                        FileID = id,
                                        UserID = item
                                    }, transaction: _transaction);

                                }
                            }
                            // delete  
                            _attachmentSharedService.Execute("DELETE AttachmentShared WHERE UserID IN ('" + String.Join("','", userDeletes) + "')", transaction: _transaction);
                        }


                        if (file != null)
                        {
                            //Save file to system 
                            string fileFolderPath = HttpContext.Current.Server.MapPath("~/Files/Upload/" + year + "/" + month + "/");
                            if (!System.IO.Directory.Exists(fileFolderPath))
                                System.IO.Directory.CreateDirectory(fileFolderPath);
                            //
                            if (imgWidth > 0)
                            {
                                WebImage webImage = new WebImage(file.InputStream);
                                double imageWidth = webImage.Width;

                                if (imgHeight == 0)
                                {
                                    int imageHeight = webImage.Height;
                                    imgHeight = (int)(imgWidth * 1.0 * imageHeight / imageWidth);
                                }

                                webImage.Resize(imgWidth, imgHeight);
                                webImage.Save(fileFolderPath + id + extension);
                            }
                            else
                                file.SaveAs(fileFolderPath + id + extension);
                        }
                        // 
                        _transaction.Commit();
                        return Notifization.Success(MessageText.UpdateSuccess);
                    }
                    catch (Exception ex)
                    {
                        _transaction.Rollback();
                        return Notifization.NotService;
                    }
                }
            }
        }

        public string DeleteFile(string fileName, DateTime dateTime)
        {
            string filePath = HttpContext.Current.Server.MapPath($"~/files/upload/{dateTime.Year}/{dateTime.Month}/{fileName}");
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
            //
            return "ok";
        }

        //##############################################################################################################################################################################################################################################################
        public Attachment GetAttachmentByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string query = string.Empty;
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = $@"SELECT TOP (1) * FROM Attachment WHERE ID = @Query";
            return _connection.Query<Attachment>(sqlQuery, new { Query = id }).FirstOrDefault();
        }

        public List<AttachmentPermisssionOptionModel> DataOption(string langID)
        {
            List<AttachmentPermisssionOptionModel> _attachmentOptionModels = new List<AttachmentPermisssionOptionModel>
            {
                new AttachmentPermisssionOptionModel
                {
                    ID = 1,
                    Title = "Xem"
                },
                new AttachmentPermisssionOptionModel
                {
                    ID = 2,
                    Title = "Sửa"
                },
                new AttachmentPermisssionOptionModel
                {
                    ID = 3,
                    Title = "Xóa"
                }
            };
            return _attachmentOptionModels;

        }
        public static string DDLAttachmentPermission(string fileId)
        {
            string result = string.Empty;
            using (var service = new AttachmentService())
            {
                List<AttachmentPermisssionOptionModel> dtList = service.DataOption("");

                string filePermission = service.GetAlls(m => m.ID == fileId).Select(m => m.Permissions).FirstOrDefault();
                List<string> permissionList = new List<string>();
                if (!string.IsNullOrWhiteSpace(filePermission))
                {
                    permissionList = filePermission.Split(',').ToList();
                }

                if (dtList.Count > 0)
                {
                    foreach (var item in dtList)
                    {
                        string select = string.Empty;
                        if (permissionList.Contains(Convert.ToString(item.ID)))
                            select = "selected";
                        //
                        result += "<option value='" + item.ID + "'" + select + ">" + item.Title + "</option>";
                    }
                }
                return result;
            }
        }


        //##############################################################################################################################################################################################################################################################

    }
}