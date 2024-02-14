using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using Random = System.Random;

namespace SyncUtil.Example
{

    [RequireComponent(typeof(Camera))]
    public class LifeGame : MonoBehaviour
    {
        private static class CommonParam
        {
            public static readonly int Width = Shader.PropertyToID("_Width");
            public static readonly int Height = Shader.PropertyToID("_Height");
        }

        private static class CsParam
        {
            public const string KernelStep = "Step";
            public static readonly int WriteBuf = Shader.PropertyToID("_WriteBuf");
            public static readonly int ReadBuf = Shader.PropertyToID("_ReadBuf");

            public const string KernelInput = "Input";
            public static readonly int InputPos = Shader.PropertyToID("_InputPos");
            public static readonly int InputRadius = Shader.PropertyToID("_InputRadius");
        }

        private static class ShaderParam
        {
            public static readonly int Buf = Shader.PropertyToID("_Buf");
        }

        private struct Data
        {
            public int alive;
        }

        public class StepData
        {
            public bool isResize;
            public int width;
            public int height;
            public int randSeed;
            public bool isInputEnable;
            public Vector2 inputPos;
            public float deltaTime;
        }
        
        
        [Header("CS")]
        public ComputeShader cs;
        public float stepInterval = 0.1f;
        public float initialAliveRate = 0.2f;
        public float inputRadius = 10f;

        [Header("Render")]
        public Shader shader;

        [Header("Reproducibility")]
        // for inspector(and copy for reproducibility)
        public int width;
        public int height;
        public int seed;
        
        private Material _mat;
        private float _interval = 0f;
        private GraphicsBuffer _writeBuffer;
        private GraphicsBuffer _readBuffer;
        


        public GraphicsBuffer ReadBuffer => _readBuffer;
        
        private void Start()
        {
            _mat = new Material(shader);
        }

        private void OnDestroy()
        {
            DestroyBuffers();
            if (_mat != null) Destroy(_mat);
        }

        private void DestroyBuffers()
        {
            if (_readBuffer != null) { _readBuffer.Release(); _readBuffer = null; }
            if (_writeBuffer != null) { _writeBuffer.Release(); _writeBuffer = null; }
        }

        
        public void Step(StepData data)
        {
            if (data.isResize)
            {
                DoResize(data);
            }

            if (data.isInputEnable)
            {
                DoInput(data);
            }
            _interval -= data.deltaTime;

            if (_interval <= 0f)
            {
                DoStep();
                _interval = Mathf.Max(0f, _interval + stepInterval);
            }
        }


        private void DoResize(StepData data)
        {
            width = width <= 0 ? data.width : width;
            height = height <= 0 ? data.height : height;

            cs.SetInt(CommonParam.Width, width);
            cs.SetInt(CommonParam.Height, height);

            DestroyBuffers();

            seed = (seed <= 0) ? data.randSeed : seed;
            var rand = new Random(seed);
            var gridNum = width * height;

            _readBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, gridNum, Marshal.SizeOf(typeof(Data)));
            _writeBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, gridNum, Marshal.SizeOf(typeof(Data)));

            var nativeArray = new NativeArray<Data>(gridNum, Allocator.Temp);
            for (var i = 0; i < gridNum; ++i)
            {
                nativeArray[i] = new Data() { alive = (rand.NextDouble() < initialAliveRate) ? 1 : 0 };
            }
            
            _readBuffer.SetData(nativeArray);

            nativeArray.Dispose();
        }


        private void DoInput(StepData data)
        {
            var kernel = cs.FindKernel(CsParam.KernelInput);
            cs.SetVector(CsParam.InputPos, data.inputPos);
            cs.SetFloat(CsParam.InputRadius, inputRadius);
            cs.SetBuffer(kernel, CsParam.WriteBuf, _readBuffer);

            Dispatch(cs, kernel, new Vector3(width, height, 1));
        }

        private void DoStep()
        {
            var kernel = cs.FindKernel(CsParam.KernelStep);

            cs.SetBuffer(kernel, CsParam.ReadBuf, _readBuffer);
            cs.SetBuffer(kernel, CsParam.WriteBuf, _writeBuffer);

            Dispatch(cs, kernel, new Vector3(width, height, 1));

            (_readBuffer, _writeBuffer) = (_writeBuffer, _readBuffer);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_readBuffer == null) return;
            _mat.SetInt(CommonParam.Width, width);
            _mat.SetInt(CommonParam.Height, height);
            _mat.SetBuffer(ShaderParam.Buf, ReadBuffer);
            Graphics.Blit(source, destination, _mat);
        }

        private static void Dispatch(ComputeShader cs, int kernel, Vector3 threadNum)
        {
            cs.GetKernelThreadGroupSizes(kernel, out var x, out var y, out var z);
            cs.Dispatch(kernel, Mathf.CeilToInt(threadNum.x / x), Mathf.CeilToInt(threadNum.y / y), Mathf.CeilToInt(threadNum.z / z));
        }
    }
}