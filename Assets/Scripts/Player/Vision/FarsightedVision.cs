public class FarsightedVision : Vision
{
    public FarsightedVision(Player player, Map map) : base(player, map) {}

    protected override void BuildVisionMask()
    {
        visionMask = new bool[5, 3];

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                visionMask[y, x] = true;
            }
        }

        // Hide furthest top corners
        visionMask[0, 2] = false; // Top-left corner
        visionMask[4, 2] = false; // Top-right corner
    }
}
