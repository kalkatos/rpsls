using Kalkatos.UnityGame;
using UnityEngine;

[CreateAssetMenu(fileName = "AutoPlaySettings", menuName = "Rps/AutoPlaySettings")]
public class AutoPlaySettings : SingletonScriptableObject<AutoPlaySettings>
{
    public bool AutoPlay;
}
