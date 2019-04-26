namespace Plugin_AutoCAD_Detalhamento_Fundacoes
{
    partial class FormDadosFundacao
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDadosFundacao));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rb4estacas = new System.Windows.Forms.RadioButton();
            this.rb3estacas = new System.Windows.Forms.RadioButton();
            this.rb2estacas = new System.Windows.Forms.RadioButton();
            this.btSelecionarPlanilha = new System.Windows.Forms.Button();
            this.btGerarDesenho = new System.Windows.Forms.Button();
            this.labelTeste = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btSair = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rb4estacas);
            this.groupBox1.Controls.Add(this.rb3estacas);
            this.groupBox1.Controls.Add(this.rb2estacas);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(150, 105);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Número de estacas";
            // 
            // rb4estacas
            // 
            this.rb4estacas.AutoSize = true;
            this.rb4estacas.Location = new System.Drawing.Point(7, 77);
            this.rb4estacas.Name = "rb4estacas";
            this.rb4estacas.Size = new System.Drawing.Size(79, 19);
            this.rb4estacas.TabIndex = 2;
            this.rb4estacas.TabStop = true;
            this.rb4estacas.Text = "4 estacas";
            this.rb4estacas.UseVisualStyleBackColor = true;
            // 
            // rb3estacas
            // 
            this.rb3estacas.AutoSize = true;
            this.rb3estacas.Location = new System.Drawing.Point(7, 52);
            this.rb3estacas.Name = "rb3estacas";
            this.rb3estacas.Size = new System.Drawing.Size(79, 19);
            this.rb3estacas.TabIndex = 1;
            this.rb3estacas.TabStop = true;
            this.rb3estacas.Text = "3 estacas";
            this.rb3estacas.UseVisualStyleBackColor = true;
            // 
            // rb2estacas
            // 
            this.rb2estacas.AutoSize = true;
            this.rb2estacas.Location = new System.Drawing.Point(7, 27);
            this.rb2estacas.Name = "rb2estacas";
            this.rb2estacas.Size = new System.Drawing.Size(79, 19);
            this.rb2estacas.TabIndex = 0;
            this.rb2estacas.TabStop = true;
            this.rb2estacas.Text = "2 estacas";
            this.rb2estacas.UseVisualStyleBackColor = true;
            // 
            // btSelecionarPlanilha
            // 
            this.btSelecionarPlanilha.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btSelecionarPlanilha.Location = new System.Drawing.Point(175, 25);
            this.btSelecionarPlanilha.Name = "btSelecionarPlanilha";
            this.btSelecionarPlanilha.Size = new System.Drawing.Size(117, 26);
            this.btSelecionarPlanilha.TabIndex = 1;
            this.btSelecionarPlanilha.Text = "Carregar planilha";
            this.btSelecionarPlanilha.UseVisualStyleBackColor = true;
            this.btSelecionarPlanilha.Click += new System.EventHandler(this.btSelecionarPlanilha_Click);
            // 
            // btGerarDesenho
            // 
            this.btGerarDesenho.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btGerarDesenho.Location = new System.Drawing.Point(175, 57);
            this.btGerarDesenho.Name = "btGerarDesenho";
            this.btGerarDesenho.Size = new System.Drawing.Size(117, 26);
            this.btGerarDesenho.TabIndex = 2;
            this.btGerarDesenho.Text = "Gerar desenho";
            this.btGerarDesenho.UseVisualStyleBackColor = true;
            this.btGerarDesenho.Click += new System.EventHandler(this.btGerarDesenho_Click);
            // 
            // labelTeste
            // 
            this.labelTeste.AutoSize = true;
            this.labelTeste.Location = new System.Drawing.Point(109, 140);
            this.labelTeste.Name = "labelTeste";
            this.labelTeste.Size = new System.Drawing.Size(41, 15);
            this.labelTeste.TabIndex = 3;
            this.labelTeste.Text = "label1";
            this.labelTeste.Visible = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FloralWhite;
            this.panel1.Controls.Add(this.progressBar1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btSair);
            this.panel1.Location = new System.Drawing.Point(0, 195);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(303, 41);
            this.panel1.TabIndex = 4;
            // 
            // btSair
            // 
            this.btSair.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btSair.Location = new System.Drawing.Point(247, 8);
            this.btSair.Name = "btSair";
            this.btSair.Size = new System.Drawing.Size(53, 26);
            this.btSair.TabIndex = 5;
            this.btSair.Text = "Sair";
            this.btSair.UseVisualStyleBackColor = true;
            this.btSair.Click += new System.EventHandler(this.btSair_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Carregando...";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(97, 8);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(126, 23);
            this.progressBar1.TabIndex = 7;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FormDadosFundacao
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 237);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.labelTeste);
            this.Controls.Add(this.btGerarDesenho);
            this.Controls.Add(this.btSelecionarPlanilha);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(320, 276);
            this.MinimumSize = new System.Drawing.Size(320, 276);
            this.Name = "FormDadosFundacao";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Detalhamento da Fundação";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rb4estacas;
        private System.Windows.Forms.RadioButton rb3estacas;
        private System.Windows.Forms.RadioButton rb2estacas;
        private System.Windows.Forms.Button btSelecionarPlanilha;
        private System.Windows.Forms.Button btGerarDesenho;
        private System.Windows.Forms.Label labelTeste;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btSair;
        private System.Windows.Forms.Timer timer1;
    }
}