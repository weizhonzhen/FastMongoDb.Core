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
            if (options == null)
            {
                throw new Exception("services.AddFastMongoDb(a => { a.ConnStr = ''; a.DbName = ''; });");
            }
            else
            {
                config = options.Value;
                client = new MongoClient(config.ConnStr);
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
                Task.Run(() =>
                {
                    SaveLog<T>(ex, "Add<T>");
                }).ConfigureAwait(false);
                return false;
            }
        }

        public async Task<bool> AddAsy<T>(T model)
        {
            return await Task.Run(() =>
            {
                try
                {
                    GetClient<T>().InsertOneAsync(model);
                    return true;
                }
                catch (Exception ex)
                {
                    Task.Run(() =>
                    {
                        SaveLog<T>(ex, "AddAsy<T>");
                    }).ConfigureAwait(false);
                    return false;
                }
            }).ConfigureAwait(false);
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
                Task.Run(() =>
                {
                    SaveLog<T>(ex, "AddList<T>");
                }).ConfigureAwait(false);
                return false;
            }
        }

        public async Task<bool> AddListAsy<T>(List<T> list)
        {
            return await Task.Run(() =>
            {
                try
                {
                    GetClient<T>().InsertManyAsync(list);
                    return true;
                }
                catch (Exception ex)
                {
                    Task.Run(() =>
                    {
                        SaveLog<T>(ex, "AddListAsy<T>");
                    }).ConfigureAwait(false);
                    return false;
                }
            }).ConfigureAwait(false);
        }


        public bool Delete<T>(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return GetClient<T>().DeleteMany<T>(predicate).DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    SaveLog<T>(ex, "Delete<T>");
                }).ConfigureAwait(false);
                return false;
            }
        }

        public async Task<bool> DeleteAsy<T>(Expression<Func<T, bool>> predicate)
        {
            return await Task.Factory.StartNew(() =>
            {
                try
                {
                    return GetClient<T>().DeleteManyAsync<T>(predicate).Result.DeletedCount > 0;
                }
                catch (Exception ex)
                {
                    Task.Run(() =>
                    {
                        SaveLog<T>(ex, "DeleteAsy<T>");
                    }).ConfigureAwait(false);
                    return false;
                }
            }).ConfigureAwait(false);
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
                Task.Run(() =>
                {
                    SaveLog<T>(ex, "Update<T>");
                }).ConfigureAwait(false);
                return false;
            }
        }

        public async Task<bool> UpdateAsy<T>(Expression<Func<T, bool>> predicate, T item, Expression<Func<T, object>> field)
        {
            return await Task.Factory.StartNew(() =>
            {
                try
                {
                    var result = GetUpdateFiled<T>(item, field);
                    if (result == null)
                        return false;
                    else
                        return GetClient<T>().UpdateManyAsync<T>(predicate, result).Result.ModifiedCount > 0;
                }
                catch (Exception ex)
                {
                    Task.Run(() =>
                    {
                        SaveLog<T>(ex, "UpdateAsy<T>");
                    }).ConfigureAwait(false);
                    return false;
                }
            }).ConfigureAwait(false);
        }


        public bool Replace<T>(Expression<Func<T, bool>> predicate, T item)
        {
            try
            {
                return GetClient<T>().ReplaceOne<T>(predicate, item).ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    SaveLog<T>(ex, "Replace<T>");
                }).ConfigureAwait(false);
                return false;
            }
        }

        public async Task<bool> ReplaceAsy<T>(Expression<Func<T, bool>> predicate, T item)
        {
            return await Task.Factory.StartNew(() =>
            {
                try
                {
                    return GetClient<T>().ReplaceOneAsync<T>(predicate, item).Result.ModifiedCount > 0;
                }
                catch (Exception ex)
                {
                    Task.Factory.StartNew(() =>
                    {
                        SaveLog<T>(ex, "ReplaceAsy<T>");
                    }).ConfigureAwait(false);
                    return false;
                }
            }).ConfigureAwait(false);
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
                Task.Run(() =>
                {
                    SaveLog<T>(ex, "GetModel<T>");
                }).ConfigureAwait(false);
                return new T();
            }
        }

        public async Task<T> GetModelAsy<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null) where T : class, new()
        {
            return await Task.Run(() =>
            {
                return GetModel<T>(predicate, field);
            }).ConfigureAwait(false);
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
                Task.Run(() =>
                {
                    SaveLog<T>(ex, "GetList<T>");
                }).ConfigureAwait(false);
                return new List<T>();
            }
        }

        public async Task<List<T>> GetListAsy<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, Expression<Func<T, object>> sort = null, bool isDesc = false)
        {
            return await Task.Run(() =>
            {
                return GetList<T>(predicate, field, sort, isDesc);
            }).ConfigureAwait(false);
        }


        public long GetCount<T>(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return GetClient<T>().Count<T>(predicate);
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    SaveLog<T>(ex, "GetCount<T>");
                });
                return -99;
            }
        }

        public async Task<long> GetLCountAsy<T>(Expression<Func<T, bool>> predicate)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return GetClient<T>().CountAsync<T>(predicate).Result;
                }
                catch (Exception ex)
                {
                    Task.Run(() =>
                    {
                        SaveLog<T>(ex, "GetLCountAsy<T>");
                    }).ConfigureAwait(false);
                    return 0;
                }
            }).ConfigureAwait(false);
        }


        public T FindDelete<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            try
            {
                return GetClient<T>().FindOneAndDelete<T>(predicate);
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    SaveLog<T>(ex, "FindDelete<T>");
                }).ConfigureAwait(false);
                return new T();
            }
        }

        public async Task<T> FindDeleteAsy<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            return await Task.Run(() =>
            {
                try
                {
                    return GetClient<T>().FindOneAndDeleteAsync<T>(predicate).Result;
                }
                catch (Exception ex)
                {
                    Task.Run(() =>
                    {
                        SaveLog<T>(ex, "FindDeleteAsy<T>");
                    }).ConfigureAwait(false);
                    return new T();
                }
            }).ConfigureAwait(false);
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

        public async Task<T> FindReplaceAsy<T>(Expression<Func<T, bool>> predicate, T item) where T : class, new()
        {
            return await Task.Run(() =>
            {
                try
                {
                    return GetClient<T>().FindOneAndReplaceAsync<T>(predicate, item).Result;
                }
                catch (Exception ex)
                {
                    Task.Run(() =>
                    {
                        SaveLog<T>(ex, "FindReplaceAsy<T>");
                    }).ConfigureAwait(false);
                    return new T();
                }
            }).ConfigureAwait(false);
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
                Task.Run(() =>
                {
                    SaveLog<T>(ex, "FindUpdate<T>");
                }).ConfigureAwait(false);
                return new T();
            }
        }

        public async Task<T> FindUpdateAsy<T>(Expression<Func<T, bool>> predicate, T item, Expression<Func<T, object>> field) where T : class, new()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var result = GetUpdateFiled<T>(item, field);
                    if (result == null)
                        return new T();
                    else
                        return GetClient<T>().FindOneAndUpdateAsync<T>(predicate, result).Result;
                }
                catch (Exception ex)
                {
                    Task.Run(() =>
                    {
                        SaveLog<T>(ex, "FindUpdateAsy<T>");
                    }).ConfigureAwait(false);
                    return new T();
                }
            }).ConfigureAwait(false);
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
                Task.Run(() =>
                {
                    SaveLog<T>(ex, "PgaeList<T>");
                }).ConfigureAwait(false);
                return new PageResult<T>();
            }
        }

        public async Task<PageResult<T>> PageListAsy<T>(PageModel pModel, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, Expression<Func<T, object>> sort = null, bool isDesc = false) where T : class, new()
        {
            return await Task.Run(() =>
            {
                return PageList<T>(pModel, predicate, field, sort, isDesc);
            }).ConfigureAwait(false);
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
                var dynGet = new DynamicGet<T>();
                var fieldList = new List<UpdateDefinition<T>>();

                var list = (field.Body as NewExpression).Members;
                foreach (var temp in list)
                {
                    var itemValue = dynGet.GetValue(item, temp.Name, true);
                    fieldList.Add(Builders<T>.Update.Set(temp.Name, itemValue));
                }

                return Builders<T>.Update.Combine(fieldList);
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    SaveLog<T>(ex, "UpdateDefinition<T>");
                }).ConfigureAwait(false);
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
