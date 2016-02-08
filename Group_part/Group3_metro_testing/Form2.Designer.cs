namespace Group3_metro_testing
{
    partial class Form2
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
            this.IdentificationRegisterButton = new MetroFramework.Controls.MetroButton();
            this.IdentificationTextBox = new MetroFramework.Controls.MetroTextBox();
            this.IdentificationLabel = new MetroFramework.Controls.MetroLabel();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.IdentificationCancleButton = new MetroFramework.Controls.MetroButton();
            this.SuspendLayout();
            // 
            // IdentificationRegisterButton
            // 
            this.IdentificationRegisterButton.AutoSize = true;
            this.IdentificationRegisterButton.Location = new System.Drawing.Point(239, 135);
            this.IdentificationRegisterButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.IdentificationRegisterButton.Name = "IdentificationRegisterButton";
            this.IdentificationRegisterButton.Size = new System.Drawing.Size(70, 34);
            this.IdentificationRegisterButton.Style = MetroFramework.MetroColorStyle.White;
            this.IdentificationRegisterButton.TabIndex = 42;
            this.IdentificationRegisterButton.Text = "등록";
            this.IdentificationRegisterButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.IdentificationRegisterButton.UseSelectable = true;
            this.IdentificationRegisterButton.UseStyleColors = true;
            this.IdentificationRegisterButton.Click += new System.EventHandler(this.IdentificationRegisterButton_Click);
            // 
            // IdentificationTextBox
            // 
            this.IdentificationTextBox.FontWeight = MetroFramework.MetroTextBoxWeight.Bold;
            this.IdentificationTextBox.ForeColor = System.Drawing.Color.Gold;
            this.IdentificationTextBox.Lines = new string[] {
        "2011112311"};
            this.IdentificationTextBox.Location = new System.Drawing.Point(94, 135);
            this.IdentificationTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.IdentificationTextBox.MaxLength = 32767;
            this.IdentificationTextBox.Name = "IdentificationTextBox";
            this.IdentificationTextBox.PasswordChar = '\0';
            this.IdentificationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.IdentificationTextBox.SelectedText = "";
            this.IdentificationTextBox.Size = new System.Drawing.Size(120, 34);
            this.IdentificationTextBox.TabIndex = 41;
            this.IdentificationTextBox.Text = "2011112311";
            this.IdentificationTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.IdentificationTextBox.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.IdentificationTextBox.UseCustomForeColor = true;
            this.IdentificationTextBox.UseSelectable = true;
            // 
            // IdentificationLabel
            // 
            this.IdentificationLabel.Location = new System.Drawing.Point(33, 135);
            this.IdentificationLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.IdentificationLabel.Name = "IdentificationLabel";
            this.IdentificationLabel.Size = new System.Drawing.Size(53, 34);
            this.IdentificationLabel.TabIndex = 40;
            this.IdentificationLabel.Text = "ID";
            this.IdentificationLabel.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.IdentificationLabel.UseStyleColors = true;
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(33, 57);
            this.metroLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(248, 19);
            this.metroLabel1.Style = MetroFramework.MetroColorStyle.Orange;
            this.metroLabel1.TabIndex = 40;
            this.metroLabel1.Text = "학번을 입력하셔야 사용이 가능합니다.";
            this.metroLabel1.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroLabel1.UseStyleColors = true;
            // 
            // IdentificationCancleButton
            // 
            this.IdentificationCancleButton.AutoSize = true;
            this.IdentificationCancleButton.Location = new System.Drawing.Point(317, 135);
            this.IdentificationCancleButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.IdentificationCancleButton.Name = "IdentificationCancleButton";
            this.IdentificationCancleButton.Size = new System.Drawing.Size(70, 34);
            this.IdentificationCancleButton.Style = MetroFramework.MetroColorStyle.White;
            this.IdentificationCancleButton.TabIndex = 42;
            this.IdentificationCancleButton.Text = "취소";
            this.IdentificationCancleButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.IdentificationCancleButton.UseSelectable = true;
            this.IdentificationCancleButton.UseStyleColors = true;
            this.IdentificationCancleButton.Click += new System.EventHandler(this.IdentificationCancleButton_Click);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 221);
            this.ControlBox = false;
            this.Controls.Add(this.IdentificationCancleButton);
            this.Controls.Add(this.IdentificationRegisterButton);
            this.Controls.Add(this.IdentificationTextBox);
            this.Controls.Add(this.metroLabel1);
            this.Controls.Add(this.IdentificationLabel);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form2";
            this.Padding = new System.Windows.Forms.Padding(29, 90, 29, 30);
            this.Resizable = false;
            this.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroButton IdentificationRegisterButton;
        private MetroFramework.Controls.MetroTextBox IdentificationTextBox;
        private MetroFramework.Controls.MetroLabel IdentificationLabel;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroButton IdentificationCancleButton;
    }
}