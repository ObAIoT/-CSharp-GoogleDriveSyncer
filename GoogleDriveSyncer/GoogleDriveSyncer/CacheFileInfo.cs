using System;

namespace GoogleDriveSyncer
{
    public class CacheFileInfo
    {
        /// <summary>
        /// File current state
        /// </summary>
        public FileCachedState FileState { get; set; }

        /// <summary>
        /// File current retry count
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// File name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// File path
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// File first cached time
        /// </summary>
        public DateTime cachedTime { get; set; }
    }
}
