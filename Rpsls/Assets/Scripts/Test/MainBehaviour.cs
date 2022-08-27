using System;
using UnityEngine;

namespace Kalkatos.Rpsls
{
    public class MainBehaviour : MonoBehaviour
    {
        public static MainBehaviour Instance { get; private set; }

        public static event Action OnSomethingHappened;

        private void Awake ()
        {
            Instance = this;
        }

        protected virtual void RaiseSomethingHappenedEvent ()
        {
            OnSomethingHappened?.Invoke();
        }

        public virtual void DoSomething ()
        {
            Debug.Log("Success");
        }
    }
}
