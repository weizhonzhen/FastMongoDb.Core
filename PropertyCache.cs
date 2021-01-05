﻿using FastUntility.Core.Cache;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FastMongoDb.Core
{
    /// <summary>
    /// 缓存类
    /// </summary>
    internal static class PropertyCache
    { 
        #region 泛型缓存属性成员
        /// <summary>
        /// 泛型缓存属性成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<PropertyInfo> GetPropertyInfo<T>(bool IsCache = true)
        {
            var key = string.Format("{0}.{1}", typeof(T).Namespace,typeof(T).Name);

            if (IsCache)
            {
                if (BaseCache.Exists(key))
                    return BaseCache.Get<List<PropertyInfo>>(key);
                else
                {
                    var info = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
                    BaseCache.Set<List<PropertyInfo>>(key, info);
                    return info;
                }
            }
            else
            {
                BaseCache.Remove(key);
                return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
            }
        }
        #endregion
    }
}
