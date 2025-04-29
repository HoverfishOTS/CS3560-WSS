public class CautiousVision : Vision
{
    public CautiousVision(Player player, Map map) : base(player, map) {}

    protected override void BuildVisionMask()
    {
        visionMask = new bool[5, 3];

        visionMask[2, 0] = true; // Player's own tile (2,0)
        visionMask[1, 0] = true; // North (one up in same column)
        visionMask[2, 1] = true; // East (one right in same row)
        visionMask[3, 0] = true; // South (one down in same column)
    }
}
