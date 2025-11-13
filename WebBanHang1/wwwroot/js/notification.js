// Notification handling with SignalR
$(document).ready(function() {
    // Chỉ khởi tạo SignalR và cập nhật badge khi user đã đăng nhập
    const isLoggedIn = $('#notificationCount').length > 0;
    
    if (!isLoggedIn) {
        console.log('User not logged in, skipping notification setup');
        return;
    }
    
    // Initialize SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();

    // Start connection
    connection.start().then(function () {
        console.log("SignalR connected successfully");
        
        // Lấy thông tin user hiện tại và join group
        $.get('/Notification/GetCurrentUser')
            .done(function(userInfo) {
                if (userInfo.isLoggedIn && userInfo.maKh) {
                    connection.invoke("JoinUserGroup", userInfo.maKh).catch(function (err) {
                        console.error("Failed to join user group:", err);
                    });
                    console.log("Joined user group for:", userInfo.maKh);
                }
            })
            .fail(function() {
                console.log("Failed to get current user info");
            });
    }).catch(function (err) {
        console.error("SignalR Connection Error: ", err);
    });

    // Handle incoming notifications
    connection.on("ReceiveNotification", function (notification) {
        console.log("Received notification:", notification);
        showNotification(notification);
        // Cập nhật số thông báo ngay lập tức khi nhận được thông báo mới
        updateNotificationCount();
        
        // Nếu là thông báo mới, thêm hiệu ứng đặc biệt
        if (notification.isNew) {
            // Thêm hiệu ứng nhấp nháy cho icon thông báo
            $('.notification-icon i').addClass('text-danger');
            setTimeout(() => {
                $('.notification-icon i').removeClass('text-danger');
            }, 2000);
        }
    });

    // Show notification toast
    function showNotification(notification) {
        // Create notification element
        const notificationHtml = `
            <div class="toast show" role="alert" aria-live="assertive" aria-atomic="true" data-bs-autohide="false">
                <div class="toast-header">
                    <img src="${notification.imageUrl || '/images/default-product.jpg'}" class="rounded me-2" alt="Product" style="width: 20px; height: 20px; object-fit: cover;">
                    <strong class="me-auto">${notification.title}</strong>
                    <small class="text-muted">${notification.createdAt}</small>
                    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
                <div class="toast-body">
                    ${notification.message}
                    ${notification.linkUrl ? `<br><a href="${notification.linkUrl}" class="btn btn-primary btn-sm mt-2">Xem chi tiết</a>` : ''}
                </div>
            </div>
        `;

        // Create toast container if it doesn't exist
        let toastContainer = $('#toastContainer');
        if (toastContainer.length === 0) {
            $('body').append('<div id="toastContainer" class="toast-container position-fixed top-0 end-0 p-3" style="z-index: 1055;"></div>');
            toastContainer = $('#toastContainer');
        }

        // Add notification to container
        toastContainer.append(notificationHtml);

        // Auto remove after 10 seconds
        setTimeout(function() {
            $('.toast').last().remove();
        }, 10000);

        // Play notification sound (optional)
        playNotificationSound();
    }

    // Play notification sound
    function playNotificationSound() {
        try {
            const audio = new Audio('/sounds/notification.mp3');
            audio.volume = 0.3;
            audio.play().catch(e => console.log('Audio play failed:', e));
        } catch (e) {
            console.log('Audio not supported');
        }
    }

    // Update notification count in header
    function updateNotificationCount() {
        $.get('/Notification/GetUnreadCount')
            .done(function(response) {
                const count = response.count;
                const $badge = $('#notificationCount');
                
                console.log("Updating notification count:", count);
                
                if (count > 0) {
                    $badge.text(count).show();
                    // Thêm hiệu ứng nhấp nháy cho badge khi có thông báo mới
                    $badge.addClass('notification-pulse');
                    setTimeout(() => {
                        $badge.removeClass('notification-pulse');
                    }, 3000);
                } else {
                    $badge.hide();
                }
            })
            .fail(function() {
                console.log('Failed to update notification count');
            });
    }

    // Initialize notification count on page load
    updateNotificationCount();

    // Cập nhật số thông báo định kỳ mỗi 30 giây (để đảm bảo đồng bộ)
    setInterval(updateNotificationCount, 30000);
    
    // Thêm debug log
    console.log('Notification system initialized for logged-in user');

    // Handle notification click
    $(document).on('click', '.toast', function() {
        const linkUrl = $(this).find('a').attr('href');
        if (linkUrl) {
            window.location.href = linkUrl;
        }
    });
    
    // Khi click vào icon thông báo, tự động đánh dấu tất cả đã đọc
    $(document).on('click', '#notificationLink', function(e) {
        // Chỉ thực hiện nếu có thông báo chưa đọc
        const $badge = $('#notificationCount');
        if ($badge.is(':visible') && parseInt($badge.text()) > 0) {
            e.preventDefault(); // Ngăn chặn chuyển trang ngay lập tức
            
            // Đánh dấu tất cả đã đọc
            $.post('/Notification/MarkAllAsRead')
                .done(function(response) {
                    if (response.success) {
                        // Ẩn badge ngay lập tức
                        $badge.hide();
                        console.log('Marked all notifications as read');
                        
                        // Sau đó chuyển đến trang thông báo
                        setTimeout(function() {
                            window.location.href = '/Notification/Index';
                        }, 100);
                    }
                })
                .fail(function() {
                    // Nếu lỗi, vẫn chuyển trang bình thường
                    console.log('Failed to mark notifications as read');
                });
        }
    });
}); 