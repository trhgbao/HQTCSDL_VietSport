USE VietSportDB;
GO

-- 1. Tạo Khách hàng mẫu
IF NOT EXISTS (SELECT * FROM KhachHang WHERE MaKhachHang = 'KH_TEST')
    INSERT INTO KhachHang (MaKhachHang, HoTen, SoDienThoai, CMND, NgaySinh, GioiTinh)
    VALUES ('KH_TEST', N'Khách Test 01', '0999000001', '999', '2000-01-01', N'Nam');

-- 2. Tạo Ca trực (Cho Quản lý xem)
DELETE FROM PhanCongCaTruc;
INSERT INTO PhanCongCaTruc (MaNhanVien, MaQuanLy, ThoiGianBatDau, ThoiGianKetThuc) VALUES
('NV_LT', 'NV_QL', DATEADD(hour, 6, GETDATE()), DATEADD(hour, 12, GETDATE())), -- Lễ tân làm sáng
('NV_TN', 'NV_QL', DATEADD(hour, 12, GETDATE()), DATEADD(hour, 18, GETDATE())); -- Thu ngân làm chiều

-- 3. Tạo Đơn nghỉ phép (Cho Quản lý duyệt)
DELETE FROM DonNghiPhep;
INSERT INTO DonNghiPhep (MaNhanVien, LyDo, TrangThaiDuyet, MaNguoiDuyet, MaNguoiThayThe) VALUES
('NV_KT', N'Bị ốm', N'Chờ duyệt', 'NV_QL', 'NV_LT');

-- 4. Tạo Phiếu đặt sân (Để Thu ngân/Lễ tân test)
-- Phiếu 1: Chưa thanh toán (Để Thu ngân test tính tiền)
INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
VALUES ('P_UNPAID', 'KH_TEST', 'SAN01', DATEADD(hour, 1, GETDATE()), DATEADD(hour, 3, GETDATE()), N'Chưa thanh toán', 'Online');

-- Phiếu 2: Đã thanh toán (Để Lễ tân test Check-in)
INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
VALUES ('P_PAID', 'KH_TEST', 'SAN01', DATEADD(day, 1, GETDATE()), DATEADD(day, 1, DATEADD(hour, 2, GETDATE())), N'Đã thanh toán', 'Trực tiếp');

GO