using System;
using Mirror;

namespace SyncUtil
{
    public interface ILockStep
    {
        #region Required

        Func<NetworkMessage> GetDataFunc { set; }
        Func<int, NetworkReader, bool> StepFunc { set; }

        #endregion


        #region Optional

        Func<NetworkMessage> GetInitDataFunc { set; }
        Func<NetworkReader, bool> InitFunc { set; } // if return false, skip current step and call initFunc at next frame.
        Func<bool> OnMissingCatchUpServer { set; } // if return true, StopHost() will be called.
        Action OnMissingCatchUpClient { set; }
        Func<string> GetHashFunc { set; } // for CheckConsistency

        #endregion

        ConsistencyChecker.ConsistencyData GetLastConsistencyData();
        void StartCheckConsistency();

        int StepCountServer { get; }
        int StepCountClient { get; }
    }
}