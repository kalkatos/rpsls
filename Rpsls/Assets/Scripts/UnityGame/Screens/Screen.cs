using Sirenix.OdinInspector;
using UnityEngine;
using Kalkatos.UnityGame.Signals;

namespace Kalkatos.UnityGame.Screens
{
    public class Screen : MonoBehaviour
    {
        [SerializeField] protected ScreenSignal screenSignal;
		[SerializeField] private bool useChildren;
		[SerializeField, HideIf(nameof(useChildren))] private GameObject[] objects;

		private void Awake ()
		{
			screenSignal.OnSignalEmittedWithParam.AddListener(HandleScreenSignal);
		}

		private void OnDestroy ()
		{
			screenSignal.OnSignalEmittedWithParam.RemoveListener(HandleScreenSignal);
		}

		private void HandleScreenSignal (bool value)
		{
			SetActive(value);
		}

		private void SetActive (bool b)
		{
			foreach (var item in objects)
				item.SetActive(b);
			if (useChildren)
				for (int i = 0; i < transform.childCount; i++)
					transform.GetChild(i).gameObject.SetActive(b);
			if (b)
				OnOpened();
			else
				OnClosed();
		}

		protected virtual void OnOpened () { }
		protected virtual void OnClosed () { }

		[Button]
		public void Activate ()
		{
			SetActive(true);
		}

		[Button]
		public void Deactivate ()
		{
			SetActive(false);
		}
	}
}