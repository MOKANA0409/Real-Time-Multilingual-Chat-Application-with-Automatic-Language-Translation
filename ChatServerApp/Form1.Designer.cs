namespace ChatServerApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox txtLogs;
        private System.Windows.Forms.Button btnLoadHistory;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnStart = new System.Windows.Forms.Button();
            this.txtLogs = new System.Windows.Forms.TextBox();
            this.btnLoadHistory = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStart.Location = new System.Drawing.Point(33, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(180, 40);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start Server";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // txtLogs
            // 
            this.txtLogs.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtLogs.Location = new System.Drawing.Point(12, 65);
            this.txtLogs.Multiline = true;
            this.txtLogs.Name = "txtLogs";
            this.txtLogs.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLogs.Size = new System.Drawing.Size(600, 370);
            this.txtLogs.TabIndex = 1;
            // 
            // btnLoadHistory
            // 
            this.btnLoadHistory.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnLoadHistory.Location = new System.Drawing.Point(210, 12);
            this.btnLoadHistory.Name = "btnLoadHistory";
            this.btnLoadHistory.Size = new System.Drawing.Size(180, 40);
            this.btnLoadHistory.TabIndex = 2;
            this.btnLoadHistory.Text = "Load Chat History";
            this.btnLoadHistory.UseVisualStyleBackColor = true;
            this.btnLoadHistory.Click += new System.EventHandler(this.btnLoadHistory_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(624, 451);
            this.Controls.Add(this.btnLoadHistory);
            this.Controls.Add(this.txtLogs);
            this.Controls.Add(this.btnStart);
            this.Name = "Form1";
            this.Text = "Chat Server with History";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
