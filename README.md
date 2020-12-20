# Marching-Cube
[SIGGRAPH 1987 - Marching Cube] Implemented by unity and add some game-like features

## Showcases
### What is marching cube?
Marching cube algorithm is a long-established algorithm introduced in SIGGRAPH in 1987. Due to its simple implementation and easy algorithm, it is popularly used to create meshes based on point crowds in games. In the Marching cube algorithm, if a vertex is marked as an selected, the corresponding edge is added accordingly. By storing 256 combinations that can be made with 8 vertices as an array (in this project, I stored data as static int array), when the weight made by 3D perlin noise reaches a certain level or higher, the corresponding edge is created. Also, by interpolating the position of the edge vertices based on each weight, the mesh was created more naturally. In the example below, the vertices with black vertices are marked, and corresponding edges and meshes are created accordingly.
| Preview of Marcing cube | Combination of vertices | Result of Random Terrain |
|---|---|---|
|![](http://emal.iptime.org/nextcloud/index.php/s/sQg7yJ4A55Qpkco/preview) | ![](https://www.researchgate.net/profile/Zhongjie_Long/publication/282209849/figure/fig2/AS:362916613246979@1463537471898/Type-of-surface-combinations-for-the-marching-cube-algorithm-The-black-circles-means.png)|![](http://emal.iptime.org/nextcloud/index.php/s/3i99KEAnMdpMjsZ/preview)|
### Before Implement Texture and threading...
![](http://emal.iptime.org/nextcloud/index.php/s/N66weAwSTfw3n8f/preview)
### After Implement Texture and thrading!!
![](http://emal.iptime.org/nextcloud/index.php/s/mq2oaSmQcpspM77/preview)

## To Do
- [x] Threading - Need to modify little
- [x] Texturing - Is there any way to simplify textuing processes?
- [x] Fix Noise (Symmetric Issue)
- [ ] Improve Noise quality
- [ ] Biome
- [ ] Add game like features
- [ ] Memory and resource management
