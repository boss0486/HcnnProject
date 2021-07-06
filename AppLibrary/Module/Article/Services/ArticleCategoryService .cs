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
    public interface IArticleCategoryService : IEntityService<ArticleCategory> { }
    public class ArticleCategoryService : EntityService<ArticleCategory>, IArticleCategoryService
    {
        public ArticleCategoryService() : base() { }
        public ArticleCategoryService(System.Data.IDbConnection db) : base(db) { }
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
            ArticleCategoryService articleCategoryService = new ArticleCategoryService(_connection);
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = $@"SELECT * FROM App_ArticleCategory WHERE Title LIKE N'%'+ dbo.Uni2NONE(@Query) +'%' {whereCondition} ORDER BY OrderID ASC ";

            List<ArticleCategoryResult> articleCategoryResults = _connection.Query<ArticleCategoryResult>(sqlQuery, new { Query = query }).ToList();
            if (articleCategoryResults.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<ArticleCategoryResult> dtList = articleCategoryResults.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            foreach (var item in dtList)
            {
                item.SubArticleCategory = SubDataList(item.ID, articleCategoryResults);
            }

            List<ArticleCategoryResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
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

        public ActionResult GetArticleCategoryOption()
        {
            ArticleCategoryService articleCategoryService = new ArticleCategoryService(_connection);
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = $@"SELECT * FROM App_ArticleCategory ORDER BY OrderID ASC ";

            List<ArticleCategoryResult> articleCategoryResults = _connection.Query<ArticleCategoryResult>(sqlQuery).ToList();
            if (articleCategoryResults.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<ArticleCategoryResult> dtList = articleCategoryResults.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            foreach (var item in dtList)
            {
                item.IsSub = false;
                item.SubArticleCategory = SubDataList(item.ID, articleCategoryResults);
            }
            return Notifization.Data(MessageText.Success, dtList);
        }
        public List<ArticleCategoryResult> SubDataList(string parentId, List<ArticleCategoryResult> articleCategoryResults)
        {
            List<ArticleCategoryResult> dtList = articleCategoryResults.Where(m => m.ParentID == parentId).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return null;
            //
            foreach (var item in dtList)
            {
                item.IsSub = true;
                item.SubArticleCategory = SubDataList(item.ID, articleCategoryResults);
            }
            return dtList;
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Create(ArticleCategoryCreateModel model)
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
            ArticleCategoryService articleCategoryService = new ArticleCategoryService(_connection);
            if (string.IsNullOrWhiteSpace(parentId))
            {
                parentId = null;
            }

            ArticleCategory ArticleCategory = articleCategoryService.GetAlls(m => m.Title.ToLower() == title.ToLower()).FirstOrDefault();
            if (ArticleCategory != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            // 
            int orderId = 1;
            if (sortType == (int)ArticleCategoryEnum.ArticleSort.FIRST)
            {
                List<ArticleCategory> articleCategorys = articleCategoryService.GetAlls(m => m.ParentID == parentId).OrderBy(m => m.OrderID).ToList();
                if (articleCategorys.Count > 0)
                {
                    int cnt = 2;
                    foreach (var item in articleCategorys)
                    {
                        item.OrderID = cnt;
                        articleCategoryService.Update(item);
                        cnt++;
                    }
                }
                //
            }
            else
            {
                orderId = articleCategoryService.GetAlls(m => m.ParentID == parentId).Select(t => t.OrderID).DefaultIfEmpty().ToList().Max() + 1;
            }
            //
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            string _controllerText = Helper.Page.MetaSEO.ControllerText.ToLower();
            //
            string imgFile = model.ImageFile;
            string id = articleCategoryService.Create<string>(new ArticleCategory()
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
            ArticleCategory ArticleCategoryParent = articleCategoryService.GetAlls(m => m.ID == parentId).FirstOrDefault();
            if (ArticleCategoryParent != null)
                strPath = ArticleCategoryParent.Path + "/" + id;
            else
                strPath = "/" + id;
            //
            ArticleCategory = articleCategoryService.GetAlls(m => m.ID == id).FirstOrDefault();
            ArticleCategory.Path = strPath;
            articleCategoryService.Update(ArticleCategory);

            return Notifization.Success(MessageText.CreateSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(ArticleCategoryUpdateModel model)
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
            ArticleCategoryService articleCategoryService = new ArticleCategoryService(_connection);
            string id = model.ID;
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.Invalid(MessageText.Invalid);
            //
            id = id.ToLower();
            ArticleCategory articleCategory = articleCategoryService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (articleCategory == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            ArticleCategory articleCategoryTitle = articleCategoryService.GetAlls(m => m.Title.ToLower() == title.ToLower() && m.ID != id).FirstOrDefault();
            if (articleCategoryTitle != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            //
            string alias = model.Alias;
            string articleCategoryPath = articleCategory.Path;
            int orderId = articleCategory.OrderID;
            // check parentId
            if (string.IsNullOrWhiteSpace(parentId))
            {
                parentId = null;
                articleCategoryPath = "/" + id;
            }
            else
            {
                //if (ArticleCategory.ID != ArticleCategory.ParentID)
                //    parentId = ArticleCategory.ParentID;
                ////
                ArticleCategory mPath = articleCategoryService.GetAlls(m => m.ID == parentId).FirstOrDefault();
                if (mPath != null)
                    articleCategoryPath = mPath.Path + "/" + id;
            }
            //
            if (sortType == (int)ArticleCategoryEnum.ArticleSort.FIRST)
            {
                orderId = 1;
                List<ArticleCategory> articleCategorys1 = articleCategoryService.GetAlls(m => m.ParentID == parentId).OrderBy(m => m.OrderID).ToList();
                if (articleCategorys1.Count > 0)
                {
                    int cnt = 2;
                    foreach (var item in articleCategorys1)
                    {
                        item.OrderID = cnt;
                        articleCategoryService.Update(item);
                        cnt++;
                    }
                }
                //
            }
            if (sortType == (int)ArticleCategoryEnum.ArticleSort.LAST)
            {
                orderId = articleCategoryService.GetAlls(m => m.ParentID == parentId).Select(t => t.OrderID).DefaultIfEmpty().ToList().Max() + 1;
            }
            // 
            string _controllerText = Helper.Page.MetaSEO.ControllerText.ToLower();
            string imgFile = model.ImageFile;
            // update content
            articleCategory.ParentID = parentId;
            articleCategory.Path = articleCategoryPath;
            articleCategory.IconFont = model.IconFont;
            articleCategory.ImageFile = imgFile;
            articleCategory.Title = title;
            articleCategory.Summary = summary;
            articleCategory.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            articleCategory.BackLink = model.BackLink;
            articleCategory.OrderID = orderId;
            articleCategory.Enabled = model.Enabled;
            articleCategoryService.Update(articleCategory);
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public ArticleCategory GetArticleCategoryByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_ArticleCategory WHERE ID = @Query";
            ArticleCategory articleCategory = _connection.Query<ArticleCategory>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (articleCategory == null)
                return null;
            //
            return articleCategory;
        }

        public ViewArticleCategory ViewArticleCategoryByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_ArticleCategory WHERE ID = @Query";
            ViewArticleCategory articleCategoryResult = _connection.Query<ViewArticleCategory>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (articleCategoryResult == null)
                return null;
            //
            return articleCategoryResult;
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(ArticleCategoryIDModel model)
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
                        ArticleCategoryService articleCategoryService = new ArticleCategoryService(_connectDb);
                        ArticleCategory articleCategory = articleCategoryService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                        if (articleCategory == null)
                            return Notifization.Invalid(MessageText.Invalid);
                        // remote ArticleCategory, children ArticleCategory
                        ////string sqlQuery = @"
                        ////        WITH ArticleCategoryTemp AS
                        ////        (select ID,ImageFile  
                        ////         from App_ArticleCategory
                        ////         where  ID  =  @ID
                        ////         union all
                        ////         select c.ID, c.ImageFile
                        ////         from App_ArticleCategory c
                        ////            inner join ArticleCategoryTemp mn on c.ParentID = mn.ID
                        ////        )SELECT ID,ImageFile FROM ArticleCategoryTemp ";
                        ////List<ArticleCategoryDeleteAndAtactmentModel> ArticleCategoryDeleteAndAtactmentModels = _connectDb.Query<ArticleCategoryDeleteAndAtactmentModel>(sqlQuery, new { ID = model.ID }, transaction: _transaction).ToList();
                        ////// delete file
                        ////if (ArticleCategoryDeleteAndAtactmentModels.Count > 0)
                        ////{
                        ////    foreach (var item in ArticleCategoryDeleteAndAtactmentModels)
                        ////        AttachmentFile.DeleteFile(item.ImageFile, transaction: _transaction);
                        ////}
                        // delete ArticleCategory
                        string queryDelete = $@"with ArticleCategoryTemp as(select ID from App_ArticleCategory Where ID = @ID union all select c.ID from App_ArticleCategory c inner join ArticleCategoryTemp mn on c.ParentID = mn.ID)DELETE App_ArticleCategory  WHERE ID IN (SELECT ID FROM ArticleCategoryTemp)";
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
        public ActionResult SortUp(ArticleCategoryIDModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    ArticleCategoryService articleCategoryService = new ArticleCategoryService(_connection);
                    ArticleCategory articleCategory = articleCategoryService.GetAlls(m => m.ID == model.ID, transaction: _transaction).FirstOrDefault();
                    if (articleCategory == null)
                        return Notifization.Invalid(MessageText.Invalid);
                    int _orderId = articleCategory.OrderID;
                    string _parentId = articleCategory.ParentID;
                    // list first
                    IList<ArticleCategorySortModel> lstFirst = new List<ArticleCategorySortModel>();
                    // list last
                    IList<ArticleCategorySortModel> lstLast = new List<ArticleCategorySortModel>();
                    //
                    if (!string.IsNullOrWhiteSpace(_parentId))
                    {
                        List<ArticleCategory> ArticleCategorys = articleCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (ArticleCategorys.Count > 0)
                        {
                            foreach (var item in ArticleCategorys)
                            {
                                // set list first
                                if (item.OrderID < articleCategory.OrderID)
                                {
                                    lstFirst.Add(new ArticleCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > articleCategory.OrderID)
                                {
                                    lstLast.Add(new ArticleCategorySortModel
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
                                        articleCategory.OrderID = _cntFirst;
                                        articleCategoryService.Update(articleCategory, transaction: _transaction);
                                        _cntFirst++;
                                    }
                                    ArticleCategory itemFirst = articleCategoryService.GetAlls(m => m.ID == lstFirst[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    articleCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }

                            }
                            else
                            {
                                articleCategory.OrderID = 1;
                                articleCategoryService.Update(articleCategory, transaction: _transaction);
                                _cntFirst++;
                            }
                            //last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count > 0)
                            {
                                foreach (var item in lstLast)
                                {
                                    ArticleCategory itemLast = articleCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    articleCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            articleCategory.OrderID = 1;
                            articleCategoryService.Update(articleCategory, transaction: _transaction);
                        }
                    }
                    else
                    {
                        // truong hop cha , _parentId = ""
                        List<ArticleCategory> articleCategorys = articleCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (articleCategorys.Count > 0)
                        {
                            foreach (var item in articleCategorys)
                            {
                                // set list first
                                if (item.OrderID < articleCategory.OrderID)
                                {
                                    lstFirst.Add(new ArticleCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > articleCategory.OrderID)
                                {
                                    lstLast.Add(new ArticleCategorySortModel
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
                                        articleCategory.OrderID = _cntFirst;
                                        articleCategoryService.Update(articleCategory, transaction: _transaction);
                                        _cntFirst++;
                                    }
                                    ArticleCategory itemFirst = articleCategoryService.GetAlls(m => m.ID == lstFirst[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    articleCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }

                            }
                            else
                            {
                                articleCategory.OrderID = 1;
                                articleCategoryService.Update(articleCategory, transaction: _transaction);
                                _cntFirst++;
                            }
                            //last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count > 0)
                            {
                                foreach (var item in lstLast)
                                {
                                    ArticleCategory itemLast = articleCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    articleCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            articleCategory.OrderID = 1;
                            articleCategoryService.Update(articleCategory, transaction: _transaction);
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
        public ActionResult SortDown(ArticleCategoryIDModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    ArticleCategoryService articleCategoryService = new ArticleCategoryService(_connection);
                    ArticleCategory articleCategory = articleCategoryService.GetAlls(m => m.ID == model.ID, transaction: _transaction).FirstOrDefault();
                    if (articleCategory == null)
                        return Notifization.TEST("::");
                    int _orderId = articleCategory.OrderID;
                    string _parentId = articleCategory.ParentID;
                    // list first
                    IList<ArticleCategorySortModel> lstFirst = new List<ArticleCategorySortModel>();
                    // list last
                    IList<ArticleCategorySortModel> lstLast = new List<ArticleCategorySortModel>();
                    //
                    if (!string.IsNullOrWhiteSpace(_parentId))
                    {
                        List<ArticleCategory> articleCategorys = articleCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (articleCategorys.Count > 0)
                        {
                            foreach (var item in articleCategorys)
                            {
                                // set list first
                                if (item.OrderID < articleCategory.OrderID)
                                {
                                    lstFirst.Add(new ArticleCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > articleCategory.OrderID)
                                {
                                    lstLast.Add(new ArticleCategorySortModel
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
                                    ArticleCategory itemFirst = articleCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    articleCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }

                            }
                            //  last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count == 1)
                            {
                                ArticleCategory itemLast = articleCategoryService.GetAlls(m => m.ID == lstLast[0].ID, transaction: _transaction).FirstOrDefault();
                                itemLast.OrderID = _cntLast;
                                articleCategoryService.Update(itemLast, transaction: _transaction);
                                //
                                articleCategory.OrderID = _cntLast + 1;
                                articleCategoryService.Update(articleCategory, transaction: _transaction);
                                _cntLast++;
                            }
                            else if (lstLast.Count > 1)
                            {
                                for (int i = 0; i < lstLast.Count; i++)
                                {
                                    if (i == 1)
                                    {
                                        articleCategory.OrderID = _cntLast;
                                        articleCategoryService.Update(articleCategory, transaction: _transaction);
                                        _cntLast++;
                                    }
                                    ArticleCategory itemLast = articleCategoryService.GetAlls(m => m.ID == lstLast[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    articleCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            articleCategory.OrderID = 1;
                            articleCategoryService.Update(articleCategory, transaction: _transaction);
                        }
                    }
                    else
                    {
                        // truong hop cha , _parentId = ""
                        List<ArticleCategory> articleCategorys = articleCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (articleCategorys.Count > 0)
                        {
                            foreach (var item in articleCategorys)
                            {
                                // set list first
                                if (item.OrderID < articleCategory.OrderID)
                                {
                                    lstFirst.Add(new ArticleCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > articleCategory.OrderID)
                                {
                                    lstLast.Add(new ArticleCategorySortModel
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
                                    ArticleCategory itemFirst = articleCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    articleCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }
                            }
                            //  last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count == 1)
                            {
                                ArticleCategory itemLast = articleCategoryService.GetAlls(m => m.ID == lstLast[0].ID, transaction: _transaction).FirstOrDefault();
                                itemLast.OrderID = _cntLast;
                                articleCategoryService.Update(itemLast, transaction: _transaction);
                                //
                                articleCategory.OrderID = _cntLast + 1;
                                articleCategoryService.Update(articleCategory, transaction: _transaction);
                                _cntLast++;
                            }
                            else if (lstLast.Count > 1)
                            {
                                for (int i = 0; i < lstLast.Count; i++)
                                {
                                    if (i == 1)
                                    {
                                        articleCategory.OrderID = _cntLast;
                                        articleCategoryService.Update(articleCategory, transaction: _transaction);
                                        _cntLast++;
                                    }
                                    ArticleCategory itemLast = articleCategoryService.GetAlls(m => m.ID == lstLast[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    articleCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            articleCategory.OrderID = 1;
                            articleCategoryService.Update(articleCategory, transaction: _transaction);
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
        public static List<ArticleCategoryBar> GetArticleCategoryBar()
        {
            string sqlQuery = "SELECT * FROM App_ArticleCategory WHERE Enabled = @Enabled ORDER BY OrderID ASC";
            ArticleCategoryService articleCategoryService = new ArticleCategoryService();
            List<ArticleCategoryBar> dtList = articleCategoryService.Query<ArticleCategoryBar>(sqlQuery, new { Enabled = (int)WebCore.Model.Enum.ModelEnum.State.ENABLED }).ToList();
            return dtList;
        }
        public static SubArticleCategoryBar SubArticleCategoryBar(string parentId, List<ArticleCategoryBar> allData, string controllerText)
        {
            bool isToggled = false;
            bool isHasitem = false;
            string result = string.Empty;
            List<ArticleCategoryBar> dtList = allData.Where(m => m.ParentID == parentId).ToList();
            if (dtList.Count == 0)
                return new SubArticleCategoryBar();
            // 
            foreach (var item in dtList)
            {
                string _cls = "";
                string toggled = string.Empty;
                SubArticleCategoryBar subArticleCategoryBar = SubArticleCategoryBar(item.ID, allData, controllerText);
                if (subArticleCategoryBar == null)
                    continue;
                //
                string subClass = string.Empty;
                string link = item.BackLink;
                if (subArticleCategoryBar.IsHasItem)
                    subClass = "ArticleCategory-has-children";
                // 
                //if (string.IsNullOrWhiteSpace(link))
                //    link = $"/{item.UrlRoot}/{item.Alias}";
                ////
                result += $"<li class='{subClass}'><a href='{link}' class='{_cls}  {toggled}'>{item.Title}</a>{subArticleCategoryBar.InnerText}</li>";
            }
            //
            if (!string.IsNullOrEmpty(result))
                isHasitem = true;
            //
            return new SubArticleCategoryBar
            {
                InnerText = $"<ul class='navbar-nav ml-auto'>{result}</ul>",
                IsToggled = isToggled,
                IsHasItem = isHasitem
            };
        }
        public static string GetArticleCategoryName(string id)
        {
            using (var service = new ArticleCategoryService())
            {
                ArticleCategory articleCategory = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (articleCategory == null)
                    return string.Empty;
                //
                return articleCategory.Title;
            }
        }
        //#######################################################################################################################################################################################
        public static string GetArticleCategoryForCategory(string selectedId, string templatePage, bool parentAllow = true)
        {
            ArticleCategoryService articleCategoryService = new ArticleCategoryService();
            string result = string.Empty;
            string sqlQuery = "SELECT * FROM App_ArticleCategory WHERE Enabled = @Enabled AND PageTemlate = @PageTemlate ORDER BY ParentID, OrderID ASC";
            List<ArticleCategory> dtList = articleCategoryService.Query<ArticleCategory>(sqlQuery, new { Enabled = (int)WebCore.Model.Enum.ModelEnum.State.ENABLED, PageTemlate = templatePage }).ToList();
            if (dtList.Count() == 0)
                return result;
            //
            List<ArticleCategory> articleCategorys = dtList.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).ToList();
            foreach (var item in articleCategorys)
            {
                SubArticleCategoryBarForCategory subArticleCategoryBarForCategory = SubArticleCategoryForCategory(item.ID, dtList, selectedId);
                string disabled = string.Empty;
                if (!parentAllow && subArticleCategoryBarForCategory.IsSubNull)
                    disabled = "disabled";
                //
                string _innerText = subArticleCategoryBarForCategory.InnerText;
                string _state = string.Empty;
                if (item.ID == selectedId)
                    _state = "checked";
                //
                result += $@"<li><input id='cbx{item.ID}' type='checkbox' class='filled-in' data-id='{item.ID}' {_state} {disabled}/><label for='cbx{item.ID}'>{item.Title}</label></li>{_innerText}";
            }
            return result;
        }

        public static SubArticleCategoryBarForCategory SubArticleCategoryForCategory(string parentId, List<ArticleCategory> allData, string selectedId)
        {
            string result = string.Empty;
            List<WebCore.Entities.ArticleCategory> dtList = allData.Where(m => m.ParentID == parentId).ToList();
            if (dtList.Count == 0)
                return new SubArticleCategoryBarForCategory
                {
                    InnerText = string.Empty,
                    IsSubNull = false
                };
            // 
            foreach (var item in dtList)
            { 
                string toggled = string.Empty;
                SubArticleCategoryBarForCategory subArticleCategoryBarForCategory = SubArticleCategoryForCategory(item.ID, allData, selectedId);
                string disabled = string.Empty;
                if (subArticleCategoryBarForCategory.IsSubNull)
                    disabled = "disabled";
                //
                string _innerText = subArticleCategoryBarForCategory.InnerText;
                string _state = string.Empty;
                if (item.ID == selectedId)
                    _state = "checked";
                //
                result += $@"<li><input id='cbx{item.ID}' data-ischild='true' type='checkbox' class='filled-in' data-id='{item.ID}' {_state} {disabled} /><label for='cbx{item.ID}'>{item.Title}</label></li>{_innerText}";
            }
            //
            return new SubArticleCategoryBarForCategory
            {
                InnerText = $"<ul>{result}</ul>",
                IsSubNull = true
            };
        }
        //#######################################################################################################################################################################################
        public static ArticleCategoryBar GetArticleCategoryBarByAlias(string alias)
        {
            using (var service = new ArticleCategoryService())
            {
                string sqlQuery = @"SELECT TOP 1 * FROM App_ArticleCategory WHERE Alias = @Alias";
                ArticleCategoryBar item = service.Query<ArticleCategoryBar>(sqlQuery, new { Alias = alias }).FirstOrDefault();
                return item;
            }
        }

    }
}