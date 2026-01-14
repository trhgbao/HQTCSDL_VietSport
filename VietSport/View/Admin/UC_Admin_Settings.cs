using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace VietSportSystem
{
    public class UC_Admin_Settings : UserControl
    {
        private TextBox txtMin, txtMax, txtLimit;
        private ComboBox cboCoSo;
        private Panel pnlCenter; // Để căn giữa toàn bộ
        private Button btnSave;

        // Các key tham số trong DB
        private const string KEY_MIN = "MIN_TIME";
        private const string KEY_MAX = "MAX_TIME";
        private const string KEY_LIMIT = "MAX_BOOK";

        public UC_Admin_Settings()
        {
            InitializeComponent();
            LoadCoSo();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            // 1. HEADER & COMBOBOX
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White };

            Label lblTitle = new Label { Text = "THAM SỐ QUY ĐỊNH", Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true, TextAlign = ContentAlignment.MiddleCenter };
            // Căn giữa Title
            lblTitle.Location = new Point((this.Width - lblTitle.Width) / 2, 20);
            this.Resize += (s, e) => lblTitle.Left = (this.Width - lblTitle.Width) / 2;

            Label lblFilter = new Label { Text = "Áp dụng cho:", Location = new Point(this.Width - 400, 30), AutoSize = true, Font = new Font("Segoe UI", 11), Anchor = AnchorStyles.Top | AnchorStyles.Right };

            cboCoSo = new ComboBox { Location = new Point(this.Width - 300, 28), Width = 250, Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            cboCoSo.SelectedIndexChanged += (s, e) => LoadParams(); // Chọn xong tự load lại dữ liệu

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblFilter, cboCoSo });

            // 2. CONTAINER CHÍNH (Để căn giữa các ô nhập)
            pnlCenter = new Panel { Size = new Size(800, 550) };
            pnlCenter.Location = new Point((this.Width - 800) / 2, 100);
            this.Resize += (s, e) => pnlCenter.Left = (this.Width - pnlCenter.Width) / 2;

            // Group Thời gian đặt
            GroupBox grpTime = new GroupBox { Text = "Thời gian đặt (phút)", Location = new Point(100, 20), Size = new Size(600, 150), Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            txtMin = CreateInput("Tối thiểu:", 50, grpTime);
            txtMax = CreateInput("Tối đa:", 350, grpTime);

            // Group Hạn mức
            GroupBox grpLimit = new GroupBox { Text = "Hạn mức số lượng sân / ngày", Location = new Point(100, 200), Size = new Size(600, 150), Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            txtLimit = CreateInput("Số lượng:", 225, grpLimit); // Căn giữa ô nhập

            // Button (Sửa text và Căn giữa)
            btnSave = new Button { Text = "Xác nhận", Size = new Size(200, 50) };
            btnSave.Location = new Point((pnlCenter.Width - btnSave.Width) / 2, 400); // Căn giữa button trong panel
            UIHelper.StyleButton(btnSave, true);
            btnSave.Click += BtnSave_Click;

            pnlCenter.Controls.AddRange(new Control[] { grpTime, grpLimit, btnSave });

            this.Controls.Add(pnlCenter);
            this.Controls.Add(pnlHeader);
        }

        private TextBox CreateInput(string label, int x, GroupBox parent)
        {
            Label lbl = new Label { Text = label, Location = new Point(x, 50), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Regular) };
            TextBox txt = new TextBox { Location = new Point(x, 80), Width = 150, BackColor = Color.WhiteSmoke, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 12) };

            // Sự kiện: Khi click vào ô xám (đang ở trạng thái Khác nhau) thì xóa trắng để nhập
            txt.Enter += (s, e) => {
                if (txt.ForeColor == Color.Gray)
                {
                    txt.Text = "";
                    txt.ForeColor = Color.Black;
                }
            };

            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
            return txt;
        }

        private void LoadCoSo()
        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT MaCoSo, TenCoSo FROM CoSo", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var list = new System.Collections.ArrayList();
                            // Item đặc biệt: Tất cả
                            list.Add(new { ID = "ALL", Name = "Toàn bộ hệ thống (Mặc định)" });

                            while (reader.Read())
                            {
                                list.Add(new { ID = reader["MaCoSo"].ToString(), Name = reader["TenCoSo"].ToString() });
                            }

                            cboCoSo.DisplayMember = "Name";
                            cboCoSo.ValueMember = "ID";
                            cboCoSo.DataSource = list;
                        }
                    }
                }
            }
            catch { }
        }

        private void LoadParams()
        {
            if (cboCoSo.SelectedValue == null) return;
            string selectedCS = cboCoSo.SelectedValue.ToString();

            // Reset trạng thái ô nhập về bình thường trước
            ResetInputState(txtMin);
            ResetInputState(txtMax);
            ResetInputState(txtLimit);

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                if (selectedCS == "ALL")
                {
                    // --- CHẾ ĐỘ TOÀN BỘ: Kiểm tra xem các cơ sở có giá trị khác nhau không ---
                    CheckAndDisplayValue(conn, KEY_MIN, txtMin);
                    CheckAndDisplayValue(conn, KEY_MAX, txtMax);
                    CheckAndDisplayValue(conn, KEY_LIMIT, txtLimit);
                }
                else
                {
                    // --- CHẾ ĐỘ CƠ SỞ RIÊNG: Load giá trị của cơ sở đó (Nếu không có thì lấy Global) ---
                    LoadSpecificValue(conn, selectedCS, KEY_MIN, txtMin);
                    LoadSpecificValue(conn, selectedCS, KEY_MAX, txtMax);
                    LoadSpecificValue(conn, selectedCS, KEY_LIMIT, txtLimit);
                }
            }
        }

        private void ResetInputState(TextBox txt)
        {
            txt.ForeColor = Color.Black;
            txt.Text = "";
        }

        // Logic cho chế độ "ALL"
        private void CheckAndDisplayValue(SqlConnection conn, string key, TextBox txt)
        {
            // Đếm xem có bao nhiêu giá trị khác nhau cho tham số này
            // (Tính cả giá trị mặc định MaCoSo IS NULL và giá trị riêng)
            string sql = "SELECT DISTINCT GiaTri FROM ThamSo WHERE MaThamSo = @Key";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Key", key);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    List<string> values = new List<string>();
                    while (reader.Read()) values.Add(reader["GiaTri"].ToString());

                    if (values.Count > 1)
                    {
                        // Nếu có nhiều hơn 1 giá trị -> Tức là các cơ sở đang không đồng bộ
                        txt.Text = "(Khác nhau)";
                        txt.ForeColor = Color.Gray; // Tô xám
                    }
                    else if (values.Count == 1)
                    {
                        txt.Text = values[0]; // Đồng bộ thì hiện giá trị đó
                    }
                    else
                    {
                        txt.Text = "0"; // Chưa có dữ liệu
                    }
                }
            }
        }

        // Logic cho chế độ "Cơ sở riêng"
        private void LoadSpecificValue(SqlConnection conn, string maCoSo, string key, TextBox txt)
        {
            // Ưu tiên lấy giá trị của cơ sở, nếu không có thì lấy giá trị Global (MaCoSo IS NULL)
            string sql = @"
                SELECT TOP 1 GiaTri FROM ThamSo 
                WHERE MaThamSo = @Key AND (MaCoSo = @CS OR MaCoSo IS NULL)
                ORDER BY MaCoSo DESC"; // DESC để MaCoSo (có giá trị) xếp trên NULL

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Key", key);
                cmd.Parameters.AddWithValue("@CS", maCoSo);

                object result = cmd.ExecuteScalar();
                txt.Text = result != null ? result.ToString() : "0";
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cboCoSo.SelectedValue == null) return;
            string selectedCS = cboCoSo.SelectedValue.ToString();

            // Validate sơ sơ
            if (!IsNumeric(txtMin.Text) || !IsNumeric(txtMax.Text) || !IsNumeric(txtLimit.Text))
            {
                MessageBox.Show("Vui lòng nhập số hợp lệ (hoặc giữ nguyên nếu đang xám)!");
                return;
            }

            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    SqlTransaction trans = conn.BeginTransaction();

                    try
                    {
                        if (selectedCS == "ALL")
                        {
                            // --- LƯU TOÀN BỘ: Xóa hết cái riêng, cập nhật cái chung ---
                            // Chỉ cập nhật những ô không bị xám (hoặc người dùng đã sửa ô xám thành số)
                            SaveGlobal(conn, trans, KEY_MIN, txtMin);
                            SaveGlobal(conn, trans, KEY_MAX, txtMax);
                            SaveGlobal(conn, trans, KEY_LIMIT, txtLimit);
                        }
                        else
                        {
                            // --- LƯU RIÊNG: Chỉ Insert/Update cho cơ sở này ---
                            SaveSpecific(conn, trans, selectedCS, KEY_MIN, txtMin.Text);
                            SaveSpecific(conn, trans, selectedCS, KEY_MAX, txtMax.Text);
                            SaveSpecific(conn, trans, selectedCS, KEY_LIMIT, txtLimit.Text);
                        }

                        trans.Commit();
                        MessageBox.Show("Cập nhật thành công!");
                        LoadParams(); // Load lại để cập nhật giao diện
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Lỗi: " + ex.Message);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi kết nối: " + ex.Message); }
        }

        private void SaveGlobal(SqlConnection conn, SqlTransaction trans, string key, TextBox txt)
        {
            // Nếu ô đang xám "(Khác nhau)" nghĩa là người dùng không sửa gì -> Bỏ qua
            if (txt.ForeColor == Color.Gray) return;

            string val = txt.Text;

            // 1. Xóa tất cả thiết lập riêng của các cơ sở (để tất cả quay về dùng chung)
            SqlCommand cmdDel = new SqlCommand("DELETE FROM ThamSo WHERE MaThamSo = @Key AND MaCoSo IS NOT NULL", conn, trans);
            cmdDel.Parameters.AddWithValue("@Key", key);
            cmdDel.ExecuteNonQuery();

            // 2. Cập nhật hoặc Thêm tham số mặc định (MaCoSo IS NULL)
            string sqlUpsert = @"
                IF EXISTS (SELECT * FROM ThamSo WHERE MaThamSo = @Key AND MaCoSo IS NULL)
                    UPDATE ThamSo SET GiaTri = @Val WHERE MaThamSo = @Key AND MaCoSo IS NULL
                ELSE
                    INSERT INTO ThamSo (MaThamSo, TenThamSo, GiaTri, MaCoSo) VALUES (@Key, @Key, @Val, NULL)";

            SqlCommand cmd = new SqlCommand(sqlUpsert, conn, trans);
            cmd.Parameters.AddWithValue("@Key", key);
            cmd.Parameters.AddWithValue("@Val", val);
            cmd.ExecuteNonQuery();
        }

        private void SaveSpecific(SqlConnection conn, SqlTransaction trans, string maCoSo, string key, string val)
        {
            // Insert hoặc Update cho dòng có MaCoSo cụ thể
            string sql = @"
                IF EXISTS (SELECT * FROM ThamSo WHERE MaThamSo = @Key AND MaCoSo = @CS)
                    UPDATE ThamSo SET GiaTri = @Val WHERE MaThamSo = @Key AND MaCoSo = @CS
                ELSE
                    INSERT INTO ThamSo (MaThamSo, TenThamSo, GiaTri, MaCoSo) VALUES (@Key, @Key, @Val, @CS)";

            SqlCommand cmd = new SqlCommand(sql, conn, trans);
            cmd.Parameters.AddWithValue("@Key", key);
            cmd.Parameters.AddWithValue("@CS", maCoSo);
            cmd.Parameters.AddWithValue("@Val", val);
            cmd.ExecuteNonQuery();
        }

        private bool IsNumeric(string text)
        {
            if (text == "(Khác nhau)") return true; // Bỏ qua check nếu đang xám
            return decimal.TryParse(text, out _);
        }
    }
}