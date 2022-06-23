using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

namespace TalosWorkshop.Core
{
    public static class DB
    {
        private static MySqlConnection connection = null;

        public static bool Connected { get; private set; }

        public static void Disconnect()
        {
            if(Connected)
            connection?.Close();
        }

        public static bool Connect()
        {
            if (Connected) return true;
            try
            {
                var builder = new MySqlConnectionStringBuilder();

                builder.Database = "talos_dnd";
                builder.Server = "192.168.0.12";
                builder.UserID = "krypt0n";
                builder.Password = "dima160597";


                connection = new(builder.ConnectionString);

                connection.Open();

                Connected = connection.State == System.Data.ConnectionState.Open || connection.State == System.Data.ConnectionState.Connecting;

                return Connected;
            }
            catch
            {
                return false;
            }
        }

        public static List<KUser> LoadUsers()
        {
            try
            {
                List<KUser> results = new();

                string sql = @"SELECT
  users.id,
  users.username,
  users.personal_id,
  users.password,
  users.allowed_messages,
  users.desktop_wallpaper
FROM users";
                MySqlCommand cmd = new MySqlCommand(sql, connection);

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["id"]);
                    results.Add(new(id)
                    {
                        Username = reader["username"].ToString(),
                        PersoanlID = reader["personal_id"].ToString(),
                        Password = reader["password"].ToString(),
                        IsMessagesAllowed = Convert.ToBoolean(reader["allowed_messages"]),
                        Wallpaper = reader["desktop_wallpaper"].ToString()
                    });


                }
                reader.Close();
                return results;
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public static KUser LoadUser(int ID)
        {
            try
            {
                KUser result = null;

                string sql = @"SELECT
  users.username,
  users.personal_id,
  users.password,
  users.allowed_messages,
  users.desktop_wallpaper
FROM users WHERE users.id = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@ID", ID);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result = (new(ID)
                    {
                        Username = reader["username"].ToString(),
                        PersoanlID = reader["personal_id"].ToString(),
                        Password = reader["password"].ToString(),
                        IsMessagesAllowed = Convert.ToBoolean(reader["allowed_messages"]),
                        Wallpaper = reader["desktop_wallpaper"].ToString()
                    });


                }
                reader.Close();


                return result;
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public static bool UpdateUsers(List<KUser> users)
        {
            try
            {
                foreach (KUser user in users)
                {

                    if (user.ID >= 0)
                    {
                        string sql = @"UPDATE users SET 
  users.username = @Username,
  users.personal_id = @P_ID,
  users.password = @PASSWORD,
  users.allowed_messages = @MESSAGES_ALLOWED,
  users.desktop_wallpaper = @WALLPAPER
WHERE users.id = @ID;";

                        MySqlCommand ccmd = new MySqlCommand(sql, connection);

                        ccmd.Parameters.AddWithValue("@ID", user.ID);
                        ccmd.Parameters.AddWithValue("@Username", user.Username);
                        ccmd.Parameters.AddWithValue("@P_ID", user.PersoanlID);
                        ccmd.Parameters.AddWithValue("@PASSWORD", user.Password);
                        ccmd.Parameters.AddWithValue("@MESSAGES_ALLOWED", user.IsMessagesAllowed);
                        ccmd.Parameters.AddWithValue("@WALLPAPER", user.DBWallpaper);
                        
                        ccmd.ExecuteScalar();
                    }
                    else
                    {
                        string sql = @"INSERT INTO users(users.username,users.personal_id,users.password,users.allowed_messages,users.desktop_wallpaper)
  VALUES (@Username,@P_ID,@PASSWORD, @MESSAGES_ALLOWED,@WALLPAPER);";

                        MySqlCommand ccmd = new MySqlCommand(sql, connection);

                        //cmd.Parameters.AddWithValue("@ID", user.ID);
                        ccmd.Parameters.AddWithValue("@Username", user.Username);
                        ccmd.Parameters.AddWithValue("@P_ID", user.PersoanlID);
                        ccmd.Parameters.AddWithValue("@PASSWORD", user.Password);
                        ccmd.Parameters.AddWithValue("@MESSAGES_ALLOWED", user.IsMessagesAllowed);
                        ccmd.Parameters.AddWithValue("@WALLPAPER", user.DBWallpaper);
                        
                        ccmd.ExecuteScalar();
                    }
                }

                List<int> IDsInDB = new();

                string ssql = @"SELECT
  users.id
FROM users";
                MySqlCommand cmd = new MySqlCommand(ssql, connection);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    IDsInDB.Add(Convert.ToInt32(reader["id"]));
                }
                reader.Close();

                for (int i = 0; i < IDsInDB.Count; i++)
                {
                    bool finded = false;
                    for (int j = 0; j < users.Count; j++)
                    {
                        finded = finded || users[j].ID == IDsInDB[i];
                    }
                    if (!finded)
                    {
                        ssql = @"DELETE FROM users WHERE users.id = @ID";
                        cmd = new MySqlCommand(ssql, connection);
                        cmd.Parameters.AddWithValue("@ID", IDsInDB[i]);
                        cmd.ExecuteScalar();
                    }
                }

                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        public static List<KFile> LoadFiles()
        {
            try
            {
                List<KFile> results = new();

                string sql = @"SELECT
  files.file_id,
  files.user_id,
  files.icon,
  files.title,
  files.content
FROM files";
                MySqlCommand cmd = new MySqlCommand(sql, connection);

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["file_id"]);
                    results.Add(new(id)
                    {
                        Owner = LoadUser(Convert.ToInt32(reader["user_id"])),
                        Icon = reader["icon"].ToString(),
                        Title = reader["title"].ToString(),
                        Content = reader["content"].ToString()
                    });


                }
                reader.Close();
                return results;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public static List<KFile> LoadFiles(int user_id)
        {
            try
            {
                List<KFile> results = new();

                string sql = @"SELECT
  files.file_id,
  files.icon,
  files.title,
  files.content
FROM files WHERE files.user_id = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@ID", user_id);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["file_id"]);
                    results.Add(new(id)
                    {
                        Owner = LoadUser(Convert.ToInt32(reader["user_id"])),
                        Icon = reader["icon"].ToString(),
                        Title = reader["title"].ToString(),
                        Content = reader["content"].ToString()
                    });


                }
                reader.Close();
                return results;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public static List<KFile> LoadFiles(KUser user)
        {
            try
            {
                List<KFile> results = new();

                string sql = @"SELECT
  files.file_id,
  files.icon,
  files.title,
  files.content
FROM files WHERE files.user_id = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@ID", user.ID);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["file_id"]);
                    results.Add(new(id)
                    {
                        Owner = user,
                        Icon = reader["icon"].ToString(),
                        Title = reader["title"].ToString(),
                        Content = reader["content"].ToString()
                    });


                }
                reader.Close();
                return results;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
