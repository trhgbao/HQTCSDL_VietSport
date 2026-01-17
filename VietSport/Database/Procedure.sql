USE VietSportDB;
GO

-- =============================================================
-- TỪ FILE: 1-Proc.sql
-- (Demo cơ bản về đặt sân có lỗi và đã fix)
-- =============================================================

CREATE OR ALTER PROCEDURE sp_DatSan_Demo_Loi
    @MaKhachHang VARCHAR(10),
    @MaSan VARCHAR(10),
    @GioBatDau DATETIME,
    @GioKetThuc DATETIME,
    @MaPhieuDat VARCHAR(10)
AS
BEGIN
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRAN
        
        DECLARE @DemSoLuong INT;
        SELECT @DemSoLuong = COUNT(*)
        FROM PhieuDatSan
        WHERE MaSan = @MaSan
          AND (
              (@GioBatDau >= GioBatDau AND @GioBatDau < GioKetThuc) 
              OR 
              (@GioKetThuc > GioBatDau AND @GioKetThuc <= GioKetThuc) 
              OR
              (@GioBatDau <= GioBatDau AND @GioKetThuc >= GioKetThuc) 
          );

        WAITFOR DELAY '00:00:10'; 

        IF (@DemSoLuong = 0)
        BEGIN
            INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, KenhDat, TrangThaiThanhToan)
            VALUES (@MaPhieuDat, @MaKhachHang, @MaSan, @GioBatDau, @GioKetThuc, 'Online', N'Chưa thanh toán');
            
            -- Trả về thành công
            SELECT 1 AS KetQua, N'Đặt sân thành công (Lỗi Demo)' AS ThongBao;
        
        END
        ELSE
        BEGIN
            -- Trả về thất bại
            SELECT 0 AS KetQua, N'Sân đã bị trùng giờ!' AS ThongBao;
        END

    COMMIT TRAN
END
GO

CREATE OR ALTER PROCEDURE sp_DatSan_Demo_Fix
    @MaKhachHang VARCHAR(10),
    @MaSan VARCHAR(10),
    @GioBatDau DATETIME,
    @GioKetThuc DATETIME,
    @MaPhieuDat VARCHAR(10)
AS
BEGIN
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; 

    BEGIN TRAN
        
        BEGIN TRY
            DECLARE @DemSoLuong INT;
            SELECT @DemSoLuong = COUNT(*)
            FROM PhieuDatSan
            WHERE MaSan = @MaSan
              AND (
                  (@GioBatDau >= GioBatDau AND @GioBatDau < GioKetThuc)
                  OR 
                  (@GioKetThuc > GioBatDau AND @GioKetThuc <= GioKetThuc)
                  OR
                  (@GioBatDau <= GioBatDau AND @GioKetThuc >= GioKetThuc)
              );

            WAITFOR DELAY '00:00:10'; 

            IF (@DemSoLuong = 0)
            BEGIN
                INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, KenhDat, TrangThaiThanhToan)
                VALUES (@MaPhieuDat, @MaKhachHang, @MaSan, @GioBatDau, @GioKetThuc, 'Online', N'Chưa thanh toán');
                
                SELECT 1 AS KetQua, N'Đặt sân thành công (Đã Fix)' AS ThongBao;
            END
            ELSE
            BEGIN
                SELECT 0 AS KetQua, N'Sân đã bị trùng giờ!' AS ThongBao;
            END

            COMMIT TRAN
        END TRY
        BEGIN CATCH
            ROLLBACK TRAN;
            SELECT -1 AS KetQua, ERROR_MESSAGE() AS ThongBao;
        END CATCH
END
GO

-- =============================================================
-- TỪ FILE: 2-Proc.sql
-- (Scenario 2: Non-Repeatable Read - Khách đặt vs Kỹ thuật bảo trì)
-- =============================================================

-- 1. Thủ tục Đặt sân (ĐÃ SỬA CÚ PHÁP BEGIN TRY)
CREATE OR ALTER PROCEDURE sp_Demo_DatSan
    @MaPhieu VARCHAR(20),
    @MaKH VARCHAR(10),
    @MaSan VARCHAR(10),
    @GioBatDau DATETIME,
    @GioKetThuc DATETIME
AS
BEGIN
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    -- BẮT ĐẦU KHỐI TRY (Phải có chữ BEGIN)
    BEGIN TRY
        BEGIN TRANSACTION; -- Bắt đầu giao dịch

        -- BƯỚC 1: Kiểm tra trạng thái sân lần 1
        DECLARE @TinhTrangHienTai NVARCHAR(50);
        SELECT @TinhTrangHienTai = TinhTrang FROM SanTheThao WHERE MaSan = @MaSan;

        IF @TinhTrangHienTai = N'Bảo trì'
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50001, N'Lỗi ngay từ đầu: Sân đang bảo trì, không thể chọn!', 1;
            RETURN;
        END

        -- GIẢ LẬP ĐỘ TRỄ 10 GIÂY
        WAITFOR DELAY '00:00:10'; 

        -- BƯỚC 2: Kiểm tra lại lần nữa (Repeatable Read Check)
        SELECT @TinhTrangHienTai = TinhTrang FROM SanTheThao WHERE MaSan = @MaSan;

        IF @TinhTrangHienTai = N'Bảo trì'
        BEGIN
            ROLLBACK TRANSACTION; -- Hủy giao dịch vì dữ liệu đã bị thay đổi
            THROW 50002, N'Rất tiếc! Kỹ thuật viên vừa báo bảo trì sân này. Vui lòng chọn sân khác.', 1;
            RETURN;
        END

        -- Nếu ổn thì Insert
        INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
        VALUES (@MaPhieu, @MaKH, @MaSan, @GioBatDau, @GioKetThuc, N'Chưa thanh toán', 'Online');

        COMMIT TRANSACTION; -- Chốt giao dịch
    END TRY
    
    -- BẮT ĐẦU KHỐI CATCH (Phải có chữ BEGIN)
    BEGIN CATCH
        -- Nếu có lỗi xảy ra và giao dịch vẫn đang mở thì Rollback
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        
        -- Ném thông báo lỗi ra ngoài cho C# bắt
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO

-- 2. Thủ tục Bảo trì (Giữ nguyên, nhưng tôi paste lại cho chắc chắn)
CREATE OR ALTER PROCEDURE sp_Demo_BaoTri
    @MaSan VARCHAR(10)
AS
BEGIN
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        UPDATE SanTheThao 
        SET TinhTrang = N'Bảo trì', GhiChu = N'Bảo trì đột xuất (Demo Conflict)'
        WHERE MaSan = @MaSan;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@Msg, 16, 1);
    END CATCH
END;
GO

-- =============================================================
-- TỪ FILE: 3-Proc.sql
-- (Tính giá sân với Repeatable Read)
-- =============================================================

CREATE OR ALTER PROCEDURE Usp_TinhGiaSan
    @MaSan VARCHAR(10),
    @KhungGio NVARCHAR(50),
    @GiaThue DECIMAL(18,0) OUTPUT -- Đã sửa thành decimal(18,0) cho khớp với tiền VNĐ
AS
BEGIN
    SET NOCOUNT ON;
    
    -- QUAN TRỌNG: Mức độ này giữ Shared Lock đến khi hết Transaction
    -- Người khác đọc được, nhưng không sửa được (Sửa sẽ bị treo chờ)
    SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
    
    BEGIN TRANSACTION;
    BEGIN TRY
        -- BƯỚC 1: Đọc giá (SQL tự đánh dấu Shared Lock dòng này)
        SELECT @GiaThue = DonGia 
        FROM GiaThueSan
        WHERE MaCoSo = (SELECT MaCoSo FROM SanTheThao WHERE MaSan = @MaSan)
        AND LoaiSan = (SELECT LoaiSan FROM SanTheThao WHERE MaSan = @MaSan)
        AND KhungGio = @KhungGio;

        IF @GiaThue IS NULL
        BEGIN
            -- Nếu không có giá thì rollback ngay
            RAISERROR(N'Không tìm thấy giá quy định.', 16, 1);
        END

        -- BƯỚC 2: Giả lập thời gian xem xét/xử lý (10 giây)
        -- Trong 10s này, Shared Lock vẫn được giữ.
        -- Nếu T2 chạy UPDATE vào lúc này -> T2 sẽ bị TREO (BLOCKED)
        WAITFOR DELAY '00:00:10';

        -- BƯỚC 3: Kết thúc (Lúc này mới nhả khóa cho T2 sửa)
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW; -- Ném lỗi ra C# xử lý
    END CATCH
END;
GO

-- =============================================================
-- TỪ FILE: 4-Proc.sql
-- (Hủy đặt sân với Lost Update protection)
-- =============================================================

CREATE OR ALTER PROCEDURE Usp_HuyDatSan
    @MaPhieuDat VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    -- Khai báo biến lưu thông tin phiếu
    DECLARE @MaSan VARCHAR(10);
    DECLARE @GioBatDau DATETIME;
    DECLARE @GioKetThuc DATETIME;
    DECLARE @DaHuy BIT;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- BƯỚC 1: Lấy thông tin và LOCK dòng dữ liệu để xử lý an toàn
        SELECT 
            @MaSan = MaSan,
            @GioBatDau = GioBatDau,
            @GioKetThuc = GioKetThuc,
            @DaHuy = DaHuy
        FROM PhieuDatSan WITH (UPDLOCK, ROWLOCK) -- Khóa dòng này lại, không cho ai sửa
        WHERE MaPhieuDat = @MaPhieuDat;

		WAITFOR DELAY '00:00:10';
        -- Kiểm tra tồn tại
        IF @MaSan IS NULL
        BEGIN
            RAISERROR(N'Không tìm thấy phiếu đặt sân.', 16, 1);
            ROLLBACK;
            RETURN;
        END

        -- Kiểm tra nếu đã hủy rồi thì không làm gì cả (Idempotent)
        IF @DaHuy = 1
        BEGIN
            COMMIT;
            RETURN;
        END

        -- BƯỚC 2: Cập nhật trạng thái Hủy (Logic mới)
        -- Chỉ bật cờ DaHuy = 1, giữ nguyên TrangThaiThanhToan để đối soát tiền nong sau này
        UPDATE PhieuDatSan 
        SET DaHuy = 1
        WHERE MaPhieuDat = @MaPhieuDat;

        -- BƯỚC 3: Giải phóng lịch đặt (Để người khác có thể đặt slot này)
        UPDATE ThoiGianDat 
        SET TinhTrangDat = N'Còn trống' 
        WHERE MaSan = @MaSan 
          AND GioBatDau = @GioBatDau;

        -- BƯỚC 4: Cập nhật trạng thái sân thực tế (Chỉ nếu đang diễn ra ngay lúc này)
        -- Nếu phiếu đặt cho tuần sau thì không cần đổi trạng thái sân hiện tại
        IF (GETDATE() >= @GioBatDau AND GETDATE() <= @GioKetThuc)
        BEGIN
            UPDATE SanTheThao 
            SET TinhTrang = N'Còn trống' 
            WHERE MaSan = @MaSan;
        END

        -- Commit giao dịch
        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            ROLLBACK;
        
        -- Ném lỗi ra cho C# bắt
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;

GO

-- =============================================================
-- TỪ FILE: 5_6_13_16.sql
-- (Bao gồm các Proc quản lý Giới hạn đặt, Kho, và Phantom Read)
-- =============================================================


CREATE OR ALTER PROCEDURE sp_ThueDungCu
    @MaDichVu VARCHAR(10),
    @SoLuongThue INT
AS
BEGIN
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRAN
            DECLARE @TonKhoHienTai INT;

            -- 1. Đọc tồn kho và KHÓA CẬP NHẬT dòng này (UPDLOCK)
            -- Các transaction khác sẽ phải chờ tại dòng này nếu muốn đọc để update
            SELECT @TonKhoHienTai = SoLuongTon
            FROM DichVu WITH (UPDLOCK) 
            WHERE MaDichVu = @MaDichVu;

            -- 2. Kiểm tra điều kiện
            IF @TonKhoHienTai IS NULL
            BEGIN
                ROLLBACK TRAN;
                RAISERROR(N'Dịch vụ không tồn tại.', 16, 1);
                RETURN;
            END

            IF @TonKhoHienTai < @SoLuongThue
            BEGIN
                ROLLBACK TRAN;
                RAISERROR(N'Số lượng tồn kho không đủ.', 16, 1);
                RETURN;
            END

            -- 3. Cập nhật tồn kho
            UPDATE DichVu
            SET SoLuongTon = @TonKhoHienTai - @SoLuongThue
            WHERE MaDichVu = @MaDichVu;

        COMMIT TRAN;
        PRINT N'Thuê dụng cụ thành công. Tồn kho còn lại: ' + CAST((@TonKhoHienTai - @SoLuongThue) AS NVARCHAR);
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_DatSan_KiemTraGioiHan
    @MaKhachHang VARCHAR(10),
    @MaSan VARCHAR(10),
    @GioBatDau DATETIME,
    @GioKetThuc DATETIME,
    @KenhDat NVARCHAR(20)
AS
BEGIN
    -- Thiết lập mức cô lập cao nhất
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

    BEGIN TRY
        BEGIN TRAN
            -- 1. Lấy MaCoSo từ sân để tìm giới hạn phù hợp
            DECLARE @MaCoSo VARCHAR(10);
            SELECT @MaCoSo = MaCoSo FROM SanTheThao WHERE MaSan = @MaSan;
            
            -- 2. Lấy giới hạn sân từ bảng THAMSO
            DECLARE @GioiHanSan INT;
            SELECT TOP 1 @GioiHanSan = CAST(GiaTri AS INT)
            FROM ThamSo
            WHERE MaThamSo = 'MAX_BOOK' 
              AND (MaCoSo = @MaCoSo OR MaCoSo IS NULL)
            ORDER BY MaCoSo DESC;
            
            IF @GioiHanSan IS NULL SET @GioiHanSan = 3;

            -- 3. Kiểm tra số lượng sân đã đặt
            DECLARE @SoLuongDaDat INT;
            DECLARE @NgayDat DATE = CAST(@GioBatDau AS DATE);

            SELECT @SoLuongDaDat = COUNT(*)
            FROM PhieuDatSan
            WHERE MaKhachHang = @MaKhachHang
              AND CAST(GioBatDau AS DATE) = @NgayDat
              AND TrangThaiThanhToan != N'Đã hủy';

            -- 4. Kiểm tra giới hạn
            IF (@SoLuongDaDat >= @GioiHanSan)
            BEGIN
                ROLLBACK TRAN;
                RAISERROR(N'Khách hàng đã đạt giới hạn đặt sân trong ngày.', 16, 1);
                RETURN;
            END

            -- 5. Kiểm tra trùng giờ
            IF EXISTS (SELECT 1 FROM PhieuDatSan 
                       WHERE MaSan = @MaSan 
                       AND TrangThaiThanhToan != N'Đã hủy'
                       AND ((@GioBatDau >= GioBatDau AND @GioBatDau < GioKetThuc)
                         OR (@GioKetThuc > GioBatDau AND @GioKetThuc <= GioKetThuc)))
            BEGIN
                 ROLLBACK TRAN;
                 RAISERROR(N'Sân đã bị đặt trong khung giờ này.', 16, 1);
                 RETURN;
            END

            -- 6. Thực hiện đặt sân
            DECLARE @NewID VARCHAR(10) = LEFT(NEWID(), 8); 
            
            INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat, DaHuy)
            VALUES (@NewID, @MaKhachHang, @MaSan, @GioBatDau, @GioKetThuc, N'Chưa thanh toán', @KenhDat, 0);

        COMMIT TRAN;
        PRINT N'Đặt sân thành công.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO


CREATE OR ALTER PROCEDURE sp_NhapKho
    @MaDichVu VARCHAR(10),
    @SoLuongNhap INT
AS
BEGIN
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRAN
            DECLARE @TonKhoHienTai INT;

            -- Sử dụng UPDLOCK để khóa dòng dữ liệu khi đọc
            SELECT @TonKhoHienTai = SoLuongTon
            FROM DichVu WITH (UPDLOCK)
            WHERE MaDichVu = @MaDichVu;

            IF @TonKhoHienTai IS NULL
            BEGIN
                ROLLBACK TRAN;
                RAISERROR(N'Mã dịch vụ không hợp lệ.', 16, 1);
                RETURN;
            END

            -- Cập nhật cộng dồn
            UPDATE DichVu
            SET SoLuongTon = @TonKhoHienTai + @SoLuongNhap
            WHERE MaDichVu = @MaDichVu;

        COMMIT TRAN;
        PRINT N'Nhập kho thành công.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_TimKiemSanTrong
    @GioBatDau DATETIME,
    @GioKetThuc DATETIME,
    @LoaiSan NVARCHAR(50)
AS
BEGIN
    -- Mức cô lập SERIALIZABLE để chặn Phantom Insert (người khác chèn phiếu đặt vào giữa lúc đang tìm)
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

    BEGIN TRY
        BEGIN TRAN
            -- Giả lập độ trễ nếu cần test concurrency
            -- WAITFOR DELAY '00:00:05';

            SELECT s.MaSan, s.LoaiSan, s.SucChua, s.TinhTrang
            FROM SanTheThao s
            WHERE s.LoaiSan = @LoaiSan
              AND s.TinhTrang = N'Còn trống'
              AND NOT EXISTS (
                  SELECT 1 
                  FROM PhieuDatSan p
                  WHERE p.MaSan = s.MaSan
                  AND p.TrangThaiThanhToan != N'Đã hủy'
                  AND (
                       (@GioBatDau >= p.GioBatDau AND @GioBatDau < p.GioKetThuc)
                    OR (@GioKetThuc > p.GioBatDau AND @GioKetThuc <= p.GioKetThuc)
                    OR (p.GioBatDau >= @GioBatDau AND p.GioKetThuc <= @GioKetThuc)
                  )
              );
              
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_DatSan_GayXungDot
    @MaKhachHang VARCHAR(10),
    @MaSan VARCHAR(10),
    @GioBatDau DATETIME,
    @GioKetThuc DATETIME
AS
BEGIN
    -- Mức cô lập mặc định: Cho phép Phantom Read (Không khóa phạm vi đếm)
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRAN

            -- 1. Kiểm tra số lượng sân đã đặt trong ngày
            DECLARE @SoLuongDaDat INT;
            DECLARE @NgayDat DATE = CAST(@GioBatDau AS DATE);

            SELECT @SoLuongDaDat = COUNT(*)
            FROM PhieuDatSan
            WHERE MaKhachHang = @MaKhachHang
              AND CAST(GioBatDau AS DATE) = @NgayDat;

            -- 2. Kiểm tra giới hạn (Max 2)
            IF (@SoLuongDaDat >= 2)
            BEGIN
                ROLLBACK TRAN;
                -- Dùng Select để trả về message cho C# dễ đọc
                SELECT N'Thất bại: Đã quá giới hạn 2 sân.' AS KetQua;
                RETURN;
            END

            -- Giả lập độ trễ 5 giây SAU KHI ĐẾM nhưng TRƯỚC KHI INSERT
            -- để 2 giao dịch cùng nhìn thấy cùng một @SoLuongDaDat
            WAITFOR DELAY '00:00:05';

            -- 3. Thực hiện đặt sân (Nếu thỏa điều kiện < 2)
            DECLARE @NewID VARCHAR(10) = LEFT(NEWID(), 8); -- Random ID để không trùng khóa chính
            
            INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
            VALUES (@NewID, @MaKhachHang, @MaSan, @GioBatDau, @GioKetThuc, N'Chưa thanh toán', 'Online');

        COMMIT TRAN;
        SELECT N'Thành công: Đã đặt thêm 1 sân.' AS KetQua;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        SELECT ERROR_MESSAGE() AS KetQua;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_ThueDungCu_GayXungDot
    @MaDichVu VARCHAR(10),
    @SoLuongThue INT
AS
BEGIN
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED; -- Mức mặc định (dễ bị Lost Update)

    BEGIN TRY
        BEGIN TRAN
            DECLARE @TonKhoHienTai INT;

            -- Đọc tồn kho (Chỉ giữ Shared Lock, nhả ngay sau khi đọc)
            -- KHÔNG CÓ UPDLOCK => Cho phép người khác đọc cùng lúc
            SELECT @TonKhoHienTai = SoLuongTon
            FROM DichVu
            WHERE MaDichVu = @MaDichVu;

            -- Giả lập độ trễ để người khác kịp chen vào đọc giá trị cũ
            WAITFOR DELAY '00:00:05';

            IF @TonKhoHienTai IS NULL
            BEGIN
                ROLLBACK TRAN;
                SELECT N'Dịch vụ không tồn tại.' AS KetQua;
                RETURN;
            END

            IF @TonKhoHienTai < @SoLuongThue
            BEGIN
                ROLLBACK TRAN;
                SELECT N'Số lượng tồn kho không đủ.' AS KetQua;
                RETURN;
            END

            -- Cập nhật tồn kho (Ghi đè giá trị của người khác nếu họ đã update trong lúc mình wait)
            UPDATE DichVu
            SET SoLuongTon = @TonKhoHienTai - @SoLuongThue
            WHERE MaDichVu = @MaDichVu;

        COMMIT TRAN;
        SELECT N'Thuê dụng cụ thành công (Có thể bị Lost Update). Tồn kho ghi nhận: ' + CAST((@TonKhoHienTai - @SoLuongThue) AS NVARCHAR) AS KetQua;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        SELECT ERROR_MESSAGE() AS KetQua;
    END CATCH
END;
GO

-- 2. SP Nhập Kho Gây Lỗi (Tương tự)
CREATE OR ALTER PROCEDURE sp_NhapKho_GayXungDot
    @MaDichVu VARCHAR(10),
    @SoLuongNhap INT
AS
BEGIN
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRAN
            DECLARE @TonKhoHienTai INT;

            SELECT @TonKhoHienTai = SoLuongTon
            FROM DichVu
            WHERE MaDichVu = @MaDichVu;

            WAITFOR DELAY '00:00:05'; -- Delay để tạo điều kiện tranh chấp

            IF @TonKhoHienTai IS NULL
            BEGIN
                ROLLBACK TRAN;
                SELECT N'Mã dịch vụ không hợp lệ.' AS KetQua;
                RETURN;
            END

            UPDATE DichVu
            SET SoLuongTon = @TonKhoHienTai + @SoLuongNhap
            WHERE MaDichVu = @MaDichVu;

        COMMIT TRAN;
        SELECT N'Nhập kho thành công (Có thể bị Lost Update).' AS KetQua;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        SELECT ERROR_MESSAGE() AS KetQua;
    END CATCH
END;
GO


-- =============================================================
-- TỪ FILE: 7-Proc.sql
-- (Scenario 7: Phantom Read - Báo cáo doanh thu)
-- =============================================================

--BUG
CREATE OR ALTER PROCEDURE dbo.sp_BaoCaoDoanhThu_BUG
    @Ngay DATE
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    DECLARE @Start DATETIME = CAST(@Ngay AS DATETIME);
    DECLARE @End   DATETIME = DATEADD(day, 1, @Start);

    -- Result set 1: Danh sách hóa đơn
    SELECT MaHoaDon, MaPhieuDat, MaNhanVien, ThoiGianLap, TongTien
    FROM dbo.HoaDon
    WHERE ThoiGianLap >= @Start
      AND ThoiGianLap <  @End
    ORDER BY ThoiGianLap;

    WAITFOR DELAY '00:00:08';

    -- Result set 2: Tổng doanh thu (KHÔNG CAST cột)
    SELECT COALESCE(SUM(TongTien), 0) AS TongDoanhThu
    FROM dbo.HoaDon
    WHERE ThoiGianLap >= @Start
      AND ThoiGianLap <  @End;
END
GO

--FIX
CREATE OR ALTER PROCEDURE dbo.sp_BaoCaoDoanhThu_FIX
    @Ngay DATE
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

    BEGIN TRAN;

    SELECT MaHoaDon, MaPhieuDat, MaNhanVien, ThoiGianLap, TongTien
    FROM dbo.HoaDon
    WHERE CAST(ThoiGianLap AS DATE) = @Ngay
    ORDER BY ThoiGianLap;

    WAITFOR DELAY '00:00:08';

    SELECT ISNULL(SUM(TongTien), 0) AS TongDoanhThu
    FROM dbo.HoaDon
    WHERE CAST(ThoiGianLap AS DATE) = @Ngay;

    COMMIT;
END
GO

-- =============================================================
-- TỪ FILE: 8-Proc.sql
-- (Scenario 8: Quản lý ca trực & Xin nghỉ)
-- =============================================================

CREATE OR ALTER PROCEDURE dbo.sp_ERR08_Manager_AssignShift_BUG
    @MaNhanVien VARCHAR(10),
    @MaQuanLy   VARCHAR(10),
    @BatDau     DATETIME,
    @KetThuc    DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRAN;

    -- BUG: chỉ check đơn nghỉ "đã duyệt" (và check theo ngày)
    IF EXISTS (
        SELECT 1
        FROM dbo.DonNghiPhep
        WHERE MaNhanVien = @MaNhanVien
          AND TrangThaiDuyet = N'Đã duyệt'
          AND CAST(NgayGui AS DATE) = CAST(@BatDau AS DATE)
    )
    BEGIN
        ROLLBACK;
        SELECT N'Không phân ca: đã có nghỉ phép (đã duyệt)' AS Result;
        RETURN;
    END

    -- tạo cửa sổ cho giao dịch xin nghỉ chen vào
    WAITFOR DELAY '00:00:08';

    -- BUG: không khóa range/slot, dễ xảy ra mâu thuẫn logic
    INSERT INTO dbo.PhanCongCaTruc(MaNhanVien, MaQuanLy, ThoiGianBatDau, ThoiGianKetThuc)
    VALUES (@MaNhanVien, @MaQuanLy, @BatDau, @KetThuc);

    COMMIT;
    SELECT N'Phân ca OK (BUG)' AS Result;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_ERR08_Employee_RequestLeave_BUG
    @MaNhanVien     VARCHAR(10),
    @MaNguoiDuyet   VARCHAR(10),
    @MaNguoiThayThe VARCHAR(10),
    @NgayNghi       DATE,
    @LyDo           NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRAN;

    -- BUG: check ca trực nhưng cố ý vẫn cho insert để tạo mâu thuẫn
    IF EXISTS (
        SELECT 1
        FROM dbo.PhanCongCaTruc
        WHERE MaNhanVien = @MaNhanVien
          AND CAST(ThoiGianBatDau AS DATE) = @NgayNghi
    )
    BEGIN
        -- không rollback để tạo "vừa trực vừa nghỉ"
        WAITFOR DELAY '00:00:02';
    END

    INSERT INTO dbo.DonNghiPhep
        (MaNhanVien, LyDo, TrangThaiDuyet, MaNguoiDuyet, MaNguoiThayThe, NgayGui)
    VALUES
        (@MaNhanVien, @LyDo, N'Chờ duyệt', @MaNguoiDuyet, @MaNguoiThayThe, GETDATE());

    COMMIT;
    SELECT N'Gửi đơn nghỉ OK (BUG)' AS Result;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_FIX08_Manager_AssignShift
    @MaNhanVien VARCHAR(10),
    @MaQuanLy   VARCHAR(10),
    @BatDau     DATETIME,
    @KetThuc    DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Ngay DATE = CAST(@BatDau AS DATE);
    DECLARE @LockResource NVARCHAR(200) =
        N'SHIFTLEAVE_' + @MaNhanVien + N'_' + CONVERT(NVARCHAR(10), @Ngay, 120);

    BEGIN TRY
        SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
        BEGIN TRAN;

        DECLARE @LockResult INT;
        EXEC @LockResult = sp_getapplock
            @Resource = @LockResource,
            @LockMode = 'Exclusive',
            @LockOwner = 'Transaction',
            @LockTimeout = 10000;

        IF @LockResult < 0
        BEGIN
            ROLLBACK;
            SELECT N'Không lấy được applock, vui lòng thử lại' AS Result;
            RETURN;
        END

        -- FIX: khóa kiểm tra để tránh race
        IF EXISTS (
            SELECT 1
            FROM dbo.DonNghiPhep WITH (UPDLOCK, HOLDLOCK)
            WHERE MaNhanVien = @MaNhanVien
              AND TrangThaiDuyet IN (N'Chờ duyệt', N'Đã duyệt')
              AND CAST(NgayGui AS DATE) = @Ngay
        )
        BEGIN
            ROLLBACK;
            SELECT N'Không phân ca: đã có đơn nghỉ trong ngày' AS Result;
            RETURN;
        END

        IF EXISTS (
            SELECT 1
            FROM dbo.PhanCongCaTruc WITH (UPDLOCK, HOLDLOCK)
            WHERE MaNhanVien = @MaNhanVien
              AND (@BatDau < ThoiGianKetThuc AND @KetThuc > ThoiGianBatDau)
        )
        BEGIN
            ROLLBACK;
            SELECT N'Không phân ca: trùng ca' AS Result;
            RETURN;
        END

        INSERT INTO dbo.PhanCongCaTruc(MaNhanVien, MaQuanLy, ThoiGianBatDau, ThoiGianKetThuc)
        VALUES (@MaNhanVien, @MaQuanLy, @BatDau, @KetThuc);

        COMMIT;
        SELECT N'Phân ca OK (FIX)' AS Result;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SELECT ERROR_MESSAGE() AS Result;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_FIX08_Employee_RequestLeave
    @MaNhanVien     VARCHAR(10),
    @MaNguoiDuyet   VARCHAR(10),
    @MaNguoiThayThe VARCHAR(10),
    @NgayNghi       DATE,
    @LyDo           NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LockResource NVARCHAR(200) =
        N'SHIFTLEAVE_' + @MaNhanVien + N'_' + CONVERT(NVARCHAR(10), @NgayNghi, 120);

    BEGIN TRY
        SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
        BEGIN TRAN;

        DECLARE @LockResult INT;
        EXEC @LockResult = sp_getapplock
            @Resource = @LockResource,
            @LockMode = 'Exclusive',
            @LockOwner = 'Transaction',
            @LockTimeout = 10000;

        IF @LockResult < 0
        BEGIN
            ROLLBACK;
            SELECT N'Không lấy được applock, vui lòng thử lại' AS Result;
            RETURN;
        END

        -- FIX: nếu đã có ca trực trong ngày => từ chối
        IF EXISTS (
            SELECT 1
            FROM dbo.PhanCongCaTruc WITH (UPDLOCK, HOLDLOCK)
            WHERE MaNhanVien = @MaNhanVien
              AND CAST(ThoiGianBatDau AS DATE) = @NgayNghi
        )
        BEGIN
            ROLLBACK;
            SELECT N'Không thể xin nghỉ: đã có ca trực trong ngày' AS Result;
            RETURN;
        END

        INSERT INTO dbo.DonNghiPhep
            (MaNhanVien, LyDo, TrangThaiDuyet, MaNguoiDuyet, MaNguoiThayThe, NgayGui)
        VALUES
            (@MaNhanVien, @LyDo, N'Chờ duyệt', @MaNguoiDuyet, @MaNguoiThayThe, GETDATE());

        COMMIT;
        SELECT N'Gửi đơn nghỉ OK (FIX)' AS Result;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SELECT ERROR_MESSAGE() AS Result;
    END CATCH
END
GO

-- =============================================================
-- TỪ FILE: 9-Proc.sql
-- (Scenario 9: Non-Repeatable Read - Chính sách giá vs Thanh toán)
-- =============================================================

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

-- 3. Procedure Fix (SERIALIZABLE)
CREATE OR ALTER PROCEDURE sp_ThanhToanDonHang_DaFix
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

    -- SU DUNG SERIALIZABLE - KHOA DU LIEU
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

    BEGIN TRY
        BEGIN TRAN;
        SELECT @HangTV = CapBacThanhVien FROM KhachHang WHERE MaKhachHang = @MaKH;

        -- LAN DOC 1 - Se khoa luon dong nay
        SELECT @GiamGia1 = GiamGia FROM ChinhSachGiamGia WHERE HangTV = @HangTV;
        
        WAITFOR DELAY '00:00:10';

        -- LAN DOC 2
        SELECT @GiamGia2 = GiamGia FROM ChinhSachGiamGia WHERE HangTV = @HangTV;

        SET @TongTienSauGiam = @TongTienGoc * (100 - @GiamGia2) / 100;

        SET @KetQua = N'Thanh toan FIX OK. Giam: ' + CAST(@GiamGia2 AS NVARCHAR) + N'%';

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @KetQua = N'Loi (Deadlock/Conflict da duoc xu ly): ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- =============================================================
-- TỪ FILE: 10-Proc.sql
-- (Scenario 10: Dirty Read - Bảo trì sân vs Xem thông tin)
-- =============================================================

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

        SET @KetQua = N'Da doc thong tin san (Dirty Read)';
        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @KetQua = N'Loi: ' + ERROR_MESSAGE();
    END CATCH
    
    -- Reset lai
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
END;
GO

-- 3. Procedure Fix (READ COMMITTED)
CREATE OR ALTER PROCEDURE sp_XemThongTinSan_DaFix
    @MaCoSo VARCHAR(10),
    @KetQua NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- SU DUNG READ COMMITTED - SE CHO T1 COMMIT/ROLLBACK XONG MOI DOC
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRAN;
        
        -- Se bi block cho den khi T1 xong
        SELECT MaSan, LoaiSan, TinhTrang, GhiChu
        FROM SanTheThao
        WHERE MaCoSo = @MaCoSo;

        SET @KetQua = N'Da doc thong tin san (Clean Read)';
        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @KetQua = N'Loi: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- =============================================================
-- TỪ FILE: 11-Proc.sql
-- (Scenario 11: Lost Update - Auto Cancel vs Check-in)
-- =============================================================

-- 1. Thủ tục Hủy Tự Động (T1 - System)
CREATE OR ALTER PROCEDURE sp_Demo_System_AutoCancel
    @MaPhieu VARCHAR(20)
AS
BEGIN
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
    BEGIN TRAN;
    BEGIN TRY
        -- Đọc và GIỮ KHÓA (UPDLOCK)
        DECLARE @DaHuy BIT;
        DECLARE @TrangThai NVARCHAR(50);

        SELECT @DaHuy = DaHuy, @TrangThai = TrangThaiThanhToan 
        FROM PhieuDatSan WITH (UPDLOCK) 
        WHERE MaPhieuDat = @MaPhieu;

        -- --- QUAN TRỌNG: DELAY TRƯỚC KHI KIỂM TRA ---
        -- Để đảm bảo bạn kịp bấm nút bên Lễ tân
        WAITFOR DELAY '00:00:10'; 
        -- --------------------------------------------

        -- Nếu đã thanh toán hoặc đã hủy rồi thì thôi
        IF @DaHuy = 1 OR @TrangThai = N'Đã thanh toán'
        BEGIN
            COMMIT TRAN; RETURN;
        END

        -- Cập nhật Hủy (DaHuy = 1)
        UPDATE PhieuDatSan SET DaHuy = 1 WHERE MaPhieuDat = @MaPhieu;
        
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN; THROW;
    END CATCH
END;
GO

-- 2. Thủ tục Check-in (T2 - Lễ tân)
CREATE OR ALTER PROCEDURE sp_Demo_Rec_CheckIn
    @MaPhieu VARCHAR(20)
AS
BEGIN
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
    BEGIN TRAN;
    BEGIN TRY
        -- Cố gắng đọc (Sẽ bị TREO ở đây nếu T1 đang chạy)
        DECLARE @DaHuy BIT;
        SELECT @DaHuy = DaHuy FROM PhieuDatSan WITH (UPDLOCK) WHERE MaPhieuDat = @MaPhieu;

        -- Sau khi T1 nhả khóa, kiểm tra lại
        IF @DaHuy = 1
        BEGIN
            ROLLBACK TRAN;
            THROW 50005, N'LỖI: Phiếu này vừa bị Hệ thống hủy tự động do quá hạn!', 1;
            RETURN;
        END

        -- Check-in thành công
        UPDATE PhieuDatSan SET TrangThaiThanhToan = N'Đã thanh toán' WHERE MaPhieuDat = @MaPhieu;
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN; THROW;
    END CATCH
END;
GO

-- =============================================================
-- TỪ FILE: 12-Proc.sql
-- (Scenario 12: Duyệt nghỉ phép vs Phân ca)
-- =============================================================

CREATE OR ALTER PROCEDURE Usp_DuyetNghiPhep
    @MaDon INT,
    @TrangThaiDuyet NVARCHAR(20),
    @UseLock BIT = 1  -- 1: An toàn, 0: Gây lỗi
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- =============================================================
        -- PHÂN TÁCH LOGIC KHÓA RÕ RÀNG
        -- =============================================================
        
        IF @UseLock = 1 
        BEGIN
            -- TRƯỜNG HỢP AN TOÀN (FIX BUG)
            -- Dùng Repeatable Read + UPDLOCK để giữ chỗ, người khác phải chờ
            SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
            
            SELECT * FROM PhanCongCaTruc WITH (UPDLOCK) 
            WHERE MaNhanVien = (SELECT MaNhanVien FROM DonNghiPhep WHERE MaDon = @MaDon);
        END
        ELSE
        BEGIN
            -- TRƯỜNG HỢP GÂY LỖI (DEMO)
            -- Dùng Read Committed: Đọc xong nhả khóa ngay -> Người khác chen vào Update được
            SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
            
            SELECT * FROM PhanCongCaTruc 
            WHERE MaNhanVien = (SELECT MaNhanVien FROM DonNghiPhep WHERE MaDon = @MaDon);
        END

        -- =============================================================
        -- GIẢ LẬP ĐỘ TRỄ (Để T2 kịp chen vào sửa trong lúc T1 đang chạy)
        -- =============================================================
        WAITFOR DELAY '00:00:10';

        -- Cập nhật trạng thái
        UPDATE DonNghiPhep SET TrangThaiDuyet = @TrangThaiDuyet WHERE MaDon = @MaDon;

        -- Logic nghiệp vụ phụ (cập nhật lịch trực)
        IF @TrangThaiDuyet = N'Đã duyệt'
        BEGIN
            UPDATE PhanCongCaTruc SET MaNhanVien = d.MaNguoiThayThe 
            FROM PhanCongCaTruc p INNER JOIN DonNghiPhep d ON p.MaNhanVien = d.MaNhanVien
            WHERE d.MaDon = @MaDon;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @Msg NVARCHAR(MAX) = ERROR_MESSAGE();
        RAISERROR(@Msg, 16, 1);
    END CATCH
END;
GO
-- =============================================================
-- TỪ FILE: 14-Proc.sql
-- (Scenario 14: Double Booking VIP Room)
-- =============================================================

CREATE OR ALTER PROCEDURE dbo.sp_ERR14_BookVIP_ByInvoice_BUG
    @MaHoaDon   VARCHAR(10),
    @MaCoSo     VARCHAR(10),
    @GioBatDau  DATETIME,
    @GioKetThuc DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    DECLARE @MaTaiSan VARCHAR(10);

    BEGIN TRY
        BEGIN TRAN;

        -- Chọn 1 phòng VIP còn trống (KHÔNG LOCK -> BUG)
        SELECT TOP (1) @MaTaiSan = MaTaiSan
        FROM dbo.TaiSan
        WHERE MaCoSo = @MaCoSo
          AND LoaiTaiSan = N'Phòng tắm VIP'
          AND TinhTrang = N'Còn trống'
        ORDER BY MaTaiSan;

        IF @MaTaiSan IS NULL
        BEGIN
            ROLLBACK;
            SELECT N'Hết phòng VIP' AS Result;
            RETURN;
        END

        -- Tạo cửa sổ để session khác chen vào (double booking)
        WAITFOR DELAY '00:00:08';

        -- Ghi dịch vụ VIP vào chi tiết hóa đơn (FK HoaDon phải hợp lệ)
        INSERT INTO dbo.ChiTietSuDungDichVu
            (MaHoaDon, MaDichVu, MaCoSo, SoLuong, MaTaiSan, GioBatDau, GioKetThuc)
        VALUES
            (@MaHoaDon, 'DV_VIP', @MaCoSo, 1, @MaTaiSan, @GioBatDau, @GioKetThuc);

        -- BUG: update không điều kiện + không khóa
        UPDATE dbo.TaiSan
        SET TinhTrang = N'Đang sử dụng'
        WHERE MaTaiSan = @MaTaiSan;

        COMMIT;

        SELECT N'OK (BUG) - Đặt VIP thành công' AS Result, @MaTaiSan AS MaTaiSanDaDat;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SELECT ERROR_MESSAGE() AS Result;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_FIX14_BookVIP_ByInvoice
    @MaHoaDon   VARCHAR(10),
    @MaCoSo     VARCHAR(10),
    @GioBatDau  DATETIME,
    @GioKetThuc DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MaTaiSan VARCHAR(10);

    BEGIN TRY
        SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
        BEGIN TRAN;

        -- Khóa phòng ngay lúc chọn (FIX)
        SELECT TOP (1) @MaTaiSan = MaTaiSan
        FROM dbo.TaiSan WITH (UPDLOCK, HOLDLOCK)
        WHERE MaCoSo = @MaCoSo
          AND LoaiTaiSan = N'Phòng tắm VIP'
          AND TinhTrang = N'Còn trống'
        ORDER BY MaTaiSan;

        IF @MaTaiSan IS NULL
        BEGIN
            ROLLBACK;
            SELECT N'Hết phòng VIP' AS Result;
            RETURN;
        END

        -- Ghi dịch vụ VIP vào chi tiết hóa đơn
        INSERT INTO dbo.ChiTietSuDungDichVu
            (MaHoaDon, MaDichVu, MaCoSo, SoLuong, MaTaiSan, GioBatDau, GioKetThuc)
        VALUES
            (@MaHoaDon, 'DV_VIP', @MaCoSo, 1, @MaTaiSan, @GioBatDau, @GioKetThuc);

        -- Update có điều kiện (chỉ 1 session đổi được)
        UPDATE dbo.TaiSan
        SET TinhTrang = N'Đang sử dụng'
        WHERE MaTaiSan = @MaTaiSan
          AND TinhTrang = N'Còn trống';

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK;
            SELECT N'Không đặt được: phòng đã có người giữ/đặt' AS Result;
            RETURN;
        END

        COMMIT;

        SELECT N'OK (FIX) - Đặt VIP thành công' AS Result, @MaTaiSan AS MaTaiSanDaDat;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;

        -- Nếu bạn có unique index theo slot, có thể bắt 2601/2627 ở đây
        IF ERROR_NUMBER() IN (2601, 2627)
            SELECT N'Không đặt được: slot đã có người đặt' AS Result;
        ELSE
            SELECT ERROR_MESSAGE() AS Result;
    END CATCH
END
GO

-- =============================================================
-- TỪ FILE: 15-Proc.sql
-- (Scenario 15: Phantom Read - Đổi giờ sân)
-- =============================================================

CREATE OR ALTER PROCEDURE sp_DoiGioSan
    @MaPhieuDat_Cu VARCHAR(10),
    @MaSan_Moi VARCHAR(10),
    @GioBatDau_Moi DATETIME,
    @GioKetThuc_Moi DATETIME,
    @KetQua NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRAN;
        
        -- Hủy phiếu cũ (Update DaHuy = 1)
        UPDATE PhieuDatSan
        SET DaHuy = 1
        WHERE MaPhieuDat = @MaPhieuDat_Cu;

        -- Delay 8s: Giả lập thời gian khách suy nghĩ hoặc mạng lag
        WAITFOR DELAY '00:00:08';

        -- Tạo phiếu mới với giờ mới
        -- Lưu ý: ID mới tự sinh
        DECLARE @NewID VARCHAR(10) = LEFT(NEWID(), 8);
        DECLARE @MaKH VARCHAR(10);
        SELECT @MaKH = MaKhachHang FROM PhieuDatSan WHERE MaPhieuDat = @MaPhieuDat_Cu;

        INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
        VALUES (@NewID, @MaKH, @MaSan_Moi, @GioBatDau_Moi, @GioKetThuc_Moi, N'Chưa thanh toán', 'Online');

        SET @KetQua = N'Đã đổi giờ thành công sang ' + CONVERT(NVARCHAR, @GioBatDau_Moi, 120);
        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @KetQua = N'Lỗi đổi giờ: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- BUG: Khách khác tìm sân trong lúc người kia đang đổi giờ
CREATE OR ALTER PROCEDURE sp_TimVaDatSanTrong_CoLoi
    @MaSan VARCHAR(10),
    @GioBatDau DATETIME,
    @GioKetThuc DATETIME,
    @MaKhachHang VARCHAR(10),
    @KetQua NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    -- READ COMMITTED -> Gặp Phantom Read
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRAN;
        
        -- B1: Kiểm tra trống (Không thấy phiếu của người đang đổi giờ vì nó chưa commit)
        DECLARE @Count INT;
        SELECT @Count = COUNT(*)
        FROM PhieuDatSan
        WHERE MaSan = @MaSan
          AND DaHuy = 0
          AND ((@GioBatDau >= GioBatDau AND @GioBatDau < GioKetThuc) OR 
               (@GioKetThuc > GioBatDau AND @GioKetThuc <= GioKetThuc));

        IF @Count = 0
        BEGIN
            -- Thấy trống -> Đặt luôn
            INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
            VALUES (LEFT(NEWID(), 8), @MaKhachHang, @MaSan, @GioBatDau, @GioKetThuc, N'Chưa thanh toán', 'Online');
            
            SET @KetQua = N'Đặt thành công (Lỗi Phantom: Có thể trùng với người đổi giờ)';
        END
        ELSE
        BEGIN
            SET @KetQua = N'Sân bận!';
        END

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @KetQua = N'Lỗi: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- FIX: Dùng SERIALIZABLE
CREATE OR ALTER PROCEDURE sp_TimVaDatSanTrong_DaFix
    @MaSan VARCHAR(10),
    @GioBatDau DATETIME,
    @GioKetThuc DATETIME,
    @MaKhachHang VARCHAR(10),
    @KetQua NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    -- SERIALIZABLE -> Khóa phạm vi, chặn Insert mới
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

    BEGIN TRY
        BEGIN TRAN;
        
        DECLARE @Count INT;
        SELECT @Count = COUNT(*)
        FROM PhieuDatSan
        WHERE MaSan = @MaSan
          AND DaHuy = 0
          AND ((@GioBatDau >= GioBatDau AND @GioBatDau < GioKetThuc) OR 
               (@GioKetThuc > GioBatDau AND @GioKetThuc <= GioKetThuc));

        IF @Count = 0
        BEGIN
            INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
            VALUES (LEFT(NEWID(), 8), @MaKhachHang, @MaSan, @GioBatDau, @GioKetThuc, N'Chưa thanh toán', 'Online');
            
            SET @KetQua = N'Đặt thành công (An toàn)';
        END
        ELSE
        BEGIN
            SET @KetQua = N'Sân bận (Hoặc đang có giao dịch khác xử lý)';
        END

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @KetQua = N'Lỗi: ' + ERROR_MESSAGE();
    END CATCH
END;
GO
