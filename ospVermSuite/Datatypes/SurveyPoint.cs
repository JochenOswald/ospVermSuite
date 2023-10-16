using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using dwgHelper;

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
using Teigha.GraphicsInterface;
using System.Globalization;
using Teigha.Colors;
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
                ObjectId blockID = dwgFuncs.InsertBlock(Code, new _AcGe.Point3d(XCoord, YCoord, ZCoord), new Scale3d(Properties.Settings.Default.BlockScale));

                //DBObject block = transaction.GetObject(blockID, OpenMode.ForWrite);
                Entity block = transaction.GetObject(blockID, OpenMode.ForWrite) as Entity;
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
                
                ObjectId beschriftID = dwgFuncs.InsertBlock(Properties.Settings.Default.LabelBlock, new _AcGe.Point3d(XCoord, YCoord, ZCoord), new Scale3d(Properties.Settings.Default.LabelScale), attributeDictionary);
                Entity beschriftung = transaction.GetObject(beschriftID, OpenMode.ForWrite) as Entity;
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

        private _AcClr.Color getPointColor()
        {
            _AcClr.Color color = new _AcClr.Color();
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
                return Color.FromColorIndex(ColorMethod.ByAci, 256);
            }
            else
            {
                if (pointPrecision <= Properties.Settings.Default.PrecisionGood)
                { return Color.FromColorIndex(ColorMethod.ByAci, Properties.Settings.Default.PrecisionGoodColor); }
                if (pointPrecision > Properties.Settings.Default.PrecisionBad)
                { return Color.FromColorIndex(ColorMethod.ByAci, Properties.Settings.Default.PrecisionBadColor); }
                // Wenn die Genauigkeit weder gut noch schlecht ist: mittlere Farbe zurückgeben
                return Color.FromColorIndex(ColorMethod.ByAci, Properties.Settings.Default.PrecisionMediumColor);
            }
        }
    }
}
