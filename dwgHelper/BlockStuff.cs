using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ODA
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;

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
                    throw new _AcRx.Exception(ErrorStatus.InvalidBlockName);
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
                    throw new _AcRx.Exception(ErrorStatus.InvalidBlockName);
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
