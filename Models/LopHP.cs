namespace QuanLyDKHP.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LopHP")]
    public partial class LopHP
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public LopHP()
        {
            DKHPs = new HashSet<DKHP>();
            LichHocs = new HashSet<LichHoc>();
        }

        [Key]
        public int ID_LHP { get; set; }

        [Required]
        [StringLength(20)]
        public string MaLop { get; set; }

        public int ID_HP { get; set; }

        public int ID_HK { get; set; }

        public int ID_NH { get; set; }

        public int ID_GV { get; set; }

        public int SiSo { get; set; }

        public byte TrangThai { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DKHP> DKHPs { get; set; }

        public virtual GiangVien GiangVien { get; set; }

        public virtual HocKy HocKy { get; set; }

        public virtual HocPhan HocPhan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LichHoc> LichHocs { get; set; }

        public virtual NamHoc NamHoc { get; set; }
    }
}
