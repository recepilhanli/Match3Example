using Cysharp.Threading.Tasks;
using Game.Level;
using Game.Utils;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    // GameManager.Signals.cs
    public partial class GameManager
    {
        public UnityAction<GridCell> onPlayerTouchCell;
        public UnityAction onGridRemixed;
        public UnityAction onGameStarted;
    }
}