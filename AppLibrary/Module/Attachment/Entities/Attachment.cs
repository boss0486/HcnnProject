using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using System;
using System.Collections.Generic;
using System.Web;
using WebCore.Model.Entities;

namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("Attachment")]
    public class Attachment : WEBModel
    {
        public Attachment()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string CategoryID { get; set; }
        public string Title { get; set; }
        public string Extension { get; set; }
        public float ContentLength { get; set; }
        public string ContentType { get; set; }
        public string Permissions { get; set; }
        public int IsShared { get; set; }
    }

    public class ViewAttachment : WEBModelResult
    {
        public string ID { get; set; }
        public string CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Title { get; set; }
        public string Extension { get; set; }
        public float ContentLength { get; set; }
        public string ContentType { get; set; }
        public bool IsImage { get; set; }
        [NotMapped]
        public string ImagePath => Helper.File.AttachmentFile.GetFile(ID, IsImage);
        public int IsShared { get; set; }

    }

    public class AttachmentIDModel
    {
        public string ID { get; set; }
    }
    public class AttachmentSearchModel : SearchModel
    {
        public string CategoryID { get; set; }
        public int FileType { get; set; }
        public int IsShared { get; set; }
    }
    public class AttachmentUploadModel
    {
        public HttpPostedFileBase DocumentFile { get; set; }
        public string CategoryID { get; set; }
        public int FileType { get; set; }
        public string FileName { get; set; }
        public string UserList { get; set; }
        public string Permissions { get; set; }
        public bool sharedAll { get; set; }
    }
    public class AttachmentUpdateModel: AttachmentUploadModel
    {
        public string ID { get; set; }
    }
    public class AttachmentSaveSettingModel
    {
        public HttpPostedFileBase File { get; set; }
        public int ImgWidth { get; set; }
        public int ImgHeight { get; set; }
    }
    public class AttachmentSaveModel
    {
        public string CategoryID { get; set; }
        public HttpPostedFileBase File { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ImgWidth { get; set; }
        public int ImgHeight { get; set; }
    }

    public class AttachmentPermisssionOptionModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
    }
}