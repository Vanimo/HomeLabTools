namespace ServerFanControlService
{
    partial class TemperatureControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ConfigWatcher = new System.IO.FileSystemWatcher();
            this.PingTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.ConfigWatcher)).BeginInit();
            // 
            // ConfigWatcher
            // 
            this.ConfigWatcher.EnableRaisingEvents = true;
            this.ConfigWatcher.Filter = "*.xml";
            this.ConfigWatcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
            this.ConfigWatcher.Changed += new System.IO.FileSystemEventHandler(this.ConfigWatcher_Changed);
            // 
            // PingTimer
            // 
            this.PingTimer.Interval = 20000;
            this.PingTimer.Tag = "HL_PingTimer";
            this.PingTimer.Tick += new System.EventHandler(this.PingTimer_Tick);
            // 
            // TemperatureControl
            // 
            this.CanPauseAndContinue = true;
            this.ServiceName = "HL_TemperatureControl";
            ((System.ComponentModel.ISupportInitialize)(this.ConfigWatcher)).EndInit();

        }

        #endregion
        private System.IO.FileSystemWatcher ConfigWatcher;
        private System.Windows.Forms.Timer PingTimer;
    }
}
