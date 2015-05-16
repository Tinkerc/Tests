using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
namespace CRL.Dynamic
{
    internal sealed class DapperRowMetaObject : DynamicMetaObject
    {
        private static readonly MethodInfo getValueMethod = typeof(IDictionary<string, object>).GetProperty("Item").GetGetMethod();
        private static readonly MethodInfo setValueMethod = typeof(DynamicObject).GetMethod("SetValue");
        public DapperRowMetaObject(Expression expression, BindingRestrictions restrictions)
            : base(expression, restrictions)
        {
        }
        public DapperRowMetaObject(Expression expression, BindingRestrictions restrictions, object value)
            : base(expression, restrictions, value)
        {
        }
        private DynamicMetaObject CallMethod(MethodInfo method, Expression[] parameters)
        {
            return new DynamicMetaObject(Expression.Call(Expression.Convert(base.Expression, base.LimitType), method, parameters), BindingRestrictions.GetTypeRestriction(base.Expression, base.LimitType));
        }
        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            Expression[] parameters = new Expression[]
			{
				Expression.Constant(binder.Name)
			};
            return this.CallMethod(DapperRowMetaObject.getValueMethod, parameters);
        }
        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            Expression[] parameters = new Expression[]
			{
				Expression.Constant(binder.Name),
				value.Expression
			};
            return this.CallMethod(DapperRowMetaObject.setValueMethod, parameters);
        }
    }
}
