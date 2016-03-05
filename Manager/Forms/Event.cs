using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Manager {
    public partial class EventForm : Form {
        public bool success { get; set; }
        public Task.Event.Event _event { get; set; }
        public Mode mode { get; set; }

        public EventForm(Task.Event.Event _event) {
            if (_event == null) {
                this.mode = Mode.Add;
            }
            else {
                this.mode = Mode.Edit;
                this._event = _event;
            }
            InitializeComponent();
            InitializeEvent();
            this.comboBox4.DataSource = Enum.GetValues(typeof(Task.Event.Ftype)).Cast<Task.Event.Ftype>().Select(t => new {
                Value = t,
                Text = t.ToString().Replace("_", " ")
            }).ToList();
            this.comboBox2.DataSource = Enum.GetValues(typeof(Task.Event.FileSystem));
        }
        void InitializeEventCopy() {
            this.textBox2.Text = this._event.name;
            var eventcopy = this._event as Task.Event.EventCopy;
            this.label8.Text = eventcopy.source;
            this.label9.Text = eventcopy.destination;
            this.checkBox4.Checked = eventcopy.overwrite;
        }
        void InitializeEventFormat() {
            this.textBox2.Text = this._event.name;
            var eventformat = this._event as Task.Event.EventFormat;
            this.comboBox1.SelectedIndex = eventformat.driveLetter[0] - 65;
            this.comboBox2.DataSource = Enum.GetValues(typeof(Task.Event.FileSystem));
            this.comboBox2.SelectedItem = eventformat.fileSystem;
            this.checkBox1.Checked = eventformat.quick;
        }
        void InitializeEvent() {
            if(this.mode == Mode.Add){
                this.Text = "New Event";
                this.button1.Text = "Add";
            }
            else if (this.mode == Mode.Edit) {
                switch (_event.type) {
                    case 'c':
                        this.radioButton1.Checked = true;
                        this.radioButton2.Enabled = false;
                        this.groupBox2.Visible = true;
                        this.groupBox2.Enabled = true;
                        this.groupBox4.Visible = false;
                        this.InitializeEventCopy();
                        
                        break;
                    case 'f':
                        this.radioButton2.Checked = true;
                        this.radioButton1.Enabled = false;
                        this.groupBox4.Visible = true;
                        this.groupBox4.Enabled = true;
                        this.groupBox2.Visible = false;
                        InitializeEventFormat();
                        break;
                }
                this.button1.Text = "Save";
                this.Text = string.Format("Edit Event : {0}", this._event.name);
            }
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e) {
            this.pictureBox2.Visible = true;
            this.pictureBox1.Visible = false;
            if(this.radioButton1.Checked && (this.mode == Mode.Add)){
                this._event = new Task.Event.EventCopy();
                this._event.name = "NewCopyEvent";
                this._event.type = 'c';
                this.InitializeEvent(); 
                this.groupBox2.Visible = true;
                this.groupBox2.Enabled = true;
                this.groupBox4.Visible = false;
            }
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e) {
            this.pictureBox1.Visible = true;
            this.pictureBox2.Visible = false;
            if (this.radioButton2.Checked && (this.mode == Mode.Add)) {
                this._event = new Task.Event.EventFormat();
                this._event.name = "NewFormatEvent";
                this._event.type = 'f';
                this.InitializeEvent();
                this.groupBox4.Visible = true;
                this.groupBox4.Enabled = true;
                this.groupBox2.Visible = false;
            }
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e) {
            if (this.checkBox2.Checked == true) {
                this.groupBox5.Enabled = true;
                return;
            }
            this.groupBox5.Enabled = false;
        }
        private void button1_Click(object sender, EventArgs e) {
            if (this.mode == Mode.Add) {
                switch (this._event.type) {
                    case 'c':
                        var tempec = new Task.Event.EventCopy();
                        tempec.name = this.textBox2.Text;
                        tempec.source = this.label8.Text;
                        tempec.destination = this.label9.Text;
                        tempec.overwrite = this.checkBox1.Checked;
                        var result = tempec.NullCheck();
                        if(!string.IsNullOrEmpty(result)){
                            MessageBox.Show(result, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        this._event = tempec as Task.Event.Event;
                        break;
                    case 'f':
                        var tempef = new Task.Event.EventFormat();

                        if (this.comboBox1.SelectedItem == null) {
                            MessageBox.Show("Please select Drive!", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        tempef.driveLetter = this.comboBox1.SelectedItem.ToString();
                        tempef.fileSystem = (Task.Event.FileSystem)this.comboBox2.SelectedItem;
                        tempef.quick = this.checkBox1.Checked;
                        this._event = tempef as Task.Event.Event;
                        break;
                }
            }
            else if (this.mode == Mode.Edit) {
                switch (this._event.type) {
                    case 'c':
                        var tempec = this._event as Task.Event.EventCopy;
                        tempec.name = this.textBox2.Text;
                        tempec.source = this.label8.Text;
                        tempec.destination = this.label9.Text;
                        tempec.overwrite = this.checkBox1.Checked;

                        var result = tempec.NullCheck();
                        if(!string.IsNullOrEmpty(result)){
                            MessageBox.Show(result, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        break;
                    case 'f':
                        var tempef = this._event as Task.Event.EventFormat;
                        if (this.comboBox1.SelectedItem == null) {
                            MessageBox.Show("Please select Drive!", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        tempef.driveLetter = this.comboBox1.SelectedItem.ToString();
                        tempef.fileSystem = (Task.Event.FileSystem)this.comboBox2.SelectedItem;
                        tempef.quick = this.checkBox1.Checked;
                        break;
                }
            }
            this.success = true;
            this.Close();
        }
        private void button2_Click(object sender, EventArgs e) {
            if((this.mode == Mode.Add)&&(this._event != null)){
                this._event.SelfDestroy();
            }
            this.Close();
        }
        private void button3_Click(object sender, EventArgs e) {
            using (this.folderBrowserDialog1) {
                if(this.folderBrowserDialog1.ShowDialog() == DialogResult.OK){
                    this.label8.Text = this.folderBrowserDialog1.SelectedPath;
                }
            }
        }
        private void button4_Click(object sender, EventArgs e) {
            using (this.folderBrowserDialog2) {
                if (this.folderBrowserDialog2.ShowDialog() == DialogResult.OK) {
                    this.label9.Text = this.folderBrowserDialog2.SelectedPath;
                }
            }
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) {
            if ((Task.Event.FileSystem)this.comboBox2.SelectedItem == Task.Event.FileSystem.NTFS) {
                this.checkBox3.Enabled = true;
                return;
            }
            this.checkBox3.Enabled = false;
        }
    }
}
