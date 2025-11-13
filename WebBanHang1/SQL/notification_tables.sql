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
    MaKh NVARCHAR(20) NULL,
    FOREIGN KEY (MaKh) REFERENCES dbo.KhachHang(MaKh)
);

-- Tạo bảng UserNotification
CREATE TABLE dbo.UserNotification (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MaKh NVARCHAR(20) NOT NULL,
    NotificationId INT NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    ReadAt DATETIME NULL,
    FOREIGN KEY (MaKh) REFERENCES dbo.KhachHang(MaKh) ON DELETE CASCADE,
    FOREIGN KEY (NotificationId) REFERENCES dbo.Notification(Id) ON DELETE CASCADE
);

-- Tạo index để tối ưu hiệu suất
CREATE INDEX IX_Notification_CreatedAt ON dbo.Notification(CreatedAt);
CREATE INDEX IX_Notification_Type ON dbo.Notification(Type);
CREATE INDEX IX_Notification_IsActive ON dbo.Notification(IsActive);
CREATE INDEX IX_UserNotification_MaKh ON dbo.UserNotification(MaKh);
CREATE INDEX IX_UserNotification_IsRead ON dbo.UserNotification(IsRead);
CREATE INDEX IX_UserNotification_NotificationId ON dbo.UserNotification(NotificationId); 