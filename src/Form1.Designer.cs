namespace smad5 {
  partial class Form1 {
    /// <summary>
    /// Требуется переменная конструктора.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Освободить все используемые ресурсы.
    /// </summary>
    /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Код, автоматически созданный конструктором форм Windows

    /// <summary>
    /// Обязательный метод для поддержки конструктора - не изменяйте
    /// содержимое данного метода при помощи редактора кода.
    /// </summary>
    private void InitializeComponent() {
      this.panel1 = new System.Windows.Forms.Panel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.richTextBox1 = new System.Windows.Forms.RichTextBox();
      this.richTextBox2 = new System.Windows.Forms.RichTextBox();
      this.richTextBox3 = new System.Windows.Forms.RichTextBox();
      this.richTextBox4 = new System.Windows.Forms.RichTextBox();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.richTextBox1);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(299, 408);
      this.panel1.TabIndex = 0;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.richTextBox3);
      this.panel2.Controls.Add(this.richTextBox2);
      this.panel2.Controls.Add(this.richTextBox4);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(299, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(462, 408);
      this.panel2.TabIndex = 1;
      // 
      // richTextBox1
      // 
      this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.richTextBox1.Location = new System.Drawing.Point(0, 0);
      this.richTextBox1.Name = "richTextBox1";
      this.richTextBox1.Size = new System.Drawing.Size(299, 408);
      this.richTextBox1.TabIndex = 0;
      this.richTextBox1.Text = "";
      // 
      // richTextBox2
      // 
      this.richTextBox2.Dock = System.Windows.Forms.DockStyle.Left;
      this.richTextBox2.Location = new System.Drawing.Point(146, 0);
      this.richTextBox2.Name = "richTextBox2";
      this.richTextBox2.Size = new System.Drawing.Size(146, 408);
      this.richTextBox2.TabIndex = 1;
      this.richTextBox2.Text = "";
      // 
      // richTextBox3
      // 
      this.richTextBox3.Dock = System.Windows.Forms.DockStyle.Left;
      this.richTextBox3.Location = new System.Drawing.Point(292, 0);
      this.richTextBox3.Name = "richTextBox3";
      this.richTextBox3.Size = new System.Drawing.Size(146, 408);
      this.richTextBox3.TabIndex = 2;
      this.richTextBox3.Text = "";
      // 
      // richTextBox4
      // 
      this.richTextBox4.Dock = System.Windows.Forms.DockStyle.Left;
      this.richTextBox4.Location = new System.Drawing.Point(0, 0);
      this.richTextBox4.Name = "richTextBox4";
      this.richTextBox4.Size = new System.Drawing.Size(146, 408);
      this.richTextBox4.TabIndex = 3;
      this.richTextBox4.Text = "";
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(761, 408);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.Name = "Form1";
      this.Text = "Form1";
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.RichTextBox richTextBox1;
    private System.Windows.Forms.RichTextBox richTextBox3;
    private System.Windows.Forms.RichTextBox richTextBox2;
    private System.Windows.Forms.RichTextBox richTextBox4;
  }
}

