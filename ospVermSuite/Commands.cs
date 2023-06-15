// system 
using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;


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
using ospVermSuite.Windows;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;
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

                //Create and display a native dockable panel (Bricscad.Windows.Panel)
                _AcWnd.DockingTemplate dockTempl = new _AcWnd.DockingTemplate();
                dockTempl.DefaultStackId = "RDOCK"; //default stack is RDOCK panelset
                dockTempl.DefaultStackZ = 20; //default to position 20 (bottom of the stack)
                dockTempl.DefaultDock = _AcWnd.DockSides.Right; //dock alone at right in case RDOCK panelset isn't available
                Windows.ProjectPanel projectPanel = new Windows.ProjectPanel();
                
                _AcWnd.Panel panel = new _AcWnd.Panel("VermSuite", projectPanel );
                panel.Title = "VermSuite";
                if (System.Convert.ToInt32(Application.GetSystemVariable("COLORTHEME"))==1)
                {
                    panel.Icon = ImageSourceFromEmbeddedResourceStream(@"ospVermSuite.Style.Bright.Tachy.ico");
                }
                else
                {
                    panel.Icon= ImageSourceFromEmbeddedResourceStream(@"ospVermSuite.Style.Dark.Tachy.ico");
                }
                //panel.Visible = true;
            }
            catch (System.Exception e)
            {
                Application.ShowAlertDialog(" An exception occurred in Initialize():\n" + e.ToString());
            }
        }

        public void Terminate()
        {
            //Application.ShowAlertDialog("The commands class is Terminating");
        }

        [CommandMethod("VermTipEin")]
        public static void VermTipEin()
        {
            Editor myEditor = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            myEditor.WriteMessage("Zum Anzeigen mit der Maus über einen Vermessungspunkt fahren");
            myEditor.PointMonitor -= new PointMonitorEventHandler(Functions.VermFunctions.OnPointMonitor);
            myEditor.PointMonitor += new PointMonitorEventHandler(Functions.VermFunctions.OnPointMonitor);
        }

        [CommandMethod("VermTipAus")]
        public static void VermTipAus()
        {
            Editor myEditor = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            myEditor.PointMonitor -= new PointMonitorEventHandler(Functions.VermFunctions.OnPointMonitor);
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
