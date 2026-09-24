https://github.com/EreliaStudio/Sparkle/issues/14
Status : Open

# Edition

- [core/src/voxel/shape.cpp:73] Replace the direct `spk::Vector3::fromJSON(reader.value())` workaround with the normal Sparkle JSON constructor once its `noexcept` contract is fixed.
