#include "voxel/palette.hpp"

#include <gtest/gtest.h>

#include <exception.hpp>

TEST(PaletteTest, StoresTypedDataAndCopiesSharedGpuResource)
{
	const std::vector<voxel::Palette::Data> data{
		{{0.1f, 0.2f, 0.3f, 0.4f}},
		{{0.5f, 0.6f, 0.7f, 0.8f}}};
	voxel::Palette original;
	original.resize(data.size());
	for (std::size_t index = 0; index < data.size(); ++index)
		original.data(static_cast<voxel::Palette::ElementIndex>(index)) = data[index];
	original.validate();
	const voxel::Palette copy = original;

	EXPECT_EQ(original.data(0), data[0]);
	EXPECT_EQ(original.data(1), data[1]);
	EXPECT_EQ(&copy.data(0), &original.data(0));
	EXPECT_EQ(copy.resourceIdentifier(), original.resourceIdentifier());
	EXPECT_TRUE(copy.contains(0));
	EXPECT_TRUE(copy.contains(1));
	EXPECT_FALSE(copy.contains(2));
}

TEST(PaletteTest, MutableDataRequiresExplicitValidation)
{
	voxel::Palette palette;
	palette.resize(1);
	palette.data(0).color = {0.1f, 0.2f, 0.3f, 0.4f};
	const spk::Color edited{0.8f, 0.7f, 0.6f, 0.5f};

	palette.data(0).color = edited;
	palette.validate();

	EXPECT_EQ(palette.data(0).color, edited);
}

TEST(PaletteTest, IndependentlyConstructedPalettesOwnDifferentResources)
{
	voxel::Palette first;
	first.resize(1);
	first.data(0).color = {1.0f, 0.0f, 0.0f, 1.0f};
	first.validate();
	voxel::Palette second;
	second.resize(1);
	second.data(0).color = {0.0f, 0.0f, 1.0f, 1.0f};
	second.validate();

	EXPECT_NE(first.resourceIdentifier(), second.resourceIdentifier());
	EXPECT_NE(first.data(0), second.data(0));
}

TEST(PaletteTest, EmptyResourceHasNoValidElementIndex)
{
	const voxel::Palette palette;

	EXPECT_TRUE(palette.empty());
	EXPECT_EQ(palette.size(), 0);
	EXPECT_FALSE(palette.contains(0));
	EXPECT_THROW((void)palette.data(0), spk::Exception);
}
