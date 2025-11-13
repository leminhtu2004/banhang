using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebBanHang1.Models;

namespace WebBanHang1.Data;

public partial class QuanLiHangContext : DbContext
{
    public QuanLiHangContext()
    {
    }

    public QuanLiHangContext(DbContextOptions<QuanLiHangContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiTietHd> ChiTietHds { get; set; }

    public virtual DbSet<EmailConfig> EmailConfigs { get; set; }

    public virtual DbSet<EmailVerification> EmailVerifications { get; set; }

    public virtual DbSet<GiamGium> GiamGia { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<GoogleAuth> GoogleAuths { get; set; }

    public virtual DbSet<HangHoa> HangHoas { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<KhachHangSuDungMa> KhachHangSuDungMas { get; set; }

    public virtual DbSet<Loai> Loais { get; set; }

    public virtual DbSet<LoginHistory> LoginHistories { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PasswordReset> PasswordResets { get; set; }

    public virtual DbSet<PhanQuyen> PhanQuyens { get; set; }

    public virtual DbSet<ProductRatingSummary> ProductRatingSummaries { get; set; }

    public virtual DbSet<ProductReview> ProductReviews { get; set; }

    public virtual DbSet<ReviewImage> ReviewImages { get; set; }

    public virtual DbSet<ReviewReaction> ReviewReactions { get; set; }

    public virtual DbSet<SanPhamGiamGium> SanPhamGiamGia { get; set; }

    public virtual DbSet<TrangThai> TrangThais { get; set; }

    public virtual DbSet<UserNotification> UserNotifications { get; set; }

    public virtual DbSet<VwProductRatingSummary> VwProductRatingSummaries { get; set; }

    public virtual DbSet<VwProductReview> VwProductReviews { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=localhost; Initial Catalog=QuanLiHang; Persist Security Info=True; User ID=sa; Password=123456; Trust Server Certificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTietHd>(entity =>
        {
            entity.HasKey(e => e.MaCt).HasName("PK__ChiTietH__27258E74637765B7");

            entity.ToTable("ChiTietHD");

            entity.Property(e => e.MaCt).HasColumnName("MaCT");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiamGia).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MaHd).HasColumnName("MaHD");
            entity.Property(e => e.MaHh).HasColumnName("MaHH");

            entity.HasOne(d => d.MaHdNavigation).WithMany(p => p.ChiTietHds)
                .HasForeignKey(d => d.MaHd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietHD__MaHD__5629CD9C");

            entity.HasOne(d => d.MaHhNavigation).WithMany(p => p.ChiTietHds)
                .HasForeignKey(d => d.MaHh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietHD__MaHH__571DF1D5");
        });

        modelBuilder.Entity<EmailConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EmailCon__3214EC0722E9A177");

            entity.ToTable("EmailConfig");

            entity.Property(e => e.GiaTri).HasMaxLength(500);
            entity.Property(e => e.MoTa).HasMaxLength(200);
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenCauHinh).HasMaxLength(50);
        });

        modelBuilder.Entity<EmailVerification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EmailVer__3214EC0701A2066E");

            entity.ToTable("EmailVerification");

            entity.HasIndex(e => e.VerificationCode, "IX_EmailVerification_Code");

            entity.HasIndex(e => e.NgayHetHan, "IX_EmailVerification_Expire");

            entity.HasIndex(e => e.MaKh, "IX_EmailVerification_MaKH");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.LoaiXacThuc).HasMaxLength(20);
            entity.Property(e => e.MaKh)
                .HasMaxLength(20)
                .HasColumnName("MaKH");
            entity.Property(e => e.NgayHetHan).HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.VerificationCode).HasMaxLength(100);

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.EmailVerifications)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK__EmailVerif__MaKH__1F98B2C1");
        });

        modelBuilder.Entity<GiamGium>(entity =>
        {
            entity.HasKey(e => e.MaGiamGia).HasName("PK__GiamGia__EF9458E4B0C54E48");

            entity.HasIndex(e => e.HieuLuc, "IX_GiamGia_HieuLuc");

            entity.HasIndex(e => new { e.NgayBatDau, e.NgayKetThuc }, "IX_GiamGia_NgayBatDau_NgayKetThuc");

            entity.Property(e => e.MaGiamGia).HasMaxLength(50);
            entity.Property(e => e.GiaTriGiam).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiaTriToiDa).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiaTriToiThieu).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HieuLuc).HasDefaultValue(true);
            entity.Property(e => e.LoaiGiamGia).HasMaxLength(50);
            entity.Property(e => e.MoTa).HasMaxLength(200);
            entity.Property(e => e.NgayBatDau).HasColumnType("datetime");
            entity.Property(e => e.NgayCapNhat).HasColumnType("datetime");
            entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NguoiTao).HasMaxLength(50);
            entity.Property(e => e.SoLuongDaSuDung).HasDefaultValue(0);
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.MaGh).HasName("PK__GioHang__2725AE8550C3308F");

            entity.ToTable("GioHang");

            entity.Property(e => e.MaGh).HasColumnName("MaGH");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaHh).HasColumnName("MaHH");
            entity.Property(e => e.MaKh)
                .HasMaxLength(20)
                .HasColumnName("MaKH");

            entity.HasOne(d => d.MaHhNavigation).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.MaHh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GioHang__MaHH__4E88ABD4");
        });

        modelBuilder.Entity<GoogleAuth>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GoogleAu__3214EC07255547A0");

            entity.ToTable("GoogleAuth");

            entity.HasIndex(e => e.GoogleId, "IX_GoogleAuth_GoogleId");

            entity.HasIndex(e => e.MaKh, "IX_GoogleAuth_MaKH");

            entity.HasIndex(e => e.GoogleId, "UQ__GoogleAu__A6FBF2FB9AC2BEB7").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.GoogleId).HasMaxLength(100);
            entity.Property(e => e.HieuLuc).HasDefaultValue(true);
            entity.Property(e => e.MaKh)
                .HasMaxLength(20)
                .HasColumnName("MaKH");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.NgayCapNhat).HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Picture).HasMaxLength(500);

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.GoogleAuths)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK__GoogleAuth__MaKH__1AD3FDA4");
        });

        modelBuilder.Entity<HangHoa>(entity =>
        {
            entity.HasKey(e => e.MaHh).HasName("PK__HangHoa__2725A6E4CF64E1D9");

            entity.ToTable("HangHoa");

            entity.Property(e => e.MaHh).HasColumnName("MaHH");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiamGia).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Hinh).HasMaxLength(255);
            entity.Property(e => e.MaNcc)
                .HasMaxLength(50)
                .HasColumnName("MaNCC");
            entity.Property(e => e.NgaySx).HasColumnName("NgaySX");
            entity.Property(e => e.TenHh)
                .HasMaxLength(100)
                .HasColumnName("TenHH");

            entity.HasOne(d => d.MaLoaiNavigation).WithMany(p => p.HangHoas)
                .HasForeignKey(d => d.MaLoai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HangHoa__MaLoai__45F365D3");

            entity.HasOne(d => d.MaNccNavigation).WithMany(p => p.HangHoas)
                .HasForeignKey(d => d.MaNcc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HangHoa__MaNCC__44FF419A");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.MaHd).HasName("PK__HoaDon__2725A6E0EF717EF7");

            entity.ToTable("HoaDon");

            entity.Property(e => e.MaHd).HasColumnName("MaHD");
            entity.Property(e => e.CachThanhToan).HasMaxLength(50);
            entity.Property(e => e.DiaChi).HasMaxLength(100);
            entity.Property(e => e.GiamGia)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaGiamGia).HasMaxLength(50);
            entity.Property(e => e.MaKh)
                .HasMaxLength(20)
                .HasColumnName("MaKH");
            entity.Property(e => e.NgayDat).HasColumnType("datetime");
            entity.Property(e => e.NgayGiao).HasColumnType("datetime");
            entity.Property(e => e.PhiVanChuyen).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaGiamGiaNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaGiamGia)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_HoaDon_GiamGia");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HoaDon__MaKH__4AB81AF0");

            entity.HasOne(d => d.MaTrangThaiNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaTrangThai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HoaDon_TrangThai");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKh).HasName("PK__KhachHan__2725CF1EC1B25D72");

            entity.ToTable("KhachHang");

            entity.HasIndex(e => e.Email, "IX_KhachHang_Email");

            entity.HasIndex(e => e.EmailVerified, "IX_KhachHang_EmailVerified");

            entity.HasIndex(e => e.Email, "UQ__KhachHan__A9D10534C25268B7").IsUnique();

            entity.Property(e => e.MaKh)
                .HasMaxLength(20)
                .HasColumnName("MaKH");
            entity.Property(e => e.DiaChi).HasMaxLength(100);
            entity.Property(e => e.DienThoai).HasMaxLength(15);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.HieuLuc).HasDefaultValue(true);
            entity.Property(e => e.Hinh).HasMaxLength(255);
            entity.Property(e => e.HoTen).HasMaxLength(50);
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.LockoutEnd).HasColumnType("datetime");
            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.NgayXacThucEmail).HasColumnType("datetime");
            entity.Property(e => e.RandomKey).HasMaxLength(50);
            entity.Property(e => e.TwoFactorSecret).HasMaxLength(100);
            entity.Property(e => e.VaiTro).HasDefaultValue((byte)1);

            entity.HasOne(d => d.VaiTroNavigation).WithMany(p => p.KhachHangs)
                .HasForeignKey(d => d.VaiTro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KhachHang_PhanQuyen");
        });

        modelBuilder.Entity<KhachHangSuDungMa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KhachHan__3214EC073DD9F3FA");

            entity.ToTable("KhachHangSuDungMa");

            entity.HasIndex(e => e.MaKh, "IX_KhachHangSuDungMa_MaKH");

            entity.Property(e => e.LanSuDungCuoi).HasColumnType("datetime");
            entity.Property(e => e.MaGiamGia).HasMaxLength(50);
            entity.Property(e => e.MaKh)
                .HasMaxLength(20)
                .HasColumnName("MaKH");

            entity.HasOne(d => d.MaGiamGiaNavigation).WithMany(p => p.KhachHangSuDungMas)
                .HasForeignKey(d => d.MaGiamGia)
                .HasConstraintName("FK__KhachHang__MaGia__0C85DE4D");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.KhachHangSuDungMas)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK__KhachHangS__MaKH__0D7A0286");
        });

        modelBuilder.Entity<Loai>(entity =>
        {
            entity.HasKey(e => e.MaLoai).HasName("PK__Loai__730A5759FCA344F1");

            entity.ToTable("Loai");

            entity.Property(e => e.Hinh).HasMaxLength(200);
            entity.Property(e => e.TenLoai).HasMaxLength(50);
            entity.Property(e => e.TenLoaiAlias).HasMaxLength(50);
        });

        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoginHis__3214EC07CFA5CACA");

            entity.ToTable("LoginHistory");

            entity.HasIndex(e => e.NgayDangNhap, "IX_LoginHistory_Date");

            entity.HasIndex(e => e.MaKh, "IX_LoginHistory_MaKH");

            entity.Property(e => e.GhiChu).HasMaxLength(200);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(45)
                .HasColumnName("IPAddress");
            entity.Property(e => e.LoaiDangNhap).HasMaxLength(20);
            entity.Property(e => e.MaKh)
                .HasMaxLength(20)
                .HasColumnName("MaKH");
            entity.Property(e => e.NgayDangNhap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ThanhCong).HasDefaultValue(true);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.LoginHistories)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK__LoginHisto__MaKH__2A164134");
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNcc).HasName("PK__NhaCungC__3A185DEBA3A71799");

            entity.ToTable("NhaCungCap");

            entity.Property(e => e.MaNcc)
                .HasMaxLength(50)
                .HasColumnName("MaNCC");
            entity.Property(e => e.DiaChi).HasMaxLength(50);
            entity.Property(e => e.DienThoai).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Logo).HasMaxLength(50);
            entity.Property(e => e.NguoiLienLac).HasMaxLength(50);
            entity.Property(e => e.TenCongTy).HasMaxLength(50);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC07E8903731");

            entity.ToTable("Notification");

            entity.HasIndex(e => e.CreatedAt, "IX_Notification_CreatedAt");

            entity.HasIndex(e => e.IsActive, "IX_Notification_IsActive");

            entity.HasIndex(e => e.Type, "IX_Notification_Type");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LinkUrl).HasMaxLength(200);
            entity.Property(e => e.MaKh)
                .HasMaxLength(20)
                .HasColumnName("MaKH");
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK__Notificati__MaKH__3864608B");
        });

        modelBuilder.Entity<PasswordReset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Password__3214EC0724AAB3F1");

            entity.ToTable("PasswordReset");

            entity.HasIndex(e => e.NgayHetHan, "IX_PasswordReset_Expire");

            entity.HasIndex(e => e.ResetToken, "IX_PasswordReset_Token");

            entity.HasIndex(e => e.ResetToken, "UQ__Password__0395685BBEBCF7DB").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.MaKh)
                .HasMaxLength(20)
                .HasColumnName("MaKH");
            entity.Property(e => e.NgayHetHan).HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ResetToken).HasMaxLength(100);

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.PasswordResets)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK__PasswordRe__MaKH__25518C17");
        });

        modelBuilder.Entity<PhanQuyen>(entity =>
        {
            entity.HasKey(e => e.VaiTro).HasName("PK__PhanQuye__4A1D9825279E3CDE");

            entity.ToTable("PhanQuyen");

            entity.Property(e => e.TenVaiTro).HasMaxLength(50);
        });

        modelBuilder.Entity<ProductRatingSummary>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductR__3214EC07032DEB51");

            entity.ToTable("ProductRatingSummary");

            entity.HasIndex(e => e.ProductId, "IX_ProductRatingSummary_ProductId");

            entity.Property(e => e.AverageRating).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductRatingSummaries)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductRa__Produ__00200768");
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductR__3214EC07CBD92472");

            entity.ToTable("ProductReview", tb => tb.HasTrigger("TR_ProductReview_UpdateRatingSummary"));

            entity.HasIndex(e => e.ParentReviewId, "IX_ProductReview_ParentReviewId");

            entity.HasIndex(e => e.ProductId, "IX_ProductReview_ProductId");

            entity.HasIndex(e => e.UserId, "IX_ProductReview_UserId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasMaxLength(20);

            entity.HasOne(d => d.ParentReview).WithMany(p => p.InverseParentReview)
                .HasForeignKey(d => d.ParentReviewId)
                .HasConstraintName("FK__ProductRe__Paren__619B8048");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductRe__Produ__5FB337D6");

            entity.HasOne(d => d.User).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductRe__UserI__60A75C0F");
        });

        modelBuilder.Entity<ReviewImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReviewIm__3214EC07A718DA88");

            entity.ToTable("ReviewImage", tb => tb.HasTrigger("TR_ReviewImage_UpdateRatingSummary"));

            entity.HasIndex(e => e.ReviewId, "IX_ReviewImage_ReviewId");

            entity.Property(e => e.ImageName).HasMaxLength(200);
            entity.Property(e => e.ImagePath).HasMaxLength(500);
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Review).WithMany(p => p.ReviewImages)
                .HasForeignKey(d => d.ReviewId)
                .HasConstraintName("FK__ReviewIma__Revie__72C60C4A");
        });

        modelBuilder.Entity<ReviewReaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReviewRe__3214EC07274DEFBE");

            entity.ToTable("ReviewReaction", tb => tb.HasTrigger("TR_ReviewReaction_UpdateRatingSummary"));

            entity.HasIndex(e => e.ReviewId, "IX_ReviewReaction_ReviewId");

            entity.HasIndex(e => e.UserId, "IX_ReviewReaction_UserId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasMaxLength(20);

            entity.HasOne(d => d.Review).WithMany(p => p.ReviewReactions)
                .HasForeignKey(d => d.ReviewId)
                .HasConstraintName("FK__ReviewRea__Revie__6D0D32F4");

            entity.HasOne(d => d.User).WithMany(p => p.ReviewReactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ReviewRea__UserI__6E01572D");
        });

        modelBuilder.Entity<SanPhamGiamGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SanPhamG__3214EC07B8D96684");

            entity.HasIndex(e => e.MaHh, "IX_SanPhamGiamGia_MaHH");

            entity.Property(e => e.MaGiamGia).HasMaxLength(50);
            entity.Property(e => e.MaHh).HasColumnName("MaHH");

            entity.HasOne(d => d.MaGiamGiaNavigation).WithMany(p => p.SanPhamGiamGia)
                .HasForeignKey(d => d.MaGiamGia)
                .HasConstraintName("FK__SanPhamGi__MaGia__07C12930");

            entity.HasOne(d => d.MaHhNavigation).WithMany(p => p.SanPhamGiamGia)
                .HasForeignKey(d => d.MaHh)
                .HasConstraintName("FK__SanPhamGia__MaHH__08B54D69");
        });

        modelBuilder.Entity<TrangThai>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai).HasName("PK__TrangTha__AADE41385BA2776F");

            entity.ToTable("TrangThai");

            entity.Property(e => e.MaTrangThai).ValueGeneratedNever();
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenTrangThai).HasMaxLength(50);
        });

        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserNoti__3214EC078327F34B");

            entity.ToTable("UserNotification");

            entity.HasIndex(e => e.IsRead, "IX_UserNotification_IsRead");

            entity.HasIndex(e => e.MaKh, "IX_UserNotification_MaKH");

            entity.HasIndex(e => e.NotificationId, "IX_UserNotification_NotificationId");

            entity.Property(e => e.MaKh)
                .HasMaxLength(20)
                .HasColumnName("MaKH");
            entity.Property(e => e.ReadAt).HasColumnType("datetime");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.UserNotifications)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK__UserNotifi__MaKH__3C34F16F");

            entity.HasOne(d => d.Notification).WithMany(p => p.UserNotifications)
                .HasForeignKey(d => d.NotificationId)
                .HasConstraintName("FK__UserNotif__Notif__3D2915A8");
        });

        modelBuilder.Entity<VwProductRatingSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ProductRatingSummary");

            entity.Property(e => e.AverageRating).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FiveStarPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.FourStarPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.GiamGia).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.LastUpdated).HasColumnType("datetime");
            entity.Property(e => e.OneStarPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ProductImage).HasMaxLength(255);
            entity.Property(e => e.ProductName).HasMaxLength(100);
            entity.Property(e => e.ThreeStarPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TwoStarPercentage).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<VwProductReview>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ProductReviews");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ParentUserName).HasMaxLength(50);
            entity.Property(e => e.UserAvatar).HasMaxLength(255);
            entity.Property(e => e.UserId).HasMaxLength(20);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(e => e.MaWishlist).HasName("PK__Wishlist__B290E8786FD5AA8E");

            entity.ToTable("Wishlist");

            entity.Property(e => e.MaHh).HasColumnName("MaHH");
            entity.Property(e => e.MaKh)
                .HasMaxLength(20)
                .HasColumnName("MaKH");
            entity.Property(e => e.NgayThem)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaHhNavigation).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.MaHh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Wishlist__MaHH__66603565");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Wishlist__MaKH__656C112C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
