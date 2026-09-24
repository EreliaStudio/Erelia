#include "erelia/core/voxel/catalog.hpp"

#include "voxel_resource_test_utils.hpp"

#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <gtest/gtest.h>

#include <array>
#include <filesystem>
#include <utility>
#include <fstream>
#include <sstream>
#include <string>
#include <type_traits>

static_assert(std::is_copy_constructible_v<Voxel::Definition>);
static_assert(std::is_move_constructible_v<Voxel::Definition>);
static_assert(!std::is_copy_assignable_v<Voxel::Definition>);
static_assert(!std::is_move_assignable_v<Voxel::Definition>);
static_assert(std::is_same_v<Voxel::Material::SlotID, std::string>);
static_assert(std::is_same_v<Voxel::Definition::SlotBindings::key_type, Voxel::Material::SlotID>);
static_assert(std::is_same_v<decltype(Voxel::Shape::Polygon::slot), Voxel::Material::SlotID>);
static_assert(std::is_convertible_v<Voxel::Shape::Catalog *, spk::JSON::Catalog<Voxel::Shape> *>);
static_assert(std::is_convertible_v<Voxel::Definition::Catalog *, spk::JSON::Catalog<Voxel::Definition> *>);

namespace
{
	const std::string CompleteCubeSlots = R"({"top":"grass","side":"dirt","bottom":"stone"})";

	[[nodiscard]] std::string readText(const std::filesystem::path &path)
	{
		std::ifstream stream(path, std::ios::binary);
		std::ostringstream content;
		content << stream.rdbuf();
		return content.str();
	}
}

TEST(VoxelCatalog, ApprovedResourcesExerciseAggregateAndDirectRootForms)
{
	Voxel::Catalog catalog;
	catalog.loadShape(voxel_test::shapeResourcePath());
	catalog.loadShape(voxel_test::slopeShapeResourcePath());
	catalog.loadShape(voxel_test::stairShapeResourcePath());
	catalog.loadDefinition(voxel_test::definitionResourcePath());
	catalog.loadDefinition(voxel_test::slopeDefinitionResourcePath());
	catalog.loadDefinition(voxel_test::stairDefinitionResourcePath());

	ASSERT_TRUE(catalog.shapes().contains("cube"));
	ASSERT_TRUE(catalog.shapes().contains("slope"));
	ASSERT_TRUE(catalog.shapes().contains("stair"));
	ASSERT_TRUE(catalog.shapes().contains("slab"));

	const std::array expected = {
		std::pair<Voxel::Definition::ID, std::string>{1u, "cube"},
		std::pair<Voxel::Definition::ID, std::string>{2u, "slope"},
		std::pair<Voxel::Definition::ID, std::string>{3u, "stair"},
		std::pair<Voxel::Definition::ID, std::string>{4u, "slab"}};

	for (const auto &[definitionID, shapeID] : expected)
	{
		const auto &definition = catalog.definitions().at(definitionID);
		EXPECT_EQ(&definition.shape(), &catalog.shapes().at(shapeID));
		EXPECT_EQ(definition.slots().at("side"), shapeID + "-side");
		EXPECT_EQ(definition.slots().at("top"), shapeID + "-top");
		EXPECT_EQ(definition.slots().at("bottom"), shapeID + "-bottom");
	}
}

TEST(VoxelCatalog, MaterialInvalidIdIsReserved)
{
	EXPECT_EQ(Voxel::Material::InvalidID, "InvalidID");
}

TEST(VoxelCatalog, AirExistsImmediatelyAndLookupApisHaveExactMissingBehavior)
{
	Voxel::Catalog catalog;

	ASSERT_TRUE(catalog.definitions().contains(0u));
	ASSERT_NE(catalog.definitions().tryGet(0u), nullptr);
	EXPECT_EQ(&catalog.definitions().at(0u), &catalog.definitions()[0u]);
	EXPECT_TRUE(catalog.definitions().at(0u).shape().polygons().empty());
	EXPECT_TRUE(catalog.definitions().at(0u).slots().empty());

	EXPECT_FALSE(catalog.shapes().contains("missing"));
	EXPECT_EQ(catalog.shapes().tryGet("missing"), nullptr);
	EXPECT_THROW((void)catalog.shapes().at("missing"), spk::Exception);
	EXPECT_THROW((void)catalog.shapes()["missing"], spk::Exception);

	EXPECT_FALSE(catalog.definitions().contains(99u));
	EXPECT_EQ(catalog.definitions().tryGet(99u), nullptr);
	EXPECT_THROW((void)catalog.definitions().at(99u), spk::Exception);
	EXPECT_THROW((void)catalog.definitions()[99u], spk::Exception);
}

TEST(VoxelCatalog, LoadsDefinitionsResolvesShapeAndPreservesBindings)
{
	const voxel_test::TemporaryJsonFile definitions(voxel_test::definitionFile(1u, "cube", CompleteCubeSlots));
	Voxel::Catalog catalog;
	catalog.load(voxel_test::shapeResourcePath(), definitions.path());

	ASSERT_TRUE(catalog.shapes().contains("cube"));
	EXPECT_EQ(catalog.shapes().tryGet("cube"), &catalog.shapes().at("cube"));
	EXPECT_EQ(&catalog.shapes()["cube"], &catalog.shapes().at("cube"));

	const auto &definition = catalog.definitions().at(1u);
	EXPECT_EQ(&definition.shape(), &catalog.shapes().at("cube"));
	EXPECT_EQ(definition.slots().at("top"), "grass");
	EXPECT_EQ(definition.slots().at("side"), "dirt");
	EXPECT_EQ(definition.slots().at("bottom"), "stone");
	EXPECT_TRUE(catalog.definitions().contains(1u));
	EXPECT_EQ(catalog.definitions().tryGet(1u), &definition);
	EXPECT_EQ(&catalog.definitions()[1u], &definition);
}

TEST(VoxelCatalog, DefinitionShapeReferenceSurvivesShapeCatalogGrowth)
{
	const voxel_test::TemporaryJsonFile shape(
		voxel_test::shapeFile("anchor", R"({"slot":"face","vertices":[[0,0,0],[0,1,0],[1,1,0],[1,0,0]]})"));
	const voxel_test::TemporaryJsonFile definition(
		voxel_test::definitionFile(1u, "anchor", R"({"face":"stone"})"));

	Voxel::Catalog catalog;
	catalog.load(shape.path(), definition.path());

	const Voxel::Definition &loadedDefinition = catalog.definitions().at(1u);
	const Voxel::Shape *resolvedShape = &loadedDefinition.shape();

	std::ostringstream additionalShapes;
	additionalShapes << R"({"elements":[)";
	for (std::size_t index = 0; index < 512u; ++index)
	{
		if (index != 0u)
		{
			additionalShapes << ',';
		}
		additionalShapes
			<< R"({"id":"extra-)" << index
			<< R"(","data":{"polygons":[{"slot":"face","vertices":[[0,0,0],[0,1,0],[1,1,0],[1,0,0]]}]}})";
	}
	additionalShapes << "]}";

	const voxel_test::TemporaryJsonFile additional(additionalShapes.str(), "many-shapes");
	catalog.loadShape(additional.path());

	EXPECT_EQ(&loadedDefinition.shape(), resolvedShape);
	EXPECT_EQ(&catalog.shapes().at("anchor"), resolvedShape);
	EXPECT_EQ(loadedDefinition.shape().polygons().size(), 1u);
	EXPECT_EQ(loadedDefinition.slots().at("face"), "stone");
}

TEST(VoxelCatalog, MissingShapeSlotLogsWarningAndBindsInvalidMaterial)
{
	const voxel_test::TemporaryJsonFile definitions(
		voxel_test::definitionFile(1u, "cube", R"({"side":"dirt","bottom":"stone"})"));
	const std::filesystem::path logPath =
		std::filesystem::temp_directory_path() /
		("erelia-warning-" + spk::UUID::generate().toString() + ".log");

	Voxel::Catalog catalog;
	catalog.loadShape(voxel_test::shapeResourcePath());
	spk::Logger::instance().muteConsole();
	{
		auto output = spk::Logger::instance().addOutput(logPath, spk::Logger::Level::Warning);
		catalog.loadDefinition(definitions.path());
	}
	spk::Logger::instance().unmuteConsole();

	EXPECT_EQ(catalog.definitions().at(1u).slots().at("top"), Voxel::Material::InvalidID);
	const std::string log = readText(logPath);
	EXPECT_NE(log.find("[Warning]"), std::string::npos);
	EXPECT_NE(log.find("missing slot 'top'"), std::string::npos);
	EXPECT_NE(log.find("Material::InvalidID"), std::string::npos);

	std::error_code error;
	std::filesystem::remove(logPath, error);
}

TEST(VoxelCatalog, RepeatedShapeSlotNeedsOnlyOneDefinitionBinding)
{
	const voxel_test::TemporaryJsonFile shapeFile(voxel_test::shapeFile("double-face", R"({"slot":"same","vertices":[[0,0,0],[0,1,0],[1,1,0],[1,0,0]]},
		   {"slot":"same","vertices":[[1,0,1],[1,1,1],[0,1,1],[0,0,1]]})"));
	const voxel_test::TemporaryJsonFile definitions(voxel_test::definitionFile(1u, "double-face", R"({"same":"stone"})"));

	Voxel::Catalog catalog;
	catalog.load(shapeFile.path(), definitions.path());
	EXPECT_EQ(catalog.definitions().at(1u).slots().size(), 1u);
	EXPECT_EQ(catalog.definitions().at(1u).slots().at("same"), "stone");
}

TEST(VoxelCatalog, RejectsInvalidDefinitions)
{
	const std::string fixtures[] = {
		voxel_test::definitionFile(1u, "cube", R"({"top":"grass","side":"dirt","bottom":"stone","extra":"bad"})"),
		voxel_test::definitionFile(1u, "unknown", R"({})"),
		voxel_test::definitionFile(0u, "cube", CompleteCubeSlots),
		R"({"elements":[{"id":536870912,"data":{"shape":"cube","slots":{"top":"grass","side":"dirt","bottom":"stone"}}}]})",
		R"({"elements":[{"id":1,"data":{"shape":"cube","slots":[]}}]})",
		R"({"elements":[{"id":1,"data":{"shape":"cube","slots":{"":"stone"}}}]})",
		R"({"elements":[{"id":1,"data":{"shape":"cube","slots":{"top":4,"side":"dirt","bottom":"stone"}}}]})",
		R"({"elements":[{"id":1,"data":{"shape":"cube","slots":{"top":"grass","side":"dirt","bottom":"stone"},"unexpected":true}}]})"};

	for (const std::string &fixture : fixtures)
	{
		const voxel_test::TemporaryJsonFile definitions(fixture, "invalid-definition");
		Voxel::Catalog catalog;
		catalog.loadShape(voxel_test::shapeResourcePath());
		EXPECT_THROW(catalog.loadDefinition(definitions.path()), spk::Exception);
		EXPECT_FALSE(catalog.definitions().contains(1u));
	}
}

TEST(VoxelCatalog, AcceptsMaximumPackedDefinitionId)
{
	const voxel_test::TemporaryJsonFile definitions(
		R"({"elements":[{"id":536870911,"data":{"shape":"cube","slots":{"top":"grass","side":"dirt","bottom":"stone"}}}]})");
	Voxel::Catalog catalog;
	catalog.loadShape(voxel_test::shapeResourcePath());
	catalog.loadDefinition(definitions.path());
	EXPECT_TRUE(catalog.definitions().contains(0x1FFFFFFFu));
}

TEST(VoxelCatalog, RepeatedLoadsAppendUniqueShapeAndDefinitionIds)
{
	const voxel_test::TemporaryJsonFile shapeOne(voxel_test::shapeFile("one", R"({"slot":"face","vertices":[[0,0,0],[0,1,0],[1,1,0],[1,0,0]]})"));
	const voxel_test::TemporaryJsonFile shapeTwo(voxel_test::shapeFile("two", R"({"slot":"face","vertices":[[1,0,1],[1,1,1],[0,1,1],[0,0,1]]})"));
	const voxel_test::TemporaryJsonFile definitionOne(voxel_test::definitionFile(1u, "one", R"({"face":"stone"})"));
	const voxel_test::TemporaryJsonFile definitionTwo(voxel_test::definitionFile(2u, "two", R"({"face":"grass"})"));

	Voxel::Catalog catalog;
	catalog.loadShape(shapeOne.path());
	catalog.loadShape(shapeTwo.path());
	catalog.loadDefinition(definitionOne.path());
	catalog.loadDefinition(definitionTwo.path());

	EXPECT_TRUE(catalog.shapes().contains("one"));
	EXPECT_TRUE(catalog.shapes().contains("two"));
	EXPECT_TRUE(catalog.definitions().contains(1u));
	EXPECT_TRUE(catalog.definitions().contains(2u));
}

TEST(VoxelCatalog, RejectsDuplicateIdsInCurrentAndPreviousLoads)
{
	const std::string validData = R"({"polygons":[{"slot":"face","vertices":[[0,0,0],[0,1,0],[1,1,0],[1,0,0]]}]})";
	const voxel_test::TemporaryJsonFile sameShapeFile(
		R"({"elements":[{"id":"dup","data":)" + validData + R"(},{"id":"dup","data":)" + validData + R"(}]})");
	Voxel::Catalog catalog;
	EXPECT_THROW(catalog.loadShape(sameShapeFile.path()), spk::Exception);
	EXPECT_TRUE(catalog.shapes().contains("dup"));

	const voxel_test::TemporaryJsonFile previousShape(voxel_test::shapeFile("existing", R"({"slot":"face","vertices":[[0,0,0],[0,1,0],[1,1,0],[1,0,0]]})"));
	catalog.loadShape(previousShape.path());
	EXPECT_THROW(catalog.loadShape(previousShape.path()), spk::Exception);

	const voxel_test::TemporaryJsonFile definitionDuplicates(
		R"({"elements":[
			{"id":1,"data":{"shape":"existing","slots":{"face":"stone"}}},
			{"id":1,"data":{"shape":"existing","slots":{"face":"grass"}}}
		]})");
	EXPECT_THROW(catalog.loadDefinition(definitionDuplicates.path()), spk::Exception);
	EXPECT_TRUE(catalog.definitions().contains(1u));

	const voxel_test::TemporaryJsonFile previousDefinition(voxel_test::definitionFile(1u, "existing", R"({"face":"stone"})"));
	EXPECT_THROW(catalog.loadDefinition(previousDefinition.path()), spk::Exception);
}

TEST(VoxelCatalog, FailedElementPreservesEarlierInsertAndSkipsLaterElements)
{
	const voxel_test::TemporaryJsonFile shapes(
		R"({"elements":[
			{"id":"first","data":{"polygons":[{"slot":"face","vertices":[[0,0,0],[0,1,0],[1,1,0],[1,0,0]]}]}},
			{"id":"bad","data":{"polygons":[{"slot":"face","vertices":[[0,0,0],[0,2,0],[1,0,0]]}]}},
			{"id":"third","data":{"polygons":[{"slot":"face","vertices":[[1,0,1],[1,1,1],[0,1,1],[0,0,1]]}]}}
		]})");
	Voxel::Catalog catalog;
	EXPECT_THROW(catalog.loadShape(shapes.path()), spk::Exception);
	EXPECT_TRUE(catalog.shapes().contains("first"));
	EXPECT_FALSE(catalog.shapes().contains("bad"));
	EXPECT_FALSE(catalog.shapes().contains("third"));

	const voxel_test::TemporaryJsonFile definitions(
		R"({"elements":[
			{"id":1,"data":{"shape":"first","slots":{"face":"stone"}}},
			{"id":2,"data":{"shape":"missing","slots":{"face":"stone"}}},
			{"id":3,"data":{"shape":"first","slots":{"face":"grass"}}}
		]})");
	EXPECT_THROW(catalog.loadDefinition(definitions.path()), spk::Exception);
	EXPECT_TRUE(catalog.definitions().contains(1u));
	EXPECT_FALSE(catalog.definitions().contains(2u));
	EXPECT_FALSE(catalog.definitions().contains(3u));
}

TEST(VoxelCatalog, AggregateLoadDoesNotStartDefinitionsAfterShapeFailure)
{
	const voxel_test::TemporaryJsonFile shapes(
		R"({"elements":[
			{"id":"first","data":{"polygons":[{"slot":"face","vertices":[[0,0,0],[0,1,0],[1,1,0],[1,0,0]]}]}},
			{"id":"bad","data":{"polygons":[]}}
		]})");
	const voxel_test::TemporaryJsonFile definitions(voxel_test::definitionFile(7u, "first", R"({"face":"stone"})"));

	Voxel::Catalog catalog;
	EXPECT_THROW(catalog.load(shapes.path(), definitions.path()), spk::Exception);
	EXPECT_TRUE(catalog.shapes().contains("first"));
	EXPECT_FALSE(catalog.definitions().contains(7u));
}

TEST(VoxelCatalog, MalformedCatalogEnvelopeAndShapeSchemaThrowWithContext)
{
	const std::string fixtures[] = {
		"{",
		R"({})",
		R"({"elements":{}})",
		R"({"elements":[7]})",
		R"({"elements":[{"data":{}}]})",
		R"({"elements":[{"id":"shape"}]})",
		R"({"elements":[{"id":"","data":{"polygons":[]}}]})",
		R"({"elements":[{"id":"shape","data":{}}]})",
		R"({"elements":[{"id":"shape","data":{"unexpected":true,"polygons":[]}}]})",
		R"({"elements":[{"id":"shape","data":{"polygons":"bad"}}]})",
		R"({"elements":[{"id":"shape","data":{"polygons":[{"slot":"face"}]}}]})",
		R"({"elements":[{"id":"shape","data":{"polygons":[{"slot":"face","unexpected":true,"vertices":[]}]}}]})",
		R"({"elements":[{"id":"shape","data":{"polygons":[{"slot":"face","vertices":"bad"}]}}]})",
		R"({"elements":[{"id":"shape","data":{"polygons":[{"slot":"face","vertices":[[0,0],[0,1,0],[1,0,0]]}]}}]})",
		R"({"elements":[],"unexpected":true})"};

	for (const std::string &fixture : fixtures)
	{
		const voxel_test::TemporaryJsonFile file(fixture, "malformed-catalog");
		Voxel::Catalog catalog;
		try
		{
			catalog.loadShape(file.path());
			FAIL() << "Expected malformed fixture to throw";
		} catch (const spk::Exception &exception)
		{
			const std::string message = exception.what();
			EXPECT_NE(message.find(file.path().generic_string()), std::string::npos);
			EXPECT_NE(message.find('$'), std::string::npos);
		}
	}
}

TEST(VoxelCatalog, EquivalentResourcesProduceEquivalentSemanticData)
{
	const voxel_test::TemporaryJsonFile firstShape(voxel_test::shapeFile("shape", R"({"slot":"face","vertices":[[0,0,0],[0,1,0],[1,1,0],[1,0,0]]})"));
	const voxel_test::TemporaryJsonFile secondShape(voxel_test::shapeFile("shape", R"({"slot":"face","vertices":[[0,0,0],[0,1,0],[1,1,0],[1,0,0]]})"));
	const voxel_test::TemporaryJsonFile firstDefinition(voxel_test::definitionFile(1u, "shape", R"({"face":"stone"})"));
	const voxel_test::TemporaryJsonFile secondDefinition(voxel_test::definitionFile(1u, "shape", R"({"face":"stone"})"));

	Voxel::Catalog first;
	Voxel::Catalog second;
	first.load(firstShape.path(), firstDefinition.path());
	second.load(secondShape.path(), secondDefinition.path());

	EXPECT_EQ(first.shapes().at("shape").polygons().front().vertices, second.shapes().at("shape").polygons().front().vertices);
	EXPECT_EQ(first.shapes().at("shape").polygons().front().slot, second.shapes().at("shape").polygons().front().slot);
	EXPECT_EQ(first.definitions().at(1u).slots(), second.definitions().at(1u).slots());
}
