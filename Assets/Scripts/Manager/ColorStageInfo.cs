using UnityEngine;

namespace Game
{
    [System.Serializable]
    public class StageInfo
    {
        public const int STAGE_DEFAULT = 0;
        public const int STAGE_A = 1; //Firework
        public const int STAGE_B = 2; //Bomb
        public const int STAGE_C = 3; //Disco Ball?

        public int[] stageThresholds = new int[3] { 3, 6, 9 }; // A, B, C

        public Sprite[] redStages = new Sprite[4];
        public Sprite[] greenStages = new Sprite[4];
        public Sprite[] blueStages = new Sprite[4];
        public Sprite[] yellowStages = new Sprite[4];
        public Sprite[] pinkStages = new Sprite[4];
        public Sprite[] purpleStages = new Sprite[4];

        public Sprite[] FindColorArray(BlastableType type)
        {
            switch (type)
            {
                case BlastableType.ColorRed:
                    return redStages;
                case BlastableType.ColorGreen:
                    return greenStages;
                case BlastableType.ColorBlue:
                    return blueStages;
                case BlastableType.ColorPink:
                    return pinkStages;
                case BlastableType.ColorPurple:
                    return purpleStages;
                case BlastableType.ColorYellow:
                    return yellowStages;
                default:
                    Debug.LogError("Color not found for type: " + type);
                    return null;
            }
        }

        public Sprite FindColorStage(BlastableType type, int stage)
        {
            return FindColorArray(type)[stage];
        }

        public int FindCurrentStageOfGroup(int memberCount)
        {
            if (memberCount >= stageThresholds[2]) return STAGE_C;
            else if (memberCount >= stageThresholds[1]) return STAGE_B;
            else if (memberCount >= stageThresholds[0]) return STAGE_A;
            return STAGE_DEFAULT;
        }

    }
}