using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teigha.DatabaseServices;

// Bricsys
using Bricscad.ApplicationServices;
using Bricscad.Runtime;
using Bricscad.EditorInput;
using Bricscad.Ribbon;
using Bricscad.Geometrical3dConstraints;

// alias
using _AcRx = Teigha.Runtime;
using _AcAp = Bricscad.ApplicationServices;
using _AcDb = Teigha.DatabaseServices;
using _AcGe = Teigha.Geometry;
using _AcEd = Bricscad.EditorInput;
using _AcGi = Teigha.GraphicsInterface;
using _AcClr = Teigha.Colors;
using _AcWnd = Bricscad.Windows;
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
