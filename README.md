# Description & Information

### Simple station to repair your shit. All at once or one at a time. Make it free, or cost {x} amount of {x} item. Just interact with this shit.


`Version checks with itself. If installed on the server, it will kick clients who do not have it installed.`

`This mod uses ServerSync, if installed on the server and all clients, it will sync all configs to client`

`This mod uses a file watcher. If the configuration file is not changed with BepInEx Configuration manager, but changed in the file directly on the server, upon file save, it will sync the changes to all clients.`



## Special Mentions
MadBuffoon#0001 - For the idea and the commission to make the mod.

## Configuration Options

`1 - General`

Lock Configuration [Synced with Server]
* If on, the configuration is locked and can be changed by server admins only.
    * Default Value: On

`2 - Repair Station`
Prevent Crafting Station Repair [Synced with Server]
* If on, Players will not be able to repair items at crafting stations. They must use the Repair Station.
  * Default Value: Off

`2 - Repair Station Cost`

Repair All Items [Synced with Server]
* If set to true, the RepairItems() method will be called in a loop until all repairable items are repaired. If set to false, the RepairItems() method will be called once.
    * Default Value: Off

Use Item Multiplier [Synced with Server]
* If set to true, the Cost Item Amount times the amount of items needing repair will be used to calculate the cost of repairing an item. If set to false, the Cost Item Amount will be used to calculate the cost of repairing an item.
    * Default Value: Off

Should Cost? [Synced with Server]
* Should using the repair station cost the player something from their inventory?
    * Default Value: Off

Cost Item [Synced with Server]
* Item needed to use the Repair Station. Limit is 1 item: Goes by prefab name and must be a valid item the player can hold. List of vanilla items here: https://valheim-modding.github.io/Jotunn/data/objects/item-list.html
    * Default Value: Coins

Cost Item Amount [Synced with Server]
* Amount of the item needed to repair all items in the inventory.
    * Default Value: 7

`piece_repairstation`

Build Table Category [Synced with Server]
* Build Category where Repair Station is available.
    * Default Value: Misc

Custom Build Category [Synced with Server]
*
    * Default Value:

Crafting Station [Synced with Server]
* Crafting station where Repair Station is available.
    * Default Value: Forge

Custom Crafting Station [Synced with Server]
*
    * Default Value:

Crafting Costs [Synced with Server]
* Item costs to craft Repair Station (Item Name:Amount:Recoverable)
    * Default Value: Iron:30:True,Wood:10:True,SurtlingCore:3:True


`Feel free to reach out to me on discord if you need manual download assistance.`


# Author Information

### Azumatt

`DISCORD:` Azumatt#2625

`STEAM:` https://steamcommunity.com/id/azumatt/

For Questions or Comments, find me in the Odin Plus Team Discord or in mine:

[![https://i.imgur.com/XXP6HCU.png](https://i.imgur.com/XXP6HCU.png)](https://discord.gg/Pb6bVMnFb2)
<a href="https://discord.gg/pdHgy6Bsng"><img src="https://i.imgur.com/Xlcbmm9.png" href="https://discord.gg/pdHgy6Bsng" width="175" height="175"></a>