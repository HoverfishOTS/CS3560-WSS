using UnityEngine;

public abstract class Vision
{
    public MapTerrain[][] fieldOfVision;

    public abstract MapTerrain ClosestFood(MapTerrain[][] vision);

    public abstract MapTerrain ClosestWater(MapTerrain[][] vision);

    public abstract MapTerrain ClosestGold(MapTerrain[][] vision);

    public abstract MapTerrain ClosestTrader(MapTerrain[][] vision);

    public abstract MapTerrain[] EasiestPath(MapTerrain[][] vision);

    public abstract MapTerrain SecondClosestFood(MapTerrain[][] vision);

    public abstract MapTerrain SecondClosestWater(MapTerrain[][] vision);

    public abstract MapTerrain SecondClosestGold(MapTerrain[][] vision);

    public abstract MapTerrain SecondClosestTrader(MapTerrain[][] vision);
}
