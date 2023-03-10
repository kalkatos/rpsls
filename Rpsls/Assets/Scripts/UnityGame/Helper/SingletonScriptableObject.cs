using UnityEngine;

namespace Kalkatos.UnityGame
{
    public class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    T[] allInstances = Resources.LoadAll<T>("");
                    if (allInstances == null || allInstances.Length == 0)
						UnityEngine.Debug.LogError($"No instances of ScriptableObject {typeof(T).Name} were found!");
                    else
                    {
                        instance = allInstances[0];
                        if (allInstances.Length > 1)
							UnityEngine.Debug.LogWarning($"More than one instance of ScriptableObject {typeof(T).Name} were found.");
                    }
                }
                return instance;
            }
        }
    }
}
