using UnityEngine;

namespace Kalkatos.Rpsls
{
    public class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = Resources.Load<T>(typeof(T).Name);
                return instance;
            }
        }
    }
}
