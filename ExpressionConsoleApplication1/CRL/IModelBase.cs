using CRL.Attribute;
using System;
namespace CRL
{
    /// <summary>
    /// 基类,包含Id, AddTime字段
    /// </summary>
    [Serializable]
    public abstract class IModelBase : IModel
    {
        private DateTime addTime = DateTime.Now;
        /// <summary>
        /// 自增主键
        /// </summary>
        [Field(IsPrimaryKey = true)]
        public int Id
        {
            get;
            set;
        }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddTime
        {
            get;
            set;
        }
    }
}
