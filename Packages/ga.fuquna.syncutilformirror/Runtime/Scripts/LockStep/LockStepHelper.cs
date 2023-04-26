using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace SyncUtil
{
    public static class LockStepHelper
    {
        #region Type Define
        
        private interface IBufferWrapper
        {
            int ByteCount { get; }
            AsyncGPUReadbackRequest AsyncGPUReadbackRequest(ref NativeArray<byte> nativeArray, Action<AsyncGPUReadbackRequest> callback = null);
        }
        
        private readonly struct ComputeBufferWrapper : IBufferWrapper
        {
            private readonly ComputeBuffer _buffer;

            public ComputeBufferWrapper(ComputeBuffer buffer)
            {
                _buffer = buffer;
            }

            public int ByteCount => _buffer.count * _buffer.stride;

            public AsyncGPUReadbackRequest AsyncGPUReadbackRequest(ref NativeArray<byte> nativeArray, Action<AsyncGPUReadbackRequest> callback = null)
            {
                return AsyncGPUReadback.RequestIntoNativeArray(ref nativeArray, _buffer, callback);
            }
        }
        
        private readonly struct GraphicsBufferWrapper : IBufferWrapper
        {
            private readonly GraphicsBuffer _buffer;

            public GraphicsBufferWrapper(GraphicsBuffer buffer)
            {
                _buffer = buffer;
            }

            public int ByteCount => _buffer.count * _buffer.stride;

            public AsyncGPUReadbackRequest AsyncGPUReadbackRequest(ref NativeArray<byte> nativeArray, Action<AsyncGPUReadbackRequest> callback = null)
            {
                return AsyncGPUReadback.RequestIntoNativeArray(ref nativeArray, _buffer, callback);
            }
        }
        
        #endregion
        
        
        private static HashAlgorithm _algorithm;
        
  
        // public static async ValueTask<string> GenerateBufferHash(params GraphicsBuffer[] buffers)
        // {
        //
        //     return await GenerateBufferHash(new GraphicsBufferWrapper(buffer));
        // }
        
        public static async ValueTask<string> GenerateBufferHash(GraphicsBuffer buffer)
        {
            if (buffer == null)
            {
                Debug.LogWarning("ComputeBuffer is null.");
                return "";
            }

            return await GenerateBufferHash(new GraphicsBufferWrapper(buffer));
        }
        
        public static async ValueTask<string> GenerateBufferHash(ComputeBuffer buffer)
        {
            if (buffer == null)
            {
                Debug.LogWarning("ComputeBuffer is null.");
                return "";
            }

            return await GenerateBufferHash(new ComputeBufferWrapper(buffer));
        }

        

        private static async ValueTask<string> GenerateBufferHash(IBufferWrapper bufferWrapper)
        {
            var nativeArray = new NativeArray<byte>(bufferWrapper.ByteCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            var tcs = new TaskCompletionSource<bool>();
            var request = bufferWrapper.AsyncGPUReadbackRequest(ref nativeArray, _ => tcs.SetResult(true));
            
            await tcs.Task;
            
            Assert.IsTrue(request.done);
            Assert.IsFalse(request.hasError);

            var hashString = NativeArrayToHash(nativeArray);
            nativeArray.Dispose();
            
            return hashString;
        }
        

        public static string NativeArrayToHash(NativeArray<byte> nativeArray)
        {
            _algorithm ??= MD5.Create();
            Assert.IsNotNull(_algorithm);

            byte[] hash;
            unsafe
            {
                var stream = new UnmanagedMemoryStream((byte*)nativeArray.GetUnsafeReadOnlyPtr(), nativeArray.Length);
                hash = _algorithm.ComputeHash(stream);
            }

            return BitConverter.ToString(hash);
        }



        public static string GenerateBufferHash<T>(GraphicsBuffer buffer) where T : struct
        {
            var count = buffer.count;
            var datas = new T[count];
            buffer.GetData(datas);

            return GenerateBufferHash(count, datas);
        }


        public static string GenerateBufferHash<T>(ComputeBuffer buffer) where T : struct
        {
            var count = buffer.count;
            var datas = new T[count];
            buffer.GetData(datas);

            return GenerateBufferHash(count, datas);
        }


        private static string GenerateBufferHash<T>(int count, T[] datas) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));

            var ptr = Marshal.AllocHGlobal(size);
            var bytes = new byte[size * count];
            for (var i = 0; i < datas.Length; ++i)
            {
                Marshal.StructureToPtr(datas[i], ptr, false);
                Marshal.Copy(ptr, bytes, i * size, size);
            }
            Marshal.FreeHGlobal(ptr);

            var algorithm = HashAlgorithm.Create("SHA256");

            var hash = algorithm.ComputeHash(bytes);
            algorithm.Clear();

            return hash.Aggregate("", (result, b) => result + b.ToString("X2"));
        }
    }
}