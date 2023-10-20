using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teigha.DatabaseServices;

#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
using _AcCm = Teigha.Colors;
using _AcDb = Teigha.DatabaseServices;
using _AcEd = Bricscad.EditorInput;
using _AcGe = Teigha.Geometry;
using _AcGi = Teigha.GraphicsInterface;
using _AcGs = Teigha.GraphicsSystem;
using _AcGsk = Bricscad.GraphicsSystem;
using _AcPl = Bricscad.PlottingServices;
using _AcBrx = Bricscad.Runtime;
using _AcTrx = Teigha.Runtime;
using _AcWnd = Bricscad.Windows;
using _AdWnd = Bricscad.Windows;
using _AcRbn = Bricscad.Ribbon;
using _AcLy = Teigha.LayerManager;
using _AcIo = Teigha.Export_Import; //Bricsys specific
using _AcBgl = Bricscad.Global; //Bricsys specific
using _AcQad = Bricscad.Quad; //Bricsys specific
using _AcInt = Bricscad.Internal;
using _AcPb = Bricscad.Publishing;
using _AcMg = Teigha.ModelerGeometry; //Bricsys specific
using _AcLic = Bricscad.Licensing; //Bricsys specific
using _AcMec = Bricscad.MechanicalComponents; //Bricsys specific
using _AcBim = Bricscad.Bim; //Bricsys specific
using _AcDm = Bricscad.DirectModeling; //Bricsys specific
using _AcIfc = Bricscad.Ifc; //Bricsys specific
using _AcRhn = Bricscad.Rhino; //Bricsys specific
using _AcCiv = Bricscad.Civil; //Bricsys specific
#elif ARX_APP
  using _AcAp = Autodesk.AutoCAD.ApplicationServices;
  using _AcCm = Autodesk.AutoCAD.Colors;
  using _AcDb = Autodesk.AutoCAD.DatabaseServices;
  using _AcEd = Autodesk.AutoCAD.EditorInput;
  using _AcGe = Autodesk.AutoCAD.Geometry;
  using _AcGi = Autodesk.AutoCAD.GraphicsInterface;
  using _AcGs = Autodesk.AutoCAD.GraphicsSystem;
  using _AcGsk = Autodesk.AutoCAD.GraphicsSystem;
  using _AcPl = Autodesk.AutoCAD.PlottingServices;
  using _AcPb = Autodesk.AutoCAD.Publishing;
  using _AcBrx = Autodesk.AutoCAD.Runtime;
  using _AcTrx = Autodesk.AutoCAD.Runtime;
  using _AcWnd = Autodesk.AutoCAD.Windows; //AcWindows.dll
  using _AdWnd = Autodesk.Windows; //AdWindows.dll
  using _AcRbn = Autodesk.AutoCAD.Ribbon; //AcWindows.dll
  using _AcInt = Autodesk.AutoCAD.Internal;
  using _AcLy = Autodesk.AutoCAD.LayerManager;
#endif 

using System.Windows.Media.Imaging;
using System.Windows.Documents;


namespace ospVermSuite.Functions
{
    internal class VermFunctions
    {
        public static void OnPointMonitor(object sender, _AcEd.PointMonitorEventArgs eventArgs)
        {
            _AcDb.ObjectId id = _AcDb.ObjectId.Null;
            _AcEd.Editor editor = (_AcEd.Editor)sender;
            _AcAp.Document document = editor.Document;
            if (!editor.IsQuiescent)
                return;
            _AcEd.InputPointContext context = eventArgs.Context;
            try
            {
                _AcGi.ViewportDraw draw = context.DrawContext;
                draw.Geometry.Circle(context.RawPoint, Properties.Settings.Default.TooltipRadius, _AcGe.Vector3d.ZAxis);
                string names = null;
                _AcDb.FullSubentityPath[] paths = eventArgs.Context.GetPickedEntities();
                using (_AcDb.Transaction transaction = document.TransactionManager.StartTransaction())
                {
                   Array.Sort(paths, sortByIds);
                    foreach (_AcDb.FullSubentityPath path in paths)
                    {
                        _AcDb.ObjectId[] ids = path.GetObjectIds();
                        if (ids.Length > 0)
                        {
                            _AcDb.BlockReference bref = transaction.GetObject(
                                ids[ids.GetLowerBound(0)], _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                            if (bref != null)
                            {
                                _AcDb.ResultBuffer rb = bref.GetXDataForApplication("osp");
                                if (rb != null)
                                {
                                    _AcDb.TypedValue[] rvArr = rb.AsArray();
                                    names += string.Format("{0,-40} {1,-30}\n", "Vermesser:", rvArr[1].Value);
                                    names += string.Format("{0,-40} {1,-30}\n", "Datum:", rvArr[2].Value);
                                    names += string.Format("{0,-40} {1,-30}\n", "Beschreibung:", rvArr[3].Value);
                                    names += string.Format("{0,-40} {1,-30}\n", "Punktnr.:", rvArr[4].Value);
                                    names += string.Format("{0,-40} {1,-30}\n", "Code:", rvArr[5].Value);
                                    names += string.Format("{0,-40} {1,-30}\n", "Bemerkung:", rvArr[6].Value);
                                    names += string.Format("{0,-40} {1,-30}\n", "Lagequalität:", rvArr[7].Value);
                                    names += string.Format("{0,-40} {1,-30}\n", "Höhenqualität:", rvArr[8].Value);
                                    names += string.Format("{0,-40} {1,-30}\n", "Datei:", rvArr[9].Value);
                                }
                            }
                        }
                    }
                    transaction.Commit();
                }
                eventArgs.AppendToolTipText(names);
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage("\nError: {0}\nStackTrace: {1}", ex.Message, ex.StackTrace);
            }
        }

        public static int sortByIds(_AcDb.FullSubentityPath a, _AcDb.FullSubentityPath b)
        {
            return a.SubentId.IndexPtr.ToInt32().CompareTo(b.SubentId.IndexPtr.ToInt32());
        }

    }
}
