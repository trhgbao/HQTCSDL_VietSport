USE VietSportDB;
GO

-- =====================================================
-- TINH HUONG 10: DIRTY READ
-- Mo ta: Ky thuat dang cap nhat bao tri san (chua commit).
--        Quan ly vao xem thay du lieu do dang, khong chinh xac.
-- =====================================================

-- =====================================================
-- SETUP DỮ LIỆU (Mock Data)
-- =====================================================
-- 1. Tao Co So
IF NOT EXISTS (SELECT * FROM CoSo WHERE MaCoSo = 'CS_HCM01')
    INSERT INTO CoSo (MaCoSo, TenCoSo, DiaChi, ThanhPho) VALUES ('CS_HCM01', N'VietSport Quận 1', N'123 Nguyễn Huệ', N'HCM');

-- 2. Tao San SAN_TH10
IF NOT EXISTS (SELECT * FROM SanTheThao WHERE MaSan = 'SAN_TH10')
BEGIN
    INSERT INTO SanTheThao (MaSan, MaCoSo, LoaiSan, SucChua, TinhTrang, GhiChu)
    VALUES ('SAN_TH10', 'CS_HCM01', N'Cầu lông', 4, N'Còn trống', N'San test tinh huong 10');
END
ELSE
BEGIN
    -- Reset trang thai ve Con trong
    UPDATE SanTheThao SET TinhTrang = N'Còn trống', GhiChu = NULL WHERE MaSan = 'SAN_TH10';
END
GO

-- =====================================================
-- STORED PROCEDURES
-- =====================================================

-- 1. Transaction 1 (T1): Ky thuat cap nhat bao tri (Chua Commit ngay)
CREATE OR ALTER PROCEDURE sp_CapNhatBaoTriSan
    @MaSan VARCHAR(10),
    @KetQua NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRAN;
        -- Update trang thai
        UPDATE SanTheThao
        SET TinhTrang = N'Bảo trì',
            GhiChu = N'Dang bao tri - Cap nhat luc ' + CONVERT(NVARCHAR, GETDATE(), 120)
        WHERE MaSan = @MaSan;

        -- MO PHONG: Chua commit, delay 10s de T2 doc thay Dirty Data
        WAITFOR DELAY '00:00:10';

        -- Sau 15s thi ROLLBACK (de chung minh T2 da doc du lieu SAI)
        ROLLBACK; 
        SET @KetQua = N'[ROLLBACK] Da huy cap nhat san ' + @MaSan + N' - San van o trang thai cu!';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @KetQua = N'Loi: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- 2. Transaction 2 (T2): Quan ly xem thong tin (GAY RA LOI DIRTY READ)
CREATE OR ALTER PROCEDURE sp_XemThongTinSan_CoLoi
    @MaCoSo VARCHAR(10),
    @KetQua NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- SU DUNG READ UNCOMMITTED - SE DOC DUOC DU LIEU CHUA COMMIT
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    BEGIN TRY
        BEGIN TRAN;
        -- Doc du lieu - co the doc duoc du lieu chua commit tu T1
        SELECT MaSan, LoaiSan, TinhTrang, GhiChu
        FROM SanTheThao
        WHERE MaCoSo = @MaCoSo;

        SET @KetQua = N'Da doc thong tin san';
        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @KetQua = N'Loi: ' + ERROR_MESSAGE();
    END CATCH
    
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
END;
GO
