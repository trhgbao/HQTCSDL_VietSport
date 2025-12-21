using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace VietSportSystem
{
    public class UC_Admin_PriceList : UserControl
    {
        private FlowLayoutPanel pnlList;
        private ComboBox cboCoSo;

        // Danh sách các loại sân cố định của hệ thống
        private List<string> GetDynamicTypes(string maCoSo)
        {
            List<string> list = new List<string>();
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Chỉ lấy những loại sân đang tồn tại ở cơ sở này (DISTINCT để không trùng)
                string sql = "SELECT DISTINCT LoaiSan FROM SanTheThao WHERE MaCoSo = @Ma";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Ma", maCoSo);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(reader["LoaiSan"].ToString());
                }
            }
            return list;
        }

        public UC_Admin_PriceList()
        {
            InitializeComponent();
            LoadCoSo(); // Load danh sách cơ sở vào ComboBox
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;

            // 1. Header & Bộ lọc Cơ sở
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.WhiteSmoke };

            Label lblTitle = new Label { Text = "ĐIỀU CHỈNH BẢNG GIÁ", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 25), AutoSize = true };

            Label lblFilter = new Label { Text = "Chọn Chi nhánh:", Location = new Point(400, 30), AutoSize = true, Font = new Font("Segoe UI", 11) };
            cboCoSo = new ComboBox { Location = new Point(530, 28), Width = 250, Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboCoSo.SelectedIndexChanged += (s, e) => LoadPrices(); // Khi chọn cơ sở thì load lại giá

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblFilter, cboCoSo });

            // 2. Danh sách giá (FlowLayout)
            pnlList = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(30) };

            this.Controls.Add(pnlList);
            this.Controls.Add(pnlHeader);
        }

        private void LoadCoSo()
        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT MaCoSo, TenCoSo FROM CoSo", conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    // Tạo class tạm để bind dữ liệu
                    var list = new System.Collections.ArrayList();
                    while (reader.Read())
                    {
                        list.Add(new { ID = reader["MaCoSo"].ToString(), Name = reader["TenCoSo"].ToString() });
                    }
                    cboCoSo.DisplayMember = "Name";
                    cboCoSo.ValueMember = "ID";
                    cboCoSo.DataSource = list;
                }
            }
            catch { }
        }

        private void LoadPrices()
        {
            if (cboCoSo.SelectedValue == null) return;
            string maCoSo = cboCoSo.SelectedValue.ToString();

            pnlList.Controls.Clear();

            // 1. Lấy giá hiện tại đã lưu trong DB (để điền vào ô nhập)
            Dictionary<string, decimal> currentPrices = GetCurrentPricesFromDB(maCoSo);

            // 2. Lấy danh sách loại sân THỰC TẾ tại cơ sở này
            List<string> dynamicTypes = GetDynamicTypes(maCoSo);

            if (dynamicTypes.Count == 0)
            {
                Label lblEmpty = new Label
                {
                    Text = "Cơ sở này chưa có sân nào được khai báo.\nVui lòng thêm sân bên phần 'Quản lý sân' trước.",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 12, FontStyle.Italic),
                    ForeColor = Color.Red
                };
                pnlList.Controls.Add(lblEmpty);
                return;
            }

            // 3. Tạo giao diện nhập giá cho từng loại tìm được
            foreach (var loai in dynamicTypes)
            {
                pnlList.Controls.Add(CreatePriceBlock(loai, currentPrices));
            }
        }

        // Hàm lấy giá từ DB về Dictionary để tra cứu nhanh
        private Dictionary<string, decimal> GetCurrentPricesFromDB(string maCoSo)
        {
            Dictionary<string, decimal> dict = new Dictionary<string, decimal>();
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Key = "LoaiSan_KhungGio" (Ví dụ: Tennis_Ngày thường)
                string sql = "SELECT LoaiSan, KhungGio, DonGia FROM GiaThueSan WHERE MaCoSo = @Ma";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Ma", maCoSo);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string key = reader["LoaiSan"].ToString() + "_" + reader["KhungGio"].ToString();
                    dict[key] = Convert.ToDecimal(reader["DonGia"]);
                }
            }
            return dict;
        }

        private Panel CreatePriceBlock(string loaiSan, Dictionary<string, decimal> prices)
        {
            Panel pnl = new Panel { Size = new Size(950, 200), BackColor = Color.WhiteSmoke, Margin = new Padding(0, 0, 0, 30) };

            // Tên Loại Sân
            Label lblName = new Label { Text = loaiSan, Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true, ForeColor = UIHelper.PrimaryColor };

            // Các ô nhập giá (Lấy giá từ dict nếu có)
            TextBox txtNgay = CreatePriceInput("Giá Ban Ngày:", 300, 20, pnl, GetPrice(prices, loaiSan, "Ngày thường"));
            TextBox txtDem = CreatePriceInput("Giá Ban Đêm:", 300, 80, pnl, GetPrice(prices, loaiSan, "Giờ cao điểm"));

            TextBox txtCuoiTuan = CreatePriceInput("Giá Cuối Tuần:", 550, 20, pnl, GetPrice(prices, loaiSan, "Cuối tuần"));
            TextBox txtLe = CreatePriceInput("Giá Ngày Lễ:", 550, 80, pnl, GetPrice(prices, loaiSan, "Giờ thấp điểm")); // Demo mapping

            // Nút Xác nhận
            Button btnSave = new Button { Text = "Lưu Giá", Location = new Point(800, 140), Size = new Size(130, 40) };
            UIHelper.StyleButton(btnSave, true);
            btnSave.Click += (s, e) => SavePrice(loaiSan, txtNgay.Text, txtDem.Text, txtCuoiTuan.Text, txtLe.Text);

            pnl.Controls.AddRange(new Control[] { lblName, btnSave });
            return pnl;
        }

        private decimal GetPrice(Dictionary<string, decimal> dict, string loai, string khung)
        {
            string key = loai + "_" + khung;
            return dict.ContainsKey(key) ? dict[key] : 0;
        }

        private TextBox CreatePriceInput(string label, int x, int y, Panel parent, decimal val)
        {
            Label lbl = new Label { Text = label, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 10) };
            TextBox txt = new TextBox { Location = new Point(x + 110, y - 3), Width = 120, Font = new Font("Segoe UI", 11) };
            txt.Text = val > 0 ? val.ToString("N0") : "0"; // Format số đẹp
            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
            return txt;
        }

        // --- LOGIC LƯU VÀO DATABASE (QUAN TRỌNG) ---
        private void SavePrice(string loaiSan, string pNgay, string pDem, string pCuoiTuan, string pLe)
        {
            if (cboCoSo.SelectedValue == null) return;
            string maCoSo = cboCoSo.SelectedValue.ToString();

            // Chuyển chuỗi thành số (Xóa dấu phẩy format)
            decimal giaNgay = decimal.Parse(pNgay.Replace(",", "").Replace(".", ""));
            decimal giaDem = decimal.Parse(pDem.Replace(",", "").Replace(".", ""));
            decimal giaCuoiTuan = decimal.Parse(pCuoiTuan.Replace(",", "").Replace(".", ""));
            decimal giaLe = decimal.Parse(pLe.Replace(",", "").Replace(".", ""));

            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    SqlTransaction trans = conn.BeginTransaction();

                    try
                    {
                        // Lưu từng khung giờ (Gọi hàm UpdateOrInsert)
                        UpdateDB(conn, trans, maCoSo, loaiSan, "Ngày thường", giaNgay);
                        UpdateDB(conn, trans, maCoSo, loaiSan, "Giờ cao điểm", giaDem); // Demo map Ban Đêm = Cao điểm
                        UpdateDB(conn, trans, maCoSo, loaiSan, "Cuối tuần", giaCuoiTuan);
                        UpdateDB(conn, trans, maCoSo, loaiSan, "Giờ thấp điểm", giaLe); // Demo map Lễ = Thấp điểm

                        trans.Commit();
                        MessageBox.Show($"Đã cập nhật giá {loaiSan} tại {cboCoSo.Text} thành công!");
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu giá: " + ex.Message);
            }
        }

        private void UpdateDB(SqlConnection conn, SqlTransaction trans, string coSo, string loai, string khung, decimal gia)
        {
            // Kiểm tra xem đã có giá này chưa. Nếu có thì Update, chưa thì Insert
            string sql = @"
                IF EXISTS (SELECT * FROM GiaThueSan WHERE MaCoSo = @CS AND LoaiSan = @Loai AND KhungGio = @Khung)
                    UPDATE GiaThueSan SET DonGia = @Gia WHERE MaCoSo = @CS AND LoaiSan = @Loai AND KhungGio = @Khung
                ELSE
                    INSERT INTO GiaThueSan (MaCoSo, LoaiSan, KhungGio, DonGia) VALUES (@CS, @Loai, @Khung, @Gia)";

            SqlCommand cmd = new SqlCommand(sql, conn, trans);
            cmd.Parameters.AddWithValue("@CS", coSo);
            cmd.Parameters.AddWithValue("@Loai", loai);
            cmd.Parameters.AddWithValue("@Khung", khung);
            cmd.Parameters.AddWithValue("@Gia", gia);
            cmd.ExecuteNonQuery();
        }
    }
}