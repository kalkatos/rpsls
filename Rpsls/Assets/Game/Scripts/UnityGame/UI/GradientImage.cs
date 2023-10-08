using UnityEngine;
using UnityEngine.UI;

namespace Kalkatos.UnityGame.UI
{
    public class GradientImage : MonoBehaviour
    {
        [SerializeField] private Gradient gradient;
        [SerializeField] private Image image;

        public void SetGradient (float value)
        {
            image.color = gradient.Evaluate(value);
        }
    }
}