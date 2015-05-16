using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applications.Entity
{
    [Table("TUser")]
    public class UserEntity : BaseEntity
    {
        [ConcurrencyCheck]
        [Column("UserName")]
        public string Name { get; set; }

        [Column("Password")]
        public string Pwd { get; set; }

        [Column("Age")]
        public int Age { get; set; }
    }
}
