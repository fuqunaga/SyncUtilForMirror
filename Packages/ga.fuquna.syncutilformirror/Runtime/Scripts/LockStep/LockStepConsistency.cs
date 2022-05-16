namespace SyncUtil
{
    public enum LockStepConsistency
    {
        NotCheckYet,
        Checking,
        Match,
        NotMatch,
        TimeOut
    }
}