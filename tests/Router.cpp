#include <gtest/gtest.h>
#include <network/local_node.hpp>
#include <network/node_router.hpp>
#include <erelia/shared/ping.hpp>
#include <vector>

TEST(RouterTest, RequestsAreIsolatedByMessageType)
{
    spk::NodeRouter router;
    spk::LocalNode a, b, extra;
    router.addNode("a", a);
    router.addNode("b", b);
    router.addNode("extra", extra);
    router.redirect(erelia::ping::RequestA, "a");
    router.redirect(erelia::ping::RequestB, "b");
    router.server().messages().publish({17, erelia::ping::request('A', 1, 100)});
    router.server().messages().publish({18, erelia::ping::request('B', 2, 200)});
    router.dispatch();
    std::vector<spk::ReceivedMessage> received;
    a.incoming().drain(received);
    ASSERT_EQ(received.size(), 1u);
    EXPECT_EQ(received[0].emitter, 17u);
    EXPECT_EQ(received[0].message.type(), erelia::ping::RequestA);
    b.incoming().drain(received);
    ASSERT_EQ(received.size(), 1u);
    EXPECT_EQ(received[0].emitter, 18u);
    EXPECT_EQ(received[0].message.type(), erelia::ping::RequestB);
    extra.incoming().drain(received);
    EXPECT_TRUE(received.empty());
    router.server().messages().publish({19, spk::Message::Writer(9999).build()});
    EXPECT_THROW(router.dispatch(), spk::Exception);
    a.incoming().drain(received);
    EXPECT_TRUE(received.empty());
    b.incoming().drain(received);
    EXPECT_TRUE(received.empty());
}
