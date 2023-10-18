using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows;
using System.Threading;
using dwgHelper;

namespace ospVermSuite.Datatypes
{
    internal class SurveyFile
    {
        #region internal Variables
        FileInfo _fileInfo;
        string _surveyor;
        string _description;
        DateTime _surveyDay;
        #endregion

        #region Properties
        public bool DrawState { get; set; }
        public FileInfo FileInfo
        {
            get
            { return _fileInfo; }
            set
            { _fileInfo = value; }
        }
        public String Surveyor
        {
            get
            { return _surveyor; }
            set
            { _surveyor = value; }
        }
        public String Description
        {
            get
            { return _description; }
            set
            { _description = value; }
        }
        public DateTime SurveyDay
        {
            get
            { return _surveyDay; }
            set
            { _surveyDay = value; }
        }
        public List<_AcGe.Point2d> minMaxPoints
        {
            get
            {
                List<_AcGe.Point2d> _minMaxPoints = new List<_AcGe.Point2d>();
                List<SurveyPoint> points = getPoints();
                _minMaxPoints.Add(new Point2d(points.Min(p => p.XCoord), points.Min(p => p.YCoord)));
                _minMaxPoints.Add(new Point2d(points.Max(p => p.XCoord), points.Max(p => p.YCoord)));

                return _minMaxPoints;
            }
        }
        #endregion

        #region Constructors
        public SurveyFile()
        { }

        public SurveyFile(string FileName)
        {
            if (File.Exists(FileName) == false)
            {
                throw new FileNotFoundException(FileName);
            }

            DrawState = true;
            _fileInfo = new FileInfo(FileName);
            string[] clines = File.ReadAllLines(FileName);
            List<string> list = new List<string>();

            foreach (string line in clines)
            {
                if (line.StartsWith(";;"))
                {
                    switch (line.Substring(0, line.IndexOf('=')))
                    {
                        case ";;Vermesser":
                            _surveyor = line.Substring(line.IndexOf('=') + 1);
                            break;
                        case ";;Vermessungsdatum":
                            string dateValue = line.Substring(line.IndexOf("=") + 1);
                            _surveyDay = DateTime.ParseExact(dateValue, "dd.MM.yyyy", CultureInfo.CurrentCulture);
                            break;
                        case ";;Beschreibung":
                            _description = line.Substring(line.IndexOf('=') + 1);
                            break;
                    }
                }
            }
        }
        #endregion,

        private List<SurveyPoint> getPoints()
        {
            if (File.Exists(_fileInfo.FullName) == false)
            { throw new FileNotFoundException(); }

            List<SurveyPoint> points = new List<SurveyPoint>();

            using (StreamReader reader = new StreamReader(_fileInfo.FullName))
            {
                while (reader.Peek() >= 0)
                {
                    string line = reader.ReadLine();
                    if (line.Substring(0, 2) != ";;") //Kommentare nicht bearbeiten
                    {
                        string[] pointArray = line.Split(';');
                        points.Add(new SurveyPoint(pointArray));
                    }
                }
            }
            return points;
        }


        public void DrawSurvey()
        {
            _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(string.Format("Zeichne {0} ", FileInfo.Name));
            List<SurveyPoint> points = getPoints();
            if (points == null)
            { return; }

            foreach (SurveyPoint point in points)
            {
                point.Draw(_surveyor, _surveyDay.ToString(), _description, _fileInfo.FullName);
                _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(".");
            }
            _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n");
        }

        public void DeleteSurvey()
        {
            _AcEd.SelectionSet cadSs = dwgFuncs.GetAllospObjects();

            if (cadSs == null)
            {
                _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Keine Punkte");
                return;
            }

            _AcDb.Database cadDatabase = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            using (_AcDb.Transaction transaction = cadDatabase.TransactionManager.StartTransaction())
            {
                 // Dictionary<string, string> XDataValue;
                // Iterate through objects and delete them
                foreach (SelectedObject cadSelectedObject in cadSs)
                {
                    _AcDb.DBObject cadObject = transaction.GetObject(cadSelectedObject.ObjectId, _AcDb.OpenMode.ForWrite);
                    if (cadObject != null)
                    {
                        _AcDb.ResultBuffer rb = cadObject.GetXDataForApplication("osp");
                        if (rb != null)
                        {
                            _AcDb.TypedValue[] rvArr = rb.AsArray();
                            if (rvArr[10].Value.ToString() == _fileInfo.FullName)
                            {
                                cadObject.Erase();
                                cadObject.Dispose();
                            }
                        }
                    }
                    cadObject.Dispose();
                }
                transaction.Commit();
                cadSs.Dispose();
            }
            cadDatabase.Dispose();
        }
    }

}
