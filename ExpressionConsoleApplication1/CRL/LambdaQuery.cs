using CRL.Attribute;
using CRL.DBAdapter;
using CRL.LambdaQuery;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace CRL
{
    /// <summary>
    /// Lamada表达式查询，Func深度暂不能超过一级 如 b.Class2.Id==1 或b.Id==a.Class2.Id
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LambdaQuery<T> where T : IModel, new()
    {
        private ExpressionVisitor<T> visitor = new ExpressionVisitor<T>();
        /// <summary>
        /// 查询的字段
        /// </summary>
        private List<FieldAttribute> QueryFields = new List<FieldAttribute>();
        /// <summary>
        /// 查询的表名
        /// </summary>
        internal string QueryTableName = "";
        /// <summary>
        /// 条件
        /// </summary>
        internal string Condition = "";
        /// <summary>
        /// 前几条
        /// </summary>
        internal int QueryTop = 0;
        /// <summary>
        /// 排序
        /// </summary>
        internal string QueryOrderBy = "";
        private bool useTableAliasesName = true;
        internal int PageSize = 10;
        internal int PageIndex = 1;
        internal DBAdapterBase dBAdapter;
        private DBExtend dBExtend;
        /// <summary>
        /// 别名
        /// </summary>
        private Dictionary<Type, string> prefixs = new Dictionary<Type, string>();
        private int prefixIndex = 0;
        /// <summary>
        /// 字段映射
        /// 属性名,字段名
        /// </summary>
        internal ParameCollection FieldMapping = new ParameCollection();
        internal Dictionary<Type, string> Relations = new Dictionary<Type, string>();
        /// <summary>
        /// 处理后的查询参数
        /// </summary>
        internal ParameCollection QueryParames
        {
            get
            {
                return this.visitor.QueryParames;
            }
        }
        /// <summary>
        /// 返回查询唯一值
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}{1}{2}", this.QueryTableName, this.Condition, this.QueryTop);
        }
        internal List<FieldAttribute> GetQueryFields()
        {
            if (this.QueryFields.Count == 0)
            {
                this.Select(0, null);
            }
            return this.QueryFields;
        }
        /// <summary>
        /// 获取别名,如t1.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal string GetPrefix(Type type = null)
        {
            if (type == null)
            {
                type = typeof(T);
            }
            string result;
            if (!this.useTableAliasesName)
            {
                result = "";
            }
            else
            {
                if (!this.prefixs.ContainsKey(type))
                {
                    this.prefixIndex++;
                    this.prefixs[type] = string.Format("t{0}.", this.prefixIndex);
                }
                result = this.prefixs[type];
            }
            return result;
        }
        /// <summary>
        /// lambda查询
        /// </summary>
        /// <param name="_dbExtend"></param>
        /// <param name="_useTableAliasesName">查询是否生成别名</param>
        internal LambdaQuery(DBExtend _dbExtend, bool _useTableAliasesName = true)
        {
            this.dBExtend = _dbExtend;
            this.dBAdapter = _dbExtend._DBAdapter;
            this.useTableAliasesName = _useTableAliasesName;
            TypeCache.SetDBAdapterCache(typeof(T), this.dBAdapter);
            this.GetPrefix(typeof(T));
            this.QueryTableName = TypeCache.GetTableName(typeof(T));
        }
        /// <summary>
        /// 设置查询TOP
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        public LambdaQuery<T> Top(int top)
        {
            this.QueryTop = top;
            return this;
        }
        /// <summary>
        /// 设定分页参数
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public LambdaQuery<T> Page(int pageSize = 15, int pageIndex = 1)
        {
            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            return this;
        }
        /// <summary>
        /// 按条件排除字段
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public LambdaQuery<T> UnSelect(Predicate<FieldAttribute> match)
        {
            List<FieldAttribute> fields = TypeCache.GetProperties(typeof(T), false);
            if (match != null)
            {
                fields.RemoveAll(match);
            }
            string aliasName = this.GetPrefix(null);
            foreach (FieldAttribute item in fields)
            {
                item.SetFieldQueryScript(aliasName, true, false);
            }
            this.QueryFields = fields;
            return this;
        }
        /// <summary>
        /// 使用匿名类型选择查询字段
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="resultSelector">like b=&gt;new {b.Name}</param>
        /// <returns></returns>
        public LambdaQuery<T> Select2<TResult>(Expression<Func<T, TResult>> resultSelector)
        {
            List<FieldAttribute> allFilds2 = TypeCache.GetProperties(typeof(T), false);
            string aliasName = this.GetPrefix(null);
            List<FieldAttribute> resultFields = new List<FieldAttribute>();
            foreach (Expression item in (resultSelector.Body as NewExpression).Arguments)
            {
                string[] arry = item.ToString().Split(new char[]
				{
					'.'
				});
                FieldAttribute f = allFilds2.Find((FieldAttribute b) => b.Name == arry[1]);
                f.SetFieldQueryScript(aliasName, true, false);
                resultFields.Add(f);
            }
            this.QueryFields = resultFields;
            return this;
        }
        /// <summary>
        /// 设置查询字段
        /// </summary>
        /// <param name="expression">调用扩展方法SelectField</param>
        /// <returns></returns>
        public LambdaQuery<T> Select(Expression<Func<T, bool>> expression)
        {
            return this.Select(0, expression);
        }
        /// <summary>
        /// 设置查询字段
        /// </summary>
        /// <param name="top"></param>
        /// <param name="expression">调用扩展方法SelectField</param>
        /// <returns></returns>
        public LambdaQuery<T> Select(int top = 0, Expression<Func<T, bool>> expression = null)
        {
            string fields = "";
            if (expression != null)
            {
                MethodCallExpression mcExp = (MethodCallExpression)expression.Body;
                if (mcExp.Method.Name != "SelectField")
                {
                    throw new Exception("Select时发生错误,请调用SelectField扩展方法");
                }
                fields = this.visitor.RouteExpressionHandler(mcExp.Arguments[1], false);
                fields = string.Format(fields, "", "");
            }
            this.QueryTop = top;
            string[] propertys = fields.Split(new char[]
			{
				','
			});
            string aliasName = this.GetPrefix(null);
            this.QueryFields = TypeCache.GetProperties(typeof(T), false);
            this.QueryFields.ForEach(delegate(FieldAttribute b)
            {
                b.SetFieldQueryScript(aliasName, true, false);
            });
            if (fields.Length > 1)
            {
                List<FieldAttribute> queryFields2 = new List<FieldAttribute>();
                string[] array = propertys;
                for (int i = 0; i < array.Length; i++)
                {
                    string item = array[i];
                    FieldAttribute f = this.QueryFields.Find((FieldAttribute b) => b.Name.ToUpper() == item.ToUpper());
                    if (f == null)
                    {
                        throw new Exception("Select时发生错误,找不到对应的字段:" + item);
                    }
                    f.SetFieldQueryScript(aliasName, true, false);
                    queryFields2.Add(f);
                }
                this.QueryFields = queryFields2;
            }
            return this;
        }
        /// <summary>
        /// 设置条件 可累加，按and
        /// </summary>
        /// <param name="expression">最好用变量代替属性或方法</param>
        /// <returns></returns>
        public LambdaQuery<T> Where(Expression<Func<T, bool>> expression)
        {
            LambdaQuery<T> result;
            if (expression == null)
            {
                result = this;
            }
            else
            {
                if (this.QueryFields.Count == 0)
                {
                    this.Select(0, null);
                }
                string condition = this.FormatExpression(expression);
                this.Condition += (string.IsNullOrEmpty(this.Condition) ? condition : (" and " + condition));
                result = this;
            }
            return result;
        }
        /// <summary>
        /// 直接字符串查询
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        private LambdaQuery<T> Where(string condition)
        {
            this.Condition += (string.IsNullOrEmpty(this.Condition) ? condition : (" and " + condition));
            return this;
        }
        /// <summary>
        /// 设置排序 可累加
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="expression"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public LambdaQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> expression, bool desc)
        {
            MemberExpression mExp = (MemberExpression)expression.Body;
            if (!string.IsNullOrEmpty(this.QueryOrderBy))
            {
                this.QueryOrderBy += ",";
            }
            this.QueryOrderBy += string.Format(" {2}{0} {1}", mExp.Member.Name, desc ? "desc" : "asc", this.GetPrefix(null));
            return this;
        }
        /// <summary>
        /// 按主键排序
        /// </summary>
        /// <param name="desc"></param>
        /// <returns></returns>
        public LambdaQuery<T> OrderByPrimaryKey(bool desc)
        {
            if (!string.IsNullOrEmpty(this.QueryOrderBy))
            {
                this.QueryOrderBy += ",";
            }
            FieldAttribute key = TypeCache.GetProperties(typeof(T), true).Find((FieldAttribute b) => b.IsPrimaryKey);
            this.QueryOrderBy += string.Format(" {2}{0} {1}", key.Name, desc ? "desc" : "asc", this.GetPrefix(null));
            return this;
        }
        /// <summary>
        /// 按当前条件累加OR条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public LambdaQuery<T> Or(Expression<Func<T, bool>> expression)
        {
            string condition = this.FormatExpression(expression);
            this.Condition = string.Format("({0}) or {1}", this.Condition, condition);
            return this;
        }
        /// <summary>
        /// Join,并返回筛选值
        /// </summary>
        /// <typeparam name="TInner">关联类型</typeparam>
        /// <param name="expression">关关表达式</param>
        /// <param name="resultSelector">返回值</param>
        /// <param name="joinType">join类型,默认Left</param>
        /// <returns></returns>
        public LambdaQuery<T> Join<TInner>(Expression<Func<T, TInner, bool>> expression, Expression<Func<T, TInner, object>> resultSelector, JoinType joinType = JoinType.Left) where TInner : IModel, new()
        {
            Type innerType = typeof(TInner);
            string condition = this.FormatJoinExpression<TInner>(expression);
            List<FieldAttribute> allFilds = TypeCache.GetProperties(typeof(T), true);
            List<FieldAttribute> allFilds2 = TypeCache.GetProperties(innerType, true);
            Dictionary<string, Type> dic = new Dictionary<string, Type>();
            foreach (ParameterExpression item in resultSelector.Parameters)
            {
                dic.Add(item.Name, item.Type);
            }
            this.QueryFields.Clear();
            List<FieldAttribute> resultFields = new List<FieldAttribute>();
            NewExpression newExpression = resultSelector.Body as NewExpression;
            int i = 0;
            foreach (Expression item2 in newExpression.Arguments)
            {
                string[] arry = item2.ToString().Split(new char[]
				{
					'.'
				});
                string aliasName = arry[0];
                Type type = dic[aliasName];
                aliasName = this.GetPrefix(type);
                string name = newExpression.Members[i].Name;
                if (type == innerType)
                {
                    FieldAttribute f = allFilds2.Find((FieldAttribute b) => b.Name == arry[1]);
                    f.AliasesName = name;
                    if (resultFields.Find((FieldAttribute b) => b.ToString() == f.ToString()) != null)
                    {
                        throw new Exception("不能指定多次相同的字段" + f.Name);
                    }
                    resultFields.Add(f);
                }
                else
                {
                    FieldAttribute f = allFilds.Find((FieldAttribute b) => b.Name == arry[1]);
                    if (this.QueryFields.Find((FieldAttribute b) => b.ToString() == f.ToString()) != null)
                    {
                        throw new Exception("不能指定多次相同的字段" + f.Name);
                    }
                    f.AliasesName = name;
                    f.SetFieldQueryScript(this.GetPrefix(null), true, true);
                    this.QueryFields.Add(f);
                }
                i++;
            }
            this.AddInnerRelation(innerType, condition, resultFields, true, joinType);
            return this;
        }
        /// <summary>
        /// 存入关联值到对象内部索引
        /// </summary>
        /// <typeparam name="TInner">关联类型</typeparam>
        /// <param name="expression">关联表达式</param>
        /// <param name="resultSelector">关联的字段</param>
        /// <param name="joinType">join类型,默认Left</param>
        /// <returns></returns>
        public LambdaQuery<T> AppendJoinValue<TInner>(Expression<Func<T, TInner, bool>> expression, Expression<Func<TInner, object>> resultSelector, JoinType joinType = JoinType.Left) where TInner : IModel, new()
        {
            Type innerType = typeof(TInner);
            if (this.QueryFields.Count == 0)
            {
                this.Select(0, null);
            }
            string condition = this.FormatJoinExpression<TInner>(expression);
            List<FieldAttribute> allFilds2 = TypeCache.GetProperties(innerType, true);
            List<FieldAttribute> resultFields = new List<FieldAttribute>();
            NewExpression newExpression = resultSelector.Body as NewExpression;
            int i = 0;
            foreach (Expression item in newExpression.Arguments)
            {
                string[] arry = item.ToString().Split(new char[]
				{
					'.'
				});
                FieldAttribute f = allFilds2.Find((FieldAttribute b) => b.Name == arry[1]);
                f.MappingName = newExpression.Members[i].Name;
                resultFields.Add(f);
                i++;
            }
            this.AddInnerRelation(innerType, condition, resultFields, true, joinType);
            return this;
        }
        internal void AddInnerRelation(Type inner, string condition, List<FieldAttribute> resultFields, bool useAliasesName = true, JoinType joinType = JoinType.Left)
        {
            this.dBExtend.CheckTableCreated(inner);
            TableAttribute table = TypeCache.GetTable(inner);
            string tableName = table.TableName;
            string aliasName = this.GetPrefix(inner);
            tableName = string.Format("{0} {1} ", tableName, aliasName.Substring(0, aliasName.Length - 1));
            string str = string.Format(" {0} join {1} on {2}", joinType, tableName + " " + this.dBAdapter.GetWithNolockFormat(), condition);
            if (!this.Relations.ContainsKey(inner))
            {
                this.Relations.Add(inner, str);
            }
            foreach (FieldAttribute f in resultFields)
            {
                f.SetFieldQueryScript(aliasName, true, useAliasesName);
                this.FieldMapping[f.MappingName] = f.AliasesName;
                this.QueryFields.Add(f);
            }
        }
        /// <summary>
        /// 转换为SQL条件，并提取参数
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal string FormatExpression(Expression<Func<T, bool>> expression)
        {
            string result;
            if (expression == null)
            {
                result = "";
            }
            else
            {
                string condition;
                if (expression.Body is BinaryExpression)
                {
                    BinaryExpression be = (BinaryExpression)expression.Body;
                    condition = this.visitor.BinaryExpressionHandler(be.Left, be.Right, be.NodeType);
                }
                else
                {
                    condition = this.visitor.RouteExpressionHandler(expression.Body, false);
                }
                condition = string.Format(condition, this.GetPrefix(null), "");
                result = condition;
            }
            return result;
        }
        internal string FormatJoinExpression<TInner>(Expression<Func<T, TInner, bool>> expression) where TInner : IModel, new()
        {
            //JoinExpressionVisitor<T, TInner> joinVisitoer = new JoinExpressionVisitor<T, TInner>();
            string condition;
            if (expression.Body is BinaryExpression)
            {
                BinaryExpression be = (BinaryExpression)expression.Body;
                //condition = joinVisitoer.BinaryExpressionHandler(be.Left, be.Right, be.NodeType);
            }
            else
            {
                //condition = joinVisitoer.RouteExpressionHandler(expression.Body, false);
            }
            condition = "";//condition.Replace("[left]", this.GetPrefix(typeof(T)));
            return condition.Replace("[right]", this.GetPrefix(typeof(TInner)));
        }
        internal string GetQueryFieldString(Predicate<FieldAttribute> removes = null)
        {
            if (this.QueryFields.Count == 0)
            {
                this.Select(0, null);
            }
            List<FieldAttribute> queryFields = this.QueryFields;
            if (removes != null)
            {
                queryFields.RemoveAll(removes);
            }
            List<FieldAttribute> constraint = queryFields.FindAll((FieldAttribute b) => b.FieldType == FieldType.关联字段 || b.FieldType == FieldType.关联对象);
            int tabIndex = 2;
            using (List<FieldAttribute>.Enumerator enumerator = constraint.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    FieldAttribute a = enumerator.Current;
                    tabIndex++;
                    if (a.FieldType == FieldType.关联字段 && a.ConstraintType == null)
                    {
                        throw new Exception(string.Format("需指定关联类型:{0}.{1}.Attribute.Field.ConstraintType", typeof(T), a.Name));
                    }
                    if (!string.IsNullOrEmpty(a.ConstraintField))
                    {
                        string[] arry = a.ConstraintField.Replace("$", "").Split(new char[]
						{
							'='
						});
                        string leftField = this.GetPrefix(null) + arry[0];
                        Type innerType = a.ConstraintType;
                        TypeCache.SetDBAdapterCache(innerType, this.dBAdapter);
                        string rightField = this.GetPrefix(innerType) + arry[1];
                        string condition = string.Format("{0}={1}", leftField, rightField);
                        if (!string.IsNullOrEmpty(a.Constraint))
                        {
                            a.Constraint = Regex.Replace(a.Constraint, "(.+?)\\=", this.GetPrefix(innerType) + "$1=");
                            condition = condition + " and " + a.Constraint;
                        }
                        List<FieldAttribute> innerFields = TypeCache.GetProperties(innerType, true);
                        if (a.FieldType == FieldType.关联字段)
                        {
                            FieldAttribute resultField = innerFields.Find((FieldAttribute b) => b.Name.ToUpper() == a.ConstraintResultField.ToUpper());
                            if (resultField == null)
                            {
                                throw new Exception(string.Format("在类型{0}找不到 ConstraintResultField {1}", innerType, a.ConstraintResultField));
                            }
                            this.AddInnerRelation(innerType, condition, new List<FieldAttribute>
							{
								resultField
							}, true, JoinType.Left);
                        }
                        else
                        {
                            this.AddInnerRelation(innerType, condition, innerFields, true, JoinType.Left);
                        }
                    }
                }
            }
            queryFields = queryFields.FindAll((FieldAttribute b) => b.FieldType == FieldType.数据库字段 || b.FieldType == FieldType.虚拟字段);
            return Base.GetQueryFields(queryFields, false);
        }
        internal string GetQueryConditions()
        {
            string join = string.Join(" ", this.Relations.Values);
            string where = this.Condition;
            where = (string.IsNullOrEmpty(where) ? " 1=1 " : where);
            return string.Format(" {0} t1 {1}  {2}  where {3}", new object[]
			{
				this.QueryTableName,
				this.dBAdapter.GetWithNolockFormat(),
				join,
				where
			});
        }
        internal string GetOrderBy()
        {
            string orderBy = this.QueryOrderBy;
            orderBy = (string.IsNullOrEmpty(orderBy) ? orderBy : (" order by " + orderBy));
            if (string.IsNullOrEmpty(orderBy))
            {
                orderBy = TypeCache.GetTable(typeof(T)).DefaultSort;
            }
            return orderBy;
        }
        internal string GetQuery()
        {
            string fields = this.GetQueryFieldString(null);
            string part = " from " + this.GetQueryConditions();
            string orderBy = this.GetOrderBy();
            return this.dBAdapter.GetSelectTop(fields, part, orderBy, this.QueryTop);
        }
    }
}
