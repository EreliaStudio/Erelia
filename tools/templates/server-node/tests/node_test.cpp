#include "__NODE_SNAKE___node.hpp"

#include <gtest/gtest.h>

TEST(__NODE_NAME__NodeRuntime, StartsStopsAndRestarts)
{
	__NODE_NAME__Node node(
		__NODE_NAME__Node::Configuration{
			.port = 0});

	node.start();
	EXPECT_TRUE(node.isRunning());
	EXPECT_NE(node.port(), 0u);

	node.stop();
	EXPECT_FALSE(node.isRunning());

	node.start();
	EXPECT_TRUE(node.isRunning());
	node.stop();
}
