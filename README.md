# Unity-Game-Feature-Testing

As said in the description, this is basically my main environment for testing and implementing any kind of video game feature that I feel inspired to do. So far there is creation of mesh using a variant of marching cubes algorithm, map generation using layering of perlin noise, a simple first person character movement controller aswell as a inventory system that has stacking of items, varying bag sizes and rotation/size based management. Theres not much that connects them all and are essentially just their own features but each can be tested and played around with by hitting play and messing around with Script settings through Unity.

Map Generation / MapGenerator Game Object, Map Generator Script
Has a lot of testing variables that aren't used but everything from Noise Scale downwards in the editor can be changed in order to effect the outcome of the terrain. Terrain does not get updated in real time but requires to hit the Re-Generate button (Re-generates all visible chunks to fit new settings, can take a while) or by exiting and re-entering play-mode. There isnt much optimisation going on behind the scenes so generation of new chunks causes fairly significant lag spikes.

Player Movement / Player Game Object, Player Movement and Player Camera Script
Very simple system that was inspired by a prefab asset from the Unity Store that I used to use for testing map generation. Has most basic settings to do with movement that you can control from these two scripts in the editor. They are well labeled and all have impact in real time.

Inventory / Player->HUD Game Object, Inventory Manager Script
I almost set this up as an asset ready to be used in any other projects that I want to in the future but I applied a version of it to the player mentioned above in order to test various features. Bag containers can also be assigned to ingame objects (as shown by the Cube Gameobject which can be tested by disabling MapGenerator and enabling TempFloor and Cube) which act as floor loot which can be transferred to the characters inventory. Since none of the bags spawn with items, in the editor you can select a mode for the Bag which is labeled Editor Click Mode. 

Add will add the current Item to Spawn to the slot which is clicked.
Remove will remove the item clicked from the inventory.
Destroy is disabled but would remove the entire bag from the players inventory.
Move is the likely intended gameplay mode which allows to move and rotate items around between bags by clicking to pick up then clicking again to place. The Rotation key can be pressed to rotate items aswell.

System works for stacks of items aswell which can be seen if you Add an apple object to the bag (stack of 20). All these items are also scriptable objects meaning you can create a bag or an item and change its settings incredbily easily.

Default keys : WASD move
               SPACE jump
               TAB  open inventory
               R    rotate item held by mouse in inventory
