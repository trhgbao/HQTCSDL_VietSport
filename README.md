Chức năng chính
Hệ thống phân quyền chi tiết cho 6 đối tượng:
1.  **Khách hàng:** Tìm kiếm sân, Đặt sân (Online/QR Code), Xem lịch sử.
2.  **Admin (Quản trị):** Cấu hình tham số hệ thống, Bảng giá, Quản lý hồ sơ nhân viên.
3.  **Quản lý (Manager):** Phân ca trực, Duyệt nghỉ phép, Xem báo cáo thống kê.
4.  **Lễ tân:** Check-in khách đến, Đặt sân trực tiếp, Hủy sân.
5.  **Thu ngân:** Tìm phiếu đặt, Thanh toán, In hóa đơn.
6.  **Kỹ thuật:** Cập nhật trạng thái sân (Bảo trì/Hoạt động).

## ⚙️ HƯỚNG DẪN CÀI ĐẶT  

Thành viên nhóm vui lòng làm theo đúng thứ tự để không bị lỗi.

### Bước 1: Cấu hình Cơ sở dữ liệu (SQL Server)
1.  Mở **SQL Server Management Studio (SSMS)**.
2.  Vào thư mục `Database` trong dự án.
3.  Chạy lần lượt các file script theo thứ tự:
    *   `1_Schema_TaoBang.sql` (Tạo bảng).
    *   `2_MockData_Full.sql` (Dữ liệu nền).
    *   Các script còn lại trong database đã tạo , nhằm để chèn các procedure cần thiết và data để test

### Bước 2: Cấu hình Kết nối (Connection String)
1.  Mở Solution bằng Visual Studio.
2.  Tìm file **`Core/DatabaseHelper.cs`**.
3.  Sửa dòng `ServerName` thành tên máy của bạn:
    ```csharp
    // Ví dụ tên máy bạn là DESKTOP-ABC hoặc (local)
    public static string ServerName = @".\SQLEXPRESS"; 
    ```
