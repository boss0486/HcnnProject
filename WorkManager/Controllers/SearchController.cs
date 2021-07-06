using Helper;
using Helper.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using WebCore.Core;
using WebCore.Entities;
using WebCore.Model.Entities;
using WebCore.Services;

namespace WebApplication.HomePage.Controllers
{
    public class SearchController : Controller
    {
        // GET: Introduction 
        public ActionResult Index(string key = "", string space = "", int page = 1)
        {
            string min = string.Empty;
            string max = string.Empty;

            if (!string.IsNullOrWhiteSpace(space))
            {
                if (space.Contains("-"))
                {
                    String[] splited = space.Split('-').ToArray();
                    if (!string.IsNullOrWhiteSpace(splited[0]) && splited[0].Contains("min"))
                    {
                        min = Regex.Replace(splited[0], @"[^0-9]", "");
                    }
                    if (!string.IsNullOrWhiteSpace(splited[1]) && splited[1].Contains("max"))
                    {
                        max = Regex.Replace(splited[1], @"[^0-9]", "");
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(space) && space.Contains("min"))
                    {
                        min = Regex.Replace(space, @"[^0-9]", "");
                    }
                    if (!string.IsNullOrWhiteSpace(space) && space.Contains("max"))
                    {
                        max = Regex.Replace(space, @"[^0-9]", "");
                    }
                }
            }
            ViewData["CategoryText"] = "Tìm kiếm" + space;
            ViewData["KeyText"] = key;
            ViewData["PriceMin"] = min;
            ViewData["PriceMax"] = max;
            int _min = -1;
            int _max = -1;
            if (Helper.Page.Validate.TestNumeric(min))
                _min = Convert.ToInt32(min);
            if (Helper.Page.Validate.TestNumeric(max))
                _max = Convert.ToInt32(max);

            //
            IEnumerable<ProductHome> models = ProductService.ProductHomeSearch(new ProductHomeSearch
            {
                Query = key,
                PriceMin = _min,
                PriceMax = _max
            }, page);
            return View(models);
        }
    }
}