namespace Emmet
{
    partial class AbbreviationPrompt
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
            this.txtAbbreviation = new System.Windows.Forms.TextBox();
            this.btnWrap = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtAbbreviation
            // 
            this.txtAbbreviation.Location = new System.Drawing.Point(13, 13);
            this.txtAbbreviation.Name = "txtAbbreviation";
            this.txtAbbreviation.Size = new System.Drawing.Size(267, 20);
            this.txtAbbreviation.TabIndex = 0;
            // 
            // btnWrap
            // 
            this.btnWrap.Location = new System.Drawing.Point(287, 12);
            this.btnWrap.Name = "btnWrap";
            this.btnWrap.Size = new System.Drawing.Size(87, 23);
            this.btnWrap.TabIndex = 1;
            this.btnWrap.Text = "Wrap";
            this.btnWrap.UseVisualStyleBackColor = true;
            this.btnWrap.Click += new System.EventHandler(this.btnWrap_Click);
            // 
            // AbbreviationPrompt
            // 
            this.AcceptButton = this.btnWrap;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 45);
            this.ControlBox = false;
            this.Controls.Add(this.btnWrap);
            this.Controls.Add(this.txtAbbreviation);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "AbbreviationPrompt";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Enter Abbreviation to Wrap With:";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtAbbreviation;
        private System.Windows.Forms.Button btnWrap;
    }
}