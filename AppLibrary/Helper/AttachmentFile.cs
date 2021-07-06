using Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using WebCore.Entities;
using WebCore.Model.Entities;
using WebCore.Services;
using Helper.Page;
using System.Drawing;
using System.IO.Compression;
using SelectPdf;
using OfficeOpenXml;

namespace Helper.File
{
    public class AttachmentFile
    {
        public static string SaveFile(HttpPostedFileBase file, DateTime dateTime, string siteId, string userId, bool isImage = false, int imgWidth = 0, int imgHeight = 0, IDbTransaction dbTransaction = null, IDbConnection dbConnection = null)
        {
            try
            {
                if (file == null || file.ContentLength == 0)
                    return string.Empty;
                string fileName = file.FileName;
                if (string.IsNullOrEmpty(fileName))
                    return "Tệp tin không hợp lệ";
                //Check allowed file
                string extension = System.IO.Path.GetExtension(fileName);
                if (isImage)
                {
                    if (!Validate.TestImageFile(extension))
                        return "Tệp tin  không hợp lệ";
                }
                else
                {
                    if (!IsAllowedFile(extension))
                        return "Tệp tin  không hợp lệ";
                }
                if (file.ContentLength / 1048576 > 5)
                    return "Tệp tin phải < 5M";
                //Save file info to database
                AttachmentService attachmentService = new AttachmentService(dbConnection);
                var guid = attachmentService.Create<string>(new Attachment()
                {
                    Title = null,
                    Extension = extension,
                    ContentLength = file.ContentLength,
                    ContentType = file.ContentType.ToLower(),
                    LanguageID = Helper.Language.LanguagePage.GetLanguageCode,

                }, transaction: dbTransaction);
                //Save file to system
                var year = dateTime.Year;
                var month = dateTime.Month;
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
                {
                    file.SaveAs(fileFolderPath + guid + extension);
                }
                return guid;

            }
            catch (Exception ex)
            {
                return "Lỗi tải tệp tin" + ex;
            }
        }

        public static string DeleteFile(string url, IDbTransaction dbTransaction = null, IDbConnection dbConnection = null)
        {
            string fileFolderPath = HttpContext.Current.Server.MapPath(url);
            if (System.IO.File.Exists(fileFolderPath))
                System.IO.File.Delete(fileFolderPath);
            //
            return string.Empty;
        }

        public static string DeleteMultiFile(List<Attachment> idList, IDbTransaction transaction = null, IDbConnection connection = null)
        {
            if (connection == null)
                connection = DbConnect.Connection.CMS;
            //
            AttachmentService attachmentService = new AttachmentService(connection);
            foreach (var item in idList)
            {
                string guid = item.ID;
                if (!Validate.TestGuid(guid))
                    continue;
                //
                DateTime date = item.CreatedDate;
                int year = date.Year;
                int month = date.Month;
                string fileFolderPath = HttpContext.Current.Server.MapPath("~/files/upload/" + year + "/" + month + "/" + guid + item.Extension);
                if (System.IO.File.Exists(fileFolderPath))
                    System.IO.File.Delete(fileFolderPath);
                //
            } 
            attachmentService.Execute("DELETE AttachmentIngredient WHERE FileID IN  ('" + String.Join("','", idList) + "')", transaction: transaction);
            attachmentService.Execute("DELETE Attachment WHERE ID IN  ('" + String.Join("','", idList) + "')", transaction: transaction);
            return string.Empty;
        }
        public static string DeleteMultiFile(List<Attachment> idList, string categoryId, IDbTransaction transaction = null, IDbConnection connection = null)
        {
            AttachmentService attachmentService = new AttachmentService(connection);
            attachmentService.Execute("DELETE Attachment WHERE CategoryID = @CategoryID", new { CategoryID = categoryId }, transaction: transaction);
            foreach (var item in idList)
            {
                string guid = item.ID;
                if (!Validate.TestGuid(guid))
                    continue;
                //
                attachmentService.Execute("DELETE AttachmentIngredient WHERE FileID = @FileID", new { FileID = guid }, transaction: transaction);
                DateTime date = item.CreatedDate;
                int year = date.Year;
                int month = date.Month;
                string fileFolderPath = HttpContext.Current.Server.MapPath("~/files/upload/" + year + "/" + month + "/" + guid + item.Extension);
                if (System.IO.File.Exists(fileFolderPath))
                    System.IO.File.Delete(fileFolderPath);
                //
            }
            return string.Empty;
        }
        public static List<AttachmentResultFilesModel> SaveFiles(IEnumerable<HttpPostedFileBase> files, DateTime dateTime, string siteId, string userId, bool isImage = false, int width = 0, int height = 0, IDbTransaction dbTransaction = null, IDbConnection dbConnection = null)
        {
            if (files.Count() > 0)
            {
                double totalSize = 0.0;
                // valid file
                foreach (var file in files)
                {
                    if (file == null || file.ContentLength == 0)
                        return new List<AttachmentResultFilesModel> {
                            new AttachmentResultFilesModel(400,"Dữ liệu không hợp lệ",null)
                        };


                    //
                    string fileName = file.FileName;
                    if (string.IsNullOrEmpty(fileName))
                        return new List<AttachmentResultFilesModel> {
                            new AttachmentResultFilesModel(404,"Tên tệp tin không hợp lệ",null)
                        };

                    //Check allowed file
                    if (!Validate.TestImageFile(fileName))
                        return new List<AttachmentResultFilesModel> {
                            new AttachmentResultFilesModel(400,"Tệp tin không hợp lệ",null)
                        };
                    totalSize += file.ContentLength;
                    //Check file size limit
                    if (totalSize / 1048576 > 10)
                        return new List<AttachmentResultFilesModel> {
                            new AttachmentResultFilesModel(400,"Tệp tin phải < 1M",null)
                        };
                }
            }

            List<string> lstFile = new List<string>();
            //Save file info to database
            var attachmentService = new AttachmentService(dbConnection);
            foreach (var file in files)
            {
                string fileName = file.FileName;
                string extension = System.IO.Path.GetExtension(fileName);
                string guid = attachmentService.Create<string>(new Attachment()
                {
                    Title = null,
                    Extension = extension,
                    ContentLength = file.ContentLength,
                    ContentType = file.ContentType.ToLower(),
                    LanguageID = Helper.Language.LanguagePage.GetLanguageCode,
                }, transaction: dbTransaction);

                //Save file to system
                var year = dateTime.Year;
                var month = dateTime.Month;
                string fileFolderPath = HttpContext.Current.Server.MapPath("~/files/upload/" + year + "/" + month + "/");
                if (!System.IO.Directory.Exists(fileFolderPath))
                    System.IO.Directory.CreateDirectory(fileFolderPath);

                file.SaveAs(fileFolderPath + guid + extension);
                lstFile.Add(guid);
            }
            if (lstFile.Count() == 0)
                return new List<AttachmentResultFilesModel> {
                    new AttachmentResultFilesModel(404,"ok",null)
                };

            return new List<AttachmentResultFilesModel> {
                    new AttachmentResultFilesModel(200,"ok",lstFile)
                };
        }
        //
        public static string SaveFile(string savePath, string id, byte[] _file, DateTime dateTime, string fileName, string siteId, string userId, IDbTransaction dbTransaction = null, IDbConnection dbConnection = null)
        {
            //Save file to system
            var year = dateTime.Year;
            var month = dateTime.Month;
            if (string.IsNullOrWhiteSpace(savePath))
                return string.Empty;
            //
            string fileFolderPath = HttpContext.Current.Server.MapPath(savePath + year + "/" + month + "/");
            if (!System.IO.Directory.Exists(fileFolderPath))
                System.IO.Directory.CreateDirectory(fileFolderPath);
            // 
            AttachmentService attachmentService = new AttachmentService(dbConnection);
            Attachment attachment = attachmentService.GetAlls(m => m.ID == id, transaction: dbTransaction).FirstOrDefault();
            string pathFile = fileFolderPath + "/" + fileName;
            System.IO.File.WriteAllBytes(pathFile, _file);
            FileInfo fileInfo = new FileInfo(pathFile);
            attachment.ContentLength = fileInfo.Length;
            attachmentService.Update(attachment, transaction: dbTransaction);
            return id;
        }

        public static string SaveImageFile(string id, Image _inpBitmap, DateTime dateTime, string imgName, System.Drawing.Imaging.ImageFormat imgFormat, string siteId, string userId, IDbTransaction dbTransaction = null, IDbConnection dbConnection = null)
        {
            //Save file to system
            var year = dateTime.Year;
            var month = dateTime.Month;
            string fileFolderPath = HttpContext.Current.Server.MapPath("~/Files/Upload/Image/");
            if (!System.IO.Directory.Exists(fileFolderPath))
                System.IO.Directory.CreateDirectory(fileFolderPath);
            //  
            AttachmentService attachmentService = new AttachmentService(dbConnection);
            Attachment attachment = attachmentService.GetAlls(m => m.ID == id, transaction: dbTransaction).FirstOrDefault();
            string fullPath = fileFolderPath + imgName;
            _inpBitmap.Save(fullPath, imgFormat);
            FileInfo file = new FileInfo(fullPath);
            attachment.ContentLength = file.Length;
            attachmentService.Update(attachment, transaction: dbTransaction);
            return id;
        }
        //
        public static string ZipDirectoryFile(string inputDirectory, string zipPath)
        {
            ZipFile.CreateFromDirectory(inputDirectory, zipPath);
            return zipPath;
        }

        public static string AttachmentPDF(string fileName, string inUrlPage, string outFolder = "~/Files/Export/")
        {
            //// Validate the Model is correct and contains valid data
            //// Generate your report output based on the model parameters
            //// This can be an Excel, PDF, Word file - whatever you need.

            //// As an example lets assume we've generated an EPPlus ExcelPackage

            //ExcelPackage workbook = new ExcelPackage();
            //// Do something to populate your workbook
            //ExcelWorksheet ws = workbook.Workbook.Worksheets.Add("testsheet");
            //// Generate a new unique identifier against which the file can be stored
            //string handle = Guid.NewGuid().ToString();

            //using (MemoryStream memoryStream = new MemoryStream())
            //{
            //    ws.Cells["B1"].Value = "Number of Used Agencies";
            //    ws.Cells["C1"].Value = "Active Agencies";
            //    ws.Cells["D1"].Value = "Inactive Agencies";
            //    ws.Cells["E1"].Value = "Total Hours Volunteered";
            //    ws.Cells["B1:E1"].Style.Font.Bold = true;
            //    workbook.SaveAs(memoryStream);
            //    memoryStream.Position = 0;
            //    // Note we are returning a filename as well as the handle

            //    return new DownLoadTest
            //    {
            //        GuidID = handle,
            //        FileName = "TestReportOutput.xlsx",
            //        DataFile = memoryStream.ToArray(),


            //    };
            //} 
            string domainName = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            var converter = new HtmlToPdf();
            PdfDocument doc = converter.ConvertUrl(domainName + inUrlPage);
            string fileFolderPath = HttpContext.Current.Server.MapPath(outFolder);
            string pathFile = fileFolderPath + fileName + ".pdf";
            if (System.IO.File.Exists(pathFile))
            {
                System.IO.File.Delete(pathFile);
            }
            //
            doc.Save(pathFile);
            doc.Close();
            return pathFile;
        }
        public static string AttachmentExls(string _fileName, ExcelPackage excel, string outFolder = "~/Files/Export/")
        {
            string fileFolderPath = HttpContext.Current.Server.MapPath(outFolder);
            string fileName = $"Bao-cao-{Helper.TimeData.TimeHelper.UtcDateTime:yyyy-MM-dd-HH-mm-ss}-{_fileName}.xlsx";
            string pathFile = fileFolderPath + fileName;
            if (System.IO.File.Exists(pathFile))
            {
                System.IO.File.Delete(pathFile);
            }
            //
            FileInfo excelFile = new FileInfo(pathFile);
            excel.SaveAs(excelFile);
            excel.Dispose();
            return outFolder + fileName;
        }

        private List<string> GetImagesInHTMLString(string htmlString)
        {
            List<string> images = new List<string>();
            string pattern = @"<(img)\b[^>]*>";

            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(htmlString);

            for (int i = 0, l = matches.Count; i < l; i++)
            {
                images.Add(matches[i].Value);
            }

            return images;
        }
        public static bool IsAllowedFile(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return false;

            extension = extension.ToLower();
            return AllowedExtensions.Contains(extension);
        }
        public static IList<string> AllowedExtensions = new List<string>
        {
            ".png", ".jpg", ".jpeg", ".svg", ".mp3", ".mp4", ".pdf", ".xls", ".xlsx", ".doc", ".docx", ".rar", ".zip", ".txt", ".ppt", ".pptx"
        };

        //* Get file *****************************************************************************************************************************************************************************************************************************************************************************************
        public static string GetFile(string id, string extension, DateTime createdDate)
        {
            var year = createdDate.Year;
            var month = createdDate.Month;
            return "/files/upload/" + year + "/" + month + "/" + id + "" + extension;
        }
        public static string GetFile(string guid, bool isImage)
        {
            if (string.IsNullOrWhiteSpace(guid))
                return Helper.Page.Default.FileNoImange;

            //
            var file = Directory.GetFiles(HttpContext.Current.Server.MapPath("~/files/upload/"), guid + ".*", System.IO.SearchOption.AllDirectories).FirstOrDefault();
            if (file != null)
            {
                string extension = System.IO.Path.GetExtension(file.Replace("\\", "/").Split(new string[] { "/files/upload/" }, StringSplitOptions.None)[1]);
                if (Helper.Page.Validate.ImgExtensions.Contains(extension))
                    return $"/files/upload/{file.Replace("\\", "/").Split(new string[] { "/files/upload/" }, StringSplitOptions.None)[1]}";
                //
                return AttachmentFile.GetAttmFile(extension);
            }

            return Helper.Page.Default.FileNoImange;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="type">type: xs |  md | lg</param>
        /// <returns></returns>
        public static string GetAttmFile(string extension = null, string type = "xs")
        {
            string result = $"data-error-{type}.png";
            switch (extension)
            {
                case ".pdf":
                    result = $"data-pdf-{type}.png";
                    break;
                case ".xls":
                    result = $"data-xls-{type}.png";
                    break;
                case ".xlsx":
                    result = $"data-xlsx-{type}.png";
                    break;
                case ".doc":
                    result = $"data-doc-{type}.png";
                    break;
                case ".docx":
                    result = $"data-doc-{type}.png";
                    break;
                case ".rar":
                    result = $"data-rar-{type}.png";
                    break;
                case ".zip":
                    result = $"data-zip-{type}.png";
                    break;
                default:
                    result = $"data-error-xs-{type}.png";
                    break;
            }
            return "/files/icon/" + result;
        }
        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

    }
    public class AttachmentResultFilesModel
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<string> FileList { get; set; }
        public AttachmentResultFilesModel(int status, string message, List<string> fileList)
        {
            Status = status;
            Message = message;
            FileList = fileList;
        }
    }


}
