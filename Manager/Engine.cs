using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Win32;

using Task;
using Logging;

namespace Manager {
    public class TaskList : List<Task.Task>{
        public event EventHandler OnAdd;
        public event EventHandler OnRemove;

        public new void Add(Task.Task _task) {
            if (this.OnAdd != null) {
                this.OnAdd(this, null);
            }
            int index = this.FindIndex(_t => _t.id == _task.id);
            if(index < 0){
                base.Add(_task);
            }
        }
        public new void RemoveAt(int index) {
            if (this.OnRemove != null) {
                this.OnRemove(this, null);
            }
            var temp = base[index] as Task.Task;
            temp.SelfDestroy();
            base.RemoveAt(index);
        }
}
    class Engine : ApplicationContext {
        internal static TaskList Tasks = new TaskList();

        #region Constructers
        public Engine() {
            this.InitializeComponent();
        } 
        #endregion
        
        #region Initialize
        public void InitializeComponent() {
            Tasks.Clear();
            Task.Task.ReadTasks(ref Tasks, true);
        } 
        #endregion

    }
}
