namespace VietSportSystem
{
    public class ServiceItem
    {
        public string MaDV { get; set; }
        public string TenDV { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }
}