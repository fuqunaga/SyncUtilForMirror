using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;


namespace SyncUtil
{
    /// <summary>
    /// SyncNet.Time, SyncNet.NetworkTime を直線の位置で表し、クライアント間の差異を視覚的に比べる表示
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class LatencyCheckerLine : MonoBehaviour
    {
        #region Type Define
        
        public enum Mode
        {
            Horizontal,
            Vertical
        }
        
        public class CameraData
        {
            public Camera camera;
            public bool enable = true;
            public Mode mode = default;

            private DebugDraw _debugDraw;

            public string Name => camera.name;
            public DebugDraw DebugDraw => _debugDraw ??= (camera.GetComponent<DebugDraw>() ?? camera.gameObject.AddComponent<DebugDraw>());
        }

        #endregion
        
        

        [Range(0f, 20f)]
        public float width = 5f;
        [Range(0.1f, 10f)]
        public float timeStride = 5f;
        
        public bool timeEnable = true;
        public Color timeColor = Color.white;

        public bool networkTimeEnable = true;
        public Color networkTimeColor = Color.gray;
        

        public List<CameraData> DataList { get; } = new();
        
        
        #region Unity
        
        protected virtual void Start()
        {
            DataList.Add(new CameraData() { camera = GetComponent<Camera>() });
        }

        public void OnPreRender()
        {
            foreach(var data in DataList.Where(data => data.enable))
            {
                if (timeEnable) DrawLine(data, SyncNet.Time, timeStride, timeColor);
                if (networkTimeEnable) DrawLine(data, (float)SyncNet.NetworkTime, timeStride, networkTimeColor);
            }
        }
        
        #endregion
        

        protected void DrawLine(CameraData data, float val, float stride, Color col)
        {
            if (stride > 0f)
            {
                var rate = (val % stride) / stride;
                var lb = Vector2.zero;
                var rt = Vector2.one;
                var idx = data.mode == Mode.Horizontal ? 1 : 0;

                lb[idx] = rt[idx] = rate;

                data.DebugDraw.LineOn2D(lb, rt, col, width);
            }
        }
    }
}