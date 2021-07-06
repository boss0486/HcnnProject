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
using WebCore.ENM;
using WebCore.Model.Enum;
using Helper.Page;

namespace WebCore.Services
{
    public interface IProductCategoryService : IEntityService<ProductCategory> { }
    public class ProductCategoryService : EntityService<ProductCategory>, IProductCategoryService
    {
        public ProductCategoryService() : base() { }
        public ProductCategoryService(System.Data.IDbConnection db) : base(db) { }
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
            ProductCategoryService ProductCategoryService = new ProductCategoryService(_connection);
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = $@"SELECT * FROM App_ProductCategory WHERE Title LIKE N'%'+ dbo.Uni2NONE(@Query) +'%' {whereCondition} ORDER BY OrderID ASC ";

            List<ProductCategoryResult> productCategoryResults = _connection.Query<ProductCategoryResult>(sqlQuery, new { Query = query }).ToList();
            if (productCategoryResults.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<ProductCategoryResult> dtList = productCategoryResults.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            foreach (var item in dtList)
            {
                item.SubData = SubDataList(item.ID, productCategoryResults);
            }

            List<ProductCategoryResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
            if (result.Count <= 0 && page > 1)
            {
                page -= 1;
                result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
            }
            if (result.Count <= 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            Helper.Pagination.PagingModel pagingModel = new Helper.Pagination.PagingModel
            {
                PageSize = Helper.Pagination.Paging.PAGESIZE,
                Total = dtList.Count,
                Page = page
            };
            //
            return Notifization.Data(MessageText.Success, data: result, role: RoleActionSettingService.RoleListForUser(), paging: pagingModel);

        }  
        public ActionResult GetProductCategoryOption()
        {
            ProductCategoryService productCategoryService = new ProductCategoryService(_connection);
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = $@"SELECT * FROM App_ProductCategory ORDER BY OrderID ASC ";

            List<ProductCategoryResult> productCategoryResults = _connection.Query<ProductCategoryResult>(sqlQuery).ToList();
            if (productCategoryResults.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<ProductCategoryResult> dtList = productCategoryResults.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            foreach (var item in dtList)
            {
                item.IsSub = false;
                item.SubData = SubDataList(item.ID, productCategoryResults);
            }
            return Notifization.Data(MessageText.Success, dtList);
        }
        public List<ProductCategoryResult> SubDataList(string parentId, List<ProductCategoryResult> productCategoryResults)
        {
            List<ProductCategoryResult> dtList = productCategoryResults.Where(m => m.ParentID == parentId).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return null;
            //
            foreach (var item in dtList)
            {
                item.IsSub = true;
                item.SubData = SubDataList(item.ID, productCategoryResults);
            }
            return dtList;
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Create(ProductCategoryCreateModel model)
        {
            if (model == null)
                return Notifization.Invalid("Dữ liệu không hợp lệ");
            string title = model.Title;
            string summary = model.Summary;
            string alias = model.Alias;
            string parentId = model.ParentID;
            int sortType = model.OrderID;

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
            //    
            ProductCategoryService productCategoryService = new ProductCategoryService(_connection);
            if (string.IsNullOrWhiteSpace(parentId))
            {
                parentId = null;
            }

            ProductCategory ProductCategory = productCategoryService.GetAlls(m => m.Title.ToLower() == title.ToLower()).FirstOrDefault();
            if (ProductCategory != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            // 
            int orderId = 1;
            if (sortType == (int)ProductCategoryEnum.ProductSort.FIRST)
            {
                List<ProductCategory> productCategorys = productCategoryService.GetAlls(m => m.ParentID == parentId).OrderBy(m => m.OrderID).ToList();
                if (productCategorys.Count > 0)
                {
                    int cnt = 2;
                    foreach (var item in productCategorys)
                    {
                        item.OrderID = cnt;
                        productCategoryService.Update(item);
                        cnt++;
                    }
                }
                //
            }
            else
            {
                orderId = productCategoryService.GetAlls(m => m.ParentID == parentId).Select(t => t.OrderID).DefaultIfEmpty().ToList().Max() + 1;
            }
            //
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            string _controllerText = Helper.Page.MetaSEO.ControllerText.ToLower();
            //
            string imgFile = model.ImageFile;
            string id = productCategoryService.Create<string>(new ProductCategory()
            {
                ParentID = parentId,
                Title = model.Title,
                Alias = Helper.Page.Library.FormatToUni2NONE(title),
                Path = "",
                Summary = summary,
                IconFont = model.IconFont,
                ImageFile = imgFile,
                OrderID = orderId,
                Enabled = model.Enabled,
            });
            id = id.ToLower();
            // file
            if (!string.IsNullOrWhiteSpace(imgFile))
            {
                string guid = attachmentIngredientService.Create<string>(new Entities.AttachmentIngredient()
                {
                    ForID = id,
                    FileID = imgFile,
                    CategoryID = _controllerText,
                    TypeID = (int)ModelEnum.FileType.ALONE
                });
            }
            // update part
            string strPath = string.Empty;
            ProductCategory ProductCategoryParent = productCategoryService.GetAlls(m => m.ID == parentId).FirstOrDefault();
            if (ProductCategoryParent != null)
                strPath = ProductCategoryParent.Path + "/" + id;
            else
                strPath = "/" + id;
            //
            ProductCategory = productCategoryService.GetAlls(m => m.ID == id).FirstOrDefault();
            ProductCategory.Path = strPath;
            productCategoryService.Update(ProductCategory);

            return Notifization.Success(MessageText.CreateSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(ProductCategoryUpdateModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            string title = model.Title;
            string summary = model.Summary;
            int sortType = model.OrderID;
            string parentId = model.ParentID;
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
            //  
            ProductCategoryService productCategoryService = new ProductCategoryService(_connection);
            string id = model.ID;
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.Invalid(MessageText.Invalid);
            //
            id = id.ToLower();
            ProductCategory productCategory = productCategoryService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (productCategory == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            ProductCategory productCategoryTitle = productCategoryService.GetAlls(m => m.Title.ToLower() == title.ToLower() && m.ID != id).FirstOrDefault();
            if (productCategoryTitle != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            //
            string alias = model.Alias;
            string productCategoryPath = productCategory.Path;
            int orderId = productCategory.OrderID;
            // check parentId
            if (string.IsNullOrWhiteSpace(parentId))
            {
                parentId = null;
                productCategoryPath = "/" + id;
            }
            else
            {
                //if (ProductCategory.ID != ProductCategory.ParentID)
                //    parentId = ProductCategory.ParentID;
                ////
                ProductCategory mPath = productCategoryService.GetAlls(m => m.ID == parentId).FirstOrDefault();
                if (mPath != null)
                    productCategoryPath = mPath.Path + "/" + id;
            }
            //
            if (sortType == (int)ProductCategoryEnum.ProductSort.FIRST)
            {
                orderId = 1;
                List<ProductCategory> productCategorys1 = productCategoryService.GetAlls(m => m.ParentID == parentId).OrderBy(m => m.OrderID).ToList();
                if (productCategorys1.Count > 0)
                {
                    int cnt = 2;
                    foreach (var item in productCategorys1)
                    {
                        item.OrderID = cnt;
                        productCategoryService.Update(item);
                        cnt++;
                    }
                }
                //
            }
            if (sortType == (int)ProductCategoryEnum.ProductSort.LAST)
            {
                orderId = productCategoryService.GetAlls(m => m.ParentID == parentId).Select(t => t.OrderID).DefaultIfEmpty().ToList().Max() + 1;
            }
            // 
            string _controllerText = Helper.Page.MetaSEO.ControllerText.ToLower();
            string imgFile = model.ImageFile;
            // update content
            productCategory.ParentID = parentId;
            productCategory.Path = productCategoryPath;
            productCategory.IconFont = model.IconFont;
            productCategory.ImageFile = imgFile;
            productCategory.Title = title;
            productCategory.Summary = summary;
            productCategory.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            productCategory.BackLink = model.BackLink;
            productCategory.OrderID = orderId;
            productCategory.Enabled = model.Enabled;
            productCategoryService.Update(productCategory);
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public ProductCategory GetProductCategoryByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_ProductCategory WHERE ID = @Query";
            ProductCategory productCategory = _connection.Query<ProductCategory>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (productCategory == null)
                return null;
            //
            return productCategory;
        }

        public ViewProductCategory ViewProductCategoryByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_ProductCategory WHERE ID = @Query";
            ViewProductCategory productCategoryResult = _connection.Query<ViewProductCategory>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (productCategoryResult == null)
                return null;
            //
            return productCategoryResult;
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(ProductCategoryIDModel model)
        {
            using (var _connectDb = DbConnect.Connection.CMS)
            {
                if (model == null)
                    return Notifization.Invalid(MessageText.Invalid);
                //
                _connectDb.Open();
                using (var _transaction = _connectDb.BeginTransaction())
                {
                    try
                    {
                        string id = model.ID;
                        if (string.IsNullOrWhiteSpace(id))
                            return Notifization.Invalid(MessageText.Invalid);
                        //
                        ProductCategoryService productCategoryService = new ProductCategoryService(_connectDb);
                        ProductCategory productCategory = productCategoryService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                        if (productCategory == null)
                            return Notifization.Invalid(MessageText.Invalid);
                        // remote ProductCategory, children ProductCategory
                        ////string sqlQuery = @"
                        ////        WITH ProductCategoryTemp AS
                        ////        (select ID,ImageFile  
                        ////         from App_ProductCategory
                        ////         where  ID  =  @ID
                        ////         union all
                        ////         select c.ID, c.ImageFile
                        ////         from App_ProductCategory c
                        ////            inner join ProductCategoryTemp mn on c.ParentID = mn.ID
                        ////        )SELECT ID,ImageFile FROM ProductCategoryTemp ";
                        ////List<ProductCategoryDeleteAndAtactmentModel> ProductCategoryDeleteAndAtactmentModels = _connectDb.Query<ProductCategoryDeleteAndAtactmentModel>(sqlQuery, new { ID = model.ID }, transaction: _transaction).ToList();
                        ////// delete file
                        ////if (ProductCategoryDeleteAndAtactmentModels.Count > 0)
                        ////{
                        ////    foreach (var item in ProductCategoryDeleteAndAtactmentModels)
                        ////        AttachmentFile.DeleteFile(item.ImageFile, transaction: _transaction);
                        ////}
                        // delete ProductCategory
                        string queryDelete = $@"with ProductCategoryTemp as(select ID from App_ProductCategory Where ID = @ID union all select c.ID from App_ProductCategory c inner join ProductCategoryTemp mn on c.ParentID = mn.ID)DELETE App_ProductCategory  WHERE ID IN (SELECT ID FROM ProductCategoryTemp)";
                        _connectDb.Query(queryDelete, new { ID = id }, transaction: _transaction);
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
        } 
        //##############################################################################################################################################################################################################################################################
        public ActionResult SortUp(ProductCategoryIDModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    ProductCategoryService productCategoryService = new ProductCategoryService(_connection);
                    ProductCategory productCategory = productCategoryService.GetAlls(m => m.ID == model.ID, transaction: _transaction).FirstOrDefault();
                    if (productCategory == null)
                        return Notifization.Invalid(MessageText.Invalid);
                    int _orderId = productCategory.OrderID;
                    string _parentId = productCategory.ParentID;
                    // list first
                    IList<ProductCategorySortModel> lstFirst = new List<ProductCategorySortModel>();
                    // list last
                    IList<ProductCategorySortModel> lstLast = new List<ProductCategorySortModel>();
                    //
                    if (!string.IsNullOrWhiteSpace(_parentId))
                    {
                        List<ProductCategory> ProductCategorys = productCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (ProductCategorys.Count > 0)
                        {
                            foreach (var item in ProductCategorys)
                            {
                                // set list first
                                if (item.OrderID < productCategory.OrderID)
                                {
                                    lstFirst.Add(new ProductCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > productCategory.OrderID)
                                {
                                    lstLast.Add(new ProductCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                            }
                            //  first
                            int _cntFirst = 1;
                            if (lstFirst.Count > 0)
                            {
                                for (int i = 0; i < lstFirst.Count; i++)
                                {
                                    if (i == lstFirst.Count - 1)
                                    {
                                        productCategory.OrderID = _cntFirst;
                                        productCategoryService.Update(productCategory, transaction: _transaction);
                                        _cntFirst++;
                                    }
                                    ProductCategory itemFirst = productCategoryService.GetAlls(m => m.ID == lstFirst[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    productCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }

                            }
                            else
                            {
                                productCategory.OrderID = 1;
                                productCategoryService.Update(productCategory, transaction: _transaction);
                                _cntFirst++;
                            }
                            //last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count > 0)
                            {
                                foreach (var item in lstLast)
                                {
                                    ProductCategory itemLast = productCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    productCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            productCategory.OrderID = 1;
                            productCategoryService.Update(productCategory, transaction: _transaction);
                        }
                    }
                    else
                    {
                        // truong hop cha , _parentId = ""
                        List<ProductCategory> productCategorys = productCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (productCategorys.Count > 0)
                        {
                            foreach (var item in productCategorys)
                            {
                                // set list first
                                if (item.OrderID < productCategory.OrderID)
                                {
                                    lstFirst.Add(new ProductCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > productCategory.OrderID)
                                {
                                    lstLast.Add(new ProductCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                            }
                            //  first
                            int _cntFirst = 1;
                            if (lstFirst.Count > 0)
                            {
                                for (int i = 0; i < lstFirst.Count; i++)
                                {
                                    if (i == lstFirst.Count - 1)
                                    {
                                        productCategory.OrderID = _cntFirst;
                                        productCategoryService.Update(productCategory, transaction: _transaction);
                                        _cntFirst++;
                                    }
                                    ProductCategory itemFirst = productCategoryService.GetAlls(m => m.ID == lstFirst[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    productCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }

                            }
                            else
                            {
                                productCategory.OrderID = 1;
                                productCategoryService.Update(productCategory, transaction: _transaction);
                                _cntFirst++;
                            }
                            //last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count > 0)
                            {
                                foreach (var item in lstLast)
                                {
                                    ProductCategory itemLast = productCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    productCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            productCategory.OrderID = 1;
                            productCategoryService.Update(productCategory, transaction: _transaction);
                        }
                    }
                    _transaction.Commit();
                    return Notifization.Success(MessageText.UpdateSuccess);
                }
                catch (Exception ex)
                {
                    _transaction.Rollback();
                    return Notifization.TEST("::" + ex);
                }
            }// end transaction
        }
        //#######################################################################################################################################################################################
        public ActionResult SortDown(ProductCategoryIDModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    ProductCategoryService productCategoryService = new ProductCategoryService(_connection);
                    ProductCategory productCategory = productCategoryService.GetAlls(m => m.ID == model.ID, transaction: _transaction).FirstOrDefault();
                    if (productCategory == null)
                        return Notifization.TEST("::");
                    int _orderId = productCategory.OrderID;
                    string _parentId = productCategory.ParentID;
                    // list first
                    IList<ProductCategorySortModel> lstFirst = new List<ProductCategorySortModel>();
                    // list last
                    IList<ProductCategorySortModel> lstLast = new List<ProductCategorySortModel>();
                    //
                    if (!string.IsNullOrWhiteSpace(_parentId))
                    {
                        List<ProductCategory> productCategorys = productCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (productCategorys.Count > 0)
                        {
                            foreach (var item in productCategorys)
                            {
                                // set list first
                                if (item.OrderID < productCategory.OrderID)
                                {
                                    lstFirst.Add(new ProductCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > productCategory.OrderID)
                                {
                                    lstLast.Add(new ProductCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                            }
                            // xu ly
                            int _cntFirst = 1;
                            if (lstFirst.Count > 0)
                            {
                                foreach (var item in lstFirst)
                                {
                                    ProductCategory itemFirst = productCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    productCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }

                            }
                            //  last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count == 1)
                            {
                                ProductCategory itemLast = productCategoryService.GetAlls(m => m.ID == lstLast[0].ID, transaction: _transaction).FirstOrDefault();
                                itemLast.OrderID = _cntLast;
                                productCategoryService.Update(itemLast, transaction: _transaction);
                                //
                                productCategory.OrderID = _cntLast + 1;
                                productCategoryService.Update(productCategory, transaction: _transaction);
                                _cntLast++;
                            }
                            else if (lstLast.Count > 1)
                            {
                                for (int i = 0; i < lstLast.Count; i++)
                                {
                                    if (i == 1)
                                    {
                                        productCategory.OrderID = _cntLast;
                                        productCategoryService.Update(productCategory, transaction: _transaction);
                                        _cntLast++;
                                    }
                                    ProductCategory itemLast = productCategoryService.GetAlls(m => m.ID == lstLast[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    productCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            productCategory.OrderID = 1;
                            productCategoryService.Update(productCategory, transaction: _transaction);
                        }
                    }
                    else
                    {
                        // truong hop cha , _parentId = ""
                        List<ProductCategory> productCategorys = productCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (productCategorys.Count > 0)
                        {
                            foreach (var item in productCategorys)
                            {
                                // set list first
                                if (item.OrderID < productCategory.OrderID)
                                {
                                    lstFirst.Add(new ProductCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > productCategory.OrderID)
                                {
                                    lstLast.Add(new ProductCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                            }
                            // xu ly
                            int _cntFirst = 1;
                            if (lstFirst.Count > 0)
                            {
                                foreach (var item in lstFirst)
                                {
                                    ProductCategory itemFirst = productCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    productCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }
                            }
                            //  last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count == 1)
                            {
                                ProductCategory itemLast = productCategoryService.GetAlls(m => m.ID == lstLast[0].ID, transaction: _transaction).FirstOrDefault();
                                itemLast.OrderID = _cntLast;
                                productCategoryService.Update(itemLast, transaction: _transaction);
                                //
                                productCategory.OrderID = _cntLast + 1;
                                productCategoryService.Update(productCategory, transaction: _transaction);
                                _cntLast++;
                            }
                            else if (lstLast.Count > 1)
                            {
                                for (int i = 0; i < lstLast.Count; i++)
                                {
                                    if (i == 1)
                                    {
                                        productCategory.OrderID = _cntLast;
                                        productCategoryService.Update(productCategory, transaction: _transaction);
                                        _cntLast++;
                                    }
                                    ProductCategory itemLast = productCategoryService.GetAlls(m => m.ID == lstLast[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    productCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            productCategory.OrderID = 1;
                            productCategoryService.Update(productCategory, transaction: _transaction);
                        }
                    }
                    _transaction.Commit();
                    return Notifization.Success(MessageText.UpdateSuccess);
                }
                catch (Exception ex)
                {
                    _transaction.Rollback();
                    return Notifization.TEST("::" + ex);
                }
            }// end transaction
        }
        //#######################################################################################################################################################################################
        public static List<ProductCategoryBar> GetProductCategoryBar()
        {
            string sqlQuery = "SELECT * FROM App_ProductCategory WHERE Enabled = @Enabled ORDER BY OrderID ASC";
            ProductCategoryService productCategoryService = new ProductCategoryService();
            List<ProductCategoryBar> dtList = productCategoryService.Query<ProductCategoryBar>(sqlQuery, new { Enabled = (int)WebCore.Model.Enum.ModelEnum.State.ENABLED }).ToList();
            return dtList;
        }
        public static SubProductCategoryBar SubProductCategoryBar(string parentId, List<ProductCategoryBar> allData, string controllerText)
        {
            bool isToggled = false;
            bool isHasitem = false;
            string result = string.Empty;
            List<ProductCategoryBar> dtList = allData.Where(m => m.ParentID == parentId).ToList();
            if (dtList.Count == 0)
                return new SubProductCategoryBar();
            // 
            foreach (var item in dtList)
            {
                string _cls = "";
                string toggled = string.Empty;
                SubProductCategoryBar subProductCategoryBar = SubProductCategoryBar(item.ID, allData, controllerText);
                if (subProductCategoryBar == null)
                    continue;
                //
                string subClass = string.Empty;
                string link = item.BackLink;
                if (subProductCategoryBar.IsHasItem)
                    subClass = "ProductCategory-has-children";
                // 
                //if (string.IsNullOrWhiteSpace(link))
                //    link = $"/{item.UrlRoot}/{item.Alias}";
                ////
                result += $"<li class='{subClass}'><a href='{link}' class='{_cls}  {toggled}'>{item.Title}</a>{subProductCategoryBar.InnerText}</li>";
            }
            //
            if (!string.IsNullOrEmpty(result))
                isHasitem = true;
            //
            return new SubProductCategoryBar
            {
                InnerText = $"<ul class='navbar-nav ml-auto'>{result}</ul>",
                IsToggled = isToggled,
                IsHasItem = isHasitem
            };
        }
        public static string GetProductCategoryName(string id)
        {
            using (var service = new ProductCategoryService())
            {
                ProductCategory productCategory = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (productCategory == null)
                    return string.Empty;
                //
                return productCategory.Title;
            }
        }

        //#######################################################################################################################################################################################
        public static string GetProductCategoryForCategory(string selectedId, string templatePage, bool parentAllow = true)
        {
            ProductCategoryService productCategoryService = new ProductCategoryService();
            string result = string.Empty;
            string sqlQuery = "SELECT * FROM App_ProductCategory WHERE Enabled = @Enabled AND PageTemlate = @PageTemlate ORDER BY ParentID, OrderID ASC";
            List<ProductCategory> dtList = productCategoryService.Query<ProductCategory>(sqlQuery, new { Enabled = (int)WebCore.Model.Enum.ModelEnum.State.ENABLED, PageTemlate = templatePage }).ToList();
            if (dtList.Count() == 0)
                return result;
            //
            List<ProductCategory> productCategorys = dtList.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).ToList();
            foreach (var item in productCategorys)
            {
                SubProductCategoryBarForCategory subProductCategoryBarForCategory = SubProductCategoryForCategory(item.ID, dtList, selectedId);
                string disabled = string.Empty;
                if (!parentAllow && subProductCategoryBarForCategory.IsSubNull)
                    disabled = "disabled";
                //
                string _innerText = subProductCategoryBarForCategory.InnerText;
                string _state = string.Empty;
                if (item.ID == selectedId)
                    _state = "checked";
                //
                result += $@"<li><input id='cbx{item.ID}' type='checkbox' class='filled-in' data-id='{item.ID}' {_state} {disabled}/><label for='cbx{item.ID}'>{item.Title}</label></li>{_innerText}";
            }
            return result;
        }

        public static SubProductCategoryBarForCategory SubProductCategoryForCategory(string parentId, List<ProductCategory> allData, string selectedId)
        {
            string result = string.Empty;
            List<WebCore.Entities.ProductCategory> dtList = allData.Where(m => m.ParentID == parentId).ToList();
            if (dtList.Count == 0)
                return new SubProductCategoryBarForCategory
                {
                    InnerText = string.Empty,
                    IsSubNull = false
                };
            // 
            foreach (var item in dtList)
            {
                string toggled = string.Empty;
                SubProductCategoryBarForCategory subProductCategoryBarForCategory = SubProductCategoryForCategory(item.ID, allData, selectedId);
                string disabled = string.Empty;
                if (subProductCategoryBarForCategory.IsSubNull)
                    disabled = "disabled";
                //
                string _innerText = subProductCategoryBarForCategory.InnerText;
                string _state = string.Empty;
                if (item.ID == selectedId)
                    _state = "checked";
                //
                result += $@"<li><input id='cbx{item.ID}' data-ischild='true' type='checkbox' class='filled-in' data-id='{item.ID}' {_state} {disabled} /><label for='cbx{item.ID}'>{item.Title}</label></li>{_innerText}";
            }
            //
            return new SubProductCategoryBarForCategory
            {
                InnerText = $"<ul>{result}</ul>",
                IsSubNull = true
            };
        }
        //#######################################################################################################################################################################################
        public static ProductCategoryBar GetProductCategoryBarByAlias(string alias)
        {
            using (var service = new ProductCategoryService())
            {
                string sqlQuery = @"SELECT TOP 1 * FROM App_ProductCategory WHERE Alias = @Alias";
                ProductCategoryBar item = service.Query<ProductCategoryBar>(sqlQuery, new { Alias = alias }).FirstOrDefault();
                return item;
            }
        }

    }
}