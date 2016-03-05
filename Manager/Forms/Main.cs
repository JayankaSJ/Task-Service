using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

using System.Diagnostics;

namespace Manager {
    public partial class MainForm : Form {
        Engine engine;
        public Task.Task SelectedTask {
            get {
                if (this.dataGridView1.SelectedRows.Count == 1) {
                    var index = this.dataGridView1.SelectedRows[0].Index;
                    return Engine.Tasks[index];
                }
                return null;
            }
        }
        private bool SetServiceState {
            set {
                if(value){
                    Service.StartService(ServiceServiceName);
                    this.CheckServiceStatus();
                }
                else {
                    Service.StopService(ServiceServiceName);
                    this.CheckServiceStatus();
                }
            }
        }
        public MainForm() {
            InitializeComponent();
            InitializeEngineComponent();
            InitializeAdminComponent();
            InitializeServiceComponent();
        }
        void InitializeEngineComponent() {
            this.engine = new Engine();
            this.RefreshComponent();
        }
        void RefreshComponent() {
            this.dataGridView1.DataSource = null;
            this.dataGridView1.DataSource = Engine.Tasks;
            if (Engine.Tasks.Count > 0) {
                this.button2.Enabled = true;
                this.button3.Enabled = true;
                //this.timer1.Start();
                return;
            }
            //this.timer1.Stop();
            this.toolStripProgressBar1.Value = 0;
            this.button2.Enabled = false;
            this.button3.Enabled = false;
        }
        
        #region Buttons
        private void button1_Click(object sender, EventArgs e) {
            Task.Task temp = null;
            this.SetServiceState = false;
            using (TaskForm taskform = new TaskForm(temp)) {
                taskform.ShowDialog();
                temp = taskform.task;
                if (taskform.success) {
                    Engine.Tasks.Add(temp);
                    this.RefreshComponent();
                }
                else {
                    if (temp != null) {
                        temp.SelfDestroy();
                    }
                }
            }
            this.SetServiceState = true;
        }
        private void button2_Click(object sender, EventArgs e) {
            this.SetServiceState = false;
            using (TaskForm taskform = new TaskForm(this.SelectedTask)) {
                taskform.ShowDialog();
                if (taskform.success) {
                    //this.engine.Restart();
                    this.RefreshComponent();
                }
            }
            this.SetServiceState = true;
        }
        private void button3_Click(object sender, EventArgs e) {
            this.SetServiceState = false;
            foreach (DataGridViewRow _item in this.dataGridView1.SelectedRows) {
                var index = this.dataGridView1.SelectedRows[0].Index;
                var result = MessageBox.Show("This operation cannot be undone!\r\nDo you want to continue?", string.Format("Delete : {0}", Engine.Tasks[index].name), MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (System.Windows.Forms.DialogResult.OK == result) {
                    Engine.Tasks.RemoveAt(index);
                    this.RefreshComponent();
                }
            }
            this.SetServiceState = true;
        }
        private void button4_Click(object sender, EventArgs e) {
            this.Close();
        } 
        #endregion

        #region ToolStripMenu
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }
        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e) {
            using (AboutBox ab = new AboutBox()) {
                ab.ShowDialog(this);
            }
        } 
        #endregion

        #region Timer
        private void timer1_Tick(object sender, EventArgs e) {
            if (this.SelectedTask != null) {
                this.toolStripProgressBar1.Maximum = (int)this.SelectedTask.interval.TotalSeconds;
                var re = (int)this.SelectedTask.remainnig.TotalSeconds;
                if (re < 0) {
                    return;
                }
                this.toolStripProgressBar1.Value = (int)this.SelectedTask.interval.TotalSeconds - re;
            }
            this.toolStripProgressBar1.PerformStep();
        }
        #endregion

        #region AdminComponent
        void InitializeAdminComponent() {
            if (!AdminPrivilages.IsAdmin()) {
                this.button5.Text += "(Standed)";
                AdminPrivilages.AddShieldToButton(this.button5);
            }
            else {
                this.button5.Text += "(Elevated)";
            }
        }
        private void button5_Click(object sender, EventArgs e) {
            if (AdminPrivilages.IsAdmin()) {
            }
            else {
                AdminPrivilages.RestartElevated();
            }
        } 

        #endregion

        #region Service

        const string ServiceServiceName = "Task Service"; 
        void InitializeServiceComponent(){
            switch (this.CheckServiceStatus()) {
                case -1:
                    this.button6_Click(this, null);
                    break;
                case 1:
                case 2:
                    break;
            }

        }
        int CheckServiceStatus() {
            System.ServiceProcess.ServiceController controller = new System.ServiceProcess.ServiceController(ServiceServiceName);
            int result = 0;
            try {
                this.label4.Text = controller.ServiceName;
                this.label5.Text = controller.ServiceType.ToString();
                this.label6.Text = "Automatic";
                switch (controller.Status) {
                    case System.ServiceProcess.ServiceControllerStatus.Running:
                        this.toolStripStatusLabel1.ForeColor = Color.Blue;
                        this.toolStripStatusLabel1.Text = "SERVICE RUNNING";
                        this.button6.Enabled = false;
                        this.button7.Enabled = false;
                        this.button8.Enabled = true;
                        this.button9.Enabled = true;
                        result = 1;
                        break;
                    case System.ServiceProcess.ServiceControllerStatus.Stopped:
                        this.toolStripStatusLabel1.ForeColor = Color.Blue;
                        this.toolStripStatusLabel1.Text = "SERVICE STOPED";
                        this.button6.Enabled = false;
                        this.button7.Enabled = true;
                        this.button8.Enabled = false;
                        this.button9.Enabled = true;
                        result = 2;
                        break;
                    default:
                        break;
                }
            }
            catch {
                this.toolStripStatusLabel1.ForeColor = Color.Red;
                this.toolStripStatusLabel1.Text = "SERVICE NOT INSTALLED";
                this.button6.Enabled = true;
                this.button7.Enabled = false;
                this.button8.Enabled = false;
                this.button9.Enabled = false;
                result = -1;
            }
            return result;
        }
        private void button6_Click(object sender, EventArgs e) {
            this.toolStripStatusLabel1.ForeColor = Color.White;
            var tempColor = this.toolStripStatusLabel1.BackColor;
            this.toolStripStatusLabel1.BackColor = Color.Red;
            this.toolStripStatusLabel1.Text = "SERVICE INSTALLING";

            ProcessStartInfo psinfo = new ProcessStartInfo();
            psinfo.CreateNoWindow = true;
            psinfo.UseShellExecute = true;
            psinfo.FileName = string.Format(@"{0}Service.exe", AppDomain.CurrentDomain.BaseDirectory);
            psinfo.Arguments = "install";
            try {
                this.toolStripStatusLabel1.Text = "SERVICE INSTALLING..";
                var process = Process.Start(psinfo);
                this.toolStripStatusLabel1.Text = "SERVICE INSTALLING....";
                process.WaitForExit();
                this.toolStripStatusLabel1.Text = "SERVICE INSTALLING.......";
                this.CheckServiceStatus();
            }
            catch{
                this.toolStripStatusLabel1.ForeColor = Color.White;
                this.toolStripStatusLabel1.BackColor = Color.Red;
                this.toolStripStatusLabel1.Text = "SERVICE FAILED";
            }
            this.toolStripStatusLabel1.BackColor = tempColor;
            
        }
        private void button7_Click(object sender, EventArgs e) {
            this.SetServiceState = true;
        }
        private void button8_Click(object sender, EventArgs e) {
            this.SetServiceState = false;
        }
        private void button9_Click(object sender, EventArgs e) {
            ProcessStartInfo psinfo = new ProcessStartInfo();
            psinfo.CreateNoWindow = true;
            psinfo.UseShellExecute = true;
            psinfo.FileName = string.Format(@"{0}Service.exe", AppDomain.CurrentDomain.BaseDirectory);
            psinfo.Arguments = "uninstall";
            try {
                var process = Process.Start(psinfo);
                process.WaitForExit();
                this.CheckServiceStatus();
            }
            catch {

            }
        } 

        #endregion
    }
}