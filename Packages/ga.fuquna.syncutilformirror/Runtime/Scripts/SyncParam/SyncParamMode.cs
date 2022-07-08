namespace SyncUtil
{
    public enum SyncParamMode
    {
        Sync,    // Always sync by the server value.
        Trigger　// Only sync when the server value changed.
    }
}