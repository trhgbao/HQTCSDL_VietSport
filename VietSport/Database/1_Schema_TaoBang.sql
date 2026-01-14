-- 1. TẠO DATABASE
CREATE DATABASE VietSportDB;
GO
USE VietSportDB;
GO

-- 2. BẢNG CƠ SỞ (Chi nhánh)
CREATE TABLE CoSo (
    MaCoSo VARCHAR(10) NOT NULL PRIMARY KEY,
    TenCoSo NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(255) NOT NULL,
    ThanhPho NVARCHAR(50) NOT NULL
);
GO

-- 3. BẢNG THAM SỐ HỆ THỐNG (Cập nhật PK tự tăng để hỗ trợ đa cơ sở)
CREATE TABLE ThamSo (
    ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaThamSo VARCHAR(20) NOT NULL,
    TenThamSo NVARCHAR(100) NOT NULL,
    GiaTri DECIMAL(10,2) NOT NULL DEFAULT 0,
    MaCoSo VARCHAR(10) NULL,
    LoaiSan NVARCHAR(50) NULL CHECK (LoaiSan IN (N'Bóng đá mini', N'Cầu lông', N'Tennis', N'Bóng rổ', N'Futsal')),
    GhiChu NVARCHAR(255) NULL,
    CONSTRAINT FK_ThamSo_CoSo FOREIGN KEY (MaCoSo) REFERENCES CoSo(MaCoSo),
    CONSTRAINT UQ_ThamSo_CoSo UNIQUE (MaThamSo, MaCoSo) 
);
GO

-- 4. BẢNG SÂN THỂ THAO (Đã thêm cột GhiChu)
CREATE TABLE SanTheThao (
    MaSan VARCHAR(10) NOT NULL PRIMARY KEY,
    MaCoSo VARCHAR(10) NOT NULL,
    LoaiSan NVARCHAR(50) NOT NULL CHECK (LoaiSan IN (N'Bóng đá mini', N'Cầu lông', N'Tennis', N'Bóng rổ', N'Futsal')),
    SucChua INT NOT NULL CHECK (SucChua > 0),
    TinhTrang NVARCHAR(50) NOT NULL DEFAULT N'Còn trống' CHECK (TinhTrang IN (N'Còn trống', N'Đã đặt', N'Đang sử dụng', N'Bảo trì')),
    GhiChu NVARCHAR(500) NULL, -- Cột này dùng để lưu lý do bảo trì
	CONSTRAINT FK_San_CoSo FOREIGN KEY (MaCoSo) REFERENCES CoSo(MaCoSo)
);
GO

-- 5. BẢNG THỜI GIAN ĐẶT
CREATE TABLE ThoiGianDat (
    MaThoiGianDat INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaSan VARCHAR(10) NOT NULL,
    GioBatDau DATETIME NOT NULL,
    GioKetThuc DATETIME NOT NULL,
    TinhTrangDat NVARCHAR(50) NOT NULL DEFAULT N'Còn trống',
    LyDoThayDoi NVARCHAR(255) NULL,
    NgayCapNhat DATETIME DEFAULT GETDATE(),
    CONSTRAINT CK_ThoiGianDat_Gio CHECK (GioKetThuc > GioBatDau),
    CONSTRAINT FK_ThoiGian_San FOREIGN KEY (MaSan) REFERENCES SanTheThao(MaSan)
);
GO

-- 6. BẢNG GIÁ THUÊ SÂN
CREATE TABLE GiaThueSan (
    MaGia INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaCoSo VARCHAR(10) NOT NULL,
    LoaiSan NVARCHAR(50) NOT NULL,
    KhungGio NVARCHAR(50) NOT NULL CHECK (KhungGio IN (N'Ngày thường', N'Cuối tuần', N'Giờ thấp điểm', N'Giờ cao điểm')),
    DonGia DECIMAL(10,2) NOT NULL CHECK (DonGia > 0),
    CONSTRAINT FK_Gia_CoSo FOREIGN KEY (MaCoSo) REFERENCES CoSo(MaCoSo)
);
GO

-- 7. BẢNG KHÁCH HÀNG (Đã thêm Email, DiaChi và check UNIQUE Email)
CREATE TABLE KhachHang (
    MaKhachHang VARCHAR(10) NOT NULL PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATETIME NOT NULL,
    SoDienThoai VARCHAR(15) NOT NULL UNIQUE,
    Email VARCHAR(100) NULL UNIQUE,
    DiaChi NVARCHAR(255) NULL, -- Thêm mới
    CapBacThanhVien NVARCHAR(20) DEFAULT 'Standard' CHECK (CapBacThanhVien IN ('Standard', 'Silver', 'Gold', 'Platinum')),
    LaHSSV BIT NOT NULL DEFAULT 0,
    LaNguoiCaoTuoi BIT NOT NULL DEFAULT 0,
    CMND VARCHAR(12) NOT NULL UNIQUE,
    GioiTinh NVARCHAR(10) CHECK (GioiTinh IN (N'Nam', N'Nữ', N'Khác'))
);
GO

-- 8. BẢNG NHÂN VIÊN (Đã thêm NgaySinh)
CREATE TABLE NhanVien (
    MaNhanVien VARCHAR(10) NOT NULL PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
	NgaySinh DATETIME NULL, -- Thêm mới để tránh lỗi load Admin
    MaCoSo VARCHAR(10) NOT NULL,
    ChucVu NVARCHAR(50) NOT NULL CHECK (ChucVu IN (N'Quản lý', N'Lễ tân', N'Kỹ thuật', N'Thu ngân', N'Quản trị', N'Huấn luyện viên')),
    CMND VARCHAR(12) NOT NULL UNIQUE,
    SoDienThoai VARCHAR(15) NOT NULL UNIQUE,
    GioiTinh NVARCHAR(10) CHECK (GioiTinh IN (N'Nam', N'Nữ', N'Khác')),
    CONSTRAINT FK_NV_CoSo FOREIGN KEY (MaCoSo) REFERENCES CoSo(MaCoSo)
);
GO

-- 9. BẢNG LƯƠNG NHÂN VIÊN
CREATE TABLE LuongNhanVien (
    MaNhanVien VARCHAR(10) NOT NULL,
    ThangNam DATETIME NOT NULL,
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
CREATE TABLE DonNghiPhep (
    MaDon INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaNhanVien VARCHAR(10) NOT NULL,
    NgayGui DATETIME NOT NULL DEFAULT GETDATE(),
    LyDo NVARCHAR(500) NULL,
    TrangThaiDuyet NVARCHAR(20) NOT NULL DEFAULT N'Chờ duyệt' CHECK (TrangThaiDuyet IN (N'Chờ duyệt', N'Đã duyệt', N'Từ chối')),
    MaNguoiDuyet VARCHAR(10) NOT NULL,
    MaNguoiThayThe VARCHAR(10) NOT NULL,
    CONSTRAINT FK_DonNghi_NV FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien),
    CONSTRAINT FK_DonNghi_NguoiDuyet FOREIGN KEY (MaNguoiDuyet) REFERENCES NhanVien(MaNhanVien),
    CONSTRAINT FK_DonNghi_ThayThe FOREIGN KEY (MaNguoiThayThe) REFERENCES NhanVien(MaNhanVien)
);
GO

-- 12. BẢNG TÀI KHOẢN (USER)
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
CREATE TABLE DichVu (
    MaDichVu VARCHAR(10) NOT NULL PRIMARY KEY,
    TenDichVu NVARCHAR(100) NOT NULL,
    DonGia DECIMAL(10,2) NOT NULL,
    DonViTinh NVARCHAR(20) NULL,
    SoLuongTon INT DEFAULT 0 CHECK (SoLuongTon >= 0),
    LoaiXuLy NVARCHAR(20) NOT NULL CHECK (LoaiXuLy IN ('TonKho', 'NhanSu', 'TaiSan'))
);
GO

-- 15. BẢNG PHIẾU ĐẶT SÂN (Đã thêm cột DaHuy)
CREATE TABLE PhieuDatSan (
    MaPhieuDat VARCHAR(10) NOT NULL PRIMARY KEY,
    MaKhachHang VARCHAR(10) NOT NULL,
    MaSan VARCHAR(10) NOT NULL,
    MaNhanVien VARCHAR(10) NULL, 
    GioBatDau DATETIME NOT NULL,
    GioKetThuc DATETIME NOT NULL,
    TrangThaiThanhToan NVARCHAR(50) NOT NULL DEFAULT N'Chưa thanh toán' CHECK (TrangThaiThanhToan IN (N'Chưa thanh toán', N'Đã cọc', N'Đã thanh toán')),
    KenhDat NVARCHAR(20) NOT NULL CHECK (KenhDat IN ('Online', N'Trực tiếp')),
    DaHuy BIT NOT NULL DEFAULT 0, -- Cột quan trọng để xử lý Lost Update
    CONSTRAINT CK_PhieuDat_ThoiGian CHECK (GioKetThuc > GioBatDau),
    CONSTRAINT FK_PhieuDat_KH FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang),
    CONSTRAINT FK_PhieuDat_San FOREIGN KEY (MaSan) REFERENCES SanTheThao(MaSan),
    CONSTRAINT FK_PhieuDat_NV FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien)
);
GO

-- 16. BẢNG HÓA ĐƠN
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

-- 18. BẢNG CHÍNH SÁCH GIẢM GIÁ (Thêm mới cho Demo Non-Repeatable Read)
CREATE TABLE ChinhSachGiamGia (
    MaChinhSach INT IDENTITY(1,1) PRIMARY KEY,
    HangTV NVARCHAR(20) NOT NULL UNIQUE, -- Standard, Silver, Gold...
    GiamGia DECIMAL(5,2) NOT NULL DEFAULT 0, -- % giảm
    NgayCapNhat DATETIME DEFAULT GETDATE()
);
GO

-- ************************
-- NẠP DỮ LIỆU MẶC ĐỊNH
-- ************************

-- Chính sách giảm giá mặc định
INSERT INTO ChinhSachGiamGia (HangTV, GiamGia) VALUES
('Standard', 0), ('Silver', 5), ('Gold', 10), ('Platinum', 20);

-- Tham số mặc định
INSERT INTO ThamSo (MaThamSo, TenThamSo, GiaTri, MaCoSo) VALUES 
('MIN_TIME', N'Thời gian đặt tối thiểu (phút)', 60, NULL),
('MAX_TIME', N'Thời gian đặt tối đa (phút)', 180, NULL),
('MAX_BOOK', N'Hạn mức số lượng sân/ngày', 3, NULL);
GO