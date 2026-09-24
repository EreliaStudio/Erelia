#pragma once

#include <condition_variable>
#include <cstddef>
#include <memory>
#include <mutex>
#include <queue>
#include <thread>
#include <utility>
#include <vector>

#include <exception.hpp>

#include <threading/task.hpp>

namespace spk
{
	class WorkerPool final
	{
	public:
		class Job
		{
		private:
			virtual void _execute() noexcept = 0;

			friend class WorkerPool;

		public:
			virtual ~Job() = default;
		};

	private:
		template <typename TResult>
			requires std::movable<TResult>
		class TaskJob final : public Job
		{
		private:
			Task<TResult> _task;

			void _execute() noexcept override
			{
				_task._execute();
			}

		public:
			explicit TaskJob(Task<TResult> task) :
				_task(std::move(task))
			{
			}
		};

		std::mutex _mutex;
		std::condition_variable _condition;
		std::queue<std::unique_ptr<Job>> _jobs;
		bool _stopping = false;
		std::vector<std::jthread> _workers;

		[[nodiscard]] static std::size_t _defaultWorkerCount() noexcept
		{
			const unsigned int count = std::thread::hardware_concurrency();
			return count == 0u ? 1u : static_cast<std::size_t>(count);
		}

		void _run()
		{
			while (true)
			{
				std::unique_ptr<Job> job;
				{
					std::unique_lock lock(_mutex);
					_condition.wait(
						lock,
						[this] {
							return _stopping || !_jobs.empty();
						});

					if (_jobs.empty())
					{
						if (_stopping)
						{
							return;
						}
						continue;
					}

					job = std::move(_jobs.front());
					_jobs.pop();
				}

				job->_execute();
			}
		}

	public:
		WorkerPool() :
			WorkerPool(_defaultWorkerCount())
		{
		}

		explicit WorkerPool(std::size_t workerCount)
		{
			if (workerCount == 0u)
			{
				throw spk::Exception("WorkerPool requires at least one worker");
			}

			_workers.reserve(workerCount);
			for (std::size_t index = 0; index < workerCount; ++index)
			{
				_workers.emplace_back([this] {
					_run();
				});
			}
		}

		WorkerPool(const WorkerPool &) = delete;
		WorkerPool(WorkerPool &&) = delete;

		WorkerPool &operator=(const WorkerPool &) = delete;
		WorkerPool &operator=(WorkerPool &&) = delete;

		~WorkerPool()
		{
			{
				const std::scoped_lock lock(_mutex);
				_stopping = true;
			}
			_condition.notify_all();
		}

		template <typename TResult>
			requires std::movable<TResult>
		[[nodiscard]] typename Task<TResult>::Answer submit(Task<TResult> task)
		{
			auto answer = task.answer();
			auto job = std::make_unique<TaskJob<TResult>>(std::move(task));

			{
				const std::scoped_lock lock(_mutex);
				if (_stopping)
				{
					throw spk::Exception("Cannot submit a Task to a stopping WorkerPool");
				}
				_jobs.push(std::move(job));
			}

			_condition.notify_one();
			return answer;
		}

		[[nodiscard]] std::size_t workerCount() const noexcept
		{
			return _workers.size();
		}
	};
}
