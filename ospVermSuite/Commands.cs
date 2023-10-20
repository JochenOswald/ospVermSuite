// system 
using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;


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
  using _AdWnd = Autodesk.AutoCAD.Windows; //AdWindows.dll
  using _AcRbn = Autodesk.AutoCAD.Ribbon; //AcWindows.dll
  using _AcInt = Autodesk.AutoCAD.Internal;
  using _AcLy = Autodesk.AutoCAD.LayerManager;
#endif 

using ospVermSuite.Windows;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;
using Teigha.Runtime;
using Bricscad.Ribbon;
//using System.Windows;

[assembly: CommandClass(typeof(ospVermSuite.Commands))]


// this attribute marks this class, as a class having ExtensionApplication methods
// Initialize and Terminate that are called on loading and unloading of this assembly 
[assembly: ExtensionApplication(typeof(ospVermSuite .Commands))]

namespace ospVermSuite
{
    public class Commands : IExtensionApplication
    {
        public void Initialize()
        {
            
            try
            {
                if (RibbonServices.RibbonPaletteSet == null)
                    RibbonServices.CreateRibbonPaletteSet(); //needed for ribbon samples
#if BRX_APP
                //Create and display a native dockable panel (Bricscad.Windows.Panel)
                _AcWnd.DockingTemplate dockTempl = new _AcWnd.DockingTemplate();
                dockTempl.DefaultStackId = "RDOCK"; //default stack is RDOCK panelset
                dockTempl.DefaultStackZ = 20; //default to position 20 (bottom of the stack)
                dockTempl.DefaultDock = _AcWnd.DockSides.Right; //dock alone at right in case RDOCK panelset isn't available
                Windows.ProjectPanel projectPanel = new Windows.ProjectPanel();
                
                _AcWnd.Panel panel = new _AcWnd.Panel("VermSuite", projectPanel );
                panel.Title = "VermSuite";
                if (System.Convert.ToInt32(_AcAp.Application.GetSystemVariable("COLORTHEME"))==1)
                {
                    panel.Icon = ImageSourceFromEmbeddedResourceStream(@"ospVermSuite.Style.Bright.Tachy.ico");
                }
                else
                {
                    panel.Icon= ImageSourceFromEmbeddedResourceStream(@"ospVermSuite.Style.Dark.Tachy.ico");
                }
                //panel.Visible = true;
#elif ARX_APP
                _AcWnd.PaletteSet paletteSet = new _AcWnd.PaletteSet("ospVermSuite");
                ProjectPanel vermSuitePanel = new ProjectPanel();
                paletteSet.AddVisual("osp", vermSuitePanel);
#endif
            }
            catch (System.Exception e)
            {
                _AcAp.Application.ShowAlertDialog(" An exception occurred in Initialize():\n" + e.ToString());
            }
        }

        public void Terminate()
        {
            //Application.ShowAlertDialog("The commands class is Terminating");
        }

        [CommandMethod("VermTipEin")]
        public static void VermTipEin()
        {
            _AcEd.Editor myEditor = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            myEditor.WriteMessage("Zum Anzeigen mit der Maus über einen Vermessungspunkt fahren");
            myEditor.PointMonitor -= new _AcEd.PointMonitorEventHandler (Functions.VermFunctions.OnPointMonitor);
            myEditor.PointMonitor += new _AcEd.PointMonitorEventHandler(Functions.VermFunctions.OnPointMonitor);
        }

        [CommandMethod("VermTipAus")]
        public static void VermTipAus()
        {
            _AcEd.Editor myEditor = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            myEditor.PointMonitor -= new _AcEd.PointMonitorEventHandler(Functions.VermFunctions.OnPointMonitor);
        }

        [CommandMethod("VermInfo")]
        public void VermInfo()
        {
            //Punktverwaltung.DoDrawDirectory();
            InfoDialog infoDialog = new InfoDialog();
            _AcAp.Application.ShowModalWindow(infoDialog);
        }

        static private System.Windows.Media.ImageSource ImageSourceFromEmbeddedResourceStream(string resName)
        {
            System.Reflection.Assembly assy = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream stream = assy.GetManifestResourceStream(resName);
            if (stream == null)
                return null;
            System.Windows.Media.Imaging.BitmapImage img = new System.Windows.Media.Imaging.BitmapImage();
            img.BeginInit();
            img.StreamSource = stream;
            img.EndInit();
            return img;
        }

    }
}
