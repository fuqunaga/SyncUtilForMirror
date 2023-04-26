using UnityEngine;
using UnityEngine.Serialization;

namespace SyncUtil.Example
{
    [RequireComponent(typeof(LifeGame))]
    public class LifeGameUpdater : MonoBehaviour
    {
        private static int _lastWidth;
        private static int _lastHeight;
        
        [FormerlySerializedAs("_resolutionScale")] public float resolutionScale = 0.5f;
        private LifeGame _lifeGame;

        private void Start()
        {
            _lifeGame = GetComponent<LifeGame>();
        }


        private void Update()
        {
            _lifeGame.Step(CreateStepData(resolutionScale));
        }

        public static void Reset()
        {
            _lastWidth = _lastHeight = 0;
        }

        public static LifeGame.StepData CreateStepData(float resolutionScale)
        {
            var isResize = (Screen.width != _lastWidth) || (Screen.height != _lastHeight);
            if (isResize)
            {
                _lastWidth = Screen.width;
                _lastHeight = Screen.height;
            }

            return new LifeGame.StepData()
            {
                isResize = isResize,
                width = Mathf.FloorToInt(_lastWidth * resolutionScale),
                height = Mathf.FloorToInt(_lastHeight * resolutionScale),
                randSeed = Mathf.FloorToInt(Random.value * int.MaxValue),
                isInputEnable = Input.GetMouseButton(0),
                inputPos = Input.mousePosition * resolutionScale,
                deltaTime = Time.deltaTime
            };

        }
    }
}