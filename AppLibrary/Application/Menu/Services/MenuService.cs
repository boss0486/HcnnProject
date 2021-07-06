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
    public interface IMenuService : IEntityService<Menu> { }
    public class MenuService : EntityService<Menu>, IMenuService
    {
        public MenuService() : base() { }
        public MenuService(System.Data.IDbConnection db) : base(db) { }
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
            MenuService menuService = new MenuService(_connection);
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = $@"SELECT * FROM App_Menu WHERE Title LIKE N'%'+ dbo.Uni2NONE(@Query) +'%' {whereCondition} ORDER BY OrderID ASC ";

            List<MenuResult> dataAll = _connection.Query<MenuResult>(sqlQuery, new { Query = query }).ToList();
            if (dataAll.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<MenuResult> dtList = dataAll.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            foreach (var item in dtList)
            {
                item.SubMenu = SubDataList(item.ID, dataAll);
            }

            List<MenuResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
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

        internal static string GetPath(string pageTemplate)
        {
            string result = string.Empty;
            MenuPageTypeOptionModel menuPageTypeOptionModel = MenuService.PageTemplateData().Where(m => m.ID == pageTemplate).FirstOrDefault();
            if (menuPageTypeOptionModel == null)
                return result;
            //
            string alias = menuPageTypeOptionModel.Alias;
            if (!string.IsNullOrWhiteSpace(alias))
                result = $"{alias}";
            //
            return result;
        }

        public ActionResult GetMenuOption()
        {
            MenuService menuService = new MenuService(_connection);
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = $@"SELECT * FROM App_Menu ORDER BY OrderID ASC ";

            List<MenuResult> menuResults = _connection.Query<MenuResult>(sqlQuery).ToList();
            if (menuResults.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<MenuResult> dtList = menuResults.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            foreach (var item in dtList)
            {
                item.IsSub = false;
                item.SubMenu = SubDataList(item.ID, menuResults);
            }
            return Notifization.Data(MessageText.Success, dtList);
        }
        public List<MenuResult> SubDataList(string parentId, List<MenuResult> menuResults)
        {
            List<MenuResult> dtList = menuResults.Where(m => m.ParentID == parentId).OrderBy(m => m.OrderID).ToList();
            if (dtList.Count == 0)
                return null;
            //
            foreach (var item in dtList)
            {
                item.IsSub = true;
                item.SubMenu = SubDataList(item.ID, menuResults);
            }
            return dtList;
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Create(MenuCreateModel model)
        {
            if (model == null)
                return Notifization.Invalid("Dữ liệu không hợp lệ");
            string title = model.Title;
            string summary = model.Summary;
            string alias = model.Alias;
            string parentId = model.ParentID;
            int sortType = model.OrderID;
            string templatePage = model.PageTemlate;

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
            MenuService menuService = new MenuService(_connection);
            if (string.IsNullOrWhiteSpace(parentId))
            {
                parentId = null;
                if (string.IsNullOrWhiteSpace(templatePage))
                    return Notifization.Invalid("Vui lòng chọn mẫu hiển thị");
                //// check template
                Menu menuTemplate = menuService.GetAlls(m => string.IsNullOrWhiteSpace(parentId) && m.PageTemlate == templatePage).FirstOrDefault();
                if (menuTemplate != null)
                    return Notifization.Invalid("Mẫu hiển thị đã được sử dụng");
                //
            }
            else
            {
                Menu menuParent1 = menuService.GetAlls(m => m.ID == parentId).FirstOrDefault();
                if (menuParent1 != null)
                {
                    templatePage = menuParent1.PageTemlate;
                }
            }


            Menu menu = menuService.GetAlls(m => m.Title.ToLower() == title.ToLower()).FirstOrDefault();
            if (menu != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            // 
            int orderId = 1;
            if (sortType == (int)MenuItemEnum.SortType.FIRST)
            {
                List<Menu> menus = menuService.GetAlls(m => m.ParentID == parentId).OrderBy(m => m.OrderID).ToList();
                if (menus.Count > 0)
                {
                    int cnt = 2;
                    foreach (var item in menus)
                    {
                        item.OrderID = cnt;
                        menuService.Update(item);
                        cnt++;
                    }
                }
                //
            }
            else
            {
                orderId = menuService.GetAlls(m => m.ParentID == parentId).Select(t => t.OrderID).DefaultIfEmpty().ToList().Max() + 1;
            }
            //
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            string _controllerText = Helper.Page.MetaSEO.ControllerText.ToLower();
            //
            string imgFile = model.ImageFile;
            string id = menuService.Create<string>(new Menu()
            {
                ParentID = parentId,
                Title = model.Title,
                Alias = Helper.Page.Library.FormatToUni2NONE(title),
                Path = "",
                Summary = summary,
                IconFont = model.IconFont,
                ImageFile = imgFile,
                PageTemlate = templatePage,
                OrderID = orderId,
                LocationID = model.LocationID,
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
            Menu menuPath = menuService.GetAlls(m => m.ID == parentId).FirstOrDefault();
            if (menuPath != null)
                strPath = menuPath.Path + "/" + id;
            else
                strPath = "/" + id;
            //
            menu = menuService.GetAlls(m => m.ID == id).FirstOrDefault();
            menu.Path = strPath;
            menuService.Update(menu);

            return Notifization.Success(MessageText.CreateSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(MenuUpdateModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            string title = model.Title;
            string summary = model.Summary;
            int sortType = model.OrderID;
            string templatePage = model.PageTemlate;
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
            MenuService menuService = new MenuService(_connection);
            string id = model.ID;
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.Invalid(MessageText.Invalid);
            //
            id = id.ToLower();
            Menu menu = menuService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (menu == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            Menu menuTitle = menuService.GetAlls(m => m.Title.ToLower() == title.ToLower() && m.ID != id).FirstOrDefault();
            if (menuTitle != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            //
            string alias = model.Alias;
            string menuPath = menu.Path;
            int orderId = menu.OrderID;
            // check parentId
            if (string.IsNullOrWhiteSpace(parentId))
            {
                if (parentId == id)
                    return Notifization.Invalid("Danh mục không hợp lệ");
                // 
                if (string.IsNullOrWhiteSpace(templatePage))
                    return Notifization.Invalid("Vui lòng chọn mẫu hiển thị");
                // check template
                Menu menuTemplate = menuService.GetAlls(m => string.IsNullOrWhiteSpace(parentId) && m.PageTemlate == templatePage).FirstOrDefault();
                if (menuTemplate != null)
                    return Notifization.Invalid("Mẫu hiển thị đã được sử dụng");
                //
                parentId = null;
                menuPath = "/" + id;
            }
            else
            {
                //if (menu.ID != menu.ParentID)
                //    parentId = menu.ParentID;
                ////
                Menu mPath = menuService.GetAlls(m => m.ID == parentId).FirstOrDefault();
                if (mPath != null)
                    menuPath = mPath.Path + "/" + id;
                //
                Menu menuParent1 = menuService.GetAlls(m => m.ID == parentId).FirstOrDefault();
                if (menuParent1 != null)
                    templatePage = menuParent1.PageTemlate;
                //
            }
            //
            if (sortType == (int)MenuItemEnum.SortType.FIRST)
            {
                orderId = 1;
                List<Menu> menus1 = menuService.GetAlls(m => m.ParentID == parentId).OrderBy(m => m.OrderID).ToList();
                if (menus1.Count > 0)
                {
                    int cnt = 2;
                    foreach (var item in menus1)
                    {
                        item.OrderID = cnt;
                        menuService.Update(item);
                        cnt++;
                    }
                }
                //
            }
            if (sortType == (int)MenuItemEnum.SortType.LAST)
            {
                orderId = menuService.GetAlls(m => m.ParentID == parentId).Select(t => t.OrderID).DefaultIfEmpty().ToList().Max() + 1;
            }
            // 
            string _controllerText = Helper.Page.MetaSEO.ControllerText.ToLower();
            string imgFile = model.ImageFile;
            // update content
            menu.ParentID = parentId;
            menu.Path = menuPath;
            menu.IconFont = model.IconFont;
            menu.ImageFile = imgFile;
            menu.Title = title;
            menu.Summary = summary;
            menu.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            menu.PageTemlate = templatePage;
            menu.BackLink = model.BackLink;
            menu.LocationID = (int)MenuEnum.Location.MAIN;
            menu.OrderID = orderId;
            menu.Enabled = model.Enabled;
            menuService.Update(menu);
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public Menu GetMenuByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_Menu WHERE ID = @Query";
            Menu menu = _connection.Query<Menu>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (menu == null)
                return null;
            //
            return menu;
        }

        public ViewMenu ViewMenuByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_Menu WHERE ID = @Query";
            ViewMenu menuResult = _connection.Query<ViewMenu>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (menuResult == null)
                return null;
            //
            return menuResult;
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(MenuIDModel model)
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
                        MenuService menuService = new MenuService(_connectDb);
                        Menu menu = menuService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                        if (menu == null)
                            return Notifization.Invalid(MessageText.Invalid);
                        // remote menu, children menu
                        ////string sqlQuery = @"
                        ////        WITH MenuTemp AS
                        ////        (select ID,ImageFile  
                        ////         from App_Menu
                        ////         where  ID  =  @ID
                        ////         union all
                        ////         select c.ID, c.ImageFile
                        ////         from App_Menu c
                        ////            inner join MenuTemp mn on c.ParentID = mn.ID
                        ////        )SELECT ID,ImageFile FROM MenuTemp ";
                        ////List<MenuDeleteAndAtactmentModel> menuDeleteAndAtactmentModels = _connectDb.Query<MenuDeleteAndAtactmentModel>(sqlQuery, new { ID = model.ID }, transaction: _transaction).ToList();
                        ////// delete file
                        ////if (menuDeleteAndAtactmentModels.Count > 0)
                        ////{
                        ////    foreach (var item in menuDeleteAndAtactmentModels)
                        ////        AttachmentFile.DeleteFile(item.ImageFile, transaction: _transaction);
                        ////}
                        // delete menu
                        string queryDelete = $@"with MenuTemp as(select ID from App_Menu Where ID = @ID union all select c.ID from App_Menu c inner join MenuTemp mn on c.ParentID = mn.ID)DELETE App_Menu  WHERE ID IN (SELECT ID FROM MenuTemp)";
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

        public static List<MenuPageTypeOptionModel> StacticPageData()
        {
            List<MenuPageTypeOptionModel> menuTypeOptionModels = new List<MenuPageTypeOptionModel>{
                new MenuPageTypeOptionModel("Home", "Trang chủ","trang-chu"),
                new MenuPageTypeOptionModel("About", "Giới thiệu","gioi-thieu"),
                new MenuPageTypeOptionModel("Quotation", "Báo giá","bao-gia"),
                new MenuPageTypeOptionModel("Contact", "Liên hệ","lien-he"),
                new MenuPageTypeOptionModel("Faq", "Hỏi đáp","hoi-dap")
            };
            return menuTypeOptionModels;
        }
        public static List<MenuPageTypeOptionModel> PageTemplateData()
        {
            List<MenuPageTypeOptionModel> menuTypeOptionModels = new List<MenuPageTypeOptionModel>{
                new MenuPageTypeOptionModel("Home", "Trang chủ","trang-chu"),
                new MenuPageTypeOptionModel("About", "Giới thiệu","gioi-thieu"),
                new MenuPageTypeOptionModel("Article", "Bài viết","bai-viet"),
                new MenuPageTypeOptionModel("Product", "Sản phẩm","san-pham"),
                new MenuPageTypeOptionModel("Project", "Dự án","du-an"),
                //new MenuPageTypeOptionModel("Service", "Dịch vụ"),
                //new MenuPageTypeOptionModel("Recruitment", "Tuyển dụng"),
                new MenuPageTypeOptionModel("Quotation", "Báo giá","bao-gia"),
                new MenuPageTypeOptionModel("Contact", "Liên hệ","lien-he"),
                new MenuPageTypeOptionModel("Faq", "Hỏi đáp","hoi-dap")
            };
            return menuTypeOptionModels;
        }
        public static string DDLPageTemplate(string id)
        {
            List<MenuPageTypeOptionModel> menuTypeOptionModels = MenuService.PageTemplateData();
            string result = string.Empty;
            foreach (var item in menuTypeOptionModels)
            {
                string selected = string.Empty;
                if (!string.IsNullOrWhiteSpace(id) && item.ID == id)
                    selected = "selected";

                result += "<option value='" + item.ID + "' " + selected + ">" + item.Title + "</option>";
            }
            return result;
        }
        public static string DDLStaticPage(string id)
        {
            List<MenuPageTypeOptionModel> menuTypeOptionModels = MenuService.StacticPageData();
            string result = string.Empty;
            foreach (var item in menuTypeOptionModels)
            {
                string selected = string.Empty;
                if (!string.IsNullOrWhiteSpace(id) && item.ID == id)
                    selected = "selected";

                result += "<option value='" + item.ID + "' " + selected + ">" + item.Title + "</option>";
            }
            return result;
        }

        //##############################################################################################################################################################################################################################################################
        public ActionResult SortUp(MenuItemIDModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    MenuService menuService = new MenuService(_connection);
                    Menu menu = menuService.GetAlls(m => m.ID == model.ID, transaction: _transaction).FirstOrDefault();
                    if (menu == null)
                        return Notifization.Invalid(MessageText.Invalid);
                    int _orderId = menu.OrderID;
                    string _parentId = menu.ParentID;
                    // list first
                    IList<MenuSortModel> lstFirst = new List<MenuSortModel>();
                    // list last
                    IList<MenuSortModel> lstLast = new List<MenuSortModel>();
                    //
                    if (!string.IsNullOrWhiteSpace(_parentId))
                    {
                        List<Menu> menus = menuService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (menus.Count > 0)
                        {
                            foreach (var item in menus)
                            {
                                // set list first
                                if (item.OrderID < menu.OrderID)
                                {
                                    lstFirst.Add(new MenuSortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > menu.OrderID)
                                {
                                    lstLast.Add(new MenuSortModel
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
                                        menu.OrderID = _cntFirst;
                                        menuService.Update(menu, transaction: _transaction);
                                        _cntFirst++;
                                    }
                                    Menu itemFirst = menuService.GetAlls(m => m.ID == lstFirst[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    menuService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }

                            }
                            else
                            {
                                menu.OrderID = 1;
                                menuService.Update(menu, transaction: _transaction);
                                _cntFirst++;
                            }
                            //last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count > 0)
                            {
                                foreach (var item in lstLast)
                                {
                                    Menu itemLast = menuService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    menuService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            menu.OrderID = 1;
                            menuService.Update(menu, transaction: _transaction);
                        }
                    }
                    else
                    {
                        // truong hop cha , _parentId = ""
                        List<Menu> menus = menuService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (menus.Count > 0)
                        {
                            foreach (var item in menus)
                            {
                                // set list first
                                if (item.OrderID < menu.OrderID)
                                {
                                    lstFirst.Add(new MenuSortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > menu.OrderID)
                                {
                                    lstLast.Add(new MenuSortModel
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
                                        menu.OrderID = _cntFirst;
                                        menuService.Update(menu, transaction: _transaction);
                                        _cntFirst++;
                                    }
                                    Menu itemFirst = menuService.GetAlls(m => m.ID == lstFirst[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    menuService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }

                            }
                            else
                            {
                                menu.OrderID = 1;
                                menuService.Update(menu, transaction: _transaction);
                                _cntFirst++;
                            }
                            //last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count > 0)
                            {
                                foreach (var item in lstLast)
                                {
                                    Menu itemLast = menuService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    menuService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            menu.OrderID = 1;
                            menuService.Update(menu, transaction: _transaction);
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
        public ActionResult SortDown(MenuItemIDModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    MenuService menuService = new MenuService(_connection);
                    Menu menu = menuService.GetAlls(m => m.ID == model.ID, transaction: _transaction).FirstOrDefault();
                    if (menu == null)
                        return Notifization.TEST("::");
                    int _orderId = menu.OrderID;
                    string _parentId = menu.ParentID;
                    // list first
                    IList<MenuSortModel> lstFirst = new List<MenuSortModel>();
                    // list last
                    IList<MenuSortModel> lstLast = new List<MenuSortModel>();
                    //
                    if (!string.IsNullOrWhiteSpace(_parentId))
                    {
                        List<Menu> menus = menuService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (menus.Count > 0)
                        {
                            foreach (var item in menus)
                            {
                                // set list first
                                if (item.OrderID < menu.OrderID)
                                {
                                    lstFirst.Add(new MenuSortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > menu.OrderID)
                                {
                                    lstLast.Add(new MenuSortModel
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
                                    Menu itemFirst = menuService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    menuService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }

                            }
                            //  last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count == 1)
                            {
                                Menu itemLast = menuService.GetAlls(m => m.ID == lstLast[0].ID, transaction: _transaction).FirstOrDefault();
                                itemLast.OrderID = _cntLast;
                                menuService.Update(itemLast, transaction: _transaction);
                                //
                                menu.OrderID = _cntLast + 1;
                                menuService.Update(menu, transaction: _transaction);
                                _cntLast++;
                            }
                            else if (lstLast.Count > 1)
                            {
                                for (int i = 0; i < lstLast.Count; i++)
                                {
                                    if (i == 1)
                                    {
                                        menu.OrderID = _cntLast;
                                        menuService.Update(menu, transaction: _transaction);
                                        _cntLast++;
                                    }
                                    Menu itemLast = menuService.GetAlls(m => m.ID == lstLast[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    menuService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            menu.OrderID = 1;
                            menuService.Update(menu, transaction: _transaction);
                        }
                    }
                    else
                    {
                        // truong hop cha , _parentId = ""
                        List<Menu> menus = menuService.GetAlls(m => m.ParentID == _parentId && m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                        if (menus.Count > 0)
                        {
                            foreach (var item in menus)
                            {
                                // set list first
                                if (item.OrderID < menu.OrderID)
                                {
                                    lstFirst.Add(new MenuSortModel
                                    {
                                        ID = item.ID,
                                        OrderID = item.OrderID
                                    });
                                }
                                // set list last
                                if (item.OrderID > menu.OrderID)
                                {
                                    lstLast.Add(new MenuSortModel
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
                                    Menu itemFirst = menuService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
                                    itemFirst.OrderID = _cntFirst;
                                    menuService.Update(itemFirst, transaction: _transaction);
                                    _cntFirst++;
                                }
                            }
                            //  last
                            int _cntLast = _cntFirst;
                            if (lstLast.Count == 1)
                            {
                                Menu itemLast = menuService.GetAlls(m => m.ID == lstLast[0].ID, transaction: _transaction).FirstOrDefault();
                                itemLast.OrderID = _cntLast;
                                menuService.Update(itemLast, transaction: _transaction);
                                //
                                menu.OrderID = _cntLast + 1;
                                menuService.Update(menu, transaction: _transaction);
                                _cntLast++;
                            }
                            else if (lstLast.Count > 1)
                            {
                                for (int i = 0; i < lstLast.Count; i++)
                                {
                                    if (i == 1)
                                    {
                                        menu.OrderID = _cntLast;
                                        menuService.Update(menu, transaction: _transaction);
                                        _cntLast++;
                                    }
                                    Menu itemLast = menuService.GetAlls(m => m.ID == lstLast[i].ID, transaction: _transaction).FirstOrDefault();
                                    itemLast.OrderID = _cntLast;
                                    menuService.Update(itemLast, transaction: _transaction);
                                    _cntLast++;
                                }
                            }
                        }
                        else
                        {
                            menu.OrderID = 1;
                            menuService.Update(menu, transaction: _transaction);
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
        public static List<MenuBar> GetMenuBar()
        {
            string sqlQuery = "SELECT * FROM App_Menu WHERE Enabled = @Enabled ORDER BY OrderID ASC";
            MenuService menuService = new MenuService();
            List<MenuBar> dtList = menuService.Query<MenuBar>(sqlQuery, new { Enabled = (int)WebCore.Model.Enum.ModelEnum.State.ENABLED }).ToList();
            return dtList;
        }
        public static SubMenuBar SubMenuBar(string parentId, List<MenuBar> allData, string controllerText)
        {
            bool isToggled = false;
            bool isHasitem = false;
            string result = string.Empty;
            List<MenuBar> dtList = allData.Where(m => m.ParentID == parentId).ToList();
            if (dtList.Count == 0)
                return new SubMenuBar();
            // 
            foreach (var item in dtList)
            {
                string _cls = "";
                string toggled = string.Empty;
                SubMenuBar subMenuBar = SubMenuBar(item.ID, allData, controllerText);
                if (subMenuBar == null)
                    continue;
                //
                string subClass = string.Empty;
                string link = item.BackLink;
                if (subMenuBar.IsHasItem)
                    subClass = "menu-has-children";
                // 
                if (string.IsNullOrWhiteSpace(link))
                    link = $"/{item.UrlRoot}/{item.Alias}";
                //
                result += $"<li class='{subClass}'><a href='{link}' class='{_cls}  {toggled}'>{item.Title}</a>{subMenuBar.InnerText}</li>";
            }
            //
            if (!string.IsNullOrEmpty(result))
                isHasitem = true;
            //
            return new SubMenuBar
            {
                InnerText = $"<ul class='navbar-nav ml-auto'>{result}</ul>",
                IsToggled = isToggled,
                IsHasItem = isHasitem
            };
        }
        public static string GetMenuName(string id)
        {
            using (var service = new MenuService())
            {
                Menu menu = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (menu == null)
                    return string.Empty;
                //
                return menu.Title;
            }
        }

        //#######################################################################################################################################################################################
        public static string GetMenuForCategory(string selectedId, string templatePage, bool parentAllow = true)
        {
            MenuService menuService = new MenuService();
            string result = string.Empty;
            string sqlQuery = "SELECT * FROM App_Menu WHERE Enabled = @Enabled AND PageTemlate = @PageTemlate ORDER BY ParentID, OrderID ASC";
            List<Menu> dtList = menuService.Query<Menu>(sqlQuery, new { Enabled = (int)WebCore.Model.Enum.ModelEnum.State.ENABLED, PageTemlate = templatePage }).ToList();
            if (dtList.Count() == 0)
                return result;
            //
            List<Menu> menus = dtList.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).ToList();
            foreach (var item in menus)
            {
                SubMenuBarForCategory subMenuBarForCategory = SubMenuForCategory(item.ID, dtList, selectedId);
                string disabled = string.Empty;
                if (!parentAllow && subMenuBarForCategory.IsSubNull)
                    disabled = "disabled";
                //
                string _innerText = subMenuBarForCategory.InnerText;
                string _state = string.Empty;
                if (item.ID == selectedId)
                    _state = "checked";
                //
                result += $@"<li><input id='cbx{item.ID}' type='checkbox' class='filled-in' data-id='{item.ID}' {_state} {disabled}/><label for='cbx{item.ID}'>{item.Title}</label></li>{_innerText}";
            }
            return result;
        }

        public static SubMenuBarForCategory SubMenuForCategory(string parentId, List<Menu> allData, string selectedId)
        {
            string result = string.Empty;
            List<WebCore.Entities.Menu> dtList = allData.Where(m => m.ParentID == parentId).ToList();
            if (dtList.Count == 0)
                return new SubMenuBarForCategory
                {
                    InnerText = string.Empty,
                    IsSubNull = false
                };
            // 
            foreach (var item in dtList)
            {
                string _path = item.PathAction;
                string toggled = string.Empty;
                SubMenuBarForCategory subMenuBarForCategory = SubMenuForCategory(item.ID, allData, selectedId);
                string disabled = string.Empty;
                if (subMenuBarForCategory.IsSubNull)
                    disabled = "disabled";
                //
                string _innerText = subMenuBarForCategory.InnerText;
                string _state = string.Empty;
                if (item.ID == selectedId)
                    _state = "checked";
                //
                result += $@"<li><input id='cbx{item.ID}' data-ischild='true' type='checkbox' class='filled-in' data-id='{item.ID}' {_state} {disabled} /><label for='cbx{item.ID}'>{item.Title}</label></li>{_innerText}";
            }
            //
            return new SubMenuBarForCategory
            {
                InnerText = $"<ul>{result}</ul>",
                IsSubNull = true
            };
        }
        //#######################################################################################################################################################################################
        public static MenuBar GetMenuBarByAlias(string alias)
        {
            using (var service = new MenuService())
            {
                string sqlQuery = @"SELECT TOP 1 * FROM App_Menu WHERE Alias = @Alias";
                MenuBar item = service.Query<MenuBar>(sqlQuery, new { Alias = alias }).FirstOrDefault();
                return item;
            }
        }

    }
}