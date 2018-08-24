using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using WebApplication1.repository;

namespace WebApplication1.server
{
    public class BaseServer<T> where T:class
    {
        public IBaseDal<T> CurrentDal { get; set; }

        public virtual IQueryable<T> LoadAll(Expression<Func<T, bool>> predicate)
        {
            return CurrentDal.LoadAll(predicate);
        }
    }
}