using CRL.Attribute;
using CRL.DBAdapter;
using System;
using System.Collections;
namespace CRL.LambdaQuery
{
    internal delegate void AddParameHandler(string name, object value);
    internal delegate string MethodHandler(string field, ref int parIndex, AddParameHandler addParame, params object[] args);

    internal class MethodAnalyze<T> where T : IModel, new()
    {
        private DBAdapterBase dBAdapter;
        public MethodAnalyze()
        {
            TableAttribute table = TypeCache.GetTable(typeof(T));
            this.dBAdapter = table.DBAdapter;
            if (this.dBAdapter == null)
            {
                throw new Exception("dBAdapter尚未初始化");
            }
        }
        public string Substring(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            return this.dBAdapter.SubstringFormat(field, (int)args[0], (int)args[1]);
        }
        public string StringLike(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            string text = string.Format("@like{0}", parIndex);
            addParame(text, args[0]);
            return this.dBAdapter.StringLikeFormat(field, text);
        }
        public string StringNotLike(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            string text = string.Format("@like{0}", parIndex);
            addParame(text, args[0]);
            return this.dBAdapter.StringNotLikeFormat(field, text);
        }
        public string StringContains(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            string text = string.Format("@contrains{0}", parIndex);
            addParame(text, args[0]);
            return this.dBAdapter.StringContainsFormat(field, text);
        }
        public string DateTimeBetween(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            string text = string.Format("@between{0}", parIndex);
            addParame(text, args[0]);
            parIndex++;
            string text2 = string.Format("@between{0}", parIndex);
            addParame(text2, args[1]);
            return this.dBAdapter.BetweenFormat(field, text, text2);
        }
        public string DateTimeDateDiff(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            string text = string.Format("@DateDiff{0}", parIndex);
            addParame(text, args[1]);
            return this.dBAdapter.DateDiffFormat(field, args[0].ToString(), text);
        }
        private string InFormat(object value, ref int parIndex, AddParameHandler addParame)
        {
            string text = "";
            if (value is string)
            {
                string text2 = string.Format("@in{0}", parIndex);
                addParame(text2, value);
                text = text2;
            }
            else if (value is string[])
            {
                IEnumerable enumerable = value as IEnumerable;
                foreach (object current in enumerable)
                {
                    string text2 = string.Format("@in{0}", parIndex);
                    addParame(text2, current);
                    parIndex++;
                    text += string.Format("{0},", text2);
                }
                if (text.Length > 1)
                {
                    text = text.Substring(0, text.Length - 1);
                }
            }
            else
            {
                IEnumerable enumerable = value as IEnumerable;
                foreach (object current in enumerable)
                {
                    string text2 = string.Format("@in{0}", parIndex);
                    addParame(text2, (int)current);
                    parIndex++;
                    text += string.Format("{0},", text2);
                }
                if (text.Length > 1)
                {
                    text = text.Substring(0, text.Length - 1);
                }
            }
            return text;
        }
        public string In(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            string parName = this.InFormat(args[0], ref parIndex, addParame);
            return this.dBAdapter.InFormat(field, parName);
        }
        public string NotIn(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            string parName = this.InFormat(args[0], ref parIndex, addParame);
            return this.dBAdapter.NotInFormat(field, parName);
        }
    }
}
