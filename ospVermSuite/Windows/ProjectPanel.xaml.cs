using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ospVermSuite.Datatypes;
using System.Threading;
using System.ComponentModel;

namespace ospVermSuite.Windows
{
    /// <summary>
    /// Interaktionslogik für ProjectPanel.xaml
    /// </summary>
    public partial class ProjectPanel : System.Windows.Controls.UserControl
    {
        BackgroundWorker bgWorker = new BackgroundWorker();

        public CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        // Maximale Tiefe bis zu der die Unterordner durchsucht werden. Wenn ein ganzes Verzeichnis durchsucht wird dauert es sonst zu lange.
        // Muss in eine Einstellung umgewandelt werden.
        private int MaximumDepth = 2;

        public ProjectPanel()
        {
            InitializeComponent();
            bgWorker.DoWork += bgWorker_DoWork;
            bgWorker.RunWorkerCompleted += bgWorker_Completed;
            bgWorker.WorkerReportsProgress = false;
            bgWorker.WorkerSupportsCancellation = true;

            PopulateTreeView();
        }

        // nötig damit die Knoten richtig angezeigt werden.
        private object dummyNode = null;

        /// <summary>
        /// Explorer-Ansicht (Treeview) mit den verfügbaren Laufwerken füllen
        /// </summary>
        private void PopulateTreeView()
        {
            folderTree.Items.Clear();
            TreeViewItem rootNode;
            foreach (string drive in Directory.GetLogicalDrives())
            {
                DirectoryInfo info = new DirectoryInfo(drive);
                if (info.Exists)
                {
                    rootNode = new TreeViewItem();
                    rootNode.Header = info.Name;
                    rootNode.Tag = info.FullName;
                    rootNode.Items.Add(dummyNode);
                    // Beim Erweitern die Unterordner ermitteln + anfügen
                    rootNode.Expanded += new RoutedEventHandler(FolderTreeItem_Expanded); //(AddDirectories(info,rootNode)) );
                    // Das selected-Event Bubbelt immer nach oben. Auch wenn e.handled auf true gesetzt wurde.
                    // Deshalb wurde die Aktion des Select-Events
                    rootNode.Selected += new RoutedEventHandler(FolderTreeItem_Selected);
                    folderTree.Items.Add(rootNode);
                }

            }
        }

        /// <summary>
        /// Ermittelt die Unterordner und fügt sie im Treeview an
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FolderTreeItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem parentNode = (TreeViewItem)sender;
            ExpandNode(parentNode);
            // wenn das nicht gesetzt wird, dann wird das Event auch für den übergeordneten Knoten ausgeführt,
            e.Handled= true;

        }

        private void ExpandNode(TreeViewItem parentNode)
        {
            parentNode.Items.Clear();
            TreeViewItem aNode;
            DirectoryInfo parentDirectory = new DirectoryInfo(parentNode.Tag.ToString());
            IEnumerable<DirectoryInfo> subDirs = parentDirectory.EnumerateDirectories().OrderBy(dir => dir.Name);

            foreach (DirectoryInfo subDir in subDirs)
            {
                try
                {
                    aNode = new TreeViewItem(); //(subDir.Name, 0, 0);
                    aNode.Header = subDir.Name;
                    aNode.Tag = subDir.FullName;
                    aNode.Items.Add(dummyNode);
                    aNode.Expanded += new RoutedEventHandler(FolderTreeItem_Expanded);
                    aNode.Selected += new RoutedEventHandler(FolderTreeItem_Selected);
                    parentNode.Items.Add(aNode);
                }
                catch { }
            }
        }

        /// <summary>
        /// Füllt die Vermessungsdatei-Liste mit den osv-Dateien der nachfolgenden 2 Ebenen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderTreeItem_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem node = (TreeViewItem)sender;
            e.Handled = true;
            bgWorker.CancelAsync();
            List<object> arguments = new List<object>();
            arguments.Add(node.Tag.ToString());
            
            bgWorker.RunWorkerAsync(argument: arguments);
            // Aus Gründen die keiner kennt wird e.Handled nicht ausgewertet. Die nachfolgenden 2 Zeilen sind dafür ein Workaround
            node.Focus();
            node.IsSelected = true;
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<SurveyFile> surveyFiles = new List<SurveyFile>();

            List<object> genericlist = e.Argument as List<object>;
            string directoryName = (string)genericlist[0];

            if (Directory.Exists(directoryName))
            {
                DirectoryInfo directory = new DirectoryInfo(directoryName);
                List<string> files = FindFilesRecurse(directoryName, 0);
                foreach (string file in files)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        SurveyFile surveyFile = new SurveyFile(fileInfo.FullName);
                        if (surveyFile != null)
                        {
                            surveyFiles.Add(surveyFile);
                        }
                    }
                    catch { }
                }
                e.Result = surveyFiles;
            }
        }

        private void bgWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                vermGrid.ItemsSource=(List<SurveyFile>)e.Result;
            }
            bgWorker.Dispose();
        }

        private List<string> FindFilesRecurse(string dir, int currentDepth)
        {
            List<string> files = new List<string>();
            
            // Dateien im Verzeichnis suchen
            try
            {
                files = Directory.GetFiles(dir, "*.osv").ToList();
            }
            catch { }

            // Unterverzeichnisse durchsuchen (wenn es nicht zu tief ist)
            if (currentDepth < 2)
            {
                foreach (string subdir in Directory.GetDirectories(dir))
                {
                    try
                    {
                        List<string> subdirFiles = FindFilesRecurse(subdir, currentDepth + 1);
                        files.AddRange(subdirFiles);
                    }
                    catch { }
                }
            }
            return files;
        }

        private void FolderItemCheckChanged(bool checkState)
        {
                List<SurveyFile> surveyFiles = (List<SurveyFile>)vermGrid.ItemsSource;
                if (surveyFiles != null)
                {
                    foreach (SurveyFile surveyFile in surveyFiles)
                    {
                        surveyFile.DrawFile = checkState;
                    }
                    vermGrid.ItemsSource = null;
                    vermGrid.ItemsSource = surveyFiles;
                }
        }

        private void SwitchDraw_Checked(object sender, RoutedEventArgs e)
        {
            FolderItemCheckChanged(true);
        }

        private void SwitchDraw_Unchecked(object sender, RoutedEventArgs e)
        {
            FolderItemCheckChanged(false);
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            var selectedPath = (TreeViewItem)folderTree.SelectedItem;
            string[] pathSplit = selectedPath.Tag.ToString().Split('\\');
            PopulateTreeView();
            TreeViewItem parentNode = null;
            TreeViewItem currentNode = null;
            string treePath = "";
            ItemCollection collection = null;
            for (int i = 0; i < pathSplit.Length; i++)
            {
                parentNode = currentNode;
                currentNode = null;
                if (parentNode == null)
                {
                    collection = folderTree.Items;
                    treePath= pathSplit[i] + "\\";
                }
                else
                {
                    collection = parentNode.Items;
                    treePath= pathSplit[i];
                }
                
                foreach (TreeViewItem item in collection)
                {
                    if (item.Header.ToString() == treePath)
                    {
                        currentNode = item;
                        currentNode.IsExpanded = true;
                        break;
                    }
                }

                if (currentNode == parentNode)
                {
                    break;
                }
            }
            parentNode.IsSelected = true;
        }
    }
}
