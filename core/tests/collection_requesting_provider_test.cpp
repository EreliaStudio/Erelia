#include "erelia/core/collection_requesting_provider.hpp"
#include "erelia/core/collection_updater.hpp"
#include "erelia/core/networking/message_dispatcher.hpp"
#include "erelia/core/networking/terrain_protocol.hpp"
#include <barrier>
#include <gtest/gtest.h>
#include <thread>
namespace
{
	using Cache = Collection<Column::Coordinate, Column>;
	using P = Networking::ColumnProtocol;
	Column value(int x, int y = 0)
	{
		return Column{{{x, y, 0}}};
	}
}
TEST(RequestingProvider, SplitsOnlyNewKeysAndPreservesLogicalGroupOrder)
{
	spk::WorkerPool pool(1);
	std::vector<spk::Message> sent;
	Cache cache{Cache::RequestingProvider(pool, [&](const auto &message) {
		sent.push_back(message);
	})};
	cache.insert({-1, 0}, value(-1));
	auto existing = cache.request(Column::Coordinate{-2, 0});
	std::vector<Column::Coordinate> keys{{-1, 0}, {-2, 0}};
	for (std::size_t i = 0; i <= Column::MaximumElementsPerRequest; ++i)
	{
		keys.push_back({static_cast<int>(i), 0});
	}
	auto group = cache.request(keys);
	ASSERT_EQ(sent.size(), 3u);
	EXPECT_EQ(group.size(), keys.size());
	EXPECT_EQ(P::Request(sent[1]).keys().size(), Column::MaximumElementsPerRequest);
	EXPECT_EQ(P::Request(sent[2]).keys(), (std::vector<Column::Coordinate>{{static_cast<int>(Column::MaximumElementsPerRequest), 0}}));
	EXPECT_EQ(P::Request(sent[1]).keys().front(), (Column::Coordinate{0, 0}));
	EXPECT_EQ(group.at(0).result().chunks.front().x, -1);
	EXPECT_EQ(existing.status(), spk::Task<Column>::Status::Pending);
}
TEST(RequestingProvider, IndependentMonotonicIDsSurviveDisconnect)
{
	spk::WorkerPool pool(1);
	std::vector<spk::Message> chunks, columns;
	using Chunks = Collection<Chunk::Coordinate, Chunk>;
	Chunks a{Chunks::RequestingProvider(pool, [&](const auto &m) {
		chunks.push_back(m);
	})};
	Cache b{Cache::RequestingProvider(pool, [&](const auto &m) {
		columns.push_back(m);
	})};
	(void)a.request(Chunk::Coordinate{1, 0, 0});
	(void)b.request(Column::Coordinate{1, 0});
	EXPECT_EQ(chunks[0].requestID(), 1u);
	EXPECT_EQ(columns[0].requestID(), 1u);
	a.provider().disconnect();
	b.provider().disconnect();
	(void)a.request(Chunk::Coordinate{1, 0, 0});
	(void)b.request(Column::Coordinate{1, 0});
	EXPECT_EQ(chunks[1].requestID(), 2u);
	EXPECT_EQ(columns[1].requestID(), 2u);
}
TEST(RequestingProvider, MixedResponseRemembersRefusalAndDisconnectClearsOnlyNetworkState)
{
	spk::WorkerPool pool(2);
	std::vector<spk::Message> sent;
	Cache cache{Cache::RequestingProvider(pool, [&](const auto &m) {
		sent.push_back(m);
	})};
	auto &provider = static_cast<Cache::RequestingProvider &>(cache.provider());
	auto group = cache.request(std::vector<Column::Coordinate>{{1, 0}, {2, 0}});
	provider.receive(P::Response::build(1, {{{1, 0}, value(1)}}, {{{2, 0}, {P::Failure::Code::AcquisitionFailed, "Refused"}}}));
	group.wait();
	EXPECT_EQ(group.at(0).result().chunks.front().x, 1);
	EXPECT_EQ(group.at(1).status(), spk::Task<Column>::Status::Failed);
	auto refused = cache.request(Column::Coordinate{2, 0});
	EXPECT_EQ(refused.status(), spk::Task<Column>::Status::Failed);
	EXPECT_EQ(sent.size(), 1u);
	auto pending = cache.request(Column::Coordinate{3, 0});
	provider.disconnect();
	EXPECT_EQ(pending.status(), spk::Task<Column>::Status::Failed);
	EXPECT_EQ(cache.state({1, 0}), Cache::State::Available);
	(void)cache.request(Column::Coordinate{2, 0});
	EXPECT_EQ(sent.size(), 3u);
	EXPECT_EQ(sent.back().requestID(), 3u);
}
TEST(RequestingProvider, ErrorIsDiagnosticOnlyAndStaleResponseCannotRepublish)
{
	spk::WorkerPool pool(1);
	std::vector<spk::Message> sent;
	Cache cache{Cache::RequestingProvider(pool, [&](const auto &m) {
		sent.push_back(m);
	})};
	auto &provider = static_cast<Cache::RequestingProvider &>(cache.provider());
	auto answer = cache.request(Column::Coordinate{1, 0});
	provider.receiveError(P::Error::build(1, {Networking::Diagnostic::Severity::Error, "Malformed"}, {{1, 0}}));
	EXPECT_EQ(answer.status(), spk::Task<Column>::Status::Pending);
	EXPECT_EQ(sent.size(), 1u);
	cache.remove({1, 0});
	provider.receive(P::Response::build(1, {{{1, 0}, value(1)}}));
	(void)pool.submit([] {
				  return true;
			  })
		.get();
	EXPECT_EQ(cache.state({1, 0}), Cache::State::Absent);
	auto current = cache.request(Column::Coordinate{1, 0});
	provider.receive(P::Response::build(1, {{{1, 0}, value(1)}}));
	EXPECT_EQ(current.status(), spk::Task<Column>::Status::Pending);
	provider.receive(P::Response::build(2, {{{1, 0}, value(1, 5)}}));
	current.wait();
	EXPECT_EQ(current.result().chunks.front().y, 5);
}
TEST(CollectionUpdater, AuthoritativeSetRemoveAndPendingSettlement)
{
	spk::WorkerPool pool(1);
	Cache cache{Cache::RequestingProvider(pool, [](const auto &) {
	})};
	Cache::Updater updater(cache);
	std::vector<Column::Coordinate> available;
	std::vector<Column::Coordinate> removed;
	auto availableContract = cache.availableEvent().subscribe(
		[&](const Column::Coordinate &key, const Column &) {
			available.push_back(key);
		});
	auto removedContract = cache.removedEvent().subscribe(
		[&](const Column::Coordinate &key) {
			removed.push_back(key);
		});
	auto pendingSet = cache.request(Column::Coordinate{1, 0});
	auto pendingRemove = cache.request(Column::Coordinate{2, 0});
	updater.receive(P::Update::build({{{1, 0}, value(1, 3)}, {{3, 0}, value(3)}}, {{2, 0}, {4, 0}}));
	EXPECT_EQ(pendingSet.result().chunks.front().y, 3);
	EXPECT_EQ(pendingRemove.status(), spk::Task<Column>::Status::Failed);
	EXPECT_EQ(cache.state({2, 0}), Cache::State::Absent);
	EXPECT_EQ(cache.state({3, 0}), Cache::State::Available);
	updater.receive(P::Update::build({{{1, 0}, value(1, 4)}}, {{3, 0}}));
	EXPECT_EQ(cache.request(Column::Coordinate{1, 0}).result().chunks.front().y, 4);
	EXPECT_EQ(cache.state({3, 0}), Cache::State::Absent);
	EXPECT_EQ(
		available,
		(std::vector<Column::Coordinate>{{1, 0}, {3, 0}, {1, 0}}));
	EXPECT_EQ(removed, (std::vector<Column::Coordinate>{{3, 0}}));
}
TEST(CollectionUpdater, ResponseUpdateClaimIsAtomic)
{
	spk::WorkerPool pool(2);
	Cache cache{Cache::RequestingProvider(pool, [](const auto &) {
	})};
	Cache::Updater updater(cache);
	auto &provider = static_cast<Cache::RequestingProvider &>(cache.provider());
	for (int index = 0; index < 30; ++index)
	{
		auto pending = cache.request(Column::Coordinate{index, 0});
		std::barrier gate(2);
		std::jthread response([&] {
			gate.arrive_and_wait();
			provider.receive(P::Response::build(static_cast<unsigned>(index + 1), {{{index, 0}, value(index, 1)}}));
		});
		gate.arrive_and_wait();
		updater.receive(P::Update::build({{{index, 0}, value(index, 9)}}));
		response.join();
		pending.wait();
		(void)pool.submit([] {
					  return true;
				  })
			.get();
		EXPECT_EQ(cache.request(Column::Coordinate{index, 0}).result().chunks.front().y, 9);
	}
}
TEST(MessageDispatcher, MultipleSubscriptionsAndContractLifetime)
{
	Networking::MessageDispatcher<> dispatcher;
	int first = 0, second = 0;
	spk::Message::Writer writer(7);
	auto message = std::move(writer).build();
	auto contract = dispatcher.subscribe(7, [&](const auto &) {
		++first;
	});
	{
		auto other = dispatcher.subscribe(7, [&](const auto &) {
			++second;
		});
		dispatcher.dispatch(7, message);
	}
	dispatcher.dispatch(7, message);
	dispatcher.dispatch(8, message);
	EXPECT_EQ(first, 2);
	EXPECT_EQ(second, 1);
	contract.resign();
	dispatcher.dispatch(7, message);
	EXPECT_EQ(first, 2);
}
TEST(RequestingProvider, WaitingForNetworkDoesNotOccupyWorker)
{
	spk::WorkerPool pool(1);
	Cache cache{Cache::RequestingProvider(pool, [](const auto &) {
	})};
	auto pending = cache.request(Column::Coordinate{1, 0});
	auto independent = pool.submit([] {
		return 42;
	});
	independent.wait();
	EXPECT_EQ(independent.result(), 42);
	EXPECT_EQ(pending.status(), spk::Task<Column>::Status::Pending);
}
TEST(RequestingProvider, InvalidSemanticResponsePublishesNothing)
{
	spk::WorkerPool pool(1);
	Cache cache{Cache::RequestingProvider(pool, [](const auto &) {
	})};
	auto &provider = static_cast<Cache::RequestingProvider &>(cache.provider());
	auto pending = cache.request(std::vector<Column::Coordinate>{{1, 0}, {2, 0}});
	provider.receive(P::Response::build(1, {{{1, 0}, value(1)}, {{3, 0}, value(3)}}));
	(void)pool.submit([] {
				  return true;
			  })
		.get();
	EXPECT_EQ(cache.state({1, 0}), Cache::State::Pending);
	EXPECT_EQ(cache.state({2, 0}), Cache::State::Pending);
	EXPECT_EQ(cache.state({3, 0}), Cache::State::Absent);
	provider.receive(P::Response::build(1, {{{1, 0}, value(1)}, {{2, 0}, value(2)}}));
	pending.wait();
	EXPECT_EQ(pending.status(), spk::Task<Column>::Status::Completed);
}
TEST(RequestingProvider, ChunkRequestsSplitAtTheirDomainLimit)
{
	spk::WorkerPool pool(1);
	using Chunks = Collection<Chunk::Coordinate, Chunk>;
	std::vector<spk::Message> sent;
	Chunks cache{Chunks::RequestingProvider(pool, [&](const auto &message) {
		sent.push_back(message);
	})};
	std::vector<Chunk::Coordinate> keys;
	for (std::size_t index = 0; index <= Chunk::MaximumElementsPerRequest; ++index)
	{
		keys.push_back({static_cast<int>(index), 0, 0});
	}
	auto pending = cache.request(keys);
	ASSERT_EQ(sent.size(), 2u);
	EXPECT_EQ(Networking::ChunkProtocol::Request(sent[0]).keys().size(), Chunk::MaximumElementsPerRequest);
	EXPECT_EQ(Networking::ChunkProtocol::Request(sent[1]).keys(), (std::vector<Chunk::Coordinate>{keys.back()}));
	EXPECT_EQ(pending.size(), keys.size());
}
TEST(RequestingProvider, ReplacementPendingInLaunchWindowEmitsOnlyOneRequest)
{
	struct Gate
	{
		std::atomic_bool first{true};
		std::atomic_bool entered{false};
		std::atomic_bool release{false};
	};
	struct Delayed : Cache::RequestingProvider
	{
		std::shared_ptr<Gate> gate;
		Delayed(spk::WorkerPool &pool, std::function<void(const spk::Message &)> send, std::shared_ptr<Gate> value) :
			Cache::RequestingProvider(pool, std::move(send)),
			gate(std::move(value))
		{
		}
		void _start(const std::vector<Column::Coordinate> &keys) override
		{
			if (gate->first.exchange(false) == true)
			{
				gate->entered.store(true);
				gate->entered.notify_all();
				gate->release.wait(false);
			}
			Cache::RequestingProvider::_start(keys);
		}
	};
	spk::WorkerPool pool(1);
	std::vector<spk::Message> sent;
	auto gate = std::make_shared<Gate>();
	auto send = [&](const auto &message) {
		sent.push_back(message);
	};
	Cache cache{Delayed(pool, send, gate)};
	std::optional<Cache::Answer> old;
	std::jthread first([&] {
		old = cache.request(Column::Coordinate{1, 0});
	});
	gate->entered.wait(false);
	cache.remove({1, 0});
	auto current = cache.request(Column::Coordinate{1, 0});
	gate->release.store(true);
	gate->release.notify_all();
	first.join();
	ASSERT_EQ(sent.size(), 1u);
	EXPECT_EQ(sent.front().requestID(), 1u);
	EXPECT_EQ(old->status(), spk::Task<Column>::Status::Failed);
	auto &provider = static_cast<Cache::RequestingProvider &>(cache.provider());
	provider.receive(P::Response::build(1, {{{1, 0}, value(1)}}));
	current.wait();
	EXPECT_EQ(current.status(), spk::Task<Column>::Status::Completed);
}
