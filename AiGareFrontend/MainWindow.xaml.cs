using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Path = System.IO.Path;

namespace AiGareFrontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            txtCommand.Text = _defaultCommand;

            LoadSettings();
        }

        private void LoadSettings()
        {
            if (File.Exists("settings.json"))
            {
                var settingsFile = File.ReadAllText("settings.json");

                Settings settings = JsonConvert.DeserializeObject<Settings>(settingsFile);

                txtCommand.Text = settings.CommandPath;
                txtDestination.Text = settings.DestinationPath;
                txtSource.Text = settings.SourcePath;
                txtExecuteFrom.Text = settings.ExecuteFrom;
                chkShellExecute.IsChecked = settings.UseShellExecute;
                txtArguments.Text = settings.Arguments;
                chkRecursive.IsChecked = settings.Recursive;
            }
        }

        private void SaveSettings()
        {
            var json = JsonConvert.SerializeObject(new Settings()
            {
                CommandPath = txtCommand.Text,
                DestinationPath = txtDestination.Text,
                SourcePath = txtSource.Text,
                ExecuteFrom = txtExecuteFrom.Text,
                UseShellExecute = chkShellExecute.IsChecked,
                Arguments = txtArguments.Text,
                Recursive = chkRecursive.IsChecked
            });

            File.WriteAllText("settings.json", json);
        }


        private string _defaultCommand = "python det.py -d \"$source\" -c \"$dest\"";

        private void btnSelectFolderSource_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = openFileDlg.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                txtSource.Text = openFileDlg.SelectedPath;
            }
        }

        private void btnSelectFolderDestination_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "onlus",
                DefaultExt = ".csv",
                Filter = "Comma Separated Values File *.csv | *.csv",
                AddExtension = true
            };

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                // Save document
                txtDestination.Text = dlg.FileName;
            }

            //System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();
            //var result = openFileDlg.ShowDialog();
            //if (result.ToString() != string.Empty)
            //{
            //    txtDestination.Text = openFileDlg.SelectedPath;
            //}
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtSource.Text))
            {
                Process.Start("explorer.exe", txtSource.Text);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtDestination.Text))
            {
                var f = Path.GetDirectoryName(txtDestination.Text);

                Process.Start("explorer.exe", f);
            }
        }

        private bool DoRecursiveFolderSave = false;

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                SaveSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Demystify().ToString());
            }

            var currentDir =
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (chkRecursive.IsChecked == true)
            {
                if (DoRecursiveFolderSave)
                {
                    var subFolders = GetFolders(txtSource.Text);
                    int i = 0;
                    foreach (var subFolder in subFolders)
                    {
                        var destFile = txtDestination.Text;
                        var currentSubFolder = subFolder.Replace(txtSource.Text, "").Remove(0, 1);

                        var destFileSubFolder =
                            $"{Path.Combine(Path.GetDirectoryName(destFile), Path.GetFileNameWithoutExtension(destFile), currentSubFolder)}";

                        if (!Directory.Exists(destFileSubFolder))
                        {
                            Directory.CreateDirectory(destFileSubFolder);
                        }

                        //var numberedDestFile =
                        //    $"{Path.Combine(Path.GetDirectoryName(destFile), Path.GetFileNameWithoutExtension(destFile))}_{i}{Path.GetExtension(destFile)}";

                        var numberedDestFile =
                            $"{Path.Combine(destFileSubFolder, Path.GetFileNameWithoutExtension(destFile))}_{i}{Path.GetExtension(destFile)}";


                        StartProcess(subFolder, numberedDestFile);
                        i++;
                    }
                }
                else
                {
                    var subFolders = GetFolders(txtSource.Text);
                    int i = 0;
                    foreach (var subFolder in subFolders)
                    {
                        var destFile = txtDestination.Text;

                        var numberedDestFile =
                            $"{Path.Combine(Path.GetDirectoryName(destFile), Path.GetFileNameWithoutExtension(destFile))}_{i}{Path.GetExtension(destFile)}";

                        StartProcess(subFolder, numberedDestFile);
                        i++;
                    }
                }


            }
            else
            {
                StartProcess(txtSource.Text, txtDestination.Text);
            }

            //var command = txtCommand.Text.Replace("$source", txtSource.Text)
            //    .Replace("$dest", txtDestination.Text);
            //var arguments = txtArguments.Text.Replace("$source", txtSource.Text)
            //    .Replace("$dest", txtDestination.Text);
            //try
            //{
            //    Process.Start(new ProcessStartInfo(command)
            //    {
            //        UseShellExecute = chkShellExecute.IsChecked ?? false,
            //        WindowStyle = ProcessWindowStyle.Normal,
            //        CreateNoWindow = false,
            //        WorkingDirectory = txtExecuteFrom.Text,
            //        Arguments = arguments
            //        //WorkingDirectory = 
            //    });
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Demystify().ToString());
            //}
        }

        private void StartProcess(string baseFolder, string destinationFile)
        {
            var command = txtCommand.Text.Replace("$source", baseFolder)
                .Replace("$dest", destinationFile);
            var arguments = txtArguments.Text.Replace("$source", baseFolder)
                .Replace("$dest", destinationFile);
            try
            {
                Process.Start(new ProcessStartInfo(command)
                {
                    UseShellExecute = chkShellExecute.IsChecked ?? false,
                    WindowStyle = ProcessWindowStyle.Normal,
                    CreateNoWindow = false,
                    WorkingDirectory = txtExecuteFrom.Text,
                    Arguments = arguments
                    //WorkingDirectory = 
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Demystify().ToString());
            }
        }

        private string[] GetFolders(string baseDirPath)
        {
            var baseDir = new DirectoryInfo(baseDirPath);

            var res = baseDir.GetDirectories("*", SearchOption.AllDirectories).Select(x => x.FullName).ToArray();

            return res;
        }

        private void btnSelectFolderExecuteFrom_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();

            var result = openFileDlg.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                txtExecuteFrom.Text = openFileDlg.SelectedPath;
            }
        }

        /// <summary>
        /// Rebases file with path fromPath to folder with baseDir.
        /// </summary>
        /// <param name="_fromPath">Full file path (absolute)</param>
        /// <param name="_baseDir">Full base directory path (absolute)</param>
        /// <returns>Relative path to file in respect of baseDir</returns>
        static public String MakeRelative(String _fromPath, String _baseDir)
        {
            String pathSep = "\\";
            String fromPath = Path.GetFullPath(_fromPath);
            String baseDir = Path.GetFullPath(_baseDir);            // If folder contains upper folder references, they gets lost here. "c:\test\..\test2" => "c:\test2"

            String[] p1 = Regex.Split(fromPath, "[\\\\/]").Where(x => x.Length != 0).ToArray();
            String[] p2 = Regex.Split(baseDir, "[\\\\/]").Where(x => x.Length != 0).ToArray();
            int i = 0;

            for (; i < p1.Length && i < p2.Length; i++)
                if (String.Compare(p1[i], p2[i], true) != 0)    // Case insensitive match
                    break;

            if (i == 0)     // Cannot make relative path, for example if resides on different drive
                return fromPath;

            String r = String.Join(pathSep, Enumerable.Repeat("..", p2.Length - i).Concat(p1.Skip(i).Take(p1.Length - i)));
            return r;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var csvFolder = Path.GetDirectoryName(txtDestination.Text);
            var csvFiles = Directory.EnumerateFiles(csvFolder, "*.csv", SearchOption.AllDirectories).ToArray();

            if (csvFiles.Length > 1)
            {
                var firstFile = csvFiles[0];

                for (int i = 1; i < csvFiles.Count(); i++)
                {
                    var file = csvFiles[i];
                    var fileData = File.ReadAllText(file);
                    fileData = fileData.Replace("filename,text\r\r\n", "");
                    //fileData = fileData.Replace("\n\r", "");

                    File.AppendAllText(firstFile, fileData);

                    File.Delete(file);
                }
            }

            var fd = File.ReadAllText(csvFiles[0]);
            fd = fd.Replace("\r\n", "");
            File.WriteAllText(csvFiles[0], fd);

            MessageBox.Show("finito");
        }
    }
}
