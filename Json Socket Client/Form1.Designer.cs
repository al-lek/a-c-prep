namespace Json_Socket_Client
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.conn_btn = new System.Windows.Forms.Button();
            this.set_btn = new System.Windows.Forms.Button();
            this.check_btn = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.init_btn = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.calib_btn = new System.Windows.Forms.Button();
            this.wait_btn = new System.Windows.Forms.Button();
            this.lift_btn = new System.Windows.Forms.Button();
            this.center_btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // conn_btn
            // 
            this.conn_btn.Location = new System.Drawing.Point(37, 39);
            this.conn_btn.Name = "conn_btn";
            this.conn_btn.Size = new System.Drawing.Size(75, 23);
            this.conn_btn.TabIndex = 0;
            this.conn_btn.Text = "Connect";
            this.conn_btn.UseVisualStyleBackColor = true;
            // 
            // set_btn
            // 
            this.set_btn.Location = new System.Drawing.Point(37, 68);
            this.set_btn.Name = "set_btn";
            this.set_btn.Size = new System.Drawing.Size(75, 23);
            this.set_btn.TabIndex = 0;
            this.set_btn.Text = "send";
            this.set_btn.UseVisualStyleBackColor = true;
            // 
            // check_btn
            // 
            this.check_btn.Location = new System.Drawing.Point(37, 97);
            this.check_btn.Name = "check_btn";
            this.check_btn.Size = new System.Drawing.Size(75, 23);
            this.check_btn.TabIndex = 0;
            this.check_btn.Text = "check";
            this.check_btn.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(141, 39);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(237, 326);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            // 
            // init_btn
            // 
            this.init_btn.Location = new System.Drawing.Point(37, 126);
            this.init_btn.Name = "init_btn";
            this.init_btn.Size = new System.Drawing.Size(75, 23);
            this.init_btn.TabIndex = 0;
            this.init_btn.Text = "init";
            this.init_btn.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(141, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(237, 20);
            this.textBox1.TabIndex = 2;
            // 
            // calib_btn
            // 
            this.calib_btn.Location = new System.Drawing.Point(37, 201);
            this.calib_btn.Name = "calib_btn";
            this.calib_btn.Size = new System.Drawing.Size(75, 23);
            this.calib_btn.TabIndex = 0;
            this.calib_btn.Text = "calibrate";
            this.calib_btn.UseVisualStyleBackColor = true;
            // 
            // wait_btn
            // 
            this.wait_btn.Location = new System.Drawing.Point(37, 230);
            this.wait_btn.Name = "wait_btn";
            this.wait_btn.Size = new System.Drawing.Size(75, 23);
            this.wait_btn.TabIndex = 0;
            this.wait_btn.Text = "wait";
            this.wait_btn.UseVisualStyleBackColor = true;
            // 
            // lift_btn
            // 
            this.lift_btn.Location = new System.Drawing.Point(37, 259);
            this.lift_btn.Name = "lift_btn";
            this.lift_btn.Size = new System.Drawing.Size(75, 23);
            this.lift_btn.TabIndex = 0;
            this.lift_btn.Text = "lift";
            this.lift_btn.UseVisualStyleBackColor = true;
            // 
            // center_btn
            // 
            this.center_btn.Location = new System.Drawing.Point(37, 288);
            this.center_btn.Name = "center_btn";
            this.center_btn.Size = new System.Drawing.Size(75, 23);
            this.center_btn.TabIndex = 0;
            this.center_btn.Text = "center";
            this.center_btn.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(738, 389);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.center_btn);
            this.Controls.Add(this.lift_btn);
            this.Controls.Add(this.wait_btn);
            this.Controls.Add(this.calib_btn);
            this.Controls.Add(this.init_btn);
            this.Controls.Add(this.check_btn);
            this.Controls.Add(this.set_btn);
            this.Controls.Add(this.conn_btn);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button conn_btn;
        private System.Windows.Forms.Button set_btn;
        private System.Windows.Forms.Button check_btn;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button init_btn;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button calib_btn;
        private System.Windows.Forms.Button wait_btn;
        private System.Windows.Forms.Button lift_btn;
        private System.Windows.Forms.Button center_btn;
    }
}

