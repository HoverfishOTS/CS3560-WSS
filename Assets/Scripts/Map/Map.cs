using UnityEngine;

public class Map
{
    public int height { get; private set; }
    public int width { get; private set; }
    public int difficulty { get; private set; }
        
    private MapTerrain[][] mapMatrix;

    public Map(int width, int height, int difficulty, MapTerrain[][] mapMatrix)
    {
        this.width = width;
        this.height = height;
        this.difficulty = difficulty;
        this.mapMatrix = mapMatrix;
    }

    public MapTerrain GetTile(int x, int y)
    {
        if(y < mapMatrix.Length && x < mapMatrix[y].Length) return mapMatrix[y][x];
        Debug.LogError("Tried to get tile outside the map's dimensions. Map Dimensions: " + mapMatrix.Length + "x" + mapMatrix[y].Length + "\nTarget: " + y + "x" + x);
        return null;
    }
}
