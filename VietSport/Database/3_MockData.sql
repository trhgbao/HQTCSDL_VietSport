USE VietSportDB;
GO
-- Thêm chi nhánh mẫu
INSERT INTO CoSo (MaCoSo, TenCoSo, DiaChi, ThanhPho) VALUES 
('CS01', N'VietSport Quận 1', N'123 Nguyễn Huệ', N'Hồ Chí Minh'),
('CS02', N'VietSport Cầu Giấy', N'456 Xuân Thủy', N'Hà Nội');

-- Thêm sân mẫu
INSERT INTO SanTheThao (MaSan, MaCoSo, LoaiSan, SucChua, TinhTrang) VALUES
('S01', 'CS01', N'Bóng đá mini', 10, N'Còn trống'),
('S02', 'CS01', N'Cầu lông', 4, N'Đang sử dụng');