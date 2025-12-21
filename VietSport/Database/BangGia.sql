USE VietSportDB;
GO

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