https://github.com/EreliaStudio/Sparkle/issues/15
Status : Open

# Edition

- [core/src/voxel/shape.cpp:162] Replace direct access to `reader.value().at("vertices")` with the Sparkle Reader array-child API.
- [core/src/voxel/shape.cpp:163-166] Remove the local array-type validation if the Sparkle Reader API performs it.
- [core/src/voxel/shape.cpp:168] Remove direct `asArray()` access.
- [core/src/voxel/shape.cpp:169-177] Replace manual indexed iteration and manual path-aware `spk::JSON::Reader` construction with the Sparkle Reader array-child API.
