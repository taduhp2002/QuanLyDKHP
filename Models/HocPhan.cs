namespace QuanLyDKHP.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("HocPhan")]
    public partial class HocPhan
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public HocPhan()
        {
            LopHPs = new HashSet<LopHP>();
            HocPhan1 = new HashSet<HocPhan>();
            HocPhans = new HashSet<HocPhan>();
        }

        [Key]
        public int ID_HP { get; set; }

        [Required]
        [StringLength(10)]
        public string MaHP { get; set; }

        [Required]
        [StringLength(50)]
        public string TenHP { get; set; }

        public int SoTC { get; set; }

        public string MoTa { get; set; }

        public byte LoaiHP { get; set; }

        public string DeCuong { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LopHP> LopHPs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HocPhan> HocPhan1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HocPhan> HocPhans { get; set; }
    }
}
