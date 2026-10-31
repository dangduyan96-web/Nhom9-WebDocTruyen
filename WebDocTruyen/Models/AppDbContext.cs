using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebDocTruyen.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BinhLuanChuong> BinhLuanChuongs { get; set; }

    public virtual DbSet<ChuongTruyen> ChuongTruyens { get; set; }

    public virtual DbSet<DanhGiaTruyen> DanhGiaTruyens { get; set; }

    public virtual DbSet<GoiDoc> GoiDocs { get; set; }

    public virtual DbSet<KhuyenMai> KhuyenMais { get; set; }

    public virtual DbSet<LichSuGiaoDich> LichSuGiaoDiches { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<NguoiDungGoiDoc> NguoiDungGoiDocs { get; set; }

    public virtual DbSet<TheLoai> TheLoais { get; set; }

    public virtual DbSet<TienDoDoc> TienDoDocs { get; set; }

    public virtual DbSet<TinNhanHoTro> TinNhanHoTros { get; set; }

    public virtual DbSet<Truyen> Truyens { get; set; }

    public virtual DbSet<TruyenDaMua> TruyenDaMuas { get; set; }

    public virtual DbSet<TruyenYeuThich> TruyenYeuThiches { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

        => optionsBuilder.UseSqlServer("Server=db68785.public.databaseasp.net;Database=db68785;User Id=db68785;Password=6Nk?p3@MG!o7;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BinhLuanChuong>(entity =>
        {
            entity.HasKey(e => e.MaBinhLuan).HasName("PK__BinhLuan__87CB66A0EA703394");

            entity.ToTable("BinhLuanChuong");

            entity.HasIndex(e => new { e.MaChuong, e.NgayTao }, "IX_BinhLuanChuong_MaChuong").IsDescending(false, true);

            entity.Property(e => e.IsAn).HasDefaultValue(false);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaBinhLuanChaNavigation).WithMany(p => p.InverseMaBinhLuanChaNavigation)
                .HasForeignKey(d => d.MaBinhLuanCha)
                .HasConstraintName("FK_BLC_Cha");

            entity.HasOne(d => d.MaChuongNavigation).WithMany(p => p.BinhLuanChuongs)
                .HasForeignKey(d => d.MaChuong)
                .HasConstraintName("FK_BLC_Chuong");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.BinhLuanChuongs)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BLC_NguoiDung");
        });

        modelBuilder.Entity<ChuongTruyen>(entity =>
        {
            entity.HasKey(e => e.MaChuong).HasName("PK__ChuongTr__0D6A804C9556FE54");

            entity.ToTable("ChuongTruyen");

            entity.HasIndex(e => new { e.MaTruyen, e.SoThuTuChuong }, "IX_ChuongTruyen_MaTruyen_SoThuTu");

            entity.Property(e => e.LuotXem).HasDefaultValue(0);
            entity.Property(e => e.MienPhi).HasDefaultValue(true);
            entity.Property(e => e.NgayPhatHanh)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TieuDe).HasMaxLength(255);

            entity.HasOne(d => d.MaTruyenNavigation).WithMany(p => p.ChuongTruyens)
                .HasForeignKey(d => d.MaTruyen)
                .HasConstraintName("FK_Chuong_Truyen");
        });

        modelBuilder.Entity<DanhGiaTruyen>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK__DanhGiaT__AA9515BF7883F7B5");

            entity.ToTable("DanhGiaTruyen");

            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DanhGiaTruyens)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DGT_NguoiDung");

            entity.HasOne(d => d.MaTruyenNavigation).WithMany(p => p.DanhGiaTruyens)
                .HasForeignKey(d => d.MaTruyen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DGT_Truyen");
        });

        modelBuilder.Entity<GoiDoc>(entity =>
        {
            entity.HasKey(e => e.MaGoiDoc).HasName("PK__GoiDoc__4449576A497B7185");

            entity.ToTable("GoiDoc");

            entity.Property(e => e.GiaTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IsHoatDong).HasDefaultValue(true);
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenGoi).HasMaxLength(100);
        });

        modelBuilder.Entity<KhuyenMai>(entity =>
        {
            entity.HasKey(e => e.MaKhuyenMai).HasName("PK__KhuyenMa__6F56B3BD2FCEDFAF");

            entity.ToTable("KhuyenMai");

            entity.HasIndex(e => e.MaGiamGia, "UQ__KhuyenMa__EF9458E5C588D134").IsUnique();

            entity.Property(e => e.IsHoatDong).HasDefaultValue(true);
            entity.Property(e => e.MaGiamGia)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NgayBatDau).HasColumnType("datetime");
            entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");
            entity.Property(e => e.SoTienGiam).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<LichSuGiaoDich>(entity =>
        {
            entity.HasKey(e => e.MaGiaoDich).HasName("PK__LichSuGi__0A2A24EB6F03AB42");

            entity.ToTable("LichSuGiaoDich");

            entity.Property(e => e.LoaiGiaoDich).HasMaxLength(50);
            entity.Property(e => e.MaGiamGia)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhuongThucThanhToan)
                .HasMaxLength(50)
                .HasDefaultValue("VNPAY");
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai).HasMaxLength(20);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.LichSuGiaoDiches)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LSGD_NguoiDung");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__C539D762131E28D9");

            entity.ToTable("NguoiDung");

            entity.HasIndex(e => e.TenDangNhap, "UQ__NguoiDun__55F68FC0E7522676").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__NguoiDun__A9D10534513C2258").IsUnique();

            entity.Property(e => e.AnhDaiDien).HasMaxLength(500);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(255);
            entity.Property(e => e.MaVaiTro).HasDefaultValue(4);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RefreshToken).HasMaxLength(500);
            entity.Property(e => e.TenDangNhap).HasMaxLength(100);
            entity.Property(e => e.ThoiGianHetHanToken).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianHetHanTokenQuenMatKhau).HasColumnType("datetime");

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.NguoiDungs)
                .HasForeignKey(d => d.MaVaiTro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NguoiDung_VaiTro");
        });

        modelBuilder.Entity<NguoiDungGoiDoc>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDungGoiDoc).HasName("PK__NguoiDun__494D5990A9F9C93F");

            entity.ToTable("NguoiDungGoiDoc");

            entity.Property(e => e.NgayBatDau)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.MaGoiDocNavigation).WithMany(p => p.NguoiDungGoiDocs)
                .HasForeignKey(d => d.MaGoiDoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NDGD_GoiDoc");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.NguoiDungGoiDocs)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NDGD_NguoiDung");
        });

        modelBuilder.Entity<TheLoai>(entity =>
        {
            entity.HasKey(e => e.MaTheLoai).HasName("PK__TheLoai__D73FF34A9B265267");

            entity.ToTable("TheLoai");

            entity.HasIndex(e => e.TenTheLoai, "UQ__TheLoai__327F958F6067F09D").IsUnique();

            entity.HasIndex(e => e.Slug, "UQ__TheLoai__BC7B5FB648AEFD76").IsUnique();

            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.Slug)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TenTheLoai).HasMaxLength(100);
        });

        modelBuilder.Entity<TienDoDoc>(entity =>
        {
            entity.HasKey(e => new { e.MaNguoiDung, e.MaTruyen }).HasName("PK__TienDoDo__6394F7C6459F71F3");

            entity.ToTable("TienDoDoc");

            entity.HasIndex(e => new { e.MaNguoiDung, e.NgayCapNhat }, "IX_TienDoDoc_MaNguoiDung").IsDescending(false, true);

            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoLanDoc).HasDefaultValue(1);

            entity.HasOne(d => d.MaChuongGanNhatNavigation).WithMany(p => p.TienDoDocs)
                .HasForeignKey(d => d.MaChuongGanNhat)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TDD_Chuong");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.TienDoDocs)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TDD_NguoiDung");

            entity.HasOne(d => d.MaTruyenNavigation).WithMany(p => p.TienDoDocs)
                .HasForeignKey(d => d.MaTruyen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TDD_Truyen");
        });

        modelBuilder.Entity<TinNhanHoTro>(entity =>
        {
            entity.HasKey(e => e.MaTinNhan).HasName("PK__TinNhanH__E5B3062A2D32E492");

            entity.ToTable("TinNhanHoTro");

            entity.Property(e => e.DaDoc).HasDefaultValue(false);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.VaiTroNguoiGui).HasMaxLength(20);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.TinNhanHoTros)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TNHT_NguoiDung");
        });

        modelBuilder.Entity<Truyen>(entity =>
        {
            entity.HasKey(e => e.MaTruyen).HasName("PK__Truyen__6AD20A4BBD8C2239");

            entity.ToTable("Truyen");

            entity.HasIndex(e => e.Slug, "UQ__Truyen__BC7B5FB6641E1DF4").IsUnique();

            entity.Property(e => e.AnhBia).HasMaxLength(500);
            entity.Property(e => e.ChinhSachTruyCap)
                .HasMaxLength(50)
                .HasDefaultValue("Free");
            entity.Property(e => e.GiaBan)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LuotThich).HasDefaultValue(0);
            entity.Property(e => e.LuotXem).HasDefaultValue(0);
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Slug)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.TacGia).HasMaxLength(150);
            entity.Property(e => e.TenTruyen).HasMaxLength(255);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("Đang phát hành");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.Truyens)
                .HasForeignKey(d => d.NguoiTao)
                .HasConstraintName("FK_Truyen_NguoiDung");

            entity.HasMany(d => d.MaTheLoais).WithMany(p => p.MaTruyens)
                .UsingEntity<Dictionary<string, object>>(
                    "TruyenTheLoai",
                    r => r.HasOne<TheLoai>().WithMany()
                        .HasForeignKey("MaTheLoai")
                        .HasConstraintName("FK_TTL_TheLoai"),
                    l => l.HasOne<Truyen>().WithMany()
                        .HasForeignKey("MaTruyen")
                        .HasConstraintName("FK_TTL_Truyen"),
                    j =>
                    {
                        j.HasKey("MaTruyen", "MaTheLoai").HasName("PK__TruyenTh__D7A1F57F06703A3F");
                        j.ToTable("TruyenTheLoai");
                    });
        });

        modelBuilder.Entity<TruyenDaMua>(entity =>
        {
            entity.HasKey(e => new { e.MaNguoiDung, e.MaTruyen }).HasName("PK__TruyenDa__6394F7C6620AB730");

            entity.ToTable("TruyenDaMua");

            entity.Property(e => e.GiaDaTra).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NgayMua)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.TruyenDaMuas)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TDM_NguoiDung");

            entity.HasOne(d => d.MaTruyenNavigation).WithMany(p => p.TruyenDaMuas)
                .HasForeignKey(d => d.MaTruyen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TDM_Truyen");
        });

        modelBuilder.Entity<TruyenYeuThich>(entity =>
        {
            entity.HasKey(e => new { e.MaNguoiDung, e.MaTruyen }).HasName("PK__TruyenYe__6394F7C6D44554AB");

            entity.ToTable("TruyenYeuThich");

            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.TruyenYeuThiches)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TYT_NguoiDung");

            entity.HasOne(d => d.MaTruyenNavigation).WithMany(p => p.TruyenYeuThiches)
                .HasForeignKey(d => d.MaTruyen)
                .HasConstraintName("FK_TYT_Truyen");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CFF819B6A1");

            entity.ToTable("VaiTro");

            entity.HasIndex(e => e.TenVaiTro, "UQ__VaiTro__1DA5581409A94C2E").IsUnique();

            entity.Property(e => e.MaVaiTro).ValueGeneratedNever();
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenVaiTro).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
