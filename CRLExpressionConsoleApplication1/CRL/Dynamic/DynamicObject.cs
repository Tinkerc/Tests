using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
namespace CRL.Dynamic
{
    internal sealed class DynamicObject : IDynamicMetaObjectProvider, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
    {
        private Dictionary<string, object> values;
        int ICollection<KeyValuePair<string, object>>.Count
        {
            get
            {
                return this.values.Count<KeyValuePair<string, object>>();
            }
        }
        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }
        object IDictionary<string, object>.this[string key]
        {
            get
            {
                object val;
                this.TryGetValue(key, out val);
                return val;
            }
            set
            {
                this.SetValue(key, value);
            }
        }
        ICollection<string> IDictionary<string, object>.Keys
        {
            get
            {
                return (
                    from kv in this
                    select kv.Key).ToArray<string>();
            }
        }
        ICollection<object> IDictionary<string, object>.Values
        {
            get
            {
                return (
                    from kv in this
                    select kv.Value).ToArray<object>();
            }
        }
        public DynamicObject(DataRow dr)
        {
            this.values = new Dictionary<string, object>();
            foreach (DataColumn col in dr.Table.Columns)
            {
                this.values.Add(col.ColumnName, dr[col.ColumnName]);
            }
        }
        public bool TryGetValue(string name, out object value)
        {
            return this.values.TryGetValue(name, out value);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("{DapperRow");
            foreach (KeyValuePair<string, object> kv in this)
            {
                object value = kv.Value;
                sb.Append(", ").Append(kv.Key);
                if (value != null)
                {
                    sb.Append(" = '").Append(kv.Value).Append('\'');
                }
                else
                {
                    sb.Append(" = NULL");
                }
            }
            return sb.Append('}').ToString();
        }
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new DapperRowMetaObject(parameter, BindingRestrictions.Empty, this);
        }
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            ((IDictionary<string, object>)this).Add(item.Key, item.Value);
        }
        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            this.values.Clear();
        }
        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            object value;
            return this.TryGetValue(item.Key, out value) && object.Equals(value, item.Value);
        }
        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<string, object> kv in this)
            {
                array[arrayIndex++] = kv;
            }
        }
        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)this).Remove(item.Key);
        }
        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return this.values.ContainsKey(key);
        }
        void IDictionary<string, object>.Add(string key, object value)
        {
            ((IDictionary<string, object>)this)[key] = value;
        }
        bool IDictionary<string, object>.Remove(string key)
        {
            return this.values.Remove(key);
        }
        public object SetValue(string key, object value)
        {
            this.values[key] = value;
            return value;
        }
    }
}
