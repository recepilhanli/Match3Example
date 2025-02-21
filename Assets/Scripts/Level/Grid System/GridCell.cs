using Cysharp.Threading.Tasks;
using Game.ScriptableObjects;
using Game.Utils;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Level
{
    using Debug = Utils.Logger.Debug;

    [System.Serializable]
    public class GridCell : MonoBehaviour, IPointerClickHandler
    {

        private static Grid _grid => GameManager.instance.grid;

        [Header("Cell Properties")]
        public ABlastableContainer item;
        public Image image;
        public Vector2Int gridPosition;
        public GridCell groupRoot = null;

        public RectTransform rectTransform => image.rectTransform;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (item != null && !_grid.isBusy && image.color.a > .75f)
            {
                item.Blast(this);
                GameManager.instance.onPlayerTouchCell?.Invoke(this);
            }
        }

        public void SetItem(ABlastableContainer item, bool animate = false, bool assignSprite = true)
        {
            if (this.item != null && GameManager.IsExplosive(this.item.type)) _grid.totalExplosives--;

            this.item = item;
            if (assignSprite) image.sprite = item.defaultSprite;
            if (animate) Tween.PunchScale(rectTransform, new Vector3(Random.Range(0f, .6f), Random.Range(0f, .6f), 0f), Random.Range(0f, .3f), easeBetweenShakes: Ease.InOutSine, startDelay: Random.Range(0f, .1f));
        }

        public void UpdateItemSprite() => image.sprite = item.defaultSprite;

        public async UniTask UpdateCellSpriteDelayed(float delay = .15f)
        {
            await UniTask.WaitForSeconds(delay, cancellationToken: GlobalToken.token);
            UpdateItemSprite();
        }

        public void Init(Grid grid, in Vector2 anchoredPosition, in Vector2Int virtualPosition)
        {
            rectTransform.anchoredPosition = anchoredPosition;
            gridPosition = virtualPosition;
#if UNITY_EDITOR
            name = $"Cell {virtualPosition.x}, {virtualPosition.y}";
#endif
            grid.AddCell(this, virtualPosition.x, virtualPosition.y);
        }

    }
}