using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;


namespace SyncUtil
{
    /// <summary>
    /// SyncNet.Time, SyncNet.NetworkTime を直線の位置で表し、複数PCでの差異を視覚的に比べる表示
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class LatencyCheckerLine : MonoBehaviour
    {
        [FormerlySerializedAs("_timeColor")]
        public Color timeColor = Color.white;
        
        [FormerlySerializedAs("_networkTimeColor")] 
        public Color networkTimeColor = Color.gray;

        public enum Mode
        {
            Horizontal,
            Vertical
        }

        public class CameraData
        {
            public Camera camera;
            public bool enable;
            public Mode mode;

            private DebugDraw _debugDraw;

            public string Name => camera.name;
            public DebugDraw DebugDraw => _debugDraw ??= (camera.GetComponent<DebugDraw>() ?? camera.gameObject.AddComponent<DebugDraw>());
        }

        public List<CameraData> DataList { get; } = new();

        float _width = 5f;
        bool _networkTimeEnable = true;
        float _networkTimeStride = 5f;

        bool _timeEnable = true;
        float _timeStride = 5f;

        protected virtual void Start()
        {
            DataList.Add(new CameraData() { camera = GetComponent<Camera>() });
        }

        public void OnPreRender()
        {
            foreach(var data in DataList.Where(data => data.enable))
            {
                if (_timeEnable) DrawLine(data, SyncNet.Time, _timeStride, timeColor);
                if (_networkTimeEnable) DrawLine(data, (float)SyncNet.NetworkTime, _networkTimeStride, networkTimeColor);
            }
        }

        protected void DrawLine(CameraData data, float val, float stride, Color col)
        {
            if (stride > 0f)
            {
                var rate = (val % stride) / stride;
                var lb = Vector2.zero;
                var rt = Vector2.one;
                var idx = data.mode == Mode.Horizontal ? 1 : 0;

                lb[idx] = rt[idx] = rate;

                data.DebugDraw.LineOn2D(lb, rt, col, _width);
            }
        }


        
        
        public virtual void DebugMenu()
        {
            enabled = GUILayout.Toggle(enabled, "LatencyCheckerLine");

            if (enabled)
            {
                GUIUtil.Indent(() =>
                {
                    _width = GUIUtil.Slider(_width, 0f, 20f, "width");

                    using (new GUILayout.HorizontalScope())
                    {
                        _timeEnable = GUILayout.Toggle(_timeEnable, "Time");
                        using(new GUIUtil.ColorScope(timeColor))
                        {
                            GUILayout.Label("■");
                        }
                        _timeStride = GUIUtil.Slider(_timeStride, 0.1f, 10f, "Stride");
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        _networkTimeEnable = GUILayout.Toggle(_networkTimeEnable, "NetworkTime");
                        using(new GUIUtil.ColorScope(networkTimeColor))
                        {
                            GUILayout.Label("■");
                        }

                        _networkTimeStride = GUIUtil.Slider(_networkTimeStride, 0.1f, 10f, "Stride");
                    }

                    DebugMenuInternal();

                    foreach(var data in DataList)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            data.enable = GUILayout.Toggle(data.enable, data.Name);
                            data.mode = GUIUtil.Field(data.mode);
                        }
                    }
                });
            }
        }

        protected virtual void DebugMenuInternal() { }
    }
}