using System.Data.SqlClient;

public static class DatabaseHelper
{
    // HÃY DÁN TÊN SERVER BẠN VỪA COPY VÀO GIỮA DẤU NGOẶC KÉP
    // Ví dụ: public static string ServerName = "DESKTOP-U12345\\SQLEXPRESS"; 
    // Hoặc nếu tên máy bạn là dấu chấm (.) thì để nguyên "."

    public static string ServerName = @"localhost\MSSQL_NEW";

    public static string DbName = "VietSportDB";

    public static string ConnectionString => $"Data Source={ServerName};Initial Catalog={DbName};Integrated Security=True";

    public static SqlConnection GetConnection()
    {
        return new SqlConnection(ConnectionString);
    }
}