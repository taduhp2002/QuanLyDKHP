namespace QuanLyDKHP.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CauHinhDKHP")]
    public partial class CauHinhDKHP
    {
        public int ID { get; set; }

        public int SoTC_ToiDa { get; set; }


        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime NgayBatDau { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime NgayKetThuc { get; set; }

        public int ID_HK { get; set; }

        public int ID_NH { get; set; }

        public virtual HocKy HocKy { get; set; }

        public virtual NamHoc NamHoc { get; set; }
    }
}
