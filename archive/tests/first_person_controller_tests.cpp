#include "behaviour/first_person_controller.hpp"

#include <gtest/gtest.h>

#include <chrono>
#include <cmath>

namespace
{
	bool approximately(float value, float expected)
	{
		return std::abs(value - expected) < 0.001f;
	}
}

TEST(FirstPersonControllerTest, MovesRotatesRecentersAndHonorsDeactivation)
{
	spk::Entity3D entity("Controlled entity");
	auto &controller = entity.addBehaviour<playground::FirstPersonController>();
	spk::Keyboard keyboard;
	spk::Mouse mouse;
	entity.setGeometry({.anchor = {0, 0}, .size = {800, 600}});
	mouse.position = {400, 300};
	keyboard[spk::Keyboard::Z] = spk::InputState::Down;
	spk::UpdateContext context{
		.time = {},
		.deltaTime = std::chrono::seconds(1),
		.keyboard = keyboard,
		.mouse = mouse};
	controller.updateState(context);
	EXPECT_TRUE(approximately(entity.transform().position().x, 0.0f));
	EXPECT_TRUE(approximately(entity.transform().position().z, -10.0f));

	keyboard[spk::Keyboard::Z] = spk::InputState::Up;
	spk::MouseMovedRecord initializationRecord{.position = {1300, 300}};
	spk::MouseMovedEvent initializationEvent(initializationRecord, mouse);
	controller.dispatch(initializationEvent);
	EXPECT_EQ(initializationEvent.mousePositionRequest(), spk::Vector2Int(400, 300));

	spk::MouseMovedRecord movementRecord{.position = {1300, 300}};
	spk::MouseMovedEvent movementEvent(movementRecord, mouse);
	controller.dispatch(movementEvent);
	EXPECT_EQ(movementEvent.mousePositionRequest(), spk::Vector2Int(400, 300));
	mouse.position = {400, 300};
	keyboard[spk::Keyboard::Z] = spk::InputState::Down;
	context.deltaTime = std::chrono::seconds(1);
	controller.updateState(context);
	EXPECT_TRUE(approximately(entity.transform().position().x, 10.0f));
	EXPECT_TRUE(approximately(entity.transform().position().z, -10.0f));
	const auto stoppedMouseRotation = entity.transform().rotation();
	spk::MouseMovedRecord centeredRecord{.position = {400, 300}};
	spk::MouseMovedEvent centeredEvent(centeredRecord, mouse);
	controller.dispatch(centeredEvent);
	EXPECT_EQ(entity.transform().rotation(), stoppedMouseRotation);
	EXPECT_FALSE(centeredEvent.mousePositionRequest().has_value());

	controller.deactivate();
	spk::MouseMovedRecord inactiveRecord{.position = {1300, 300}};
	spk::MouseMovedEvent inactiveEvent(inactiveRecord, mouse);
	controller.dispatch(inactiveEvent);
	EXPECT_FALSE(inactiveEvent.mousePositionRequest().has_value());
}
