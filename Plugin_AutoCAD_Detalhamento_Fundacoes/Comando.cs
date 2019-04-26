using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_AutoCAD_Detalhamento_Fundacoes
{
    public class Comando
    {

        [CommandMethod("gerarDesenhoFundacao")]
        public static void gerarDesenhoFundacao()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            FormDadosFundacao frmPrincipal = new FormDadosFundacao();
            frmPrincipal.ShowDialog();
            
        }



    }
}
