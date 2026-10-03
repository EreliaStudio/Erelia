#pragma once

#include <design_pattern/contract_provider.hpp>

namespace Core
{
	// Callbacks run synchronously on the emitting thread; publishers choose that thread.
	template <typename... TArguments>
	using Event = spk::ContractProvider<TArguments...>;

	// Shared domain events belong here when their contracts are defined.
	class EventCenter
	{
	};
}
