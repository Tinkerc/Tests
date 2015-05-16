using CoreHelper;
using CRL.Attribute;
using CRL.DBAdapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
namespace CRL
{
    /// <summary>
    /// 基本方法
    /// </summary>
    public class Base
    {
        /// <summary>
        /// 对集合进行分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="index"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static List<T> CutList<T>(List<T> table, int index, int pageSize) where T : class, new()
        {
            return table.Skip(index * pageSize).Take(pageSize).ToList<T>();
        }
        /// <summary>
        /// 获取查询字段,并自动转换虚拟字段
        /// </summary>
        /// <param name="typeArry"></param>
        /// <param name="aliases">是否别名</param>
        /// <returns></returns>
        internal static string GetQueryFields(List<FieldAttribute> typeArry, bool aliases)
        {
            string str = "";
            foreach (FieldAttribute info in typeArry)
            {
                if (info.FieldType == FieldType.虚拟字段)
                {
                    str += string.Format("{0} as {1},", info.VirtualField, info.KeyWordName);
                }
                else if (string.IsNullOrEmpty(info.QueryFullName))
                {
                    str += string.Format("{0},", info.KeyWordName);
                }
                else
                {
                    str += string.Format("{0},", info.QueryFullName);
                }
            }
            if (str.Length > 1)
            {
                str = str.Substring(0, str.Length - 1);
            }
            return str;
        }
        /// <summary>
        /// 检测所有对象
        /// </summary>
        /// <param name="dbHelper"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static string CheckAllModel(DBHelper dbHelper, Type baseType)
        {
            string msg = "";
            DBExtend helper = new DBExtend(dbHelper);
            Assembly assembyle = Assembly.GetAssembly(baseType);
            Type[] types = assembyle.GetTypes();
            List<Type> findTypes = new List<Type>();
            Type typeCRL = typeof(IModel);
            Type[] array = types;
            for (int i = 0; i < array.Length; i++)
            {
                Type type = array[i];
                if (type.IsClass)
                {
                    Type type2 = type.BaseType;
                    while (type2.BaseType != null)
                    {
                        if (type2.BaseType == typeCRL || type2 == typeCRL)
                        {
                            findTypes.Add(type);
                            break;
                        }
                        type2 = type2.BaseType;
                    }
                }
            }
            try
            {
                foreach (Type type in findTypes)
                {
                    try
                    {
                        object obj = Activator.CreateInstance(type);
                        IModel b = obj as IModel;
                        msg += b.CreateTable(helper);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ero_12B)
            {
            }
            EventLog.Log(msg);
            return msg;
        }
        /// <summary>
        /// SQL语句转换为存储过程
        /// </summary>
        /// <param name="template"></param>
        /// <param name="dbHelper"></param>
        /// <param name="sql"></param>
        /// <param name="procedureName"></param>
        /// <param name="templateParame"></param>
        /// <returns></returns>
        internal static string SqlToProcedure(string template, DBHelper dbHelper, string sql, string procedureName, Dictionary<string, string> templateParame = null)
        {
            DBAdapterBase adpater = DBAdapterBase.GetDBAdapterBase(dbHelper.CurrentDBType);
            template = template.Trim();
            Regex r = new Regex("\\@(\\w+)", RegexOptions.IgnoreCase);
            List<string> pars = new List<string>();
            Match i = r.Match(sql);
            while (i.Success)
            {
                string par = i.Groups[1].ToString();
                if (!pars.Contains(par))
                {
                    pars.Add(par);
                }
                i = i.NextMatch();
            }
            Dictionary<Type, string> typeMappint = adpater.GetFieldMapping();
            sql = sql.Replace("'", "''");
            template = template.Replace("{name}", procedureName);
            template = template.Replace("{sql}", sql);
            string parames = "";
            foreach (KeyValuePair<string, object> p in dbHelper.Params)
            {
                string key = p.Key.Replace("@", "");
                Type t = p.Value.GetType();
                if (!typeMappint.ContainsKey(t))
                {
                    throw new Exception(string.Format("找不到对应的字段类型映射 {0} 在 {1}", t, adpater));
                }
                string par = typeMappint[t];
                if (t == typeof(string))
                {
                    par = string.Format(par, 500);
                }
                parames += adpater.SpParameFormat(key, par, false);
            }
            foreach (KeyValuePair<string, object> p in dbHelper.OutParams)
            {
                string key = p.Key;
                Type t = p.Value.GetType();
                string par = typeMappint[t];
                parames += adpater.SpParameFormat(key, par, true);
            }
            if (parames.Length > 0)
            {
                parames = parames.Substring(0, parames.Length - 1);
            }
            template = template.Replace("{parame}", parames);
            if (templateParame != null)
            {
                foreach (KeyValuePair<string, string> item in templateParame)
                {
                    string value = item.Value;
                    value = value.Replace("'", "''");
                    template = template.Replace("{" + item.Key + "}", value);
                }
            }
            template = adpater.GetCreateSpScript(procedureName, template);
            return template;
        }
        public static string GetVersion()
        {
            AssemblyName assembly = Assembly.GetExecutingAssembly().GetName();
            return assembly.Version.ToString();
        }
    }
}
