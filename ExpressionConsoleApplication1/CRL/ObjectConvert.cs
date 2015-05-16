using CRL.Attribute;
using System;
using System.Collections.Generic;
using System.Data.Common;
namespace CRL
{
    public class ObjectConvert
    {
        /// <summary>
        /// 转化值,并处理默认值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static object SetNullValue(object value, Type type = null)
        {
            object result;
            if (type == null && value == null)
            {
                result = DBNull.Value;
            }
            else
            {
                if (value != null)
                {
                    type = value.GetType();
                }
                if (type == typeof(Enum))
                {
                    value = (int)value;
                }
                else if (type == typeof(DateTime))
                {
                    if (((DateTime)value).Year == 1)
                    {
                        value = DateTime.Now;
                    }
                }
                else if (type == typeof(byte[]))
                {
                    if (value == null)
                    {
                        result = 0;
                        return result;
                    }
                }
                else if (type == typeof(Guid))
                {
                    if (value == null)
                    {
                        result = Guid.NewGuid().ToString();
                        return result;
                    }
                }
                else if (type == typeof(string))
                {
                    value = string.Concat(value);
                }
                result = value;
            }
            return result;
        }
        /// <summary>
        /// 转换为为强类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static object ConvertObject(Type type, object obj)
        {
            if (type == typeof(int))
            {
                obj = Convert.ToInt32(obj);
            }
            else if (type == typeof(short))
            {
                obj = Convert.ToInt16(obj);
            }
            else if (type == typeof(long))
            {
                obj = Convert.ToInt64(obj);
            }
            else if (type == typeof(DateTime))
            {
                obj = Convert.ToDateTime(obj);
            }
            else if (type == typeof(decimal))
            {
                obj = Convert.ToDecimal(obj);
            }
            else if (type == typeof(double))
            {
                obj = Convert.ToDouble(obj);
            }
            else if (type == typeof(byte[]))
            {
                obj = (byte[])obj;
            }
            else if (type.BaseType == typeof(Enum))
            {
                obj = Convert.ToInt32(obj);
            }
            else if (type == typeof(bool))
            {
                obj = Convert.ToBoolean(obj);
            }
            else if (type == typeof(Guid))
            {
                obj = new Guid(obj.ToString());
            }
            return obj;
        }
        /// <summary>
        /// 转换为为强类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static T ConvertObject<T>(object obj)
        {
            T result;
            if (obj == null)
            {
                result = default(T);
            }
            else if (obj is DBNull)
            {
                result = default(T);
            }
            else
            {
                Type type = typeof(T);
                result = (T)((object)ObjectConvert.ConvertObject(type, obj));
            }
            return result;
        }
        /// <summary>
        /// 把复杂对象转换为简单对象
        /// </summary>
        /// <typeparam name="TSimple"></typeparam>
        /// <typeparam name="TComplex"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TSimple CloneToSimple<TSimple, TComplex>(TComplex source)
            where TSimple : class, new()
            where TComplex : class, new()
        {
            List<FieldAttribute> simpleTypes = TypeCache.GetProperties(typeof(TSimple), false);
            List<FieldAttribute> complexTypes = TypeCache.GetProperties(typeof(TComplex), false);
            TSimple obj = Activator.CreateInstance<TSimple>();
            using (List<FieldAttribute>.Enumerator enumerator = simpleTypes.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    FieldAttribute info = enumerator.Current;
                    FieldAttribute complexInfo = complexTypes.Find((FieldAttribute b) => b.Name == info.Name);
                    if (complexInfo != null)
                    {
                        object value = complexInfo.GetValue(source);
                        info.SetValue(obj, value);
                    }
                }
            }
            return obj;
        }
        /// <summary>
        /// 把复杂对象转换为简单对象
        /// 不会转换不对应的字段
        /// </summary>
        /// <typeparam name="TSimple"></typeparam>
        /// <typeparam name="TComplex"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<TSimple> CloneToSimple<TSimple, TComplex>(List<TComplex> source)
            where TSimple : class, new()
            where TComplex : class, new()
        {
            List<FieldAttribute> simpleTypes = TypeCache.GetProperties(typeof(TSimple), false);
            List<FieldAttribute> complexTypes = TypeCache.GetProperties(typeof(TComplex), false);
            List<TSimple> list = new List<TSimple>();
            foreach (TComplex item in source)
            {
                TSimple obj = Activator.CreateInstance<TSimple>();
                using (List<FieldAttribute>.Enumerator enumerator2 = simpleTypes.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        FieldAttribute info = enumerator2.Current;
                        FieldAttribute complexInfo = complexTypes.Find((FieldAttribute b) => b.Name == info.Name);
                        if (complexInfo != null)
                        {
                            object value = complexInfo.GetValue(item);
                            info.SetValue(obj, value);
                        }
                    }
                }
                list.Add(obj);
            }
            return list;
        }
        internal static List<TItem> DataReaderToList<TItem>(DbDataReader reader, bool setConstraintObj = false, ParameCollection fieldMapping = null) where TItem : class, new()
        {
            Type mainType = typeof(TItem);
            return ObjectConvert.DataReaderToList<TItem>(reader, mainType, setConstraintObj, fieldMapping);
        }
        internal static List<TItem> DataReaderToList<TItem>(DbDataReader reader, Type mainType, bool setConstraintObj = false, ParameCollection fieldMapping = null) where TItem : class, new()
        {
            List<TItem> list = new List<TItem>();
            List<FieldAttribute> typeArry = TypeCache.GetProperties(mainType, !setConstraintObj);
            while (reader.Read())
            {
                TItem detailItem = ObjectConvert.DataReaderToObj(reader, mainType, typeArry, fieldMapping) as TItem;
                list.Add(detailItem);
            }
            reader.Close();
            return list;
        }
        private static object DataReaderToObj(DbDataReader reader, Type mainType, List<FieldAttribute> typeArry, ParameCollection fieldMapping = null)
        {
            object detailItem = Activator.CreateInstance(mainType);
            List<string> columns = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columns.Add(reader.GetName(i).ToLower());
            }
            IModel obj2 = null;
            if (detailItem is IModel)
            {
                obj2 = (detailItem as IModel);
                obj2.BoundChange = false;
                bool b = obj2.BoundChange;
                if (fieldMapping != null)
                {
                    foreach (string name in fieldMapping.Keys)
                    {
                        obj2[name] = reader[fieldMapping[name].ToString()];
                    }
                }
            }
            foreach (FieldAttribute info in typeArry)
            {
                if (info.FieldType == FieldType.关联字段)
                {
                    string tab = TypeCache.GetTableName(info.ConstraintType);
                    string fieldName = info.GetTableFieldFormat(tab, info.ConstraintResultField);
                    object value = reader[fieldName];
                    info.SetValue(detailItem, value);
                    if (obj2 != null)
                    {
                        obj2[info.Name] = value;
                    }
                }
                else if (info.FieldType == FieldType.关联对象)
                {
                    Type type = info.PropertyType;
                    object oleObject = Activator.CreateInstance(type);
                    string tableName = TypeCache.GetTableName(type);
                    List<FieldAttribute> typeArry2 = TypeCache.GetProperties(type, true);
                    foreach (FieldAttribute info2 in typeArry2)
                    {
                        string fieldName = info2.AliasesName;
                        object value = reader[fieldName];
                        info2.SetValue(oleObject, value);
                        if (obj2 != null)
                        {
                            obj2[info2.Name] = value;
                        }
                    }
                    info.SetValue(detailItem, oleObject);
                }
                else if (columns.Contains(info.Name.ToLower()))
                {
                    object value = reader[info.Name];
                    info.SetValue(detailItem, value);
                }
            }
            if (obj2 != null)
            {
                obj2.BoundChange = true;
            }
            return detailItem;
        }
        /// <summary>
        /// DataRead转为字典
        /// </summary>
        /// <typeparam name="Tkey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal static Dictionary<Tkey, TValue> DataReadToDictionary<Tkey, TValue>(DbDataReader reader)
        {
            Dictionary<Tkey, TValue> dic = new Dictionary<Tkey, TValue>();
            while (reader.Read())
            {
                object data = reader[0];
                object data2 = reader[1];
                Tkey key = ObjectConvert.ConvertObject<Tkey>(data);
                TValue value = ObjectConvert.ConvertObject<TValue>(data2);
                dic.Add(key, value);
            }
            reader.Close();
            return dic;
        }
    }
}
