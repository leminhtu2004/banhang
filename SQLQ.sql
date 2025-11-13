CREATE DATABASE QuanLiHang;
GO

USE QuanLiHang;
GO

-- Tạo bảng NhaCungCap trước
CREATE TABLE dbo.NhaCungCap (
    MaNCC NVARCHAR(50) PRIMARY KEY,
    TenCongTy NVARCHAR(50) NOT NULL,
    Logo NVARCHAR(50),
    NguoiLienLac NVARCHAR(50),
    Email NVARCHAR(50) NOT NULL,
    DienThoai NVARCHAR(50),
    DiaChi NVARCHAR(50),
    MoTa NVARCHAR(MAX)
);
GO

CREATE TABLE dbo.Loai (
    MaLoai INT IDENTITY(1,1) PRIMARY KEY,
    TenLoai NVARCHAR(50) NOT NULL,
    TenLoaiAlias NVARCHAR(50),
    MoTa NVARCHAR(MAX),
    Hinh NVARCHAR(200)
);
GO

-- Tạo bảng KhachHang
CREATE TABLE dbo.KhachHang (
    MaKH NVARCHAR(20) PRIMARY KEY,
    MatKhau NVARCHAR(255) NOT NULL,
    HoTen NVARCHAR(50) NOT NULL,
    GioiTinh BIT NOT NULL,
    NgaySinh DATE NOT NULL,
    DiaChi NVARCHAR(100) NULL,
    DienThoai NVARCHAR(15) NULL,
    Email NVARCHAR(50) NOT NULL UNIQUE,
    Hinh NVARCHAR(255) NULL,
    HieuLuc BIT NOT NULL DEFAULT 1,
    VaiTro TINYINT NOT NULL DEFAULT 1,
    RandomKey NVARCHAR(50) NULL
);
GO

-- Tạo bảng HangHoa sau khi đã có NhaCungCap
CREATE TABLE dbo.HangHoa (
    MaHH INT IDENTITY(1,1) PRIMARY KEY,
    TenHH NVARCHAR(100) NOT NULL,
    MaLoai INT NOT NULL, -- Đảm bảo cột này có kiểu dữ liệu INT
    Hinh NVARCHAR(255) NULL,
    DonGia DECIMAL(18,2) NOT NULL CHECK (DonGia >= 0),
    MoTa NVARCHAR(MAX) NULL,
    NgaySX DATE NOT NULL,
    GiamGia DECIMAL(5,2) NOT NULL DEFAULT 0 CHECK (GiamGia >= 0 AND GiamGia <= 100),
    SoLanXem INT NOT NULL DEFAULT 0,
    MaNCC NVARCHAR(50) NOT NULL,
    SoLuong INT NOT NULL DEFAULT 0, -- Thêm cột SoLuong

    FOREIGN KEY (MaNCC) REFERENCES dbo.NhaCungCap(MaNCC),
    FOREIGN KEY (MaLoai) REFERENCES dbo.Loai(MaLoai) 
);
GO

-- Tạo bảng HoaDon
CREATE TABLE dbo.HoaDon (
    MaHD INT IDENTITY(1,1) PRIMARY KEY,
    MaKH NVARCHAR(20) NOT NULL,
    NgayDat DATETIME NOT NULL,
    NgayGiao DATETIME NULL,
    DiaChi NVARCHAR(100) NOT NULL,
    CachThanhToan NVARCHAR(50) NOT NULL,
    PhiVanChuyen DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (PhiVanChuyen >= 0),
    MaTrangThai INT NOT NULL, -- Thêm cột MaTrangThai vào bảng HoaDon
    FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH)
);
GO

-- Tạo bảng GioHang
CREATE TABLE dbo.GioHang (
    MaGH INT IDENTITY(1,1) PRIMARY KEY,
    MaKH NVARCHAR(20) NOT NULL,
    MaHH INT NOT NULL,
    SoLuong SMALLINT NOT NULL CHECK (SoLuong > 0),
    DonGia DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (MaHH) REFERENCES dbo.HangHoa(MaHH)
);
GO

-- Tạo bảng PhanQuyen
CREATE TABLE dbo.PhanQuyen (
    VaiTro TINYINT PRIMARY KEY,
    TenVaiTro NVARCHAR(50) NOT NULL
);
GO

-- Tạo bảng ChiTietHD
CREATE TABLE dbo.ChiTietHD (
    MaCT INT IDENTITY(1,1) PRIMARY KEY,
    MaHD INT NOT NULL,
    MaHH INT NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL,
    SoLuong SMALLINT NOT NULL CHECK (SoLuong > 0),
    GiamGia DECIMAL(5,2) NOT NULL DEFAULT 0 CHECK (GiamGia >= 0 AND GiamGia <= 100),
    FOREIGN KEY (MaHD) REFERENCES dbo.HoaDon(MaHD),
    FOREIGN KEY (MaHH) REFERENCES dbo.HangHoa(MaHH)
);
GO

-- Tạo bảng TrangThai
CREATE TABLE dbo.TrangThai (
    MaTrangThai INT PRIMARY KEY,
    TenTrangThai NVARCHAR(50) NOT NULL,
    MoTa NVARCHAR(500)
);
GO

CREATE TABLE dbo.GiamGia (
    MaGiamGia NVARCHAR(50) PRIMARY KEY,
    GiaTriGiam DECIMAL(18,2) NOT NULL, -- Sửa đổi kiểu dữ liệu từ FLOAT sang DECIMAL
    NgayBatDau DATETIME NOT NULL,
    NgayKetThuc DATETIME NOT NULL,
    LoaiGiamGia NVARCHAR(50) NOT NULL -- Thêm cột LoaiGiamGia
);
GO

CREATE TABLE dbo.ProductReview (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    UserId NVARCHAR(20) NOT NULL,
    Rating INT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment NVARCHAR(MAX) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    ParentReviewId INT NULL, -- Thêm cột ParentReviewId
    FOREIGN KEY (ProductId) REFERENCES dbo.HangHoa(MaHH),
    FOREIGN KEY (UserId) REFERENCES dbo.KhachHang(MaKH),
    FOREIGN KEY (ParentReviewId) REFERENCES dbo.ProductReview(Id) ON DELETE NO ACTION -- Thêm khóa ngoại cho ParentReviewId
);
GO
CREATE TABLE dbo.Wishlist (
    MaWishlist INT IDENTITY(1,1) PRIMARY KEY,
    MaKH NVARCHAR(20) NOT NULL,
    MaHH INT NOT NULL,
    NgayThem DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH),
    FOREIGN KEY (MaHH) REFERENCES dbo.HangHoa(MaHH)
);
GO

-- Thêm dữ liệu vào bảng NhaCungCap
INSERT INTO dbo.NhaCungCap (MaNCC, TenCongTy, Logo, NguoiLienLac, Email, DienThoai, DiaChi, MoTa) 
VALUES 
    (N'NCC001', N'Công Ty Sách ABC', N'abc_logo.jpg', N'Nguyễn Văn A', N'abc@example.com', N'0123456789', N'123 Đường Sách, TP.HCM', N'Chuyên cung cấp sách văn học'),
    (N'NCC002', N'Công Ty Sách XYZ', N'xyz_logo.jpg', N'Trần Văn B', N'xyz@example.com', N'0987654321', N'456 Đường Sách, Hà Nội', N'Chuyên cung cấp sách khoa học');
GO

-- Thêm dữ liệu vào bảng PhanQuyen
INSERT INTO dbo.PhanQuyen (VaiTro, TenVaiTro) 
VALUES 
    (1, N'Khách Hàng'),  -- Vai trò khách hàng
    (2, N'Quản Trị Viên'); -- Vai trò admin
GO

-- Thêm khóa ngoại cho bảng KhachHang
ALTER TABLE dbo.KhachHang
ADD CONSTRAINT FK_KhachHang_PhanQuyen 
FOREIGN KEY (VaiTro) REFERENCES dbo.PhanQuyen(VaiTro);
GO

-- Thêm khóa ngoại cho bảng HoaDon
ALTER TABLE dbo.HoaDon
ADD CONSTRAINT FK_HoaDon_TrangThai 
FOREIGN KEY (MaTrangThai) REFERENCES dbo.TrangThai(MaTrangThai) 
ON UPDATE CASCADE;
GO

ALTER TABLE dbo.HoaDon
ADD MaGiamGia NVARCHAR(50) NULL; -- Thêm trường mã giảm giá
GO

ALTER TABLE dbo.HoaDon
ADD CONSTRAINT FK_HoaDon_GiamGia 
FOREIGN KEY (MaGiamGia) REFERENCES dbo.GiamGia(MaGiamGia) 
ON UPDATE CASCADE 
ON DELETE SET NULL; -- Nếu mã giảm giá bị xóa, trường này sẽ được đặt thành NULL
GO
-- Tạo bảng ReviewReaction để lưu like/dislike
CREATE TABLE dbo.ReviewReaction (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReviewId INT NOT NULL,
    UserId NVARCHAR(20) NOT NULL,
    Type INT NOT NULL, -- 1: Like, 2: Dislike
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    
    FOREIGN KEY (ReviewId) REFERENCES dbo.ProductReview(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES dbo.KhachHang(MaKH) ON DELETE CASCADE
);
GO

-- Tạo bảng ReviewImage để lưu hình ảnh review
CREATE TABLE dbo.ReviewImage (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReviewId INT NOT NULL,
    ImagePath NVARCHAR(500) NOT NULL,
    ImageName NVARCHAR(200) NOT NULL,
    UploadedAt DATETIME NOT NULL DEFAULT GETDATE(),
    IsMain BIT NOT NULL DEFAULT 0,
    
    FOREIGN KEY (ReviewId) REFERENCES dbo.ProductReview(Id) ON DELETE CASCADE
);
GO

-- Tạo bảng ProductRatingSummary để lưu thống kê rating
CREATE TABLE dbo.ProductRatingSummary (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    AverageRating DECIMAL(3,2) NOT NULL DEFAULT 0,
    TotalReviews INT NOT NULL DEFAULT 0,
    FiveStarCount INT NOT NULL DEFAULT 0,
    FourStarCount INT NOT NULL DEFAULT 0,
    ThreeStarCount INT NOT NULL DEFAULT 0,
    TwoStarCount INT NOT NULL DEFAULT 0,
    OneStarCount INT NOT NULL DEFAULT 0,
    TotalLikes INT NOT NULL DEFAULT 0,
    TotalDislikes INT NOT NULL DEFAULT 0,
    TotalImages INT NOT NULL DEFAULT 0,
    LastUpdated DATETIME NOT NULL DEFAULT GETDATE(),
    
    FOREIGN KEY (ProductId) REFERENCES dbo.HangHoa(MaHH) ON DELETE CASCADE
);
GO

-- Thêm dữ liệu vào bảng TrangThai
INSERT INTO dbo.TrangThai (MaTrangThai, TenTrangThai, MoTa) VALUES
(1, N'Đang chờ xử lý', N'Đơn hàng đang chờ xử lý bởi admin'),
(2, N'Xử lý thành công', N'Đơn hàng đã được xử lý thành công');
GO

-- Sửa cột Rating cho phép NULL và thêm constraint mới
ALTER TABLE dbo.ProductReview 
ALTER COLUMN Rating INT NULL;

ALTER TABLE dbo.ProductReview 
ADD CONSTRAINT CK_Rating_Range 
CHECK (
    (ParentReviewId IS NOT NULL AND Rating IS NULL) -- Reply không có rating
    OR 
    (ParentReviewId IS NULL AND Rating BETWEEN 1 AND 5) -- Đánh giá chính phải có rating 1-5
);
GO

-- Thêm ràng buộc NOT NULL cho các trường bắt buộc
ALTER TABLE dbo.ProductReview 
ALTER COLUMN ProductId INT NOT NULL;

ALTER TABLE dbo.ProductReview 
ALTER COLUMN UserId NVARCHAR(20) NOT NULL;

ALTER TABLE dbo.ProductReview 
ALTER COLUMN CreatedAt DATETIME NOT NULL;
GO

-- Sửa đổi khóa ngoại cho bảng ProductReview
ALTER TABLE dbo.ProductReview
DROP CONSTRAINT FK__ProductRe__Produ__5EBF139D;

ALTER TABLE dbo.ProductReview
ADD CONSTRAINT FK__ProductRe__Produ__5EBF139D
FOREIGN KEY (ProductId) REFERENCES dbo.HangHoa(MaHH)
ON DELETE CASCADE;
GO
-- Cập nhật bảng GiamGia với các trường mới
ALTER TABLE dbo.GiamGia
ADD MoTa NVARCHAR(200) NULL,
    GiaTriToiThieu DECIMAL(18,2) NULL,
    GiaTriToiDa DECIMAL(18,2) NULL,
    SoLuongSuDung INT NULL,
    SoLuongDaSuDung INT NULL DEFAULT 0,
    HieuLuc BIT NOT NULL DEFAULT 1,
    MotLanSuDung BIT NOT NULL DEFAULT 0,
    SoLanSuDungToiDaMoiKhachHang INT NULL,
    NgayTao DATETIME NULL DEFAULT GETDATE(),
    NgayCapNhat DATETIME NULL,
    NguoiTao NVARCHAR(50) NULL;
GO

-- Tạo bảng SanPhamGiamGia để liên kết mã giảm giá với sản phẩm
CREATE TABLE dbo.SanPhamGiamGia (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MaGiamGia NVARCHAR(50) NOT NULL,
    MaHH INT NOT NULL,
    FOREIGN KEY (MaGiamGia) REFERENCES dbo.GiamGia(MaGiamGia) ON DELETE CASCADE,
    FOREIGN KEY (MaHH) REFERENCES dbo.HangHoa(MaHH) ON DELETE CASCADE
);
GO

-- Tạo bảng KhachHangSuDungMa để theo dõi việc sử dụng mã giảm giá của khách hàng
CREATE TABLE dbo.KhachHangSuDungMa (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MaGiamGia NVARCHAR(50) NOT NULL,
    MaKH NVARCHAR(20) NOT NULL,
    SoLanSuDung INT NOT NULL DEFAULT 0,
    LanSuDungCuoi DATETIME NULL,
    FOREIGN KEY (MaGiamGia) REFERENCES dbo.GiamGia(MaGiamGia) ON DELETE CASCADE,
    FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH) ON DELETE CASCADE
);
GO

-- Thêm các ràng buộc cho bảng GiamGia
ALTER TABLE dbo.GiamGia
ADD CONSTRAINT CK_GiamGia_GiaTriToiThieu CHECK (GiaTriToiThieu IS NULL OR GiaTriToiThieu >= 0),
    CONSTRAINT CK_GiamGia_GiaTriToiDa CHECK (GiaTriToiDa IS NULL OR GiaTriToiDa >= 0),
    CONSTRAINT CK_GiamGia_SoLuongSuDung CHECK (SoLuongSuDung IS NULL OR SoLuongSuDung > 0),
    CONSTRAINT CK_GiamGia_SoLuongDaSuDung CHECK (SoLuongDaSuDung IS NULL OR SoLuongDaSuDung >= 0),
    CONSTRAINT CK_GiamGia_SoLanSuDungToiDaMoiKhachHang CHECK (SoLanSuDungToiDaMoiKhachHang IS NULL OR SoLanSuDungToiDaMoiKhachHang > 0),
    CONSTRAINT CK_GiamGia_NgayKetThuc CHECK (NgayKetThuc > NgayBatDau);
GO

-- Thêm index cho các bảng để tối ưu hiệu suất truy vấn
CREATE INDEX IX_GiamGia_HieuLuc ON dbo.GiamGia(HieuLuc);
CREATE INDEX IX_GiamGia_NgayBatDau_NgayKetThuc ON dbo.GiamGia(NgayBatDau, NgayKetThuc);
CREATE INDEX IX_SanPhamGiamGia_MaHH ON dbo.SanPhamGiamGia(MaHH);
CREATE INDEX IX_KhachHangSuDungMa_MaKH ON dbo.KhachHangSuDungMa(MaKH);
GO

-- Thêm cột GiamGia vào bảng HoaDon để lưu giá trị giảm giá thực tế
ALTER TABLE dbo.HoaDon
ADD GiamGia DECIMAL(18,2) NULL DEFAULT 0;
GO

-- Thêm ràng buộc cho cột GiamGia trong bảng HoaDon
ALTER TABLE dbo.HoaDon
ADD CONSTRAINT CK_HoaDon_GiamGia CHECK (GiamGia IS NULL OR GiamGia >= 0);
GO
-- Thêm ràng buộc cho cột GiamGia trong bảng HoaDon
ALTER TABLE dbo.HoaDon
ADD CONSTRAINT CK_HoaDon_GiamGia CHECK (GiamGia IS NULL OR GiamGia >= 0);
GO

-- Bổ sung các bảng cho chức năng đăng nhập Google, quên mật khẩu và xác thực email

-- Bảng lưu thông tin đăng nhập Google
CREATE TABLE dbo.GoogleAuth (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MaKH NVARCHAR(20) NOT NULL,
    GoogleId NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL,
    Name NVARCHAR(100) NULL,
    Picture NVARCHAR(500) NULL,
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    NgayCapNhat DATETIME NULL,
    HieuLuc BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH) ON DELETE CASCADE
);
GO

-- Bảng lưu mã xác thực email
CREATE TABLE dbo.EmailVerification (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MaKH NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    VerificationCode NVARCHAR(10) NOT NULL,
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    NgayHetHan DATETIME NOT NULL,
    DaSuDung BIT NOT NULL DEFAULT 0,
    LoaiXacThuc NVARCHAR(20) NOT NULL, -- 'EMAIL_VERIFICATION', 'PASSWORD_RESET'
    FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH) ON DELETE CASCADE
);
GO

-- Bảng lưu thông tin quên mật khẩu
CREATE TABLE dbo.PasswordReset (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MaKH NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    ResetToken NVARCHAR(100) NOT NULL UNIQUE,
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    NgayHetHan DATETIME NOT NULL,
    DaSuDung BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH) ON DELETE CASCADE
);
GO

-- Bảng lưu lịch sử đăng nhập
CREATE TABLE dbo.LoginHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MaKH NVARCHAR(20) NOT NULL,
    LoaiDangNhap NVARCHAR(20) NOT NULL, -- 'LOCAL', 'GOOGLE', 'FACEBOOK'
    IPAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    NgayDangNhap DATETIME NOT NULL DEFAULT GETDATE(),
    ThanhCong BIT NOT NULL DEFAULT 1,
    GhiChu NVARCHAR(200) NULL,
    FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH) ON DELETE CASCADE
);
GO

-- Bảng cấu hình email
CREATE TABLE dbo.EmailConfig (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenCauHinh NVARCHAR(50) NOT NULL,
    GiaTri NVARCHAR(500) NOT NULL,
    MoTa NVARCHAR(200) NULL,
    NgayCapNhat DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Thêm dữ liệu mẫu cho cấu hình email
INSERT INTO dbo.EmailConfig (TenCauHinh, GiaTri, MoTa) VALUES
('SMTP_HOST', 'smtp.gmail.com', 'SMTP server host'),
('SMTP_PORT', '587', 'SMTP server port'),
('SMTP_USERNAME', 'your-email@gmail.com', 'Email username'),
('SMTP_PASSWORD', 'your-app-password', 'Email password'),
('SMTP_ENABLE_SSL', 'true', 'Enable SSL for SMTP'),
('FROM_EMAIL', 'noreply@yourdomain.com', 'From email address'),
('FROM_NAME', 'WebBanHang', 'From name'),
('EMAIL_VERIFICATION_EXPIRE_MINUTES', '30', 'Email verification code expire time in minutes'),
('PASSWORD_RESET_EXPIRE_MINUTES', '60', 'Password reset token expire time in minutes');
GO

-- Thêm các cột mới vào bảng KhachHang
ALTER TABLE dbo.KhachHang
ADD EmailVerified BIT NOT NULL DEFAULT 0,
    NgayXacThucEmail DATETIME NULL,
    LastLoginDate DATETIME NULL,
    LoginAttempts INT NOT NULL DEFAULT 0,
    LockoutEnd DATETIME NULL,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    TwoFactorSecret NVARCHAR(100) NULL;
GO

-- Tạo các index để tối ưu hiệu suất
CREATE INDEX IX_GoogleAuth_GoogleId ON dbo.GoogleAuth(GoogleId);
CREATE INDEX IX_GoogleAuth_MaKH ON dbo.GoogleAuth(MaKH);
CREATE INDEX IX_EmailVerification_MaKH ON dbo.EmailVerification(MaKH);
CREATE INDEX IX_EmailVerification_Code ON dbo.EmailVerification(VerificationCode);
CREATE INDEX IX_EmailVerification_Expire ON dbo.EmailVerification(NgayHetHan);
CREATE INDEX IX_PasswordReset_Token ON dbo.PasswordReset(ResetToken);
CREATE INDEX IX_PasswordReset_Expire ON dbo.PasswordReset(NgayHetHan);
CREATE INDEX IX_LoginHistory_MaKH ON dbo.LoginHistory(MaKH);
CREATE INDEX IX_LoginHistory_Date ON dbo.LoginHistory(NgayDangNhap);
CREATE INDEX IX_KhachHang_Email ON dbo.KhachHang(Email);
CREATE INDEX IX_KhachHang_EmailVerified ON dbo.KhachHang(EmailVerified);
GO

-- Thêm ràng buộc cho các bảng mới
ALTER TABLE dbo.EmailVerification
ADD CONSTRAINT CK_EmailVerification_LoaiXacThuc 
CHECK (LoaiXacThuc IN ('EMAIL_VERIFICATION', 'PASSWORD_RESET'));

ALTER TABLE dbo.LoginHistory
ADD CONSTRAINT CK_LoginHistory_LoaiDangNhap 
CHECK (LoaiDangNhap IN ('LOCAL', 'GOOGLE', 'FACEBOOK'));

ALTER TABLE dbo.KhachHang
ADD CONSTRAINT CK_KhachHang_LoginAttempts 
CHECK (LoginAttempts >= 0);
GO
   ALTER TABLE dbo.EmailVerification
   ALTER COLUMN VerificationCode NVARCHAR(100) NOT NULL;
   
-- Tạo bảng Notification
CREATE TABLE dbo.Notification (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(500) NULL,
    Type NVARCHAR(50) NOT NULL,
    ImageUrl NVARCHAR(100) NULL,
    LinkUrl NVARCHAR(200) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    IsRead BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    MaKH NVARCHAR(20) NULL,
    FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH)
);

-- Tạo bảng UserNotification
CREATE TABLE dbo.UserNotification (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MaKH NVARCHAR(20) NOT NULL,
    NotificationId INT NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    ReadAt DATETIME NULL,
    FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH) ON DELETE CASCADE,
    FOREIGN KEY (NotificationId) REFERENCES dbo.Notification(Id) ON DELETE CASCADE
);

-- Tạo index để tối ưu hiệu suất
CREATE INDEX IX_Notification_CreatedAt ON dbo.Notification(CreatedAt);
CREATE INDEX IX_Notification_Type ON dbo.Notification(Type);
CREATE INDEX IX_Notification_IsActive ON dbo.Notification(IsActive);
CREATE INDEX IX_UserNotification_MaKH ON dbo.UserNotification(MaKH);
CREATE INDEX IX_UserNotification_IsRead ON dbo.UserNotification(IsRead);
CREATE INDEX IX_UserNotification_NotificationId ON dbo.UserNotification(NotificationId);



