# GoogleDriveSyncer
C# Windows Form Application<br/>
Upload to Google drive from FTP server synchronously upon new files created<br/>

# Scenario
* A FTP server running Windows Server
* FTP clients to upload files to the server
* The server uploads files upon creation to Google drive in a synchronous way.<br/><br/>

# Nuget packages to install<br/>
  1. log4net - logging<br/>
  2. Google.Apis.Drive.v3 - upload files to Google drive<br/>
  3. Newtonsoft.Json - parsing Google API JSON secret<br/>
  4. Ionic.Zip - Although we don't compress files in this project, to make the reference code working
      + GoogleDriveManager@e4ab28d on 4 Jun 2017
<br/><br/>
  
# References<br/>
  1. [GoogleDriveManager@e4ab28d on 4 Jun 2017](https://github.com/Obrelix/.net-Google-Drive-API-v3-File-Handling) - show how to use Google Drive API<br/>
     GTools.cs / GoogleDriveUploader.cs / MimeTypeConverter.cs are with minor modifications<br/><br/>
  2. [FileSystemWatcherMemoryCache@c788b20 on 25 Nov 2017](https://github.com/benbhall/FileSystemWatcherMemoryCache) - An extension to [FileSystemWatcher](https://msdn.microsoft.com/en-us/library/system.io.filesystemwatcher(v=vs.110).aspx) with the capability to know when a file is done with FTP client transfer and is ready to upload to Google drive.<br/> 
      
      
      
# Documents
* [4 Steps to Empower Logging for your Windows Application Development with Log4Net](https://drive.google.com/file/d/1aTPz7TWOUhI6jBQNInqq6OMeKLcG62Qz/view?usp=sharing)

