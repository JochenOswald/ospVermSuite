using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using dwgHelper;

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
    using _AcRbn = Autodesk.AutoCAD.Ribbon; //AcWindows.dll
    using _AcInt = Autodesk.AutoCAD.Internal;
    using _AcLy = Autodesk.AutoCAD.LayerManager;
#endif 


using System.Globalization;
using System.Windows.Documents;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace ospVermSuite.Datatypes
{
    internal class SurveyPoint
    {
        public string StandardCode = "151"; // muss noch aus den Einstellungen übernommen werden.
        
        private string _code;
        private string _pointnr;
        private double _xcoord;
        private double _ycoord;
        private Single _zcoord;
        private DateTime _date;

        public string PointNr
        {
            get
            { return _pointnr; }
            set
            { _pointnr = value; }
        }
        public double XCoord
        {
            get
            { return _xcoord; }
            set
            { _xcoord = value; }
        }
        public double YCoord { get; set; }
        public Single ZCoord { get; set; }
        public Single rH { get; set; }
        public string Code
        {
            get
            {
                if (string.IsNullOrEmpty(_code))
                { 
                    return StandardCode; 
                }
                else
                {
                    return _code;
                }
            }
            set
            {
                _code = value;
            }
        }
        public string Attribut { get; set; }
        public Single XPrecision { get; set; }
        public Single YPrecision { get; set; }
        public Single XYPrecision
        { get
            { return Convert.ToSingle(Math.Sqrt(Math.Pow(XPrecision, 2) + Math.Pow(YPrecision, 2))); }
        }
        public Single ZPrecision { get; set; }
        public DateTime Date { get; set; }
        
        public SurveyPoint()
        { }

        public SurveyPoint(string pointNr,double xCoord, double yCoord, float zCoord)
        {
            PointNr = pointNr;
            XCoord = xCoord;
            YCoord = yCoord;
            ZCoord = zCoord;
        }

        public SurveyPoint(string pointNr, double xCoord, double yCoord, float zCoord, float rH, string code, string attribut, float xPrecision, float yPrecision, float zPrecision, DateTime date)
        {
            PointNr = pointNr;
            XCoord = xCoord;
            YCoord = yCoord;
            ZCoord = zCoord;
            this.rH = rH;
            Code = code;
            Attribut = attribut;
            XPrecision = xPrecision;
            YPrecision = yPrecision;
            ZPrecision = zPrecision;
            Date = date;
        }

        public SurveyPoint(string[] pointArray)
        {
            NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

            PointNr = pointArray[0];
            XCoord = double.Parse(pointArray[1],NumberStyles.Any,nfi);
            YCoord = double.Parse(pointArray[2], NumberStyles.Any, nfi);
            ZCoord = Single.Parse(pointArray[3], NumberStyles.Any, nfi);
            rH = Single.Parse(pointArray[4], NumberStyles.Any, nfi);
            Code = pointArray[5];
            Attribut = pointArray[6];
            XPrecision = Single.Parse(pointArray[7], NumberStyles.Any, nfi);
            YPrecision = Single.Parse(pointArray[8], NumberStyles.Any, nfi);
            ZPrecision = Single.Parse(pointArray[9], NumberStyles.Any, nfi);
            Date = DateTime.Parse(pointArray[10]);
        }

        public void Draw(string surveyor = "", string date = "", string description = "", string filename = "")
        {
            _AcDb.Database cadDatabase = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            using (_AcDb.Transaction transaction = cadDatabase.TransactionManager.StartTransaction())
            {
                _AcDb.ObjectId blockID = dwgFuncs.InsertBlock(Code, new _AcGe.Point3d(XCoord, YCoord, ZCoord), new _AcGe.Scale3d(Properties.Settings.Default.BlockScale));

                //DBObject block = transaction.GetObject(blockID, OpenMode.ForWrite);
                _AcDb.Entity block = transaction.GetObject(blockID, _AcDb.OpenMode.ForWrite) as _AcDb.Entity;
                block.Color = getPointColor();
                if (Properties.Settings.Default.LayerByCode==true)
                {
                    string layerName;
                    layerName = Properties.Settings.Default.LayerPrefix + Code;
                    dwgHelper.dwgFuncs.CheckLayer(layerName);
                                      block.Layer = layerName;
                }
                string[] xValues = new string[] { surveyor, date, description, PointNr, Code, Attribut, Math.Round(XPrecision, 3).ToString(), Math.Round(ZPrecision, 3).ToString(), this.Date.ToString(), filename };
                block.XData = dwgFuncs.AddXRecordToBlock("osp", xValues);
                // Beschriftungsblock einfügen
                var attributeDictionary = new Dictionary<string, string>();
                attributeDictionary.Add("POINTNR", PointNr.ToString());
                attributeDictionary.Add("XCOORD", XCoord.ToString());
                attributeDictionary.Add("YCOORD", YCoord.ToString());
                attributeDictionary.Add("ZCOORD", ZCoord.ToString());
                attributeDictionary.Add("RH", rH.ToString());
                attributeDictionary.Add("CODE", Code);
                attributeDictionary.Add("DESCRIPTION", Attribut);
                attributeDictionary.Add("XPRECISION", XPrecision.ToString());
                attributeDictionary.Add("YPRECISION", YPrecision.ToString());
                attributeDictionary.Add("ZPRECISION", ZPrecision.ToString());
                attributeDictionary.Add("DATE", Date.ToString());

                _AcDb.ObjectId beschriftID = dwgFuncs.InsertBlock(Properties.Settings.Default.LabelBlock, new _AcGe.Point3d(XCoord, YCoord, ZCoord), new _AcGe.Scale3d(Properties.Settings.Default.LabelScale), attributeDictionary);
                _AcDb.Entity beschriftung = transaction.GetObject(beschriftID, _AcDb.OpenMode.ForWrite) as _AcDb.Entity;
                beschriftung.XData = dwgFuncs.AddXRecordToBlock("osp", xValues);
                beschriftung.Color = getPointColor();
                if (Properties.Settings.Default.LayerByCode == true)
                {
                    string textLayerName;
                    textLayerName = Properties.Settings.Default.LayerPrefix + Code + Properties.Settings.Default.TextLayerPostfix;
                    dwgHelper.dwgFuncs.CheckLayer(textLayerName);
                    beschriftung.Layer = textLayerName;
                }
                xValues = null;
                attributeDictionary = null;
                block.Dispose();
                beschriftung.Dispose();
                transaction.Commit();
                transaction.Dispose();
            }
            cadDatabase.Dispose();
        }

        private _AcCm.Color getPointColor()
        {
            _AcCm.Color color = new _AcCm.Color();
            double pointPrecision = 0;
            if (Properties.Settings.Default.PrecisionPosition==true)
            {
                pointPrecision = XYPrecision;
            }
            if (Properties.Settings.Default.PrecisionHeight==true)
            {
                pointPrecision= Math.Sqrt(Math.Pow(pointPrecision, 2) + Math.Pow(ZPrecision,2));
            }

            if (pointPrecision == 0)
            {
                return _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByAci, 256);
            }
            else
            {
                if (pointPrecision <= Properties.Settings.Default.PrecisionGood)
                { return _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByAci, Properties.Settings.Default.PrecisionGoodColor); }
                if (pointPrecision > Properties.Settings.Default.PrecisionBad)
                { return _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByAci, Properties.Settings.Default.PrecisionBadColor); }
                // Wenn die Genauigkeit weder gut noch schlecht ist: mittlere Farbe zurückgeben
                return _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByAci, Properties.Settings.Default.PrecisionMediumColor);
            }
        }
    }
}
