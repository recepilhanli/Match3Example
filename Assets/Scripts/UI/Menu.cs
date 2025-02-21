using Cysharp.Threading.Tasks;
using Game.Utils;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{

    public class Menu : MonoBehaviour
    {

        [Header("Menu Properties")]
        [SerializeField] private TextMeshProUGUI _titleTMP;
        [SerializeField] private Image _loadingFadeImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Transform _buttonTransform;
        [SerializeField] private Transform _levelSelectionTransform;
        [SerializeField] private ParticleSystem _leavesParticleSystem;
        [SerializeField] private AudioSource _menuMusicAudioSource;
        [SerializeField] private AudioClip _buttonTapClip;


        private bool _titleColorAnimationFlag = false;
        private bool _isLoadingGame = false;
        private Sequence _titleSequence;

        private void Start()
        {
            PrimeTweenConfig.SetTweensCapacity(15);
            _loadingFadeImage.gameObject.SetActive(false);
            TitleAnimation();
            ButtonAnimation();

#if !UNITY_EDITOR && UNITY_ANDROID
            Application.targetFrameRate = 60;
#endif
        }


        private void TitleAnimation()
        {
            void SetTitleColor() // Local function
            {
                _titleColorAnimationFlag = !_titleColorAnimationFlag;

                if (_titleColorAnimationFlag) Tween.Color(_titleTMP, CachedColors.menuTitleColor2, .2f, Ease.InOutCirc);
                else Tween.Color(_titleTMP, CachedColors.menuTitleColor1, .2f, Ease.InOutCirc);
            }

            Vector3 strength = new Vector3(0.1f, 0.1f, 0.1f);
            _titleSequence = Sequence.Create(-1, CycleMode.Rewind, Ease.InOutCirc);
            Tween scaleTween = Tween.ShakeScale(_titleTMP.transform, strength, .5f, 2);
            Tween rotateTween = Tween.LocalRotation(_titleTMP.transform, new Vector3(0, 0, 1), new Vector3(0, 0, -1f), .5f);
            Tween colorTween = Tween.Delay(.125f).OnComplete(SetTitleColor);
            _titleSequence.Group(scaleTween).Group(rotateTween).Group(colorTween);
        }

        private void ButtonAnimation()
        {
            Tween.PunchScale(_buttonTransform, new Vector3(0.025f, 0.025f, 0.025f), 1f, 4, easeBetweenShakes: Ease.InOutCirc, cycles: -1);
        }

        public void PlayButtonClip() => _menuMusicAudioSource.PlayOneShot(_buttonTapClip, 1f);

        public void Play(int index)
        {
            if (_isLoadingGame) return;
            PlayAsync(index).Forget();
        }

        private async UniTaskVoid PlayAsync(int index)
        {
            HideLevelSelection();
            _leavesParticleSystem.Stop();
            _loadingFadeImage.gameObject.SetActive(true);
            _isLoadingGame = true;
            Tween fadeTween = Tween.Alpha(_loadingFadeImage, 0, 1, .5f, ease: Ease.Linear);
            Tween backgroundTween = Tween.UIAnchoredPositionX(_backgroundImage.rectTransform, 0, 2f, ease: Ease.InOutSine);
            _ = Tween.AudioVolume(_menuMusicAudioSource, 0f, .5f, ease: Ease.InOutSine);
            await fadeTween;
            backgroundTween.Stop();
            _titleSequence.Stop();

            await UniTask.DelayFrame(3);
            SceneManager.LoadScene(index);
        }

        public void HideLevelSelection()
        {
            if (!_levelSelectionTransform.gameObject.activeSelf) return;
            Tween.Scale(_levelSelectionTransform, Vector3.zero, .25f, ease: Ease.InOutBounce).OnComplete(DisableLevelSelection);
            PlayButtonClip();
        }

        public void ShowLevelSelection()
        {
            _levelSelectionTransform.gameObject.SetActive(true);
            Vector3 startScale = new Vector3(0.75f, 0.75f, 0.75f);
            Tween.Scale(_levelSelectionTransform, startScale, Vector3.one, .2f, ease: Ease.InOutSine);
            PlayButtonClip();
        }

        private void DisableLevelSelection() => _levelSelectionTransform.gameObject.SetActive(false);


    }

}