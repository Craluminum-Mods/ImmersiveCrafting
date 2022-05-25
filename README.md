# ImmersiveCrafting

---

Here is all the documentation that will someday be moved to another place.

## `IC_UseOnLiquidContainer` collectible behavior

Use this itemstack on liquid container with A liquid to output B itemstack.

Example of implementation:

```json
{
  "name": "IC_UseOnLiquidContainer",
  "properties": {
    "actionLangCode": "immersivecrafting:heldhelp-makedough",
    "consumeLiters": 1,
    "ingredientQuantity": 1,
    "liquidStack": { "type": "item", "code": "waterportion" },
    "outputStack": { "type": "item", "code": "dough-{type}", "quantity": 1 },
    "sound": "effect/water-fill",
    "spawnParticles": true
  }
}
```

1. **actionLangCode** (`string`) interaction help when holding the itemstack.
2. **consumeLiters** (`float`) consume the amount of liquid
3. **ingredientQuantity** (`int`) defines how much of ingredient to consume.
4. **liquidStack** (`ItemStack`) liquid itemstack (to set quantity use **consumeLiters** instead).
5. **outputStack** (`ItemStack`) output itemstack.
6. **sound** (`asset location`) the sound played when crafting.
7. **spawnParticles** (`bool`) spawn particles based on the itemstack.

Currently limited to one liquidStack per item.

---

## `IC_UseToolThenRemoveBlock` block behavior

Use A tool on this block to output B itemstack.

Example of implementation:

```json
{
  "name": "IC_UseToolThenRemoveBlock",
  "properties": {
    "actionLangCode": "immersivecrafting:blockhelp-cutintoslices",
    "outputStack": { "type": "item", "code": "fruit-pineapple", "quantity": 12 },
    "spawnParticles": true,
    "toolDurabilityCost": 1,
    "toolTypes": ["Knife", "Sword"]
  }
}
```

1. **actionLangCode** (`string`) interaction help when looking at the block.
2. **outputStack** (`ItemStack`) output itemstack.
3. **sound** (`asset location`) the sound played when crafting.
4. **spawnParticles** (`bool`) spawn particles based on the block
5. **toolDurabilityCost** (`int`) consume tool durability.
6. **tooltypes** (`string[]`) list of allowed tool types.

Currently limited to one list of tool types per block.

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
