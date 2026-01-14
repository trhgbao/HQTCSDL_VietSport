USE VietSportDB;
GO

-- =============================================
-- PHẦN 1: DỌN DẸP DỮ LIỆU CŨ (CLEAN UP)
-- =============================================
-- Tắt check khóa ngoại để xóa nhanh
EXEC sp_msforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";

DELETE FROM ChiTietSuDungDichVu;
DELETE FROM HoaDon;
DELETE FROM PhieuDatSan;
DELETE FROM DonNghiPhep;
DELETE FROM PhanCongCaTruc;
DELETE FROM LuongNhanVien;
DELETE FROM TaiKhoan;
DELETE FROM KhachHang;
DELETE FROM NhanVien;
DELETE FROM GiaThueSan;
DELETE FROM ThoiGianDat;
DELETE FROM DichVu;
DELETE FROM TaiSan;
DELETE FROM ThamSo;
DELETE FROM SanTheThao;
DELETE FROM CoSo;
DELETE FROM ChinhSachGiamGia;

-- Bật lại check khóa ngoại
EXEC sp_msforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";
GO

-- =============================================
-- PHẦN 2: DỮ LIỆU CẤU HÌNH HỆ THỐNG
-- =============================================

-- 1. Cơ sở (Chi nhánh)
INSERT INTO CoSo (MaCoSo, TenCoSo, DiaChi, ThanhPho) VALUES 
('CS_HCM01', N'VietSport Quận 1', N'123 Nguyễn Huệ, Q1', N'Hồ Chí Minh'),
('CS_HCM02', N'VietSport Thủ Đức', N'45 Võ Văn Ngân, TĐ', N'Hồ Chí Minh');

-- 2. Tham số quy định
INSERT INTO ThamSo (MaThamSo, TenThamSo, GiaTri, MaCoSo) VALUES 
('MIN_TIME', N'Thời gian đặt tối thiểu (phút)', 60, NULL),
('MAX_TIME', N'Thời gian đặt tối đa (phút)', 180, NULL),
('MAX_BOOK', N'Hạn mức số lượng sân/ngày', 3, NULL);

-- 3. Chính sách giảm giá (Cho Scenario 9 - Non-Repeatable Read)
INSERT INTO ChinhSachGiamGia (HangTV, GiamGia) VALUES
('Standard', 0), 
('Silver', 5), 
('Gold', 10), 
('Platinum', 20); -- Khách VIP sẽ được giảm 20%

-- 4. Dịch vụ (Kho & Tài sản)
INSERT INTO DichVu (MaDichVu, TenDichVu, DonGia, DonViTinh, SoLuongTon, LoaiXuLy) VALUES 
('DV01', N'Nước Suối Aquafina', 10000, N'Chai', 100, 'TonKho'),
('DV02', N'Khăn Lạnh', 5000, N'Cái', 200, 'TonKho'),
('DV03', N'Thuê Vợt Cầu Lông', 30000, N'Cái', 20, 'TaiSan'),
('DV_VIP', N'Phòng Tắm VIP', 100000, N'Suất', 1, 'TaiSan'); -- Chỉ có 1 phòng để test tranh chấp

-- 5. Tài sản (Phòng VIP cho Scenario 14)
INSERT INTO TaiSan (MaTaiSan, TenTaiSan, MaCoSo, LoaiTaiSan, TinhTrang) VALUES
('VIP01', N'Phòng Tắm VIP 01', 'CS_HCM01', N'Phòng tắm VIP', N'Còn trống');

-- =============================================
-- PHẦN 3: DỮ LIỆU SÂN BÃI & GIÁ
-- =============================================

-- 1. Sân thể thao
INSERT INTO SanTheThao (MaSan, MaCoSo, LoaiSan, SucChua, TinhTrang, GhiChu) VALUES
('SAN01', 'CS_HCM01', N'Bóng đá mini', 10, N'Còn trống', N'Sân cỏ nhân tạo mới'),
('SAN02', 'CS_HCM01', N'Bóng đá mini', 10, N'Còn trống', N'Gần cổng vào'),
('SAN03', 'CS_HCM01', N'Cầu lông', 4, N'Bảo trì', N'Lưới rách (Dự kiến xong: 30/12/2025)'),
('SAN04', 'CS_HCM02', N'Tennis', 4, N'Còn trống', N'Sân đất nện'),
('SAN_TEST', 'CS_HCM01', N'Bóng đá mini', 10, N'Còn trống', N'Sân dùng để test Code');

-- 2. Bảng giá (Quan trọng cho logic tính tiền)
-- Giá Quận 1 (Bóng đá)
INSERT INTO GiaThueSan (MaCoSo, LoaiSan, KhungGio, DonGia) VALUES
('CS_HCM01', N'Bóng đá mini', N'Ngày thường', 200000),
('CS_HCM01', N'Bóng đá mini', N'Giờ cao điểm', 300000), -- 17h-21h
('CS_HCM01', N'Bóng đá mini', N'Cuối tuần', 350000);

-- Giá Quận 1 (Cầu lông)
INSERT INTO GiaThueSan (MaCoSo, LoaiSan, KhungGio, DonGia) VALUES
('CS_HCM01', N'Cầu lông', N'Ngày thường', 80000),
('CS_HCM01', N'Cầu lông', N'Giờ cao điểm', 120000),
('CS_HCM01', N'Cầu lông', N'Cuối tuần', 150000);

-- Giá Thủ Đức (Tennis)
INSERT INTO GiaThueSan (MaCoSo, LoaiSan, KhungGio, DonGia) VALUES
('CS_HCM02', N'Tennis', N'Ngày thường', 400000),
('CS_HCM02', N'Tennis', N'Giờ cao điểm', 600000),
('CS_HCM02', N'Tennis', N'Cuối tuần', 700000);

-- =============================================
-- PHẦN 4: NHÂN SỰ & TÀI KHOẢN (Mật khẩu: 123)
-- =============================================

-- 1. Nhân viên
INSERT INTO NhanVien (MaNhanVien, HoTen, MaCoSo, ChucVu, CMND, SoDienThoai, GioiTinh, NgaySinh) VALUES 
('AD01', N'Super Admin', 'CS_HCM01', N'Quản trị', '999', '0999999999', N'Nam', '1990-01-01'),
('NV_QL', N'Trần Quản Lý', 'CS_HCM01', N'Quản lý', '001', '0901111111', N'Nam', '1985-05-05'),
('NV_LT', N'Lê Lễ Tân', 'CS_HCM01', N'Lễ tân', '002', '0902222222', N'Nữ', '1995-10-10'),
('NV_KT', N'Nguyễn Kỹ Thuật', 'CS_HCM01', N'Kỹ thuật', '003', '0903333333', N'Nam', '1992-12-12'),
('NV_TN', N'Phạm Thu Ngân', 'CS_HCM01', N'Thu ngân', '004', '0904444444', N'Nữ', '1998-08-08');

-- 2. Khách hàng
INSERT INTO KhachHang (MaKhachHang, HoTen, SoDienThoai, Email, CapBacThanhVien, CMND, GioiTinh, NgaySinh) VALUES 
('KH01', N'Nguyễn Văn A', '0911111111', 'a@gmail.com', 'Standard', '111', N'Nam', '2000-01-01'),
('KH_VIP', N'Đại Gia Platinum', '0988888888', 'vip@gmail.com', 'Platinum', '888', N'Nam', '1980-01-01'),
('KH_TEST', N'User Test Xung Đột', '0900000000', 'test@gmail.com', 'Standard', '000', N'Nữ', '2002-02-02');

-- 3. Tài khoản (Tất cả pass là 123)
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, MaNhanVien, MaKhachHang) VALUES 
('admin', '123', 'AD01', NULL),
('quanly', '123', 'NV_QL', NULL),
('letan', '123', 'NV_LT', NULL),
('kythuat', '123', 'NV_KT', NULL),
('thungan', '123', 'NV_TN', NULL),
('khachhang', '123', NULL, 'KH01'),
('khachvip', '123', NULL, 'KH_VIP');

-- =============================================
-- PHẦN 5: DỮ LIỆU GIAO DỊCH MẪU
-- =============================================

-- 1. Phiếu đặt sân (Dữ liệu nền)
-- Phiếu đã thanh toán (Để Lễ tân Check-in)
INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat, DaHuy) VALUES 
('P_PAID01', 'KH01', 'SAN01', DATEADD(HOUR, 2, GETDATE()), DATEADD(HOUR, 4, GETDATE()), N'Đã thanh toán', 'Online', 0);

-- Phiếu chưa thanh toán (Để Thu ngân tính tiền)
INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat, DaHuy) VALUES 
('P_UNPAID', 'KH01', 'SAN02', DATEADD(DAY, 1, GETDATE()), DATEADD(DAY, 1, DATEADD(HOUR, 2, GETDATE())), N'Chưa thanh toán', 'Online', 0);

-- Phiếu của khách VIP (Để test giảm giá)
INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat, DaHuy) VALUES 
('P_VIP', 'KH_VIP', 'SAN01', DATEADD(DAY, 2, GETDATE()), DATEADD(DAY, 2, DATEADD(HOUR, 2, GETDATE())), N'Chưa thanh toán', 'Online', 0);

-- Phiếu dùng cho Demo Race Condition (Scenario 11 - Quá hạn thanh toán)
-- Set giờ bắt đầu lùi về quá khứ 30 phút để đủ điều kiện hủy
INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat, DaHuy) VALUES 
('DEMO_RACE', 'KH_TEST', 'SAN_TEST', DATEADD(MINUTE, -30, GETDATE()), DATEADD(MINUTE, 90, GETDATE()), N'Chưa thanh toán', 'Online', 0);

-- 2. Hóa đơn mẫu (Cho báo cáo doanh thu)
INSERT INTO HoaDon (MaHoaDon, MaPhieuDat, MaNhanVien, ThoiGianLap, TongTien) VALUES
('HD01', 'P_PAID01', 'NV_TN', GETDATE(), 400000);

GO