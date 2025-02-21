using Game.Utils;
using PrimeTween;
using UnityEngine;

namespace Game
{

    public partial class GameManager : MonoSingleton<GameManager>
    {

        protected override void Awake()
        {
            CheckInstance();
            PrimeTweenConfig.warnEndValueEqualsCurrent = false;
            GlobalToken.Refresh();
        }

        public void StartGame(Vector2Int gridDimensions, float gridSpacing)   
        {
            CreateGrid(gridDimensions.x, gridDimensions.y, gridSpacing);
            InitInactivityHighlighting();
            onGameStarted?.Invoke();
        }
        
    }
}