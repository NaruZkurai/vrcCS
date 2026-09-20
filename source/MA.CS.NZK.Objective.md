also NZK_NaNimationBoneHolder should be the object source and holder* and bone referencer 
its just so i the human can varify it works and makesit easier for me to test and do my own avatars right?
ANYWAYS

so your directive is now
code:
i have weights assigned in the blend file and blendshapes with scale to 0 as blendshape. name = bone
confirmed? sortof? but need image proof:
use the verts affected by that blendshape
via the blend shape's name and nanimate the appropriate bone using the existing nanimations.

that blend shape should in theory just replace the existing bone slot via the blender made nanimation bone

the NZK_NaNimationBoneHolder oblect functionality:
you have lists:
1 bones, 2  the avatar root object or prefab, source mesh and changed mesh.
it does:
takes the bones / objects to use (ue.object.name) to use as target blendshapes for the source mesh blendshapes 
(nolonger looks just for nanimate or nanimations)
it looks for blend shapes in ANY smr in the avatar AND collects all smrs on the avatar to check that have these vertex groups.in  addition adds to the temp list the smrs you explicitely give.
creates a child object for each smr, with a script containing.
it then stores arbitrarry data ideally in a format that wont escape yaml so maybe hex/bin/b64?? better compression == better == less escapelikelyhood

so like it contians blendshape, object, smr, bone pointers and the weights and whatever ur storing in memory rn so it doesnt corrupt and can be redirrived via a button that is redirrive scenne mesh for when it melts.
anyways you can just right click repair mesh. use the asset  path  for generated asssets stored in sealed zip.
when update when scene updated or saved and or source meshes get updated you check the diff of original object mesh stored in the generated assets file != then update prefab else dont update unless user updates
so ig you store a copy of each mesh as it was when you stored the edits in the prefab, DO NOT REMOVE ANY FUNCTIONALITY FROM ITS CURRENT  STATE
reuse as much as possible
create more sub functions on which methods you use
eg
from existing bone or new bone from blenddshape name
i need imperical proof it works as is rn thennn upload and do the rest of this task. give imperical proof it works after using the new implementation.
do not ask me what todo just do it

the current nemtest prefab has only 2 meshes jacket and stockinngs

current step of todo task currenlty has an issue where the jacket and stockings have the in memory corruptionn issue i wanted to fix where the sleaves are drooping to 0,0,0 like a few verts are nanimated but not all verts are nanimated. but in editor mode not play mode. this is the ma pitfall that needs to be fixed for the test before it can continue. if it looks like the curent sate theres corruption, if you discard all changes to mesh that should be the starting point unannimated looks like


the partial nanimation issue is caused by not re-scaling properly the influences to 1.  
solution:
 1: delete smr 4th bone weights and name.
 2: Rescale slots 0-2 to 1.
 3: rescale slots 0-2 to 0.99999 total weight* is this 32bit precision safe for now? if not we just round to the nearest (0.00000x && each value of each bone is stable and v>0 && total =1)  if value was v>0 altered vertecy weights must be ((av0>0||v=1 + av1>0||uav=0 + av2>0||uav=0 + av3>0||uav=0) == 1)  at all times. potentnial code oversight atm or logic trap with following exactly is the pose board where the whole object has 1 vertex group or vertexes with original vertexes that have lessthan 4 weights by default with bone slots with weight values of 0 by default. or are reassigned to hipbone with a remainder value or a value of 0 or 1 via unity engine

 4: set bone 4 to be the nanimation bone name of choice generated or selected in the way that doesnt break things (atm however it is ) of the vertex group.
 5: set nan weight to missing weight per nan weight vert the stable selected value.

 refreshing / recalculating using source meshes semi frequently as mentioned in previous coment
then re-applying the weight values to the nan bone of the object. 
 this is a normal thing modular avatar does on modern ddr5 systems when the ddr5 gets hot... faulty ecc at high temps
 this is not a we need to reinvent the wheel thing its a we need to optomise it issue.
 the whole object issuposed to be nnanimated and the blend shape is changing the size of the whole body.
 the dropiung and melting is a more than 5 weights originaly issue meaning the mesh does not have uniform weights 
 on each vert.
 or atleast in memory has bits flipped and doesnt free the verts to nanimate or doesnt recalculate 
 the nan weights frequently enogh to restore the meshes to their original state.
 also it looks like the mesh of the stockings rotated 90 deg forward which is a sign it was nearly uniformly assigned to hipbone OR was not saved correctly inn the original meshes orientation pre armature transform