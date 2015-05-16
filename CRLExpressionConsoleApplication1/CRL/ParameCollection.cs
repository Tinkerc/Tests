using System;
using System.Collections.Generic;
namespace CRL
{
    /// <summary>
    /// 键值的集合,不区分大小写
    /// 如果不需要以参数形式处理,名称前加上$ 如 c2["$SoldCount"]="SoldCount+" + num;
    /// 分页参数仅在分页时才会用到,查询缓存参数则相反
    /// </summary>
    public class ParameCollection : Dictionary<string, object>
    {
        /// <summary>
        /// 获取键值,按小写
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public new object this[string key]
        {
            get
            {
                object obj = null;
                base.TryGetValue(key.ToLower(), out obj);
                return obj;
            }
            set
            {
                base[key.ToLower()] = value;
            }
        }
        /// <summary>
        /// 按小写名添加到字典
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public new void Add(string key, object value)
        {
            base.Add(key.ToLower(), value);
        }
        /// <summary>
        /// 设置查询字段
        /// </summary>
        /// <param name="fields"></param>
        public void SetQueryFields(string fields)
        {
            this.Add("Fields", fields);
        }
        /// <summary>
        /// 设置排序字段
        /// </summary>
        /// <param name="sort"></param>
        public void SetQuerySort(string sort)
        {
            this.Add("Sort", sort);
        }
        /// <summary>
        /// 设置每页大小
        /// </summary>
        /// <param name="pageSize"></param>
        public void SetQueryPageSize(int pageSize)
        {
            this.Add("PageSize", pageSize);
        }
        /// <summary>
        /// 设置页索引
        /// </summary>
        /// <param name="pageIndex"></param>
        public void SetQueryPageIndex(int pageIndex)
        {
            this.Add("PageIndex", pageIndex);
        }
        /// <summary>
        /// 设置条件
        /// </summary>
        /// <param name="condition"></param>
        public void SetQueryCondition(string condition)
        {
            this.Add("Condition", condition);
        }
        /// <summary>
        /// 设置查询前几条
        /// </summary>
        /// <param name="top"></param>
        public void SetQueryTop(int top)
        {
            this.Add("Top", top);
        }
        /// <summary>
        /// 设置查询缓存时间,分
        /// 大于0则会产生缓存
        /// </summary>
        /// <param name="minute"></param>
        public void SetCacheTime(int minute)
        {
            this.Add("CacheTime", minute);
        }
    }
}
