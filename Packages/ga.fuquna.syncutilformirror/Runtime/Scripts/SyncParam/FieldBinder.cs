using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace SyncUtil
{
    public static class FieldBinder
    {
        [Serializable]
        public class FieldData
        {
            [Tooltip("field name of target")] 
            public string name;
            [Tooltip("Sync: Always sync by the server value.\nTrigger: Only sync when the server value changed.")]
            public SyncParamMode mode;
        }


        private static MethodInfo _createParamBinderMi;
        private static readonly object[] MiArgs = new object[2];

        public static IParamBinder Create(FieldData data, Object target)
        {
            var t = target.GetType();
            var name = data.name;

            FieldInfo fi = null;
            while (t != null && fi == null)
            {
                fi = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                t = t.BaseType;
            }

            Assert.IsNotNull(fi, $"Can't get field. [{target.GetType()}.{name}]");

            _createParamBinderMi ??= typeof(FieldBinder).GetMethod(nameof(CreateParamBinder), BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(_createParamBinderMi);

            var mi = _createParamBinderMi.MakeGenericMethod(fi.FieldType);
            MiArgs[0] = target;
            MiArgs[1] = data;
            
            return (IParamBinder)mi.Invoke(null, MiArgs);
        }


        private static ParamBinder<T> CreateParamBinder<T>(Object target, FieldData data)
        {
            var key = $"{nameof(FieldBinder)}/{target.name}/{data.name}"; // TODO: case _target object has multi instance

            return new ParamBinder<T>(
                key,
                data.mode,
                CreateGetValueFunc<T>(target, data.name),
                CreateSetValueAction<T>(target, data.name)
            );
        }


        static Func<T> CreateGetValueFunc<T>(Object target, string propertyOrFieldName)
        {
            // () => _target.[_data.name]
            return Expression.Lambda<Func<T>>(
                Expression.PropertyOrField(
                    Expression.Constant(target),
                    propertyOrFieldName
                )
            ).Compile();
        }

        static Action<T> CreateSetValueAction<T>(Object target, string propertyOrFieldName)
        {
            // (t) => _target.[_data.name] = t;
            var arg = Expression.Parameter(typeof(T));
            return Expression.Lambda<Action<T>>(
                Expression.Assign(
                    Expression.PropertyOrField(
                        Expression.Constant(target),
                        propertyOrFieldName
                    ),
                    arg
                ),
                arg
            ).Compile();
        }
    }
}