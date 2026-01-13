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
            
            -- 2. Lấy giới hạn sân từ bảng THAMSO (ưu tiên theo cơ sở, nếu không có thì lấy global)
            DECLARE @GioiHanSan INT;
            SELECT TOP 1 @GioiHanSan = CAST(GiaTri AS INT)
            FROM ThamSo
            WHERE MaThamSo = 'MAX_BOOK' 
              AND (MaCoSo = @MaCoSo OR MaCoSo IS NULL)
            ORDER BY MaCoSo DESC; -- Ưu tiên MaCoSo cụ thể trước, sau đó mới đến NULL (global)
            
            -- Nếu không tìm thấy trong THAMSO, dùng giá trị mặc định là 3
            IF @GioiHanSan IS NULL
                SET @GioiHanSan = 3;

            -- 3. Kiểm tra số lượng sân đã đặt trong ngày
            DECLARE @SoLuongDaDat INT;
            DECLARE @NgayDat DATE = CAST(@GioBatDau AS DATE);

            SELECT @SoLuongDaDat = COUNT(*)
            FROM PhieuDatSan
            WHERE MaKhachHang = @MaKhachHang
              AND CAST(GioBatDau AS DATE) = @NgayDat
              AND TrangThaiThanhToan != N'Đã hủy';

            -- 4. Kiểm tra giới hạn (sử dụng giá trị từ bảng THAMSO)
            IF (@SoLuongDaDat >= @GioiHanSan)
            BEGIN
                -- Nếu vi phạm, rollback và báo lỗi
                ROLLBACK TRAN;
                RAISERROR(N'Khách hàng đã đạt giới hạn đặt sân trong ngày.', 16, 1);
                RETURN;
            END

            -- 5. Kiểm tra trùng giờ (Logic nghiệp vụ)
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
            -- Tạo mã phiếu tự động đơn giản để test
            DECLARE @NewID VARCHAR(10) = LEFT(NEWID(), 8); 
            
            INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
            VALUES (@NewID, @MaKhachHang, @MaSan, @GioBatDau, @GioKetThuc, N'Chưa thanh toán', @KenhDat);

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
            
            -- Giả lập độ trễ 5 giây:
            -- Để đảm bảo Giao dịch 2 kịp chạy vào và ĐỌC số lượng trước khi Giao dịch 1 kịp INSERT
            WAITFOR DELAY '00:00:05';

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