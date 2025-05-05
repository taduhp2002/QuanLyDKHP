namespace QuanLyDKHP.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LichHoc")]
    public partial class LichHoc
    {
        [Key]
        public int ID_LH { get; set; }

        public int ID_LHP { get; set; }

        [Required]
        [StringLength(3)]
        public string Tiet { get; set; }

        public int Thu { get; set; }

        [StringLength(10)]
        public string PhongHoc { get; set; }

        [Column(TypeName = "date")]
        public DateTime TuNgay { get; set; }

        [Column(TypeName = "date")]
        public DateTime DenNgay { get; set; }

        public virtual LopHP LopHP { get; set; }
    }
}
