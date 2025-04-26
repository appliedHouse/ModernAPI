using System.Data.SQLite;
using System.Runtime.CompilerServices;

namespace ModernAPI.Data
{
    public class Connections
    {
        public SQLiteConnection MyConnection { get; set; }


        public Connections()
        {
            MyConnection = new();
        }

        public Connections(string _File)
        {
            MyConnection = new();
            GetConnection(_File);           // established MyConnecton by valid DB file.
        }


        public void GetConnection(string _File)
        {

            string _Path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "DB");          // Assign a folder path for DB.
            if(!Directory.Exists(_Path)) { Directory.CreateDirectory(_Path); }                       // Create a Folder if not exist.


            string DBPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "DB", _File);

            if (!File.Exists(DBPath))
            {
                SQLiteConnection.CreateFile(DBPath);
                CreateDataTables();
            }


            if (File.Exists(DBPath))
            {
                MyConnection = new SQLiteConnection($"Data Source={DBPath};Version=3;");

                if (MyConnection.State == System.Data.ConnectionState.Closed)
                {
                    MyConnection.Open();
                }
            }
        }

        private void CreateDataTables()
        {
            // Create Data Tables as need by application
        }
    }
}
