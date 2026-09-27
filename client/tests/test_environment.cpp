#include <diagnostics/logger.hpp>

#include <gtest/gtest.h>

namespace
{
	class ClientTestEnvironment final : public ::testing::Environment
	{
	public:
		void SetUp() override
		{
			spk::logger.muteConsole();
		}

		void TearDown() override
		{
			spk::logger.unmuteConsole();
		}
	};

	[[maybe_unused]] ::testing::Environment *clientTestEnvironment =
		::testing::AddGlobalTestEnvironment(new ClientTestEnvironment());
}
