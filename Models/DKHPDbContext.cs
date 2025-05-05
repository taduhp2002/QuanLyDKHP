namespace QuanLyDKHP.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DKHPDbContext : DbContext
    {
        public DKHPDbContext()
            : base("name=DKHPDbContext")
        {
        }

        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<CauHinhDKHP> CauHinhDKHPs { get; set; }
        public virtual DbSet<DKHP> DKHPs { get; set; }
        public virtual DbSet<GiangVien> GiangViens { get; set; }
        public virtual DbSet<HocKy> HocKies { get; set; }
        public virtual DbSet<HocPhan> HocPhans { get; set; }
        public virtual DbSet<LichHoc> LichHocs { get; set; }
        public virtual DbSet<LopHP> LopHPs { get; set; }
        public virtual DbSet<NamHoc> NamHocs { get; set; }
        public virtual DbSet<SinhVien> SinhViens { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>()
                .Property(e => e.MaAdmin)
                .IsUnicode(false);

            modelBuilder.Entity<Admin>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<Admin>()
                .Property(e => e.MatKhau)
                .IsUnicode(false);

            modelBuilder.Entity<GiangVien>()
                .Property(e => e.MaGV)
                .IsUnicode(false);

            modelBuilder.Entity<GiangVien>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<GiangVien>()
                .HasMany(e => e.LopHPs)
                .WithRequired(e => e.GiangVien)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<HocKy>()
                .HasMany(e => e.CauHinhDKHPs)
                .WithRequired(e => e.HocKy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<HocKy>()
                .HasMany(e => e.LopHPs)
                .WithRequired(e => e.HocKy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<HocPhan>()
                .Property(e => e.MaHP)
                .IsUnicode(false);

            modelBuilder.Entity<HocPhan>()
                .HasMany(e => e.LopHPs)
                .WithRequired(e => e.HocPhan)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<HocPhan>()
                .HasMany(e => e.HocPhan1)
                .WithMany(e => e.HocPhans)
                .Map(m => m.ToTable("DieuKienTienQuyet").MapLeftKey("ID_HP").MapRightKey("ID_HP_TienQuyet"));

            modelBuilder.Entity<LichHoc>()
                .Property(e => e.Tiet)
                .IsUnicode(false);

            modelBuilder.Entity<LichHoc>()
                .Property(e => e.PhongHoc)
                .IsUnicode(false);

            modelBuilder.Entity<LopHP>()
                .Property(e => e.MaLop)
                .IsUnicode(false);

            modelBuilder.Entity<LopHP>()
                .HasMany(e => e.DKHPs)
                .WithRequired(e => e.LopHP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<LopHP>()
                .HasMany(e => e.LichHocs)
                .WithRequired(e => e.LopHP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<NamHoc>()
                .Property(e => e.TenNH)
                .IsUnicode(false);

            modelBuilder.Entity<NamHoc>()
                .HasMany(e => e.CauHinhDKHPs)
                .WithRequired(e => e.NamHoc)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<NamHoc>()
                .HasMany(e => e.LopHPs)
                .WithRequired(e => e.NamHoc)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SinhVien>()
                .Property(e => e.MaSV)
                .IsUnicode(false);

            modelBuilder.Entity<SinhVien>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<SinhVien>()
                .Property(e => e.MatKhau)
                .IsUnicode(false);

            modelBuilder.Entity<SinhVien>()
                .Property(e => e.NienKhoa)
                .IsUnicode(false);

            modelBuilder.Entity<SinhVien>()
                .HasMany(e => e.DKHPs)
                .WithRequired(e => e.SinhVien)
                .WillCascadeOnDelete(false);
        }
    }
}
