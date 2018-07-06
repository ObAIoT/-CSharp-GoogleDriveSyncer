using System;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace GoogleDriveSyncer
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FormMain : Form
    {
        /// <summary>
        /// This computer running the application is a FTP server as well and
        /// the default ftp folder for incoming new files is "DefaultFTPFolder".
        /// To keep it clean, the files to upload to Google drive shall not be in the same folder;
        /// otherwise a stack overflow exception may occur.
        /// So we create a subfolder for upload to Google drive: "SubfolderNameForUploadToGoogleDrive"
        /// </summary>
        public const string DefaultFTPFolder = @"C:\IPCImages";

        /// <summary>
        /// The subfolder of default FTP folder for files to upload to Google drive.
        /// Once a new file is created in "DefaultFTPFolder", it will be moved to the subfolder and be uploaded to Google drive.
        /// </summary>
        public const string SubfolderNameForUploadToGoogleDrive = "ToUpload";

        /// <summary>
        /// The Google drive file ID of the folder for uploading files to
        /// </summary>
        public const string GDriveUploadFolderID = "1dF1q_X3tGkha0547SxSec5dnA09hTJ4J";

        /// <summary>
        /// Google secret JSON file for Google APIs
        /// </summary>
        public const string SecretFileName = "client_secret_616147814140-d9sku4lfs69hff1abeojtv23h48rb58g.apps.googleusercontent.com.json";

        /// <summary>
        /// Path for secret file, please put your secret file in the same folder as the application .exe file
        /// </summary>
        public static string DefaultJSONSecretPath = Application.StartupPath + "\\" + SecretFileName;

        /// <summary>
        /// User name
        /// </summary>
        public const string UserName = "Josh Chang";

        /// <summary>
        /// If to delete a file after being uploaded successfully
        /// </summary>
        public const bool DeleteFileAfterUploaded = true;

        /// <summary>
        /// If to upload files in "SubfolderNameForUploadToGoogleDrive" folder upon application runs
        /// Any files in "SubfolderNameForUploadToGoogleDrive" will be uploaded.
        /// Any files in "DefaultFTPFolder" will be moved to "SubfolderNameForUploadToGoogleDrive" and then be uploaded.
        /// </summary>
        public const bool UploadExistingFiles = true;

        /// <summary>
        /// Log4net
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Extended file creation watcher
        /// </summary>
        FileSystemWatcherEx watcher;

        /// <summary>
        /// If Google Drive connected
        /// </summary>
        bool connected;

        /// <summary>
        /// Constructor
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
            connected = false;

            //Default invisible
            this.Opacity = 0;
            this.trayIcon.Icon = Properties.Resources.RedLight;
        }

        /// <summary>
        /// Upload a file to Google Drive
        /// </summary>
        /// <param name="filePath"></param>
        public void Upload(string filePath)
        {
            try
            {
                string parentID = GDriveUploadFolderID;
                string fileName = Path.GetFileName(filePath);
                var ret = GoogleDriveAPIV3.uploadNewFileToDrive(filePath, fileName, parentID, this);
                if (ret != null)
                {
                    if (ret == "")
                    {
                        log.Info("Not a (new) file: " + fileName);
                    }
                    else
                    {
                        log.Info(string.Format("File \"{0}\" upload successfully, GDrive fileId : {1}", fileName, ret));
                        if (DeleteFileAfterUploaded)
                        {
                            File.Delete(filePath);
                        }
                    }
                }
                else
                {
                    log.Error("File upload failed: " + fileName);
                }
            }
            catch (Exception ex)
            {
                log.Error("Upload, ex = " + ex);
            }
        }

        /// <summary>
        /// Monitor the new created files in default FTP folder
        /// </summary>
        private void startWatcher()
        {
            watcher = new FileSystemWatcherEx(DefaultFTPFolder);
            watcher.FileCacheStateInformed += Watcher_FileCacheStateInformed;
        }

        /// <summary>
        /// Move any files in "DefaultFTPFolder" to subfolder "SubfolderNameForUploadToGoogleDrive" or
        /// any files in "SubfolderNameForUploadToGoogleDrive" to be uploaded to Google drive while the 
        /// application first being executed
        /// </summary>
        private void moveAndUploadExistingFiles()
        {
            try
            {
                var destFolderPath = Path.Combine(DefaultFTPFolder, SubfolderNameForUploadToGoogleDrive);
                var files = Directory.EnumerateFiles(DefaultFTPFolder, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var f in files)
                {
                    FileMove(f, Path.Combine(destFolderPath, Path.GetFileName(f)));
                }

                files = Directory.EnumerateFiles(destFolderPath, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var f in files)
                {
                    log.Info("Existing file to upload, path = " + f);
                    Task.Run(() => { Upload(f); });
                }
            }
            catch (Exception ex)
            {
                log.Error("moveAndUploadExistingFiles, ex = " + ex);
            }
        }

        /// <summary> 
        /// Instead of using File.Move; for faster file move with adjustable buffers; 
        /// Large file with greater the buffer; the faster the transfer.
        /// </summary>
        /// <param name="source">Source file path</param> 
        /// <param name="destination">Destination file path</param> 
        private void FileMove(string source, string destination)
        {
            try
            {
                int array_length = (int)Math.Pow(2, 10);
                byte[] dataArray = new byte[array_length];
                using (FileStream fsread = new FileStream
                (source, FileMode.Open, FileAccess.Read, FileShare.None, array_length))
                {
                    using (BinaryReader bwread = new BinaryReader(fsread))
                    {
                        using (FileStream fswrite = new FileStream
                        (destination, FileMode.Create, FileAccess.Write, FileShare.None, array_length))
                        {
                            using (BinaryWriter bwwrite = new BinaryWriter(fswrite))
                            {
                                for (;;)
                                {
                                    int read = bwread.Read(dataArray, 0, array_length);
                                    if (0 == read)
                                        break;
                                    bwwrite.Write(dataArray, 0, read);
                                }
                            }
                        }
                    }
                }
                File.Delete(source);
            }
            catch (Exception ex)
            {
                log.Error("FileMove, ex = " + ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
                //Hide the Form
                BeginInvoke(new MethodInvoker(delegate
                {
                    Hide();
                }));

                //Make sure the directory structure is ready
                Directory.CreateDirectory(Path.Combine(DefaultFTPFolder, SubfolderNameForUploadToGoogleDrive));

                //Start folder file created watcher
                startWatcher();

                //Connect to Google Drive
                if (GoogleDriveAPIV3.GoogleDriveConnection(
                      DefaultJSONSecretPath,
                      UserName))
                {
                    log.Info("Google Drive has been connected, ready to upload files...");
                    connected = true;
                    this.trayIcon.Icon = Properties.Resources.GreenLight;
                    this.trayIcon.ShowBalloonTip(100, "Status", "Google Drive connected, ready to upload files...", ToolTipIcon.Info);

                    if (UploadExistingFiles)
                    {
                        moveAndUploadExistingFiles();
                    }
                }
                else
                {
                    log.Error("Make conncection to Google Drive failed!");
                }
            }
            catch (Exception ex)
            {
                log.Error("FormMain_Load, ex = " + ex);
            }
        }

        private void Watcher_FileCacheStateInformed(object sender, FileCacheStateInformedEventArg e)
        {
            try
            {
                var info = e.FileInfo;
                if (info.FileState == FileCachedState.Ready)
                {
                    var destPath = Path.Combine(Path.GetDirectoryName(info.FilePath), SubfolderNameForUploadToGoogleDrive, Path.GetFileName(info.FilePath));
                    FileMove(info.FilePath, destPath);
                    if (connected)
                    {
                        Task.Run(() => { Upload(destPath); });
                    }
                    else
                    {
                        log.Error("Google Drive is not connected!");
                    }
                }
                else
                {
                    log.Error(string.Format("File \"{0}\" is still busy after waiting \"{1}\" seconds, abort!", DateTime.Now.Subtract(info.cachedTime)));
                }
            }
            catch (Exception ex)
            {
                log.Error("Watcher_FileCacheStateInformed, ex = " + ex);
            }
        }

        /// <summary>
        /// Event raises when Exit in context menu is being clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemExit_Click(object sender, EventArgs e)
        {
            //Confirm to close
            switch (MessageBox.Show(this, "Are you sure to exit GoogleDriverSyncer?", "Exit confirm", MessageBoxButtons.YesNo))
            {
                case DialogResult.No:
                    break;
                case DialogResult.Yes:
                    Application.Exit();
                    break;
                default:
                    break;
            }
        }
    }
}
