using UnityEngine;

namespace Kalkatos.FirecardEngine
{
    public class ResourceSpriteFieldWithVariation : FieldBehaviour
    {
        [SerializeField] private string variationFieldName;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public override void Setup (CardData data)
        {
            if (spriteRenderer == null)
            {
				this.LogWarning("Sprite renderer is null.");
				return; 
            }
            string spriteName = data.GetFieldValue(FieldName);
            if (string.IsNullOrEmpty(spriteName))
            {
				this.LogWarning($"Field {FieldName} value is empty.");
				return; 
            }
            string variation = data.GetFieldValue(variationFieldName);
			if (string.IsNullOrEmpty(variation))
			{
				this.LogWarning($"Field {variationFieldName} value is empty.");
				return;
			}
			if (spriteName.EndsWith(".png"))
                spriteName = spriteName.Replace(".png", "");
            Sprite sprite = Resources.Load<Sprite>($"{spriteName}-{variation}");
            if (sprite == null)
            {
				this.LogWarning($"Image {spriteName} not found in Resources folders.");
				return; 
            }
            spriteRenderer.sprite = sprite;
        }
    }
}
