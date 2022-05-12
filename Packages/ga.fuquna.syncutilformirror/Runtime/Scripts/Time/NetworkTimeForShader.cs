using UnityEngine;
using UnityEngine.Serialization;

namespace SyncUtil
{
	/// <summary>
    /// Unity's _Time style shader property
    /// </summary>
    public class NetworkTimeForShader : MonoBehaviour
    {
        [FormerlySerializedAs("_propertyName")] 
        public string propertyName = "g_Time";

        public void Update()
        {
            Shader.SetGlobalVector(propertyName, GetVector4Time() );
        }

		public static Vector4 GetVector4Time()
		{
			var time = (float)SyncNet.NetworkTime;
			return new Vector4(time / 20f, time, time * 2f, time * 3f);
		}
    }

}