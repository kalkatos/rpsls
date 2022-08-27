using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos.Rpsls
{
    public class InheritedBehaviour : MainBehaviour
    {
        private void Start ()
        {
            this.Wait(2f, RaiseSomethingHappenedEvent);
        }
    }
}
