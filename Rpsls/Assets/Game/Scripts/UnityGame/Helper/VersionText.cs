using TMPro;
using UnityEngine;

namespace Kalkatos.UnityGame
{
    public class VersionText : MonoBehaviour
    {
        [SerializeField] private TMP_Text tmpText;

		private void OnEnable ()
		{
			tmpText?.SetText($"v{Application.version}");
		}
	}
}