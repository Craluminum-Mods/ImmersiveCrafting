# ImmersiveCrafting

---

Here is all the documentation that will someday be moved to another place.

## `IC_UseOnLiquidContainer` collectible behavior

Use this itemstack on liquid container with A liquid to output B itemstack.

One behavior equals one recipe. To add multiple recipes, use the same behavior multiple times.

Usage example:

```json
{
  "name": "IC_UseOnLiquidContainer",
  "properties": {
    "consumeLiters": 1,
    "ingredientQuantity": 1,
    "liquidStack": { "type": "item", "code": "waterportion" },
    "outputStack": { "type": "item", "code": "dough-{type}", "quantity": 1 },
    "sound": "sounds/effect/water-fill",
    "spawnParticles": true
  }
}
```

1. **consumeLiters** (`float`) consume the amount of liquid
2. **ingredientQuantity** (`int`) defines how much of ingredient to consume.
3. **liquidStack** (`ItemStack`) liquid itemstack (to set quantity use **consumeLiters** instead).
4. **outputStack** (`ItemStack`) output itemstack.
5. **sound** (`AssetLocation`) sound played when interacting.
6. **spawnParticles** (`bool`) spawn particles based on the itemstack.
7. **forbidInteraction** (`bool`) forbid interaction for certain variants within one behavior.

---

## `IC_RemoveByTool` block behavior

Use A tool on this block to output B itemstack.

One behavior equals one recipe. To add multiple recipes, use the same behavior multiple times.

Usage example:

```json
{
  "name": "IC_RemoveByTool",
  "properties": {
    "outputStack": { "type": "item", "code": "lime", "quantity": 1 },
    "actionLangCode": "immersivecrafting:blockhelp-crushinto-lime",
    "toolDurabilityCost": 1,
    "toolTypes": ["Hammer"],
    "spawnParticles": true,
    "forbidInteractionByType": {
      "*-limestone-*": false,
      "*-chalk-*": false,
      "*": true
    }
  }
}
```

1. **actionLangCode** (`string`) interaction help when looking at the block.
2. **outputStack** (`ItemStack`) output itemstack.
3. **sound** (`AssetLocation`) sound played when interacting.
4. **spawnParticles** (`bool`) spawn particles based on the block
5. **toolDurabilityCost** (`int`) consume tool durability.
6. **tooltypes** (`string[]`) list of allowed tool types.
7. **forbidInteraction** (`bool`) forbid interaction for certain variants within one behavior.

---

## `IC_SealCrock` collectible behavior

Use this itemstack to seal placed crocks.

```json
{ "name": "IC_SealCrock" }
```

---

## `IC_WaxCheese` collectible behavior

Use this itemstack to wax placed raw cheese.

```json
{ "name": "IC_WaxCheese" }
```
