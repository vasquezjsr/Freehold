using UnityEngine;
using UnityEngine.UI;

namespace Freehold.UI
{
    public class InteractionPrompt : MonoBehaviour
    {
        Text _label;
        float _flashUntil;

        public void Bind(Text label)
        {
            _label = label;
            Hide();
        }

        public void Show(string message)
        {
            if (_label == null || Time.time < _flashUntil)
            {
                return;
            }

            _label.gameObject.SetActive(true);
            _label.text = message;
        }

        public void Hide()
        {
            if (_label == null || Time.time < _flashUntil)
            {
                return;
            }

            _label.text = string.Empty;
            _label.gameObject.SetActive(false);
        }

        public void Flash(string message, float duration = 1.4f)
        {
            if (_label == null)
            {
                return;
            }

            _flashUntil = Time.time + duration;
            _label.gameObject.SetActive(true);
            _label.text = message;
        }
    }
}
