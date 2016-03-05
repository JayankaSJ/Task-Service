using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Manager {
    public enum Mode {
        Add,
        Edit
    }
   public partial class TaskForm : Form {

        public bool success { get; set; }
        public Task.Task task { get; set; }
        public Mode mode { get; set; }
        public Task.Event.Event SelectedEvent {
            get {
                if (this.dataGridView1.SelectedRows.Count == 1) {
                    var index = this.dataGridView1.SelectedRows[0].Index;
                    if (this.task.Eventlist.Count == 0) {
                        MessageBox.Show("No Events", null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    return this.task.Eventlist[index];
                }
                else {
                    MessageBox.Show("Please Select an Item", string.Format("Delete : {0}", "null"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                return null;
            }
        }
        public TaskForm(Task.Task _task = null) {
            if (_task == null) {
                this.mode = Mode.Add;
                this.task = new Task.Task();
            }
            else {
                this.mode = Mode.Edit;
                this.task = _task;
            }
            InitializeComponent();
            this.Experimental();
            InitializeTask();
        }

        void Experimental() {
            this.dateTimePicker1.Enabled = false;
            this.dateTimePicker2.Enabled = false;
        }

        void InitializeTask() {
            if (this.mode == Mode.Add) {
                this.Text = "Add Task";
                this.button1.Text = "Add";
                this.textBox1.Text = "new task";
                this.numericUpDown1.Value = 0;
                this.dateTimePicker3.Value = DateTime.FromOADate(0.0);
            }
            else if(this.mode == Mode.Edit){
                this.Text = string.Format("Edit Task : {0}", this.task.name);
                this.textBox1.Text = this.task.name;
                this.numericUpDown1.Value = this.task.repeat;
                this.dateTimePicker3.Value = DateTime.FromOADate(0.0);
                this.dateTimePicker3.Value = this.dateTimePicker3.Value.Add(this.task.interval);
                this.checkBox2.Checked = this.task.TaskDependOnEvent;
                this.checkBox3.Checked = this.task.DependOnPreviousEvent;
                this.button1.Text = "Save";
            }
            this.RefreshComponent();
            this.success = false;
        }
        void RefreshComponent() {
            this.dataGridView1.DataSource = null;
            this.dataGridView1.DataSource = task.Eventlist;
            if (task.Eventlist.Count == 0) {
                this.button4.Enabled = false;
                this.button5.Enabled = false;
                return;
            }
            this.button4.Enabled = true;
            this.button5.Enabled = true;
        }
        private void button1_Click(object sender, EventArgs e) {
            if (string.IsNullOrEmpty(this.textBox1.Text)) {
                MessageBox.Show("Task Name Required!", null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            this.task.name = this.textBox1.Text;
            this.task.repeat = (int)this.numericUpDown1.Value;
            //for start time
            //for end time
            var newinterval = new TimeSpan(this.dateTimePicker3.Value.Hour,
                                                this.dateTimePicker3.Value.Minute,
                                                this.dateTimePicker3.Value.Second);
            if (!newinterval.Equals(this.task.interval)) {
                this.task.interval = newinterval;
                this.task.remainnig = new TimeSpan(0,0,0);
            }
            if ((int)this.task.interval.TotalSeconds == 0) {
                MessageBox.Show("00:00:00 is not proper value for Interval!", null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            this.task.TaskDependOnEvent = this.checkBox2.Checked;
            this.task.DependOnPreviousEvent = this.checkBox3.Checked;
            if (this.task.Suspended) {
                this.task.Suspended = false;
            }
            this.success = true;
            this.Close();
            
        }
        private void button2_Click(object sender, EventArgs e) {
            if (this.mode == Mode.Add) {
                task.SelfDestroy();
            }
            this.Close();
        }

        #region EventGroupBox
        private void button3_Click(object sender, EventArgs e) {
            Task.Event.Event temp = null;
            using (EventForm eventform = new EventForm(temp)) {
                eventform.ShowDialog();
                if (eventform.success) {
                    temp = eventform._event;
                    task.Eventlist.Add(ref temp);
                    this.RefreshComponent();
                }
            }

        }
        private void button4_Click(object sender, EventArgs e) {
            using (EventForm eventform = new EventForm(this.SelectedEvent)) {
                eventform.ShowDialog();
                if (eventform.success) {
                    this.RefreshComponent();
                }
            }
        }
        private void button5_Click(object sender, EventArgs e) {
            foreach (DataGridViewRow _item in this.dataGridView1.SelectedRows) {
                var index = this.dataGridView1.SelectedRows[0].Index;
                var result = MessageBox.Show("This operation cannot be undone!\r\nDo you want to continue?", string.Format("Delete : {0}", "null"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (System.Windows.Forms.DialogResult.OK == result) {
                    this.task.Eventlist.RemoveAt(index);
                    this.RefreshComponent();
                }
            }
        } 
        #endregion

        private void checkBox2_CheckedChanged(object sender, EventArgs e) {
            if (this.checkBox2.Checked == true) {
                this.checkBox3.Enabled = false;
                return;
            }
            this.checkBox3.Enabled = true;
        }
    }
}
