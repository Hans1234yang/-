using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Entity;
namespace WebApplication1.repository
{
    public class baseDal<T> : IBaseDal<T> where T : class
    {
        studentttEntities content = new studentttEntities();

        public IQueryable<T> LoadAll(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return content.Set<T>().Where(predicate).AsNoTracking<T>();
        }
    }
}