using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kalkatos.UnityGame.UI
{
	public class SelectableImage : MonoBehaviour
    {
        public Action<SelectableImage> OnImageSelected;

        public int Index;

        [SerializeField] private Image image;
        [SerializeField] private Button button;

		private void Awake ()
		{
            button.onClick.AddListener(HandleButtonClick);
		}

		private void OnDestroy ()
		{
			button.onClick.RemoveListener(HandleButtonClick);
		}

		public void SetSprite (Sprite sprite)
        {
            image.sprite = sprite;
		}

        private void HandleButtonClick ()
        {
			OnImageSelected?.Invoke(this);
		}
    }
}
