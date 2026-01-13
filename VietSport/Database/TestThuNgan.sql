USE VietSportDB;
GO

-- 1. Tạo Khách hàng mẫu (Nếu chưa có)
IF NOT EXISTS (SELECT * FROM KhachHang WHERE MaKhachHang = 'KH_TEST')
BEGIN
    INSERT INTO KhachHang (MaKhachHang, HoTen, SoDienThoai, CMND, NgaySinh, GioiTinh)
    VALUES ('KH_TEST', N'Nguyễn Văn Test', '0999888777', '123123123', '2000-01-01', N'Nam');
END

-- 2. Tạo Sân mẫu (Nếu chưa có)
IF NOT EXISTS (SELECT * FROM SanTheThao WHERE MaSan = 'SAN_TEST')
BEGIN
    -- Cần có cơ sở trước
    IF NOT EXISTS (SELECT * FROM CoSo WHERE MaCoSo = 'CS_TEST')
        INSERT INTO CoSo (MaCoSo, TenCoSo, DiaChi, ThanhPho) VALUES ('CS_TEST', N'VietSport Test', N'123 Test Street', N'Test City');

    INSERT INTO SanTheThao (MaSan, MaCoSo, LoaiSan, SucChua, TinhTrang)
    VALUES ('SAN_TEST', 'CS_TEST', N'Bóng đá mini', 10, N'Còn trống');
    
    -- Thêm giá cho sân này luôn để tính tiền được
    INSERT INTO GiaThueSan (MaCoSo, LoaiSan, KhungGio, DonGia) VALUES ('CS_TEST', N'Bóng đá mini', N'Ngày thường', 200000);
END

-- 3. TẠO 3 PHIẾU ĐẶT SÂN (Trạng thái: Chưa thanh toán)
-- Phiếu 1: Đá hôm nay
INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
VALUES 
('P_TEST01', 'KH_TEST', 'SAN_TEST', DATEADD(hour, 1, GETDATE()), DATEADD(hour, 3, GETDATE()), N'Chưa thanh toán', 'Online');

-- Phiếu 2: Đá ngày mai
INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
VALUES 
('P_TEST02', 'KH_TEST', 'SAN_TEST', DATEADD(day, 1, GETDATE()), DATEADD(day, 1, DATEADD(hour, 2, GETDATE())), N'Chưa thanh toán', 'Trực tiếp');

-- Phiếu 3: Khách vãng lai
INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
VALUES 
('P_TEST03', 'KH_TEST', 'SAN01', GETDATE(), DATEADD(hour, 1, GETDATE()), N'Chưa thanh toán', 'Online');

GO