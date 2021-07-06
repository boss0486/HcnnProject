using PagedList.Mvc;
using System;
using System.Web.Mvc.Ajax;

namespace Helper.Pagination
{
    public class Paging
    {
        public const int PAGESIZE = 20;
        //

    }
    public class PagedRender
    {
        public const int PAGENUMBER =  20;
        public const int FAQ_PAGENUMBER = 5;
        public static PagedListRenderOptions PagedListRenderOption(int isLeft = -1)
        {
            string ulClass = "";
            if (isLeft == 0)
            {
                ulClass = "justify-content-center";
            }
            else if (isLeft == 1)
            {
                ulClass = "justify-content-end";
            }
            var abbb = new PagedListRenderOptions
            {
                LinkToFirstPageFormat = "←",
                LinkToPreviousPageFormat = "«",
                LinkToNextPageFormat = "»",
                LinkToLastPageFormat = "→",
                DisplayLinkToFirstPage = PagedListDisplayMode.Always,
                DisplayLinkToPreviousPage = PagedListDisplayMode.Always,
                DisplayLinkToLastPage = PagedListDisplayMode.Always,
                DisplayLinkToNextPage = PagedListDisplayMode.Always,
                MaximumPageNumbersToDisplay = 5,
                DisplayEllipsesWhenNotShowingAllPageNumbers = false,
                UlElementClasses = new string[] { $"pagination {ulClass}" },
                LiElementClasses = new string[] { "page-item" },
            };
            return PagedListRenderOptions.EnableUnobtrusiveAjaxReplacing(abbb, new AjaxOptions() { HttpMethod = "GET", UpdateTargetId = "bootpag" });
        }

    }

    public class PagingModel
    {
        public int Page { get; set; }
        public int Total { get; set; }
        public int PageSize { get; set; }
        public int TotalPage
        {
            get
            {
                if (Total % PageSize != 0)
                    return Convert.ToInt32(Total / PageSize) + 1;
                else
                    return Total / PageSize;
            }
        }
    }
}