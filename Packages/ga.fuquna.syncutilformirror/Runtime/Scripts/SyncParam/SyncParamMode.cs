namespace SyncUtil
{
    public enum SyncParamMode
    {
        Sync,    // The client value is always overwritten by the server value
        Trigger　// The client value is only overwritten when server values change
    }
}