using System;

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
using System.Windows.Documents;
using System.Windows;
using Teigha.Colors;

namespace dwgHelper
{
    public class dwgFuncs
    {
        public static _AcDb.ObjectId InsertBlock(string blockName, _AcGe.Point3d coordinate, _AcGe.Scale3d scale, System.Collections.Generic.Dictionary<string,string> attributeDict=null)
        {
            _AcDb.Database cadDatabase = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            // ObjectID vom Block suchen. Erst in der Zeichnung
            _AcDb.ObjectId blockRecordId;
                try
            {
                blockRecordId = BlockStuff.getBlockFromDrawing(blockName);
            }
            catch
            {
                try
                {
                    blockRecordId = BlockStuff.getBlockFromSearchPath(blockName);
                }
                catch
                {
                    blockRecordId = BlockStuff.createStandardBlock(blockName);
                }
            }

            _AcDb.BlockReference blockReference = new _AcDb.BlockReference(coordinate, blockRecordId);
            using (_AcDb.Transaction transaction = cadDatabase.TransactionManager.StartTransaction())
            {
                using (blockReference)
                {
                    _AcDb.BlockTableRecord blockRecord = transaction.GetObject(cadDatabase.CurrentSpaceId, _AcDb.OpenMode.ForWrite) as _AcDb.BlockTableRecord;
                    blockRecord.AppendEntity(blockReference);
                    blockReference.ScaleFactors = scale;
                    transaction.AddNewlyCreatedDBObject(blockReference, true);

                    if (attributeDict!=null)
                    {
                        // Wenn es ein Attributblock ist: die Attribute mit Werten füllen.
                        _AcDb.BlockTable blockTable;
                        blockTable = transaction.GetObject(cadDatabase.BlockTableId, _AcDb.OpenMode.ForRead) as _AcDb.BlockTable;
                        _AcDb.BlockTableRecord blockTemplate = blockRecordId.GetObject(_AcDb.OpenMode.ForRead) as _AcDb.BlockTableRecord;
                        foreach (_AcDb.ObjectId blockEntityID in blockTemplate)
                        {
                            _AcDb.DBObject blockEntity = blockEntityID.GetObject(_AcDb.OpenMode.ForRead);
                            _AcDb.AttributeDefinition attDefinition = blockEntity as _AcDb.AttributeDefinition;
                            if ((attDefinition != null) && (!attDefinition.Constant))
                            {
                                // Attribute können nicht direkt geändert werden, sondern es muss ein neues
                                // Attribut ersellt werden. Aber nur, wenn es eine Attributdefinition ist
                                // und die nicht konstant ist.
                                using (_AcDb.AttributeReference attReference = new _AcDb.AttributeReference())
                                {
                                    attReference.SetAttributeFromBlock(attDefinition, blockReference.BlockTransform);

                                    string dictValue;
                                    attributeDict.TryGetValue(attReference.Tag, out dictValue);
                                    
                                    // Attributreferenz mit Werten der Blockreferenz zuordnen
                                    attReference.TextString = dictValue;
                                    blockReference.AttributeCollection.AppendAttribute(attReference);
                                    transaction.AddNewlyCreatedDBObject(attReference, true);
                                }
                            }
                        }
                        blockTemplate.Dispose();
                    }
                    transaction.Commit();
                    blockReference.Dispose();
                }

                return blockReference.ObjectId;
            }
        }

        public static ResultBuffer AddXRecordToBlock(string appName, string[] xValues)
        {
            if (appName==null)
            { 
                return null;
            }
            if (xValues == null || xValues.Length == 0)
            {
                return null;
            }

            _AcDb.Database cadDatabase = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;

            ResultBuffer xBuffer = new ResultBuffer();

            using (_AcDb.Transaction cadTransaction = cadDatabase.TransactionManager.StartTransaction())
            {
                _AcDb.RegAppTable acRegTable = (_AcDb.RegAppTable)cadTransaction.GetObject(cadDatabase.RegAppTableId, _AcDb.OpenMode.ForRead);
                if (acRegTable.Has(appName) == false)
                {
                    //Registrieren der App wenn noch nicht vorhanden
                    acRegTable.UpgradeOpen();
                    _AcDb.RegAppTableRecord XApplication = new _AcDb.RegAppTableRecord();
                    XApplication.Name = appName;
                    acRegTable.Add(XApplication);
                    cadTransaction.AddNewlyCreatedDBObject(XApplication, true);
                }

                xBuffer.Add(new TypedValue(1001, appName));
                foreach (string xValue  in xValues)
                {
                    xBuffer.Add(new TypedValue(1000, xValue));
                }
                
                cadTransaction.Commit();
            }

            return xBuffer;
        }

        public static void ZoomWindow(_AcGe.Point2d minPoint, _AcGe.Point2d maxPoint)
        {
            if (minPoint == null || maxPoint == null)
            { return; }

            Document cadDocument = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database cadDatabase = cadDocument.Database;
            Editor cadEditor = cadDocument.Editor;

            _AcDb.ViewTableRecord view = new _AcDb.ViewTableRecord();

            view.CenterPoint = minPoint + ((maxPoint - minPoint) / 2.0);
            if (maxPoint.Y - minPoint.Y < 100)
            { view.Height = 100; }
            else
            { view.Height = maxPoint.Y - minPoint.Y; }
            if (maxPoint.X - minPoint.X < 100)
            { view.Width = 100; }
            else
            { view.Width = maxPoint.X - minPoint.X; }

            cadEditor.SetCurrentView(view);
        }

        public static void CheckLayer(string layerName)
        {
            _AcDb.Database cadDatabase = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;

            using (_AcDb.Transaction cadTransaction = cadDatabase.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                _AcDb.LayerTable cadLayerTable;
                // Layertable zum Lesen öffnen
                cadLayerTable = cadTransaction.GetObject(cadDatabase.LayerTableId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;

                // Wenn Layer nicht existiert: Anlegen mit Farbe 253
                if (!cadLayerTable.Has(layerName))
                {
                    using (_AcDb.LayerTableRecord cadLayerTableRecord = new _AcDb.LayerTableRecord())
                    {
                        cadLayerTableRecord.Color = _AcClr.Color.FromColorIndex(ColorMethod.ByAci, 253);
                        cadLayerTableRecord.Name = layerName;

                        // Jetzt muss die Layertable zum Schreiben upgegradet werden
                        cadLayerTable.UpgradeOpen();

                        // Layerrecord anhängen
                        cadLayerTable.Add(cadLayerTableRecord);
                        cadTransaction.AddNewlyCreatedDBObject(cadLayerTableRecord, true);

                        cadTransaction.Commit();
                    }
                }
            }
        }

    }
}
