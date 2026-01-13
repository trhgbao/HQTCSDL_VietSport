-- 1. TẠO DATABASE
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'VietSportDB')
    CREATE DATABASE VietSportDB;
GO
USE VietSportDB;
GO

-- 2. BẢNG CƠ SỞ (Chi nhánh)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CoSo')
CREATE TABLE CoSo (
    MaCoSo VARCHAR(10) NOT NULL PRIMARY KEY,
    TenCoSo NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(255) NOT NULL,
    ThanhPho NVARCHAR(50) NOT NULL
);
GO

-- 3. BẢNG THAM SỐ HỆ THỐNG
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ThamSo')
CREATE TABLE ThamSo (
    MaThamSo VARCHAR(10) NOT NULL PRIMARY KEY,
    TenThamSo NVARCHAR(100) NOT NULL UNIQUE,
    GiaTri DECIMAL(10,2) NOT NULL CHECK (GiaTri > 0), -- Giả định giá trị là số
    MaCoSo VARCHAR(10) NULL,
    LoaiSan NVARCHAR(50) NULL CHECK (LoaiSan IN (N'Bóng đá mini', N'Cầu lông', N'Tennis', N'Bóng rổ', N'Futsal')),
    GhiChu NVARCHAR(255) NULL,
    CONSTRAINT FK_ThamSo_CoSo FOREIGN KEY (MaCoSo) REFERENCES CoSo(MaCoSo)
);
GO

-- 4. BẢNG SÂN THỂ THAO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SanTheThao')
CREATE TABLE SanTheThao (
    MaSan VARCHAR(10) NOT NULL PRIMARY KEY,
    MaCoSo VARCHAR(10) NOT NULL,
    LoaiSan NVARCHAR(50) NOT NULL CHECK (LoaiSan IN (N'Bóng đá mini', N'Cầu lông', N'Tennis', N'Bóng rổ', N'Futsal')),
    SucChua INT NOT NULL CHECK (SucChua > 0),
    TinhTrang NVARCHAR(50) NOT NULL DEFAULT N'Còn trống' CHECK (TinhTrang IN (N'Còn trống', N'Đã đặt', N'Đang sử dụng', N'Bảo trì')),
    GhiChu NVARCHAR(500),
	CONSTRAINT FK_San_CoSo FOREIGN KEY (MaCoSo) REFERENCES CoSo(MaCoSo)
);
GO

-- 5. BẢNG THỜI GIAN ĐẶT (QUẢN LÝ SLOT TRÊN SÂN)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ThoiGianDat')
CREATE TABLE ThoiGianDat (
    MaThoiGianDat INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaSan VARCHAR(10) NOT NULL,
    GioBatDau DATETIME NOT NULL,
    GioKetThuc DATETIME NOT NULL,
    TinhTrangDat NVARCHAR(50) NOT NULL DEFAULT N'Còn trống' CHECK (TinhTrangDat IN (N'Còn trống', N'Đã đặt', N'Đang sử dụng', N'Bảo trì')),
    LyDoThayDoi NVARCHAR(255) NULL,
    NgayCapNhat DATETIME DEFAULT GETDATE(),
    CONSTRAINT CK_ThoiGianDat_Gio CHECK (GioKetThuc > GioBatDau),
    CONSTRAINT FK_ThoiGian_San FOREIGN KEY (MaSan) REFERENCES SanTheThao(MaSan)
);
GO

-- 6. BẢNG GIÁ THUÊ SÂN
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'GiaThueSan')
CREATE TABLE GiaThueSan (
    MaGia INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaCoSo VARCHAR(10) NOT NULL,
    LoaiSan NVARCHAR(50) NOT NULL,
    KhungGio NVARCHAR(50) NOT NULL CHECK (KhungGio IN (N'Ngày thường', N'Cuối tuần', N'Giờ thấp điểm', N'Giờ cao điểm')),
    DonGia DECIMAL(10,2) NOT NULL CHECK (DonGia > 0),
    CONSTRAINT FK_Gia_CoSo FOREIGN KEY (MaCoSo) REFERENCES CoSo(MaCoSo)
);
GO

-- 7. BẢNG KHÁCH HÀNG
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'KhachHang')
CREATE TABLE KhachHang (
    MaKhachHang VARCHAR(10) NOT NULL PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATETIME NOT NULL,
    SoDienThoai VARCHAR(15) NOT NULL UNIQUE,
    Email VARCHAR(100) NULL UNIQUE,
    CapBacThanhVien NVARCHAR(20) DEFAULT 'Standard' CHECK (CapBacThanhVien IN ('Standard', 'Silver', 'Gold', 'Platinum')),
    LaHSSV BIT NOT NULL DEFAULT 0,
    LaNguoiCaoTuoi BIT NOT NULL DEFAULT 0,
    CMND VARCHAR(12) NOT NULL UNIQUE,
    GioiTinh NVARCHAR(10) CHECK (GioiTinh IN (N'Nam', N'Nữ', N'Khác')),
	DiaChi NVARCHAR(50)
);
GO

-- 8. BẢNG NHÂN VIÊN
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'NhanVien')
CREATE TABLE NhanVien (
    MaNhanVien VARCHAR(10) NOT NULL PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
	NgaySinh DATETIME NOT NULL,
    MaCoSo VARCHAR(10) NOT NULL,
    ChucVu NVARCHAR(50) NOT NULL CHECK (ChucVu IN (N'Quản lý', N'Lễ tân', N'Kỹ thuật', N'Thu ngân', N'Quản trị', N'Huấn luyện viên')),
    CMND VARCHAR(12) NOT NULL UNIQUE,
    SoDienThoai VARCHAR(15) NOT NULL UNIQUE,
    GioiTinh NVARCHAR(10) CHECK (GioiTinh IN (N'Nam', N'Nữ', N'Khác')),
    CONSTRAINT FK_NV_CoSo FOREIGN KEY (MaCoSo) REFERENCES CoSo(MaCoSo)
);
GO

-- 9. BẢNG LƯƠNG NHÂN VIÊN
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LuongNhanVien')
CREATE TABLE LuongNhanVien (
    MaNhanVien VARCHAR(10) NOT NULL,
    ThangNam DATETIME NOT NULL, -- Dùng ngày đầu tháng để chốt
    LuongCoBan DECIMAL(10,2) NOT NULL CHECK (LuongCoBan >= 0),
    PhuCap DECIMAL(10,2) NOT NULL CHECK (PhuCap >= 0),
    ThuLaoCaTruc DECIMAL(10,2) NOT NULL CHECK (ThuLaoCaTruc >= 0),
    HoaHongDoanhThu DECIMAL(10,2) NOT NULL CHECK (HoaHongDoanhThu >= 0),
    TienPhat DECIMAL(10,2) NOT NULL CHECK (TienPhat >= 0),
    TongLuong DECIMAL(10,2) NOT NULL,
    NguoiTinhLuong VARCHAR(10) NOT NULL,
    PRIMARY KEY (MaNhanVien, ThangNam),
    CONSTRAINT FK_Luong_NV FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien),
    CONSTRAINT FK_Luong_NguoiTinh FOREIGN KEY (NguoiTinhLuong) REFERENCES NhanVien(MaNhanVien)
);
GO

-- 10. BẢNG PHÂN CÔNG CA TRỰC
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PhanCongCaTruc')
CREATE TABLE PhanCongCaTruc (
    MaPhanCong INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaNhanVien VARCHAR(10) NOT NULL,
    MaNguoiThayThe VARCHAR(10) NULL,
    MaQuanLy VARCHAR(10) NOT NULL,
    ThoiGianBatDau DATETIME NOT NULL,
    ThoiGianKetThuc DATETIME NOT NULL,
    CONSTRAINT CK_CaTruc_Gio CHECK (ThoiGianKetThuc > ThoiGianBatDau),
    CONSTRAINT FK_CaTruc_NV FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien),
    CONSTRAINT FK_CaTruc_ThayThe FOREIGN KEY (MaNguoiThayThe) REFERENCES NhanVien(MaNhanVien),
    CONSTRAINT FK_CaTruc_QuanLy FOREIGN KEY (MaQuanLy) REFERENCES NhanVien(MaNhanVien)
);
GO

-- 11. BẢNG ĐƠN NGHỈ PHÉP
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DonNghiPhep')
CREATE TABLE DonNghiPhep (
    MaDon INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaNhanVien VARCHAR(10) NOT NULL,
    NgayGui DATETIME NOT NULL DEFAULT GETDATE(),
    LyDo NVARCHAR(500) NULL,
    TrangThaiDuyet NVARCHAR(20) NOT NULL DEFAULT N'Chờ duyệt' CHECK ( TrangThaiDuyet IN (N'Chờ duyệt', N'Đã duyệt', N'Từ chối')),
    MaNguoiDuyet VARCHAR(10) NOT NULL,
    MaNguoiThayThe VARCHAR(10) NOT NULL,
    CONSTRAINT FK_DonNghi_NV FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien),
    CONSTRAINT FK_DonNghi_NguoiDuyet FOREIGN KEY (MaNguoiDuyet) REFERENCES NhanVien(MaNhanVien),
    CONSTRAINT FK_DonNghi_ThayThe FOREIGN KEY (MaNguoiThayThe) REFERENCES NhanVien(MaNhanVien)
);
GO

-- 12. BẢNG TÀI KHOẢN (USER)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TaiKhoan')
CREATE TABLE TaiKhoan (
    TenDangNhap VARCHAR(50) NOT NULL PRIMARY KEY,
    MatKhau VARCHAR(255) NOT NULL,
    MaNhanVien VARCHAR(10) NULL,
    MaKhachHang VARCHAR(10) NULL,
    CONSTRAINT CK_TaiKhoan_Owner CHECK ((MaNhanVien IS NOT NULL AND MaKhachHang IS NULL) OR (MaNhanVien IS NULL AND MaKhachHang IS NOT NULL)),
    CONSTRAINT FK_TaiKhoan_NV FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien),
    CONSTRAINT FK_TaiKhoan_KH FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang)
);
GO

-- 13. BẢNG TÀI SẢN (LOCKER, PHÒNG TẮM...)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TaiSan')
CREATE TABLE TaiSan (
    MaTaiSan VARCHAR(10) NOT NULL PRIMARY KEY,
    TenTaiSan NVARCHAR(100) NOT NULL,
    MaCoSo VARCHAR(10) NOT NULL,
    LoaiTaiSan NVARCHAR(50) NOT NULL CHECK (LoaiTaiSan IN (N'Phòng tắm VIP', N'Tủ đồ cá nhân')),
    TinhTrang NVARCHAR(50) NOT NULL DEFAULT N'Còn trống' CHECK (TinhTrang IN (N'Còn trống', N'Đang sử dụng', N'Bảo trì')),
    CONSTRAINT FK_TaiSan_CoSo FOREIGN KEY (MaCoSo) REFERENCES CoSo(MaCoSo)
);
GO

-- 14. BẢNG DỊCH VỤ (NƯỚC, THUÊ VỢT, HLV...)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DichVu')
CREATE TABLE DichVu (
    MaDichVu VARCHAR(10) NOT NULL PRIMARY KEY,
    TenDichVu NVARCHAR(100) NOT NULL,
    DonGia DECIMAL(10,2) NOT NULL,
    DonViTinh NVARCHAR(20) NULL,
    SoLuongTon INT DEFAULT 0 CHECK (SoLuongTon >= 0),
    LoaiXuLy NVARCHAR(20) NOT NULL CHECK (LoaiXuLy IN ('TonKho', 'NhanSu', 'TaiSan'))
);
GO

-- 15. BẢNG PHIẾU ĐẶT SÂN
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PhieuDatSan')
CREATE TABLE PhieuDatSan (
    MaPhieuDat VARCHAR(10) NOT NULL PRIMARY KEY,
    MaKhachHang VARCHAR(10) NOT NULL,
    MaSan VARCHAR(10) NOT NULL,
    MaNhanVien VARCHAR(10) NULL, -- Null nếu đặt Online chưa ai duyệt
    GioBatDau DATETIME NOT NULL,
    GioKetThuc DATETIME NOT NULL,
    TrangThaiThanhToan NVARCHAR(50) NOT NULL DEFAULT N'Chưa thanh toán' CHECK ( TrangThaiThanhToan IN (N'Chưa thanh toán', N'Đã cọc', N'Đã thanh toán')),
    KenhDat NVARCHAR(20) NOT NULL CHECK (KenhDat IN ('Online', N'Trực tiếp')),
    CONSTRAINT CK_PhieuDat_ThoiGian CHECK (GioKetThuc > GioBatDau),
    CONSTRAINT FK_PhieuDat_KH FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang),
    CONSTRAINT FK_PhieuDat_San FOREIGN KEY (MaSan) REFERENCES SanTheThao(MaSan),
    CONSTRAINT FK_PhieuDat_NV FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien)
);
GO

-- 16. BẢNG HÓA ĐƠN
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'HoaDon')
CREATE TABLE HoaDon (
    MaHoaDon VARCHAR(10) NOT NULL PRIMARY KEY,
    MaPhieuDat VARCHAR(10) NOT NULL,
    MaNhanVien VARCHAR(10) NOT NULL, -- Người xuất hóa đơn
    ThoiGianLap DATETIME DEFAULT GETDATE(),
    GhiChu NVARCHAR(255) NULL,
    TyLeGiamGia DECIMAL(5,2) DEFAULT 0 CHECK (TyLeGiamGia >= 0 AND TyLeGiamGia <= 100),
    TongTien DECIMAL(10,2) DEFAULT 0 CHECK (TongTien >= 0),
    CONSTRAINT FK_HoaDon_Phieu FOREIGN KEY (MaPhieuDat) REFERENCES PhieuDatSan(MaPhieuDat),
    CONSTRAINT FK_HoaDon_NV FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien)
);
GO

-- 17. BẢNG CHI TIẾT SỬ DỤNG DỊCH VỤ
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ChiTietSuDungDichVu')
CREATE TABLE ChiTietSuDungDichVu (
    MaChiTietDV INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaHoaDon VARCHAR(10) NOT NULL,
    MaDichVu VARCHAR(10) NOT NULL,
    MaCoSo VARCHAR(10) NOT NULL,
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    MaNhanVienHLV VARCHAR(10) NULL, -- Nếu thuê HLV
    MaTaiSan VARCHAR(10) NULL, -- Nếu thuê Tủ đồ/Phòng tắm
    GioBatDau DATETIME NULL,
    GioKetThuc DATETIME NULL,
    CONSTRAINT FK_CTDV_HoaDon FOREIGN KEY (MaHoaDon) REFERENCES HoaDon(MaHoaDon),
    CONSTRAINT FK_CTDV_DichVu FOREIGN KEY (MaDichVu) REFERENCES DichVu(MaDichVu),
    CONSTRAINT FK_CTDV_CoSo FOREIGN KEY (MaCoSo) REFERENCES CoSo(MaCoSo),
    CONSTRAINT FK_CTDV_HLV FOREIGN KEY (MaNhanVienHLV) REFERENCES NhanVien(MaNhanVien),
    CONSTRAINT FK_CTDV_TaiSan FOREIGN KEY (MaTaiSan) REFERENCES TaiSan(MaTaiSan)
);
GO

-- Insert Mock Data
-- Thêm chi nhánh mẫu
IF NOT EXISTS (SELECT * FROM CoSo WHERE MaCoSo = 'CS_HCM01')
INSERT INTO CoSo (MaCoSo, TenCoSo, DiaChi, ThanhPho) VALUES 
('CS_HCM01', N'VietSport Quận 1', N'123 Nguyễn Huệ', N'Hồ Chí Minh');
IF NOT EXISTS (SELECT * FROM CoSo WHERE MaCoSo = 'CS_HCM02')
INSERT INTO CoSo (MaCoSo, TenCoSo, DiaChi, ThanhPho) VALUES 
('CS_HCM02', N'VietSport Cầu Giấy', N'456 Xuân Thủy', N'Hà Nội');

-- Thêm sân mẫu
IF NOT EXISTS (SELECT * FROM SanTheThao WHERE MaSan = 'SAN01')
INSERT INTO SanTheThao (MaSan, MaCoSo, LoaiSan, SucChua, TinhTrang) VALUES
('SAN01', 'CS_HCM01', N'Bóng đá mini', 10, N'Còn trống');
IF NOT EXISTS (SELECT * FROM SanTheThao WHERE MaSan = 'SAN02')
INSERT INTO SanTheThao (MaSan, MaCoSo, LoaiSan, SucChua, TinhTrang) VALUES
('SAN02', 'CS_HCM01', N'Cầu lông', 4, N'Đang sử dụng');
GO

-- BangGia.sql
-- Xóa giá cũ làm lại cho chuẩn
DELETE FROM GiaThueSan;

-- 1. Giá cho Bóng đá mini (Quận 1)
INSERT INTO GiaThueSan (MaCoSo, LoaiSan, KhungGio, DonGia) VALUES
('CS_HCM01', N'Bóng đá mini', N'Ngày thường', 200000),
('CS_HCM01', N'Bóng đá mini', N'Giờ cao điểm', 300000),
('CS_HCM01', N'Bóng đá mini', N'Cuối tuần', 350000),
('CS_HCM01', N'Bóng đá mini', N'Giờ thấp điểm', 150000);

-- 2. Giá cho Cầu lông (Quận 1)
INSERT INTO GiaThueSan (MaCoSo, LoaiSan, KhungGio, DonGia) VALUES
('CS_HCM01', N'Cầu lông', N'Ngày thường', 80000),
('CS_HCM01', N'Cầu lông', N'Giờ cao điểm', 120000),
('CS_HCM01', N'Cầu lông', N'Cuối tuần', 150000);

-- 3. Giá cho Tennis (Thủ Đức - Giả sử bạn có CS_HCM02)
-- Nếu chưa có CS_HCM02, bạn cần insert CoSo trước, hoặc đổi thành CS_HCM01
INSERT INTO GiaThueSan (MaCoSo, LoaiSan, KhungGio, DonGia) VALUES
('CS_HCM01', N'Tennis', N'Ngày thường', 400000),
('CS_HCM01', N'Tennis', N'Giờ cao điểm', 600000),
('CS_HCM01', N'Tennis', N'Cuối tuần', 700000);
GO

-- TaiKhoan_ThongTinNV.sql
-- Xóa dữ liệu cũ để tránh trùng lặp
DELETE FROM TaiKhoan WHERE MaNhanVien IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT');

-- Delete dependent records before deleting NhanVien
DELETE FROM ChiTietSuDungDichVu WHERE MaNhanVienHLV IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT');
DELETE FROM HoaDon WHERE MaNhanVien IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT');
DELETE FROM PhieuDatSan WHERE MaNhanVien IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT');
DELETE FROM DonNghiPhep WHERE MaNhanVien IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT') OR MaNguoiDuyet IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT') OR MaNguoiThayThe IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT');
DELETE FROM PhanCongCaTruc WHERE MaNhanVien IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT') OR MaQuanLy IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT') OR MaNguoiThayThe IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT');
DELETE FROM LuongNhanVien WHERE MaNhanVien IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT') OR NguoiTinhLuong IN ('NV_QL', 'NV_LT', 'NV_TN', 'NV_KT');

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

-- SQLQuery2.sql
-- Thêm nhân viên Quản trị (Admin)
DELETE FROM TaiKhoan WHERE MaNhanVien = 'AD01';

-- Delete dependent records for AD01
DELETE FROM ChiTietSuDungDichVu WHERE MaNhanVienHLV = 'AD01';
DELETE FROM HoaDon WHERE MaNhanVien = 'AD01';
DELETE FROM PhieuDatSan WHERE MaNhanVien = 'AD01';
DELETE FROM DonNghiPhep WHERE MaNhanVien = 'AD01' OR MaNguoiDuyet = 'AD01' OR MaNguoiThayThe = 'AD01';
DELETE FROM PhanCongCaTruc WHERE MaNhanVien = 'AD01' OR MaQuanLy = 'AD01' OR MaNguoiThayThe = 'AD01';
DELETE FROM LuongNhanVien WHERE MaNhanVien = 'AD01' OR NguoiTinhLuong = 'AD01';

DELETE FROM NhanVien WHERE MaNhanVien = 'AD01';
INSERT INTO NhanVien (MaNhanVien, HoTen, MaCoSo, ChucVu, CMND, SoDienThoai, GioiTinh, NgaySinh) VALUES 
('AD01', N'Super Admin', 'CS_HCM01', N'Quản trị', '999999999999', '0999999999', N'Nam', '1980-01-01');

-- Tạo tài khoản cho Admin
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, MaNhanVien) VALUES 
('admin', '123', 'AD01');

-- Đảm bảo bảng ThamSo có dữ liệu để chỉnh sửa
DELETE FROM ThamSo;
INSERT INTO ThamSo (MaThamSo, TenThamSo, GiaTri) VALUES 
('MIN_TIME', N'Thời gian đặt tối thiểu (phút)', 60),
('MAX_TIME', N'Thời gian đặt tối đa (phút)', 180),
('MAX_BOOK', N'Hạn mức số lượng sân/ngày', 3);
GO

-- DataTestChucNang.sql
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
DELETE FROM PhieuDatSan WHERE MaPhieuDat IN ('P_UNPAID', 'P_PAID');
INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
VALUES ('P_UNPAID', 'KH_TEST', 'SAN01', DATEADD(hour, 1, GETDATE()), DATEADD(hour, 3, GETDATE()), N'Chưa thanh toán', 'Online');

-- Phiếu 2: Đã thanh toán (Để Lễ tân test Check-in)
INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
VALUES ('P_PAID', 'KH_TEST', 'SAN01', DATEADD(day, 1, GETDATE()), DATEADD(day, 1, DATEADD(hour, 2, GETDATE())), N'Đã thanh toán', N'Trực tiếp');

GO

-- TestThuNgan.sql
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
DELETE FROM PhieuDatSan WHERE MaPhieuDat IN ('P_TEST01', 'P_TEST02', 'P_TEST03');
-- Phiếu 1: Đá hôm nay
INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
VALUES 
('P_TEST01', 'KH_TEST', 'SAN_TEST', DATEADD(hour, 1, GETDATE()), DATEADD(hour, 3, GETDATE()), N'Chưa thanh toán', 'Online');

-- Phiếu 2: Đá ngày mai
INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
VALUES 
('P_TEST02', 'KH_TEST', 'SAN_TEST', DATEADD(day, 1, GETDATE()), DATEADD(day, 1, DATEADD(hour, 2, GETDATE())), N'Chưa thanh toán', N'Trực tiếp');

-- Phiếu 3: Khách vãng lai
INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
VALUES 
('P_TEST03', 'KH_TEST', 'SAN01', GETDATE(), DATEADD(hour, 1, GETDATE()), N'Chưa thanh toán', 'Online');

GO