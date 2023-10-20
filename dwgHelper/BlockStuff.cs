using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if BRX_APP
using _AcRx = Teigha.Runtime;
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
using _AcRx = Autodesk.AutoCAD.Runtime;
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
using _AcRbn = Autodesk.AutoCAD.Ribbon; //AcWindows.dll
using _AcInt = Autodesk.AutoCAD.Internal;
using _AcLy = Autodesk.AutoCAD.LayerManager;
#endif 

using System.Windows.Media.Imaging;
using System.IO;

namespace dwgHelper
{
    internal class BlockStuff
    {
        internal static _AcDb.ObjectId getBlockFromDrawing(string blockName)
        {
            if (string.IsNullOrEmpty(blockName))
            {
                throw new ArgumentNullException(nameof(blockName));
            }
            //if (cadDatabase == null)
            //{
            _AcDb.Database cadDatabase = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            //}

            using (_AcDb.Transaction cadTransaction = cadDatabase.TransactionManager.StartTransaction())
            {
                // BlockTabelle öffnen mit Lesezugriff
                _AcDb.BlockTable blockTable = cadTransaction.GetObject(cadDatabase.BlockTableId, _AcDb.OpenMode.ForRead) as _AcDb.BlockTable;

                _AcDb.ObjectId cadBlockRecordID = _AcDb.ObjectId.Null;

                if (blockTable.Has(blockName))
                // wenn der Block schon in der Zeichnung vorhanden ist: ID zum Blocknamen zurückgeben
                {
                    cadTransaction.Commit(); 
                    return blockTable[blockName]; 
                }
                else
                {
                    throw new _AcRx.Exception(_AcRx.ErrorStatus.InvalidBlockName);
                }
            }
        }

        internal static _AcDb.ObjectId getBlockFromSearchPath(string blockName)
        {
            if (string.IsNullOrEmpty(blockName))
            {
                throw new ArgumentNullException(nameof(blockName));
            }
            //if (cadDatabase == null)
            //{
            _AcDb.Database cadDatabase = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            //}

            using (_AcDb.Transaction cadTransaction = cadDatabase.TransactionManager.StartTransaction())
            {
                string blockPath = _AcDb.HostApplicationServices.Current.FindFile(blockName + ".dwg", cadDatabase, _AcDb.FindFileHint.Default);
                // Wenn eine Datei gefunden wurde:
                if (!string.IsNullOrEmpty(blockPath))
                {
                    // zum Schreiben öffnen
                    _AcDb.BlockTable blockTable = cadTransaction.GetObject(cadDatabase.BlockTableId, _AcDb.OpenMode.ForRead) as _AcDb.BlockTable;
                    blockTable.UpgradeOpen();
                    // Datei in eine temporäre Datenbank lesen und in die aktuelle einfügen
                    using (_AcDb.Database temporaryDB = new _AcDb.Database(false, true))
                    {
                        temporaryDB.ReadDwgFile(blockPath, FileShare.Read, true, null);
                        cadDatabase.Insert(blockName, temporaryDB, true);
                    }
                    //cadTransaction.Commit();
                    // jetzt ist der Block in der Zeichnung vorhanden und kann zurückgegeben werden.
                    cadTransaction.Commit();
                    return blockTable[blockName];
                }
                else
                {
                    throw new _AcRx.Exception(_AcRx.ErrorStatus.InvalidBlockName);
                }
            }
        }

        internal static _AcDb.ObjectId createStandardBlock(string blockName)
        {
            if (string.IsNullOrEmpty(blockName))
            {
                throw new ArgumentNullException(nameof(blockName));
            }
            //if (cadDatabase == null)
            //{
            _AcDb.Database cadDatabase = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            //}

            using (_AcDb.Transaction cadTransaction = cadDatabase.TransactionManager.StartTransaction())
            {
                _AcDb.BlockTable blockTable = cadTransaction.GetObject(cadDatabase.BlockTableId, _AcDb.OpenMode.ForRead) as _AcDb.BlockTable;

                using (_AcDb.BlockTableRecord blockTableRecord = new _AcDb.BlockTableRecord())
                {
                    // Block mit Name und Einfügepunkt erstellen
                    blockTableRecord.Name = blockName;
                    blockTableRecord.Origin = new _AcGe.Point3d(0, 0, 0);

                    // Der Block besteht aus einem Punkt und dem Code als Text
                    using (_AcDb.DBPoint cadPoint = new _AcDb.DBPoint(new _AcGe.Point3d(0, 0, 0)))
                    {
                        blockTableRecord.AppendEntity(cadPoint);
                    }
                    using (_AcDb.DBText cadText = new _AcDb.DBText())
                    {
                        cadText.Position = new _AcGe.Point3d(0, 0, 0);
                        cadText.Height = 0.5;
                        cadText.TextString = blockName;

                        blockTableRecord.AppendEntity(cadText);
                        //acTrans.AddNewlyCreatedDBObject(acText, true);
                        blockTable.UpgradeOpen();
                        blockTable.Add(blockTableRecord);
                    }
                }
                cadTransaction.Commit();
                return blockTable[blockName];
            }
        }

    }
}
