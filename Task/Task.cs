using System;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using System.IO;
using Task.Event;

namespace Task{
    public class Task : IComparable<Task>{

        #region Configurations
        public static string Database = string.Format("{0}database.db", AppDomain.CurrentDomain.BaseDirectory);
        public const string Table = "TASKS";
        public const string TAG = "t"; 
        #endregion

        #region InternalClasses
        class Row<T> : IRow<T> {
            public int id { get; set; }
            public Row(string _id) {
                this.id = Convert.ToInt16(_id.Substring(1));
            }
            public T this[string _property] {
                get {
                    using (Connection connection = new Connection(Database)) {
                        var c = connection.CreateCommand();
                        c.CommandText = string.Format(@"SELECT * from {0} WHERE id='{1}'", Table, this.id);
                        var r = c.ExecuteReader();
                        return (r[_property] == DBNull.Value) ? default(T) : (T)Convert.ChangeType(r[_property], typeof(T));
                    }
                }
                set {
                    using (Connection connection = new Connection(Database)) {
                        var c = connection.CreateCommand();
                        c.CommandText = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE id={3}", Table, _property, value, this.id);
                        c.ExecuteNonQuery();
                    }
                }
            }
            public void Drop() {
                using (Connection connection = new Connection(Database)) {
                    var c = connection.CreateCommand();
                    c.CommandText = string.Format(@"DELETE FROM {0} WHERE id={1}", Table, this.id);
                    c.ExecuteNonQuery();
                }
            }
        }
        public class EventList : List<Event.Event> {
            string taskid;
            string eventString {
                get {
                    Row<string> temp = new Row<string>(this.taskid);
                    if (string.IsNullOrEmpty(temp["events"])) {
                        return string.Empty;
                    }
                    return temp["events"];
                }
                set {
                    Row<string> temp = new Row<string>(this.taskid);
                    temp["events"] = value;
                }
            }
            public EventList(string _id) {
                this.taskid = _id;
                if (!string.IsNullOrEmpty(this.eventString)) {
                    foreach (var item in this.eventString.Split(':')) {
                        if (!string.IsNullOrEmpty(item)) {
                            this.Add(Event.Event.Get(item));
                        }
                    }
                }
            }
            public void Add(ref Event.Event _event) {
                if (!this.eventString.Contains(_event.id)) {
                    this.eventString = string.Format("{0}:{1}", this.eventString, _event.id);
                    base.Add(_event);
                }

            }
            public void Remove(string _id) {
                if (this.eventString.Contains(_id)) {
                    String toRemove = string.Format(":{0}", _id);
                    this.eventString = this.eventString.Replace(toRemove, String.Empty);
                    var item = this.Find(i => (i.id == _id));
                    item.SelfDestroy();
                    int index = this.FindIndex(i => (i.id == _id));
                    base.RemoveAt(index);
                }
            }
            public new void RemoveAt(int index) {
                this.Remove(this[index].id);
            }
            public void Destroy() {
                foreach (var item in this) {
                    item.SelfDestroy();
                }
            }

        }

        #endregion

        #region Constructers/Destructers
        public Task() {
            if (string.IsNullOrEmpty(this.id)) {
                this.id = TaskTableId();
            }
            this.Eventlist = new EventList(this.id);
        }
        public Task(string _id) {
            this.id = _id;
            this.Eventlist = new EventList(this.id);
        }
        public void SelfDestroy() {
            Row<string> temp = new Row<string>(this.id);
            temp.Drop();
            this.Eventlist.Destroy();

        }
        #endregion

        #region Holders
        [Browsable(false)]
        public string id {
            get;
            private set;
        }
        [DisplayName("Name")]
        public string name {
            get {
                Row<string> temp = new Row<string>(this.id);
                return temp["name"];
            }
            set {
                Row<string> temp = new Row<string>(this.id);
                temp["name"] = value;
            }
        }
        [DisplayName("Repeat")]
        public int repeat {
            get {
                Row<int> temp = new Row<int>(this.id);
                return Convert.ToInt16(temp["repeat"]);
            }
            set {
                Row<int> temp = new Row<int>(this.id);
                temp["repeat"] = value;
            }
        }
        [DisplayName("Start At")]
        public TimeSpan start {
            get {
                Row<int> temp = new Row<int>(this.id);
                return TimeSpan.FromSeconds(Convert.ToDouble(temp["start"]));
            }
            set {
                Row<int> temp = new Row<int>(this.id);
                temp["start"] = (int)value.TotalSeconds;
            }
        }
        [DisplayName("End At")]
        public TimeSpan end {
            get {
                Row<int> temp = new Row<int>(this.id);
                return TimeSpan.FromSeconds(Convert.ToDouble(temp["end"]));
            }
            set {
                Row<int> temp = new Row<int>(this.id);
                temp["end"] = (int)value.TotalSeconds;
            }
        }
        [DisplayName("Interval")]
        public TimeSpan interval {
            get {
                Row<int> temp = new Row<int>(this.id);
                return TimeSpan.FromSeconds(Convert.ToDouble(temp["interval"]));
            }
            set {
                Row<int> temp = new Row<int>(this.id);
                temp["interval"] = (int)value.TotalSeconds;
            }
        }
        [DisplayName("Remainnig")]
        public TimeSpan remainnig {
            get {
                Row<int> temp = new Row<int>(this.id);
                var rem = TimeSpan.FromSeconds(Convert.ToDouble(temp["remainnig"]));
                if(((int)rem.TotalSeconds == 0) && (this.repeat > 0)){
                    this.repeat = this.repeat - 1;
                    this.remainnig = this.interval;
                }
                return TimeSpan.FromSeconds(Convert.ToDouble(temp["remainnig"]));
            }
            set {
                Row<int> temp = new Row<int>(this.id);
                temp["remainnig"] = (int)value.TotalSeconds;
            }
        }
        [Browsable(false)]
        public bool DependOnPreviousEvent {
            get {
                Row<int> temp = new Row<int>(this.id);
                return (temp["DOPE"] == 1);
            }
            set {
                Row<int> temp = new Row<int>(this.id);
                temp["DOPE"] = (value) ? 1 : 0;
            }
        }
        [Browsable(false)]
        public bool TaskDependOnEvent {
            get {
                Row<int> temp = new Row<int>(this.id);
                return (temp["TDOE"] == 1);
            }
            set {
                Row<int> temp = new Row<int>(this.id);
                temp["TDOE"] = (value) ? 1 : 0;
            }
        }
        [DisplayName("Suspended")]
        public bool Suspended {
            get {
                Row<int> temp = new Row<int>(this.id);
                return (temp["Suspended"] == 1);
            }
            set {
                Row<int> temp = new Row<int>(this.id);
                temp["Suspended"] = (value) ? 1 : 0;
            }
        }
        [Browsable(false)]
        string eventString {
            get {
                Row<string> temp = new Row<string>(this.id);
                return temp["events"];
            }
            set {
                Row<string> temp = new Row<string>(this.id);
                temp["events"] = value;
            }
        }
        public EventList Eventlist;
        #endregion

        #region Action
        const int SUCCUSS = 0;
        const int TDOE_FAILED = -1;
        const int DOPE_FAILED = -2;

        public int Action() {
            System.Console.WriteLine("fuck!");
            foreach (var item in this.Eventlist) {
                int result = -1;
                switch (item.type) {
                    case 'c':
                        result = Copy.Start(item);
                        break;
                    case 'f':
                        result = Format.Format_CommandLine_Process(item);
                        break;
                    default:
                        break;
                }
                if (result != 0) {
                    if (this.TaskDependOnEvent) {
                        this.Suspended = true;
                        return TDOE_FAILED;
                    }
                    if (this.DependOnPreviousEvent) {
                        this.Suspended = true;
                        return DOPE_FAILED;
                    }
                }
            }
            return SUCCUSS;
        } 
        #endregion

        #region IComparable
        public int CompareTo(Task other) {
            return this.remainnig.CompareTo(other.remainnig);
        } 
        #endregion

        #region Static
        static string TaskTableId() {
            using (Connection connection = new Connection(Database)) {
                var command = connection.CreateCommand();
                command.CommandText = string.Format(@"INSERT INTO [{0}] DEFAULT VALUES", Table);
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
                command.CommandText = @"CREATE Table IF NOT EXISTS [TASKS](" +
                    "[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                    "[name] VARCHAR(128) NULL," +
                    "[repeat] INTEGER NULL," +
                    "[start] INTEGER NULL," +
                    "[end] INTEGER NULL," +
                    "[interval] INTEGER NULL," +
                    "[remainnig] INTEGER NULL," +
                    "[DOPE] INTEGER NULL," +
                    "[TDOE] INTEGER NULL," +
                    "[Suspended] INTEGER NULL," +
                    "[events] VARCHAR(1024) NULL" +
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
        public static void ReadTasks<T>(ref T _list, bool _loadall = false) {
            (_list as List<Task>).Clear();
            if (!File.Exists(Database)) {
                Task.InitializeTable();
                return;
            }
            SQLiteConnection connection = new SQLiteConnection(string.Format(@"Data Source={0}", Database));
            if (connection.State == System.Data.ConnectionState.Closed) {
                connection.Open();
            }
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = string.Format("SELECT * FROM {0}", Table);
            using (SQLiteDataReader reader = command.ExecuteReader()) {
                while (reader.Read()) {
                    string id = Convert.ToString(reader["id"]);
                    if (!String.IsNullOrEmpty(id)) {
                        (_list as List<Task>).Add(new Task(string.Format("t{0}", id)));
                    }
                }
            }

        }
        #endregion
    }
}
