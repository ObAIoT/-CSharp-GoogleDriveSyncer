namespace GoogleDriveSyncer
{
    public enum FileCachedState
    {
        /// <summary>
        /// File is occupied by another process
        /// </summary>
        Busy, 

        /// <summary>
        /// File is ready
        /// </summary>
        Ready,

        /// <summary>
        /// After certain wait time, it is still busy.
        /// </summary>
        Timeout,
    }
}
