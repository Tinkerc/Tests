using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Applications.DataAccess;
using Applications.Entity;

namespace Applications.Biz
{
    public class UserBiz
    {
        public List<UserEntity> GetUserList()
        {
            using (var context = new MyContext())
            {
                return context.Users.ToList();
            }
        }

        /// <summary>
        /// 根据条件查询实体内容
        /// </summary>
        /// <returns></returns>
        public List<UserEntity> GetUserByCondition(Expression<Func<UserEntity, bool>> predicate)
        {
            using (var context = new MyContext())
            {
                context.Database.SqlQuery("asdf", null);
                var result = context.Users.Where(predicate).ToList();

                return result;
            }
        }

        public bool Insert(UserEntity model)
        {
            bool flag = false;
            using (var context = new MyContext())
            {
                context.Users.Add(model);

                context.SaveChanges();

                flag = true;
            }

            return flag;
        }
    }
}
