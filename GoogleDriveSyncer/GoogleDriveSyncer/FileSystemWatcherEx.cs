using System;
using System.IO;
using System.Runtime.Caching;

namespace GoogleDriveSyncer
{
    public class FileSystemWatcherEx: FileSystemWatcher
    {
        /// <summary>
        /// Log4net
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Memory cache
        /// </summary>
        private readonly MemoryCache _memCache;

        /// <summary>
        /// Cache item policy 
        /// </summary>
        private readonly CacheItemPolicy _cacheItemPolicy;

        /// <summary>
        /// Cache expiration time and retries
        /// </summary>
        private const int InitialCacheTimeSeconds = 3; //First wait time
        //3*1=3, 3*2=6, 3*4=12, 3*8=24, 3*16=48, 3*32=96 seconds:
        //i.e max total wait time for a file upload since it has been created is 3+6+12+24+48+96=...
        //Adjust retries and "InitialCacheTimeSeconds" accordingly to fit your needs.
        private const int MaxRetries = 5;

        /// <summary>
        /// Event raises when platform has been renamed
        /// </summary>
        public event EventHandler<FileCacheStateInformedEventArg> FileCacheStateInformed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filter"></param>
        public FileSystemWatcherEx(string path, string filter = "*.*")
        {
            _memCache = MemoryCache.Default;
            _cacheItemPolicy = new CacheItemPolicy
            {
                RemovedCallback = OnRemovedFromCache
            };

            this.Path = path;
            this.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName; 
            this.Filter = filter;
            this.EnableRaisingEvents = true;

            //Events to handle
            this.Created += FileSystemWatcherEx_Created;
        }

        /// <summary>
        /// Check if file is ready or FTP client is still in the middle of uploading 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        protected static bool IsFileLocked(string filePath)
        {
            FileStream stream = null;
            var file = new FileInfo(filePath);

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                stream?.Close();
            }
            return false;
        }

        /// <summary>
        /// Add file event to cache (won't add if already there so assured of only one occurrence)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileSystemWatcherEx_Created(object sender, FileSystemEventArgs e)
        {
            _cacheItemPolicy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(InitialCacheTimeSeconds);

            var fileData = new CacheFileInfo()
            {
                FileState = FileCachedState.Busy,
                FilePath = e.FullPath,
                RetryCount = 0,
                FileName = e.Name,
                cachedTime = DateTime.Now
            };

            _memCache.AddOrGetExisting(e.Name, fileData, _cacheItemPolicy);
        }

        /// <summary>
        /// Handle cache item expiring 
        /// </summary>
        /// <param name="args"></param>
        private void OnRemovedFromCache(CacheEntryRemovedArguments args)
        {
            // Checking if expired, for a bit of future-proofing
            if (args.RemovedReason != CacheEntryRemovedReason.Expired) return;

            var cacheItemValue = (CacheFileInfo)args.CacheItem.Value;

            // If file is locked, send it back into the cache again
            // Could make the expiration period scale exponentially on retries
            if (IsFileLocked(cacheItemValue.FilePath))
            {
                if (cacheItemValue.RetryCount <= MaxRetries)
                {
                    cacheItemValue.RetryCount++;
                    _cacheItemPolicy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(InitialCacheTimeSeconds * Math.Pow(2, cacheItemValue.RetryCount));
                    _memCache.Add(cacheItemValue.FileName, cacheItemValue, _cacheItemPolicy);
                    return;
                }

                cacheItemValue.FileState = FileCachedState.Timeout;
                //log.Error(string.Format("OnRemovedFromCache, the file \"{0}\" is still occupied by another process, abort!"));
            }
            else
            {
                cacheItemValue.FileState = FileCachedState.Ready;
            }

            OnFileCacheStateInformed(new FileCacheStateInformedEventArg(cacheItemValue));
        }

        #region Events handlers

        protected virtual void OnFileCacheStateInformed(FileCacheStateInformedEventArg e)
        {
            var handler = FileCacheStateInformed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion 
    }
}
