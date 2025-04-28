using UnityEngine;
using System;

public class CurrentGameState
{
    private Vision vision;
    private int currentFood;
    private int currentWater;
    private int currentEnergy;
    private int currentGold;
    private Tuple<int, int> currentMapPosition;

    public CurrentGameState(Vision vision, int currentFood, int currentWater, int currentEnergy, int currentGold, Tuple<int,int> currentMapPosition)
    {
        this.vision = vision;
        this.currentFood = currentFood;
        this.currentWater = currentWater;
        this.currentEnergy = currentEnergy;
        this.currentGold = currentGold;
        this.currentMapPosition = currentMapPosition;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(new SerializableCurrentGameState(this));
    }

    [System.Serializable]
    private class SerializableCurrentGameState
    {
        public string visionType;
        public int currentFood;
        public int currentWater;
        public int currentEnergy;
        public int currentGold;
        public MapPosition currentMapPosition;

        public SerializableCurrentGameState(CurrentGameState state)
        {
            this.visionType = state.vision?.GetType().Name ?? "None";
            this.currentFood = state.currentFood;
            this.currentWater = state.currentWater;
            this.currentEnergy = state.currentEnergy;
            this.currentGold = state.currentGold;
            this.currentMapPosition = new MapPosition(state.currentMapPosition.Item1, state.currentMapPosition.Item2);
        }
    }

    [System.Serializable]
    public struct MapPosition
    {
        public int x;
        public int y;

        public MapPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

}
