namespace QuanLyDKHP.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DKHP")]
    public partial class DKHP
    {
        [Key]
        public int ID_DK { get; set; }

        public int ID_SV { get; set; }

        public int ID_LHP { get; set; }

        public byte TrangThai { get; set; }

        public DateTime ThoiGianDangKy { get; set; }

        public DateTime? ThoiGianHuy { get; set; }

        public virtual LopHP LopHP { get; set; }

        public virtual SinhVien SinhVien { get; set; }
    }
}
