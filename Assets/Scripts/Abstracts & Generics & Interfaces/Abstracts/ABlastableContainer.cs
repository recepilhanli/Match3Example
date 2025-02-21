using Game;
using Game.Level;
using UnityEngine;
using UnityEngine.UI;
using Grid = Game.Level.Grid;

public abstract class ABlastableContainer : ScriptableObject
{
    protected static Grid _grid => GameManager.instance.grid;

    [SerializeField] private BlastableType _type;
    public BlastableType type => _type;

    [SerializeField] private Sprite _defaultSprite;
    public Sprite defaultSprite => _defaultSprite;

    public abstract void Blast(GridCell cell);
}


public enum BlastableType
{
    Obstacle,
    SodaMachine,

    //Colored Blastables
    ColorRed,
    ColorBlue,
    ColorGreen,
    ColorYellow,
    ColorPurple,
    ColorPink,

    Bomb,
    Firework,
}

