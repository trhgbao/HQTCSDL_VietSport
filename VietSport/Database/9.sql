USE VietSportDB;
GO

-- =====================================================
-- TINH HUONG 9: NON-REPEATABLE READ
-- Mo ta: Khach H (Platinum) thay giam 20%. Quan ly doi thanh 15%.
--        Khi H thanh toan, gia bi tinh la 15% khac voi luc xem.
-- =====================================================

-- =====================================================
-- SETUP DỮ LIỆU (Mock Data)
-- =====================================================
-- 1. Tao bang ChinhSachGiamGia
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChinhSachGiamGia')
BEGIN
    CREATE TABLE ChinhSachGiamGia (
        MaChinhSach INT IDENTITY(1,1) PRIMARY KEY,
        HangTV NVARCHAR(20) NOT NULL UNIQUE CHECK (HangTV IN ('Standard', 'Silver', 'Gold', 'Platinum')),
        GiamGia DECIMAL(5,2) NOT NULL CHECK (GiamGia >= 0 AND GiamGia <= 100),
        NgayCapNhat DATETIME DEFAULT GETDATE()
    );
END

-- 2. Reset Chinh Sach
DELETE FROM ChinhSachGiamGia;
INSERT INTO ChinhSachGiamGia (HangTV, GiamGia) VALUES
('Standard', 0), ('Silver', 5), ('Gold', 10), ('Platinum', 20); -- Reset ve 20%

-- 3. Tao Khach Hang H (Platinum)
IF NOT EXISTS (SELECT * FROM KhachHang WHERE MaKhachHang = 'KH_H')
BEGIN
    INSERT INTO KhachHang (MaKhachHang, HoTen, NgaySinh, SoDienThoai, Email, CapBacThanhVien, CMND, GioiTinh)
    VALUES ('KH_H', N'Nguyễn Văn H', '1990-05-15', '0901234567', 'khachhang.h@gmail.com', 'Platinum', '079090001234', N'Nam');
END

-- 4. Tao San SAN01 (neu chua co)
IF NOT EXISTS (SELECT * FROM CoSo WHERE MaCoSo = 'CS_HCM01')
    INSERT INTO CoSo (MaCoSo, TenCoSo, DiaChi, ThanhPho) VALUES ('CS_HCM01', N'VietSport Quận 1', N'123 Nguyễn Huệ', N'HCM');

IF NOT EXISTS (SELECT * FROM SanTheThao WHERE MaSan = 'SAN01')
    INSERT INTO SanTheThao (MaSan, MaCoSo, LoaiSan, SucChua, TinhTrang, GhiChu)
    VALUES ('SAN01', 'CS_HCM01', N'Bóng đá mini', 10, N'Còn trống', N'San bong da mini so 1');

-- 5. Tao Phieu Dat PDS_TH9
DELETE FROM PhieuDatSan WHERE MaPhieuDat = 'PDS_TH9';
INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, MaNhanVien, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
VALUES ('PDS_TH9', 'KH_H', 'SAN01', NULL, DATEADD(HOUR, 14, GETDATE()), DATEADD(HOUR, 16, GETDATE()), N'Chưa thanh toán', 'Online');
GO

-- =====================================================
-- STORED PROCEDURES
-- =====================================================

-- 1. Transaction 1 (T1): Quan ly cap nhat gia
CREATE OR ALTER PROCEDURE sp_CapNhatChinhSachGia
    @HangTV NVARCHAR(20),
    @MucGiamMoi DECIMAL(5,2),
    @KetQua NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRAN;
        -- Cap nhat muc giam gia
        UPDATE ChinhSachGiamGia
        SET GiamGia = @MucGiamMoi,
            NgayCapNhat = GETDATE()
        WHERE HangTV = @HangTV;
        COMMIT;
        SET @KetQua = N'Đã cập nhật giảm giá ' + @HangTV + N' thành ' + CAST(@MucGiamMoi AS NVARCHAR) + N'%';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @KetQua = N'Lỗi: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- 2. Transaction 2 (T2): Khach hang thanh toan (GAY RA LOI)
CREATE OR ALTER PROCEDURE sp_ThanhToanDonHang_CoLoi
    @MaKH VARCHAR(10),
    @MaPhieuDat VARCHAR(10),
    @KetQua NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @HangTV NVARCHAR(20);
    DECLARE @GiamGia1 DECIMAL(5,2);
    DECLARE @GiamGia2 DECIMAL(5,2);
    DECLARE @TongTienGoc DECIMAL(10,2) = 500000;
    DECLARE @TongTienSauGiam DECIMAL(10,2);

    -- SU DUNG READ COMMITTED (default) - SE BI LOI NON-REPEATABLE READ
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRAN;
        SELECT @HangTV = CapBacThanhVien FROM KhachHang WHERE MaKhachHang = @MaKH;

        -- LAN DOC 1
        SELECT @GiamGia1 = GiamGia FROM ChinhSachGiamGia WHERE HangTV = @HangTV;
        
        -- MO PHONG: Khach dang xem lai hoa don (10 giay) - Luc nay T1 chen vao update
        WAITFOR DELAY '00:00:10';

        -- LAN DOC 2
        SELECT @GiamGia2 = GiamGia FROM ChinhSachGiamGia WHERE HangTV = @HangTV;

        SET @TongTienSauGiam = @TongTienGoc * (100 - @GiamGia2) / 100;

        IF @GiamGia1 <> @GiamGia2
             SET @KetQua = N'LOI NON-REPEATABLE READ! Lan 1: ' + CAST(@GiamGia1 AS NVARCHAR) + N'%, Lan 2: ' + CAST(@GiamGia2 AS NVARCHAR) + N'%';
        ELSE
             SET @KetQua = N'Thanh toan thanh cong. Giam: ' + CAST(@GiamGia2 AS NVARCHAR) + N'%';

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @KetQua = N'Loi: ' + ERROR_MESSAGE();
    END CATCH
END;
GO
