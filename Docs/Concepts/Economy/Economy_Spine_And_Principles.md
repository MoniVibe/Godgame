# Economy Spine & Principles

**Type:** Foundation Document  
**Status:** Foundation – applies to all economy work  
**Version:** Chunk 0  
**Scope:** Wealth, resources, businesses, trade, markets, policies (Godgame + Space4X)  
**Dependencies:** None (this is the foundation)  
**Last Updated:** 2025-01-27

---

## Purpose

The economic layer has grown complex: personal wealth, businesses, families, guilds, villages, resources with mass, trade routes, sanctions, etc.

**Chunk 0 exists to:**

- **Prevent "ghost money" and "ghost goods"** – All wealth and items must exist in explicit components, never as hidden floats or magical numbers
- **Avoid tangled dependencies** – Clear layer boundaries prevent wealth, items, trade, politics, and AI from becoming unmaintainable spaghetti
- **Make the economy moddable** – Behavior described in data catalogs, not hardcoded magic numbers
- **Make the economy predictable** – Deterministic, rewind-safe, observable
- **Make the economy debuggable** – Events and logs for all significant changes
- **Provide a simple checklist** – "Is this new system legal?" validation for future work

**This document has no code.** It is a set of rules and constraints that every later system (Chunks 1–6) must respect.

---

## Core Vocabulary

**Wealth:** Abstract currency/value owned by an entity (villager, family, business, guild, village, empire, carrier, colony). Stored in explicit wealth components or ledger entries.

**Resource / Item:** Physical goods with mass/volume (grain, ore, tools, weapons, luxuries, ships, modules, etc.). Must exist in inventories or they do not exist.

**Ledger:** Structured record of wealth balances and transfers. Provides audit trail and prevents ghost money.

**Inventory:** Container of resources/items attached to an entity. All physical goods must live in inventories.

**Layer:** Conceptual tier of the economy stack. Higher layers can depend on lower layers; never the reverse.

**Transaction:** Explicit record of wealth transfer (wages, profits, taxes, tribute, tariffs, fines, bribes, loans, interest, pillage). All wealth changes go through transactions.

**Exchange:** Explicit conversion between wealth and items (buy/sell, tax-in-kind, loot, tribute-in-goods). The only legal way to move between wealth and items.

---

## Rule 1 – Single Source of Truth

**One kind of thing, one place where that thing lives.**

### 3.1 Wealth

Wealth exists **only** in explicit wealth components, never as random floats inside other systems.

**Examples of correct wealth storage:**

```csharp
// ✅ CORRECT - Explicit wealth component
public struct VillagerWealth : IComponentData
{
    public float Balance;
}

// ✅ CORRECT - Ledger entry for aggregates
public struct WealthLedgerEntry : IBufferElementData
{
    public Entity Owner;
    public float Balance;
    public uint LastTransactionTick;
}

// ✅ CORRECT - Business balance
public struct BusinessBalance : IComponentData
{
    public float Cash;
    public float AccountsReceivable;
    public float AccountsPayable;
}

// ✅ CORRECT - Aggregate treasuries
public struct VillageTreasury : IComponentData
{
    public float Balance;
}

public struct EmpireTreasury : IComponentData
{
    public float Balance;
}
```

**All changes to wealth go through explicit transactions:**

```csharp
// ✅ CORRECT - Transaction record
public struct WealthTransaction : IBufferElementData
{
    public Entity From;
    public Entity To;
    public float Amount;
    public FixedString64Bytes Reason;  // "wage", "tax", "purchase", "loan", etc.
    public uint Tick;
}

// ✅ CORRECT - Transaction system
[BurstCompile]
public partial struct PayWageSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // ... query employees and employers ...
        
        // Record transaction
        var transaction = new WealthTransaction
        {
            From = employer,
            To = employee,
            Amount = wageAmount,
            Reason = WageTransactionReason,  // Pre-defined constant
            Tick = SystemAPI.GetSingleton<TickTimeState>().Tick
        };
        
        // Update balances
        var employerWealth = SystemAPI.GetComponentRW<BusinessBalance>(employer);
        var employeeWealth = SystemAPI.GetComponentRW<VillagerWealth>(employee);
        
        employerWealth.ValueRW.Cash -= wageAmount;
        employeeWealth.ValueRW.Balance += wageAmount;
        
        // Add transaction to ledger
        SystemAPI.GetBuffer<WealthTransaction>(ledgerEntity).Add(transaction);
    }
    
    private static readonly FixedString64Bytes WageTransactionReason = "wage";
}
```

**Anti-patterns – DO NOT DO THIS:**

```csharp
// ❌ WRONG - Ghost money in quest system
public struct QuestReward : IComponentData
{
    public float Experience;
    public float WealthBonus;  // NO! Wealth doesn't belong here
}

// ❌ WRONG - Silent wealth modification
public partial struct QuestCompletionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // ... complete quest ...
        
        // NO! Don't directly modify wealth in quest system
        var villagerWealth = SystemAPI.GetComponentRW<VillagerWealth>(villager);
        villagerWealth.ValueRW.Balance += 100;  // Ghost money!
    }
}

// ❌ WRONG - Miracle generates food without items
public partial struct MiracleFoodSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // NO! Food must exist as items in inventory
        village.FoodCount += 1000;  // Ghost goods!
    }
}

// ❌ WRONG - Business upgrade magically increases owner wealth
public partial struct BusinessUpgradeSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // NO! Upgrade should increase profit-generating capacity, not directly add wealth
        var ownerWealth = SystemAPI.GetComponentRW<VillagerWealth>(owner);
        ownerWealth.ValueRW.Balance += upgradeCost;  // Ghost money!
    }
}
```

**Correct patterns:**

```csharp
// ✅ CORRECT - Quest gives wealth via transaction
public partial struct QuestCompletionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // ... complete quest ...
        
        // Record transaction
        RecordTransaction(villager, questGiver, rewardAmount, "quest_reward");
    }
}

// ✅ CORRECT - Miracle creates items in inventory
public partial struct MiracleFoodSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Create actual items in storehouse inventory
        var inventory = SystemAPI.GetBuffer<InventoryItem>(storehouse);
        inventory.Add(new InventoryItem
        {
            ResourceTypeId = GrainResourceId,
            Quantity = 1000f,
            Mass = 1000f * GrainMassPerUnit
        });
    }
}

// ✅ CORRECT - Business upgrade increases capacity, profits flow naturally
public partial struct BusinessUpgradeSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Upgrade increases production capacity
        var business = SystemAPI.GetComponentRW<BusinessProduction>(businessEntity);
        business.ValueRW.MaxOutputPerTick *= 1.5f;  // More capacity
        
        // Profits increase naturally through production system
        // Owner wealth increases through profit distribution system
    }
}
```

**If a system "gives" something of value, it must either:**
- Credit wealth via transaction, OR
- Deliver a physical item to an inventory, OR
- Both (if intended), but never neither

### 3.2 Resources & Items

All physical goods live in inventories, or they do not exist.

**Examples of correct inventory storage:**

```csharp
// ✅ CORRECT - Inventory item
public struct InventoryItem : IBufferElementData
{
    public FixedString64Bytes ResourceTypeId;
    public float Quantity;
    public float Mass;  // Derived from ResourceSpec + quantity
    public float Volume;  // Derived from ResourceSpec + quantity
}

// ✅ CORRECT - Inventory component
public struct Inventory : IComponentData
{
    public float MaxMass;
    public float MaxVolume;
    public float CurrentMass;
    public float CurrentVolume;
}

// ✅ CORRECT - Resource spec in blob catalog
public struct ResourceSpecBlob
{
    public FixedString64Bytes Id;
    public float MassPerUnit;
    public float VolumePerUnit;
    public float BaseValue;
    public ResourceCategory Category;
}
```

**Anti-patterns – DO NOT DO THIS:**

```csharp
// ❌ WRONG - Hidden counters in business
public struct BusinessState : IComponentData
{
    public float IronStockpile;  // NO! Items belong in inventory
    public float WoodStockpile;  // NO! Items belong in inventory
}

// ❌ WRONG - Village food counter outside inventory
public struct VillageStats : IComponentData
{
    public float FoodSupply;  // NO! Food must be in storehouse inventory
    public float WaterSupply;  // NO! Water must be in inventory
}

// ❌ WRONG - Re-inventing mass calculation
public struct MyResource : IComponentData
{
    public float Amount;
    public float MyCustomMass;  // NO! Use ResourceSpec + quantity
}
```

**Correct patterns:**

```csharp
// ✅ CORRECT - Business uses inventory
public struct BusinessInventory : IComponentData
{
    public Entity InventoryEntity;  // Reference to inventory entity
}

// ✅ CORRECT - Village food in storehouse inventory
public partial struct VillageFoodSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Query storehouse inventory
        var inventory = SystemAPI.GetBuffer<InventoryItem>(storehouse);
        
        // Calculate total food from inventory items
        float totalFood = 0f;
        for (int i = 0; i < inventory.Length; i++)
        {
            if (IsFoodType(inventory[i].ResourceTypeId))
            {
                totalFood += inventory[i].Quantity;
            }
        }
    }
}

// ✅ CORRECT - Mass derived from spec
public partial struct CalculateInventoryMassSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        ref var resourceCatalog = ref SystemAPI.GetSingleton<ResourceTypeIndex>().Catalog.Value;
        
        var inventory = SystemAPI.GetBuffer<InventoryItem>(entity);
        float totalMass = 0f;
        
        for (int i = 0; i < inventory.Length; i++)
        {
            var item = inventory[i];
            if (resourceCatalog.TryFind(item.ResourceTypeId, out var spec))
            {
                totalMass += item.Quantity * spec.MassPerUnit;
            }
        }
        
        var inventoryComp = SystemAPI.GetComponentRW<Inventory>(entity);
        inventoryComp.ValueRW.CurrentMass = totalMass;
    }
}
```

**Transport, spoilage, production all operate by moving/transforming inventory entries:**

```csharp
// ✅ CORRECT - Transport moves items between inventories
public partial struct TransportSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Remove from source inventory
        var sourceInventory = SystemAPI.GetBuffer<InventoryItem>(source);
        // ... find and remove items ...
        
        // Add to destination inventory
        var destInventory = SystemAPI.GetBuffer<InventoryItem>(destination);
        destInventory.Add(item);
    }
}

// ✅ CORRECT - Spoilage removes items from inventory
public partial struct SpoilageSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var inventory = SystemAPI.GetBuffer<InventoryItem>(entity);
        
        for (int i = inventory.Length - 1; i >= 0; i--)
        {
            if (ShouldSpoil(inventory[i]))
            {
                inventory.RemoveAt(i);  // Item destroyed, removed from inventory
            }
        }
    }
}

// ✅ CORRECT - Production adds items to inventory
public partial struct ProductionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // ... consume inputs from inventory ...
        // ... produce outputs ...
        
        // Add produced items to inventory
        var inventory = SystemAPI.GetBuffer<InventoryItem>(producer);
        inventory.Add(new InventoryItem
        {
            ResourceTypeId = outputType,
            Quantity = outputQuantity,
            Mass = outputQuantity * spec.MassPerUnit
        });
    }
}
```

### 3.3 Linking Wealth and Items

You can move between wealth and items **only** via explicit exchange.

**Legal exchange operations:**

```csharp
// ✅ CORRECT - Buy operation
public partial struct BuyItemSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // 1. Check buyer has enough wealth
        var buyerWealth = SystemAPI.GetComponentRO<VillagerWealth>(buyer);
        if (buyerWealth.ValueRO.Balance < price) return;
        
        // 2. Check seller has item in inventory
        var sellerInventory = SystemAPI.GetBuffer<InventoryItem>(seller);
        if (!HasItem(sellerInventory, itemType, quantity)) return;
        
        // 3. Record wealth transaction
        RecordTransaction(buyer, seller, price, "purchase");
        
        // 4. Transfer wealth
        var buyerWealthRW = SystemAPI.GetComponentRW<VillagerWealth>(buyer);
        var sellerWealthRW = SystemAPI.GetComponentRW<VillagerWealth>(seller);
        buyerWealthRW.ValueRW.Balance -= price;
        sellerWealthRW.ValueRW.Balance += price;
        
        // 5. Transfer item
        RemoveFromInventory(sellerInventory, itemType, quantity);
        AddToInventory(buyerInventory, itemType, quantity);
    }
}

// ✅ CORRECT - Tax-in-kind
public partial struct TaxInKindSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Tax collector takes items directly
        var taxpayerInventory = SystemAPI.GetBuffer<InventoryItem>(taxpayer);
        var taxCollectorInventory = SystemAPI.GetBuffer<InventoryItem>(taxCollector);
        
        // Remove items from taxpayer
        var taxItems = CalculateTaxInKind(taxpayerInventory);
        RemoveFromInventory(taxpayerInventory, taxItems);
        
        // Add to tax collector inventory
        AddToInventory(taxCollectorInventory, taxItems);
    }
}

// ✅ CORRECT - Loot operation
public partial struct LootSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Looter takes items from looted entity
        var lootedInventory = SystemAPI.GetBuffer<InventoryItem>(looted);
        var looterInventory = SystemAPI.GetBuffer<InventoryItem>(looter);
        
        // Transfer all items
        for (int i = 0; i < lootedInventory.Length; i++)
        {
            looterInventory.Add(lootedInventory[i]);
        }
        lootedInventory.Clear();
    }
}
```

**Anti-patterns – DO NOT DO THIS:**

```csharp
// ❌ WRONG - Prestige as secret wealth
public struct VillagerStats : IComponentData
{
    public float Wealth;
    public float Prestige;  // NO! If valuable but non-tradable, model as separate stat
    // Prestige should be its own component, not hidden wealth
}

// ❌ WRONG - Favor as wealth
public struct RelationshipState : IComponentData
{
    public float Favor;  // NO! Favor is not wealth
    // If favor can be "spent", model as separate resource, not wealth
}
```

**Correct patterns for non-tradable value:**

```csharp
// ✅ CORRECT - Prestige as separate stat
public struct Prestige : IComponentData
{
    public float Value;  // Separate from wealth
}

// ✅ CORRECT - Favor as relationship metric
public struct RelationshipFavor : IComponentData
{
    public float Value;  // Not wealth, but can influence transactions
}

// ✅ CORRECT - Favor can influence prices, but isn't wealth itself
public partial struct CalculateTradePriceSystem : ISystem
{
    public float CalculatePrice(Entity buyer, Entity seller, float basePrice)
    {
        var favor = SystemAPI.GetComponentRO<RelationshipFavor>(buyer);
        float discount = favor.ValueRO.Value * 0.01f;  // 1% discount per favor point
        return basePrice * (1f - discount);
    }
}
```

---

## Rule 2 – Layers, Not Tangles

**Higher layers can depend on lower layers; never the other way around.**

### The 6-Layer Stack

```
Layer 6: Policies & Macro
    ↓ (depends on)
Layer 5: Markets & Prices
    ↓ (depends on)
Layer 4: Trade & Logistics
    ↓ (depends on)
Layer 3: Businesses & Production
    ↓ (depends on)
Layer 2: Resources & Mass (Inventories)
    ↓ (depends on)
Layer 1: Wealth & Ledgers
```

**Layer 1: Wealth & Ledgers** (Bottom)
- Components: `VillagerWealth`, `BusinessBalance`, `VillageTreasury`, `WealthLedger`, `WealthTransaction`
- Systems: Transaction recording, balance updates, ledger maintenance
- Dependencies: None (only global constants/configs)

**Layer 2: Resources & Mass (Inventories)**
- Components: `Inventory`, `InventoryItem`, `ResourceSpec`, `ResourceTypeIndex`
- Systems: Inventory management, mass/volume calculation, item transfer
- Dependencies: Layer 1 (for payment queries only, via well-defined interfaces)

**Layer 3: Businesses & Production**
- Components: `BusinessEntity`, `BusinessBalance`, `BusinessProduction`, `EmploymentContract`
- Systems: Production recipes, wage payment, profit calculation
- Dependencies: Layer 1 (wealth), Layer 2 (inventories)
- May look up base prices from `ItemSpec` but NOT dynamic market prices

**Layer 4: Trade & Logistics**
- Components: `TradeRoute`, `Caravan`, `TradeAgreement`, `LogisticsRoute`
- Systems: Route calculation, transport, trade execution
- Dependencies: Layer 1 (wealth), Layer 2 (inventories), route configs
- Does NOT modify tax policy, macro rules, or create/modify markets directly

**Layer 5: Markets & Prices**
- Components: `MarketPrice`, `MarketState`, `SupplyDemandState`
- Systems: Price calculation, supply/demand updates, market events
- Dependencies: Layers 1-4 (observes inventories, production, trade flows, demand signals)
- Updates `MarketPrice` components only

**Layer 6: Policies & Macro** (Top)
- Components: `TaxPolicy`, `TariffPolicy`, `SanctionPolicy`, `SubsidyPolicy`, `LoanPolicy`
- Systems: Policy updates, macro effects calculation
- Dependencies: Layers 1-5 (reads everything below)
- Writes only policy components; lower layers react to policy via their own systems

### 4.1 Allowed Dependencies

**Layer 1 (Wealth) – Bottom:**
- ✅ May depend on: Global constants/configs (currency format, transaction types)
- ❌ May NOT depend on: Prices, markets, trade routes, policies, inventories, production

**Layer 2 (Resources):**
- ✅ May depend on: Layer 1 (for payment queries via interfaces), ResourceSpec catalogs
- ❌ May NOT depend on: Markets, prices, trade routes, policies, production

**Layer 3 (Businesses):**
- ✅ May depend on: Layer 1 (wealth), Layer 2 (inventories), base prices from ItemSpecs
- ❌ May NOT depend on: Dynamic market prices, trade routes, policies

**Layer 4 (Trade):**
- ✅ May depend on: Layer 1 (wealth), Layer 2 (inventories), route configs, trade agreements
- ❌ May NOT depend on: Market price calculations, tax policy modifications, macro rules

**Layer 5 (Markets):**
- ✅ May depend on: Layers 1-4 (observes all below), demand signals, supply data
- ❌ May NOT depend on: Policy modifications, direct business recipe changes

**Layer 6 (Policies):**
- ✅ May depend on: All layers below (reads everything)
- ❌ May NOT depend on: Direct modification of wealth/inventories (changes rules, not data)

### 4.2 Cross-Layer Communication Pattern

**Use explicit data and events, not cross-cutting special cases.**

**✅ CORRECT - Event-based communication:**

```csharp
// Layer 3: Production system posts event
public struct ProductionEvent : IBufferElementData
{
    public Entity Producer;
    public FixedString64Bytes ResourceTypeId;
    public float Quantity;
    public uint Tick;
}

public partial struct ProductionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // ... produce items ...
        
        // Post event for Layer 5 (Markets) to observe
        var events = SystemAPI.GetBuffer<ProductionEvent>(eventEntity);
        events.Add(new ProductionEvent
        {
            Producer = producer,
            ResourceTypeId = outputType,
            Quantity = quantity,
            Tick = SystemAPI.GetSingleton<TickTimeState>().Tick
        });
    }
}

// Layer 5: Market system reads events
public partial struct MarketPriceSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Read production events
        var events = SystemAPI.GetBuffer<ProductionEvent>(eventEntity);
        for (int i = 0; i < events.Length; i++)
        {
            // Update supply/demand based on production
            UpdateSupply(events[i].ResourceTypeId, events[i].Quantity);
        }
    }
}
```

**✅ CORRECT - Policy reads by lower layers:**

```csharp
// Layer 6: Policy system sets policy
public struct VillageTaxPolicy : IComponentData
{
    public float IncomeTaxRate;
    public float SalesTaxRate;
    public float PropertyTaxRate;
}

public partial struct TaxPolicySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Policy system updates policy component only
        var policy = SystemAPI.GetComponentRW<VillageTaxPolicy>(village);
        policy.ValueRW.IncomeTaxRate = 0.15f;  // 15% tax
    }
}

// Layer 3: Payroll system uses policy
public partial struct PayrollSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Read policy
        var policy = SystemAPI.GetComponentRO<VillageTaxPolicy>(village);
        
        // Calculate wage
        float grossWage = CalculateWage(employee);
        
        // Apply tax policy
        float tax = grossWage * policy.ValueRO.IncomeTaxRate;
        float netWage = grossWage - tax;
        
        // Record transaction (Layer 1)
        RecordTransaction(employer, employee, netWage, "wage");
        RecordTransaction(employee, village, tax, "tax");
    }
}
```

**❌ WRONG - Anti-patterns:**

```csharp
// ❌ WRONG - Market system directly editing business recipes
public partial struct MarketSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // NO! Markets shouldn't modify production recipes
        var business = SystemAPI.GetComponentRW<BusinessProduction>(businessEntity);
        business.ValueRW.Recipe.InputAmount *= 0.5f;  // "Steel is cheap, use more"
    }
}

// ❌ WRONG - Trade route system modifying tax policy
public partial struct TradeRouteSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // NO! Trade shouldn't modify tax policy
        var policy = SystemAPI.GetComponentRW<VillageTaxPolicy>(village);
        policy.ValueRW.SalesTaxRate = 0f;  // "Free trade route, no tax"
    }
}

// ❌ WRONG - Policy system directly giving wealth/items
public partial struct PolicySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // NO! Policy should change rules, not directly modify data
        var wealth = SystemAPI.GetComponentRW<VillagerWealth>(villager);
        wealth.ValueRW.Balance += 1000f;  // "Subsidy"
        
        // Instead, policy should set subsidy component, subsidy system applies it
    }
}
```

**✅ CORRECT - Policy sets rules, other systems apply:**

```csharp
// Layer 6: Policy sets subsidy rule
public struct SubsidyPolicy : IComponentData
{
    public Entity TargetEntity;
    public float AmountPerTick;
    public uint EndTick;
}

public partial struct PolicySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Policy creates subsidy component
        SystemAPI.AddComponent<SubsidyPolicy>(villager, new SubsidyPolicy
        {
            TargetEntity = villager,
            AmountPerTick = 10f,
            EndTick = currentTick + 100
        });
    }
}

// Layer 1: Subsidy application system applies subsidy
public partial struct SubsidyApplicationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Read subsidy policy
        var subsidy = SystemAPI.GetComponentRO<SubsidyPolicy>(villager);
        
        // Apply via transaction (Layer 1)
        RecordTransaction(village, villager, subsidy.ValueRO.AmountPerTick, "subsidy");
    }
}
```

**When in doubt, ask:**
- "Which layer does this concept belong to?"
- "Does this system live at that layer or higher?"
- "Am I modifying data from a lower layer directly, or setting rules that lower layers will use?"

---

## Rule 3 – Data > Code

**Behavior should be described in data; systems should just read data and apply generic math.**

### 5.1 What Goes Into Data Catalogs

Everything you can imagine mods changing should be in data catalogs (BlobAsset catalogs built from ScriptableObjects/data files).

**Economy shape (moddable):**

```csharp
// ✅ CORRECT - Wealth tiers in catalog
public struct WealthTierBlob
{
    public FixedString64Bytes TierName;  // "Pauper", "Peasant", "Middle", "Elite", "Oligarch"
    public float MinWealth;
    public float MaxWealth;
    public FixedString64Bytes Title;
}

// ✅ CORRECT - Item specs in catalog
public struct ItemSpecBlob
{
    public FixedString64Bytes Id;
    public float MassPerUnit;
    public float VolumePerUnit;
    public float BaseValue;
    public ResourceCategory Category;
    public bool IsTradable;
    public bool IsPerishable;
    public float PerishRate;
}

// ✅ CORRECT - Business recipes in catalog
public struct BusinessRecipeBlob
{
    public FixedString64Bytes BusinessType;
    public BlobArray<RecipeInput> Inputs;
    public BlobArray<RecipeOutput> Outputs;
    public float OperatingCostPerTick;
    public float BaseWagePerTick;
    public float ExperienceGainRate;
}

// ✅ CORRECT - Trade route templates in catalog
public struct TradeRouteTemplateBlob
{
    public FixedString64Bytes RouteType;  // "Stagecoach", "Caravan", "Ship"
    public float BaseSpeedKmPerDay;
    public float BaseCostPer100kgPer100km;
    public float CapacityKg;
    public float RiskMultiplier;
}
```

**Price & market behavior (moddable):**

```csharp
// ✅ CORRECT - Supply/demand curves in catalog
public struct SupplyDemandCurveBlob
{
    public FixedString64Bytes ResourceTypeId;
    public BlobArray<CurvePoint> SupplyCurve;  // Price vs quantity supplied
    public BlobArray<CurvePoint> DemandCurve;  // Price vs quantity demanded
    public float DampingFactor;  // How quickly prices adjust
    public float VolatilityFactor;  // Price swing magnitude
}

// ✅ CORRECT - Event multipliers in catalog
public struct EconomicEventMultiplierBlob
{
    public FixedString64Bytes EventType;  // "Famine", "War", "Plague", "Festival"
    public BlobArray<ResourceMultiplier> ResourceMultipliers;
    public float DurationTicks;
    public float PriceImpactMultiplier;
}
```

**Policies & macro (moddable):**

```csharp
// ✅ CORRECT - Tax brackets in catalog
public struct TaxBracketBlob
{
    public float MinIncome;
    public float MaxIncome;
    public float TaxRate;
}

// ✅ CORRECT - Tariff rules in catalog
public struct TariffRuleBlob
{
    public FixedString64Bytes SourceVillageId;
    public FixedString64Bytes DestinationVillageId;
    public BlobArray<ResourceTariff> ResourceTariffs;
    public float BaseTariffRate;
}

// ✅ CORRECT - Debt rules in catalog
public struct DebtRuleBlob
{
    public float InterestRate;
    public float DefaultThreshold;  // % of income
    public float MaxDebtToIncomeRatio;
    public uint GracePeriodTicks;
}

// ✅ CORRECT - Sanction rules in catalog
public struct SanctionRuleBlob
{
    public FixedString64Bytes TargetEntityId;
    public BlobArray<FixedString64Bytes> BlockedResourceTypes;
    public float SmugglingPenalty;
    public float DetectionChance;
}
```

**Benefits of data-driven design:**
- Base game, Godgame, Space4X, and mods can all ship their own configs
- Same systems behave very differently based on loaded data
- Easy to test different economic scenarios
- Non-programmers can balance the economy

### 5.2 What Stays In Code

Systems should:
- ✅ Read config, inputs (wealth, inventories, routes, policies)
- ✅ Apply simple, generic formulas
- ✅ Write outputs (updated balances, inventories, prices, events)

Systems should NOT:
- ❌ Hardcode magic constants that belong in config ("tax is 20%", "max caravan weight = 1,000")
- ❌ Embed narrative-specific logic ("if kingdom is evil, always set interest to 30%")

**✅ CORRECT - Generic system reading config:**

```csharp
// ✅ CORRECT - System reads tax rate from policy component
public partial struct TaxCalculationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var policy = SystemAPI.GetComponentRO<VillageTaxPolicy>(village);
        var taxBrackets = SystemAPI.GetSingleton<TaxBracketCatalog>().Catalog.Value;
        
        // Read income from wealth component
        var wealth = SystemAPI.GetComponentRO<VillagerWealth>(villager);
        float income = wealth.ValueRO.Balance;
        
        // Apply tax brackets from catalog (data-driven)
        float tax = CalculateTax(income, taxBrackets);
        
        // Record transaction
        RecordTransaction(villager, village, tax, "tax");
    }
    
    private float CalculateTax(float income, BlobArray<TaxBracketBlob> brackets)
    {
        float totalTax = 0f;
        for (int i = 0; i < brackets.Length; i++)
        {
            if (income > brackets[i].MinIncome)
            {
                float taxableInThisBracket = math.min(income, brackets[i].MaxIncome) - brackets[i].MinIncome;
                totalTax += taxableInThisBracket * brackets[i].TaxRate;
            }
        }
        return totalTax;
    }
}
```

**❌ WRONG - Hardcoded magic numbers:**

```csharp
// ❌ WRONG - Hardcoded tax rate
public partial struct TaxCalculationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var wealth = SystemAPI.GetComponentRO<VillagerWealth>(villager);
        float income = wealth.ValueRO.Balance;
        
        // NO! Tax rate should be in policy/config
        float tax = income * 0.20f;  // Hardcoded 20%
        
        RecordTransaction(villager, village, tax, "tax");
    }
}

// ❌ WRONG - Hardcoded max weight
public partial struct CaravanSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // NO! Max weight should be in TradeRouteTemplate catalog
        const float MaxCaravanWeight = 1000f;  // Hardcoded
        
        if (totalWeight > MaxCaravanWeight)
        {
            // Reject cargo
        }
    }
}

// ❌ WRONG - Narrative-specific logic
public partial struct LoanSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var alignment = SystemAPI.GetComponentRO<VillageAlignment>(village);
        
        // NO! Interest rate should be in DebtRule catalog
        float interestRate = alignment.ValueRO.IsEvil ? 0.30f : 0.10f;  // Narrative logic in code
    }
}
```

**✅ CORRECT - Narrative logic in data:**

```csharp
// ✅ CORRECT - Interest rate in catalog, can vary by alignment via data
public struct DebtRuleBlob
{
    public float BaseInterestRate;
    public BlobArray<AlignmentMultiplier> AlignmentMultipliers;  // Data-driven alignment effects
}

public partial struct LoanSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var debtRules = SystemAPI.GetSingleton<DebtRuleCatalog>().Catalog.Value;
        var alignment = SystemAPI.GetComponentRO<VillageAlignment>(village);
        
        // Read base rate from catalog
        float interestRate = debtRules.BaseInterestRate;
        
        // Apply alignment multiplier from catalog (data-driven)
        for (int i = 0; i < debtRules.AlignmentMultipliers.Length; i++)
        {
            if (debtRules.AlignmentMultipliers[i].Alignment == alignment.ValueRO.Value)
            {
                interestRate *= debtRules.AlignmentMultipliers[i].Multiplier;
                break;
            }
        }
        
        // Apply interest
        ApplyInterest(loan, interestRate);
    }
}
```

**Decision guide:**
- "Is this bound to this one scenario?" → Maybe ok, but consider per-scenario config
- "Can I imagine a mod wanting to change this?" → Move it to a catalog/config
- "Does this vary by game mode, difficulty, or narrative?" → Definitely in catalog/config

---

## Cross-Cutting Constraints

These apply to all economy layers.

### Determinism & Rewind-Friendly

**All economic systems must be deterministic for a given world state & tick.**

```csharp
// ✅ CORRECT - Deterministic price calculation
public partial struct MarketPriceSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var rewind = SystemAPI.GetSingleton<RewindState>();
        if (rewind.Mode != RewindMode.Record) return;  // Rewind guard
        
        var tick = SystemAPI.GetSingleton<TickTimeState>().Tick;
        var random = new Unity.Mathematics.Random((uint)(tick + entityHash));  // Seeded RNG
        
        // Deterministic calculation
        float price = CalculatePrice(supply, demand, random);
        
        var marketPrice = SystemAPI.GetComponentRW<MarketPrice>(market);
        marketPrice.ValueRW.Value = price;
    }
}

// ❌ WRONG - Non-deterministic random
public partial struct MarketPriceSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // NO! Random without seed is non-deterministic
        float price = CalculatePrice(supply, demand, UnityEngine.Random.value);
    }
}
```

**No hidden random wealth/loot adjustments; randomness is seeded and explicit:**

```csharp
// ✅ CORRECT - Seeded random for loot
public partial struct LootGenerationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var tick = SystemAPI.GetSingleton<TickTimeState>().Tick;
        var random = new Unity.Mathematics.Random((uint)(tick + entityHash));
        
        // Deterministic loot generation
        float lootValue = baseValue * (0.8f + random.NextFloat() * 0.4f);  // 80-120% of base
        
        // Generate items in inventory (explicit, not hidden)
        GenerateLootItems(inventory, lootValue, random);
    }
}
```

### Observability

**Big changes generate events/log entries that debug tools and narrative can read.**

```csharp
// ✅ CORRECT - Price spike event
public struct PriceSpikeEvent : IBufferElementData
{
    public Entity MarketEntity;
    public FixedString64Bytes ResourceTypeId;
    public float OldPrice;
    public float NewPrice;
    public float ChangePercent;
    public uint Tick;
}

public partial struct MarketPriceSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var oldPrice = marketPrice.ValueRO.Value;
        marketPrice.ValueRW.Value = newPrice;
        
        // Emit event if significant change
        if (math.abs(newPrice - oldPrice) / oldPrice > 0.1f)  // 10% change
        {
            var events = SystemAPI.GetBuffer<PriceSpikeEvent>(eventEntity);
            events.Add(new PriceSpikeEvent
            {
                MarketEntity = market,
                ResourceTypeId = resourceType,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                ChangePercent = (newPrice - oldPrice) / oldPrice * 100f,
                Tick = SystemAPI.GetSingleton<TickTimeState>().Tick
            });
        }
    }
}

// ✅ CORRECT - Large trade event
public struct LargeTradeEvent : IBufferElementData
{
    public Entity Buyer;
    public Entity Seller;
    public FixedString64Bytes ResourceTypeId;
    public float Quantity;
    public float TotalValue;
    public uint Tick;
}

// ✅ CORRECT - Loan default event
public struct LoanDefaultEvent : IBufferElementData
{
    public Entity Borrower;
    public Entity Lender;
    public float DefaultedAmount;
    public uint Tick;
}
```

**Economies are opaque by default; these logs are how you'll debug weirdness later.**

### Performance & Scaling

**Always assume thousands of entities (villagers, caravans, businesses).**

```csharp
// ✅ CORRECT - Aggregated market stats
public struct VillageMarketStats : IComponentData
{
    public float TotalSupply;
    public float TotalDemand;
    public float AveragePrice;
    public uint LastUpdateTick;
}

// System updates aggregate stats, not per-entity
public partial struct MarketStatsSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Aggregate all businesses in village
        float totalSupply = 0f;
        float totalDemand = 0f;
        
        // ... aggregate from businesses ...
        
        var stats = SystemAPI.GetComponentRW<VillageMarketStats>(village);
        stats.ValueRW.TotalSupply = totalSupply;
        stats.ValueRW.TotalDemand = totalDemand;
        stats.ValueRW.AveragePrice = CalculateAveragePrice(totalSupply, totalDemand);
    }
}

// ❌ WRONG - Per-entity heavy logic
public partial struct MarketSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // NO! Don't calculate market stats for every villager individually
        foreach (var (entity, villager) in SystemAPI.Query<Entity, Villager>())
        {
            CalculateMarketStatsForVillager(entity);  // Too expensive!
        }
    }
}
```

**Prefer aggregated views over per-entity heavy logic where possible.**

---

## Checklist for Future Work

When you or future-you implement a new economic feature, answer these questions:

### 1. Which layer is it?

- Wealth / Resources / Business / Trade / Market / Policy?
- If it spans multiple layers, split it into separate systems (one per layer)

### 2. What is the single source of truth?

- Where do the numbers live?
- Ledger, inventory, price buffer, policy component?
- If you're storing the same data in two places, you're violating Rule 1

### 3. Which lower layers does it depend on?

- Is that dependency allowed by the stack?
- If Layer 4 depends on Layer 5, you're violating Rule 2

### 4. What data should be externalized?

- Add or extend a catalog/config before hardcoding numbers
- If a mod might want to change it, it belongs in a catalog

### 5. What events or logs should it emit?

- How will we know it's working or misbehaving?
- Price spikes, large trades, loan defaults, tax changes → all need events

### Validation

If a system breaks any of the three rules, either:
- Move it to a different layer,
- Split it into two systems (one per layer), or
- Add/adjust data catalogs so you're not baking content into code

---

## Integration Notes

### Related Documents

This foundational document guides all economy work. Related concept documents:

- **Chunk 1 (Wealth)**: [`Wealth_And_Ledgers_Chunk1.md`](Wealth_And_Ledgers_Chunk1.md) - Wealth & Ledgers design specification  
  - Related: [`Wealth_And_Social_Dynamics.md`](../Villagers/Wealth_And_Social_Dynamics.md) - Wealth strata, social dynamics
- **Chunk 2 (Resources)**: [`Resources_And_Mass_Chunk2.md`](Resources_And_Mass_Chunk2.md) - Resources & Mass design specification  
  - Related: [`Resource_Production_And_Crafting.md`](Resource_Production_And_Crafting.md) - Production recipes (Chunk 3)
- **Chunk 3 (Businesses)**: [`Businesses_And_Production_Chunk3.md`](Businesses_And_Production_Chunk3.md) - Businesses & Production design specification  
  - Related: [`BusinessAndAssets.md`](BusinessAndAssets.md) - Business ownership, production, employment
- **Chunk 4 (Trade)**: [`Trade_And_Logistics_Chunk4.md`](Trade_And_Logistics_Chunk4.md) - Trade & Logistics design specification  
  - Related: [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) - Trade routes, caravans, logistics
- **Chunk 5 (Markets)**: [`Markets_And_Prices_Chunk5.md`](Markets_And_Prices_Chunk5.md) - Markets & Prices design specification  
  - Related: [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) - Market pricing, supply/demand dynamics
- **Chunk 6 (Policies)**: [`Policies_And_Macro_Chunk6.md`](Policies_And_Macro_Chunk6.md) - Policies & Macro design specification  
  - Related: [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) - Debt mechanics, loan behaviors, vassalization

### PureDOTS Integration

PureDOTS provides foundational systems that economy layers build upon:

- **Resource Systems**: `ResourceRegistrySystem`, `ResourceSourceConfig`, `ResourceTypeId` (Layer 2)
- **Storehouse API**: Inventory management (Layer 2)
- **Registry Infrastructure**: Entity registries for efficient queries (all layers)
- **Time/Rewind**: Deterministic tick time, rewind-safe simulation (all layers)

Economy systems should integrate with PureDOTS patterns:
- Use `ResourceTypeId` for resource identification
- Use storehouse inventory APIs for item storage
- Follow rewind guards (`RewindState.Mode != RewindMode.Record`)
- Use deterministic RNG with tick-based seeds

### Cross-Project Applicability

These principles apply to both:

- **Godgame**: Villagers, villages, bands, businesses, trade routes
- **Space4X**: Carriers, colonies, fleets, logistics routes, empire economies

Examples in this document use Godgame terminology (villagers, villages), but the same patterns apply to Space4X (carriers, colonies) with appropriate entity type substitutions.

---

**Last Updated:** 2025-01-27  
**Maintainer:** Economy Architecture Team  
**Status:** Foundation – All future economy work must comply with these principles

