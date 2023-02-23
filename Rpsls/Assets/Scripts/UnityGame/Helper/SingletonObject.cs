using UnityEngine;

namespace Kalkatos.UnityGame
{
	public class SingletonObject : MonoBehaviour
	{
		public static SingletonObject Instance;

		[SerializeField] private bool useGameObject = true;

		private void Awake ()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(useGameObject ? gameObject : this);
		}
	}
}