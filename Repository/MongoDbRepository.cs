using FastUntility.Core.Base;
using FastUntility.Core.Page;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FastMongoDb.Core.Repository
{
    public class MongoDbRepository : IMongoDbRepository
    {
        private ConfigModel config;
        private MongoClient client;

        public MongoDbRepository(IOptions<ConfigModel> options)
        {
            try
            {
                if (options == null || options.Value.ConnStr == null)
                {
                    config = BaseConfig.GetValue<ConfigModel>(AppSettingKey.MongoDb, "db.json");
                    client = new MongoClient(config.ConnStr);
                }
                else
                {
                    config = options.Value;
                    client = new MongoClient(config.ConnStr);
                }
            }
            catch
            {
                throw new Exception(@"services.AddFastMongoDb(a => { a.ConnStr = ''; a.DbName = ''; }); 
                                    or ( services.AddFastMongoDb(); and db.json add MongoDb:{'ConnStr':'mongodb address','DbName':'dbname','Max'': 100,'Min': 10} )");
            }
        }


        public bool Add<T>(T model)
        {
            try
            {
                GetClient<T>().InsertOne(model);
                return true;
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "Add<T>");
                return false;
            }
        }

        public ValueTask<bool> AddAsy<T>(T model)
        {
            try
            {
                GetClient<T>().InsertOneAsync(model);
                return new ValueTask<bool>(true);
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "AddAsy<T>");
                return new ValueTask<bool>(false);
            }
        }


        public bool AddList<T>(List<T> list)
        {
            try
            {
                GetClient<T>().InsertMany(list);
                return true;
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "AddList<T>");
                return false;
            }
        }

        public ValueTask<bool> AddListAsy<T>(List<T> list)
        {
            try
            {
                GetClient<T>().InsertManyAsync(list);
                return new ValueTask<bool>(true);
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "AddListAsy<T>");
                return new ValueTask<bool>(false);
            }
        }


        public bool Delete<T>(Expression<Func<T, bool>> predicate)

        {
            try
            {
                return GetClient<T>().DeleteMany<T>(predicate).DeletedCount > 0;
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "Delete<T>");
                return false;
            }
        }
        public ValueTask<bool> DeleteAsy<T>(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return new ValueTask<bool>(GetClient<T>().DeleteManyAsync<T>(predicate).Result.DeletedCount > 0);
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "DeleteAsy<T>");
                return new ValueTask<bool>(false);
            }
        }


        public bool Update<T>(Expression<Func<T, bool>> predicate, T item, Expression<Func<T, object>> field)
        {
            try
            {
                var result = GetUpdateFiled<T>(item, field);
                if (result == null)
                    return false;
                else
                    return GetClient<T>().UpdateMany<T>(predicate, result).ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "Update<T>");
                return false;
            }
        }

        public ValueTask<bool> UpdateAsy<T>(Expression<Func<T, bool>> predicate, T item, Expression<Func<T, object>> field)
        {
            try
            {
                var result = GetUpdateFiled<T>(item, field);
                if (result == null)
                    return new ValueTask<bool>(false);
                else
                    return new ValueTask<bool>(GetClient<T>().UpdateManyAsync<T>(predicate, result).Result.ModifiedCount > 0);
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "UpdateAsy<T>");
                return new ValueTask<bool>(false);
            }
        }


        public bool Replace<T>(Expression<Func<T, bool>> predicate, T item)
        {
            try
            {
                return GetClient<T>().ReplaceOne<T>(predicate, item).ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "Replace<T>");
                return false;
            }
        }

        public ValueTask<bool> ReplaceAsy<T>(Expression<Func<T, bool>> predicate, T item)
        {
            try
            {
                return new ValueTask<bool>(GetClient<T>().ReplaceOneAsync<T>(predicate, item).Result.ModifiedCount > 0);
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "ReplaceAsy<T>");
                return new ValueTask<bool>(false);
            }
        }


        public T GetModel<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null) where T : class, new()
        {
            try
            {
                if (field != null)
                    return GetClient<T>().Find<T>(predicate).Project<T>(GetField(field)).FirstOrDefault<T>();
                else
                    return GetClient<T>().Find<T>(predicate).FirstOrDefault<T>();
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "GetModel<T>");
                return new T();
            }
        }

        public ValueTask<T> GetModelAsy<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null) where T : class, new()
        {
            return new ValueTask<T>(GetModel<T>(predicate, field));
        }


        public List<T> GetList<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, Expression<Func<T, object>> sort = null, bool isDesc = false)
        {
            try
            {
                if (field == null)
                {
                    if (sort == null)
                        return GetClient<T>().Find<T>(predicate).ToList<T>();
                    else
                        return GetClient<T>().Find<T>(predicate).Sort(isDesc ? Builders<T>.Sort.Descending(sort) : Builders<T>.Sort.Ascending(sort)).ToList<T>();
                }
                else
                {
                    if (sort == null)
                        return GetClient<T>().Find<T>(predicate).Project<T>(GetField(field)).ToList<T>();
                    else
                        return GetClient<T>().Find<T>(predicate).Project<T>(GetField(field)).Sort(isDesc ? Builders<T>.Sort.Descending(sort) : Builders<T>.Sort.Ascending(sort)).ToList<T>();
                }
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "GetList<T>");
                return new List<T>();
            }
        }

        public ValueTask<List<T>> GetListAsy<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, Expression<Func<T, object>> sort = null, bool isDesc = false)
        {
            return new ValueTask<List<T>>(GetList<T>(predicate, field, sort, isDesc));
        }


        public long GetCount<T>(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return GetClient<T>().Count<T>(predicate);
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "GetCount<T>");
                return 0;
            }
        }

        public ValueTask<long> GetLCountAsy<T>(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return new ValueTask<long>(GetClient<T>().Count<T>(predicate));
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "GetLCountAsy<T>");
                return new ValueTask<long>(0);
            }
        }


        public T FindDelete<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            try
            {
                return GetClient<T>().FindOneAndDelete<T>(predicate);
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "FindDelete<T>");
                return new T();
            }
        }

        public ValueTask<T> FindDeleteAsy<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            try
            {
                return new ValueTask<T>(GetClient<T>().FindOneAndDeleteAsync<T>(predicate));
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "FindDeleteAsy<T>");
                return new ValueTask<T>(new T());
            }
        }


        public T FindReplace<T>(Expression<Func<T, bool>> predicate, T item) where T : class, new()
        {
            try
            {
                return GetClient<T>().FindOneAndReplace<T>(predicate, item);
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    SaveLog<T>(ex, "FindReplace<T>");
                }).ConfigureAwait(false);
                return new T();
            }
        }

        public ValueTask<T> FindReplaceAsy<T>(Expression<Func<T, bool>> predicate, T item) where T : class, new()
        {
            try
            {
                return new ValueTask<T>(GetClient<T>().FindOneAndReplace<T>(predicate, item));
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "FindReplaceAsy<T>");
                return new ValueTask<T>(new T());
            }
        }


        public T FindUpdate<T>(Expression<Func<T, bool>> predicate, T item, Expression<Func<T, object>> field) where T : class, new()
        {
            try
            {
                var result = GetUpdateFiled<T>(item, field);
                if (result == null)
                    return new T();
                else
                    return GetClient<T>().FindOneAndUpdate<T>(predicate, result);
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "FindUpdate<T>");
                return new T();
            }
        }

        public ValueTask<T> FindUpdateAsy<T>(Expression<Func<T, bool>> predicate, T item, Expression<Func<T, object>> field) where T : class, new()
        {
            try
            {
                var result = GetUpdateFiled<T>(item, field);
                if (result == null)
                    return new ValueTask<T>(new T());
                else
                    return new ValueTask<T>(GetClient<T>().FindOneAndUpdate<T>(predicate, result));
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "FindUpdateAsy<T>");
                return new ValueTask<T>(new T());
            }
        }


        public PageResult<T> PageList<T>(PageModel pModel, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, Expression<Func<T, object>> sort = null, bool isDesc = false) where T : class, new()
        {
            try
            {
                var item = new PageResult<T>();
                if (pModel.PageId < 0)
                    pModel.PageId = 0;

                pModel.StarId = (pModel.PageId - 1) * pModel.PageSize;
                pModel.EndId = pModel.PageId * pModel.PageSize;

                pModel.TotalRecord = int.Parse(GetClient<T>().Count(predicate).ToString());

                if ((pModel.TotalRecord % pModel.PageSize) == 0)
                    pModel.TotalPage = pModel.TotalRecord / pModel.PageSize;
                else
                    pModel.TotalPage = (pModel.TotalRecord / pModel.PageSize) + 1;

                item.pModel = pModel;

                if (field == null)
                {
                    if (sort == null)
                        item.list = GetClient<T>().Find<T>(predicate).Skip(pModel.StarId).Limit(pModel.PageSize).ToList<T>();
                    else
                        item.list = GetClient<T>().Find<T>(predicate).Sort(isDesc ? Builders<T>.Sort.Descending(sort) : Builders<T>.Sort.Ascending(sort)).Skip(pModel.StarId).Limit(pModel.PageSize).ToList<T>();
                }
                else
                {
                    if (sort == null)
                        item.list = GetClient<T>().Find<T>(predicate).Project<T>(GetField(field)).Skip(pModel.StarId).Limit(pModel.PageSize).ToList<T>();
                    else
                        item.list = GetClient<T>().Find<T>(predicate).Project<T>(GetField(field)).Sort(isDesc ? Builders<T>.Sort.Descending(sort) : Builders<T>.Sort.Ascending(sort)).Skip(pModel.StarId).Limit(pModel.PageSize).ToList<T>();
                }

                return item;
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "PgaeList<T>");
                return new PageResult<T>();
            }
        }

        public ValueTask<PageResult<T>> PageListAsy<T>(PageModel pModel, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, Expression<Func<T, object>> sort = null, bool isDesc = false) where T : class, new()
        {
            return new ValueTask<PageResult<T>>(PageList<T>(pModel, predicate, field, sort, isDesc));
        }


        private ProjectionDefinition<T> GetField<T>(Expression<Func<T, object>> field)
        {
            if (field != null)
            {
                var projection = Builders<T>.Projection.Exclude("_id");
                foreach (var item in (field.Body as NewExpression).Arguments)
                {
                    projection = projection.Include((item as MemberExpression).Member.Name);
                }

                return projection;
            }
            else
                return Builders<T>.Projection.Exclude("_id");
        }

        private UpdateDefinition<T> GetUpdateFiled<T>(T item, Expression<Func<T, object>> field)
        {
            try
            {
                var pInfo = PropertyCache.GetPropertyInfo<T>();
                var fieldList = new List<UpdateDefinition<T>>();

                var list = (field.Body as NewExpression).Members;
                foreach (var temp in list)
                {
                    var itemValue = BaseEmit.Get(item, temp.Name);
                    fieldList.Add(Builders<T>.Update.Set(temp.Name, itemValue));
                }

                return Builders<T>.Update.Combine(fieldList);
            }
            catch (Exception ex)
            {
                SaveLog<T>(ex, "UpdateDefinition<T>");
                return null;
            }
        }

        private void SaveLog<T>(Exception ex, string CurrentMethod)
        {
            BaseLog.SaveLog(string.Format("方法：{0},对象：{1},出错详情：{2}", CurrentMethod, typeof(T).Name, ex.ToString()), "MongoDb_exp");
        }

        private IMongoCollection<T> GetClient<T>()
        {
            return client.GetDatabase(config.DbName).GetCollection<T>(typeof(T).Name);
        }
    }
}