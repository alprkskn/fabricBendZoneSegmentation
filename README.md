# fabricBendZoneSegmentation

Segmentation and mesh creation on a voxel based world. This project is based on [Fabric's](http://torrenglabs.com/fabric/) world structure. And aims to improve the performance by reducing the amount of GameObjects needed.

Segmentation is based on "bending points" which are special voxels within the world space.

Bending points divide the world into segments, which always move together no matter how many bending operation is carried on. This implementation finds those region bounds and create meshes out of the voxels within those bounds.

Some screenshots from unity editor:

![alt text](https://www.dropbox.com/s/3bdf453dusde8c8/Capture4.PNG?dl=0 "screenshot")

![alt text](https://www.dropbox.com/s/t2qsts88pxh05w8/Capture5.PNG?dl=0 "screenshot")

![alt text](https://www.dropbox.com/s/8ridao464w0d5xl/Capture6.PNG?dl=0 "screenshot")

![alt text](https://www.dropbox.com/s/fxzp63zsxv685kj/Capture2.PNG?dl=0 "screenshot")
