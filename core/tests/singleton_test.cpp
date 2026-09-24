#include <design_pattern/singleton.hpp>

#include <exception.hpp>
#include <gtest/gtest.h>

namespace
{
	struct MissingValue
	{
	};

	struct MovableValue
	{
		int value = 0;
	};

	struct PointerOnlyValue
	{
		int value = 0;

		explicit PointerOnlyValue(int p_value) :
			value(p_value)
		{
		}

		PointerOnlyValue(const PointerOnlyValue &) = delete;
		PointerOnlyValue(PointerOnlyValue &&) = delete;
	};
}

TEST(Singleton, InstanceThrowsUntilInstanciated)
{
	EXPECT_FALSE(spk::Singleton<MissingValue>::isInstanciated());
	EXPECT_THROW(
		(void)spk::Singleton<MissingValue>::instance(),
		spk::Exception);
}

TEST(Singleton, InstanciatesMovableValue)
{
	spk::Singleton<MovableValue>::instanciate(MovableValue{37});

	EXPECT_TRUE(spk::Singleton<MovableValue>::isInstanciated());
	EXPECT_EQ(spk::Singleton<MovableValue>::instance().value, 37);
}

TEST(Singleton, InstanciatesPointerAndOwnsValue)
{
	spk::Singleton<PointerOnlyValue>::instanciate(
		new PointerOnlyValue(91));

	EXPECT_TRUE(spk::Singleton<PointerOnlyValue>::isInstanciated());
	EXPECT_EQ(
		spk::Singleton<PointerOnlyValue>::instance().value,
		91);
}

TEST(Singleton, RejectsNullPointer)
{
	EXPECT_THROW(
		spk::Singleton<PointerOnlyValue>::instanciate(nullptr),
		spk::Exception);
}
