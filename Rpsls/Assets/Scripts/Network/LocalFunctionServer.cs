using System;
using System.Reflection;
using UnityEngine;

namespace Kalkatos.Network
{
    internal static partial class LocalFunctionServer
    {
        internal enum Error
        {
            WrongParameters,
            MethodNotFound,
            NotAvailable,
        };

        internal static void ExecuteFunction (string name, object[] parameters, Action<object> success, Action<object> failure)
        {
            var methods = typeof(LocalFunctionServer).GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++)
            {
                string methodName = methods[i].Name;
                if (methodName == name && name != nameof(ExecuteFunction))
                {
                    object result = typeof(LocalFunctionServer).InvokeMember(name, BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, parameters);
                    if (result is Error)
                        failure?.Invoke(result);
                    else
                        success?.Invoke(result);
                    return;
                }
            }
            failure?.Invoke(Error.MethodNotFound);
        }

        internal static object Foo (object[] parameters)
        {
            return parameters[0];
        }

        
    }
}
