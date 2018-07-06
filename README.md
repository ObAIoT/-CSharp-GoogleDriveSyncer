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
      + https://github.com/Obrelix/.net-Google-Drive-API-v3-File-Handling (e4ab28d on 4 Jun 2017)<br/><br/>
  
# References<br/>
  1. GoogleDriveManager- e4ab28d on 4 Jun 2017(https://github.com/Obrelix/.net-Google-Drive-API-v3-File-Handling) - show how to use Google Drive API<br/>
     GTools.cs / GoogleDriveUploader.cs / MimeTypeConverter.cs are from 
     https://github.com/Obrelix/.net-Google-Drive-API-v3-File-Handling (e4ab28d on 4 Jun 2017) 
     with minor modifications<br/><br/>
  2. FileSystemWatcherMemoryCache - An extension to [FileSystemWatcher](https://msdn.microsoft.com/en-us/library/system.io.filesystemwatcher(v=vs.110).aspx) with the capability to trigger event whenever a new created file is ready.<br/> 
      + https://github.com/benbhall/FileSystemWatcherMemoryCache (c788b20 on 25 Nov 2017)<br/>
      
      
      
      how to apply for a google secret
      log4net
