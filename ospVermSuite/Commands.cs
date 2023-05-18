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
                _AcWnd.Panel panel = new _AcWnd.Panel("VermSuite", new Windows.ProjectPanel() );
                panel.Title = "VermSuite";

                panel.Icon =  ImageSourceFromEmbeddedResourceStream(@"ospVermSuite.Style.Bright.Tachy.ico");
                panel.Visible = true;
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
