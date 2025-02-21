using Cysharp.Threading.Tasks;
using Game.Utils;
using PrimeTween;
using UnityEngine;

namespace Game
{
    // GameManager.PlayerHelper.cs
    public partial class GameManager
    {
        private const float HELPER_HIGHLIGHT_DELAY = 5; //in seconds
        private const float HELPER_INTERACTION_DELAY = 6f; //in seconds
        private float _blastedTime = 0;

        private async UniTaskVoid HighlightGroups()
        {
            while (true)
            {
                await UniTask.WaitForSeconds(HELPER_HIGHLIGHT_DELAY, cancellationToken: GlobalToken.token);
                if (GlobalToken.token == null || GlobalToken.token.IsCancellationRequested) return;
                else if (_blastedTime > Time.timeSinceLevelLoad || Time.timeScale == 0) continue;

                MarkBlastableGroups(Random.Range(1, 3));
            }
        }

        private void InitInactivityHighlighting()
        {
            onPlayerTouchCell += (cell) => _blastedTime = Time.timeSinceLevelLoad + HELPER_INTERACTION_DELAY;
            HighlightGroups().Forget();
        }

    }
}