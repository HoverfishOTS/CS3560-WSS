public class KeeneyedVision : Vision
{
    public KeeneyedVision(Player player, Map map) : base(player, map) {}

    protected override void BuildVisionMask()
    {
        visionMask = new bool[5, 3];

        visionMask[2, 0] = true; // Player's tile
        visionMask[2, 1] = true; // East
        visionMask[1, 1] = true; // Northeast
        visionMask[3, 1] = true; // Southeast
        visionMask[2, 2] = true; // East + 1 (further east)
        visionMask[1, 0] = true; // North
        visionMask[3, 0] = true; // South
    }
}
