using CRL.Attribute;
using CRL.DBAdapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace CRL
{
    internal class TypeCache
    {
        private static object lockObj = new object();
        internal static Dictionary<Type, TableAttribute> typeCache = new Dictionary<Type, TableAttribute>();
        /// <summary>
        /// 对象类型缓存
        /// </summary>
        internal static Dictionary<Type, string> ModelKeyCache = new Dictionary<Type, string>();
        /// <summary>
        /// 根据类型返回表名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetTableName<T>() where T : class, new()
        {
            Type type = typeof(T);
            return TypeCache.GetTableName(type);
        }
        /// <summary>
        /// 根据类型返回表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableName(Type type)
        {
            return TypeCache.GetTable(type).TableName;
        }
        public static TableAttribute GetTable(Type type)
        {
            TableAttribute result;
            if (TypeCache.typeCache.ContainsKey(type))
            {
                result = TypeCache.typeCache[type];
            }
            else
            {
                object[] objAttrs = type.GetCustomAttributes(typeof(TableAttribute), true);
                TableAttribute des;
                if (objAttrs == null || objAttrs.Length == 0)
                {
                    des = new TableAttribute
                    {
                        TableName = type.Name
                    };
                }
                else
                {
                    des = (objAttrs[0] as TableAttribute);
                }
                des.Type = type;
                lock (TypeCache.lockObj)
                {
                    if (!TypeCache.typeCache.ContainsKey(type))
                    {
                        TypeCache.typeCache.Add(type, des);
                    }
                }
                if (string.IsNullOrEmpty(des.TableName))
                {
                    des.TableName = type.Name;
                }
                TypeCache.SetProperties(des);
                result = des;
            }
            return result;
        }
        internal static void SetDBAdapterCache(Type type, DBAdapterBase dBAdapter)
        {
            TableAttribute table = TypeCache.GetTable(type);
            if (table.DBAdapter == null)
            {
                table.DBAdapter = dBAdapter;
            }
        }
        internal static DBAdapterBase GetDBAdapterFromCache(Type type)
        {
            TableAttribute table = TypeCache.GetTable(type);
            if (table.DBAdapter == null)
            {
                throw new Exception("未初始对应的适配器,在类型:" + type);
            }
            return table.DBAdapter;
        }
        /// <summary>
        /// 获取字段,并指定是否为基本查询字段(包函虚拟字段)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="onlyField">是否为基本查询字段</param>
        /// <returns></returns>
        public static List<FieldAttribute> GetProperties(Type type, bool onlyField)
        {
            TableAttribute table = TypeCache.GetTable(type);
            List<FieldAttribute> list = new List<FieldAttribute>(table.Fields);
            List<FieldAttribute> result;
            if (onlyField)
            {
                result = list.FindAll((FieldAttribute b) => b.FieldType == FieldType.数据库字段 || b.FieldType == FieldType.虚拟字段);
            }
            else
            {
                result = list;
            }
            return result;
        }
        private static void SetProperties(TableAttribute table)
        {
            if (table.Fields.Count <= 0)
            {
                Type type = table.Type;
                List<FieldAttribute> list = new List<FieldAttribute>();
                int i = 0;
                List<PropertyInfo> typeArry = table.Type.GetProperties().ToList<PropertyInfo>();
                Dictionary<string, PropertyInfo> dic = new Dictionary<string, PropertyInfo>();
                foreach (PropertyInfo info in typeArry)
                {
                    if (!dic.ContainsKey(info.Name))
                    {
                        dic.Add(info.Name, info);
                    }
                }
                foreach (PropertyInfo info in dic.Values)
                {
                    if (!(info.GetSetMethod() == null))
                    {
                        Type propertyType = info.PropertyType;
                        FieldAttribute f = new FieldAttribute();
                        if (propertyType.FullName.IndexOf("System.Collections") <= -1)
                        {
                            object[] attrs = info.GetCustomAttributes(typeof(FieldAttribute), true);
                            if (attrs != null && attrs.Length > 0)
                            {
                                f = (attrs[0] as FieldAttribute);
                            }
                            f.SetPropertyInfo(info);
                            f.PropertyType = propertyType;
                            f.Name = info.Name;
                            f.TableName = table.TableName;
                            f.ModelType = table.Type;
                            if (!string.IsNullOrEmpty(f.VirtualField))
                            {
//                                if (SettingConfig.StringFormat != null)
//                                {
//                                    f.VirtualField = SettingConfig.StringFormat(f.VirtualField);
//                                }
                            }
                            if (f.MappingField)
                            {
                                if (propertyType == typeof(string))
                                {
                                    if (f.Length == 0)
                                    {
                                        f.Length = 30;
                                    }
                                }
                                if (f.IsPrimaryKey)
                                {
                                    table.PrimaryKey = f;
                                    i++;
                                }
                                list.Add(f);
                            }
                        }
                    }
                }
                if (i == 0)
                {
                    throw new Exception(string.Format("对象{0}未设置任何主键", type.Name));
                }
                if (i > 1)
                {
                    throw new Exception(string.Format("对象{0}设置的主键字段太多 {1}", type.Name, i));
                }
                table.Fields = list;
            }
        }
    }
}
