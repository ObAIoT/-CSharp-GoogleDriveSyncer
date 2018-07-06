using System;

namespace GoogleDriveSyncer
{
    public class FileCacheStateInformedEventArg : EventArgs
    {
        /// <summary>
        /// File information
        /// </summary>
        public CacheFileInfo FileInfo { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileInfo"></param>
        public FileCacheStateInformedEventArg(CacheFileInfo fileInfo)
        {
            FileInfo = fileInfo;
        }
    }
}
