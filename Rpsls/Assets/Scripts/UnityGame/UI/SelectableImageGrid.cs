using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kalkatos.UnityGame.UI
{
	public class SelectableImageGrid : MonoBehaviour
    {
		[PropertyOrder(99)] public UnityEvent<int> OnImageSelected;

		[SerializeField] private SpriteListScriptable spriteList;
		[SerializeField] private SelectableImage mold;
		[SerializeField] private Transform origin;
		[SerializeField] private Transform selectedImage;
		[SerializeField] private IntValueGetter startingIndex;

		private List<SelectableImage> imageList = new List<SelectableImage>();

		private void Awake ()
		{
			Populate();
		}

		private void Start ()
		{
			SetSelectedIndex(startingIndex.GetValue());
			selectedImage.gameObject.SetActive(true);
		}

		private void OnDestroy ()
		{
			foreach (var item in imageList)
				item.OnImageSelected -= HandleClickOnSelectable;
		}

		public void SetSelectedIndex (int index)
		{
			if (index < imageList.Count)
			{
				selectedImage.SetParent(imageList[index].transform, false);
				selectedImage.localPosition = Vector3.zero;
			}
		}

		[Button]
        private void Populate ()
        {
			if (spriteList == null || spriteList.Sprites == null || spriteList.Sprites.Length == 0)
				return;
			SetSprite(mold, 0);
			for (int i = 1; i < spriteList.Sprites.Length; i++)
				SetSprite(Instantiate(mold, origin), i);
		}

		private void SetSprite (SelectableImage image, int index)
		{
			image.SetSprite(spriteList.Sprites[index]);
			image.Index = index;
			if (Application.isPlaying)
			{
				image.OnImageSelected += HandleClickOnSelectable;
				imageList.Add(image);
			}
		}

		private void HandleClickOnSelectable (SelectableImage image)
		{
			selectedImage.SetParent(image.transform, false);
			OnImageSelected?.Invoke(image.Index);
		}
	}
}
