using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plugin_AutoCAD_Detalhamento_Fundacoes
{
    public partial class FormDadosFundacao : Form
    {
        GerenciamentoArquivo arquivo = new GerenciamentoArquivo();
        ComandosAutoCAD cad = new ComandosAutoCAD();
        public List<double> listaDadosFundacao = new List<double>();
        int iNumeroEstacas = 0;
        bool pararCronometro = false;

        public FormDadosFundacao()
        {
            InitializeComponent();
        }

        private void btSelecionarPlanilha_Click(object sender, EventArgs e)
        {
            pararCronometro = false;
            try
            {
                timer1.Start();
                if (rb2estacas.Checked == true) { iNumeroEstacas = 2; }
                if (rb3estacas.Checked == true) { iNumeroEstacas = 3; }
                if (rb4estacas.Checked == true) { iNumeroEstacas = 4; }
                listaDadosFundacao = arquivo.selecionarArquivoExcel(iNumeroEstacas);
                labelTeste.Text = listaDadosFundacao[2].ToString();
                pararCronometro = true;
            }
            catch
            {

            }
        }

        private void btGerarDesenho_Click(object sender, EventArgs e)
        {
            if (listaDadosFundacao != null && iNumeroEstacas != 0)
            {
                this.Hide();
                if (iNumeroEstacas == 2) { cad.gerarDesenhos2Estacas(listaDadosFundacao); }
                if (iNumeroEstacas == 3) { cad.gerarDesenhos3Estacas(listaDadosFundacao); }
                if (iNumeroEstacas == 4) { cad.gerarDesenhos4Estacas(listaDadosFundacao); }
            }

            this.Close();
        }

        private void btSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Maximum = 100;
            if (pararCronometro == true)
            {
                timer1.Stop();
                progressBar1.Value = 100;
            }
            else
            {
                if (progressBar1.Value == progressBar1.Maximum)
                { progressBar1.Value = 0; }
            }
            progressBar1.Increment(1);
        }
    }
}
