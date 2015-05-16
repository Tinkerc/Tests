using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Applications.Entity;

namespace Applications.DataAccess
{
    public class MyContext : DbContext
    {
        public MyContext()
            : base("ConnectionStringName")
        {

        }

        public DbSet<UserEntity> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntity>().Property(p => p.RowVersion).IsRowVersion();
            modelBuilder.Entity<UserEntity>().Property(p => p.Name).IsConcurrencyToken();

            base.OnModelCreating(modelBuilder);
        }
    }
}
