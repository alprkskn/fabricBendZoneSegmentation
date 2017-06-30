# fabricBendZoneSegmentation

Segmentation and mesh creation on a voxel based world. This project is based on [Fabric's](http://torrenglabs.com/fabric/) world structure. And aims to improve the performance by reducing the amount of GameObjects needed.

Segmentation is based on "bending points" which are special voxels within the world space.

Bending points divide the world into segments, which always move together no matter how many bending operation is carried on. This implementation finds those region bounds and create meshes out of the voxels within those bounds.
