
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

    public bool Equals(MapPosition other)
    {
        return x == other.x && y == other.y;
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ")";
    }
}
