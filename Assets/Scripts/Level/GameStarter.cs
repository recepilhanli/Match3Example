using UnityEngine;


namespace Game.Level
{
    /// <summary>
    /// GameStarter is a simple script that starts the game by calling GameManager.StartGame
    /// </summary>
    public class GameStarter : MonoBehaviour
    {
        [SerializeField] private Vector2Int _gridDimensions = new Vector2Int(8, 10);
        [SerializeField] private float _gridSpacing = 100f;

        private void Start()
        {
            GameManager.instance.StartGame(_gridDimensions, _gridSpacing);
            Destroy(this); //no longer needed
        }
    }
}