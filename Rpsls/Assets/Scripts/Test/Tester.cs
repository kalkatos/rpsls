using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos.Rpsls
{
    public class Tester : MonoBehaviour
    {
        private void Awake ()
        {
            MainBehaviour.OnSomethingHappened += DoThing;
        }

        void Start()
        {
            MainBehaviour.Instance.DoSomething();
        }

        private void DoThing ()
        {
            Debug.Log("Thing started being done");
        }
    }
}
