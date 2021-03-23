# Enhanced-Toolkit
 Jades Enhanced Toolkit backup repo (This repository may not always be up to date or even functional)

Getting Started:
Getting started with the Enhanced Renderer component is pretty much a 3 step process!

1. Head to the Enhanced menu in the toolbar, select Utils and then Load Materials. (This process will be done automatically if you press play)
2. Select a gameobject which has a Mesh or Skinned Mesh Renderer component and add the Enhanced Renderer component!
3. If you wish to modify the object at runtime through code, you can uncheck the Runtime Static checkbox, otherwise you can leave that as is!

And thats it! you're now started up with the Enhanced Renderer, part of the Enhanced Toolkit! If you wish to modify textures, colours or exposed float and int values on the material in a renderer you can! itll handle most of the work for you and further documentation will be coming on the process. 

As a quick jump guide, get a reference to the Enhanced Renderer component and from there you can use the "Apply" functions (Excluding "Apply Block") to instantly make singular changes to the renderer! if you want to make a bunch of changes you can use the "Set" Functions and call "Apply Block" after to ensure your Renderer updates!

So how does it work? Pretty simple, the system uses a Scriptable object and a backend set of classes to find and cache references to all of your materials in the project! Then any Enhanced Renderers will automatically set themselves up at runtime! 
In static mode they use a copy of the asset materials, these are deemed "static" and should not be changed lest every Renderer which shares that material change aswell. 
In instanced mode (when the RuntimeStatic boolean is unchecked) they will generate their own copies of their base materials. These materials will be cached as by default Unity will not keep track of them, which means it will not destroy them even when the object containing them is destroyed, until the scene is unloaded. The way the instanced mode works is that when the Enhanced Renderer is destroyed so are the references to the materials it contains!

"But what if I want to have multiple renderers share a material and not others?" I hear you say, and to that I answer something I'm currently working on called Master Instances. Though admittedly this may take soem time to complete. And thats all! if you have any questions feel free to hit up my Discord: https://discord.gg/j87PvEFQGE
