#include <gtest/gtest.h>
#include <erelia/client/ping_controller.hpp>
#include <erelia/client/commands.hpp>
#include <erelia/shared/ping.hpp>
#include <chrono>
#include <thread>
#include <vector>

TEST(ClientTest, ButtonsAndCommandsBuildCorrectPingMessages)
{
    std::vector<spk::Message> sent;
    erelia::client::PingController controller(std::chrono::seconds(1), [&](const auto &message) { sent.push_back(message); });
    erelia::client::Commands commands(controller);
    EXPECT_TRUE(commands.execute("/ping A", "button"));
    EXPECT_TRUE(commands.execute("/ping B", "socket"));
    ASSERT_EQ(sent.size(), 2u);
    EXPECT_EQ(sent[0].type(), erelia::ping::RequestA);
    EXPECT_EQ(sent[1].type(), erelia::ping::RequestB);
    EXPECT_NE(sent[0].requestID(), sent[1].requestID());
    EXPECT_GT(erelia::ping::read(sent[0]).emittedMicroseconds, 0);
    EXPECT_GT(erelia::ping::read(sent[1]).emittedMicroseconds, 0);
    EXPECT_FALSE(commands.execute("/ping A", "test"));
    EXPECT_EQ(sent.size(), 2u);
    EXPECT_FALSE(commands.execute("/ping C", "test"));
    EXPECT_FALSE(commands.execute("/invalid command", "test"));
}
TEST(ClientTest, ValidResponseChangesOnlyCorrectTargetAndAllowsRetry)
{
    std::vector<spk::Message> sent;
    erelia::client::PingController controller(std::chrono::seconds(1), [&](const auto &message) { sent.push_back(message); });
    ASSERT_TRUE(controller.ping('A'));
    ASSERT_TRUE(controller.ping('B'));
    const auto responseA = erelia::ping::response('A', sent[0]);
    EXPECT_TRUE(controller.receive(responseA));
    EXPECT_EQ(controller.state('A').status, erelia::client::PingStatus::Received);
    EXPECT_EQ(controller.state('B').status, erelia::client::PingStatus::Waiting);
    EXPECT_FALSE(controller.receive(responseA));
    EXPECT_TRUE(controller.ping('A'));
    EXPECT_FALSE(controller.receive(responseA));
    EXPECT_EQ(controller.state('A').status, erelia::client::PingStatus::Waiting);
    EXPECT_TRUE(controller.receive(erelia::ping::response('B', sent[1])));
    EXPECT_EQ(controller.state('B').status, erelia::client::PingStatus::Received);
}
TEST(ClientTest, TimeoutAllowsRetryAndRejectsOldAnswer)
{
    std::vector<spk::Message> sent;
    erelia::client::PingController controller(std::chrono::milliseconds(1), [&](const auto &message) { sent.push_back(message); });
    ASSERT_TRUE(controller.ping('A'));
    std::this_thread::sleep_for(std::chrono::milliseconds(4));
    controller.poll();
    EXPECT_EQ(controller.state('A').status, erelia::client::PingStatus::Failed);
    EXPECT_FALSE(controller.receive(erelia::ping::response('A', sent[0])));
    EXPECT_TRUE(controller.ping('A'));
}
