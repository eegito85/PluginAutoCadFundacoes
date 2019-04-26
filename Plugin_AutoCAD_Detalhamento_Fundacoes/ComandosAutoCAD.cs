using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plugin_AutoCAD_Detalhamento_Fundacoes
{
    class ComandosAutoCAD
    {
        ////LISTA DE CORES
        //public Color[] cores = new Color[] {
        //                                     Color.FromRgb(255, 0, 0), //vermelho
        //                                     Color.FromRgb(255, 215, 0), //amarelo dourado
        //                                     Color.FromRgb(0, 0, 255), //azul
        //                                     Color.FromRgb(100, 149, 237), //azul claro
        //                                     Color.FromRgb(0, 255, 0) }; //verde

        //LISTA DE CORES
        public Color[] cores = new Color[] {
                                             Color.FromColorIndex(ColorMethod.ByAci, 1),
                                             Color.FromColorIndex(ColorMethod.ByAci, 5),
                                             Color.FromColorIndex(ColorMethod.ByAci, 10),
                                             Color.FromColorIndex(ColorMethod.ByAci, 12),
                                             Color.FromColorIndex(ColorMethod.ByAci, 30),
                                             Color.FromColorIndex(ColorMethod.ByAci, 41),
                                             Color.FromColorIndex(ColorMethod.ByAci, 50),
                                             Color.FromColorIndex(ColorMethod.ByAci, 82),
                                             Color.FromColorIndex(ColorMethod.ByAci, 103),
                                             Color.FromColorIndex(ColorMethod.ByAci, 150),
                                             Color.FromColorIndex(ColorMethod.ByAci, 212),
                                             Color.FromColorIndex(ColorMethod.ByAci, 231),
                                             Color.FromColorIndex(ColorMethod.ByAci, 254) };


        //LISTA DE ESPESSURAS
        public LineWeight[] espessuras = new LineWeight[] {
                                             LineWeight.LineWeight009,
                                             LineWeight.LineWeight020,
                                             LineWeight.LineWeight030,
                                             LineWeight.LineWeight040,
                                             LineWeight.LineWeight050 };

        


        //DESENHAR BLOCO COM 2 ESTACAS
        public void gerarDesenhos2Estacas(List<double> listaDeDados)
        {
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            //PONTO DE INSERÇÃO DOS DESENHOS
            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");

            pPtOpts.Message = "\nSelecione o ponto onde deseja inserir os desenhos: ";
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point3d ptInicio = pPtRes.Value;

            double x0 = ptInicio.X;
            double y0 = ptInicio.Y;
            double dx = listaDeDados[6];
            double dy = listaDeDados[7];
            double aux = 0;
            double aux1 = 0;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;

                criarLayers(acTrans, acLyrTbl);

                //criar bloco - vista superior
                Polyline blocoVS = constroiPolilinhaBlocoRetangular(x0, y0, x0 + dx, y0, x0 + dx, y0 + dy, x0, y0 + dy, cores[2]);
                blocoVS.LineWeight = espessuras[3];
                blocoVS.Layer = "CONTORNO FUNDACAO";

                double xp = x0 + (0.5 * dx) - (0.5 * listaDeDados[0]);
                double yp = y0 + (0.5 * dy) - (0.5 * listaDeDados[1]);
                double dpx = listaDeDados[0];
                double dpy = listaDeDados[1];

                //criar pilar - vista superior
                Polyline pilarVS = constroiPolilinhaBlocoRetangular(xp, yp, xp + listaDeDados[0], yp, xp + listaDeDados[0], yp + listaDeDados[1], xp, yp + listaDeDados[1], cores[5]);
                pilarVS.LineWeight = espessuras[3];
                pilarVS.Layer = "CONTORNO PILAR";

                if (listaDeDados[6] < listaDeDados[7])
                {
                    aux = dx;
                    dx = dy;
                    dy = aux;
                    aux1 = dpx;
                    dpx = dpy;
                    dpy = aux1;

                    //Rotacionar o conjunto, caso necessário
                    Matrix3d curUCSMatrix = acDoc.Editor.CurrentUserCoordinateSystem;
                    CoordinateSystem3d curUCS = curUCSMatrix.CoordinateSystem3d;

                    blocoVS.TransformBy(Matrix3d.Rotation(-1.5708, curUCS.Zaxis, ptInicio));
                    pilarVS.TransformBy(Matrix3d.Rotation(-1.5708, curUCS.Zaxis, ptInicio));

                    //translacionar o bloco + pilar para ajustar a posição
                    Point3d ptBlocoVS = new Point3d(x0, y0, 0);
                    Vector3d blocoVSVec3d = ptBlocoVS.GetVectorTo(new Point3d(x0, y0 + dy, 0));
                    blocoVS.TransformBy(Matrix3d.Displacement(blocoVSVec3d));

                    Point3d ptPilarVS = new Point3d(xp, yp, 0);
                    Vector3d pilarVSVec3d = ptPilarVS.GetVectorTo(new Point3d(xp, yp + dy, 0));
                    pilarVS.TransformBy(Matrix3d.Displacement(pilarVSVec3d));
                }

                acBlkTblRec.AppendEntity(blocoVS);
                acTrans.AddNewlyCreatedDBObject(blocoVS, true);
                acBlkTblRec.AppendEntity(pilarVS);
                acTrans.AddNewlyCreatedDBObject(pilarVS, true);

                double xe1 = x0 + (0.5 * (dx - listaDeDados[5]));
                double ye1 = y0 + (0.5 * dy);
                double xe2 = x0 + dx - (0.5 * (dx - listaDeDados[5]));
                double ye2 = ye1;

                //criar estacas
                inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe1, ye1, listaDeDados[2]);
                inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe2, ye2, listaDeDados[2]);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, ye1 - (0.5 * dy), x0 + dx, ye1 - (0.5 * dy), 10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, ye1 - (0.5 * dy), x0 + (0.5 * dx) - (0.5 * dpx), ye1 - (0.5 * dy), 25);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + (0.5 * dx) - (0.5 * dpx), ye1 - (0.5 * dy), x0 + (0.5 * dx) + (0.5 * dpx), ye1 - (0.5 * dy), 25);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + (0.5 * dx) + (0.5 * dpx), ye1 - (0.5 * dy), x0 + dx, ye1 - (0.5 * dy), 25);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, ye1 - (0.5 * dy), x0, ye1 + (0.5 * dy), -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, ye1 + (0.5 * dy), xe1, ye1 + (0.5 * dy), -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe1, ye1 + (0.5 * dy), xe2, ye1 + (0.5 * dy), -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe2, ye1 + (0.5 * dy), x0 + dx, ye1 + (0.5 * dy), -10);

                //criar bloco - vista frontal
                double dy1 = 218 + listaDeDados[8];
                Polyline blocoVF = constroiPolilinhaBlocoRetangular(x0, y0 - dy1, x0 + dx, y0 - dy1, x0 + dx, y0 - dy1 + listaDeDados[8], x0, y0 - dy1 + listaDeDados[8], cores[2]);
                blocoVF.LineWeight = espessuras[3];
                blocoVF.LineWeight = LineWeight.LineWeight200;
                blocoVF.Layer = "CONTORNO FUNDACAO";
                acBlkTblRec.AppendEntity(blocoVF);
                acTrans.AddNewlyCreatedDBObject(blocoVF, true);

                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, y0 - dy1, x0, y0 - dy1 + listaDeDados[8], -10);

                //inserção de estaca na vista frontal
                double xe3 = xe1;
                double ye3 = y0 - dy1 + listaDeDados[4];
                double xe4 = xe1 + listaDeDados[5];
                double ye4 = ye3;
                Point3d pt3 = new Point3d(xe3, ye3, 0);
                Point3d pt4 = new Point3d(xe4, ye4, 0);
                //int fatorEscala = Convert.ToInt16(listaDeDados[2]);

                inserirEstacas1(acDoc, acLyrTbl, acTrans, acBlkTblRec, pt3, listaDeDados[2], 0);
                inserirEstacas1(acDoc, acLyrTbl, acTrans, acBlkTblRec, pt4, listaDeDados[2], 0);
                constroiPilar(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(0.5 * (xe3 + xe4), y0 - dy1 + listaDeDados[8], 0), dpx, listaDeDados[8], 0, cores[5], espessuras[3]);

                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, y0 - dy1 + listaDeDados[8], x0 + (0.5 * dx) - (0.5 * dpx), y0 - dy1 + listaDeDados[8], -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + (0.5 * dx) - (0.5 * dpx), y0 - dy1 + listaDeDados[8], x0 + (0.5 * dx) + (0.5 * dpx), y0 - dy1 + listaDeDados[8], -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + (0.5 * dx) + (0.5 * dpx), y0 - dy1 + listaDeDados[8], x0 + dx, y0 - dy1 + listaDeDados[8], -10);


                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, y0 - dy1, xe3 - (0.5 * listaDeDados[2]), y0 - dy1, 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe3 - (0.5 * listaDeDados[2]), y0 - dy1, xe3 + (0.5 * listaDeDados[2]), y0 - dy1, 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe3 + (0.5 * listaDeDados[2]), y0 - dy1, xe4 - (0.5 * listaDeDados[2]), y0 - dy1, 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe4 - (0.5 * listaDeDados[2]), y0 - dy1, xe4 + (0.5 * listaDeDados[2]), y0 - dy1, 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe4 + (0.5 * listaDeDados[2]), y0 - dy1, x0 + dx, y0 - dy1, 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, y0 - dy1, xe3, y0 - dy1, 25 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe3, y0 - dy1, xe4, y0 - dy1, 25 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe4, y0 - dy1, x0 + dx, y0 - dy1, 25 + (1.25 * listaDeDados[2]));

                // criar bloco - vista lateral
                double dx1 = dx + 210;
                Polyline blocoVL = constroiPolilinhaBlocoRetangular(x0 + dx1, y0, x0 + dx1 + listaDeDados[8], y0, x0 + dx1 + listaDeDados[8], y0 + dy, x0 + dx1, y0 + dy, cores[2]);
                blocoVL.LineWeight = espessuras[3];
                blocoVL.Layer = "CONTORNO FUNDACAO";
                acBlkTblRec.AppendEntity(blocoVL);
                acTrans.AddNewlyCreatedDBObject(blocoVL, true);

                double xe5 = x0 + dx1 + listaDeDados[8] - listaDeDados[4];
                double ye5 = y0 + (0.5 * dy);
                Point3d pt5 = new Point3d(xe5, ye5, 0);
                inserirEstacas1(acDoc, acLyrTbl, acTrans, acBlkTblRec, pt5, listaDeDados[2], 1.5708);
                constroiPilar(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(x0 + dx1, y0 + (dy * 0.5), 0), dpy, listaDeDados[8], 1.5708, cores[5], espessuras[3]);

                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1, y0, xe5, y0, 10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1, y0, x0 + dx1, y0 + (dy * 0.5) - (0.5 * dpy), -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1, y0 + (dy * 0.5) - (0.5 * dpy), x0 + dx1, y0 + (dy * 0.5) + (0.5 * dpy), -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1, y0 + (dy * 0.5) + (0.5 * dpy) + dpy, x0 + dx1, y0 + dy, -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + listaDeDados[8] + (1.25 * listaDeDados[2]), y0, x0 + dx1 + listaDeDados[8] + (1.25 * listaDeDados[2]), y0 + (dy * 0.5) - (0.5 * listaDeDados[2]), 1);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + listaDeDados[8] + (1.25 * listaDeDados[2]), y0 + (dy * 0.5) - (0.5 * listaDeDados[2]), x0 + dx1 + listaDeDados[8] + (1.25 * listaDeDados[2]), y0 + (dy * 0.5) + (0.5 * listaDeDados[2]), 1);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + listaDeDados[8] + (1.25 * listaDeDados[2]), y0 + (dy * 0.5) + (0.5 * listaDeDados[2]), x0 + dx1 + listaDeDados[8] + (1.25 * listaDeDados[2]), y0 + dy, 1);


                //criar bloco - vista lateral com armaduras
                double dx2 = dx1 + listaDeDados[8] + 162;
                Polyline blocoVLA = constroiPolilinhaBlocoRetangular(x0 + dx2, y0, x0 + dx2 + listaDeDados[8], y0, x0 + dx2 + listaDeDados[8], y0 + dy, x0 + dx2, y0 + dy, cores[9]);
                blocoVLA.LineWeight = espessuras[0];
                blocoVLA.Layer = "VISTA";
                acBlkTblRec.AppendEntity(blocoVLA);
                acTrans.AddNewlyCreatedDBObject(blocoVLA, true);
                double xe6 = x0 + dx2 + listaDeDados[8] - listaDeDados[4];
                double ye6 = y0 + (0.5 * dy);
                inserirEstacaVArmadura(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(xe6, ye6, 0), listaDeDados[2], 1.5708, listaDeDados[4]);
                constroiPilar(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(x0 + dx2, y0 + (dy * 0.5), 0), dpy, listaDeDados[8], 1.5708, cores[9], espessuras[0]);

                //criar bloco - vista frontal armadura
                double dx3 = dx1;
                Polyline blocoVFA = constroiPolilinhaBlocoRetangular(x0 + dx3, y0 - dy1, x0 + dx3 + dx, y0 - dy1, x0 + dx3 + dx, y0 - dy1 + listaDeDados[8], x0 + dx3, y0 - dy1 + listaDeDados[8], cores[9]);
                blocoVFA.LineWeight = espessuras[0];
                blocoVFA.Layer = "VISTA";
                acBlkTblRec.AppendEntity(blocoVFA);
                acTrans.AddNewlyCreatedDBObject(blocoVFA, true);
                double xe7 = xe1 + dx3;
                double ye7 = ye3;
                inserirEstacaVArmadura(acDoc, acLyrTbl,acTrans, acBlkTblRec, new Point3d(xe7, ye7, 0), listaDeDados[2], 0, listaDeDados[4]);
                inserirEstacaVArmadura(acDoc, acLyrTbl,acTrans, acBlkTblRec, new Point3d(xe7 + listaDeDados[5], ye7, 0), listaDeDados[2], 0, listaDeDados[4]);
                constroiPilar(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(xe7 + (0.5 * listaDeDados[5]), ye7 + listaDeDados[8] - listaDeDados[4], 0), dpx, listaDeDados[8], 0, cores[9], espessuras[0]);

                //criar bloco - vista superior armadura
                double dy2 = dy1 + listaDeDados[8] + 189;
                Polyline blocoVSA = constroiPolilinhaBlocoRetangular(x0, y0 - dy2, x0 + dx, y0 - dy2, x0 + dx, y0 + dy - dy2, x0, y0 + dy - dy2, cores[2]);
                blocoVSA.LineWeight = espessuras[3];
                blocoVSA.Layer = "VISTA";
                acBlkTblRec.AppendEntity(blocoVSA);
                acTrans.AddNewlyCreatedDBObject(blocoVSA, true);

                double xevs1 = xe1; double xevs2 = xe2;
                double yevs1 = y0 - dy2 + listaDeDados[3] + (listaDeDados[2] * 0.5); double yevs2 = yevs1;

                inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xevs1, yevs1, listaDeDados[2]);
                inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xevs2, yevs2, listaDeDados[2]);

                inserirArmadurasB2VS(acDoc, acTrans, acBlkTblRec, new Point3d(xevs1, yevs1, 0), listaDeDados[2], dx, listaDeDados[5], listaDeDados[12], listaDeDados[9]);
                inserirArmadurasB2VL(acDoc, acTrans, acBlkTblRec, new Point3d(x0 + dx2, y0, 0), listaDeDados[2], dy, listaDeDados[8], listaDeDados[5], listaDeDados[4], listaDeDados[9], 2, listaDeDados[11], listaDeDados[12]);
                inserirArmadurasB2VF(acDoc, acTrans, acBlkTblRec, new Point3d(x0 + dx3, y0 - dy1, 0), listaDeDados[2], dx, listaDeDados[8], listaDeDados[5], listaDeDados[4], listaDeDados[10], listaDeDados[11]);

                dynamic acadApp = Autodesk.AutoCAD.ApplicationServices.Application.AcadApplication;
                acadApp.ZoomExtents();

                // Salva o novo objeto para o banco de dados
                acTrans.Commit();
            }
        }

        //DESENHAR BLOCO COM 3 ESTACAS
        public void gerarDesenhos3Estacas(List<double> listaDeDados)
        {
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
        
            //PONTO DE INSERÇÃO DOS DESENHOS
            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");
        
            pPtOpts.Message = "\nSelecione o ponto onde deseja inserir os desenhos: ";
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point3d ptInicio = pPtRes.Value;
        
            double x0 = ptInicio.X;
            double y0 = ptInicio.Y;
        
            double sen60 = (Math.Sqrt(3)) / 2;
            double cos60 = 0.5;
            double a = listaDeDados[6];
            double b = listaDeDados[7];
            double x1 = x0 + a;
            double y1 = y0;
            double x2 = x0 + a + (b * cos60);
            double y2 = y0 + (b * sen60);
            double x3 = x0 + ((a + b) * cos60);
            double y3 = y0 + ((a + b) * sen60);
            double x4 = x0 + ((a - b) * cos60);
            double y4 = y3;
            double x5 = x0 - (b * cos60);
            double y5 = y2;
        
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;

                criarLayers(acTrans, acLyrTbl);

                //criar bloco - vista superior
                Polyline blocoVS = constroiBlocoHexagonal(x0, y0, x1, y1, x2, y2, x3, y3, x4, y4, x5, y5, cores[2]);
                blocoVS.LineWeight = espessuras[3];
                blocoVS.Layer = "CONTORNO FUNDACAO";
        
                acBlkTblRec.AppendEntity(blocoVS);
                acTrans.AddNewlyCreatedDBObject(blocoVS, true);
        
                double c = (0.5 * listaDeDados[2]) + listaDeDados[3];
                double l = listaDeDados[5];
                double xe1 = x0 + (b * 0.5);
                double ye1 = y0 + c;
                double xe2 = x0 + (l + (b * 0.5));
                double ye2 = ye1;
                double xe3 = x0 + ((b + l) * 0.5);
                double ye3 = ye1 + (l * sen60);
        
                inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe1, ye1, listaDeDados[2]);
                inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe2, ye2, listaDeDados[2]);
                inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe3, ye3, listaDeDados[2]);
        
                double xp = xe3 - (0.5 * listaDeDados[0]);
                double yp = ye1 + (0.5 * sen60);
        
                //criar pilar - vista superior
                Polyline pilarVS = constroiPolilinhaBlocoRetangular(xp, yp, xp + listaDeDados[0], yp, xp + listaDeDados[0], yp + listaDeDados[1], xp, yp + listaDeDados[1], cores[5]);
                pilarVS.LineWeight = espessuras[3];
                pilarVS.Layer = "CONTORNO PILAR";
        
                acBlkTblRec.AppendEntity(pilarVS);
                acTrans.AddNewlyCreatedDBObject(pilarVS, true);

                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x5, y0, x0, y0, 10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, y0, x1, y0, 10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x1, y0, x2, y0, 10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x5, y0, x2, y0, 25);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x5, y0, x5, y3, -25);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x5, y5, x0, y0, -15);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x5, y5, x4, y4, -15);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x2, y0, x2, y3, 25);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xp, yp, xp + listaDeDados[0], yp, 10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xp + listaDeDados[0], yp, xp + listaDeDados[0], yp + listaDeDados[1], 10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe1, y0, xe1, ye1 - (0.5 * listaDeDados[2]), 10);


                //criar bloco - vista frontal
                double dy1 = 300;
                double largura = x2 - x5;
                double alturaB = listaDeDados[8];
                Polyline blocoVF = constroiPolilinhaBlocoRetangular(x5, y5 - dy1, x5 + largura, y5 - dy1, x5 + largura, y5 - dy1 + alturaB, x5, y5 - dy1 + alturaB, cores[2]);
                blocoVF.LineWeight = espessuras[3];
                blocoVF.Layer = "CONTORNO FUNDACAO";
                inserirEstacas1(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(xe1, y5 - dy1 + listaDeDados[4], 0), listaDeDados[2], 0);
                inserirEstacas1(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(xe2, y5 - dy1 + listaDeDados[4], 0), listaDeDados[2], 0);
                constroiPilar(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(0.5 * (xe1 + xe2), y5 - dy1 + listaDeDados[8], 0), listaDeDados[0], listaDeDados[8], 0, cores[5], espessuras[3]);
        
                acBlkTblRec.AppendEntity(blocoVF);
                acTrans.AddNewlyCreatedDBObject(blocoVF, true);

                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x5, y5 - dy1 + listaDeDados[8], xp, y5 - dy1 + listaDeDados[8], -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xp, y5 - dy1 + listaDeDados[8], xp + listaDeDados[0], y5 - dy1 + listaDeDados[8], -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xp + listaDeDados[0], y5 - dy1 + listaDeDados[8], x2, y5 - dy1 + listaDeDados[8], -10);

                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x5, y5 - dy1, xe1 - (0.5 * listaDeDados[2]), y5 - dy1, 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe1 - (0.5 * listaDeDados[2]), y5 - dy1, xe1 + (0.5 * listaDeDados[2]), y5 - dy1, 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe1 + (0.5 * listaDeDados[2]), y5 - dy1, xe2 - (0.5 * listaDeDados[2]), y5 - dy1, 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe2 - (0.5 * listaDeDados[2]), y5 - dy1, xe2 + (0.5 * listaDeDados[2]), y5 - dy1, 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe2 + (0.5 * listaDeDados[2]), y5 - dy1, x2, y5 - dy1, 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x5, y5 - dy1, xe1, y5 - dy1, 25 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe1, y5 - dy1, xe2, y5 - dy1, 25 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe2, y5 - dy1, x2, y5 - dy1, 25 + (1.25 * listaDeDados[2]));



                //criar bloco - vista frontal com as armaduras
                double nx = x5 + (2 * largura);
                Polyline blocoVFA = constroiPolilinhaBlocoRetangular(nx, y5 - dy1, nx + largura, y5 - dy1, nx + largura, y5 - dy1 + alturaB, nx, y5 - dy1 + alturaB, cores[9]);
                blocoVFA.LineWeight = espessuras[0];
                blocoVFA.Layer = "VISTA";
                inserirEstacas1(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(xe1 + (2 * largura), y5 - dy1 + listaDeDados[4], 0), listaDeDados[2], 0);
                inserirEstacas1(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(xe2 + (2 * largura), y5 - dy1 + listaDeDados[4], 0), listaDeDados[2], 0);
                constroiPilar(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d((2 * largura) + (0.5 * (xe1 + xe2)), y5 - dy1 + listaDeDados[8], 0), listaDeDados[0], listaDeDados[8], 0, cores[9], espessuras[0]);
        
                //blocoVFA.Closed = true;
                acBlkTblRec.AppendEntity(blocoVFA);
                acTrans.AddNewlyCreatedDBObject(blocoVFA, true);
        
                double largura1 = y3 - y0;
        
                //criar bloco - vista lateral
                double dx1 = 2.1 * a;
                Polyline blocoVL = constroiPolilinhaBlocoRetangular(x0 + dx1, y0, x0 + dx1 + alturaB, y0, x0 + dx1 + alturaB, y0 + largura1, x0 + dx1, y0 + largura1, cores[2]);
                blocoVL.LineWeight = espessuras[3];
                blocoVL.Layer = "CONTORNO FUNDACAO";
                inserirEstacas1(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(x0 + dx1 + alturaB - listaDeDados[4], ye1, 0), listaDeDados[2], 1.5708);
                inserirEstacas1(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(x0 + dx1 + alturaB - listaDeDados[4], ye3, 0), listaDeDados[2], 1.5708);
                constroiPilar(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(x0 + dx1, yp + (0.5 * listaDeDados[1]), 0), listaDeDados[1], listaDeDados[8], 1.5708, cores[5], espessuras[3]);
        
                acBlkTblRec.AppendEntity(blocoVL);
                acTrans.AddNewlyCreatedDBObject(blocoVL, true);

                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1, y0, x0 + dx1, yp, -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1, yp, x0 + dx1, yp + listaDeDados[1], -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1, yp + listaDeDados[1], x0 + dx1, y3, -10);

                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, y0, x0 + dx1 + alturaB, ye1-(0.5*listaDeDados[2]), 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, ye1 - (0.5 * listaDeDados[2]), x0 + dx1 + alturaB, ye1 + (0.5 * listaDeDados[2]), 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, ye1 + (0.5 * listaDeDados[2]), x0 + dx1 + alturaB, ye3 - (0.5 * listaDeDados[2]), 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, ye3 - (0.5 * listaDeDados[2]), x0 + dx1 + alturaB, ye3 + (0.5 * listaDeDados[2]), 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, ye3 + (0.5 * listaDeDados[2]), x0 + dx1 + alturaB, y3, 10 + (1.25 * listaDeDados[2]));

                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, y0, x0 + dx1 + alturaB, ye1, 25 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, ye1, x0 + dx1 + alturaB, ye3, 25 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, ye3, x0 + dx1 + alturaB, y3, 25 + (1.25 * listaDeDados[2]));

                //criar bloco - vista lateral armadura
                double dx2 = 2.1 * a;
                Polyline blocoVLA = constroiPolilinhaBlocoRetangular(x0 + dx1 + dx2, y0, x0 + dx1 + dx2 + alturaB, y0, x0 + dx1 + dx2 + alturaB, y0 + largura1, x0 + dx1 + dx2, y0 + largura1, cores[9]);
                blocoVLA.LineWeight = espessuras[0];
                blocoVLA.Layer = "VISTA";
                inserirEstacas1(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(x0 + dx1 + dx2 + alturaB - listaDeDados[4], ye1, 0), listaDeDados[2], 1.5708);
                inserirEstacas1(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(x0 + dx1 + dx2 + alturaB - listaDeDados[4], ye3, 0), listaDeDados[2], 1.5708);
                constroiPilar(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(x0 + dx1 + dx2, yp + (0.5 * listaDeDados[1]), 0), listaDeDados[1], listaDeDados[8], 1.5708, cores[9], espessuras[0]);
        
                acBlkTblRec.AppendEntity(blocoVLA);
                acTrans.AddNewlyCreatedDBObject(blocoVLA, true);
        
                inserirArmadurasB3VS(acCurDb, acLyrTbl, acTrans, acBlkTblRec, new Point3d(x0 + 1.5 * (dx1 + dx2), y0, 0), listaDeDados[2], a, b, listaDeDados[5], listaDeDados[12], listaDeDados[9], listaDeDados[3]);

                dynamic acadApp = Autodesk.AutoCAD.ApplicationServices.Application.AcadApplication;
                acadApp.ZoomExtents();

                acTrans.Commit();
            }
        }
        
        //DESENHAR BLOCO COM 4 ESTACAS
        public void gerarDesenhos4Estacas(List<double> listaDeDados)
        {
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
        
            //PONTO DE INSERÇÃO DOS DESENHOS
            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");
        
            pPtOpts.Message = "\nSelecione o ponto onde deseja inserir os desenhos: ";
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point3d ptInicio = pPtRes.Value;
        
            double x0 = ptInicio.X;
            double y0 = ptInicio.Y;
            double dx = listaDeDados[6];
            double dy = listaDeDados[7];
            double aux = 0;
            double aux1 = 0;
        
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;

                criarLayers(acTrans, acLyrTbl);

                //criar bloco - vista superior
                Polyline blocoVS = constroiPolilinhaBlocoRetangular(x0, y0, x0 + dx, y0, x0 + dx, y0 + dy, x0, y0 + dy, cores[2]);
                blocoVS.LineWeight = espessuras[3];
                blocoVS.Layer = "CONTORNO FUNDACAO";
        
                double xp = x0 + (0.5 * dx) - (0.5 * listaDeDados[0]);
                double yp = y0 + (0.5 * dy) - (0.5 * listaDeDados[1]);
                double dpx = listaDeDados[0];
                double dpy = listaDeDados[1];
        
                //criar pilar - vista superior
                Polyline pilarVS = constroiPolilinhaBlocoRetangular(xp, yp, xp + listaDeDados[0], yp, xp + listaDeDados[0], yp + listaDeDados[1], xp, yp + listaDeDados[1], cores[5]);
                pilarVS.LineWeight = espessuras[3];
                pilarVS.Layer = "CONTORNO PILAR";
        
                if (listaDeDados[6] < listaDeDados[7])
                {
                    aux = dx;
                    dx = dy;
                    dy = aux;
                    aux1 = dpx;
                    dpx = dpy;
                    dpy = aux1;
        
                    //Rotacionar o conjunto, caso necessário
                    Matrix3d curUCSMatrix = acDoc.Editor.CurrentUserCoordinateSystem;
                    CoordinateSystem3d curUCS = curUCSMatrix.CoordinateSystem3d;
        
                    blocoVS.TransformBy(Matrix3d.Rotation(1.5708, curUCS.Zaxis, ptInicio));
                    pilarVS.TransformBy(Matrix3d.Rotation(1.5708, curUCS.Zaxis, ptInicio));
        
                    //translacionar o bloco + pilar para ajustar a posição
                    Point3d ptBlocoVS = new Point3d(x0, y0, 0);
                    Vector3d blocoVSVec3d = ptBlocoVS.GetVectorTo(new Point3d(x0, y0 + dy, 0));
                    blocoVS.TransformBy(Matrix3d.Displacement(blocoVSVec3d));
        
                    Point3d ptPilarVS = new Point3d(xp, yp, 0);
                    Vector3d pilarVSVec3d = ptPilarVS.GetVectorTo(new Point3d(xp, yp + dy, 0));
                    pilarVS.TransformBy(Matrix3d.Displacement(pilarVSVec3d));
        
                }
        
                acBlkTblRec.AppendEntity(blocoVS);
                acTrans.AddNewlyCreatedDBObject(blocoVS, true);
                acBlkTblRec.AppendEntity(pilarVS);
                acTrans.AddNewlyCreatedDBObject(pilarVS, true);
        
                double xe1 = x0 + (0.5 * (dx - listaDeDados[5]));
                double ye1 = y0 + listaDeDados[3] + (0.5 * listaDeDados[2]);
                double xe2 = x0 + dx - (0.5 * (dx - listaDeDados[5]));
                double ye2 = ye1;
                double xe3e = xe1;
                double ye3e = y0 + dy - listaDeDados[3] - (0.5 * listaDeDados[2]);
                double xe4e = xe2;
                double ye4e = ye3e;
        
                //criar estacas
                inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe1, ye1, listaDeDados[2]);
                inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe2, ye2, listaDeDados[2]);
                inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe3e, ye3e, listaDeDados[2]);
                inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe4e, ye4e, listaDeDados[2]);

                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, y0, x0, ye1, -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, ye1, x0, ye4e, -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, ye4e, x0, y0 + dy, -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, y0, x0, y0 + dy, -25);

                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, y0 + dy, x0+dx, y0 + dy, -25);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, y0 + dy, xe1, y0 + dy, -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe1, y0 + dy, xe2, y0 + dy, -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe2, y0 + dy, x0 + dx, y0 + dy, -10);

                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe1, y0, xe1, ye1 - (0.5 * listaDeDados[2]), 10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, ye1, xe1 - (0.5 * listaDeDados[2]), ye1, (0.5 * listaDeDados[2]) + 2);

                //criar bloco - vista frontal
                double dy1 = 205 + listaDeDados[8];
                Polyline blocoVF = constroiPolilinhaBlocoRetangular(x0, y0 - dy1, x0 + dx, y0 - dy1, x0 + dx, y0 - dy1 + listaDeDados[8], x0, y0 - dy1 + listaDeDados[8], cores[2]);
                blocoVF.LineWeight = espessuras[3];
                blocoVF.Layer = "CONTORNO FUNDACAO";
                acBlkTblRec.AppendEntity(blocoVF);
                acTrans.AddNewlyCreatedDBObject(blocoVF, true);
        
                //inserção de estaca na vista frontal
                double xe3 = xe1;
                double ye3 = y0 - dy1 + listaDeDados[4];
                double xe4 = xe1 + listaDeDados[5];
                double ye4 = ye3;
                Point3d pt3 = new Point3d(xe3, ye3, 0);
                Point3d pt4 = new Point3d(xe4, ye4, 0);
                //int fatorEscala = Convert.ToInt16(listaDeDados[2]);
        
                inserirEstacas1(acDoc, acLyrTbl, acTrans, acBlkTblRec, pt3, listaDeDados[2], 0);
                inserirEstacas1(acDoc, acLyrTbl, acTrans, acBlkTblRec, pt4, listaDeDados[2], 0);
                constroiPilar(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(0.5 * (xe3 + xe4), y0 - dy1 + listaDeDados[8], 0), dpx, listaDeDados[8], 0, cores[5], espessuras[3]);


                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, y0 - dy1 + listaDeDados[8], x0 + (0.5 * dx) - (0.5 * dpx), y0 - dy1 + listaDeDados[8], -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + (0.5 * dx) - (0.5 * dpx), y0 - dy1 + listaDeDados[8], x0 + (0.5 * dx) + (0.5 * dpx), y0 - dy1 + listaDeDados[8], -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + (0.5 * dx) + (0.5 * dpx), y0 - dy1 + listaDeDados[8], x0 + dx, y0 - dy1 + listaDeDados[8], -10);


                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, y0 - dy1, xe3 - (0.5 * listaDeDados[2]), y0 - dy1, 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe3 - (0.5 * listaDeDados[2]), y0 - dy1, xe3 + (0.5 * listaDeDados[2]), y0 - dy1, 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe3 + (0.5 * listaDeDados[2]), y0 - dy1, xe4 - (0.5 * listaDeDados[2]), y0 - dy1, 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe4 - (0.5 * listaDeDados[2]), y0 - dy1, xe4 + (0.5 * listaDeDados[2]), y0 - dy1, 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe4 + (0.5 * listaDeDados[2]), y0 - dy1, x0 + dx, y0 - dy1, 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0, y0 - dy1, xe3, y0 - dy1, 25 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe3, y0 - dy1, xe4, y0 - dy1, 25 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, xe4, y0 - dy1, x0 + dx, y0 - dy1, 25 + (1.25 * listaDeDados[2]));

                // criar bloco - vista lateral
                double dx1 = 242 + dx;
                double alturaB = listaDeDados[8];
                Polyline blocoVL = constroiPolilinhaBlocoRetangular(x0 + dx1, y0, x0 + dx1 + listaDeDados[8], y0, x0 + dx1 + listaDeDados[8], y0 + dy, x0 + dx1, y0 + dy, cores[2]);
                blocoVL.LineWeight = espessuras[3];
                blocoVL.Layer = "CONTORNO FUNDACAO";
                acBlkTblRec.AppendEntity(blocoVL);
                acTrans.AddNewlyCreatedDBObject(blocoVL, true);
        
                double xe5 = x0 + dx1 + listaDeDados[8] - listaDeDados[4];
                double ye5 = ye1;
                Point3d pt5 = new Point3d(xe5, ye5, 0);
                inserirEstacas1(acDoc, acLyrTbl, acTrans, acBlkTblRec, pt5, listaDeDados[2], 1.5708);
                inserirEstacas1(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(xe5, ye3e, 0), listaDeDados[2], 1.5708);
                constroiPilar(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(x0 + dx1, y0 + (dy * 0.5), 0), dpy, listaDeDados[8], 1.5708, cores[5], espessuras[3]);

                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1, y0, x0 + dx1, yp, -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1, yp, x0 + dx1, yp + listaDeDados[1], -10);
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1, yp + listaDeDados[1], x0 + dx1, y0+dy, -10);

                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, y0, x0 + dx1 + alturaB, ye5-(0.5*listaDeDados[2]), 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, ye5 - (0.5 * listaDeDados[2]), x0 + dx1 + alturaB, ye5 + (0.5 * listaDeDados[2]), 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, ye5 + (0.5 * listaDeDados[2]), x0 + dx1 + alturaB, ye3e - (0.5 * listaDeDados[2]), 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, ye3e - (0.5 * listaDeDados[2]), x0 + dx1 + alturaB, ye3e + (0.5 * listaDeDados[2]), 10 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, ye3e + (0.5 * listaDeDados[2]), x0 + dx1 + alturaB, y0 + dy, 10 + (1.25 * listaDeDados[2]));

                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, y0, x0 + dx1 + alturaB, ye5, 25 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, ye5, x0 + dx1 + alturaB, ye3e, 25 + (1.25 * listaDeDados[2]));
                colocarDimensaoLinear(acCurDb, acTrans, acBlkTblRec, x0 + dx1 + alturaB, ye3e, x0 + dx1 + alturaB, y0 + dy, 25 + (1.25 * listaDeDados[2]));

                //criar bloco - vista lateral com armaduras
                double dx2 = dx1 + listaDeDados[8] + 192;
                Polyline blocoVLA = constroiPolilinhaBlocoRetangular(x0 + dx2, y0, x0 + dx2 + listaDeDados[8], y0, x0 + dx2 + listaDeDados[8], y0 + dy, x0 + dx2, y0 + dy, cores[9]);
                blocoVLA.LineWeight = espessuras[0];
                blocoVLA.Layer = "VISTA";
                acBlkTblRec.AppendEntity(blocoVLA);
                acTrans.AddNewlyCreatedDBObject(blocoVLA, true);
                double xe6 = x0 + dx2 + listaDeDados[8] - listaDeDados[4];
                double ye6 = ye1;
                inserirEstacaVArmadura(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(xe6, ye6, 0), listaDeDados[2], 1.5708, listaDeDados[4]);
                inserirEstacaVArmadura(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(xe6, ye3e, 0), listaDeDados[2], 1.5708, listaDeDados[4]);
                constroiPilar(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(x0 + dx2, y0 + (dy * 0.5), 0), dpy, listaDeDados[8], 1.5708, cores[9], espessuras[0]);
        
                //criar bloco - vista frontal armadura
                double dx3 = dx1;
                Polyline blocoVFA = constroiPolilinhaBlocoRetangular(x0 + dx3, y0 - dy1, x0 + dx3 + dx, y0 - dy1, x0 + dx3 + dx, y0 - dy1 + listaDeDados[8], x0 + dx3, y0 - dy1 + listaDeDados[8], cores[9]);
                blocoVFA.LineWeight = espessuras[0];
                blocoVFA.Layer = "VISTA";
                acBlkTblRec.AppendEntity(blocoVFA);
                acTrans.AddNewlyCreatedDBObject(blocoVFA, true);
                double xe7 = xe1 + dx3;
                double ye7 = ye3;
                inserirEstacaVArmadura(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(xe7, ye7, 0), listaDeDados[2], 0, listaDeDados[4]);
                inserirEstacaVArmadura(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(xe7 + listaDeDados[5], ye7, 0), listaDeDados[2], 0, listaDeDados[4]);
                constroiPilar(acDoc, acLyrTbl, acTrans, acBlkTblRec, new Point3d(xe7 + (0.5 * listaDeDados[5]), ye7 + listaDeDados[8] - listaDeDados[4], 0), dpx, listaDeDados[8], 0, cores[9], espessuras[0]);

                //criar bloco - vista superior armadura
                double dy2 = dy1 + dy + 170;
                Polyline blocoVSA = constroiPolilinhaBlocoRetangular(x0, y0 - dy2, x0 + dx, y0 - dy2, x0 + dx, y0 + dy - dy2, x0, y0 + dy - dy2, cores[9]);
                blocoVSA.LineWeight = espessuras[0];
                blocoVSA.Layer = "VISTA";
                acBlkTblRec.AppendEntity(blocoVSA);
                acTrans.AddNewlyCreatedDBObject(blocoVSA, true);
        
                inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe1, y0 - dy2 + listaDeDados[3] + (0.5 * listaDeDados[2]), listaDeDados[2]);
                inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe2, y0 - dy2 + listaDeDados[3] + (0.5 * listaDeDados[2]), listaDeDados[2]);
                inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe3e, y0 - dy2 + listaDeDados[3] + listaDeDados[5] + (0.5 * listaDeDados[2]), listaDeDados[2]);
                inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe4e, y0 - dy2 + listaDeDados[3] + listaDeDados[5] + (0.5 * listaDeDados[2]), listaDeDados[2]);
        
                inserirArmadurasB4VS(acDoc, acTrans, acBlkTblRec, new Point3d(xe1, y0 - dy2 + listaDeDados[3] + (0.5 * listaDeDados[2]), 0), listaDeDados[2], listaDeDados[7], listaDeDados[5], listaDeDados[13], listaDeDados[9]);

                dynamic acadApp = Autodesk.AutoCAD.ApplicationServices.Application.AcadApplication;
                acadApp.ZoomExtents();

                // Salva o novo objeto para o banco de dados
                acTrans.Commit();
            }
        }

        //DESENHAR CÍRCULO PARA REPRESENTAR AS ESTACAS
        private Circle constroiCirculo(double xc, double yc, double raio)
        {
            Circle circulo = new Circle();
            circulo.Center = new Point3d(xc, yc, 0);
            circulo.Radius = raio;
            return circulo;
        }

        //DESENHAR RETÂNGULOS FEITOS POR POLILINHAS
        private Polyline constroiPolilinhaBlocoRetangular(double x0, double y0, double x1, double y1, double x2, double y2, double x3, double y3, Color cor)
        {
            Polyline polilinha = new Polyline();
            polilinha.SetDatabaseDefaults();
            polilinha.AddVertexAt(0, new Point2d(x0, y0), 0, 0, 0);
            polilinha.AddVertexAt(1, new Point2d(x1, y1), 0, 0, 0);
            polilinha.AddVertexAt(2, new Point2d(x2, y2), 0, 0, 0);
            polilinha.AddVertexAt(3, new Point2d(x3, y3), 0, 0, 0);
            polilinha.AddVertexAt(4, new Point2d(x0, y0), 0, 0, 0);
            polilinha.Color = cor;
            polilinha.Closed = true;
            return polilinha;
        }

        //DESENHAR POLÍGONO HEXAGONAL QUE REPRESENTA O BLOCO DE 3 ESTACAS 
        private Polyline constroiBlocoHexagonal(double x0, double y0, double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, double x5, double y5, Color cor)
        {
            Polyline bloco = new Polyline();
            bloco.SetDatabaseDefaults();
            bloco.AddVertexAt(0, new Point2d(x0, y0), 0, 0, 0);
            bloco.AddVertexAt(1, new Point2d(x1, y1), 0, 0, 0);
            bloco.AddVertexAt(2, new Point2d(x2, y2), 0, 0, 0);
            bloco.AddVertexAt(3, new Point2d(x3, y3), 0, 0, 0);
            bloco.AddVertexAt(4, new Point2d(x4, y4), 0, 0, 0);
            bloco.AddVertexAt(5, new Point2d(x5, y5), 0, 0, 0);
            bloco.AddVertexAt(6, new Point2d(x0, y0), 0, 0, 0);
            bloco.Color = cor;
            bloco.Closed = true;
            return bloco;
        }

        //COLOCAR AS DIMENSÕES PARA MEDIR O BLOCO
        //d positivo para cota abaixo e negativo para cota acima. 
        private void colocarDimensaoLinear(Database acCurDb, Transaction acTrans, BlockTableRecord acBlkTblRec, double xi, double yi, double xf, double yf, double d)
        {
            Point3d ptIni = new Point3d(xi, yi, 0);
            Point3d ptFim = new Point3d(xf, yf, 0);

            double a = (xf - xi) * (xf - xi);
            double b = (yf - yi) * (yf - yi);
            double L = Math.Sqrt(a + b);
            L = Math.Round(L, 4);

            double cos = (xf - xi) / L;
            cos = Math.Abs(cos);
            double sen = (yf - yi) / L;
            sen = Math.Abs(sen);

            double xmedio = (ptIni.X + ptFim.X) * 0.5;
            double ymedio = (ptIni.Y + ptFim.Y) * 0.5;
            Point3d pontoMedio = new Point3d(xmedio, ymedio, 0);

            double xp = xmedio + (d * sen);
            double yp = ymedio - (d * cos);
            Point3d ptPosicao = new Point3d(xp, yp, 0);

            AlignedDimension dimensao = new AlignedDimension();
            dimensao.SetDatabaseDefaults();
            dimensao.XLine1Point = ptIni;
            dimensao.XLine2Point = ptFim;
            dimensao.Dimasz = dimensao.Dimasz * 50;
            dimensao.Dimtxt = dimensao.Dimtxt * 25;
            dimensao.DimensionStyle = acCurDb.Dimstyle;
            dimensao.Dimdec = 0;
            dimensao.DimLinePoint = ptPosicao;
            dimensao.Color = cores[2];
            dimensao.LineWeight = LineWeight.LineWeight009;

            acBlkTblRec.AppendEntity(dimensao);
            acTrans.AddNewlyCreatedDBObject(dimensao, true);
        }

        //INSERIR ESTACAS
        private void inserirEstacas1(Document acDoc, LayerTable acLyrTbl,Transaction acTrans, BlockTableRecord acBlkTblRec, Point3d ptoInsercao, double diametroEstaca, double anguloRotacao)
        {
            //Desenhando a polilinha que representa o bloco
            double x0 = ptoInsercao.X; double y0 = ptoInsercao.Y;
            double x1 = x0 - (diametroEstaca * 0.5); double y1 = y0;
            double x2 = x1; double y2 = y0 - diametroEstaca;
            double x3 = x0 + (diametroEstaca * 0.5); double y3 = y2;
            double x4 = x3; double y4 = y0;

            Polyline polilinha = new Polyline();
            polilinha.SetDatabaseDefaults();
            polilinha.AddVertexAt(0, new Point2d(x2, y2), 0, 0, 0);
            polilinha.AddVertexAt(1, new Point2d(x1, y1), 0, 0, 0);
            polilinha.AddVertexAt(2, new Point2d(x4, y4), 0, 0, 0);
            polilinha.AddVertexAt(3, new Point2d(x3, y3), 0, 0, 0);
            polilinha.Color = cores[9];
            polilinha.LineWeight = espessuras[0];
            polilinha.Layer = "VISTA";

            double xc1 = x0 - (diametroEstaca * 0.25); double yc1 = y2;
            double xc2 = x0 + (diametroEstaca * 0.25); double yc2 = y2;
            double raio = (diametroEstaca * 0.25);

            Circle c1 = new Circle();
            c1.Center = new Point3d(xc1, yc1, 0);
            c1.Radius = raio;
            c1.Color = cores[9];
            c1.LineWeight = espessuras[0];
            c1.Layer = "VISTA";
            //Circle c2 = new Circle();
            //c2.Center = new Point3d(xc2, yc2, 0);
            Point3d p1 = new Point3d(x0, y3, 0);
            Point3d p2 = new Point3d(xc2, (y3 - (diametroEstaca * 0.25)), 0);
            Point3d p3 = new Point3d(x3, y3, 0);
            Point3d ptCentroC2 = new Point3d(xc2, yc2, 0);

            // criar a CircularArc3d
            CircularArc3d carc = new CircularArc3d(p1, p2, p3);

            Arc arc = null;

            // converter CircularArc3d para Arc
            Point3d cpt = carc.Center;
            Vector3d normal = carc.Normal;
            Vector3d refVec = carc.ReferenceVector;
            Plane plan = new Plane(cpt, normal);
            double ang = refVec.AngleOnPlane(plan);
            arc = new Arc(cpt, normal, carc.Radius, carc.StartAngle + ang, carc.EndAngle + ang);
            arc.Color = cores[9];
            arc.LineWeight = espessuras[0];
            arc.Layer = "VISTA";
            arc.SetDatabaseDefaults();

            acBlkTblRec.AppendEntity(polilinha);
            acTrans.AddNewlyCreatedDBObject(polilinha, true);
            acBlkTblRec.AppendEntity(c1);
            acTrans.AddNewlyCreatedDBObject(c1, true);
            acBlkTblRec.AppendEntity(arc);
            acTrans.AddNewlyCreatedDBObject(arc, true);

            //Rotacionar o conjunto, caso necessário
            Matrix3d curUCSMatrix = acDoc.Editor.CurrentUserCoordinateSystem;
            CoordinateSystem3d curUCS = curUCSMatrix.CoordinateSystem3d;

            polilinha.TransformBy(Matrix3d.Rotation(anguloRotacao, curUCS.Zaxis, ptoInsercao));
            c1.TransformBy(Matrix3d.Rotation(anguloRotacao, curUCS.Zaxis, ptoInsercao));
            arc.TransformBy(Matrix3d.Rotation(anguloRotacao, curUCS.Zaxis, ptoInsercao));
        }

        //INSERE PILAR
        private void constroiPilar(Document acDoc, LayerTable acLyrTbl, Transaction acTrans, BlockTableRecord acBlkTblRec, Point3d ptoInsercao, double b, double h, double anguloRotacao, Color cor, LineWeight espessura)
        {
            //h pode ser a mesma altura do bloco

            double x0 = ptoInsercao.X; double y0 = ptoInsercao.Y;
            double x1 = x0 - (b * 0.5); double y1 = y0;
            double x2 = x0 + (b * 0.5); double y2 = y0;

            Line linha1 = new Line(new Point3d(x1, y1, 0), new Point3d(x1, y1 + h, 0));
            linha1.Color = cor;
            linha1.LineWeight = espessura;
            if (cor == cores[5]) { linha1.Layer = "CONTORNO PILAR"; }
            if (cor == cores[9]) { linha1.Layer = "VISTA"; }
            Line linha2 = new Line(new Point3d(x2, y2, 0), new Point3d(x2, y2 + h, 0));
            linha2.Color = cor;
            linha2.LineWeight = espessura;

            double a = (b * 0.25);
            Polyline linhaCorte = new Polyline();
            linhaCorte.SetDatabaseDefaults();
            linhaCorte.AddVertexAt(0, new Point2d(x1 - a, y1 + h), 0, 0, 0);
            linhaCorte.AddVertexAt(1, new Point2d(x1 + a, y1 + h), 0, 0, 0);
            linhaCorte.AddVertexAt(2, new Point2d(x1 + (a * 1.5), y1 + h - (a * Math.Sqrt(3) * 0.5)), 0, 0, 0);
            linhaCorte.AddVertexAt(3, new Point2d(x1 + (a * 2.5), y1 + h + (a * Math.Sqrt(3) * 0.5)), 0, 0, 0);
            linhaCorte.AddVertexAt(4, new Point2d(x1 + (a * 3), y1 + h), 0, 0, 0);
            linhaCorte.AddVertexAt(5, new Point2d(x1 + (a * 5), y1 + h), 0, 0, 0);
            linhaCorte.Color = cores[9];
            linhaCorte.LineWeight = espessuras[0];
            linhaCorte.Layer = "VISTA";

            //Rotacionar o conjunto, caso necessário
            Matrix3d curUCSMatrix = acDoc.Editor.CurrentUserCoordinateSystem;
            CoordinateSystem3d curUCS = curUCSMatrix.CoordinateSystem3d;

            linha1.TransformBy(Matrix3d.Rotation(anguloRotacao, curUCS.Zaxis, ptoInsercao));
            linha2.TransformBy(Matrix3d.Rotation(anguloRotacao, curUCS.Zaxis, ptoInsercao));
            linhaCorte.TransformBy(Matrix3d.Rotation(anguloRotacao, curUCS.Zaxis, ptoInsercao));

            acBlkTblRec.AppendEntity(linha1);
            acTrans.AddNewlyCreatedDBObject(linha1, true);
            acBlkTblRec.AppendEntity(linha2);
            acTrans.AddNewlyCreatedDBObject(linha2, true);
            acBlkTblRec.AppendEntity(linhaCorte);
            acTrans.AddNewlyCreatedDBObject(linhaCorte, true);

        }

        //INSERE ESTACAS PARA ARMADURAS
        private void inserirEstacaVArmadura(Document acDoc, LayerTable acLyrTbl, Transaction acTrans, BlockTableRecord acBlkTblRec, Point3d ptoInsercao, double diametroEstaca, double anguloRotacao, double embutimento)
        {
            //h pode ser a mesma altura do bloco

            double x0 = ptoInsercao.X; double y0 = ptoInsercao.Y;
            double x1 = x0 - (diametroEstaca * 0.5); double y1 = y0;
            double x2 = x0 + (diametroEstaca * 0.5); double y2 = y0;
            double h = (diametroEstaca * 0.5);
            double x3 = x1; double y3 = y1 - h;
            double x4 = x2; double y4 = y2 - h;
            double x5 = x0 - (diametroEstaca * 0.125); double y5 = y3;
            double x6 = x0; double y6 = y3 + (diametroEstaca * 0.125);
            double x7 = x6; double y7 = y3 - (diametroEstaca * 0.125);
            double x8 = x6 + (diametroEstaca * 0.125); double y8 = y3;

            Polyline estaca = new Polyline();
            estaca.SetDatabaseDefaults();
            estaca.AddVertexAt(0, new Point2d(x1, y1), 0, 0, 0);
            estaca.AddVertexAt(1, new Point2d(x3, y3), 0, 0, 0);
            estaca.AddVertexAt(2, new Point2d(x5, y5), 0, 0, 0);
            estaca.AddVertexAt(3, new Point2d(x6, y6), 0, 0, 0);
            estaca.AddVertexAt(4, new Point2d(x7, y7), 0, 0, 0);
            estaca.AddVertexAt(5, new Point2d(x8, y8), 0, 0, 0);
            estaca.AddVertexAt(6, new Point2d(x4, y4), 0, 0, 0);
            estaca.AddVertexAt(7, new Point2d(x2, y2), 0, 0, 0);
            estaca.AddVertexAt(8, new Point2d(x1, y1), 0, 0, 0);
            estaca.Color = cores[9];
            estaca.LineWeight = espessuras[0];
            estaca.Layer = "VISTA";

            estaca.Closed = true;
            

            Line linha = new Line(new Point3d(x6, y6, 0), new Point3d(x6, y3 + h - embutimento, 0));

            //Rotacionar o conjunto, caso necessário
            Matrix3d curUCSMatrix = acDoc.Editor.CurrentUserCoordinateSystem;
            CoordinateSystem3d curUCS = curUCSMatrix.CoordinateSystem3d;
            estaca.TransformBy(Matrix3d.Rotation(anguloRotacao, curUCS.Zaxis, ptoInsercao));
            linha.TransformBy(Matrix3d.Rotation(anguloRotacao, curUCS.Zaxis, ptoInsercao));
            linha.Color = cores[9];
            linha.LineWeight = espessuras[0];
            linha.Layer = "VISTA";

            acBlkTblRec.AppendEntity(estaca);
            acTrans.AddNewlyCreatedDBObject(estaca, true);
            acBlkTblRec.AppendEntity(linha);
            acTrans.AddNewlyCreatedDBObject(linha, true);

        }

        //INSERE ESTACAS VISTA SUPERIOR
        public void inserirEstacaVistaSuperior(Database acCurDb, LayerTable acLyrTbl, Transaction acTrans, BlockTableRecord acBlkTblRec, double x0, double y0, double diametroEstaca)
        {
            //double x0 = ptoInsercao.X;
            //double y0 = ptoInsercao.Y;
            Circle estaca1 = constroiCirculo(x0, y0, (0.5 * diametroEstaca));
            estaca1.Color = cores[9];
            estaca1.LineWeight = espessuras[0];
            estaca1.Layer = "VISTA";
            Line linhaHorizontal = new Line(new Point3d(x0 - (0.75 * diametroEstaca), y0, 0), new Point3d(x0 + (0.75 * diametroEstaca), y0, 0));
            Line linhaVertical = new Line(new Point3d(x0, y0 - (0.75 * diametroEstaca), 0), new Point3d(x0, y0 + (0.75 * diametroEstaca), 0));
            linhaHorizontal.Color = cores[9];
            linhaHorizontal.LineWeight = espessuras[0];
            linhaHorizontal.Layer = "VISTA";


            LinetypeTable acLineTypTbl;
            acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId,
                                                OpenMode.ForRead) as LinetypeTable;
            
            string sLineTypName = "OCULTA";
            string filename = "acad.lin";
            int idioma = 0; //0 PARA PORTUGUÊS E 1 PARA INGLÊS
            
            try
            {
                string path = HostApplicationServices.Current.FindFile(filename, acCurDb, FindFileHint.Default);
                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    // Load Linetype
                    acCurDb.LoadLineTypeFile(sLineTypName, filename);
                    linhaHorizontal.Linetype = sLineTypName;
                    linhaVertical.Linetype = sLineTypName;
                }
                //MessageBox.Show("Passou aqui","Oi");
            }
            catch
            {
                //MessageBox.Show("Não foi possível carregar a Linetype HIDDEN", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                idioma = 1;
            }
            if (idioma == 1)
            {
                sLineTypName = "HIDDEN";
                try
                {
                    string path = HostApplicationServices.Current.FindFile(filename, acCurDb, FindFileHint.Default);
                    if (acLineTypTbl.Has(sLineTypName) == false)
                    {
                        // Load Linetype
                        acCurDb.LoadLineTypeFile(sLineTypName, path);
                        linhaHorizontal.Linetype = sLineTypName;
                        linhaVertical.Linetype = sLineTypName;
                    }
                }
                catch
                {
                    MessageBox.Show("Não foi possível carregar a Linetype HIDDEN", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            linhaVertical.Color = cores[9];
            linhaVertical.LineWeight = espessuras[0];
            linhaVertical.Layer = "VISTA";

            acBlkTblRec.AppendEntity(estaca1);
            acTrans.AddNewlyCreatedDBObject(estaca1, true);
            acBlkTblRec.AppendEntity(linhaHorizontal);
            acTrans.AddNewlyCreatedDBObject(linhaHorizontal, true);
            acBlkTblRec.AppendEntity(linhaVertical);
            acTrans.AddNewlyCreatedDBObject(linhaVertical, true);

            
        }

        //CRIA AS LAYERS PARA O DESENHO DOS BLOCOS
        public void criarLayers(Transaction acTrans, LayerTable acLyrTbl)
        {
            if (acLyrTbl.Has("CONTORNO FUNDACAO") == false)
            {
                using (LayerTableRecord acLyrTblRec1 = new LayerTableRecord())
                {
                    acLyrTblRec1.Name = "CONTORNO FUNDACAO";
                    acLyrTblRec1.LineWeight = LineWeight.LineWeight040;
                    acLyrTblRec1.Color = cores[2];
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec1);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec1, true);
                }
            }

            if (acLyrTbl.Has("CONTORNO PILAR") == false)
            {
                using (LayerTableRecord acLyrTblRec2 = new LayerTableRecord())
                {
                    acLyrTblRec2.Name = "CONTORNO PILAR";
                    acLyrTblRec2.LineWeight = LineWeight.LineWeight040;
                    acLyrTblRec2.Color = cores[5];
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec2);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec2, true);
                }
            }

            if (acLyrTbl.Has("COTA") == false)
            {
                using (LayerTableRecord acLyrTblRec3 = new LayerTableRecord())
                {
                    acLyrTblRec3.Name = "COTA";
                    acLyrTblRec3.LineWeight = LineWeight.LineWeight009;
                    acLyrTblRec3.Color = Color.FromColorIndex(ColorMethod.ByAci, 12);
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec3);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec3, true);
                }
            }

            if (acLyrTbl.Has("HC VIGA PLANTA") == false)
            {
                using (LayerTableRecord acLyrTblRec4 = new LayerTableRecord())
                {
                    acLyrTblRec4.Name = "HC VIGA PLANTA";
                    acLyrTblRec4.LineWeight = LineWeight.LineWeight009;
                    acLyrTblRec4.Color = Color.FromColorIndex(ColorMethod.ByAci, 254);
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec4);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec4, true);
                }
            }

            if (acLyrTbl.Has("LINHA 1") == false)
            {
                using (LayerTableRecord acLyrTblRec5 = new LayerTableRecord())
                {
                    acLyrTblRec5.Name = "LINHA 1";
                    acLyrTblRec5.LineWeight = LineWeight.LineWeight009;
                    acLyrTblRec5.Color = Color.FromColorIndex(ColorMethod.ByAci, 7);
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec5);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec5, true);
                }
            }

            if (acLyrTbl.Has("LINHA 2") == false)
            {
                using (LayerTableRecord acLyrTblRec6 = new LayerTableRecord())
                {
                    acLyrTblRec6.Name = "LINHA 2";
                    acLyrTblRec6.LineWeight = LineWeight.LineWeight009;
                    acLyrTblRec6.Color = Color.FromColorIndex(ColorMethod.ByAci, 212);
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec6);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec6, true);
                }
            }

            if (acLyrTbl.Has("LINHA 3") == false)
            {
                using (LayerTableRecord acLyrTblRec7 = new LayerTableRecord())
                {
                    acLyrTblRec7.Name = "LINHA 3";
                    acLyrTblRec7.LineWeight = LineWeight.LineWeight030;
                    acLyrTblRec7.Color = Color.FromColorIndex(ColorMethod.ByAci, 7);
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec7);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec7, true);
                }
            }

            if (acLyrTbl.Has("LINHA 4") == false)
            {
                using (LayerTableRecord acLyrTblRec8 = new LayerTableRecord())
                {
                    acLyrTblRec8.Name = "LINHA 4";
                    acLyrTblRec8.LineWeight = LineWeight.LineWeight040;
                    acLyrTblRec8.Color = Color.FromColorIndex(ColorMethod.ByAci, 7);
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec8);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec8, true);
                }
            }

            if (acLyrTbl.Has("LINHA DE ARMADURA") == false)
            {
                using (LayerTableRecord acLyrTblRec9 = new LayerTableRecord())
                {
                    acLyrTblRec9.Name = "LINHA DE ARMADURA";
                    acLyrTblRec9.LineWeight = LineWeight.LineWeight050;
                    acLyrTblRec9.Color = Color.FromColorIndex(ColorMethod.ByAci, 7);
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec9);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec9, true);
                }
            }

            if (acLyrTbl.Has("PILAR QUE NASCE") == false)
            {
                using (LayerTableRecord acLyrTblRec10 = new LayerTableRecord())
                {
                    acLyrTblRec10.Name = "PILAR QUE NASCE";
                    acLyrTblRec10.LineWeight = LineWeight.LineWeight009;
                    acLyrTblRec10.Color = Color.FromColorIndex(ColorMethod.ByAci, 82);
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec10);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec10, true);
                }
            }

            if (acLyrTbl.Has("QF TEXTO FERROS") == false)
            {
                using (LayerTableRecord acLyrTblRec11 = new LayerTableRecord())
                {
                    acLyrTblRec11.Name = "QF TEXTO FERROS";
                    acLyrTblRec11.LineWeight = LineWeight.LineWeight030;
                    acLyrTblRec11.Color = Color.FromColorIndex(ColorMethod.ByAci, 7);
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec11);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec11, true);
                }
            }

            if (acLyrTbl.Has("TEXTO 3") == false)
            {
                using (LayerTableRecord acLyrTblRec12 = new LayerTableRecord())
                {
                    acLyrTblRec12.Name = "TEXTO 3";
                    acLyrTblRec12.LineWeight = LineWeight.LineWeight030;
                    acLyrTblRec12.Color = Color.FromColorIndex(ColorMethod.ByAci, 7);
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec12);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec12, true);
                }
            }

            if (acLyrTbl.Has("TEXTO 4") == false)
            {
                using (LayerTableRecord acLyrTblRec13 = new LayerTableRecord())
                {
                    acLyrTblRec13.Name = "TEXTO 4";
                    acLyrTblRec13.LineWeight = LineWeight.LineWeight040;
                    acLyrTblRec13.Color = Color.FromColorIndex(ColorMethod.ByAci, 7);
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec13);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec13, true);
                }
            }

            if (acLyrTbl.Has("TEXTO 5") == false)
            {
                using (LayerTableRecord acLyrTblRec14 = new LayerTableRecord())
                {
                    acLyrTblRec14.Name = "TEXTO 5";
                    acLyrTblRec14.LineWeight = LineWeight.LineWeight050;
                    acLyrTblRec14.Color = Color.FromColorIndex(ColorMethod.ByAci, 7);
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec14);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec14, true);
                }
            }

            if (acLyrTbl.Has("VISTA") == false)
            {
                using (LayerTableRecord acLyrTblRec15 = new LayerTableRecord())
                {
                    acLyrTblRec15.Name = "VISTA";
                    acLyrTblRec15.LineWeight = LineWeight.LineWeight009;
                    acLyrTblRec15.Color = cores[9];
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec15);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec15, true);
                }
            }
            // Save the changes and dispose of the transaction
            //acTrans.Commit();
            
            //return nome;
        }

        //INSERIR CONJUNTO DE ARMADURAS BLOCO DE 4 ESTACAS
        //PONTO DE INSERÇÃO -> CENTRO DA ESTACA INFERIOR ESQUERDA
        private void inserirArmadurasB4VS(Document acDoc, Transaction acTrans, BlockTableRecord acBlkTblRec, Point3d ptoInsercao, double diametroEstaca, double ladoBloco, double distanciaEntreEstacas, double indicaAreaAmpliada, double n1)
        {
            //BARRAS HORIZONTAIS
            double l = diametroEstaca;
            double dx = (0.45 * ladoBloco) - (0.5 * distanciaEntreEstacas);
            double x0 = ptoInsercao.X - dx;

            if (indicaAreaAmpliada == 1) { l = 1.2 * diametroEstaca; }
            double y0 = ptoInsercao.Y - (l * 0.5);
            //double dy = (0.1 * diametroEstaca);
            double d = l / (n1 - 1);
            d = Math.Round(d, 4);
            double y1 = y0;
            for (int i = 0; i < n1; i++)
            {
                Line linha = new Line(new Point3d(x0, y1, 0), new Point3d(x0 + (0.9 * ladoBloco), y1, 0));
                linha.LineWeight = espessuras[4];
                linha.Color = cores[10];
                linha.Layer = "LINHA DE ARMADURA";
                acBlkTblRec.AppendEntity(linha);
                acTrans.AddNewlyCreatedDBObject(linha, true);
                y1 = y1 + d;
            }
            y1 = y0;
            for (int i = 0; i < n1; i++)
            {
                Line linha = new Line(new Point3d(x0, y1 + distanciaEntreEstacas, 0), new Point3d(x0 + (0.9 * ladoBloco), y1 + distanciaEntreEstacas, 0));
                linha.LineWeight = espessuras[4];
                linha.Color = cores[10];
                linha.Layer = "LINHA DE ARMADURA";
                acBlkTblRec.AppendEntity(linha);
                acTrans.AddNewlyCreatedDBObject(linha, true);
                y1 = y1 + d;
            }

            //BARRAS VERTICAIS
            l = diametroEstaca;
            double dy = (0.45 * ladoBloco) - (0.5 * distanciaEntreEstacas);
            y0 = ptoInsercao.Y - dy;
            if (indicaAreaAmpliada == 1) { l = 1.2 * diametroEstaca; }
            x0 = ptoInsercao.X - (l * 0.5);
            
            //double d = l / (n1 - 1);
            //d = Math.Round(d, 4);
            double x1 = x0;
            for (int i = 0; i < n1; i++)
            {
                Line linha = new Line(new Point3d(x1, y0, 0), new Point3d(x1, y0 + (0.9 * ladoBloco), 0));
                linha.LineWeight = espessuras[4];
                linha.Color = cores[10];
                linha.Layer = "LINHA DE ARMADURA";
                acBlkTblRec.AppendEntity(linha);
                acTrans.AddNewlyCreatedDBObject(linha, true);
                x1 = x1 + d;
            }
            x1 = x0;
            for (int i = 0; i < n1; i++)
            {
                Line linha = new Line(new Point3d(x1 + distanciaEntreEstacas, y0, 0), new Point3d(x1 + distanciaEntreEstacas, y0 + (0.9 * ladoBloco), 0));
                linha.LineWeight = espessuras[4];
                linha.Color = cores[10];
                linha.Layer = "LINHA DE ARMADURA";
                acBlkTblRec.AppendEntity(linha);
                acTrans.AddNewlyCreatedDBObject(linha, true);
                x1 = x1 + d;
            }

        }

        //INSERIR CONJUNTO DE ARMADURAS BLOCO DE 2 ESTACAS
        //PONTO DE INSERÇÃO -> CENTRO DA ESTACA ESQUERDA
        private void inserirArmadurasB2VS(Document acDoc, Transaction acTrans, BlockTableRecord acBlkTblRec, Point3d ptoInsercao, double diametroEstaca, double ladoMaiorBloco, double distanciaEntreEstacas, double indicaAreaAmpliada, double n1)
        {
            double l = diametroEstaca;
            double dx = (0.45 * ladoMaiorBloco) - (0.5 * distanciaEntreEstacas);
            double x0 = ptoInsercao.X - dx;
            double xf = x0 + (0.9 * ladoMaiorBloco);

            if (indicaAreaAmpliada == 1) { l = 1.2 * diametroEstaca; }
            double y0 = ptoInsercao.Y - (l * 0.5);

            double d = l / (n1 - 1);
            d = Math.Round(d, 4);
            double y1 = y0;
            for (int i = 0; i < n1; i++)
            {
                Line linha = new Line(new Point3d(x0, y1, 0), new Point3d(x0 + (0.9 * ladoMaiorBloco), y1, 0));
                linha.LineWeight = espessuras[4];
                linha.Color = cores[10];
                linha.Layer = "LINHA DE ARMADURA";
                acBlkTblRec.AppendEntity(linha);
                acTrans.AddNewlyCreatedDBObject(linha, true);
                y1 = y1 + d;
            }
        }

        //INSERIR CONJUNTO DE ARMADURAS BLOCO DE 3 ESTACAS
        //INSERE NOVO BLOCO HEXAGONAL COM AS ARMADURAS
        //PONTO DE INSERÇÃO -> VÉRTICE MAIS INFERIOR E À ESQUERDA BLOCO HEXAGONAL
        private void inserirArmadurasB3VS(Database acCurDb, LayerTable acLyrTbl, Transaction acTrans, BlockTableRecord acBlkTblRec, Point3d ptoInsercao, double diametroEstaca, double a, double b, double distanciaEntreEstacas, double indicaAreaAmpliada, double n1, double cobrimentoEstaca)
        {
            //BLOCO HEXAGONAL

            double x0 = ptoInsercao.X;
            double y0 = ptoInsercao.Y;

            double sen60 = (Math.Sqrt(3)) / 2;
            double cos60 = 0.5;
            double x1 = x0 + a;
            double y1 = y0;
            double x2 = x0 + a + (b * cos60);
            double y2 = y0 + (b * sen60);
            double x3 = x0 + ((a + b) * cos60);
            double y3 = y0 + ((a + b) * sen60);
            double x4 = x0 + ((a - b) * cos60);
            double y4 = y3;
            double x5 = x0 - (b * cos60);
            double y5 = y2;

            Polyline blocoHexagonal = constroiBlocoHexagonal(x0, y0, x1, y1, x2, y2, x3, y3, x4, y4, x5, y5, cores[2]);
            blocoHexagonal.Layer = "CONTORNO FUNDACAO";
            acBlkTblRec.AppendEntity(blocoHexagonal);
            acTrans.AddNewlyCreatedDBObject(blocoHexagonal, true);

            double c = (0.5 * diametroEstaca) + cobrimentoEstaca;
            double l = distanciaEntreEstacas;
            double xe1 = x0 + (b * 0.5);
            double ye1 = y0 + c;
            double xe2 = x0 + (l + (b * 0.5));
            double ye2 = ye1;
            double xe3 = x0 + ((b + l) * 0.5);
            double ye3 = ye1 + (l * sen60);

            //ESTACAS
            inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe1, ye1, diametroEstaca);
            inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe2, ye2, diametroEstaca);
            inserirEstacaVistaSuperior(acCurDb, acLyrTbl, acTrans, acBlkTblRec, xe3, ye3, diametroEstaca);

            //ARMADURAS
            double sen30 = 0.5;
            double cos30 = Math.Sqrt(3) / 2;
            double sen90 = 1;
            double cos90 = 0;
            double d = diametroEstaca;
            double r = 0.5 * d;
            if (indicaAreaAmpliada == 1) { r = 1.2 * r; d = 1.2 * d; }
            double e = d / (n1 - 1);
            e = Math.Round(e, 4);
            double dx1 = e * cos30; double dy1 = 0 - (e * sen30);
            double dx2 = e * cos90; double dy2 = 0 - (e * sen90);
            double dx3 = 0 - (e * cos30); double dy3 = 0 - (e * sen30);
            double compArmadura = (2.4 * r) + distanciaEntreEstacas;

            //INÍCIO DAS RETAS

            double xa1 = xe1 - (r * cos30) - (1.2 * r * cos60);
            double ya1 = ye1 + (r * sen30) - (1.2 * r * sen60);
            double xaf1 = xa1 + (compArmadura * cos60);
            double yaf1 = ya1 + (compArmadura * sen60);

            double xa2 = xe1 - (1.2 * r);
            double ya2 = ye1 + (r);
            double xaf2 = xa2 + (compArmadura);
            double yaf2 = ya2;

            double xa3 = xe2 + (r * cos30) + (1.2 * r * cos60);
            double ya3 = ye2 + (r * sen30) - (1.2 * r * sen60);
            double xaf3 = xa3 - (compArmadura * cos60);
            double yaf3 = ya3 + (compArmadura * sen60);

            for (int i = 0; i < n1; i++)
            {
                Line l1 = new Line(new Point3d(xa1, ya1, 0), new Point3d(xaf1, yaf1, 0));
                l1.Color = cores[10];
                l1.Layer = "LINHA DE ARMADURA";
                acBlkTblRec.AppendEntity(l1);
                acTrans.AddNewlyCreatedDBObject(l1, true);
                Line l2 = new Line(new Point3d(xa2, ya2, 0), new Point3d(xaf2, yaf2, 0));
                l2.Color = cores[10];
                l2.Layer = "LINHA DE ARMADURA";
                acBlkTblRec.AppendEntity(l2);
                acTrans.AddNewlyCreatedDBObject(l2, true);
                Line l3 = new Line(new Point3d(xa3, ya3, 0), new Point3d(xaf3, yaf3, 0));
                l3.Color = cores[10];
                l3.Layer = "LINHA DE ARMADURA";
                acBlkTblRec.AppendEntity(l3);
                acTrans.AddNewlyCreatedDBObject(l3, true);
                xa1 = xa1 + dx1; ya1 = ya1 + dy1;
                xa2 = xa2 + dx2; ya2 = ya2 + dy2;
                xa3 = xa3 + dx3; ya3 = ya3 + dy3;
                xaf1 = xaf1 + dx1; yaf1 = yaf1 + dy1;
                xaf2 = xaf2 + dx2; yaf2 = yaf2 + dy2;
                xaf3 = xaf3 + dx3; yaf3 = yaf3 + dy3;
            }

        }

        //INSERIR CONJUNTO DE ARMADURAS
        //PONTO DE INSERÇÃO -> VÉRTICE MAIS INFERIOR E À ESQUERDA BLOCO
        private void inserirArmadurasB2VF(Document acDoc, Transaction acTrans, BlockTableRecord acBlkTblRec, Point3d ptoInsercao, double diametroEstaca, double ladoMaiorBloco, double alturaBloco, double distanciaEntreEstacas, double embutimentoEstaca, double n2, double n4)
        {
            //ARMADURA PELE
            //raio maio=1.4 raio menor=1
            double raioMaior = 1.4;
            double raioMenor = 1;
            double dx = 4.5 + (2 * raioMaior);
            double dy = embutimentoEstaca + (2 * raioMenor);
            double lx = ladoMaiorBloco - dx - dx;
            double ly = alturaBloco - dy - dy;

            double x0 = ptoInsercao.X + dx;
            double y0 = ptoInsercao.Y + dy;
            double x1 = x0 + lx;
            double y1 = y0;
            double x2 = x1;
            double y2 = y0 + ly;
            double x3 = x0;
            double y3 = y2;

            Polyline polilinha = new Polyline();
            polilinha.SetDatabaseDefaults();
            polilinha.AddVertexAt(0, new Point2d(x0, y0), 0, 0, 0);
            polilinha.AddVertexAt(1, new Point2d(x1, y1), 0, 0, 0);
            polilinha.AddVertexAt(2, new Point2d(x2, y2), 0, 0, 0);
            polilinha.AddVertexAt(3, new Point2d(x3, y3), 0, 0, 0);
            polilinha.AddVertexAt(4, new Point2d(x0, y0), 0, 0, 0);
            polilinha.Color = cores[10];
            polilinha.Closed = true;
            polilinha.Layer = "LINHA 2";
            acBlkTblRec.AppendEntity(polilinha);
            acTrans.AddNewlyCreatedDBObject(polilinha, true);

            ////ARMADURAS N2
            double lxn2 = lx - 3.6;
            double en2 = (lxn2 - 7.2) / (n2 - 1);
            en2 = Math.Round(en2, 4);
            int in2 = Convert.ToInt16(n2);
            double xen2 = x0 + 3.6+ raioMenor;
            double yen2 = y0 - raioMenor;

            for (int i = 0; i < in2; i++)
            {
                //linha inferior
                desenharArmadura(acDoc, acTrans, acBlkTblRec, xen2, yen2, raioMenor);
                //linha superior
                desenharArmadura(acDoc, acTrans, acBlkTblRec, xen2, yen2 + ly + (2 * raioMenor), raioMenor);
                xen2 = xen2 + en2;
            }

            ////ARMADURAS N4
            double lyn4 = ly;
            double en4 = lyn4 / (n4 - 1);
            en4 = Math.Round(en4, 4);
            int in4 = Convert.ToInt16(n4);
            double xen4 = x0 - raioMaior;
            double yen4 = y0;

            for (int i = 0; i < in4; i++)
            {
                //linha esquerda
                desenharArmadura(acDoc, acTrans, acBlkTblRec, xen4, yen4, raioMaior);
                //linha direira
                desenharArmadura(acDoc, acTrans, acBlkTblRec, xen4 + lx + (2 * raioMaior), yen4, raioMaior);
                yen4 = yen4 + en4;
            }
        }

        //INSERIR CONJUNTO DE ARMADURAS
        //PONTO DE INSERÇÃO -> VÉRTICE MAIS INFERIOR E À ESQUERDA BLOCO
        private void inserirArmadurasB2VL(Document acDoc, Transaction acTrans, BlockTableRecord acBlkTblRec, Point3d ptoInsercao, double diametroEstaca, double ladoMenorBloco, double alturaBloco, double distanciaEntreEstacas, double embutimentoEstaca, double n1, double n3, double n4, double indicaAreaAmpliada)
        {
            //ARMADURA PELE
            double dx = embutimentoEstaca + 1; double dy = 4.5; double raioArmadura = 1.4;
            double x0 = ptoInsercao.X + dy;
            double y0 = ptoInsercao.Y + dy;
            double lx = alturaBloco - dx - dy;  double ly = ladoMenorBloco - dy - dy;
            double x1 = x0 + lx;
            double y1 = y0;
            double x2 = x1;
            double y2 = y0 + ly;
            double x3 = x0;
            double y3 = y2;

            Polyline polilinha = new Polyline();
            polilinha.SetDatabaseDefaults();
            polilinha.AddVertexAt(0, new Point2d(x0, y0), 0, 0, 0);
            polilinha.AddVertexAt(1, new Point2d(x1, y1), 0, 0, 0);
            polilinha.AddVertexAt(2, new Point2d(x2, y2), 0, 0, 0);
            polilinha.AddVertexAt(3, new Point2d(x3, y3), 0, 0, 0);
            polilinha.AddVertexAt(4, new Point2d(x0, y0), 0, 0, 0);
            polilinha.Color = cores[10];
            polilinha.Layer = "LINHA 2";
            polilinha.Closed = true;
            acBlkTblRec.AppendEntity(polilinha);
            acTrans.AddNewlyCreatedDBObject(polilinha, true);

            ////ARMADURAS N4
            double lxn4 = lx - (2 * raioArmadura);
            double en4 = lxn4 / (n4 - 1);
            en4 = Math.Round(en4, 4);
            int in4 = Convert.ToInt16(n4);
            double xen4 = x0 + raioArmadura;
            double yen4 = y0 + raioArmadura;

            for (int i = 0; i < in4; i++)
            {
                //linha inferior
                desenharArmadura(acDoc, acTrans, acBlkTblRec, xen4, yen4, raioArmadura);
                //linha superior
                desenharArmadura(acDoc, acTrans, acBlkTblRec, xen4, yen4 + ly - (2 * raioArmadura), raioArmadura);
                xen4 = xen4 + en4;
            }

            //ARMADURAS N3
            double lyn3 = ly - (2 * raioArmadura);
            double en3 = lyn3 / (n3 + 1);
            en3 = Math.Round(en3, 4);
            int in3 = Convert.ToInt16(n3);
            double xen3 = x0 + raioArmadura;
            double yen3 = y0 + raioArmadura + en3;

            for (int i = 0; i < in3; i++)
            {
                //linha esquerda
                desenharArmadura(acDoc, acTrans, acBlkTblRec, xen3, yen3, raioArmadura);
                yen3 = yen3 + en3;
            }

            //ARMADURAS N1
            if(indicaAreaAmpliada==1) { diametroEstaca = diametroEstaca * 1.2; }
            double lyn1 = diametroEstaca;
            double en1 = lyn1 / (n1 - 1);
            en1 = Math.Round(en1, 4);
            int in1 = Convert.ToInt16(n1);
            double xen1 = x0 + lx - raioArmadura;
            double yen1 = y0 + (ly * 0.5) - (0.5 * diametroEstaca);

            for (int i = 0; i < in1; i++)
            {
                //linha esquerda
                desenharArmadura(acDoc, acTrans, acBlkTblRec, xen1, yen1, raioArmadura);
                yen1 = yen1 + en1;
            }

        }

        private void inserirArmadurasB4VF(Document acDoc, Transaction acTrans, BlockTableRecord acBlkTblRec, Point3d ptoInsercao, double diametroEstaca, double ladoBloco, double alturaBloco, double distanciaEntreEstacas, double embutimentoEstaca, double n2, double n3, double n4, double n7)
        {
            double x0 = ptoInsercao.X; double y0 = ptoInsercao.Y;
            double espacamentoH = 0;
            double espacamentoV = (alturaBloco - 12) / (n2 - 1);



        }

        //FUNÇÃO PARA DESENHAR A BARRA DA ARMADURA VISTA FRONTALMENTE, REPRESENTADA POR UM CÍRCULO HACHURADO
        private void desenharArmadura(Document acDoc, Transaction acTrans, BlockTableRecord acBlkTblRec, double xc, double yc, double raio)
        {
            Circle circulo = new Circle();
            circulo.Center = new Point3d(xc, yc, 0);
            circulo.Radius = raio;
            circulo.Color = cores[10];
            circulo.Layer = "LINHA 2";
            // Add the new circle object to the block table record and the transaction
            acBlkTblRec.AppendEntity(circulo);
            acTrans.AddNewlyCreatedDBObject(circulo, true);

            //// Adds the circle to an object id array
            //ObjectIdCollection acObjIdColl = new ObjectIdCollection();
            //acObjIdColl.Add(circulo.ObjectId);
            //
            //// Create the hatch object and append it to the block table record
            //Hatch acHatch = new Hatch();
            //acBlkTblRec.AppendEntity(acHatch);
            //acTrans.AddNewlyCreatedDBObject(acHatch, true);
            //
            //// Set the properties of the hatch object
            //// Associative must be set after the hatch object is appended to the 
            //// block table record and before AppendLoop
            //acHatch.SetDatabaseDefaults();
            ////acHatch.IsSolidFill

            //acHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
            //acHatch.Associative = true;
            //acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);
            //acHatch.EvaluateHatch(true);

        }


    }
}