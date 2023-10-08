using UnityEngine;

namespace Kalkatos.UnityGame
{
	public class DontDestroyOnLoad : MonoBehaviour
    {
		[SerializeField] private bool useGameObject = true;

		private void Awake ()
		{
			DontDestroyOnLoad(useGameObject ? gameObject : this);
		}
	}
}