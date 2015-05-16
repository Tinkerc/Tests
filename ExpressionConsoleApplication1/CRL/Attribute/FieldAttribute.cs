using System;
using System.Reflection;

namespace CRL.Attribute
{
    /// <summary>
    /// 字段属性设置
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : System.Attribute
    {
        private bool isPrimaryKey = false;
        private FieldType fieldType = FieldType.NONE;
        /// <summary>
        /// 索引类型
        /// </summary>
        public FieldIndexType FieldIndexType;
        /// <summary>
        /// 是否映射该字段
        /// 为false时则不参与查询
        /// </summary>
        public bool MappingField = true;
        /// <summary>
        /// 属性名称
        /// </summary>
        internal string Name;
        /// <summary>
        /// 对象类型
        /// </summary>
        internal Type ModelType;
        private string keyWordName;
        private string aliasesName;
        private string mappingName;
        /// <summary>
        /// 字段完整查询语法
        /// like t1.Name as Order__Name
        /// </summary>
        internal string QueryFullName;
        internal string ModelRemark;
        /// <summary>
        /// 备注
        /// </summary>
        internal string Remark;
        /// <summary>
        /// 主表名
        /// </summary>
        internal string TableName;
        /// <summary>
        /// 字段前辍,在查询转换时用
        /// like t1.
        /// </summary>
        internal string Prefix;
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue;
        /// <summary>
        /// 自定义数据库字段类型,如 varchar(50)
        /// </summary>
        public string ColumnType;
        /// <summary>
        /// 是否为空
        /// </summary>
        public bool NotNull;
        /// <summary>
        /// 长度,超过3000字段类型将会设为ntext
        /// 若是需要指定长度,请赋值
        /// 默认30
        /// </summary>
        public int Length = 30;
        /// <summary>
        /// 属性类型
        /// </summary>
        internal Type PropertyType;
        /// <summary>
        /// 自动转换虚拟字段
        /// 如year(addtime)
        /// </summary>
        public string VirtualField;
        /// <summary>
        /// 约束字段
        /// 格式:$CategoryCode[当前类型字段]=SequenceCode[关联表字段]
        /// </summary>
        public string ConstraintField;
        /// <summary>
        /// 子表查询附加条件
        /// 如:CategoryCode=1
        /// </summary>
        public string Constraint;
        /// <summary>
        /// 关联表类型
        /// 只是字段时使用
        /// typeof(ClassA)
        /// </summary>
        public Type ConstraintType;
        /// <summary>
        /// 关联表要取出的字段
        /// 只是字段时使用
        /// </summary>
        public string ConstraintResultField;
        private PropertyInfo propertyInfo;
        /// <summary>
        /// 是否为主键
        /// 名称为id自动为主键
        /// </summary>
        public bool IsPrimaryKey
        {
            get
            {
                return this.Name.ToLower() == "id" || this.isPrimaryKey;
            }
            set
            {
                this.isPrimaryKey = value;
            }
        }
        /// <summary>
        /// 字段类型
        /// </summary>
        public FieldType FieldType
        {
            get
            {
                if (this.fieldType == FieldType.NONE)
                {
                    bool isSystemType = this.PropertyType.Namespace == "System" || this.PropertyType.BaseType.Name == "Enum";
                    if (!string.IsNullOrEmpty(this.VirtualField))
                    {
                        this.fieldType = FieldType.虚拟字段;
                    }
                    else if (!string.IsNullOrEmpty(this.ConstraintField))
                    {
                        this.fieldType = (isSystemType ? FieldType.关联字段 : FieldType.关联对象);
                    }
                    else
                    {
                        this.fieldType = FieldType.数据库字段;
                    }
                }
                return this.fieldType;
            }
        }
        /// <summary>
        /// 关键字处理后的名称(字段名) 
        /// </summary>
        internal string KeyWordName
        {
            get
            {
                if (string.IsNullOrEmpty(this.keyWordName))
                {
                    this.keyWordName = TypeCache.GetDBAdapterFromCache(this.ModelType).KeyWordFormat(this.Name);
                }
                return this.keyWordName;
            }
        }
        /// <summary>
        /// 字段别名,带表名,关联字段查询时用
        /// like table1__name
        /// </summary>
        internal string AliasesName
        {
            get
            {
                if (string.IsNullOrEmpty(this.aliasesName))
                {
                    this.aliasesName = this.GetTableFieldFormat(this.TableName, this.Name);
                }
                return this.aliasesName;
            }
            set
            {
                this.aliasesName = value;
            }
        }
        /// <summary>
        /// 映射名称,在查询时用
        /// </summary>
        internal string MappingName
        {
            get
            {
                if (string.IsNullOrEmpty(this.mappingName))
                {
                    this.mappingName = this.Name;
                }
                return this.mappingName;
            }
            set
            {
                this.mappingName = value;
            }
        }
        public override string ToString()
        {
            return string.Format("{0}.{1}", this.TableName, this.Name);
        }
        /// <summary>
        /// 设置字段查询语法
        /// </summary>
        /// <param name="prefix">前缀</param>
        /// <param name="usePrefix">是否使用前缀</param>
        /// <param name="useAliasesName">是否使用别名 like as field1</param>
        internal void SetFieldQueryScript(string prefix, bool usePrefix, bool useAliasesName)
        {
            this.Prefix = prefix;
            string s = usePrefix ? this.Prefix : "";
            string script = s + this.KeyWordName;
            if (useAliasesName)
            {
                script = script + " as " + this.AliasesName;
            }
            if (this.FieldType == FieldType.虚拟字段)
            {
                script = string.Format("{0} as {1}", this.VirtualField, useAliasesName ? this.AliasesName : this.KeyWordName);
            }
            this.QueryFullName = script;
        }
        /// <summary>
        /// 按表名格式化字段名
        /// </summary>
        /// <param name="table"></param>
        /// <param name="fileld"></param>
        /// <returns></returns>
        internal string GetTableFieldFormat(string table, string fileld)
        {
            return string.Format("{0}__{1}", table, fileld);
        }
        /// <summary>
        /// 设置对象属性值
        /// </summary>
        /// <param name="_propertyInfo"></param>
        internal void SetPropertyInfo(PropertyInfo _propertyInfo)
        {
            this.propertyInfo = _propertyInfo;
        }
        /// <summary>
        /// 获取对象属性值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal object GetValue(object obj)
        {
            return this.propertyInfo.GetValue(obj, null);
        }
        /// <summary>
        /// 设置对象属性值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        internal void SetValue(object obj, object value)
        {
            if (value != null)
            {
                if (!(value is DBNull))
                {
                    Type type = value.GetType();
                    if (this.propertyInfo.PropertyType != type)
                    {
                        if (value is int && this.propertyInfo.PropertyType == typeof(string))
                        {
                            value = value.ToString();
                        }
                    }
                    try
                    {
                        value = ObjectConvert.ConvertObject(this.propertyInfo.PropertyType, value);
                        this.propertyInfo.SetValue(obj, value, null);
                    }
                    catch (Exception ero)
                    {
                        throw new Exception(string.Concat(new object[]
						{
							ero.Message,
							" 在属性",
							this.propertyInfo.Name,
							" ",
							this.propertyInfo.PropertyType
						}));
                    }
                }
            }
        }
    }
}
