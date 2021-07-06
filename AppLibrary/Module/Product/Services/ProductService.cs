using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using System;
using Dapper;
using System.Linq;
using PagedList;
using Helper;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Web;
using WebCore.Entities;
using WebCore.ENM;
using WebCore.Model.Enum;
using Helper.File;
using WebCore.Model.Entities;
using Helper.Page;
using Helper.Pagination;

namespace WebCore.Services
{
    public interface IProductService : IEntityService<Product> { }
    public class ProductService : EntityService<Product>, IProductService
    {
        public ProductService() : base() { }
        public ProductService(System.Data.IDbConnection db) : base(db) { }
        //##############################################################################################################################################################################################################################################################
        public ActionResult DataList(ProductSearchModel model)
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
            string sqlQuery = $@"SELECT p.*, c.Title as 'CategoryName'
            FROM App_Product as p
            LEFT JOIN App_ProductCategory AS c ON c.ID = p.CategoryID 
            WHERE dbo.Uni2NONE(p.Title) LIKE N'%'+ @Query +'%' {whereCondition} ORDER BY p.CreatedDate";
            var dtList = _connection.Query<ProductResult>(sqlQuery, new { Query = Helper.Page.Library.FormatNameToUni2NONE(query) }).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
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
            //
            return Notifization.Data(MessageText.Success, data: result, role: RoleActionSettingService.RoleListForUser(), paging: pagingModel);
        }

        //##############################################################################################################################################################################################################################################################
        public ActionResult Create(ProductCreateModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    if (model == null)
                        return Notifization.Invalid();
                    //
                    string menuId = model.MenuID;
                    string title = model.Title;
                    string summary = model.Summary;
                    string htmlNote = model.HtmlNote;
                    string htmlText = model.HtmlText;
                    string imgFile = model.ImageFile;
                    IEnumerable<string> arrFile = model.Photos;
                    //
                    if (string.IsNullOrWhiteSpace(title))
                        return Notifization.Invalid("Không được để trống tiêu đề");
                    title = title.Trim();
                    if (!Validate.TestText(title))
                        return Notifization.Invalid("Tiêu đề không hợp lệ");
                    if (title.Length < 2 || title.Length > 80)
                        return Notifization.Invalid("Tiêu đề giới hạn [2-80] ký tự");
                    // summary valid               
                    if (!string.IsNullOrWhiteSpace(summary))
                    {
                        summary = summary.Trim();
                        if (!Validate.TestText(summary))
                            return Notifization.Invalid("Mô tả không hợp lệ");
                        if (summary.Length < 1 || summary.Length > 120)
                            return Notifization.Invalid("Mô tả giới hạn [1-120] ký tự");
                    }
                    double price = model.Price;
                    if (price < 0)
                        return Notifization.Invalid("Giá sản phẩm phải >= 0");

                    double priceListed = model.PriceListed;
                    if (priceListed < 0)
                        return Notifization.Invalid("Giá khuyến mại phải >= 0");
                    // create date display
                    string viewDate = model.ViewDate;
                    if (!string.IsNullOrWhiteSpace(viewDate))
                    {
                        if (!Validate.TestDate(viewDate))
                            return Notifization.Invalid("Ngày hiển thị không hợp lệ");
                    }
                    // 
                    if (string.IsNullOrWhiteSpace(menuId))
                        return Notifization.Invalid("Vui lòng chọn danh mục");
                    //
                    string groupId = model.GroupID;
                    if (string.IsNullOrWhiteSpace(groupId))
                        return Notifization.Invalid("Vui lòng chọn nhóm sản phẩm");
                    //
                    var warrantyId = model.WarrantyID;
                    if (string.IsNullOrWhiteSpace(warrantyId))
                        return Notifization.Invalid("Vui lòng chọn thời gian bảo hành");
                    //
                    var madeInId = model.MadeInID;
                    if (string.IsNullOrWhiteSpace(madeInId))
                        return Notifization.Invalid("Vui lòng chọn nhà cung cấp");
                    //
                    var state = model.State;
                    if (state == -1)
                        return Notifization.Invalid("Vui lòng chọn tình trạng");
                    //
                    ProductService productService = new ProductService(_connection);
                    var product = productService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == model.Title.ToLower(), transaction: _transaction).FirstOrDefault();
                    if (product != null)
                        return Notifization.Invalid("Tiêu đề đã được sử dụng");
                    //
                    groupId = groupId.ToLower();
                    ProductGroupService productGroupService = new ProductGroupService(_connection);
                    ProductGroup productGroup = productGroupService.GetAlls(m => m.ID == groupId, transaction: _transaction).FirstOrDefault();
                    if (productGroup == null)
                        return Notifization.NotFound();
                    // 
                    MenuService menuService = new MenuService(_connection);
                    Menu menu = menuService.GetAlls(m => m.ParentID == menuId, transaction: _transaction).FirstOrDefault();
                    if (menu != null)
                        return Notifization.Invalid("Danh mục không hợp lệ");
                    //  
                    if (arrFile != null && arrFile.Count() > 5)
                        return Notifization.Invalid("Tệp tin đính kèm giới hạn [1-5]");
                    //
                    var id = productService.Create<string>(new Product()
                    {
                        TextID = model.TextID,
                        Title = title,
                        Alias = Helper.Page.Library.FormatToUni2NONE(title),
                        Summary = summary,
                        ImageFile = imgFile,
                        HtmlNote = htmlNote,
                        HtmlText = htmlText,
                        Tag = model.Tag,
                        ViewTotal = model.ViewTotal,
                        ViewDate = Helper.TimeData.TimeFormat.FormatToServerDate(viewDate),
                        MenuID = menuId,
                        GroupID = groupId,
                        WarrantyID = warrantyId,
                        MadeInID = madeInId,
                        Price = price,
                        PriceListed = priceListed,
                        PriceText = model.PriceText,
                        State = model.State,
                        LanguageID = Helper.Current.UserLogin.LanguageID,
                        Enabled = model.Enabled,
                    }, transaction: _transaction);
                    // file  
                    AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
                    attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.ALONE, id, new List<string>
                    {
                        imgFile
                    }, _transaction, _connection);
                    attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.MULTI, id, arrFile, _transaction, _connection);
                    //
                    _transaction.Commit();
                    return Notifization.Success(MessageText.CreateSuccess);
                }
                catch (Exception ex)
                {
                    return Notifization.NotService;
                }
            }
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(ProductUpdateModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    if (model == null)
                        return Notifization.Invalid();
                    //
                    string menuId = model.MenuID;
                    string textId = model.TextID;
                    string title = model.Title;
                    string summary = model.Summary;
                    string htmlNote = model.HtmlNote;
                    string htmlText = model.HtmlText;

                    string categoryId = model.GroupID;
                    string warrantyId = model.WarrantyID;
                    string madein = model.MadeInID;

                    double price = model.Price;
                    double priceListed = model.PriceListed;
                    int viewTotal = model.ViewTotal;
                    int state = model.State;
                    IEnumerable<string> arrFile = model.Photos;
                    //
                    if (string.IsNullOrWhiteSpace(title))
                        return Notifization.Invalid("Không được để trống tiêu đề");
                    title = title.Trim();
                    if (!Validate.TestText(title))
                        return Notifization.Invalid("Tiêu đề không hợp lệ");
                    if (title.Length < 2 || title.Length > 80)
                        return Notifization.Invalid("Tiêu đề giới hạn [2-80] ký tự");
                    // summary valid               
                    if (!string.IsNullOrWhiteSpace(summary))
                    {
                        summary = summary.Trim();
                        if (!Validate.TestText(summary))
                            return Notifization.Invalid("Mô tả không hợp lệ");
                        if (summary.Length < 1 || summary.Length > 120)
                            return Notifization.Invalid("Mô tả giới hạn [1-120] ký tự");
                    }
                    // category
                    if (string.IsNullOrWhiteSpace(model.GroupID))
                        return Notifization.Invalid("Vui lòng chọn nhóm sản phẩm");
                    // price 
                    if (price < 0)
                        return Notifization.Invalid("Giá sản phẩm phải >= 0");
                    //
                    if (priceListed < 0)
                        return Notifization.Invalid("Giá khuyến mại phải >= 0");
                    // create date display
                    string viewDate = model.ViewDate;
                    if (!string.IsNullOrWhiteSpace(viewDate))
                    {
                        if (!Validate.TestDate(viewDate))
                            return Notifization.Invalid("Ngày hiển thị không hợp lệ");
                    }
                    //
                    if (string.IsNullOrWhiteSpace(menuId))
                        return Notifization.Invalid("Vui lòng chọn danh mục");
                    //
                    if (string.IsNullOrWhiteSpace(categoryId))
                        return Notifization.Invalid("Vui lòng chọn nhóm sản phẩm");
                    //
                    var warranty = model.WarrantyID;
                    if (string.IsNullOrWhiteSpace(warranty))
                        return Notifization.Invalid("Vui lòng chọn thời gian bảo hành");
                    //
                    var provider = model.MadeInID;
                    if (string.IsNullOrWhiteSpace(provider))
                        return Notifization.Invalid("Vui lòng chọn nhà cung cấp");
                    // 
                    if (state == -1)
                        return Notifization.Invalid("Vui lòng chọn tình trạng");
                    //
                    ProductService productService = new ProductService(_connection);
                    string id = model.ID.ToLower();
                    Product product = productService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    if (product == null)
                        return Notifization.NotFound(MessageText.NotFound);
                    //
                    Product productTitle = productService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower() && product.ID != id, transaction: _transaction).FirstOrDefault();
                    if (productTitle != null)
                        return Notifization.Invalid("Tiêu đề đã được sử dụng");
                    //
                    string imgFile = product.ImageFile;
                    if (!string.IsNullOrWhiteSpace(model.ImageFile))
                    {
                        if (model.ImageFile.Length != 36)
                            return Notifization.Invalid("Hình ảnh không hợp lệ");
                        //
                        imgFile = model.ImageFile;
                    }
                    //
                    MenuService menuService = new MenuService(_connection);
                    Menu menu = menuService.GetAlls(m => m.ParentID == menuId, transaction: _transaction).FirstOrDefault();
                    if (menu != null)
                        return Notifization.Invalid("Danh mục không hợp lệ");
                    // 
                    if (arrFile != null && arrFile.Count() > 5)
                        return Notifization.Invalid("Tệp tin đính kèm giới hạn [1-5]");
                    //
                    product.TextID = textId;
                    product.Title = title;
                    product.Alias = Helper.Page.Library.FormatToUni2NONE(title);
                    product.Summary = summary;
                    product.ImageFile = imgFile;
                    product.HtmlNote = htmlNote;
                    product.HtmlText = htmlText;
                    product.Tag = "";
                    product.ViewTotal = viewTotal;
                    product.ViewDate = Helper.TimeData.TimeFormat.FormatToServerDate(viewDate);
                    product.MenuID = menuId;
                    product.GroupID = categoryId;
                    product.WarrantyID = warrantyId;
                    product.MadeInID = madein;
                    product.Price = price;
                    product.PriceListed = priceListed;
                    product.PriceText = "";
                    product.State = model.State;
                    product.LanguageID = Helper.Current.UserLogin.LanguageID;
                    product.Enabled = model.Enabled;
                    productService.Update(product, transaction: _transaction);
                    // file  
                    AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
                    attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.ALONE, id, new List<string>
                    {
                        imgFile
                    }, _transaction, _connection);
                    attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.MULTI, id, arrFile, _transaction, _connection);
                    //
                    _transaction.Commit();
                    return Notifization.Success(MessageText.UpdateSuccess);
                }
                catch (Exception ex)
                {
                    _transaction.Rollback();
                    return Notifization.TEST("Ex:" + ex);
                }
            }
        }
        public Product GetProductByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                //
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_Product WHERE ID = @Query";
                var item = _connection.Query<Product>(sqlQuery, new { Query = id }).FirstOrDefault();
                if (item == null)
                    return new Product();
                //
                return item;
            }
            catch
            {
                return new Product();
            }
        }
        public ProductResult ViewProductByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return new ProductResult();
                //
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_Product WHERE ID = @Query";
                var item = _connection.Query<ProductResult>(sqlQuery, new { Query = id }).FirstOrDefault();
                return item;
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
                    ProductService productService = new ProductService(_connection);
                    Product product = productService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    if (product == null)
                        return Notifization.NotFound();
                    // delete 
                    productService.Remove(id, transaction: _transaction); 
                    // delete file  
                    AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
                    attachmentIngredientService.RemoveAllFileByForID(id, transaction: _transaction, connection: _connection);
                    //
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
        public static string DDLProduct(string id)
        {
            try
            {
                string result = string.Empty;
                using (var ProductService = new ProductService())
                {
                    var dtList = ProductService.DataOption(id);
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
        public List<ProductOption> DataOption(string langID)
        {
            try
            {
                string sqlQuery = @"SELECT * FROM App_Product ORDER BY Title ASC";
                return _connection.Query<ProductOption>(sqlQuery, new { LangID = langID }).ToList();
            }
            catch
            {
                return new List<ProductOption>();
            }
        }
        //##############################################################################################################################################################################################################################################################
        public static string DDLProductState(int id)
        {
            try
            {
                var productStateModels = new List<ProductStateModel>{
                    new ProductStateModel(1, "Còn hàng"),
                    new ProductStateModel(2, "Hết hàng")
                };
                string result = string.Empty;
                foreach (var item in productStateModels)
                {
                    string selected = string.Empty;
                    if (item.ID == id)
                        selected = "selected";
                    result += "<option value='" + item.ID + "' " + selected + ">" + item.Title + "</option>";
                }
                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string ViewState(int id)
        {
            string result;
            switch (id)
            {
                case 1:
                    result = "Còn hàng";
                    break;
                case 2:
                    result = "Hết hàng";
                    break;
                default:
                    result = string.Empty;
                    break;
            }
            return result;
        }



        //##############################################################################################################################################################################################################################################################
        public static List<ProductHome> GetProductForHome()
        {
             
            using (var service = new ProductService())
            {
                string sqlQuery = @"SELECT TOP (12) * FROM App_Product WHERE Enabled = 1 ORDER BY ViewDate DESC";
                List<ProductHome> items = service.Query<ProductHome>(sqlQuery).ToList();
                return items;
            }
        }
        public static IEnumerable<ProductHome> GetProductByCategory(string categoryId, int page = 1)
        {
            using (var service = new ProductService())
            {
                string whereCondition = string.Empty;
                if (!string.IsNullOrWhiteSpace(categoryId))
                    whereCondition = " AND CategoryID = @CategoryID";
                //
                string sqlQuery = $@"SELECT TOP (30) * FROM App_Product WHERE CategoryID = @CategoryID ORDER BY ViewDate DESC";
                IEnumerable<ProductHome> items = service.Query<ProductHome>(sqlQuery, new { CategoryID = categoryId }).OrderBy(m => m.ViewDate).ToList();
                return items.ToPagedList(page, PagedRender.PAGENUMBER);
            }
        }
        public static IEnumerable<ProductHome> GetProductByMenu(string menuId, int page = 1)
        {
            using (var service = new ProductService())
            {
                string whereCondition = string.Empty;
                if (!string.IsNullOrWhiteSpace(menuId))
                    whereCondition = " AND MenuID = @MenuID";
                //
                string sqlQuery = $@"SELECT TOP (30) * FROM App_Product WHERE Enabled = @Enabled {whereCondition} ORDER BY ViewDate DESC";
                IEnumerable<ProductHome> items = service.Query<ProductHome>(sqlQuery, new { Enabled = (int)ModelEnum.State.ENABLED, MenuID = menuId }).OrderBy(m => m.ViewDate).ToList();
                return items.ToPagedList(page, PagedRender.PAGENUMBER);
            }
        }

        public static ProductResult GetProductByAlias(string alias)
        {

            if (string.IsNullOrWhiteSpace(alias))
                return new ProductResult();
            //
            using (var service = new ProductService())
            {
                string sqlQuery = @"SELECT TOP (1) * FROM App_Product WHERE Alias = @Alias";
                ProductResult item = service.Query<ProductResult>(sqlQuery, new { Alias = alias }).FirstOrDefault();
                return item;
            }
        }

        public static List<ProductHome> GetProductOther(string categoryId, string id)
        {
            using (var service = new ProductService())
            {
                string whereCondition = string.Empty;
                if (!string.IsNullOrWhiteSpace(categoryId))
                    whereCondition += " AND CategoryID = @CategoryID";
                //
                if (!string.IsNullOrWhiteSpace(id))
                    whereCondition += " AND ID != @ID";
                //
                string sqlQueryAll = $@"SELECT TOP(30) * FROM App_Product WHERE ID IS NOT NULL {whereCondition} ORDER BY ViewDate";
                List<ProductHome> productHomes = service.Query<ProductHome>(sqlQueryAll, new { CategoryID = categoryId, ID = id }).ToList();
                return productHomes;
            }
        }

        public static IEnumerable<ProductHome> ProductHomeSearch(ProductHomeSearch model, int page = 1)
        {
            using (var service = new ProductService())
            {
                string whereCondition = string.Empty;
                if (model == null)
                    return new List<ProductHome>();
                //
                if (model.PriceMin >= 0)
                    whereCondition += " AND Price >= @PriceMin";
                // 
                if (model.PriceMax >= 0)
                    whereCondition += " AND Price <= @PriceMax";
                //
                string sqlQuery = $@"SELECT TOP (30) * FROM App_Product WHERE (dbo.Uni2NONE(Title) LIKE N'%'+ @Query +'%' OR dbo.Uni2NONE(TextID) LIKE N'%'+ @Query +'%')  {whereCondition}";
                IEnumerable<ProductHome> items = service.Query<ProductHome>(sqlQuery, new { Query = Helper.Page.Library.FormatNameToUni2NONE(model.Query), PriceMin = model.PriceMin, PriceMax = model.PriceMax }).OrderBy(m => m.ViewDate).ToList();
                return items.ToPagedList(page, PagedRender.PAGENUMBER);
            }
        }
    }
}