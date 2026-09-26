#include "chunk_collection_batch.hpp"

#include <exception>
#include <optional>
#include <utility>

Chunk::Collection::Batch::Batch(std::size_t count) :
	_remaining(count)
{
}

Chunk::Collection::Batch::Task::Answer
Chunk::Collection::Batch::answer() const
{
	return _task.answer();
}

void Chunk::Collection::Batch::addContract(
	CompletionContract contract)
{
	const std::scoped_lock lock(_mutex);
	if (!_settled)
	{
		_contracts.push_back(
			std::move(contract));
	}
}

void Chunk::Collection::Batch::acquired(
	const Chunk::Coordinate &coordinate,
	const Chunk &chunk)
{
	std::optional<BatchResult> completed;
	std::exception_ptr failure;

	{
		const std::scoped_lock lock(_mutex);
		if (_settled)
		{
			return;
		}

		try
		{
			_result.acquired.push_back(
				{coordinate, chunk});
		} catch (...)
		{
			_settled = true;
			failure = std::current_exception();
		}

		if (failure == nullptr)
		{
			--_remaining;
			if (_remaining == 0u)
			{
				_settled = true;
				completed.emplace(
					std::move(_result));
			}
		}
	}

	if (failure != nullptr)
	{
		_task.fail(std::move(failure));
	}
	else if (completed.has_value())
	{
		_task.validate(
			std::move(*completed));
	}
}

void Chunk::Collection::Batch::failed(
	const Chunk::Coordinate &coordinate,
	std::exception_ptr exception)
{
	std::optional<BatchResult> completed;
	std::exception_ptr aggregationFailure;

	{
		const std::scoped_lock lock(_mutex);
		if (_settled)
		{
			return;
		}

		try
		{
			_result.failed.push_back(
				{coordinate, std::move(exception)});
		} catch (...)
		{
			_settled = true;
			aggregationFailure =
				std::current_exception();
		}

		if (aggregationFailure == nullptr)
		{
			--_remaining;
			if (_remaining == 0u)
			{
				_settled = true;
				completed.emplace(
					std::move(_result));
			}
		}
	}

	if (aggregationFailure != nullptr)
	{
		_task.fail(
			std::move(aggregationFailure));
	}
	else if (completed.has_value())
	{
		_task.validate(
			std::move(*completed));
	}
}

void Chunk::Collection::Batch::abort(
	std::exception_ptr exception)
{
	bool shouldFail = false;
	{
		const std::scoped_lock lock(_mutex);
		if (!_settled)
		{
			_settled = true;
			shouldFail = true;
		}
	}

	if (shouldFail)
	{
		_task.fail(std::move(exception));
	}
}

void Chunk::Collection::Batch::completeEmpty()
{
	std::optional<BatchResult> completed;
	{
		const std::scoped_lock lock(_mutex);
		if (!_settled && _remaining == 0u)
		{
			_settled = true;
			completed.emplace(
				std::move(_result));
		}
	}

	if (completed.has_value())
	{
		_task.validate(
			std::move(*completed));
	}
}
