using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class FadeOutImage : MonoBehaviour
    {

        [SerializeField] private Image _image;
        [SerializeField] private float _duration = .5f;
        [SerializeField] private bool _disableAfterFade = true;

        void Start()
        {
            Tween.Alpha(_image, 0, _duration, Ease.Linear).OnComplete(OnFadeOutComplete);
        }

        private void OnFadeOutComplete()
        {
            if (!_disableAfterFade) return;
            gameObject.SetActive(false);
        }

    }
}
