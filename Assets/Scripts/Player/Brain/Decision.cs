public enum DecisionType
{
    Move,
    Rest,
    Trade,
    Invalid
}

public class Decision
{
    public DecisionType decisionType;
    public string direction; // only used if decisionType == Move
}
