using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plugin_AutoCAD_Detalhamento_Fundacoes
{
    class GerenciamentoArquivo
    {
        public List<double> selecionarArquivoExcel(int iNumeroEstacas)
        {
            Stream myStream = null;
            OpenFileDialog caixaDialogo = new OpenFileDialog();
            List<double> listaDadosFundacao = new List<double>();
            caixaDialogo.InitialDirectory = "c:\\";
            caixaDialogo.Filter = "Excel files (*.xlsx)|*.xlsx|Excel macro files (*.xlsm)|*.xlsm";
            caixaDialogo.FilterIndex = 1;
            caixaDialogo.RestoreDirectory = true;



            if (caixaDialogo.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = caixaDialogo.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            string nomeArquivo = caixaDialogo.FileName;
                            listaDadosFundacao = pegaDadosExcel(nomeArquivo, iNumeroEstacas);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Não foi possível ler o arquivo. Original error: " + ex.Message);
                }
            }
            return listaDadosFundacao;
        }

        public List<double> pegaDadosExcel(string nomeArquivo, int numeroEstacas)
        {
            string nomeWorksheet = montaNomeWorksheet(numeroEstacas);
            List<double> listaDadosFundacao = new List<double>();
            OleDbConnection conexao = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + nomeArquivo + ";Extended Properties='Excel 12.0 Xml;HDR=NO;IMEX=1'");
            OleDbDataAdapter adapter = new OleDbDataAdapter("Select * from [" + nomeWorksheet + "]", conexao);
            DataSet ds = new DataSet();

            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();

            excelApp.Visible = false;

            Microsoft.Office.Interop.Excel.Workbook wbExcel = excelApp.Workbooks.Open(nomeArquivo, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, null, null);

            Microsoft.Office.Interop.Excel.Worksheet activeSheet = (Microsoft.Office.Interop.Excel.Worksheet)wbExcel.ActiveSheet;

            Microsoft.Office.Interop.Excel.Worksheet wsPlanilha = (Microsoft.Office.Interop.Excel.Worksheet)wbExcel.Worksheets.get_Item(numeroEstacas - 1);

            try
            {
                //conexao.Open();
                //adapter.Fill(ds);
                //LER AS CÉLULAS NECESSÁRIAS PARA A MONTAGEM DOS DADOS
                if (numeroEstacas == 2)
                {
                    string sDimXPilar = wsPlanilha.get_Range("A16", "A16").Text;
                    string sDimYPilar = wsPlanilha.get_Range("B16", "B16").Text;
                    string sDiametroEstacas = wsPlanilha.get_Range("C16", "C16").Text;
                    string sCobrimentoEstacas = wsPlanilha.get_Range("F16", "F16").Text;
                    string sEmbutimentoEstaca = wsPlanilha.get_Range("G16", "G16").Text;
                    string sDistanciaEntreEixos = wsPlanilha.get_Range("H16", "H16").Text;
                    string sDimXBloco = wsPlanilha.get_Range("E21", "E21").Text;
                    string sDimYBloco = wsPlanilha.get_Range("F21", "F21").Text;
                    string sAlturaTotalBloco = wsPlanilha.get_Range("G21", "G21").Text;

                    double fDimXPilar = converteParaFloat(sDimXPilar);
                    double fDimYPilar = converteParaFloat(sDimYPilar);
                    double fDiametroEstacas = converteParaFloat(sDiametroEstacas);
                    double fCobrimentoEstacas = converteParaFloat(sCobrimentoEstacas);
                    double fEmbutimentoEstaca = converteParaFloat(sEmbutimentoEstaca);
                    double fDistanciaEntreEixos = converteParaFloat(sDistanciaEntreEixos);
                    double fDimXBloco = converteParaFloat(sDimXBloco);
                    double fDimYBloco = converteParaFloat(sDimYBloco);
                    double fAlturaTotalBloco = converteParaFloat(sAlturaTotalBloco);

                    listaDadosFundacao.Add(fDimXPilar);
                    listaDadosFundacao.Add(fDimYPilar);
                    listaDadosFundacao.Add(fDiametroEstacas);
                    listaDadosFundacao.Add(fCobrimentoEstacas);
                    listaDadosFundacao.Add(fEmbutimentoEstaca);
                    listaDadosFundacao.Add(fDistanciaEntreEixos);
                    listaDadosFundacao.Add(fDimXBloco);
                    listaDadosFundacao.Add(fDimYBloco);
                    listaDadosFundacao.Add(fAlturaTotalBloco);

                    string sN1 = wsPlanilha.get_Range("M38", "M38").Text;
                    double fN1 = Convert.ToDouble(sN1);
                    listaDadosFundacao.Add(fN1);
                    string sN2 = wsPlanilha.get_Range("M39", "M39").Text;
                    double fN2 = Convert.ToDouble(sN2);
                    listaDadosFundacao.Add(fN2);
                    string sN4 = wsPlanilha.get_Range("M40", "M40").Text;
                    double fN4 = Convert.ToDouble(sN4);
                    listaDadosFundacao.Add(fN4);
                    string sIndicaAreaAmpliada = wsPlanilha.get_Range("C26", "C26").Text;
                    double fIndicaAreaAmpliada = 0;
                    if (sIndicaAreaAmpliada == "SIM") { fIndicaAreaAmpliada = 1; }
                    listaDadosFundacao.Add(fIndicaAreaAmpliada);

                }
                else if (numeroEstacas == 3)
                {
                    string sDimXPilar = wsPlanilha.get_Range("A16", "A16").Text;
                    string sDimYPilar = wsPlanilha.get_Range("B16", "B16").Text;
                    string sDiametroEstacas = wsPlanilha.get_Range("C16", "C16").Text;
                    string sCobrimentoEstacas = wsPlanilha.get_Range("F16", "F16").Text;
                    string sEmbutimentoEstaca = wsPlanilha.get_Range("G16", "G16").Text;
                    string sDistanciaEntreEixos = wsPlanilha.get_Range("H16", "H16").Text;
                    string sDimMaiorBloco = wsPlanilha.get_Range("A21", "A21").Text;
                    string sDimMenorBloco = wsPlanilha.get_Range("B21", "B21").Text;
                    string sAlturaTotalBloco = wsPlanilha.get_Range("G21", "G21").Text;

                    double fDimXPilar = converteParaFloat(sDimXPilar);
                    double fDimYPilar = converteParaFloat(sDimYPilar);
                    double fDiametroEstacas = converteParaFloat(sDiametroEstacas);
                    double fCobrimentoEstacas = converteParaFloat(sCobrimentoEstacas);
                    double fEmbutimentoEstaca = converteParaFloat(sEmbutimentoEstaca);
                    double fDistanciaEntreEixos = converteParaFloat(sDistanciaEntreEixos);
                    double fDimMaiorBloco = converteParaFloat(sDimMaiorBloco);
                    double fDimMenorBloco = converteParaFloat(sDimMenorBloco);
                    double fAlturaTotalBloco = converteParaFloat(sAlturaTotalBloco);
                    

                    listaDadosFundacao.Add(fDimXPilar);
                    listaDadosFundacao.Add(fDimYPilar);
                    listaDadosFundacao.Add(fDiametroEstacas);
                    listaDadosFundacao.Add(fCobrimentoEstacas);
                    listaDadosFundacao.Add(fEmbutimentoEstaca);
                    listaDadosFundacao.Add(fDistanciaEntreEixos);
                    listaDadosFundacao.Add(fDimMaiorBloco);
                    listaDadosFundacao.Add(fDimMenorBloco);
                    listaDadosFundacao.Add(fAlturaTotalBloco);

                    string sN1 = wsPlanilha.get_Range("M38", "M38").Text;
                    double fN1 = Convert.ToDouble(sN1);
                    listaDadosFundacao.Add(fN1);
                    string sN2 = wsPlanilha.get_Range("M41", "M41").Text;
                    double fN2 = Convert.ToDouble(sN2);
                    listaDadosFundacao.Add(fN2);
                    string sMalha = wsPlanilha.get_Range("M39", "M39").Text;
                    double fMalha = Convert.ToDouble(sMalha);
                    listaDadosFundacao.Add(fMalha);
                    string sIndicaAreaAmpliada = wsPlanilha.get_Range("C26", "C26").Text;
                    double fIndicaAreaAmpliada = 0;
                    if (sIndicaAreaAmpliada == "SIM") { fIndicaAreaAmpliada = 1; }
                    listaDadosFundacao.Add(fIndicaAreaAmpliada);

                }
                else if (numeroEstacas == 4)
                {
                    string sDimXPilar = wsPlanilha.get_Range("A16", "A16").Text;
                    string sDimYPilar = wsPlanilha.get_Range("B16", "B16").Text;
                    string sDiametroEstacas = wsPlanilha.get_Range("C16", "C16").Text;
                    string sCobrimentoEstacas = wsPlanilha.get_Range("F16", "F16").Text;
                    string sEmbutimentoEstaca = wsPlanilha.get_Range("G16", "G16").Text;
                    string sDistanciaEntreEixos = wsPlanilha.get_Range("H16", "H16").Text;
                    string sDimXBloco = wsPlanilha.get_Range("E21", "E21").Text;
                    string sDimYBloco = wsPlanilha.get_Range("F21", "F21").Text;
                    string sAlturaTotalBloco = wsPlanilha.get_Range("G21", "G21").Text;

                    double fDimXPilar = converteParaFloat(sDimXPilar);
                    double fDimYPilar = converteParaFloat(sDimYPilar);
                    double fDiametroEstacas = converteParaFloat(sDiametroEstacas);
                    double fCobrimentoEstacas = converteParaFloat(sCobrimentoEstacas);
                    double fEmbutimentoEstaca = converteParaFloat(sEmbutimentoEstaca);
                    double fDistanciaEntreEixos = converteParaFloat(sDistanciaEntreEixos);
                    double fDimMaiorBloco = converteParaFloat(sDimXBloco);
                    double fDimMenorBloco = converteParaFloat(sDimYBloco);
                    double fAlturaTotalBloco = converteParaFloat(sAlturaTotalBloco);


                    listaDadosFundacao.Add(fDimXPilar);
                    listaDadosFundacao.Add(fDimYPilar);
                    listaDadosFundacao.Add(fDiametroEstacas);
                    listaDadosFundacao.Add(fCobrimentoEstacas);
                    listaDadosFundacao.Add(fEmbutimentoEstaca);
                    listaDadosFundacao.Add(fDistanciaEntreEixos);
                    listaDadosFundacao.Add(fDimMaiorBloco);
                    listaDadosFundacao.Add(fDimMenorBloco);
                    listaDadosFundacao.Add(fAlturaTotalBloco);

                    string sN1 = wsPlanilha.get_Range("M38", "M38").Text;
                    double fN1 = Convert.ToDouble(sN1);
                    listaDadosFundacao.Add(fN1);
                    string sN2 = wsPlanilha.get_Range("M42", "M42").Text;
                    double fN2 = Convert.ToDouble(sN2);
                    listaDadosFundacao.Add(fN2);
                    string sN3 = wsPlanilha.get_Range("M39", "M39").Text;
                    double fN3 = Convert.ToDouble(sN3);
                    listaDadosFundacao.Add(fN3);
                    string sMalha = wsPlanilha.get_Range("M40", "M40").Text;
                    double fMalha = Convert.ToDouble(sMalha);
                    listaDadosFundacao.Add(fMalha);
                    string sIndicaAreaAmpliada = wsPlanilha.get_Range("C26", "C26").Text;
                    double fIndicaAreaAmpliada = 0;
                    if (sIndicaAreaAmpliada == "SIM") { fIndicaAreaAmpliada = 1; }
                    listaDadosFundacao.Add(fIndicaAreaAmpliada);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao acessar arquivo: " + ex.Message);
            }

            excelApp.Workbooks.Close();
            excelApp.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(wbExcel);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);


            return listaDadosFundacao;
        }

        private string montaNomeWorksheet(int iNumeroEstacas)
        {
            string nomeWorksheet = "";
            string sNumeroEstacas = Convert.ToString(iNumeroEstacas);
            sNumeroEstacas = sNumeroEstacas.TrimStart();
            sNumeroEstacas = sNumeroEstacas.TrimEnd();
            nomeWorksheet = sNumeroEstacas + " Estacas";
            return nomeWorksheet;
        }


        private double converteParaFloat(string sNumero)
        {
            double n = 0;
            sNumero = sNumero.TrimStart();
            sNumero = sNumero.TrimEnd();
            int qtd = sNumero.Length;
            sNumero = sNumero.Remove(qtd - 2, 2);
            sNumero = sNumero.TrimStart();
            sNumero = sNumero.TrimEnd();
            //mudanças aqui
            string[] partes = sNumero.Split(',');
            int pot = partes[1].Length;
            double div = Math.Pow(10, pot);
            double parte1 = Convert.ToDouble(partes[0]);
            double parte2 = Convert.ToDouble(partes[1]);
            parte2 = parte2 / div;
            n = parte1 + parte2;
            //n = Convert.ToDouble(sNumero);
            return n;
        }


    }
}

