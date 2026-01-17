HƯỚNG DẪN CÀI ĐẶT  

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
