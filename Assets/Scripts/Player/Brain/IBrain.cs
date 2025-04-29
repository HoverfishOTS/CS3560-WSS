using System.Threading.Tasks;

public interface IBrain
{
    Task<Decision> GetDecisionAsync();
}
