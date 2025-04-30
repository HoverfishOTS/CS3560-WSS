public class FocusedVision : Vision
{
    public FocusedVision(Player player, Map map) : base(player, map) {}

    protected override void BuildVisionMask()
    {
        visionMask = new bool[5, 3];

        visionMask[2, 0] = true; // Player's tile
        visionMask[2, 1] = true; // East (directly right)
        visionMask[1, 1] = true; // Northeast (one up and right)
        visionMask[3, 1] = true; // Southeast (one down and right)
    }
}
