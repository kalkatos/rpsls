using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Kalkatos.Network
{
    public partial class FunctionInvoker : MonoBehaviour
    {
        private static FunctionInvoker instance;
        public static FunctionInvoker Instance
        {
            get
            {
                if (instance == null)
                    instance = new GameObject(nameof(FunctionInvoker)).AddComponent<FunctionInvoker>();
                return instance;
            }
        }

        private List<Tuple<Task<object>, Action<object>, Action<object>>> delayedExecutions =
            new List<Tuple<Task<object>, Action<object>, Action<object>>>();

        protected static bool IsMasterClient => NetworkManager.Instance.MyPlayerInfo.IsMasterClient;

        public static DataAccess DataAccess => NetworkManager.Instance.DataAccess;

        private void Update ()
        {
            for (int i = delayedExecutions.Count - 1; i >= 0; i--)
            {
                var item = delayedExecutions[i];
                if (item.Item1.Status == TaskStatus.RanToCompletion)
                {
                    object result = item.Item1.Result;
                    if (result is Error)
                        item.Item3.Invoke(result);
                    else
                        item.Item2.Invoke(result);
                    delayedExecutions.RemoveAt(i);
                }
            }
        }

        public static void ExecuteFunction (string name, object parameter, Action<object> success, Action<object> failure)
        {
            var methods = typeof(FunctionInvoker).GetMethods(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++)
            {
                string methodName = methods[i].Name;
                if (methodName == name && name != nameof(ExecuteFunction))
                {
                    object result = null;
                    try
                    {
                        object[] args = new object[] { parameter };
                        result = typeof(FunctionInvoker).InvokeMember(name, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, args);
                    }
                    catch (Exception ex)
                    {
                        failure?.Invoke(ex.Message);
                        return;
                    }
                    if (result is Error)
                        failure?.Invoke(result);
                    else if (result is Task<object>)
                        Instance.delayedExecutions.Add(new Tuple<Task<object>, Action<object>, Action<object>>((Task<object>)result, success, failure));
                    else
                        success?.Invoke(result);
                    return;
                }
            }
            failure?.Invoke(Error.MethodNotFound);
        }

        #region =================== Data =================================

        public static async Task<object> GetData (string key, string container = "")
        {
            string data = await DataAccess.RequestData(key, "", container);
            if (string.IsNullOrEmpty(data))
                return null;
            return data;
        }

        public static async Task SetData (string key, string value, string container = "")
        {
            await DataAccess.SendData(key, value, container);
        }

        #endregion

        internal enum Error
        {
            WrongParameters,
            MethodNotFound,
            NotAvailable,
            NonExistent,
            NotAllowed,
            MustBeMaster,
        };
    }

    public class DataAccess
    {
#pragma warning disable
        public virtual async Task<string> RequestData (string key, string defaultValue, string container = "")
        {
            throw new NotImplementedException("RequestData is not implemented.");
        }

        public virtual async Task SendData (string key, string value, string container = "")
        {
            throw new NotImplementedException("SendData is not implemented.");
        }
#pragma warning restore
    }
}
