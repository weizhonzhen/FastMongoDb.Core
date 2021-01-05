using FastUntility.Core.Page;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FastMongoDb.Core.Repository
{
    public interface IMongoDbRepository
    {
        bool Add<T>(T model);

        Task<bool> AddAsy<T>(T model);

        bool Delete<T>(Expression<Func<T, bool>> predicate);

        Task<bool> DeleteAsy<T>(Expression<Func<T, bool>> predicate);

        bool Update<T>(Expression<Func<T, bool>> predicate, T item, Expression<Func<T, object>> field);

        Task<bool> UpdateAsy<T>(Expression<Func<T, bool>> predicate, T item, Expression<Func<T, object>> field);

        bool Replace<T>(Expression<Func<T, bool>> predicate, T item);

        Task<bool> ReplaceAsy<T>(Expression<Func<T, bool>> predicate, T item);

        T GetModel<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null) where T : class, new();

        Task<T> GetModelAsy<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null) where T : class, new();

        List<T> GetList<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, Expression<Func<T, object>> sort = null, bool isDesc = false);

        Task<List<T>> GetListAsy<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, Expression<Func<T, object>> sort = null, bool isDesc = false);

        long GetCount<T>(Expression<Func<T, bool>> predicate);

        Task<long> GetLCountAsy<T>(Expression<Func<T, bool>> predicate);

        T FindDelete<T>(Expression<Func<T, bool>> predicate) where T : class, new();

        Task<T> FindDeleteAsy<T>(Expression<Func<T, bool>> predicate) where T : class, new();

        T FindReplace<T>(Expression<Func<T, bool>> predicate, T item) where T : class, new();

        Task<T> FindReplaceAsy<T>(Expression<Func<T, bool>> predicate, T item) where T : class, new();

        T FindUpdate<T>(Expression<Func<T, bool>> predicate, T item, Expression<Func<T, object>> field) where T : class, new();

        Task<T> FindUpdateAsy<T>(Expression<Func<T, bool>> predicate, T item, Expression<Func<T, object>> field) where T : class, new();

        PageResult<T> PageList<T>(PageModel pModel, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, Expression<Func<T, object>> sort = null, bool isDesc = false) where T : class, new();

        Task<PageResult<T>> PageListAsy<T>(PageModel pModel, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, Expression<Func<T, object>> sort = null, bool isDesc = false) where T : class, new();
    }
}
