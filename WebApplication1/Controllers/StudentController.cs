
using Enterprise.BackServer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.server;

namespace WebApplication1.Controllers
{
    public class StudentController : Controller
    {
        private IStudentServer _studentServer;

        public StudentController(IStudentServer thestudentServer)
        {
            _studentServer = thestudentServer;
        }

        // GET: Student
        public ActionResult Index()
        {
            var query = _studentServer.LoadAll(s => s.stuname == "hans");
            //排序
            query = query.OrderBy(s => s.stuid);

            var result = query.ToList();
            //分页
            var jsonData = JsonConverter.Serialize(new PageLayUi(query.ToList().Count, result, ""), true);
            return Content(jsonData, "application/json");
        }
    }

    /// <summary>
    /// layui分页处理逻辑类
    /// </summary>
    public class PageLayUi
    {
        public PageLayUi(int count, object data, string msg = "")
        {
            code = 0;
            this.msg = msg;
            this.count = count;
            this.data = data;
        }
        public int code { get; private set; }
        public string msg { get; private set; }
        public int count { get; private set; }
        public object data { get; private set; }
    }
}