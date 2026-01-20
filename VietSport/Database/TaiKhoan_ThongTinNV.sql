USE VietSportDB;
GO

-- =====================================================
-- BƯỚC 1: TẠO DỮ LIỆU CƠ SỞ (Chi nhánh) TRƯỚC
-- (Phải chạy cái này thì mới có nơi để Nhân viên làm việc)
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM CoSo WHERE MaCoSo = 'CS_HCM01')
BEGIN
    INSERT INTO CoSo (MaCoSo, TenCoSo, DiaChi, ThanhPho)
    VALUES ('CS_HCM01', N'VietSport Quận 1', N'123 Nguyễn Huệ, Quận 1', N'TP.HCM');
    PRINT N'-> Đã tạo cơ sở CS_HCM01';
END
GO

-- =====================================================
-- BƯỚC 2: TẠO NHÂN VIÊN & TÀI KHOẢN (Script của bạn)
-- =====================================================

-- Xóa dữ liệu cũ (nếu có) để chạy lại cho sạch
DELETE FROM TaiKhoan WHERE MaNhanVien IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT');
DELETE FROM NhanVien WHERE MaNhanVien IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT');

-- Thêm Nhân viên (Lúc này 'CS_HCM01' đã tồn tại nên sẽ không lỗi nữa)
INSERT INTO NhanVien (MaNhanVien, HoTen, MaCoSo, ChucVu, CMND, SoDienThoai, GioiTinh, NgaySinh) VALUES 
('NV_QL', N'Trần Quản Lý', 'CS_HCM01', N'Quản lý', '001', '0901111111', N'Nam', '1990-01-01'),
('NV_LT', N'Lê Lễ Tân', 'CS_HCM01', N'Lễ tân', '002', '0902222222', N'Nữ', '1995-05-05'),
('NV_TN', N'Phạm Thu Ngân', 'CS_HCM01', N'Thu ngân', '003', '0903333333', N'Nữ', '1998-08-08'),
('NV_KT', N'Nguyễn Kỹ Thuật', 'CS_HCM01', N'Kỹ thuật', '004', '0904444444', N'Nam', '1992-02-02');

PRINT N'-> Đã thêm 4 nhân viên.';

-- Tạo Tài khoản
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, MaNhanVien) VALUES 
('quanly', '123', 'NV_QL'),
('letan', '123', 'NV_LT'),
('thungan', '123', 'NV_TN'),
('kythuat', '123', 'NV_KT');

PRINT N'-> Đã tạo tài khoản thành công.';
GO