USE VietSportDB;
GO

-- Thêm nhân viên Quản trị (Admin)
INSERT INTO NhanVien (MaNhanVien, HoTen, MaCoSo, ChucVu, CMND, SoDienThoai, GioiTinh) VALUES 
('AD01', N'Super Admin', 'CS_HCM01', N'Quản trị', '999999999999', '0999999999', N'Nam');

-- Tạo tài khoản cho Admin
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, MaNhanVien) VALUES 
('admin', '123', 'AD01');

-- Đảm bảo bảng ThamSo có dữ liệu để chỉnh sửa
DELETE FROM ThamSo;
INSERT INTO ThamSo (MaThamSo, TenThamSo, GiaTri) VALUES 
('MIN_TIME', N'Thời gian đặt tối thiểu (phút)', '60'),
('MAX_TIME', N'Thời gian đặt tối đa (phút)', '180'),
('MAX_BOOK', N'Hạn mức số lượng sân/ngày', '3');
GO