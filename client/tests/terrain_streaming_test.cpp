#include "erelia/client/client_configuration.hpp"
#include "erelia/client/terrain_streaming_behaviour.hpp"
#include "erelia/core/chunk_builder.hpp"
#include <chrono>
#include <diagnostics/logger.hpp>
#include <fstream>
#include <gtest/gtest.h>
#include <set>
#include <thread>
#include <type/uuid.hpp>
namespace
{
	template <typename TKey, typename TElement>
	struct Acquisition
	{
		std::unordered_map<TKey, std::shared_ptr<spk::Task<TElement>>> tasks;
		std::vector<TKey> requested;
	};
	template <typename TKey, typename TElement>
	class Controlled final : public Collection<TKey, TElement>::Provider
	{
		std::shared_ptr<Acquisition<TKey, TElement>> _acquisition;

	protected:
		typename spk::Task<TElement>::Answer _acquire(const TKey &key) override
		{
			auto task = std::make_shared<spk::Task<TElement>>();
			_acquisition->tasks.insert_or_assign(key, task);
			_acquisition->requested.push_back(key);
			return task->answer();
		}

	public:
		explicit Controlled(decltype(_acquisition) acquisition) :
			_acquisition(std::move(acquisition))
		{
		}
	};
	class Streaming : public testing::Test
	{
	protected:
		std::shared_ptr<Acquisition<Column::Coordinate, Column>> columnWork = std::make_shared<Acquisition<Column::Coordinate, Column>>();
		std::shared_ptr<Acquisition<Chunk::Coordinate, Chunk>> chunkWork = std::make_shared<Acquisition<Chunk::Coordinate, Chunk>>();
		TerrainCollections::Columns columns{Controlled<Column::Coordinate, Column>(columnWork)};
		TerrainCollections::Chunks chunks{Controlled<Chunk::Coordinate, Chunk>(chunkWork)};
		spk::Entity3D owner{"Player"};
		TerrainStreamingBehaviour *behaviour = nullptr;
		void start(spk::Vector3 position = {})
		{
			owner.transform().place(position);
			behaviour = &owner.addBehaviour<TerrainStreamingBehaviour>(columns, chunks, TerrainStreamingBehaviour::Ranges{1, 2});
		}
	};
}
TEST_F(Streaming, InitialCurrentTransformAndExactInclusiveSquare)
{
	start({32, 0, -16});
	EXPECT_EQ(behaviour->center(), (Chunk::Coordinate{2, 0, -1}));
	std::set<Column::Coordinate> expected;
	for (int x = 1; x <= 3; ++x)
	{
		for (int z = -2; z <= 0; ++z)
		{
			expected.insert({x, z});
		}
	}
	EXPECT_EQ((std::set<Column::Coordinate>{columnWork->requested.begin(), columnWork->requested.end()}), expected);
	EXPECT_EQ(chunkWork->requested.size(), 0u);
}
TEST_F(Streaming, SameChunkNoOpPositiveNegativeAndVerticalCrossingsReusePending)
{
	start();
	owner.transform().place({15.99f, 15.99f, 15.99f});
	EXPECT_EQ(columnWork->requested.size(), 9u);
	owner.transform().place({16, 0, 0});
	EXPECT_EQ(behaviour->center(), (Chunk::Coordinate{1, 0, 0}));
	EXPECT_EQ(columnWork->requested.size(), 12u);
	owner.transform().place({-0.01f, 0, -0.01f});
	EXPECT_EQ(behaviour->center(), (Chunk::Coordinate{-1, 0, -1}));
	const auto calls = columnWork->requested.size();
	owner.transform().place({-0.01f, -0.01f, -0.01f});
	EXPECT_EQ(behaviour->center(), (Chunk::Coordinate{-1, -1, -1}));
	EXPECT_EQ(columnWork->requested.size(), calls);
}
TEST_F(Streaming, SuccessfulColumnRequestsFullSparseChunksAndLogsCompletion)
{
	start();
	columnWork->tasks.at({0, 0})->validate(Column{{{0, 0, 0}, {0, 2, 0}, {0, 4, 0}}});
	behaviour->dispatch();
	EXPECT_EQ(chunkWork->requested, (std::vector<Chunk::Coordinate>{{0, 0, 0}, {0, 2, 0}, {0, 4, 0}}));
	std::size_t logs = 0;
	auto contract = spk::logger.subscribeToEntry([&](const spk::Logger::Level &level, const std::string &message) {
		if (level == spk::Logger::Level::UserValueB && message.find("Chunk acquired") != std::string::npos)
		{
			++logs;
		}
	});
	chunkWork->tasks.at({0, 2, 0})->validate(Chunk{});
	behaviour->dispatch();
	EXPECT_EQ(logs, 1u);
}
TEST_F(Streaming, FailedColumnDoesNotLaunchChunks)
{
	start();
	columnWork->tasks.at({0, 0})->fail(std::make_exception_ptr(spk::Exception("Failed")));
	behaviour->dispatch();
	EXPECT_EQ(chunkWork->requested.size(), 0u);
}
TEST_F(Streaming, UnloadsColumnsChunksAndLatePendingCannotRepublish)
{
	start();
	auto oldColumn = columnWork->tasks.at({0, 0});
	oldColumn->validate(Column{{{0, 0, 0}, {0, 1, 0}}});
	behaviour->dispatch();
	chunks.insert({1, 0, 0}, Chunk{});
	auto oldChunk = chunkWork->tasks.at({0, 0, 0});
	owner.transform().place({64, 0, 0});
	EXPECT_EQ(columns.state({0, 0}), TerrainCollections::Columns::State::Absent);
	EXPECT_EQ(chunks.state({0, 0, 0}), TerrainCollections::Chunks::State::Absent);
	EXPECT_EQ(chunks.state({1, 0, 0}), TerrainCollections::Chunks::State::Absent);
	oldChunk->validate(Chunk{});
	behaviour->dispatch();
	EXPECT_EQ(chunks.state({0, 0, 0}), TerrainCollections::Chunks::State::Absent);
}
TEST_F(Streaming, RetainsUnloadBoundaryAndRejectsOutdatedColumnDemand)
{
	start();
	auto oldColumn = columnWork->tasks.at({-1, 0});
	chunks.insert({0, 1, 0}, Chunk{});
	owner.transform().place({32, 0, 0});
	EXPECT_EQ(chunks.state({0, 1, 0}), TerrainCollections::Chunks::State::Available);
	oldColumn->validate(Column{{{-1, 0, 0}}});
	behaviour->dispatch();
	EXPECT_EQ(chunkWork->requested.size(), 0u);
}
TEST_F(Streaming, AvailableColumnAndChunkAreReused)
{
	columns.insert({0, 0}, Column{{{0, 0, 0}}});
	chunks.insert({0, 0, 0}, Chunk{});
	start();
	EXPECT_EQ(columnWork->requested.size(), 8u);
	EXPECT_EQ(chunkWork->requested.size(), 0u);
	owner.transform().place({0, 16, 0});
	EXPECT_EQ(columnWork->requested.size(), 8u);
	EXPECT_EQ(chunkWork->requested.size(), 0u);
}
TEST(TerrainStreamingRanges, ValidatesBothRangesAndConfiguration)
{
	for (auto ranges : {TerrainStreamingBehaviour::Ranges{0, 1}, {-1, 2}, {1, 0}, {2, 1}})
	{
		EXPECT_THROW(ranges.validate(), spk::Exception);
	}
	EXPECT_NO_THROW((TerrainStreamingBehaviour::Ranges{1, 1}.validate()));
	const auto path = std::filesystem::temp_directory_path() / (spk::UUID::generate().toString() + ".json");
	for (auto values : {std::pair{1, 2}, {0, 2}, {2, 1}, {-1, 2}})
	{
		{
			std::ofstream file(path);
			file << "{\"server config\":{\"address\":\"127.0.0.1\",\"port\":1,\"retryDelayMs\":1},\"terrain config\":{\"viewRange\":" << values.first << ",\"unloadRange\":" << values.second << "}}";
		}
		if (values.first == 1)
		{
			const auto configuration = ClientConfiguration::load(path);
			EXPECT_EQ(configuration.terrain.viewRange, 1);
			EXPECT_EQ(configuration.terrain.unloadRange, 2);
		}
		else
		{
			EXPECT_THROW((void)ClientConfiguration::load(path), spk::Exception);
		}
	}
	std::filesystem::remove(path);
}

TEST_F(Streaming, RetainsCompletionSubscriptionUntilMailboxPublication)
{
	auto pending = columns.request(Column::Coordinate{0, 0});
	std::atomic_bool entered{false}, release{false}, dispatched{false};
	auto observer = pending.subscribeToCompletion([&] {
		entered.store(true);
		entered.notify_all();
		release.wait(false);
	});
	start();
	std::jthread completion([&] {
		columnWork->tasks.at({0, 0})->validate(Column{{{0, 0, 0}}});
	});
	entered.wait(false);
	// Task status is terminal, but our streaming subscriber has not published its event yet.
	std::jthread dispatch([&] {
		behaviour->dispatch();
		dispatched.store(true);
	});
	const auto deadline = std::chrono::steady_clock::now() + std::chrono::seconds(1);
	while (dispatched.load() == false && std::chrono::steady_clock::now() < deadline)
	{
		std::this_thread::yield();
	}
	const bool returnedBeforePublication = dispatched.load();
	release.store(true);
	release.notify_all();
	completion.join();
	dispatch.join();
	EXPECT_EQ(returnedBeforePublication, true);
	behaviour->dispatch();
	EXPECT_EQ(chunkWork->requested, (std::vector<Chunk::Coordinate>{{0, 0, 0}}));
}
