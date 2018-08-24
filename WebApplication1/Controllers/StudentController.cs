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
            var query = _studentServer.LoadAll(s=>s.stuname=="hans");
            //排序
            query = query.OrderBy(s => s.stuid);
            //分页
            Util.PagedList<studnet> list = new Util.PagedList<studnet>(query,pageIndex,pageIndex);
        }
    }
}