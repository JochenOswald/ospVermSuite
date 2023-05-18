using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public bool DrawFile { get; set; }
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
        public String Description { 
            get
            {  return _description; }
            set
            {  _description = value; }
        }
        public DateTime SurveyDay
        {
            get
            { return _surveyDay; }
            set
            { _surveyDay = value; }
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

            DrawFile = true;
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

        }
}
