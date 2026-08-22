using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace EmployeeDetails
{
    public class User
    {
        private static string myConn = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;

        public int UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }

        private const string SelectUserForLoginQuery = "SELECT UserID, Password, Role FROM dbo.Users WHERE Username = @Username";
        private const string CheckUsernameQuery = "SELECT COUNT(1) FROM dbo.Users WHERE Username = @Username";
        private const string InsertUserQuery = "INSERT INTO dbo.Users (Username, Password, Role) VALUES (@Username, @Password, @Role); SELECT SCOPE_IDENTITY();";
        private const string UpdatePasswordHashQuery = "UPDATE dbo.Users SET Password = @Password WHERE UserID = @UserID";
        private const string InsertHistoryQuery = "INSERT INTO dbo.LoginHistory (UserID, Username, Action) VALUES (@UserID, @Username, @Action)";

        public static string HashPassword(string rawPassword)
        {
            if (string.IsNullOrEmpty(rawPassword)) return string.Empty;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawPassword));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static int ValidateLogin(string username, string password)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(myConn))
                {
                    con.Open();
                    using (SqlCommand com = new SqlCommand(SelectUserForLoginQuery, con))
                    {
                        com.Parameters.AddWithValue("@Username", username);
                        using (SqlDataReader reader = com.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = Convert.ToInt32(reader["UserID"]);
                                string storedPassword = reader["Password"].ToString();
                                string role = reader["Role"] != DBNull.Value ? reader["Role"].ToString() : "User";

                                string inputHash = HashPassword(password);

                                // Check if stored password matches hashed password OR legacy plaintext password
                                if (storedPassword == inputHash || storedPassword == password)
                                {
                                    reader.Close();

                                    // If password was stored in plaintext, auto-upgrade to SHA-256 hash
                                    if (storedPassword == password && storedPassword != inputHash)
                                    {
                                        using (SqlCommand updateCmd = new SqlCommand(UpdatePasswordHashQuery, con))
                                        {
                                            updateCmd.Parameters.AddWithValue("@Password", inputHash);
                                            updateCmd.Parameters.AddWithValue("@UserID", userId);
                                            updateCmd.ExecuteNonQuery();
                                        }
                                    }

                                    Session.UserID = userId;
                                    Session.Username = username;
                                    Session.Role = role;
                                    return userId;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ValidateLogin Error: " + ex.Message);
            }
            return 0;
        }

        public static bool UsernameExists(string username)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(myConn))
                {
                    con.Open();
                    using (SqlCommand com = new SqlCommand(CheckUsernameQuery, con))
                    {
                        com.Parameters.AddWithValue("@Username", username);
                        object result = com.ExecuteScalar();
                        int count = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UsernameExists Error: " + ex.Message);
                return false;
            }
        }

        public static int RegisterUser(string username, string password, string role = "User")
        {
            try
            {
                string hashedPassword = HashPassword(password);
                using (SqlConnection con = new SqlConnection(myConn))
                {
                    con.Open();
                    using (SqlCommand com = new SqlCommand(InsertUserQuery, con))
                    {
                        com.Parameters.AddWithValue("@Username", username);
                        com.Parameters.AddWithValue("@Password", hashedPassword);
                        com.Parameters.AddWithValue("@Role", role);

                        object result = com.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("RegisterUser Error: " + ex.Message);
            }
            return 0;
        }

        public static void LogActivity(int? userId, string username, string action)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(myConn))
                {
                    con.Open();
                    using (SqlCommand com = new SqlCommand(InsertHistoryQuery, con))
                    {
                        com.Parameters.AddWithValue("@UserID", (object)userId ?? DBNull.Value);
                        com.Parameters.AddWithValue("@Username", username ?? string.Empty);
                        com.Parameters.AddWithValue("@Action", action ?? string.Empty);
                        com.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LogActivity Error: " + ex.Message);
            }
        }
    }
}
