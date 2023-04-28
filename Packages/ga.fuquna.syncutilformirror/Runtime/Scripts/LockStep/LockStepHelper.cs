using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
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

            public static IBufferWrapper Create<TBuffer>(TBuffer buffer)
            {
                return buffer switch
                {
                    ComputeBuffer computeBuffer => new ComputeBufferWrapper(computeBuffer),
                    GraphicsBuffer graphicsBuffer => new GraphicsBufferWrapper(graphicsBuffer),
                    _ => throw new ArgumentException($"buffer type {typeof(TBuffer)} is not supported")
                };
            }
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
        
        
        public static Task<string> GenerateBufferHashAsync(params GraphicsBuffer[] buffers) =>
            GenerateBufferHashAsync(buffers.AsEnumerable());
        
        public static Task<string> GenerateBufferHashAsync(IEnumerable<GraphicsBuffer> buffers)
        {
            return GenerateBufferHashAsync(buffers.Select(IBufferWrapper.Create));
        }
        
        public static Task<string> GenerateBufferHashAsync(params ComputeBuffer[] buffers) =>
            GenerateBufferHashAsync(buffers.AsEnumerable());
        
        public static Task<string> GenerateBufferHashAsync(IEnumerable<ComputeBuffer> buffers)
        {
            return GenerateBufferHashAsync(buffers.Select(IBufferWrapper.Create));
        }
        

        private static async Task<string> GenerateBufferHashAsync(IEnumerable<IBufferWrapper> bufferWrappers)
        {
            using var p0 = ListPool<NativeArray<byte>>.Get(out var nativeArrays);
            using var p1 = ListPool<Task<bool>>.Get(out var tasks);

            foreach(var bufferWrapper in bufferWrappers)
            {
                var nativeArray = new NativeArray<byte>(bufferWrapper.ByteCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                

                var tcs = new TaskCompletionSource<bool>();
                bufferWrapper.AsyncGPUReadbackRequest(ref nativeArray, request => tcs.SetResult(request.hasError));
                
                nativeArrays.Add(nativeArray);
                tasks.Add(tcs.Task);
            }
            
            var hasErrors = await Task.WhenAll(tasks);
            Assert.IsTrue(hasErrors.All(hasError => !hasError));
            
            // ハッシュ求める処理重いので別スレッドにする
            var hashString = await Task.Run(() =>
            {
                var algorithm = MD5.Create();
                Assert.IsNotNull(algorithm);

                var maxSize = nativeArrays.Max(nativeArray => nativeArray.Length);
                var bytes = ArrayPool<byte>.Shared.Rent(maxSize);

                foreach (var nativeArray in nativeArrays)
                {
                    var size = nativeArray.Length;
                    NativeArray<byte>.Copy(nativeArray, bytes, size);
                    algorithm.TransformBlock(bytes, 0, size, null, 0);
                }

                ArrayPool<byte>.Shared.Return(bytes);

                algorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);


                return BitConverter.ToString(algorithm.Hash);
            });

            foreach (var nativeArray in nativeArrays)
            {
                nativeArray.Dispose();
            }

            return hashString;
        }
   
        [Obsolete("Use GenerateBufferHashAsync")]
        public static string GenerateBufferHash<T>(GraphicsBuffer buffer) where T : struct
        {
            var task = GenerateBufferHashAsync(buffer);
            task.Wait();
            return task.Result;
        }


        [Obsolete("Use GenerateBufferHashAsync")]
        public static string GenerateBufferHash<T>(ComputeBuffer buffer) where T : struct
        {
            var task = GenerateBufferHashAsync(buffer);
            task.Wait();
            return task.Result;
        }
    }
}