using UnityEditor;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
    public static GameConfig instance;

    [Header("Map Config")]
    [SerializeField] private const Difficulty DEFAULT_DIFFICULTY = Difficulty.Medium;
    [SerializeField] private Vector2 WIDTH_CONSTRAINTS = new Vector2(5, 20);
    [SerializeField] private const int DEFAULT_WIDTH = 10;
    [SerializeField] private Vector2 HEIGHT_CONSTRAINTS = new Vector2(3, 10);
    [SerializeField] private const int DEFAULT_HEIGHT = 5;

    [Header("Player Config")]
    [SerializeField] private const VisionType DEFAULT_VISION = VisionType.Focused;
    [SerializeField] private const BrainType DEFAULT_BRAIN = BrainType.CPU;
    [SerializeField] private const int DEFAULT_MAX_FOOD = 5;
    [SerializeField] private Vector2 MAX_FOOD_CONSTRAINTS = new Vector2(3, 10);
    [SerializeField] private const int DEFAULT_MAX_WATER = 5;
    [SerializeField] private Vector2 MAX_WATER_CONSTRAINTS = new Vector2(3, 10);
    [SerializeField] private const int DEFAULT_MAX_ENERGY = 5;
    [SerializeField] private Vector2 MAX_ENERGY_CONSTRAINTS = new Vector2(3, 10);


    public MapConfig mapConfig { get; private set; }
    public PlayerConfig playerConfig { get; private set; }

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(instance.gameObject);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        mapConfig = new MapConfig(DEFAULT_DIFFICULTY, DEFAULT_WIDTH, DEFAULT_HEIGHT, WIDTH_CONSTRAINTS, HEIGHT_CONSTRAINTS);
        playerConfig = new PlayerConfig(DEFAULT_VISION, DEFAULT_BRAIN, DEFAULT_MAX_FOOD, DEFAULT_MAX_WATER, DEFAULT_MAX_ENERGY, MAX_FOOD_CONSTRAINTS, MAX_WATER_CONSTRAINTS, MAX_ENERGY_CONSTRAINTS);
    }
}

public class MapConfig
{
    public Difficulty difficulty { get; private set; }
    public int width { get; private set; }
    public int height { get; private set; }

    private Vector2 widthMinMax;
    private Vector2 heightMinMax;

    public MapConfig(Difficulty difficulty, int width, int height, Vector2 widthMinMax, Vector2 heightMinMax)
    {
        this.difficulty = difficulty;
        this.width = width;
        this.height = height;
        this.widthMinMax = widthMinMax;
        this.heightMinMax = heightMinMax;
    }

    public string IncrementDifficulty()
    {
        int count = System.Enum.GetValues(typeof(Difficulty)).Length;
        difficulty = (Difficulty)(((int)difficulty + 1) % count);
        return difficulty.ToString();
    }

    public string DecrementDifficulty()
    {
        int count = System.Enum.GetValues(typeof(Difficulty)).Length;
        difficulty = (Difficulty)(((int)difficulty - 1 + count) % count);
        return difficulty.ToString();
    }

    public string IncrementWidth()
    {
        width++;
        if (width > widthMinMax.y)
            width = (int)widthMinMax.x;

        return width.ToString();
    }

    public string DecrementWidth()
    {
        width--;
        if (width < widthMinMax.x)
            width = (int)widthMinMax.y;

        return width.ToString();
    }

    public string IncrementHeight()
    {
        height++;
        if (height > heightMinMax.y)
            height = (int)heightMinMax.x;

        return height.ToString();
    }

    public string DecrementHeight()
    {
        height--;
        if (height < heightMinMax.x)
            height = (int)heightMinMax.y;

        return height.ToString();
    }
}

public class PlayerConfig
{
    public VisionType visionType { get; private set; }
    public BrainType brainType { get; private set; }

    public int maxFood { get; private set; }
    public int maxWater { get; private set; }
    public int maxEnergy { get; private set; }

    private Vector2 maxFoodMinMax, maxWaterMinMax, maxEnergyMinMax;

    public PlayerConfig(VisionType visionType, BrainType brainType, int maxFood, int maxWater, int maxEnergy, Vector2 maxFoodMinMax, Vector2 maxWaterMinMax, Vector2 maxEnergyMinMax)
    {
        this.visionType = visionType;
        this.brainType = brainType;
        this.maxFood = maxFood;
        this.maxWater = maxWater;
        this.maxEnergy = maxEnergy;
        this.maxFoodMinMax = maxFoodMinMax;
        this.maxWaterMinMax = maxWaterMinMax;
        this.maxEnergyMinMax = maxEnergyMinMax;
    }

    public string IncrementVisionType()
    {
        int count = System.Enum.GetValues(typeof(VisionType)).Length;
        visionType = (VisionType)(((int)visionType + 1) % count);

        return visionType.ToString();
    }

    public string DecrementVisionType()
    {
        int count = System.Enum.GetValues(typeof(VisionType)).Length;
        visionType = (VisionType)(((int)visionType - 1 + count) % count);

        return visionType.ToString();
    }

    public string IncrementBrainType()
    {
        int count = System.Enum.GetValues(typeof(BrainType)).Length;
        brainType = (BrainType)(((int)brainType + 1) % count);

        return brainType.ToString();
    }

    public string DecrementBrainType()
    {
        int count = System.Enum.GetValues(typeof(BrainType)).Length;
        brainType = (BrainType)(((int)brainType - 1 + count) % count);

        return brainType.ToString();
    }

    public string IncrementMaxFood()
    {
        maxFood++;
        if (maxFood > maxFoodMinMax.y)
            maxFood = (int)maxFoodMinMax.x;

        return maxFood.ToString();
    }

    public string DecrementMaxFood()
    {
        maxFood--;
        if (maxFood < maxFoodMinMax.x)
            maxFood = (int)maxFoodMinMax.y;

        return maxFood.ToString();
    }

    public string IncrementMaxWater()
    {
        maxWater++;
        if (maxWater > maxWaterMinMax.y)
            maxWater = (int)maxWaterMinMax.x;

        return maxWater.ToString();
    }

    public string DecrementMaxWater()
    {
        maxWater--;
        if (maxWater < maxWaterMinMax.x)
            maxWater = (int)maxWaterMinMax.y;

        return maxWater.ToString();
    }

    public string IncrementMaxEnergy()
    {
        maxEnergy++;
        if (maxEnergy > maxEnergyMinMax.y)
            maxEnergy = (int)maxEnergyMinMax.x;

        return maxEnergy.ToString();
    }

    public string DecrementMaxEnergy()
    {
        maxEnergy--;
        if (maxEnergy < maxEnergyMinMax.x)
            maxEnergy = (int)maxEnergyMinMax.y;

        return maxEnergy.ToString();
    }
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

public enum VisionType
{
    Focused,
    Cautious,
    Keeneyed,
    Farsighted
}

public enum BrainType
{
    CPU,
    Player
}
