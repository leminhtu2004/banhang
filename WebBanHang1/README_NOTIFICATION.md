# Hệ thống Thông báo Real-time

## Mô tả
Hệ thống thông báo real-time cho phép admin gửi thông báo đến tất cả khách hàng khi thêm sản phẩm mới. Thông báo sẽ hiển thị ngay lập tức trên giao diện người dùng mà không cần refresh trang.

## Tính năng

### 1. Thông báo Real-time
- Sử dụng SignalR để gửi thông báo real-time
- Thông báo hiển thị dưới dạng toast notification
- Tự động biến mất sau 10 giây
- Có thể click để xem chi tiết sản phẩm

### 2. Quản lý Thông báo
- Xem danh sách tất cả thông báo
- Đánh dấu đã đọc từng thông báo
- Đánh dấu tất cả đã đọc
- Xóa thông báo không cần thiết
- Phân trang thông báo

### 3. Hiển thị Số lượng
- Badge hiển thị số thông báo chưa đọc
- Tự động cập nhật khi có thông báo mới
- Ẩn badge khi không có thông báo

## Cài đặt

### 1. Chạy SQL Script
```sql
-- Chạy file SQL/notification_tables.sql để tạo các bảng cần thiết
```

### 2. Cấu hình SignalR
SignalR đã được cấu hình trong `Program.cs`:
```csharp
// Đăng ký SignalR
builder.Services.AddSignalR();

// Cấu hình Hub
app.MapHub<NotificationHub>("/notificationHub");
```

### 3. Service Registration
Các service đã được đăng ký trong `Program.cs`:
```csharp
builder.Services.AddScoped<INotificationService, NotificationService>();
```

## Sử dụng

### 1. Admin thêm sản phẩm
Khi admin thêm sản phẩm mới trong `ProductsController.Create()`, hệ thống sẽ tự động:
- Tạo thông báo cho tất cả khách hàng
- Gửi thông báo real-time qua SignalR
- Hiển thị toast notification cho người dùng

### 2. Khách hàng nhận thông báo
- Thông báo sẽ hiển thị ngay lập tức khi admin thêm sản phẩm
- Click vào thông báo để xem chi tiết sản phẩm
- Số thông báo chưa đọc được hiển thị trên icon chuông

### 3. Quản lý thông báo
- Truy cập `/Notification/Index` để xem tất cả thông báo
- Click nút "✓" để đánh dấu đã đọc
- Click nút "🗑️" để xóa thông báo
- Click "Đánh dấu tất cả đã đọc" để đánh dấu tất cả

## Cấu trúc Database

### Bảng Notification
- `Id`: Khóa chính
- `Title`: Tiêu đề thông báo
- `Message`: Nội dung thông báo
- `Type`: Loại thông báo (PRODUCT_ADDED, etc.)
- `ImageUrl`: URL hình ảnh sản phẩm
- `LinkUrl`: Link đến trang chi tiết
- `CreatedAt`: Thời gian tạo
- `IsRead`: Đã đọc chưa
- `IsActive`: Còn hoạt động không
- `MaKH`: Mã khách hàng (null = broadcast to all)

### Bảng UserNotification
- `Id`: Khóa chính
- `MaKH`: Mã khách hàng
- `NotificationId`: ID thông báo
- `IsRead`: Đã đọc chưa
- `ReadAt`: Thời gian đọc

## API Endpoints

### GET /Notification/Index
- Hiển thị trang thông báo
- Phân trang thông báo

### POST /Notification/MarkAsRead
- Đánh dấu thông báo đã đọc
- Parameter: `notificationId`

### POST /Notification/MarkAllAsRead
- Đánh dấu tất cả thông báo đã đọc

### POST /Notification/Delete
- Xóa thông báo
- Parameter: `notificationId`

### GET /Notification/GetUnreadCount
- Lấy số thông báo chưa đọc
- Trả về JSON: `{ count: number }`

## SignalR Hub

### NotificationHub
- `/notificationHub`: Endpoint SignalR
- `ReceiveNotification`: Event nhận thông báo mới
- `JoinUserGroup`: Tham gia nhóm user
- `LeaveUserGroup`: Rời nhóm user

## JavaScript

### notification.js
- Kết nối SignalR
- Hiển thị toast notification
- Cập nhật số lượng thông báo
- Xử lý click vào thông báo

## Tùy chỉnh

### 1. Thêm loại thông báo mới
```csharp
// Trong NotificationService
public async Task BroadcastPromotionNotificationAsync(string title, string message, string linkUrl)
{
    await CreateNotificationAsync(title, message, "PROMOTION", null, linkUrl);
}
```

### 2. Thay đổi thời gian hiển thị toast
```javascript
// Trong notification.js
setTimeout(function() {
    $('.toast').last().remove();
}, 15000); // 15 giây thay vì 10 giây
```

### 3. Thêm âm thanh thông báo
- Thêm file âm thanh vào `/wwwroot/sounds/notification.mp3`
- JavaScript sẽ tự động phát âm thanh khi có thông báo mới

## Troubleshooting

### 1. Thông báo không hiển thị
- Kiểm tra kết nối SignalR trong Console
- Đảm bảo đã đăng nhập
- Kiểm tra quyền truy cập

### 2. Số thông báo không cập nhật
- Kiểm tra API `/Notification/GetUnreadCount`
- Đảm bảo session còn hiệu lực

### 3. SignalR không kết nối
- Kiểm tra endpoint `/notificationHub`
- Đảm bảo đã cấu hình SignalR trong Program.cs 