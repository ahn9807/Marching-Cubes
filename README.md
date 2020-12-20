# Marching-Cube
[SIGGRAPH 1987 - Marching Cube] Implemented by unity and add some game-like features with threading and texture. Also, each information about biome is saved as unity asset. You can create Texture2DArray with biome asset easily with Unity wizard. 

## What is marching cube?
Marching cube algorithm is a long-established algorithm introduced in SIGGRAPH in 1987. Due to its simple implementation and easy algorithm, it is popularly used to create meshes based on point crowds in games. In the Marching cube algorithm, if a vertex is marked as an selected, the corresponding edge is added accordingly. By storing 256 combinations that can be made with 8 vertices as an array (in this project, I stored data as static int array), when the weight made by 3D perlin noise reaches a certain level or higher, the corresponding edge is created. Also, by interpolating the position of the edge vertices based on each weight, the mesh was created more naturally. In the example below, the vertices with black vertices are marked, and corresponding edges and meshes are created accordingly.
| Preview of Marcing cube | Combination of vertices | Result of Random Terrain |
|---|---|---|
|![](http://emal.iptime.org/nextcloud/index.php/s/sQg7yJ4A55Qpkco/preview) | ![](https://www.researchgate.net/profile/Zhongjie_Long/publication/282209849/figure/fig2/AS:362916613246979@1463537471898/Type-of-surface-combinations-for-the-marching-cube-algorithm-The-black-circles-means.png)|![](http://emal.iptime.org/nextcloud/index.php/s/3i99KEAnMdpMjsZ/preview)|

|Before texture | After texture|
|:---:|:---:|
|![](http://emal.iptime.org/nextcloud/index.php/s/N66weAwSTfw3n8f/preview)|![](http://emal.iptime.org/nextcloud/index.php/s/mq2oaSmQcpspM77/preview)|

## Showcases
![](http://emal.iptime.org/nextcloud/index.php/s/mq2oaSmQcpspM77/preview)
![](http://emal.iptime.org/nextcloud/index.php/s/pKXmz35J2DJknM7/preview)
![](http://emal.iptime.org/nextcloud/index.php/s/PDHgtx8mmnm3Bwf/preview)
![](http://emal.iptime.org/nextcloud/index.php/s/5dAksJ2CzmbpR2M/preview)
## To Do
- [x] Threading - Need to modify little.
- [x] Texturing - Is there any way to simplify textuing processes?
- [x] Fix Noise (Symmetric Issue)
- [ ] Improve Noise quality
- [ ] Biome - Height map texture is boring.
- [ ] Memory and resource management - Currently all the chunks are loaded as gameobject. 

## References
1. https://www.youtube.com/watch?v=M3iI2l0ltbE&t=4s
2. https://catlikecoding.com/unity/tutorials/hex-map
3. http://paulbourke.net/geometry/polygonise/
