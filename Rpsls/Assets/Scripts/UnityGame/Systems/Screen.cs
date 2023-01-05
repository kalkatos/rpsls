using Sirenix.OdinInspector;
using UnityEngine;

namespace Kalkatos.UnityGame.Systems
{
    public class Screen : MonoBehaviour
    {
        [SerializeField] private string screenName;
		[SerializeField] private bool useChildren;
		[SerializeField] private GameObject[] objects;

		private void Awake ()
		{
			ScreenManager.OnStarted += HandleScreenStarted;
			ScreenManager.OnEnded += HandleScreenEnded;
		}

		private void OnDestroy ()
		{
			ScreenManager.OnStarted -= HandleScreenStarted;
			ScreenManager.OnEnded -= HandleScreenEnded;
		}

		private void HandleScreenStarted (string name)
		{
			if (name != screenName)
				return;
			SetActive(true);
		}

		private void HandleScreenEnded (string name)
		{
			if (name != screenName)
				return;
			SetActive(false);
		}

		private void SetActive (bool b)
		{
			foreach (var item in objects)
				item.SetActive(b);
			if (useChildren)
				for (int i = 0; i < transform.childCount; i++)
					transform.GetChild(i).gameObject.SetActive(b);
		}

		[Button]
		public void Activate ()
		{
			SetActive(true);
		}

		[Button]
		public void Deativate ()
		{
			SetActive(false);
		}
	}
}