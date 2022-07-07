using RosettaUI;

namespace SyncUtil.Example
{
    public class SyncParamPerformanceTestUI : ExampleUIBase
    {
        public SyncParamPerformanceTest syncParamPerformanceTest;
        protected override Element CreateElement()
        {
            return UI.Page(
                syncParamPerformanceTest.CreateElement(null)
            );
        }
    }
}