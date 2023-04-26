using UnityEngine;

namespace SyncUtil.Test
{
    public class TestLockStepHelpderPerformance : MonoBehaviour
    {
        public int stride = 100;
        public int count = 100;

        void Update()
        {
            DoTest();
        }

        private async void DoTest()
        {
            var buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, stride);
            // var buffer = new ComputeBuffer( count, stride);
            
            var hash = await LockStepHelper.GenerateBufferHash(buffer);
            
            buffer.Dispose();
        }
    }
}