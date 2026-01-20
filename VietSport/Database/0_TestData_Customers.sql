USE VietSportDB;
GO

-- =====================================================
-- BƯỚC 1: TẠO DỮ LIỆU KHÁCH HÀNG (Đã sửa lỗi thiếu cột)
-- =====================================================

-- Insert KH_H
IF NOT EXISTS (SELECT 1 FROM KhachHang WHERE MaKhachHang = 'KH_H')
BEGIN
    -- Đã thêm NgaySinh và CMND vào danh sách cột và giá trị
    INSERT INTO KhachHang (MaKhachHang, HoTen, NgaySinh, CMND, SoDienThoai, DiaChi, Email)
    VALUES ('KH_H', N'Nguyễn Văn H', '1990-01-01', '001090000001', '0912345678', N'Hà Nội', 'h@example.com');
    PRINT N'-> Đã thêm KH_H thành công';
END

-- Insert KH_M
IF NOT EXISTS (SELECT 1 FROM KhachHang WHERE MaKhachHang = 'KH_M')
BEGIN
    INSERT INTO KhachHang (MaKhachHang, HoTen, NgaySinh, CMND, SoDienThoai, DiaChi, Email)
    VALUES ('KH_M', N'Trần Thị M', '1995-05-05', '001095000002', '0912345679', N'Đà Nẵng', 'm@example.com');
    PRINT N'-> Đã thêm KH_M thành công';
END

-- Insert KH_N
IF NOT EXISTS (SELECT 1 FROM KhachHang WHERE MaKhachHang = 'KH_N')
BEGIN
    INSERT INTO KhachHang (MaKhachHang, HoTen, NgaySinh, CMND, SoDienThoai, DiaChi, Email)
    VALUES ('KH_N', N'Lê Văn N', '1992-02-02', '001092000003', '0912345680', N'TP.HCM', 'n@example.com');
    PRINT N'-> Đã thêm KH_N thành công';
END
GO

-- =====================================================
-- BƯỚC 2: TẠO TÀI KHOẢN
-- (Giữ nguyên như cũ, giờ sẽ chạy được vì Bước 1 đã OK)
-- =====================================================

-- Tài khoản cho KH_H
IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE MaKhachHang = 'KH_H')
BEGIN
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, MaNhanVien, MaKhachHang)
    VALUES ('khach_h', '123456', NULL, 'KH_H');
    PRINT N'Đã tạo tài khoản cho KH_H';
END
GO

-- Tài khoản cho KH_M
IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE MaKhachHang = 'KH_M')
BEGIN
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, MaNhanVien, MaKhachHang)
    VALUES ('khach_m', '123456', NULL, 'KH_M');
    PRINT N'Đã tạo tài khoản cho KH_M';
END
GO

-- Tài khoản cho KH_N
IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE MaKhachHang = 'KH_N')
BEGIN
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, MaNhanVien, MaKhachHang)
    VALUES ('khach_n', '123456', NULL, 'KH_N');
    PRINT N'Đã tạo tài khoản cho KH_N';
END
GO

-- Hiển thị kết quả kiểm tra
SELECT tk.TenDangNhap, kh.HoTen, kh.NgaySinh, kh.CMND
FROM TaiKhoan tk
JOIN KhachHang kh ON tk.MaKhachHang = kh.MaKhachHang
WHERE tk.MaKhachHang IN ('KH_H', 'KH_M', 'KH_N');

PRINT N'=== HOÀN TẤT ===';