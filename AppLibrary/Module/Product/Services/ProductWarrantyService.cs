using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using System;
using Dapper;
using System.Linq;
using PagedList;
using System.Web.Mvc;
using System.Collections.Generic;
using Helper;
using System.Web;
using WebCore.Entities;
using WebCore.Model.Entities;
using Helper.Page;

namespace WebCore.Services
{
    public interface IProductWarrantyService : IEntityService<ProductWarranty> { }
    public class ProductWarrantyService : EntityService<ProductWarranty>, IProductWarrantyService
    {
        public ProductWarrantyService() : base() { }
        public ProductWarrantyService(System.Data.IDbConnection db) : base(db) { }
        //##############################################################################################################################################################################################################################################################
        public ActionResult DataList(SearchModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            int page = model.Page;
            string query = model.Query;
            if (string.IsNullOrWhiteSpace(query))
                query = "";
            //
            string whereCondition = string.Empty;
            //
            SearchResult searchResult = WebCore.Model.Services.ModelService.SearchDefault(new SearchModel
            {
                Query = model.Query,
                TimeExpress = model.TimeExpress,
                Status = model.Status,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Page = model.Page,
                AreaID = model.AreaID,
                TimeZoneLocal = model.TimeZoneLocal
            });
            if (searchResult != null)
            {
                if (searchResult.Status == 1)
                    whereCondition = searchResult.Message;
                else
                    return Notifization.Invalid(searchResult.Message);
            }
            // 
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT * FROM App_ProductWarranty WHERE dbo.Uni2NONE(Title) LIKE N'%'+ @Query +'%' " + whereCondition + " ORDER BY [CreatedDate]";


            List<ProductWarrantyResult> dtList = _connection.Query<ProductWarrantyResult>(sqlQuery, new { Query = Helper.Page.Library.FormatToUni2NONE(query) }).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            var result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
            if (result.Count == 0 && page > 1)
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
            return Notifization.Data(MessageText.Success + "::" + sqlQuery, data: result, role: RoleActionSettingService.RoleListForUser(), paging: pagingModel);
        }

        //##############################################################################################################################################################################################################################################################
        public ActionResult Create(ProductWarrantyCreateModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    if (model == null)
                        return Notifization.Invalid();

                    string title = model.Title;
                    string summary = model.Summary;
                    if (string.IsNullOrEmpty(title))
                        return Notifization.Invalid("Không được để trống tiêu đề");
                    title = title.Trim();
                    if (!Validate.TestText(title))
                        return Notifization.Invalid("Tiêu đề không hợp lệ");
                    if (title.Length < 2 || title.Length > 80)
                        return Notifization.Invalid("Tiêu đề giới hạn [2-80] ký tự");
                    // summary valid               
                    if (!string.IsNullOrEmpty(summary))
                    {
                        summary = summary.Trim();
                        if (!Validate.TestText(summary))
                            return Notifization.Invalid("Mô tả không hợp lệ");
                        //
                        if (summary.Length < 1 || summary.Length > 120)
                            return Notifization.Invalid("Mô tả giới hạn [1-120] ký tự");
                    }
                    ProductWarrantyService productWarrantyService = new ProductWarrantyService(_connection);
                    ProductWarranty productWarranty = productWarrantyService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == model.Title.ToLower(), transaction: _transaction).FirstOrDefault();
                    if (productWarranty != null)
                        return Notifization.Invalid("Tiêu đề đã được sử dụng");
                    //
                    var Id = productWarrantyService.Create<string>(new ProductWarranty()
                    {
                        Title = title,
                        Alias = Helper.Page.Library.FormatToUni2NONE(title),
                        Summary = summary,
                        LanguageID = Helper.Current.UserLogin.LanguageID,
                        Enabled = model.Enabled,
                    }, transaction: _transaction);
                    string temp = string.Empty;

                    //sort
                    _transaction.Commit();
                    return Notifization.Success(MessageText.CreateSuccess);
                }
                catch
                {
                    _transaction.Rollback();
                    return Notifization.NotService;
                }
            }
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(ProductWarrantyUpdateModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    if (model == null)
                        return Notifization.Invalid();

                    string title = model.Title;
                    string summary = model.Summary;
                    if (string.IsNullOrEmpty(title))
                        return Notifization.Invalid("Không được để trống tiêu đề");
                    title = title.Trim();
                    if (!Validate.TestText(title))
                        return Notifization.Invalid("Tiêu đề không hợp lệ");
                    if (title.Length < 2 || title.Length > 80)
                        return Notifization.Invalid("Tiêu đề giới hạn [2-80] ký tự");
                    // summary valid               
                    if (!string.IsNullOrEmpty(summary))
                    {
                        summary = summary.Trim();
                        if (!Validate.TestText(summary))
                            return Notifization.Invalid("Mô tả không hợp lệ");
                        if (summary.Length < 1 || summary.Length > 120)
                            return Notifization.Invalid("Mô tả giới hạn [1-120] ký tự");
                    }
                    ProductWarrantyService productWarrantyService = new ProductWarrantyService(_connection);
                    string id = model.ID.ToLower();
                    ProductWarranty productWarranty = productWarrantyService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    if (productWarranty == null)
                        return Notifization.NotFound(MessageText.NotFound);
                    //
                    ProductWarranty  productWarrantyTitle = productWarrantyService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower() && productWarranty.ID != id, transaction: _transaction).FirstOrDefault();
                    if (productWarrantyTitle != null)
                        return Notifization.Invalid("Tiêu đề đã được sử dụng");
                    // update user information
                    productWarranty.Title = title;
                    productWarranty.Alias = Helper.Page.Library.FormatToUni2NONE(title);
                    productWarranty.Summary = model.Summary;
                    productWarranty.Enabled = model.Enabled;
                    productWarrantyService.Update(productWarranty, transaction: _transaction);
                    _transaction.Commit();
                    return Notifization.Success(MessageText.UpdateSuccess);
                }
                catch
                {
                    _transaction.Rollback();
                    return Notifization.NotService;
                }
            }
        }
        public ProductWarranty GetProductWarrantyByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_ProductWarranty WHERE ID = @Query";
                return _connection.Query<ProductWarranty>(sqlQuery, new { Query = id }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        public ProductWarrantyResult ViewProductWarrantyByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_ProductWarranty WHERE ID = @Query";
                return _connection.Query<ProductWarrantyResult>(sqlQuery, new { Query = id }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.NotFound();
            // 
            id = id.ToLower();
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    var ProductWarrantyService = new ProductWarrantyService(_connection);
                    var ProductWarranty = ProductWarrantyService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    if (ProductWarranty == null)
                        return Notifization.NotFound();
                    ProductWarrantyService.Remove(ProductWarranty.ID, transaction: _transaction);
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
        //##############################################################################################################################################################################################################################################################
        public ActionResult Details(string Id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Id))
                    return Notifization.NotFound(MessageText.Invalid);
                //
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT * FROM App_ProductWarranty WHERE ID = @ID";
                var item = _connection.Query<ProductWarrantyResult>(sqlQuery, new { ID = Id }).FirstOrDefault();
                if (item == null)
                    return Notifization.NotFound(MessageText.NotFound);
                //
                return Notifization.Data(MessageText.Success, data: item, role: null, paging: null);
            }
            catch
            {
                return Notifization.NotService;
            }
        }
        //##############################################################################################################################################################################################################################################################
        public static string DDLProductWarranty(string id)
        {
            try
            {
                string result = string.Empty;
                using (var ProductWarrantyService = new ProductWarrantyService())
                {
                    var dtList = ProductWarrantyService.DataOption(id);
                    if (dtList.Count > 0)
                    {
                        foreach (var item in dtList)
                        {
                            string select = string.Empty;
                            if (!string.IsNullOrWhiteSpace(id) && item.ID == id.ToLower())
                                select = "selected";
                            result += "<option value='" + item.ID + "'" + select + ">" + item.Title + "</option>";
                        }
                    }
                    return result;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
        public List<ProductWarrantyOption> DataOption(string langID)
        {
            try
            {
                string sqlQuery = @"SELECT * FROM App_ProductWarranty ORDER BY Title ASC";
                return _connection.Query<ProductWarrantyOption>(sqlQuery, new { LangID = langID }).ToList();
            }
            catch
            {
                return new List<ProductWarrantyOption>();
            }
        }
        public static string GetNameByID(string id)
        {
            using (var service = new ProductWarrantyService())
            {
                ProductWarranty productWarranty = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (productWarranty == null)
                    return string.Empty;
                //
                return productWarranty.Title;
            }
        }
    }
}
