#pragma once

#include <sparkle.hpp>

class Actor
{
public:
	using Contract = spk::ContractProvider::Contract;
	using Job = spk::ContractProvider::Job;

private:
	spk::Vector2 _position = spk::Vector2(0, 0);

	spk::Vector2Int _origin;
	spk::Vector2Int _destination;
	spk::Timer _motionTimer = spk::Timer(spk::Duration(150, spk::TimeUnit::Millisecond));

	spk::ContractProvider _onEditionContractProvider;

public:
	void update();
	Contract subscribeToEdition(const Job& p_job);

	bool isMoving() const;

	void move(const spk::Vector2& p_delta);
	void place(const spk::Vector2& p_position);

	const spk::Vector2& position() const;
};