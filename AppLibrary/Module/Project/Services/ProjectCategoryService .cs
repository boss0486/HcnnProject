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
    public interface IProjectCategoryService : IEntityService<ProjectCategory> { }
    public class ProjectCategoryService : EntityService<ProjectCategory>, IProjectCategoryService
    {
        public ProjectCategoryService() : base() { }
        public ProjectCategoryService(System.Data.IDbConnection db) : base(db) { }
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
            ProjectCategoryService ProjectCategoryService = new ProjectCategoryService(_connection);
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = $@"SELECT * FROM App_ProjectCategory WHERE Title LIKE N'%'+ dbo.Uni2NONE(@Query) +'%' {whereCondition} ORDER BY OrderID ASC ";

            List<ProjectCategoryResult> projectCategoryResults = _connection.Query<ProjectCategoryResult>(sqlQuery, new { Query = query }).ToList();
            if (projectCategoryResults.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<ProjectCategoryResult> dtList = projectCategoryResults.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            foreach (var item in dtList)
            {
                item.SubData = SubDataList(item.ID, projectCategoryResults);
            }

            List<ProjectCategoryResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
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

        public ActionResult GetProjectCategoryOption()
        {
            ProjectCategoryService projectCategoryService = new ProjectCategoryService(_connection);
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = $@"SELECT * FROM App_ProjectCategory ORDER BY OrderID ASC ";

            List<ProjectCategoryResult> projectCategoryResults = _connection.Query<ProjectCategoryResult>(sqlQuery).ToList();
            if (projectCategoryResults.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<ProjectCategoryResult> dtList = projectCategoryResults.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            foreach (var item in dtList)
            {
                item.IsSub = false;
                item.SubData = SubDataList(item.ID, projectCategoryResults);
            }
            return Notifization.Data(MessageText.Success, dtList);
        }
        public List<ProjectCategoryResult> SubDataList(string parentId, List<ProjectCategoryResult> projectCategoryResults)
        {
            List<ProjectCategoryResult> dtList = projectCategoryResults.Where(m => m.ParentID == parentId).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return null;
            //
            foreach (var item in dtList)
            {
                item.IsSub = true;
                item.SubData = SubDataList(item.ID, projectCategoryResults);
            }
            return dtList;
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Create(ProjectCategoryCreateModel model)
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
            ProjectCategoryService projectCategoryService = new ProjectCategoryService(_connection);
            if (string.IsNullOrWhiteSpace(parentId))
            {
                parentId = null;
            }

            ProjectCategory ProjectCategory = projectCategoryService.GetAlls(m => m.Title.ToLower() == title.ToLower()).FirstOrDefault();
            if (ProjectCategory != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            // 
            int orderId = 1;
            if (sortType == (int)ProjectCategoryEnum.ProjectSort.FIRST)
            {
                List<ProjectCategory> projectCategorys = projectCategoryService.GetAlls(m => m.ParentID == parentId).OrderBy(m => m.OrderID).ToList();
                if (projectCategorys.Count > 0)
                {
                    int cnt = 2;
                    foreach (var item in projectCategorys)
                    {
                        item.OrderID = cnt;
                        projectCategoryService.Update(item);
                        cnt++;
                    }
                }
                //
            }
            else
            {
                orderId = projectCategoryService.GetAlls(m => m.ParentID == parentId).Select(t => t.OrderID).DefaultIfEmpty().ToList().Max() + 1;
            }
            //
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            string _controllerText = Helper.Page.MetaSEO.ControllerText.ToLower();
            //
            string imgFile = model.ImageFile;
            string id = projectCategoryService.Create<string>(new ProjectCategory()
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
            ProjectCategory ProjectCategoryParent = projectCategoryService.GetAlls(m => m.ID == parentId).FirstOrDefault();
            if (ProjectCategoryParent != null)
                strPath = ProjectCategoryParent.Path + "/" + id;
            else
                strPath = "/" + id;
            //
            ProjectCategory = projectCategoryService.GetAlls(m => m.ID == id).FirstOrDefault();
            ProjectCategory.Path = strPath;
            projectCategoryService.Update(ProjectCategory);

            return Notifization.Success(MessageText.CreateSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(ProjectCategoryUpdateModel model)
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
            ProjectCategoryService projectCategoryService = new ProjectCategoryService(_connection);
            string id = model.ID;
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.Invalid(MessageText.Invalid);
            //
            id = id.ToLower();
            ProjectCategory projectCategory = projectCategoryService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (projectCategory == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            ProjectCategory projectCategoryTitle = projectCategoryService.GetAlls(m => m.Title.ToLower() == title.ToLower() && m.ID != id).FirstOrDefault();
            if (projectCategoryTitle != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            //
            string alias = model.Alias;
            string projectCategoryPath = projectCategory.Path;
            int orderId = projectCategory.OrderID;
            // check parentId
            if (string.IsNullOrWhiteSpace(parentId))
            {
                parentId = null;
                projectCategoryPath = "/" + id;
            }
            else
            {
                //if (ProjectCategory.ID != ProjectCategory.ParentID)
                //    parentId = ProjectCategory.ParentID;
                ////
                ProjectCategory mPath = projectCategoryService.GetAlls(m => m.ID == parentId).FirstOrDefault();
                if (mPath != null)
                    projectCategoryPath = mPath.Path + "/" + id;
            }
            //
            if (sortType == (int)ProjectCategoryEnum.ProjectSort.FIRST)
            {
                orderId = 1;
                List<ProjectCategory> projectCategorys1 = projectCategoryService.GetAlls(m => m.ParentID == parentId).OrderBy(m => m.OrderID).ToList();
                if (projectCategorys1.Count > 0)
                {
                    int cnt = 2;
                    foreach (var item in projectCategorys1)
                    {
                        item.OrderID = cnt;
                        projectCategoryService.Update(item);
                        cnt++;
                    }
                }
                //
            }
            if (sortType == (int)ProjectCategoryEnum.ProjectSort.LAST)
            { 
                orderId = projectCategoryService.GetAlls(m => m.ParentID == parentId).Select(t => t.OrderID).DefaultIfEmpty().ToList().Max() + 1;
            }
            // 
            string _controllerText = Helper.Page.MetaSEO.ControllerText.ToLower();
            string imgFile = model.ImageFile;
            // update content
            projectCategory.ParentID = parentId;
            projectCategory.Path = projectCategoryPath;
            projectCategory.IconFont = model.IconFont;
            projectCategory.ImageFile = imgFile;
            projectCategory.Title = title;
            projectCategory.Summary = summary;
            projectCategory.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            projectCategory.BackLink = model.BackLink;
            projectCategory.OrderID = orderId;
            projectCategory.Enabled = model.Enabled;
            projectCategoryService.Update(projectCategory);
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public ProjectCategory GetProjectCategoryByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_ProjectCategory WHERE ID = @Query";
            ProjectCategory projectCategory = _connection.Query<ProjectCategory>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (projectCategory == null)
                return null;
            //
            return projectCategory;
        }

        public ViewProjectCategory ViewProjectCategoryByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_ProjectCategory WHERE ID = @Query";
            ViewProjectCategory projectCategoryResult = _connection.Query<ViewProjectCategory>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (projectCategoryResult == null)
                return null;
            //
            return projectCategoryResult;
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(ProjectCategoryIDModel model)
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
                        ProjectCategoryService projectCategoryService = new ProjectCategoryService(_connectDb);
                        ProjectCategory projectCategory = projectCategoryService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                        if (projectCategory == null)
                            return Notifization.Invalid(MessageText.Invalid);
                        // remote ProjectCategory, children ProjectCategory
                        ////string sqlQuery = @"
                        ////        WITH ProjectCategoryTemp AS
                        ////        (select ID,ImageFile  
                        ////         from App_ProjectCategory
                        ////         where  ID  =  @ID
                        ////         union all
                        ////         select c.ID, c.ImageFile
                        ////         from App_ProjectCategory c
                        ////            inner join ProjectCategoryTemp mn on c.ParentID = mn.ID
                        ////        )SELECT ID,ImageFile FROM ProjectCategoryTemp ";
                        ////List<ProjectCategoryDeleteAndAtactmentModel> ProjectCategoryDeleteAndAtactmentModels = _connectDb.Query<ProjectCategoryDeleteAndAtactmentModel>(sqlQuery, new { ID = model.ID }, transaction: _transaction).ToList();
                        ////// delete file
                        ////if (ProjectCategoryDeleteAndAtactmentModels.Count > 0)
                        ////{
                        ////    foreach (var item in ProjectCategoryDeleteAndAtactmentModels)
                        ////        AttachmentFile.DeleteFile(item.ImageFile, transaction: _transaction);
                        ////}
                        // delete ProjectCategory
                        string queryDelete = $@"with ProjectCategoryTemp as(select ID from App_ProjectCategory Where ID = @ID union all select c.ID from App_ProjectCategory c inner join ProjectCategoryTemp mn on c.ParentID = mn.ID)DELETE App_ProjectCategory  WHERE ID IN (SELECT ID FROM ProjectCategoryTemp)";
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
        public ActionResult SortUp(ProjectCategoryIDModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    ProjectCategoryService projectCategoryService = new ProjectCategoryService(_connection);
                    ProjectCategory projectCategory = projectCategoryService.GetAlls(m => m.ID == model.ID, transaction: _transaction).FirstOrDefault();
                    if (projectCategory == null)
                        return Notifization.Invalid(MessageText.Invalid);
                    int _orderId = projectCategory.OrderID;
                    string _parentId = projectCategory.ParentID;
                    // list first
                    IList<ProjectCategorySortModel> lstFirst = new List<ProjectCategorySortModel>();
                    // list last
                    IList<ProjectCategorySortModel> lstLast = new List<ProjectCategorySortModel>();
                    //
                    if (!string.IsNullOrWhiteSpace(_parentId))
                    {
                        List<ProjectCategory> ProjectCategorys = projectCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (ProjectCategorys.Count > 0)
                        {
                            foreach (var item in ProjectCategorys)
                            {
                                // set list first
                                if (item.OrderID < projectCategory.OrderID)
                                {
                                    lstFirst.Add(new ProjectCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > projectCategory.OrderID)
                                {
                                    lstLast.Add(new ProjectCategorySortModel
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
                                        projectCategory.OrderID = _cntFirst;
                                        projectCategoryService.Update(projectCategory, transaction: _transaction);
                                        _cntFirst++;
                                    }
                                    ProjectCategory itemFirst = projectCategoryService.GetAlls(m => m.ID == lstFirst[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    projectCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }

                            }
                            else
                            {
                                projectCategory.OrderID = 1;
                                projectCategoryService.Update(projectCategory, transaction: _transaction);
                                _cntFirst++;
                            }
                            //last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count > 0)
                            {
                                foreach (var item in lstLast)
                                {
                                    ProjectCategory itemLast = projectCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    projectCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            projectCategory.OrderID = 1;
                            projectCategoryService.Update(projectCategory, transaction: _transaction);
                        }
                    }
                    else
                    {
                        // truong hop cha , _parentId = ""
                        List<ProjectCategory> projectCategorys = projectCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (projectCategorys.Count > 0)
                        {
                            foreach (var item in projectCategorys)
                            {
                                // set list first
                                if (item.OrderID < projectCategory.OrderID)
                                {
                                    lstFirst.Add(new ProjectCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > projectCategory.OrderID)
                                {
                                    lstLast.Add(new ProjectCategorySortModel
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
                                        projectCategory.OrderID = _cntFirst;
                                        projectCategoryService.Update(projectCategory, transaction: _transaction);
                                        _cntFirst++;
                                    }
                                    ProjectCategory itemFirst = projectCategoryService.GetAlls(m => m.ID == lstFirst[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    projectCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }

                            }
                            else
                            {
                                projectCategory.OrderID = 1;
                                projectCategoryService.Update(projectCategory, transaction: _transaction);
                                _cntFirst++;
                            }
                            //last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count > 0)
                            {
                                foreach (var item in lstLast)
                                {
                                    ProjectCategory itemLast = projectCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    projectCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            projectCategory.OrderID = 1;
                            projectCategoryService.Update(projectCategory, transaction: _transaction);
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
        public ActionResult SortDown(ProjectCategoryIDModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    ProjectCategoryService projectCategoryService = new ProjectCategoryService(_connection);
                    ProjectCategory projectCategory = projectCategoryService.GetAlls(m => m.ID == model.ID, transaction: _transaction).FirstOrDefault();
                    if (projectCategory == null)
                        return Notifization.TEST("::");
                    int _orderId = projectCategory.OrderID;
                    string _parentId = projectCategory.ParentID;
                    // list first
                    IList<ProjectCategorySortModel> lstFirst = new List<ProjectCategorySortModel>();
                    // list last
                    IList<ProjectCategorySortModel> lstLast = new List<ProjectCategorySortModel>();
                    //
                    if (!string.IsNullOrWhiteSpace(_parentId))
                    {
                        List<ProjectCategory> projectCategorys = projectCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (projectCategorys.Count > 0)
                        {
                            foreach (var item in projectCategorys)
                            {
                                // set list first
                                if (item.OrderID < projectCategory.OrderID)
                                {
                                    lstFirst.Add(new ProjectCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > projectCategory.OrderID)
                                {
                                    lstLast.Add(new ProjectCategorySortModel
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
                                    ProjectCategory itemFirst = projectCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    projectCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }

                            }
                            //  last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count == 1)
                            {
                                ProjectCategory itemLast = projectCategoryService.GetAlls(m => m.ID == lstLast[0].ID, transaction: _transaction).FirstOrDefault();
                                itemLast.OrderID = _cntLast;
                                projectCategoryService.Update(itemLast, transaction: _transaction);
                                //
                                projectCategory.OrderID = _cntLast + 1;
                                projectCategoryService.Update(projectCategory, transaction: _transaction);
                                _cntLast++;
                            }
                            else if (lstLast.Count > 1)
                            {
                                for (int i = 0; i < lstLast.Count; i++)
                                {
                                    if (i == 1)
                                    {
                                        projectCategory.OrderID = _cntLast;
                                        projectCategoryService.Update(projectCategory, transaction: _transaction);
                                        _cntLast++;
                                    }
                                    ProjectCategory itemLast = projectCategoryService.GetAlls(m => m.ID == lstLast[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    projectCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            projectCategory.OrderID = 1;
                            projectCategoryService.Update(projectCategory, transaction: _transaction);
                        }
                    }
                    else
                    {
                        // truong hop cha , _parentId = ""
                        List<ProjectCategory> projectCategorys = projectCategoryService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (projectCategorys.Count > 0)
                        {
                            foreach (var item in projectCategorys)
                            {
                                // set list first
                                if (item.OrderID < projectCategory.OrderID)
                                {
                                    lstFirst.Add(new ProjectCategorySortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > projectCategory.OrderID)
                                {
                                    lstLast.Add(new ProjectCategorySortModel
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
                                    ProjectCategory itemFirst = projectCategoryService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    projectCategoryService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }
                            }
                            //  last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count == 1)
                            {
                                ProjectCategory itemLast = projectCategoryService.GetAlls(m => m.ID == lstLast[0].ID, transaction: _transaction).FirstOrDefault();
                                itemLast.OrderID = _cntLast;
                                projectCategoryService.Update(itemLast, transaction: _transaction);
                                //
                                projectCategory.OrderID = _cntLast + 1;
                                projectCategoryService.Update(projectCategory, transaction: _transaction);
                                _cntLast++;
                            }
                            else if (lstLast.Count > 1)
                            {
                                for (int i = 0; i < lstLast.Count; i++)
                                {
                                    if (i == 1)
                                    {
                                        projectCategory.OrderID = _cntLast;
                                        projectCategoryService.Update(projectCategory, transaction: _transaction);
                                        _cntLast++;
                                    }
                                    ProjectCategory itemLast = projectCategoryService.GetAlls(m => m.ID == lstLast[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    projectCategoryService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            projectCategory.OrderID = 1;
                            projectCategoryService.Update(projectCategory, transaction: _transaction);
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
        public static List<ProjectCategoryBar> GetProjectCategoryBar()
        {
            string sqlQuery = "SELECT * FROM App_ProjectCategory WHERE Enabled = @Enabled ORDER BY OrderID ASC";
            ProjectCategoryService projectCategoryService = new ProjectCategoryService();
            List<ProjectCategoryBar> dtList = projectCategoryService.Query<ProjectCategoryBar>(sqlQuery, new { Enabled = (int)WebCore.Model.Enum.ModelEnum.State.ENABLED }).ToList();
            return dtList;
        }
        public static SubCategoryBar SubProjectCategoryBar(string parentId, List<ProjectCategoryBar> allData, string controllerText)
        {
            bool isToggled = false;
            bool isHasitem = false;
            string result = string.Empty;
            List<ProjectCategoryBar> dtList = allData.Where(m => m.ParentID == parentId).ToList();
            if (dtList.Count == 0)
                return new SubCategoryBar();
            // 
            foreach (var item in dtList)
            {
                string _cls = "";
                string toggled = string.Empty;
                SubCategoryBar subProjectCategoryBar = SubProjectCategoryBar(item.ID, allData, controllerText);
                if (subProjectCategoryBar == null)
                    continue;
                //
                string subClass = string.Empty;
                string link = item.BackLink;
                if (subProjectCategoryBar.IsHasItem)
                    subClass = "ProjectCategory-has-children";
                // 
                //if (string.IsNullOrWhiteSpace(link))
                //    link = $"/{item.UrlRoot}/{item.Alias}";
                ////
                result += $"<li class='{subClass}'><a href='{link}' class='{_cls}  {toggled}'>{item.Title}</a>{subProjectCategoryBar.InnerText}</li>";
            }
            //
            if (!string.IsNullOrEmpty(result))
                isHasitem = true;
            //
            return new SubCategoryBar
            {
                InnerText = $"<ul class='navbar-nav ml-auto'>{result}</ul>",
                IsToggled = isToggled,
                IsHasItem = isHasitem
            };
        }
        public static string GetProjectCategoryName(string id)
        {
            using (var service = new ProjectCategoryService())
            {
                ProjectCategory projectCategory = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (projectCategory == null)
                    return string.Empty;
                //
                return projectCategory.Title;
            }
        }
        //#######################################################################################################################################################################################
        public static string GetDataForCategory(string selectedId, string templatePage, bool parentAllow = true)
        {
            ProjectCategoryService projectCategoryService = new ProjectCategoryService();
            string result = string.Empty;
            string sqlQuery = "SELECT * FROM App_ProjectCategory WHERE Enabled = @Enabled AND PageTemlate = @PageTemlate ORDER BY ParentID, OrderID ASC";
            List<ProjectCategory> dtList = projectCategoryService.Query<ProjectCategory>(sqlQuery, new { Enabled = (int)WebCore.Model.Enum.ModelEnum.State.ENABLED, PageTemlate = templatePage }).ToList();
            if (dtList.Count() == 0)
                return result;
            //
            List<ProjectCategory> projectCategorys = dtList.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).ToList();
            foreach (var item in projectCategorys)
            {
                SubBarForCategory subProjectCategoryBarForCategory = SubDataForCategory(item.ID, dtList, selectedId);
                string disabled = string.Empty;
                if (!parentAllow && subProjectCategoryBarForCategory.IsSubNull)
                    disabled = "disabled";
                //
                string _innerText = subProjectCategoryBarForCategory.InnerText;
                string _state = string.Empty;
                if (item.ID == selectedId)
                    _state = "checked";
                //
                result += $@"<li><input id='cbx{item.ID}' type='checkbox' class='filled-in' data-id='{item.ID}' {_state} {disabled}/><label for='cbx{item.ID}'>{item.Title}</label></li>{_innerText}";
            }
            return result;
        }

        public static SubBarForCategory SubDataForCategory(string parentId, List<ProjectCategory> allData, string selectedId)
        {
            string result = string.Empty;
            List<WebCore.Entities.ProjectCategory> dtList = allData.Where(m => m.ParentID == parentId).ToList();
            if (dtList.Count == 0)
                return new SubBarForCategory
                {
                    InnerText = string.Empty,
                    IsSubNull = false
                };
            // 
            foreach (var item in dtList)
            { 
                string toggled = string.Empty;
                SubBarForCategory subProjectCategoryBarForCategory = SubDataForCategory(item.ID, allData, selectedId);
                string disabled = string.Empty;
                if (subProjectCategoryBarForCategory.IsSubNull)
                    disabled = "disabled";
                //
                string _innerText = subProjectCategoryBarForCategory.InnerText;
                string _state = string.Empty;
                if (item.ID == selectedId)
                    _state = "checked";
                //
                result += $@"<li><input id='cbx{item.ID}' data-ischild='true' type='checkbox' class='filled-in' data-id='{item.ID}' {_state} {disabled} /><label for='cbx{item.ID}'>{item.Title}</label></li>{_innerText}";
            }
            //
            return new SubBarForCategory
            {
                InnerText = $"<ul>{result}</ul>",
                IsSubNull = true
            };
        }
        //#######################################################################################################################################################################################
        public static ProjectCategoryBar GetDataBarByAlias(string alias)
        {
            using (var service = new ProjectCategoryService())
            {
                string sqlQuery = @"SELECT TOP 1 * FROM App_ProjectCategory WHERE Alias = @Alias";
                ProjectCategoryBar item = service.Query<ProjectCategoryBar>(sqlQuery, new { Alias = alias }).FirstOrDefault();
                return item;
            }
        }

    }
}