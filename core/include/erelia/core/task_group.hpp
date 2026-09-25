#pragma once

#include <concepts>
#include <cstddef>
#include <memory>
#include <span>
#include <utility>
#include <vector>

#include <exception.hpp>
#include <threading/task.hpp>
#include <threading/worker_pool.hpp>

namespace spk
{
	template <typename TResult>
		requires std::movable<TResult>
	class TaskGroup final
	{
	public:
		using TaskType = Task<TResult>;
		using TaskAnswer = typename TaskType::Answer;
		using Status = typename TaskType::Status;

	private:
		struct State
		{
			std::vector<TaskAnswer> answers;
		};

	public:
		class Answer final
		{
		private:
			std::shared_ptr<const State> _state;

			explicit Answer(std::shared_ptr<const State> state) :
				_state(std::move(state))
			{
			}

			friend class TaskGroup;

		public:
			[[nodiscard]] Status status() const noexcept
			{
				bool hasFailure = false;

				for (const TaskAnswer &answer : _state->answers)
				{
					switch (answer.status())
					{
					case Status::Pending:
						return Status::Pending;
					case Status::Failed:
						hasFailure = true;
						break;
					case Status::Completed:
						break;
					}
				}

				return hasFailure ? Status::Failed : Status::Completed;
			}

			[[nodiscard]] std::size_t size() const noexcept
			{
				return _state->answers.size();
			}

			[[nodiscard]] const TaskAnswer &at(std::size_t index) const
			{
				if (index >= _state->answers.size())
				{
					throw spk::Exception("TaskGroup answer index is outside the group");
				}
				return _state->answers[index];
			}

			[[nodiscard]] std::span<const TaskAnswer> answers() const noexcept
			{
				return _state->answers;
			}
		};

	private:
		std::vector<TaskType> _tasks;

	public:
		TaskGroup() = default;
		TaskGroup(const TaskGroup &) = delete;
		TaskGroup(TaskGroup &&) noexcept = default;

		TaskGroup &operator=(const TaskGroup &) = delete;
		TaskGroup &operator=(TaskGroup &&) noexcept = default;

		void add(TaskType task)
		{
			_tasks.push_back(std::move(task));
		}

		[[nodiscard]] std::size_t size() const noexcept
		{
			return _tasks.size();
		}

		[[nodiscard]] Answer submit(WorkerPool &workerPool) &&
		{
			auto state = std::make_shared<State>();
			state->answers.reserve(_tasks.size());

			for (TaskType &task : _tasks)
			{
				state->answers.push_back(
					workerPool.submit(std::move(task)));
			}

			_tasks.clear();
			return Answer(std::move(state));
		}
	};
}
