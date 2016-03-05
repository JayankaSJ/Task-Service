using System;
using System.IO;
using System.Data.SQLite;
using System.ComponentModel;

namespace Task.Event {

    public abstract class Event {

        public static string Database = string.Format("{0}database.db", AppDomain.CurrentDomain.BaseDirectory);

        [Browsable(false)]
        public string id { get; set; }
        [DisplayName("Name")]
        public virtual string name { get; set; }
        [DisplayName("Type")]
        public char type { get; set; }
        internal Event() {

        }
        public abstract void SelfDestroy();
        public abstract string NullCheck();

        public static Event Get(string id) {
            Event temp = null;
            switch (id[1]) {
                case 'c':
                    temp = new EventCopy(id);
                    break;
                case 'f':
                    temp = new EventFormat(id);
                    break;
            }
            return temp;
        }
    }

    public class EventCopy : Event {
        const string TABLE = "EventCopy";
        const string TAG = "ec";

        internal class Row<T> : IRow<T> {

            public int id {
                get;
                set;
            }
            public Row(string _id) {
                try {
                    this.id = Convert.ToInt16(_id.Substring(2));
                }
                catch (Exception e) {
                    throw e;
                }
            }
            public T this[string _property] {
                get {
                    using (Connection connection = new Connection(Database)) {
                        var c = connection.CreateCommand();
                        c.CommandText = string.Format(@"SELECT * from {0} WHERE id='{1}'", TABLE, this.id);
                        var r = c.ExecuteReader();
                        return (r[_property] == DBNull.Value) ? default(T) : (T)Convert.ChangeType(r[_property], typeof(T));
                    }
                }
                set {
                    using (Connection connection = new Connection(Database)) {
                        var c = connection.CreateCommand();
                        c.CommandText = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE id={3}", TABLE, _property, value, this.id);
                        c.ExecuteNonQuery();
                    }
                }
            }
            public void Drop() {
                try {
                    using (Connection connection = new Connection(Database)) {
                        var c = connection.CreateCommand();
                        c.CommandText = string.Format(@"DELETE FROM {0} WHERE id={1}", TABLE, this.id);
                        c.ExecuteNonQuery();
                    }
                }
                catch (Exception e) {
                    throw e;
                }
            }
        }

        #region Holders
        public override string name {
            get {
                Row<string> temp = new Row<string>(this.id);
                return temp["name"];
            }
            set {
                Row<string> temp = new Row<string>(this.id);
                temp["name"] = value;
            }
        }
        public string source {
            get {
                Row<string> temp = new Row<string>(this.id);
                return temp["source"];
            }
            set {
                Row<string> temp = new Row<string>(this.id);
                temp["source"] = value;
            }
        }
        public string destination {
            get {
                Row<string> temp = new Row<string>(this.id);
                return temp["destination"];
            }
            set {
                Row<string> temp = new Row<string>(this.id);
                temp["destination"] = value;
            }
        }
        public bool overwrite {
            get {
                Row<string> temp = new Row<string>(this.id);
                return (temp["overwrite"] == "1");
            }
            set {
                Row<string> temp = new Row<string>(this.id);
                temp["overwrite"] = (value ? "1" : "0");
            }
        }

        
        #endregion
        public EventCopy()
            : base() {
            this.type = 'c';
            this.id = EventCopy.EventTableId();
        }
        public EventCopy(String _id)
            : base() {
            this.id = _id;
            this.type = 'c';
        }
        public override void SelfDestroy() {
            Row<string> temp = new Row<string>(this.id);
            temp.Drop();

        }
        static string EventTableId() {
            using (Connection connection = new Connection(Database)) {
                var command = connection.CreateCommand();
                command.CommandText = string.Format(@"INSERT INTO [{0}] DEFAULT VALUES", TABLE);
                command.ExecuteNonQuery();
                command.CommandText = @"SELECT last_insert_rowid()";
                return String.Format("{0}{1}", TAG, Convert.ToString(command.ExecuteScalar()));
            }
        }
        public static void InitializeTable() {
            SQLiteConnection connection = null;
            SQLiteCommand command;
            try {
                if (!File.Exists(Database)) {
                    SQLiteConnection.CreateFile(Database);
                }

                connection = new SQLiteConnection(@"Data Source=" + Database);
                if (connection.State == System.Data.ConnectionState.Closed) {
                    connection.Open();
                }
                command = connection.CreateCommand();
                command.CommandText = String.Format(@"CREATE TABLE IF NOT EXISTS [{0}](", TABLE) +
                    "[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                    "[name] VARCHAR(64) NULL," +
                    "[source] VARCHAR(128) NULL," +
                    "[destination] VARCHAR(128) NULL," +
                    "[overwrite] VARCHAR(128) NULL" +
                    ")";
                command.ExecuteNonQuery();
            }
            catch (Exception e) {
                throw e;
            }
            finally {
                connection.Close();
            }

        }
        public override string NullCheck() {
            string error = string.Empty;
            if (string.IsNullOrWhiteSpace(this.name)) {
                error += string.Format("\r\nName cann't leave as blank!");
            }
            if (string.IsNullOrWhiteSpace(this.source)) {
                error += string.Format("\r\nSource path not valid!");
            }
            if (string.IsNullOrWhiteSpace(this.destination)) {
                error += string.Format("\r\ndestination path not valid!");
            }
            return error;
        }

    }

    public enum FileSystem {
        [Description("NTFS")]
        NTFS = 1,
        [Description("FAT")]
        FAT = 2,
        [Description("FAT32")]
        FAT32 = 3,
        [Description("exFAT")]
        exFAT = 4
    }
    public enum Ftype {
        Management_Object = 'm',
        CommandLine_Process = 'c',
    }
    public enum Allocation {

    }
    public class EventFormat : Event {

        const string TABLE = "EventFormat";
        const string TAG = "ef";

        internal class Row<T> : IRow<T> {

            public int id {
                get;
                set;
            }
            public Row(string _id) {
                try {
                    this.id = Convert.ToInt16(_id.Substring(2));
                }
                catch (Exception e) {
                    throw e;
                }
            }
            public T this[string _property] {
                get {
                    using (Connection connection = new Connection(Database)) {
                        var c = connection.CreateCommand();
                        c.CommandText = string.Format(@"SELECT * from {0} WHERE id='{1}'", TABLE, this.id);
                        var r = c.ExecuteReader();
                        return (r[_property] == DBNull.Value) ? default(T) : (T)Convert.ChangeType(r[_property], typeof(T));
                    }
                }
                set {
                    using (Connection connection = new Connection(Database)) {
                        var c = connection.CreateCommand();
                        c.CommandText = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE id={3}", TABLE, _property, value, this.id);
                        c.ExecuteNonQuery();
                    }
                }
            }
            public void Drop() {
                try {
                    using (Connection connection = new Connection(Database)) {
                        var c = connection.CreateCommand();
                        c.CommandText = string.Format(@"DELETE FROM {0} WHERE id={1}", TABLE, this.id);
                        c.ExecuteNonQuery();
                    }
                }
                catch (Exception e) {
                    throw e;
                }
            }
        }

        #region Holders
        public override string name {
            get {
                Row<string> temp = new Row<string>(this.id);
                return temp["name"];
            }
            set {
                Row<string> temp = new Row<string>(this.id);
                temp["name"] = value;
            }
        }
        public string driveLetter {
            get {
                Row<string> temp = new Row<string>(this.id);
                return temp["driveLetter"];
            }
            set {
                Row<string> temp = new Row<string>(this.id);
                temp["driveLetter"] = value;
            }
        }
        public FileSystem fileSystem {
            get {
                Row<string> temp = new Row<string>(this.id);
                string iw = temp["fileSystem"];
                return (FileSystem)Enum.Parse(typeof(FileSystem), temp["fileSystem"]);
            }
            set {
                Row<string> temp = new Row<string>(this.id);
                string iwa = string.Format("{0}", value.ToString());
                temp["fileSystem"] = value.ToString();
            }
        }
        public bool quick {
            get {
                Row<string> temp = new Row<string>(this.id);
                return (temp["quick"] == "1");
            }
            set {
                Row<string> temp = new Row<string>(this.id);
                temp["quick"] = value ? "1" : "0";
            }
        }


        public bool AdvancedEnabled {
            get {
                Row<string> temp = new Row<string>(this.id);
                return (temp["AdvancedEnabled"] == "1");
            }
            set {
                Row<string> temp = new Row<string>(this.id);
                temp["AdvancedEnabled"] = value ? "1" : "0";
            }
        }
        public Ftype ftype {
            get {
                if (this.AdvancedEnabled) {
                    Row<string> temp = new Row<string>(this.id);
                    return (Ftype)Enum.Parse(typeof(Ftype), temp["ftype"]);
                }
                return Ftype.CommandLine_Process;
            }
            set {
                Row<string> temp = new Row<string>(this.id);
                temp["ftype"] = value.ToString();
            }
        }
        public bool compression {
            get {
                if (this.AdvancedEnabled && this.fileSystem == FileSystem.NTFS) {
                    Row<string> temp = new Row<string>(this.id);
                    return (temp["compression"] == "1");
                }
                return false;
            }
            set {
                Row<string> temp = new Row<string>(this.id);
                temp["compression"] = value ? "1" : "0";
            }
        }
        public int allocation {
            get {
                Row<string> temp = new Row<string>(this.id);
                return Convert.ToInt16(temp["allocation"]);
            }
            set {
                Row<string> temp = new Row<string>(this.id);
                temp["allocation"] = Convert.ToString(value);
            }
        }
        #endregion
        public EventFormat()
            : base() {
            this.type = 'f';
            this.id = EventTableId();
        }
        public EventFormat(String _id)
            : base() {
            this.id = _id;
            this.type = 'f';
        }
        public override void SelfDestroy() {
            Row<string> temp = new Row<string>(this.id);
            temp.Drop();

        }
        public override string NullCheck() {
            return string.Empty;
        }
        static string EventTableId() {
            using (Connection connection = new Connection(Database)) {
                var command = connection.CreateCommand();
                command.CommandText = string.Format(@"INSERT INTO [{0}] DEFAULT VALUES", TABLE);
                command.ExecuteNonQuery();
                command.CommandText = @"SELECT last_insert_rowid()";
                return String.Format("{0}{1}", TAG, Convert.ToString(command.ExecuteScalar()));
            }
        }
        public static void InitializeTable() {
            SQLiteConnection connection = null;
            SQLiteCommand command;
            try {
                if (!File.Exists(Database)) {
                    SQLiteConnection.CreateFile(Database);
                }

                connection = new SQLiteConnection(@"Data Source=" + Database);
                if (connection.State == System.Data.ConnectionState.Closed) {
                    connection.Open();
                }
                command = connection.CreateCommand();
                command.CommandText = String.Format(@"CREATE TABLE IF NOT EXISTS [{0}](", TABLE) +
                    "[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                    "[name] VARCHAR(128) NULL," +
                    "[driveLetter] VARCHAR(128) NULL," +
                    "[fileSystem] VARCHAR(128) NULL," +
                    "[label] VARCHAR(128) NULL," +
                    "[allocation] VARCHAR(128) NULL," +
                    "[quick] VARCHAR(128) NULL," +
                    "[compression] VARCHAR(128) NULL," +
                    "[AdvancedEnabled] VARCHAR(128) NULL, " +
                    "[ftype] VARCHAR(128) NULL" +
                    ")";
                command.ExecuteNonQuery();
            }
            catch (Exception e) {
                throw e;
            }
            finally {
                connection.Close();
            }

        }
    }
}
