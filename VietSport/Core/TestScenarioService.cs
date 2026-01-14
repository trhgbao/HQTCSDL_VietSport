using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietSportSystem.Core
{
    public class TestScenarioService
    {
        // --- SCENARIO 9: Non-Repeatable Read ---
        public string Scenario9_T1_UpdatePrice_Manager(string hangTV, decimal mucGiamMoi)
        {
            return DatabaseHelper.Sp_CapNhatChinhSachGia(hangTV, mucGiamMoi);
        }

        public string Scenario9_T2_Payment_Error(string maKH, string maPhieu)
        {
            return DatabaseHelper.Sp_ThanhToanDonHang(maKH, maPhieu, false);
        }

        public string Scenario9_T2_Payment_Fix(string maKH, string maPhieu)
        {
            return DatabaseHelper.Sp_ThanhToanDonHang(maKH, maPhieu, true);
        }

        // --- SCENARIO 10: Dirty Read (Maintenance) ---
        public string Scenario10_T1_Maintenance_Tech(string maSan)
        {
            return DatabaseHelper.Sp_CapNhatBaoTriSan(maSan);
        }

        public string Scenario10_T2_ViewStatus_Manager_Error(string maCoSo)
        {
            return DatabaseHelper.Sp_XemThongTinSan(maCoSo, false);
        }

        public string Scenario10_T2_ViewStatus_Manager_Fix(string maCoSo)
        {
            return DatabaseHelper.Sp_XemThongTinSan(maCoSo, true);
        }

        // --- SCENARIO 15: Dirty Read (Change Time vs Booking) ---
        public string Scenario15_T1_ChangeTime_Customer(string maPhieuCu, string maSanMoi, DateTime start, DateTime end)
        {
            return DatabaseHelper.Sp_DoiGioSan(maPhieuCu, maSanMoi, start, end);
        }

        public string Scenario15_T2_Booking_Receptionist_Error(string maSan, DateTime start, DateTime end, string maKH)
        {
            return DatabaseHelper.Sp_TimVaDatSanTrong(maSan, start, end, maKH, false);
        }

        public string Scenario15_T2_Booking_Receptionist_Fix(string maSan, DateTime start, DateTime end, string maKH)
        {
            return DatabaseHelper.Sp_TimVaDatSanTrong(maSan, start, end, maKH, true);
        }

        public void ResetData()
        {
            // Optional: Call a stored proc to reset data if needed
            // For now, we rely on the specific test data being ready in SQL
        }
    }
}
