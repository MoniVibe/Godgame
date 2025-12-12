# Markets & Prices – Chunk 5 Implementation Guide

**Date:** 2025-01-27  
**Status:** Implementation Plan  
**Purpose:** Step-by-step guide for implementing Chunk 5 – Markets & Prices  
**Reference:** Design specification in [`Markets_And_Prices_Chunk5.md`](Markets_And_Prices_Chunk5.md)

---

## Prerequisites Check

Before starting implementation, verify:

- ✅ **Chunk 0** (Economy Spine & Principles) - Design doc complete
- ✅ **Chunk 1** (Wealth & Ledgers) - Design doc complete, wallet components should exist
- ✅ **Chunk 2** (Resources & Mass) - Design doc complete, inventory components should exist  
- ✅ **Chunk 3** (Businesses & Production) - Design doc complete
- ✅ **Chunk 4** (Trade & Logistics) - Design doc complete
- ✅ **Chunk 5** (Markets & Prices) - Design spec complete

**Dependencies to verify:**
- Trade nodes (villages/colonies) exist and are identifiable
- Inventory components exist and can be queried
- Wealth/wallet components exist and support transactions
- BasePrice catalog can be created as BlobAsset

---

## Implementation Steps

### Step 1: Wire Up Per-Market Data

**Goal:** Every village/colony has a small market state blob you can read/write.

#### Task 1.1: Create MarketPrice Buffer Component

**File:** `Assets/Scripts/Godgame/Economy/MarketComponents.cs` (new file)

**Component Structure:**
```csharp
[InternalBufferCapacity(16)]
public struct MarketPrice : IBufferElementData
{
    public FixedString64Bytes GoodType;
    public float BasePrice;
    public float CurrentPrice;
    public float Supply;
    public float MonthlyDemand;
    
    // Debug fields (visible in inspector/debug views)
    public float SupplyDemandRatio;
    public float SupplyDemandMultiplier;
    public float VillageWealthMultiplier;
    public float EventMultiplier;
    
    public uint LastUpdateTick;
}
```

**Add to trade nodes:**
- Same nodes Chunk 4 uses (villages, colonies, trade hubs)
- Can be added via authoring or bootstrap system

#### Task 1.2: Create BasePrice Catalog

**File:** `Assets/Scripts/Godgame/Economy/BasePriceCatalog.cs` (new file)

**BlobAsset Structure:**
```csharp
public struct BasePriceEntry
{
    public FixedString64Bytes GoodType;
    public float BasePrice;  // Currency per kg or per unit
}

public struct BasePriceCatalogBlob
{
    public BlobArray<BasePriceEntry> Entries;
    
    public bool TryFind(FixedString64Bytes goodType, out float basePrice)
    {
        // Linear search or use hash map if many entries
        for (int i = 0; i < Entries.Length; i++)
        {
            if (Entries[i].GoodType.Equals(goodType))
            {
                basePrice = Entries[i].BasePrice;
                return true;
            }
        }
        basePrice = 0f;
        return false;
    }
}
```

**ScriptableObject Authoring:**
```csharp
[CreateAssetMenu(menuName = "Godgame/Economy/BasePriceCatalog")]
public class BasePriceCatalogAuthoring : ScriptableObject
{
    [System.Serializable]
    public class BasePriceEntryData
    {
        public string GoodType;
        public float BasePrice;
    }
    
    public List<BasePriceEntryData> Entries = new List<BasePriceEntryData>();
}
```

**Baker:**
```csharp
public class BasePriceCatalogBaker : Baker<BasePriceCatalogAuthoring>
{
    public override void Bake(BasePriceCatalogAuthoring authoring)
    {
        // Build BlobAsset from ScriptableObject
        // Store as singleton component
    }
}
```

**Initial GoodTypes:**
- Grain
- Flour
- IronOre
- IronIngot
- Tools
- Weapons

#### Task 1.3: Initialize MarketPrice Buffers

**File:** `Assets/Scripts/Godgame/Economy/MarketBootstrapSystem.cs` (new file)

**System:**
- Query all trade nodes (villages/colonies)
- For each node, ensure MarketPrice buffer exists
- For each GoodType in BasePrice catalog:
  - Add MarketPrice entry with BasePrice from catalog
  - Set CurrentPrice = BasePrice
  - Set Supply = 0, MonthlyDemand = 0
  - Set multipliers = 1.0

**Deliverable:** Trade nodes have MarketPrice buffers with initial values from BasePrice catalog.

---

### Step 2: Supply & Demand Metrics

**Goal:** Calculate Supply and MonthlyDemand for each GoodType per node.

#### Task 2.1: Create GoodType Mapping System

**File:** `Assets/Scripts/Godgame/Economy/GoodTypeMapping.cs` (new file)

**BlobAsset Structure:**
```csharp
public struct ItemSpecToGoodTypeMapping
{
    public FixedString64Bytes ItemSpecId;
    public FixedString64Bytes GoodType;
}

public struct GoodTypeMappingBlob
{
    public BlobArray<ItemSpecToGoodTypeMapping> Mappings;
    
    public bool TryGetGoodType(FixedString64Bytes itemSpecId, out FixedString64Bytes goodType)
    {
        // Find mapping for ItemSpec
        // Return GoodType
    }
}
```

**ScriptableObject Authoring:**
- Define mappings: ItemSpec ID → GoodType
- Example: "grain" → "Grain", "iron_ingot" → "IronIngot"

#### Task 2.2: Implement Supply Calculation System

**File:** `Assets/Scripts/Godgame/Economy/MarketSupplyCalculationSystem.cs` (new file)

**System Logic:**
```
For each trade node:
  For each GoodType in MarketPrice buffer:
    Supply = 0
    
    Query all inventories in node:
      - Trade stockpiles
      - Business sale inventories  
      - Guild shops
    
    For each inventory:
      For each item in inventory:
        If ItemSpec maps to this GoodType:
          If item not reserved:
            Supply += item.Quantity
    
    Update MarketPrice.Supply
```

**Integration:**
- Use Chunk 2 inventory APIs
- Query inventories by entity relationships (village → stockpiles → inventories)
- Exclude reserved items (if reservation system exists)

#### Task 2.3: Implement Demand Calculation System

**File:** `Assets/Scripts/Godgame/Economy/MarketDemandCalculationSystem.cs` (new file)

**System Logic:**
```
For each trade node:
  Get VillageStats (population, workers, guards, etc.)
  Get MarketPricingConfig (demand formulas)
  
  For each GoodType in MarketPrice buffer:
    MonthlyDemand = 0
    
    // Food demand
    If GoodType == "Grain" or "Flour":
      MonthlyDemand = Population × FoodPerCapita (from config)
    
    // Tool/weapon demand
    If GoodType == "Tools" or "Weapons":
      MonthlyDemand = (Workers × ToolsPerWorker) + (Guards × WeaponsPerGuard)
      + WarModifier (if war active)
    
    // Material demand
    If GoodType == "IronIngot" or other materials:
      MonthlyDemand = ActiveProductionPlans × MaterialPerPlan (from Chunk 3)
    
    Update MarketPrice.MonthlyDemand
```

**Future Enhancement:**
- Track actual consumption over time
- Use EMA (Exponential Moving Average) to smooth historical consumption
- Blend historical consumption with projected needs

**Deliverable:** Supply and Demand metrics calculated and stored in MarketPrice buffers.

---

### Step 3: Pricing Loop

**Goal:** Implement monthly pricing updates that compute CurrentPrice from multipliers.

#### Task 3.1: Create MarketPricingConfig Catalog

**File:** `Assets/Scripts/Godgame/Economy/MarketPricingConfig.cs` (new file)

**BlobAsset Structure:**
```csharp
public struct VillageWealthBreakpoint
{
    public float WealthThreshold;
    public float Multiplier;
}

public struct EventMultiplierEntry
{
    public FixedString64Bytes EventType;  // "War", "Famine", "Festival", etc.
    public float Multiplier;
}

public struct MarketPricingConfigBlob
{
    public float SupplyDemandExponent;  // alpha, default 0.5 (sqrt)
    public float MultiplierMin;        // default 0.2
    public float MultiplierMax;        // default 5.0
    public BlobArray<VillageWealthBreakpoint> WealthBreakpoints;
    public BlobArray<EventMultiplierEntry> DefaultEventMultipliers;
}
```

**ScriptableObject Authoring:**
- Define exponent (alpha = 0.5 for sqrt)
- Define multiplier caps (0.2 to 5.0)
- Define wealth breakpoints (poor/avg/rich multipliers)
- Define event multipliers (war, famine, festival, etc.)

#### Task 3.2: Implement MarketPricingSystem

**File:** `Assets/Scripts/Godgame/Economy/MarketPricingSystem.cs` (new file)

**System Logic:**
```
[BurstCompile]
[UpdateInGroup(typeof(EconomySystemGroup))]
[UpdateAfter(typeof(MarketSupplyCalculationSystem))]
[UpdateAfter(typeof(MarketDemandCalculationSystem))]
public partial struct MarketPricingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Run monthly (or every N ticks, configurable)
        var tick = SystemAPI.GetSingleton<TickTimeState>().Tick;
        if (tick % TicksPerMonth != 0) return;
        
        var config = SystemAPI.GetSingleton<MarketPricingConfig>().Config.Value;
        
        foreach (var (marketPrices, villageStats, entity) in 
            SystemAPI.Query<DynamicBuffer<MarketPrice>, RefRO<VillageStats>>().WithEntityAccess())
        {
            // Get village wealth multiplier
            float avgWealth = CalculateAverageWealth(villageStats);
            float wealthMultiplier = GetWealthMultiplier(avgWealth, config);
            
            // Get event multiplier
            float eventMultiplier = GetEventMultiplier(villageStats, config);
            
            // Update each GoodType price
            for (int i = 0; i < marketPrices.Length; i++)
            {
                var price = marketPrices[i];
                
                // Compute supply/demand multiplier
                float ratio = price.MonthlyDemand / math.max(price.Supply, 1f);
                float supplyDemandMultiplier = math.pow(ratio, config.SupplyDemandExponent);
                supplyDemandMultiplier = math.clamp(supplyDemandMultiplier, 
                    config.MultiplierMin, config.MultiplierMax);
                
                // Compute current price
                float currentPrice = price.BasePrice 
                    × supplyDemandMultiplier 
                    × wealthMultiplier 
                    × eventMultiplier;
                
                // Clamp to sensible bounds
                currentPrice = math.max(currentPrice, price.BasePrice * 0.1f);  // Min 10% of base
                currentPrice = math.min(currentPrice, price.BasePrice * 10f);  // Max 10× base
                
                // Update buffer entry
                price.CurrentPrice = currentPrice;
                price.SupplyDemandRatio = price.Supply / math.max(price.MonthlyDemand, 1f);
                price.SupplyDemandMultiplier = supplyDemandMultiplier;
                price.VillageWealthMultiplier = wealthMultiplier;
                price.EventMultiplier = eventMultiplier;
                price.LastUpdateTick = tick;
                
                marketPrices[i] = price;
            }
        }
    }
}
```

**Integration Points:**
- Reads VillageStats (from existing village systems or Chunk 1)
- Reads event state (war, famine, etc. from existing event systems)
- Uses MarketPricingConfig singleton

**Deliverable:** Prices update monthly based on supply/demand, wealth, and events.

---

### Step 4: Simple Buy/Sell API

**Goal:** Entities can buy and sell goods at market prices.

#### Task 4.1: Create Intent Components

**File:** `Assets/Scripts/Godgame/Economy/MarketIntentComponents.cs` (new file)

**Components:**
```csharp
public struct MarketBuyIntent : IComponentData
{
    public Entity TargetMarket;      // Trade node (village/colony)
    public FixedString64Bytes GoodType;
    public float DesiredQuantity;
    public float MaxPrice;            // Optional, default = current price
    public Entity BuyerWallet;        // Chunk 1 wallet
    public Entity BuyerInventory;    // Chunk 2 inventory
}

public struct MarketSellIntent : IComponentData
{
    public Entity TargetMarket;      // Trade node (village/colony)
    public FixedString64Bytes GoodType;
    public float QuantityOffered;
    public float MinPrice;           // Optional, default = current price
    public Entity SellerInventory;   // Chunk 2 inventory
    public Entity SellerWallet;      // Chunk 1 wallet
}
```

#### Task 4.2: Implement MarketClearingSystem

**File:** `Assets/Scripts/Godgame/Economy/MarketClearingSystem.cs` (new file)

**System Logic:**
```
[BurstCompile]
[UpdateInGroup(typeof(EconomySystemGroup))]
[UpdateAfter(typeof(MarketPricingSystem))]
public partial struct MarketClearingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Group intents by market and GoodType
        // For each (market, GoodType) pair:
        
        // 1. Collect buy/sell intents
        var buyIntents = CollectBuyIntents(market, goodType);
        var sellIntents = CollectSellIntents(market, goodType);
        
        // 2. Get current price
        float currentPrice = GetCurrentPrice(market, goodType);
        
        // 3. Filter by price constraints
        buyIntents = FilterBuyIntents(buyIntents, currentPrice);
        sellIntents = FilterSellIntents(sellIntents, currentPrice);
        
        // 4. Match volume
        float totalDemand = SumQuantities(buyIntents);
        float totalSupply = SumQuantities(sellIntents);
        
        if (totalSupply >= totalDemand)
        {
            // All buyers filled, sellers partial (pro-rata)
            FillBuyers(buyIntents, totalDemand);
            FillSellersProRata(sellIntents, totalDemand);
        }
        else
        {
            // All sellers filled, buyers partial (pro-rata)
            FillSellers(sellIntents, totalSupply);
            FillBuyersProRata(buyIntents, totalSupply);
        }
        
        // 5. Execute trades
        foreach (var match in matches)
        {
            // Chunk 1: Wealth transaction
            RecordTransaction(match.BuyerWallet, match.SellerWallet, 
                match.Quantity * currentPrice, "market_purchase");
            
            // Chunk 2: Inventory move
            MoveItems(match.SellerInventory, match.BuyerInventory, 
                match.GoodType, match.Quantity);
        }
        
        // 6. Remove intent components
        RemoveIntentComponents(executedIntents);
    }
}
```

**Matching Logic:**
- Simple price-taker model (everyone trades at CurrentPrice)
- Pro-rata filling when supply/demand mismatch
- FIFO alternative (first-come-first-served) can be added later

**Deliverable:** Entities can buy/sell goods at market prices via intent components.

---

### Step 5: Hook in Chunk 4 Arrivals

**Goal:** Trade arrivals update market supply automatically.

#### Task 5.1: Create Trade Arrival Event

**File:** `Assets/Scripts/Godgame/Economy/MarketEvents.cs` (new file)

**Component:**
```csharp
[InternalBufferCapacity(8)]
public struct TradeArrivalEvent : IBufferElementData
{
    public Entity DestinationNode;
    public FixedString64Bytes GoodType;
    public float Quantity;
    public Entity SourceNode;        // Optional
    public uint Tick;
}
```

#### Task 5.2: Integrate with Chunk 4 Unload System

**Modify:** Chunk 4 unload system (or create hook)

**Integration Point:**
- When Chunk 4 unloads cargo at destination:
  1. Move items to destination inventory (Chunk 2) - existing Chunk 4 logic
  2. Emit TradeArrivalEvent with GoodType and Quantity
  3. Log: "+X of GoodType Y imported from Node A"

**Option A:** MarketSupplyCalculationSystem reads TradeArrivalEvents  
**Option B:** Rely on next pricing tick's supply calculation to see new inventories

**Recommendation:** Option B (simpler, no extra system needed). TradeArrivalEvents are optional for telemetry/debug.

**Deliverable:** Trade arrivals are logged and reflected in next supply calculation.

---

### Step 6: Minimum Debug Tooling

**Goal:** Debug view to inspect market state and tune parameters.

#### Task 6.1: Create Debug Display System

**File:** `Assets/Scripts/Godgame/Economy/MarketDebugDisplaySystem.cs` (new file)

**MonoBehaviour or EditorWindow:**
```csharp
public class MarketDebugWindow : EditorWindow
{
    private Entity selectedMarket;
    
    void OnGUI()
    {
        // Select market dropdown
        selectedMarket = SelectMarket();
        
        if (selectedMarket == Entity.Null) return;
        
        // Get MarketPrice buffer
        var marketPrices = GetMarketPrices(selectedMarket);
        
        // Display table
        GUILayout.Label("Market Prices", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        
        foreach (var price in marketPrices)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(price.GoodType.ToString());
            EditorGUILayout.LabelField($"Base: {price.BasePrice:F2}");
            EditorGUILayout.LabelField($"Current: {price.CurrentPrice:F2}");
            EditorGUILayout.LabelField($"Supply: {price.Supply:F0}");
            EditorGUILayout.LabelField($"Demand: {price.MonthlyDemand:F0}");
            EditorGUILayout.EndHorizontal();
            
            // Multipliers
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("  Multipliers:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"S/D: {price.SupplyDemandMultiplier:F2}");
            EditorGUILayout.LabelField($"Wealth: {price.VillageWealthMultiplier:F2}");
            EditorGUILayout.LabelField($"Event: {price.EventMultiplier:F2}");
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }
}
```

#### Task 6.2: Create Trade History Buffer (Optional)

**File:** `Assets/Scripts/Godgame/Economy/MarketTradeHistory.cs` (new file)

**Component:**
```csharp
[InternalBufferCapacity(32)]
public struct MarketTradeHistory : IBufferElementData
{
    public Entity Buyer;
    public Entity Seller;
    public FixedString64Bytes GoodType;
    public float Quantity;
    public float Price;
    public uint Tick;
}
```

**Integration:**
- MarketClearingSystem records trades to this buffer
- Debug display shows last N trades
- Can be used for telemetry and narrative hooks

#### Task 6.3: Add Telemetry Hooks

**Integration with PureDOTS Telemetry:**
- Emit telemetry metrics for:
  - Price changes (spikes, drops)
  - Supply/demand imbalances
  - Trade volume per GoodType

**File:** `Assets/Scripts/Godgame/Economy/MarketTelemetrySystem.cs` (new file)

**Deliverable:** Debug view shows market state, multipliers, and recent trades for tuning.

---

## Implementation Order

**Phase 1: Foundation (Steps 1-2)**
1. Create MarketPrice buffer component
2. Create BasePrice catalog (ScriptableObject + BlobAsset)
3. Create MarketBootstrapSystem to initialize buffers
4. Create GoodType mapping system
5. Implement MarketSupplyCalculationSystem
6. Implement MarketDemandCalculationSystem

**Phase 2: Core Pricing (Step 3)**
7. Create MarketPricingConfig (ScriptableObject + BlobAsset)
8. Implement MarketPricingSystem
9. Test price updates with simple scenarios

**Phase 3: Market Interaction (Step 4)**
10. Create MarketBuyIntent and MarketSellIntent components
11. Implement MarketClearingSystem
12. Test buy/sell flows end-to-end

**Phase 4: Integration (Step 5)**
13. Create TradeArrivalEvent component
14. Hook Chunk 4 unload to emit events (or rely on supply calculation)
15. Verify supply updates on trade arrivals

**Phase 5: Debug (Step 6)**
16. Create MarketDebugWindow (EditorWindow)
17. Create MarketTradeHistory buffer (optional)
18. Add telemetry hooks
19. Test debug view with real data

---

## Testing Strategy

**Simple Test Scenario:**
1. Create 2 villages (VillageA, VillageB)
2. Initialize MarketPrice buffers with Grain, IronIngot
3. Set VillageA: Supply=1000, Demand=500 (surplus)
4. Set VillageB: Supply=200, Demand=1000 (shortage)
5. Run pricing system → verify VillageA price lower, VillageB price higher
6. Create buy intent in VillageB, sell intent in VillageA
7. Run clearing system → verify trade executes at correct price
8. Verify wealth transaction recorded (Chunk 1)
9. Verify items moved (Chunk 2)

**Famine Test:**
1. Set VillageA with Famine event
2. Set Grain Supply=500, Demand=3000
3. Run pricing system → verify EventMultiplier applied
4. Verify price spike (CurrentPrice >> BasePrice)

---

## Key Files to Create/Modify

**New Files:**
- `Assets/Scripts/Godgame/Economy/MarketComponents.cs`
- `Assets/Scripts/Godgame/Economy/BasePriceCatalog.cs` (+ ScriptableObject authoring)
- `Assets/Scripts/Godgame/Economy/MarketBootstrapSystem.cs`
- `Assets/Scripts/Godgame/Economy/GoodTypeMapping.cs` (+ ScriptableObject authoring)
- `Assets/Scripts/Godgame/Economy/MarketSupplyCalculationSystem.cs`
- `Assets/Scripts/Godgame/Economy/MarketDemandCalculationSystem.cs`
- `Assets/Scripts/Godgame/Economy/MarketPricingConfig.cs` (+ ScriptableObject authoring)
- `Assets/Scripts/Godgame/Economy/MarketPricingSystem.cs`
- `Assets/Scripts/Godgame/Economy/MarketIntentComponents.cs`
- `Assets/Scripts/Godgame/Economy/MarketClearingSystem.cs`
- `Assets/Scripts/Godgame/Economy/MarketEvents.cs`
- `Assets/Scripts/Godgame/Economy/MarketDebugDisplaySystem.cs` (EditorWindow)
- `Assets/Scripts/Godgame/Economy/MarketTelemetrySystem.cs` (optional)

**Modify Existing Files:**
- Chunk 4 unload system (add TradeArrivalEvent emission, or rely on supply calculation)

**ScriptableObject Assets to Create:**
- `Assets/Data/Economy/BasePriceCatalog.asset`
- `Assets/Data/Economy/GoodTypeMapping.asset`
- `Assets/Data/Economy/MarketPricingConfig.asset`

---

## Validation Checklist

After each phase:

**Phase 1:**
- [ ] MarketPrice buffers exist on trade nodes
- [ ] BasePrice catalog loads correctly
- [ ] Supply calculation queries inventories correctly
- [ ] Demand calculation uses population/stats correctly

**Phase 2:**
- [ ] Prices update monthly
- [ ] Multipliers computed correctly
- [ ] Prices reflect supply/demand changes
- [ ] Event multipliers affect prices

**Phase 3:**
- [ ] Buy/sell intents can be created
- [ ] Market clearing matches buyers/sellers
- [ ] Wealth transactions recorded (Chunk 1)
- [ ] Items moved between inventories (Chunk 2)

**Phase 4:**
- [ ] Trade arrivals logged (optional)
- [ ] Supply updates after trade arrivals

**Phase 5:**
- [ ] Debug view shows market state
- [ ] Multipliers visible for tuning
- [ ] Trade history visible (if implemented)

---

## DOTS Compliance Notes

**Follow TRI_PROJECT_BRIEFING.md patterns:**
- Use `SystemAPI.Query<>()` with `.WithEntityAccess()` for entity access
- Use `HasBuffer<T>` / `GetBuffer<T>` for buffers (not `HasComponent<T>`)
- Use `ref` for blob access: `ref var catalog = ref blobRef.Value`
- Use `in` for read-only struct parameters in Burst
- Pre-define FixedString constants outside Burst
- Use rewind guards: `if (rewind.Mode != RewindMode.Record) return;`
- Use deterministic RNG with tick-based seeds

**Burst Compatibility:**
- Price calculations should be Burst-compiled
- Market clearing can be Burst-compiled if intent matching is simple
- Debug display systems stay in managed code (EditorWindow)

---

**Last Updated:** 2025-01-27  
**Status:** Implementation Plan – Ready to begin Phase 1
















