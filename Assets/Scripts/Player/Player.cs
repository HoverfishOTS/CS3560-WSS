using System;
using UnityEngine;

public class Player
{
    public int food { get; private set; }
    public int water { get; private set; }
    public int energy { get; private set; }
    public int gold { get; private set; }

    public int maxFood { get; private set; }
    public int maxWater { get; private set; }
    public int maxEnergy { get; private set; }

    public VisionType visionType { get; private set; }
    private Vision vision;

    public Map map;
    public MapPosition mapPosition { get; private set; }

    public Player(Map map)
    {
        this.map = map;
    }

    public void InitializePlayer(int maxFood, int maxWater, int maxEnergy, VisionType visionType)
    {
        this.maxFood = maxFood;
        this.maxWater = maxWater;
        this.maxEnergy = maxEnergy;

        gold = 5;

        food = maxFood;
        water = maxWater;
        energy = maxEnergy;

        this.visionType = visionType;
    }

    public void SetMapPosition(int x, int y)
    {
        mapPosition = new MapPosition(x, y);
        PlayerRenderer.Instance?.UpdatePosition(x, y);
    }

    public void SetMapPosition(MapPosition position)
    {
        mapPosition = position;
    }

    public void Move(string direction)
    {
        Debug.Log($"[Player] Moving {direction}");

        // disable selectable on surrounding tiles
        MapTerrain[] surroundingTiles = new MapTerrain[9] { 
            map.GetTile(mapPosition.x + 1, mapPosition.y),   // east 
            map.GetTile(mapPosition.x, mapPosition.y + 1),   // north
            map.GetTile(mapPosition.x, mapPosition.y - 1),   // south
            map.GetTile(mapPosition.x - 1, mapPosition.y),   // west
            map.GetTile(mapPosition.x + 1, mapPosition.y + 1),   // northeast
            map.GetTile(mapPosition.x + 1, mapPosition.y - 1),   // southeast
            map.GetTile(mapPosition.x - 1, mapPosition.y - 1),   // southwest
            map.GetTile(mapPosition.x - 1, mapPosition.y + 1),   // northwest
            map.GetTile(mapPosition.x, mapPosition.y)        // current
        };
        for(int i = 0; i < surroundingTiles.Length; i++)
        {
            if( surroundingTiles[i] == null ) continue;
            surroundingTiles[i].tile.SetSelectionFrameActive(false);
        }

        // move
        switch(direction)
        {
            case "EAST":
                SetMapPosition(mapPosition.x + 1, mapPosition.y); break;
            case "NORTH":
                SetMapPosition(mapPosition.x, mapPosition.y + 1); break;
            case "SOUTH":
                SetMapPosition(mapPosition.x, mapPosition.y - 1); break;
            case "WEST":
                SetMapPosition(mapPosition.x - 1, mapPosition.y); break;
            case "NORTHEAST":
                SetMapPosition(mapPosition.x + 1, mapPosition.y + 1); break;
            case "SOUTHEAST":
                SetMapPosition(mapPosition.x + 1, mapPosition.y - 1); break;
            case "SOUTHWEST":
                SetMapPosition(mapPosition.x - 1, mapPosition.y - 1); break;
            case "NORTHWEST":
                SetMapPosition(mapPosition.x - 1, mapPosition.y + 1); break;
        }

        MapTerrain newTerrain = GetCurrentMapTerrain();
        if (newTerrain != null)
        {
            // apply costs
            if (newTerrain.movementCost > 0) ApplyCost("energy", newTerrain.movementCost);
            if (newTerrain.foodCost > 0) ApplyCost("food", newTerrain.foodCost);
            if (newTerrain.waterCost > 0) ApplyCost("water", newTerrain.waterCost);

            // apply bonuses
            if (newTerrain.hasFoodBonus) ApplyBonus("food", newTerrain.foodBonus, newTerrain);
            if (newTerrain.hasWaterBonus) ApplyBonus("water", newTerrain.waterBonus, newTerrain);
            if (newTerrain.hasGoldBonus) ApplyBonus("gold", newTerrain.goldBonus, newTerrain);
        }
    }

    public void Rest()
    {
        Debug.Log("[Player] Resting this turn");
        
        MapTerrain newTerrain = GetCurrentMapTerrain();
        if (newTerrain != null)
        {
            // apply half costs
            if (newTerrain.foodCost > 0) ApplyCost("food", Mathf.RoundToInt(Mathf.Ceil(newTerrain.foodCost / 2f)));
            if (newTerrain.waterCost > 0) ApplyCost("water", Mathf.RoundToInt(Mathf.Ceil(newTerrain.waterCost / 2f)));

            // apply bonuses
            if (newTerrain.hasFoodBonus) ApplyBonus("food", newTerrain.foodBonus, newTerrain);
            if (newTerrain.hasWaterBonus) ApplyBonus("water", newTerrain.waterBonus, newTerrain);
            
            // regain energy
            ApplyBonus("energy", 2, newTerrain); 
        }
    }

    public void AttemptTrade()
    {
        Debug.Log("[Player] Received TRADE action.");

        // First, check if there's a trader on the current tile
        MapTerrain currentTerrain = GetCurrentMapTerrain();
        if (currentTerrain != null && currentTerrain.hasTrader)
        {
            Debug.Log("[Player] Trader present. Initializing Trade...");

            TradeManager.Instance.StartTrade(currentTerrain.trader);
        }
        else
        {
            // No trader on the current tile
            Debug.LogWarning("[Player] Trade failed! No trader present on the current tile.");
            // Optionally, you could penalize the player for attempting to trade with thin air,
            // but for now, just logging a warning is fine.
        }
    }
    public MapTerrain GetCurrentMapTerrain()
    {
        return map.GetTile(mapPosition.x, mapPosition.y);
    }

    private void ApplyCost(string type, int cost)
    {
        switch (type.ToLower())
        {
            case "energy":
                energy -= cost; break;
            case "food":
                food -= cost; break;
            case "water":
                water -= cost; break;
            case "gold": // Added gold cost handling
                gold -= cost; break;
        }
        // Clamp values to ensure they don't go below 0 (optional, depends on game rules)
        // food = Mathf.Max(0, food);
        // water = Mathf.Max(0, water);
        // energy = Mathf.Max(0, energy);
        // gold = Mathf.Max(0, gold);
    }

    private void ApplyBonus(string type, int bonus, MapTerrain terrain)
    {
        switch (type.ToLower())
        {
            case "gold":
                gold += bonus; terrain.TakeBonus(type); break;
            case "food":
                food = Mathf.Min(maxFood, food + bonus); terrain.TakeBonus(type);  break;
            case "water":
                water = Mathf.Min(maxWater, water + bonus); terrain.TakeBonus(type); break;
            case "energy": // for resting
                energy = Mathf.Min(maxEnergy, energy + bonus); break;
        }
    }

    public void ApplyTrade(TradeOffer offer)
    {
        gold = Mathf.Max(0, gold - offer.goldToTrader);
        food = Mathf.Min(maxFood, food + offer.foodToPlayer);
        water = Mathf.Min(maxWater, water + offer.waterToPlayer);
    }
}
