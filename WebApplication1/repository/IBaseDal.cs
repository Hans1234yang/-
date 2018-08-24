using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.repository
{
    public partial interface IBaseDal<T> where T : class
    {
        IQueryable<T> LoadAll(Expression <Func<T,bool>> predicate);
    };
}
