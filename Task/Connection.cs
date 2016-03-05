using System;
using System.Data.SQLite;

namespace Task{
    class Connection : IDisposable {
        static SQLiteConnection connection = null;
        public Connection(string _Database) {
            try {
                if (connection == null) {
                    connection = new System.Data.SQLite.SQLiteConnection("Data Source=" + _Database);
                }
            }
            catch (Exception e) {
                throw e;
            }
        }
        public SQLiteCommand CreateCommand() {
            if (connection.State == System.Data.ConnectionState.Closed) {
                connection.Open();
            }
            return connection.CreateCommand();
        }
        public void Dispose() {
            connection.Close();
            connection = null;
        }
    }

}
