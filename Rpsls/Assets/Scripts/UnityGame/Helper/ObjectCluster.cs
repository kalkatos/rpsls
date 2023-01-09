using UnityEngine;
using Sirenix.OdinInspector;

namespace Kalkatos.UnityGame
{
	public class ObjectCluster : MonoBehaviour
    {
		[SerializeField] private bool useChildren = true;
		[SerializeField, HideIf(nameof(useChildren))] private GameObject[] objects;

		private void SetActive (bool b)
		{
			if (useChildren)
				for (int i = 0; i < transform.childCount; i++)
					transform.GetChild(i).gameObject.SetActive(b);
			else
				foreach (var item in objects)
					item.SetActive(b);
		}

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