using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class BaseController : Controller
    {
        /// <summary>
        /// 视图传递的分页页码
        /// </summary>
        public int pageIndex { get; set; }
        /// <summary>
        /// 视图传递的分页条数
        /// </summary>
        public int pageSize { get; set; }

        // GET: Base
        public ActionResult Index()
        {
            return View();
        }
    }
}