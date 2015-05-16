using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Applications.Entity
{
    public abstract class BaseEntity
    {
        public BaseEntity()
        {
            Valid = 1;
            AddTime = DateTime.Now;
            ModifiedTime = DateTime.Now;
        }

        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 添加人
        /// </summary>
        [Column("AddBy")]
        public long? AddBy { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        [Column("AddTime")]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 最后修改人
        /// </summary>
        [Column("LastModifiedBy")]
        public long? ModifiedBy { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [Column("LastModifiedTime")]
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 是否有效（1有效，0失效）
        /// </summary>
        [Column("Valid")]
        public int Valid { get; set; }

        /// <summary>
        /// 行版本
        /// </summary>
        [Column("RowVersion")]
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
