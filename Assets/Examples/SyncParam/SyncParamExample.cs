using System;
using System.Collections.Generic;
using UnityEngine;

namespace SyncUtil.Example
{
    public class SyncParamExample : MonoBehaviour
    {
        #region Type Define

        public enum MyEnum
        {
            One,
            Two,
            Three
        }

        [Serializable]
        public class MyClass
        {
            public int intVal;
            public float floatVal;
        }

        #endregion

        public MyEnum enumVal;
        public bool boolVal;
        public int intVal;
        public uint uintVal;
        public float floatVal;
        public string stringVal;
        public Vector2 vector2Val;
        public Vector3 vector3Val;
        public Vector4 vector4Val;
        public int[] arrayVal;
        public List<int> listVal;
        public MyClass classVal;
        public MyClass[] classArrayVal;
        public List<MyClass> classListVal;
    }
}
