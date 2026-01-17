-- =====================================================
-- 0_TestData_Customers.sql
-- Script tạo tài khoản đăng nhập cho Khách hàng
-- Chạy file này để khách hàng có thể đăng nhập vào hệ thống
-- =====================================================
USE VietSportDB;
GO

-- Kiểm tra và Insert tài khoản cho các khách hàng đã có trong bảng KhachHang
-- Password mặc định: 123456 (không hash cho đơn giản)

-- Tài khoản cho KH_H (dùng trong Scenario 9)
IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE MaKhachHang = 'KH_H')
BEGIN
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, MaNhanVien, MaKhachHang)
    VALUES ('khach_h', '123456', NULL, 'KH_H');
    PRINT N'Đã tạo tài khoản cho KH_H: khach_h / 123456';
END
GO

-- Tài khoản cho KH_M (dùng trong Scenario 15 - T1)
IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE MaKhachHang = 'KH_M')
BEGIN
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, MaNhanVien, MaKhachHang)
    VALUES ('khach_m', '123456', NULL, 'KH_M');
    PRINT N'Đã tạo tài khoản cho KH_M: khach_m / 123456';
END
GO

-- Tài khoản cho KH_N (dùng trong Scenario 15 - T2)
IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE MaKhachHang = 'KH_N')
BEGIN
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, MaNhanVien, MaKhachHang)
    VALUES ('khach_n', '123456', NULL, 'KH_N');
    PRINT N'Đã tạo tài khoản cho KH_N: khach_n / 123456';
END
GO

-- Hiển thị kết quả
SELECT tk.TenDangNhap, tk.MatKhau, kh.HoTen, kh.MaKhachHang
FROM TaiKhoan tk
JOIN KhachHang kh ON tk.MaKhachHang = kh.MaKhachHang;

PRINT N'=== HOÀN TẤT TẠO TÀI KHOẢN KHÁCH HÀNG ===';
