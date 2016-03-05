using System;
using System.Timers;
using System.Collections.Generic;
using Logging;
using Task;

namespace Service {

    public class ServiceTimer : Timer {

        public event EventHandler OnTime;

        public TimeSpan Spended {
            get;
            set;
        }
        public TimeSpan Remaning {
            get;
            set;
        }
        public ServiceTimer()
            : base() {
                this.Interval = 1000;
                this.Enabled = true;
                this.AutoReset = true;
                this.Elapsed += new ElapsedEventHandler(this.Second);
        }

        private void Second(object sender, ElapsedEventArgs e) {

#if DEBUG
            System.Console.WriteLine("Spended" + this.Spended + "\tRemaning" + this.Remaning);
            Log.Write("Spended" + this.Spended + "\tRemaning" + this.Remaning);
#endif

            var second = new TimeSpan(0,0,1);
            this.Spended = this.Spended.Add(second);
            this.Remaning = this.Remaning.Subtract(second);
            if((int)this.Remaning.TotalSeconds == 0){
                if (this.OnTime != null) {
                    this.OnTime(this, null);
                }
            }
            else if ((int)this.Remaning.TotalSeconds < 0) {
                this.Stop();
            }
        }

    }
    public class ServiceQueue : List<Task.Task> {
        private ServiceTimer timer;
        public event EventHandler OnTime;
        public ServiceQueue() {
            this.timer = new ServiceTimer();
            this.timer.OnTime += new EventHandler(this.TimeToPop);
            this.timer.Stop();
        }
        private void TimeToPop(object sender, EventArgs e) {
            if (this.OnTime != null) {
               var top = this.Pop();
                top.remainnig = new TimeSpan(0,0,0);
                this.OnTime(top, null);
            }
        }

        public void Push(Task.Task _task) {
            this.timer.Stop();
            var index = this.FindIndex(x => x.id == _task.id);
            if (index < 0) {
                this.Add(_task);
                this.Sort();
            }
            this.timer.Start();
        }
        public Task.Task Pop() {
            this.timer.Stop();
            Task.Task temp = null;
            if(this.Count > 0){
                temp = this[0];
                this.RemoveAt(0);
            }
            foreach(var item in this){
                item.remainnig = item.remainnig.Subtract(this.timer.Spended);
            }
            return temp;
        }

        public void Start() {
            if(this.Count > 0){
                this.Sort();
                this.timer.Remaning = this[0].remainnig;
                this.timer.Start();
            }
        }
        public void Stop() {
            this.timer.Stop();
            foreach (var item in this) {
                item.remainnig = item.remainnig.Subtract(this.timer.Spended);
            }
        }
    }
    public static class ServiceEngine {
        static ServiceQueue SQueue;
        static ServiceEngine() {
            SQueue = new ServiceQueue();
            SQueue.OnTime += new EventHandler(Ontime);
        }

        public static void Start() {
            Task.Task.ReadTasks(ref SQueue);
            SQueue.Start();
        }
        public static void Stop() {
            SQueue.Stop();
        }
        static void Ontime(object sender, EventArgs e) {
            var topitem = sender as Task.Task;
            var result = topitem.Action();
            Log.Write("result" + result);
            switch(result){
                case -2:
                case -1:
                case 0:
                    if (((int)topitem.remainnig.TotalSeconds > 0) && (!topitem.Suspended)) {
                        SQueue.Push(topitem);
                    }
                    break;
            }
            SQueue.Start();
        }
       
    }
}
