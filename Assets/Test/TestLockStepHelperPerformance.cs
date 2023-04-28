using System.Linq;
using UnityEngine;

namespace SyncUtil.Test
{
    public class TestLockStepHelperPerformance : MonoBehaviour
    {
        public int bufferCount = 100;
        public int stride = 100;
        public int count = 100;

        void Update()
        {
            DoTest();
        }

        private async void DoTest()
        {
            var buffers = Enumerable.Range(0, bufferCount).Select(_ =>
                new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, stride)
            ).ToList();
            
            
            // var buffer = new ComputeBuffer( count, stride);
            
            var hash = await LockStepHelper.GenerateBufferHashAsync(buffers);
            Debug.Log(hash);
            
            

            foreach (var buffer in buffers)
            {
                buffer.Dispose();
            }
        }
    }
}