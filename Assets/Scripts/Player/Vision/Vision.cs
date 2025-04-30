using UnityEngine;

public abstract class Vision
{
    protected Player player;
    protected Map map;
    protected bool[,] visionMask = new bool[5, 3]; // y,x format
    protected MapTerrain[][] fieldOfVision = new MapTerrain[5][];

    public Vision(Player player, Map map)
    {
        this.player = player;
        this.map = map;

        for (int y = 0; y < 5; y++)
            fieldOfVision[y] = new MapTerrain[3];

        BuildVisionMask();
        GenerateField();
    }

    protected abstract void BuildVisionMask();

    protected void GenerateField()
    {
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                int worldX = player.mapPosition.x + x;
                int worldY = player.mapPosition.y - (2 - y);

                if (visionMask[y, x])
                {
                    fieldOfVision[y][x] = map.GetTile(worldX, worldY);
                }
                else
                {
                    fieldOfVision[y][x] = null;
                }
            }
        }

        // Discover all visible tiles
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                if (fieldOfVision[y][x]?.tile != null)
                {
                    fieldOfVision[y][x].tile.DiscoverTile();
                }
            }
        }
    }
    
    public MapTerrain[][] GetField()
    {
        return fieldOfVision;
    }
}
