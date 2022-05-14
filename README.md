# ImmersiveCrafting
---
### Behavior: `IC_UseOnLiquidContainer`
Currently it is limited to one liquidStack per item.

Example of implementation:
```json
{
  "name": "IC_UseOnLiquidContainer",
  "properties": {
    "outputStack": { "type": "item", "code": "dough-{type}", "quantity": 1 },
    "liquidStack": { "type": "item", "code": "waterportion" },
    "actionLangCode": "ic_heldhelp-makedough",
    "sound": "effect/water-fill",
    "consumeLiters": 1,
    "ingredientQuantity": 1,
    "spawnParticles": true
  }
}
```
1. **outputStack** (`ItemStack`) can be any item or block.
2. **liquidStack** (`ItemStack`) is a liquid code (liters/quantity/stacksize wlll be ignored, use consumeLiters instead).
3. **actionLangCode** (`string`) is a lang key for tooltip when holding the item.
4. **sound** (`asset location`) is a path to sound which is played upon crafting.
5. **consumeLiters** (`float`) defines how much of liquid to consume.
6. **ingredientQuantity** (`int`) defines how much of ingredient to consume.
7. **spawnParticles** (`bool`) if true, then will spawn particles under player based on the main ingredient
----
### Behavior `IC_SealCrock`
Add this to any item/block you want to use to seal placed crocks.
```json
{ "name": "IC_SealCrock" }
```