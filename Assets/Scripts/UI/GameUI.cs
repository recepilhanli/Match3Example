using System;
using Cysharp.Threading.Tasks;
using Game.Level;
using Game.Utils;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Playground UI
    /// </summary>

    public class GameUI : MonoSingleton<GameUI>
    {

        [Header("Game UI Properties")]
        [SerializeField] private RawImage _backgroundImage;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Image _fadeImage;

        private bool _isReturningMenu = false;


        private void Start()
        {
            BackgroundLoop();
        }


        #region  UI Interactions

        public void PauseGame()
        {
            if (_isReturningMenu || Time.timeScale == 0f) return;
            Time.timeScale = 0;
            _fadeImage.raycastTarget = true;
            Tween.Alpha(_fadeImage, 0f, 0.5f, 0.3f, Ease.Linear, useUnscaledTime: true).OnComplete(ShowPausePanel);
            GameManager.instance.grid.DisableCells();
            SoundManager.instance.PlaySound(SoundManager.sounds.buttonTap, .5f);
        }

        public void ResumeGame()
        {
            if (_isReturningMenu) return;
            _fadeImage.raycastTarget = false;
            DisablePausePanel();
            Time.timeScale = 1;
            Tween.Alpha(_fadeImage, 0.5f, 0f, 0.5f, Ease.Linear);
            GameManager.instance.grid.EnableCells();
            SoundManager.instance.PlaySound(SoundManager.sounds.buttonTap, .5f);
        }

        private void ShowPausePanel()
        {
            _pausePanel.transform.localScale = Vector3.one;
            _pausePanel.SetActive(true);
            Vector3 shakeStrength = new Vector3(0.1f, 0.1f, 0.1f);
            Tween.ShakeScale(_pausePanel.transform, shakeStrength, 0.5f, 3, easeBetweenShakes: Ease.InOutCirc, useUnscaledTime: true);
        }

        private void DisablePausePanel()
        {
            void SetActiveFalse() => _pausePanel.SetActive(false); //local function
            Tween.Scale(_pausePanel.transform, Vector3.zero, .25f, Ease.InOutBounce).OnComplete(SetActiveFalse);
        }

        public void ReturnMenu()
        {
            if (_isReturningMenu) return;
            SoundManager.instance.PlaySound(SoundManager.sounds.buttonTap, .5f);
            _isReturningMenu = true;
            ReturnMainMenuAsync().Forget();
        }

        private async UniTaskVoid ReturnMainMenuAsync()
        {
            GlobalToken.Cancel();
            _fadeImage.raycastTarget = true;
            Time.timeScale = 1;

            Tween.StopAll(_fadeImage);
            Tween.StopAll(_pausePanel.transform);

            DisablePausePanel();

            await Tween.Alpha(_fadeImage, _fadeImage.color.a, 1f, 1f, Ease.Linear);
            Tween.StopAll();
            await UniTask.DelayFrame(3);

            SceneManager.LoadScene(GameData.SCENE_MENU);
        }

        public Tween BackgroundColorFade(Color startColor, float duration = .5f)
        {
            return Tween.Color(_backgroundImage, startColor, Color.white, duration, Ease.InOutSine);
        }

        public Tween SerializeTypeForBackgroundFade(BlastableType type, float duration = .5f)
        {
            Color color = type switch
            {
                BlastableType.ColorRed => CachedColors.softRed,
                BlastableType.ColorGreen => CachedColors.softGreen,
                BlastableType.ColorBlue => CachedColors.softCyan,
                BlastableType.ColorYellow => CachedColors.softYellow,
                BlastableType.ColorPurple => CachedColors.softPurple,
                BlastableType.ColorPink => CachedColors.softPink,
                _ => Color.black
            };

            return BackgroundColorFade(color, duration);
        }

        public Sequence BackgroundColorHueShift(float eachDuration = .05f)
        {
            Sequence sequence = Sequence.Create()
            .Chain(Tween.Color(_backgroundImage, CachedColors.softRed, CachedColors.softGreen, eachDuration, Ease.Linear))
            .Chain(Tween.Color(_backgroundImage, CachedColors.softGreen, CachedColors.softBlue, eachDuration, Ease.Linear))
            .Chain(Tween.Color(_backgroundImage, CachedColors.softBlue, CachedColors.softYellow, eachDuration, Ease.Linear))
            .Chain(Tween.Color(_backgroundImage, CachedColors.softCyan, CachedColors.softPink, eachDuration, Ease.Linear))
            .Chain(Tween.Color(_backgroundImage, CachedColors.softPink, Color.white, eachDuration, Ease.Linear));
            return sequence;
        }

        #endregion

        #region  Tweens
        private Tween BackgroundLoop() => Tween.Custom(0f, 1f, 8f, (value) => _backgroundImage.uvRect = new Rect(0, value, 1, 1), Ease.Linear, -1, CycleMode.Rewind);
        #endregion

    }
}