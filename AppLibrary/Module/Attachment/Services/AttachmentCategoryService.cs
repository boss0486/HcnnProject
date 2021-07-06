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
    public interface IAttachmentCategoryService : IEntityService<AttachmentCategory> { }
    public class AttachmentCategoryService : EntityService<AttachmentCategory>, IAttachmentCategoryService
    {
        public AttachmentCategoryService() : base() { }
        public AttachmentCategoryService(System.Data.IDbConnection db) : base(db) { }
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
            AttachmentCategoryService attachmentCategoryService = new AttachmentCategoryService(_connection);
            string langID = Helper.Current.UserLogin.LanguageID;
            whereCondition += $@" AND SiteID = @SiteID AND CreatedBy = @CreatedBy";
            string sqlQuery = $@"SELECT * FROM AttachmentCategory WHERE Title LIKE N'%'+ dbo.Uni2NONE(@Query) +'%' {whereCondition} ORDER BY OrderID ASC ";
            //
            List<AttachmentCategoryResult> attachmentCategoryResults = _connection.Query<AttachmentCategoryResult>(sqlQuery, new
            {
                Query = Helper.Page.Library.FormatNameToUni2NONE(query),
                SiteID = Helper.Current.UserLogin.SiteID,
                CreatedBy = Helper.Current.UserLogin.IdentifierID
            }).ToList();
            if (attachmentCategoryResults.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<AttachmentCategoryResult> dtList = attachmentCategoryResults.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            foreach (var item in dtList)
            {
                item.SubOptionData = SubDataList(item.ID, attachmentCategoryResults);
            }

            List<AttachmentCategoryResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
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

        public ActionResult GetAttachmentCategoryOption(int showLevel = 0)
        {
            AttachmentCategoryService attachmentCategoryService = new AttachmentCategoryService(_connection);
            string langID = Helper.Current.UserLogin.LanguageID;
            string whereCondition = $@" AND SiteID = @SiteID AND CreatedBy = @CreatedBy";
            string sqlQuery = $"SELECT * FROM AttachmentCategory WHERE Enabled = @Enabled {whereCondition} ORDER BY ParentID, OrderID ASC";
            List<AttachmentCategoryOptionModel> attachmentCategoryOptionModels = _connection.Query<AttachmentCategoryOptionModel>(sqlQuery, new
            {
                Enabled = (int)WebCore.Model.Enum.ModelEnum.State.ENABLED,
                SiteID = Helper.Current.UserLogin.SiteID,
                CreatedBy = Helper.Current.UserLogin.IdentifierID
            }).ToList();
            if (attachmentCategoryOptionModels.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<AttachmentCategoryOptionModel> dtList = attachmentCategoryOptionModels.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            foreach (var item in dtList)
            {
                item.IsSub = false;
                item.SubOptionData = SubOptionList(item.ID, attachmentCategoryOptionModels, showLevel).SubData;
            }
            return Notifization.Data(MessageText.Success, dtList);
        }
        public SubAttachmentCategoryOptionModel SubOptionList(string parentId, List<AttachmentCategoryOptionModel> attachmentCategoryResults, int showLevel = 0)
        {
            if (showLevel == 1)
                return null;
            //
            List<AttachmentCategoryOptionModel> dtList = attachmentCategoryResults.Where(m => m.ParentID == parentId).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return null;
            //
            foreach (var item in dtList)
            {
                item.IsSub = true;
                item.SubOptionData = SubOptionList(item.ID, attachmentCategoryResults, showLevel).SubData;
            }
            //
            return new SubAttachmentCategoryOptionModel
            {
                level = showLevel,
                SubData = dtList
            };
        }

        public List<AttachmentCategoryResult> SubDataList(string parentId, List<AttachmentCategoryResult> attachmentCategoryResults)
        {

            List<AttachmentCategoryResult> dtList = attachmentCategoryResults.Where(m => m.ParentID == parentId).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return null;
            //
            foreach (var item in dtList)
            {
                item.IsSub = true;
                item.SubOptionData = SubDataList(item.ID, attachmentCategoryResults);
            }
            return dtList;
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Create(AttachmentCategoryCreateModel model)
        {
            if (model == null)
                return Notifization.Invalid("Dữ liệu không hợp lệ");

            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {

                string title = model.Title;
                string summary = model.Summary;
                string alias = model.Alias;
                string parentId = model.ParentID;
                int sortType = model.OrderID;

                if (string.IsNullOrWhiteSpace(title))
                    return Notifization.Invalid("Không được để trống nhóm tài liệu");
                title = title.Trim();
                if (!Validate.TestText(title))
                    return Notifization.Invalid("Nhóm tài liệu không hợp lệ");
                if (title.Length < 2 || title.Length > 80)
                    return Notifization.Invalid("Nhóm tài liệu giới hạn [2-80] ký tự");
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
                AttachmentCategoryService attachmentCategoryService = new AttachmentCategoryService(_connection);
                if (!string.IsNullOrWhiteSpace(parentId))
                {
                    AttachmentCategory attachmentCategoryIsChild = attachmentCategoryService.GetAlls(m => m.ID == parentId, transaction: _transaction).FirstOrDefault();
                    if (attachmentCategoryIsChild != null && !string.IsNullOrWhiteSpace(attachmentCategoryIsChild.ParentID))
                        return Notifization.Invalid("Nhóm tài liệu giới hạn [1-2] cấp");
                    //
                }
                //
                AttachmentCategory attachmentCategoryTitle = attachmentCategoryService.GetAlls(m => m.Title.ToLower() == title.ToLower(), transaction: _transaction).FirstOrDefault();
                if (attachmentCategoryTitle != null)
                    return Notifization.Invalid("Nhóm tài liệu đã được sử dụng");
                // 
                int orderId = 1;
                if (sortType == (int)AttachmentEnum.Sort.FIRST)
                {
                    List<AttachmentCategory> attachmentCategories = attachmentCategoryService.GetAlls(m => m.ParentID == parentId, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                    if (attachmentCategories.Count > 0)
                    {
                        int cnt = 2;
                        foreach (var item in attachmentCategories)
                        {
                            item.OrderID = cnt;
                            attachmentCategoryService.Update(item, transaction: _transaction);
                            cnt++;
                        }
                    }
                    //
                }
                else
                {
                    orderId = attachmentCategoryService.GetAlls(m => m.ParentID == parentId, transaction: _transaction).Select(t => t.OrderID).DefaultIfEmpty().ToList().Max() + 1;
                }
                //
                AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
                string _controllerText = Helper.Page.MetaSEO.ControllerText.ToLower();
                //
                string id = attachmentCategoryService.Create<string>(new AttachmentCategory()
                {
                    ParentID = parentId,
                    Title = title,
                    Alias = Helper.Page.Library.FormatToUni2NONE(title),
                    Path = "",
                    Summary = summary,
                    OrderID = orderId,
                    Enabled = model.Enabled,
                }, transaction: _transaction);
                // update part
                string strPath = string.Empty;
                AttachmentCategory attachmentCategoryParent = attachmentCategoryService.GetAlls(m => m.ID == parentId, transaction: _transaction).FirstOrDefault();
                if (attachmentCategoryParent != null)
                    strPath = attachmentCategoryParent.Path + "/" + id;
                else
                    strPath = "/" + id;
                //
                attachmentCategoryTitle = attachmentCategoryService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                attachmentCategoryTitle.Path = strPath;
                attachmentCategoryService.Update(attachmentCategoryTitle, transaction: _transaction);

                _transaction.Commit();
                return Notifization.Success(MessageText.CreateSuccess);

            }
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(AttachmentCategoryUpdateModel model)
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
                return Notifization.Invalid("Không được để trống nhóm tài liệu");
            title = title.Trim();
            if (!Validate.TestText(title))
                return Notifization.Invalid("Nhóm tài liệu không hợp lệ");
            if (title.Length < 2 || title.Length > 80)
                return Notifization.Invalid("Nhóm tài liệu giới hạn [2-80] ký tự");
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
            AttachmentCategoryService attachmentCategoryService = new AttachmentCategoryService(_connection);
            string id = model.ID;
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.Invalid(MessageText.Invalid);
            //
            id = id.ToLower();
            AttachmentCategory attachmentCategory = attachmentCategoryService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (attachmentCategory == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            AttachmentCategory attachmentCategoryTitle = attachmentCategoryService.GetAlls(m => m.Title.ToLower() == title.ToLower() && m.ID != id).FirstOrDefault();
            if (attachmentCategoryTitle != null)
                return Notifization.Invalid("Nhóm tài liệu đã được sử dụng");
            //
            string alias = model.Alias;
            string attachmentCategoryPath = attachmentCategory.Path;
            int orderId = attachmentCategory.OrderID;
            // check parentId
            if (string.IsNullOrWhiteSpace(parentId))
            {
                parentId = null;
                attachmentCategoryPath = "/" + id;
            }
            else
            {
                // ko dc chon item hien tai lam cha
                if (attachmentCategory.ID != parentId)
                    parentId = attachmentCategory.ParentID;
                //
                AttachmentCategory mPath = attachmentCategoryService.GetAlls(m => m.ID == parentId).FirstOrDefault();
                if (mPath != null)
                    attachmentCategoryPath = mPath.Path + "/" + id;
                //
            }
            //
            if (sortType == (int)AttachmentEnum.Sort.FIRST)
            {
                orderId = 1;
                List<AttachmentCategory> attachmentCategorys1 = attachmentCategoryService.GetAlls(m => m.ParentID == parentId).OrderBy(m => m.OrderID).ToList();
                if (attachmentCategorys1.Count > 0)
                {
                    int cnt = 2;
                    foreach (var item in attachmentCategorys1)
                    {
                        item.OrderID = cnt;
                        attachmentCategoryService.Update(item);
                        cnt++;
                    }
                }
                //
            }
            if (sortType == (int)AttachmentEnum.Sort.LAST)
            {
                orderId = attachmentCategoryService.GetAlls(m => m.ParentID == parentId).Select(t => t.OrderID).DefaultIfEmpty().ToList().Max() + 1;
            }
            // 
            // update content
            attachmentCategory.ParentID = parentId;
            attachmentCategory.Path = attachmentCategoryPath;
            attachmentCategory.Title = title;
            attachmentCategory.Summary = summary;
            attachmentCategory.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            attachmentCategory.OrderID = orderId;
            attachmentCategory.Enabled = model.Enabled;
            attachmentCategoryService.Update(attachmentCategory);
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public AttachmentCategory GetAttachmentCategoryByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM AttachmentCategory WHERE ID = @Query";
            AttachmentCategory attachmentCategory = _connection.Query<AttachmentCategory>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (attachmentCategory == null)
                return null;
            //
            return attachmentCategory;
        }

        public AttachmentCategoryResult ViewAttachmentCategoryByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM AttachmentCategory WHERE ID = @Query";
            AttachmentCategoryResult viewAttachmentCategory = _connection.Query<AttachmentCategoryResult>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (viewAttachmentCategory == null)
                return null;
            //
            return viewAttachmentCategory;
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(AttachmentCategoryIDModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    string id = model.ID;
                    if (string.IsNullOrWhiteSpace(id))
                        return Notifization.Invalid(MessageText.Invalid);
                    //
                    string query = $@"with AttachmentCategoryTemp as(select ID from AttachmentCategory Where ID = @ID union all select c.ID from AttachmentCategory c inner join AttachmentCategoryTemp mn on c.ParentID = mn.ID)SELECT ID FROM AttachmentCategory  WHERE ID IN (SELECT ID FROM AttachmentCategoryTemp)";
                    List<string> lstId = _connection.Query<string>(query, new { ID = id }, transaction: _transaction).ToList();
                    if (lstId.Count == 0)
                        return Notifization.NotFound(MessageText.NotFound);
                    // delete attachmentCategory
                    string queryAttm = $@"DELETE Attachment WHERE CategoryID IN ('" + String.Join("','", lstId) + "')";
                    _connection.Query(queryAttm, transaction: _transaction);
                    // delete attachmentCategory
                    string queryDelete = $@"with AttachmentCategoryTemp as(select ID from AttachmentCategory Where ID = @ID union all select c.ID from AttachmentCategory c inner join AttachmentCategoryTemp mn on c.ParentID = mn.ID)DELETE AttachmentCategory  WHERE ID IN (SELECT ID FROM AttachmentCategoryTemp)";
                    _connection.Query(queryDelete, new { ID = id }, transaction: _transaction);
                    _transaction.Commit();
                    return Notifization.Success(MessageText.DeleteSuccess);
                }
                catch (Exception ex)
                {
                    _transaction.Rollback();
                    return Notifization.NotService;
                }
            }
        }
        //##############################################################################################################################################################################################################################################################
        public static string DDLAttachmentCategoryLevelLinitOne(string id, string notId = null)
        {
            string result = string.Empty;
            using (var service = new AttachmentCategoryService())
            {
                string whereCondition = string.Empty;
                if (!string.IsNullOrWhiteSpace(notId))
                    whereCondition += " AND ID != @NotID ";
                //
                whereCondition += @" AND SiteID = @SiteID AND CreatedBy = @CreatedBy";
                string query = $@"SELECT * FROM AttachmentCategory WHERE ParentID IS NULL AND Enabled = 1 {whereCondition} ORDER BY OrderID, Title ASC";
                List<AttachmentCategoryOption> dtList = service.Query<AttachmentCategoryOption>(query, new
                {
                    NotID = notId,
                    SiteID = Helper.Current.UserLogin.SiteID,
                    CreatedBy = Helper.Current.UserLogin.IdentifierID
                }).ToList();
                if (dtList.Count == 0)
                    return result;
                //
                foreach (var item in dtList)
                {
                    string select = string.Empty;
                    if (!string.IsNullOrWhiteSpace(id) && item.ID == id.ToLower())
                        select = "selected";
                    result += "<option value='" + item.ID + "'" + select + ">" + item.Title + "</option>";
                }
                return result;
            }
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult SortUp(AttachmentCategoryIDModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    AttachmentCategoryService attachmentCategoryService = new AttachmentCategoryService(_connection);
                    AttachmentCategory attachmentCategory = attachmentCategoryService.GetAlls(m => m.ID == model.ID, transaction: _transaction).FirstOrDefault();
                    if (attachmentCategory == null)
                        return Notifization.Invalid(MessageText.Invalid);
                    int _orderId = attachmentCategory.OrderID;
                    string _parentId = attachmentCategory.ParentID;
                    // list first
                    IList<AttachmentCategorySortModel> lstFirst = new List<AttachmentCategorySortModel>();
                    // list last
                    IList<AttachmentCategorySortModel> lstLast = new List<AttachmentCategorySortModel>();
                    //
                    if (!string.IsNullOrWhiteSpace(_parentId))
                    {
                        List<AttachmentCategory> attachmentCategorys = attachmentCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (attachmentCategorys.Count > 0)
                        {
                            foreach (var item in attachmentCategorys)
                            {
                                // set list first
                                if (item.OrderID < attachmentCategory.OrderID)
                                {
                                    lstFirst.Add(new AttachmentCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > attachmentCategory.OrderID)
                                {
                                    lstLast.Add(new AttachmentCategorySortModel
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
                                        attachmentCategory.OrderID = _cntFirst;
                                        attachmentCategoryService.Update(attachmentCategory, transaction: _transaction);
                                        _cntFirst++;
                                    }
                                    AttachmentCategory itemFirst = attachmentCategoryService.GetAlls(m => m.ID == lstFirst[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    attachmentCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }

                            }
                            else
                            {
                                attachmentCategory.OrderID = 1;
                                attachmentCategoryService.Update(attachmentCategory, transaction: _transaction);
                                _cntFirst++;
                            }
                            //last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count > 0)
                            {
                                foreach (var item in lstLast)
                                {
                                    AttachmentCategory itemLast = attachmentCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    attachmentCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            attachmentCategory.OrderID = 1;
                            attachmentCategoryService.Update(attachmentCategory, transaction: _transaction);
                        }
                    }
                    else
                    {
                        // truong hop cha , _parentId = ""
                        List<AttachmentCategory> attachmentCategorys = attachmentCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (attachmentCategorys.Count > 0)
                        {
                            foreach (var item in attachmentCategorys)
                            {
                                // set list first
                                if (item.OrderID < attachmentCategory.OrderID)
                                {
                                    lstFirst.Add(new AttachmentCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > attachmentCategory.OrderID)
                                {
                                    lstLast.Add(new AttachmentCategorySortModel
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
                                        attachmentCategory.OrderID = _cntFirst;
                                        attachmentCategoryService.Update(attachmentCategory, transaction: _transaction);
                                        _cntFirst++;
                                    }
                                    AttachmentCategory itemFirst = attachmentCategoryService.GetAlls(m => m.ID == lstFirst[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    attachmentCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }

                            }
                            else
                            {
                                attachmentCategory.OrderID = 1;
                                attachmentCategoryService.Update(attachmentCategory, transaction: _transaction);
                                _cntFirst++;
                            }
                            //last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count > 0)
                            {
                                foreach (var item in lstLast)
                                {
                                    AttachmentCategory itemLast = attachmentCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    attachmentCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            attachmentCategory.OrderID = 1;
                            attachmentCategoryService.Update(attachmentCategory, transaction: _transaction);
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
        public ActionResult SortDown(AttachmentCategoryIDModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    AttachmentCategoryService attachmentCategoryService = new AttachmentCategoryService(_connection);
                    AttachmentCategory attachmentCategory = attachmentCategoryService.GetAlls(m => m.ID == model.ID, transaction: _transaction).FirstOrDefault();
                    if (attachmentCategory == null)
                        return Notifization.TEST("::");
                    int _orderId = attachmentCategory.OrderID;
                    string _parentId = attachmentCategory.ParentID;
                    // list first
                    IList<AttachmentCategorySortModel> lstFirst = new List<AttachmentCategorySortModel>();
                    // list last
                    IList<AttachmentCategorySortModel> lstLast = new List<AttachmentCategorySortModel>();
                    //
                    if (!string.IsNullOrWhiteSpace(_parentId))
                    {
                        List<AttachmentCategory> attachmentCategorys = attachmentCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (attachmentCategorys.Count > 0)
                        {
                            foreach (var item in attachmentCategorys)
                            {
                                // set list first
                                if (item.OrderID < attachmentCategory.OrderID)
                                {
                                    lstFirst.Add(new AttachmentCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > attachmentCategory.OrderID)
                                {
                                    lstLast.Add(new AttachmentCategorySortModel
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
                                    AttachmentCategory itemFirst = attachmentCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    attachmentCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }

                            }
                            //  last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count == 1)
                            {
                                AttachmentCategory itemLast = attachmentCategoryService.GetAlls(m => m.ID == lstLast[0].ID, transaction: _transaction).FirstOrDefault();
                                itemLast.OrderID = _cntLast;
                                attachmentCategoryService.Update(itemLast, transaction: _transaction);
                                //
                                attachmentCategory.OrderID = _cntLast + 1;
                                attachmentCategoryService.Update(attachmentCategory, transaction: _transaction);
                                _cntLast++;
                            }
                            else if (lstLast.Count > 1)
                            {
                                for (int i = 0; i < lstLast.Count; i++)
                                {
                                    if (i == 1)
                                    {
                                        attachmentCategory.OrderID = _cntLast;
                                        attachmentCategoryService.Update(attachmentCategory, transaction: _transaction);
                                        _cntLast++;
                                    }
                                    AttachmentCategory itemLast = attachmentCategoryService.GetAlls(m => m.ID == lstLast[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    attachmentCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            attachmentCategory.OrderID = 1;
                            attachmentCategoryService.Update(attachmentCategory, transaction: _transaction);
                        }
                    }
                    else
                    {
                        // truong hop cha , _parentId = ""
                        List<AttachmentCategory> attachmentCategorys = attachmentCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (attachmentCategorys.Count > 0)
                        {
                            foreach (var item in attachmentCategorys)
                            {
                                // set list first
                                if (item.OrderID < attachmentCategory.OrderID)
                                {
                                    lstFirst.Add(new AttachmentCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > attachmentCategory.OrderID)
                                {
                                    lstLast.Add(new AttachmentCategorySortModel
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
                                    AttachmentCategory itemFirst = attachmentCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    attachmentCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }
                            }
                            //  last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count == 1)
                            {
                                AttachmentCategory itemLast = attachmentCategoryService.GetAlls(m => m.ID == lstLast[0].ID, transaction: _transaction).FirstOrDefault();
                                itemLast.OrderID = _cntLast;
                                attachmentCategoryService.Update(itemLast, transaction: _transaction);
                                //
                                attachmentCategory.OrderID = _cntLast + 1;
                                attachmentCategoryService.Update(attachmentCategory, transaction: _transaction);
                                _cntLast++;
                            }
                            else if (lstLast.Count > 1)
                            {
                                for (int i = 0; i < lstLast.Count; i++)
                                {
                                    if (i == 1)
                                    {
                                        attachmentCategory.OrderID = _cntLast;
                                        attachmentCategoryService.Update(attachmentCategory, transaction: _transaction);
                                        _cntLast++;
                                    }
                                    AttachmentCategory itemLast = attachmentCategoryService.GetAlls(m => m.ID == lstLast[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    attachmentCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            attachmentCategory.OrderID = 1;
                            attachmentCategoryService.Update(attachmentCategory, transaction: _transaction);
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
        public static string GetAttachmentCategoryName(string id)
        {
            using (var service = new AttachmentCategoryService())
            {
                AttachmentCategory attachmentCategory = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (attachmentCategory == null)
                    return string.Empty;
                //
                return attachmentCategory.Title;
            }
        }

        //#######################################################################################################################################################################################
        public static string GetAttachmentCategory(string selectedId, bool parentAllow = true)
        {
            AttachmentCategoryService attachmentCategoryService = new AttachmentCategoryService();
            string result = string.Empty;
            string whereCondition = $@" AND SiteID = @SiteID AND CreatedBy = @CreatedBy";
            string sqlQuery = $"SELECT * FROM AttachmentCategory WHERE Enabled = @Enabled {whereCondition} ORDER BY ParentID, OrderID ASC";
            List<AttachmentCategory> dtList = attachmentCategoryService.Query<AttachmentCategory>(sqlQuery, new
            {
                Enabled = (int)WebCore.Model.Enum.ModelEnum.State.ENABLED,
                SiteID = Helper.Current.UserLogin.SiteID,
                CreatedBy = Helper.Current.UserLogin.IdentifierID
            }).ToList();
            if (dtList.Count() == 0)
                return result;
            //
            List<AttachmentCategory> attachmentCategorys = dtList.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).ToList();
            int level = 1;

            foreach (var item in attachmentCategorys)
            {
                SubAttachmentCategoryBarForCategory subAttachmentCategoryBarForCategory = SubAttachmentCategory(item.ID, dtList, selectedId);
                string disabled = string.Empty;
                if (!parentAllow && subAttachmentCategoryBarForCategory.IsSubNull)
                    disabled = "disabled";
                //
                string _innerText = subAttachmentCategoryBarForCategory.InnerText;
                string _state = string.Empty;
                if (item.ID == selectedId)
                    _state = "checked";
                //
                result += $@"<li><input id='cbx{item.ID}' type='checkbox' class='filled-in' data-id='{item.ID}' {_state} {disabled}/><label for='cbx{item.ID}'>{item.Title}</label></li>{_innerText}";
            }
            return result;
        }

        public static SubAttachmentCategoryBarForCategory SubAttachmentCategory(string parentId, List<AttachmentCategory> allData, string selectedId)
        {
            string result = string.Empty;
            List<WebCore.Entities.AttachmentCategory> dtList = allData.Where(m => m.ParentID == parentId).ToList();
            if (dtList.Count == 0)
                return new SubAttachmentCategoryBarForCategory
                {
                    InnerText = string.Empty,
                    IsSubNull = false
                };
            // 
            foreach (var item in dtList)
            {
                string toggled = string.Empty;
                SubAttachmentCategoryBarForCategory subAttachmentCategoryBarForCategory = SubAttachmentCategory(item.ID, allData, selectedId);
                string disabled = string.Empty;
                if (subAttachmentCategoryBarForCategory.IsSubNull)
                    disabled = "disabled";
                //
                string _innerText = subAttachmentCategoryBarForCategory.InnerText;
                string _state = string.Empty;
                if (item.ID == selectedId)
                    _state = "checked";
                //
                result += $@"<li><input id='cbx{item.ID}' data-ischild='true' type='checkbox' class='filled-in' data-id='{item.ID}' {_state} {disabled} /><label for='cbx{item.ID}'>{item.Title}</label></li>{_innerText}";
            }
            //
            return new SubAttachmentCategoryBarForCategory
            {
                InnerText = $"<ul>{result}</ul>",
                IsSubNull = true
            };
        }

        public ActionResult GetAttachmentCategoryJs(string selectedId)
        {
            AttachmentCategoryService attachmentCategoryService = new AttachmentCategoryService();
            string whereCondition = $@" AND SiteID = @SiteID AND CreatedBy = @CreatedBy";
            string sqlQuery = $"SELECT * FROM AttachmentCategory WHERE Enabled = @Enabled {whereCondition} ORDER BY ParentID, OrderID ASC";
            List<AttachmentCategoryOption> allData = attachmentCategoryService.Query<AttachmentCategoryOption>(sqlQuery, new
            {
                Enabled = (int)WebCore.Model.Enum.ModelEnum.State.ENABLED,
                SiteID = Helper.Current.UserLogin.SiteID,
                CreatedBy = Helper.Current.UserLogin.IdentifierID
            }).ToList();
            if (allData.Count() == 0)
                return Notifization.NotFound();
            //
            List<AttachmentCategoryOption> attachmentCategorys = allData.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).ToList();

            List<AttachmentCategoryOption> dtList = new List<AttachmentCategoryOption>();
            foreach (var item in attachmentCategorys)
            {
                item.SubOption = SubAttachmentCategoryJS(item.ID, allData, selectedId);

                if (item.ID == selectedId)
                    item.IsActived = true;
                //
                dtList.Add(item);
            }
            return Notifization.Option(MessageText.Success, data: dtList);
        }

        public static List<AttachmentCategoryOption> SubAttachmentCategoryJS(string parentId, List<AttachmentCategoryOption> allData, string selectedId)
        {
            List<AttachmentCategoryOption> dtList = allData.Where(m => m.ParentID == parentId).ToList();
            if (dtList.Count == 0)
                return new List<AttachmentCategoryOption>();
            // 
            foreach (var item in dtList)
            {
                item.SubOption = SubAttachmentCategoryJS(item.ID, allData, selectedId);
                if (item.ID == selectedId)
                    item.IsActived = true;
                //
            }
            //
            return dtList;
        }

        //#######################################################################################################################################################################################

    }
}