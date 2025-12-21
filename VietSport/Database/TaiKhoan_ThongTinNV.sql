USE VietSportDB;
GO

-- Xóa dữ liệu cũ để tránh trùng lặp
DELETE FROM TaiKhoan WHERE MaNhanVien IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT');
DELETE FROM NhanVien WHERE MaNhanVien IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT');

-- 1. Thêm Nhân viên đủ chức vụ (Pass chung: 123)
INSERT INTO NhanVien (MaNhanVien, HoTen, MaCoSo, ChucVu, CMND, SoDienThoai, GioiTinh, NgaySinh) VALUES 
('NV_QL', N'Trần Quản Lý', 'CS_HCM01', N'Quản lý', '001', '0901111111', N'Nam', '1990-01-01'),
('NV_LT', N'Lê Lễ Tân', 'CS_HCM01', N'Lễ tân', '002', '0902222222', N'Nữ', '1995-05-05'),
('NV_TN', N'Phạm Thu Ngân', 'CS_HCM01', N'Thu ngân', '003', '0903333333', N'Nữ', '1998-08-08'),
('NV_KT', N'Nguyễn Kỹ Thuật', 'CS_HCM01', N'Kỹ thuật', '004', '0904444444', N'Nam', '1992-02-02');

-- 2. Tạo Tài khoản đăng nhập
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, MaNhanVien) VALUES 
('quanly', '123', 'NV_QL'),
('letan', '123', 'NV_LT'),
('thungan', '123', 'NV_TN'),
('kythuat', '123', 'NV_KT');
GO