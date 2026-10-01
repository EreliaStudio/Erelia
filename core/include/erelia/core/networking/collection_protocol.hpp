#pragma once
#include "erelia/core/collection.hpp"
#include "erelia/core/networking/diagnostic.hpp"
#include "erelia/core/networking/message_type.hpp"
#include <algorithm>
#include <limits>
#include <set>
#include <vector>

namespace Networking
{
	template <typename TElement>
	struct CollectionMessageTypes;
	template <MessageSerializable TKey, MessageSerializable TElement>
	struct CollectionProtocol
	{
		using Types = CollectionMessageTypes<TElement>;
		struct Failure
		{
			enum class Code : std::uint8_t
			{
				AcquisitionFailed = 0
			};
			Code code = Code::AcquisitionFailed;
			std::string message;
			friend spk::Message::Writer &operator<<(spk::Message::Writer &writer, const Failure &failure)
			{
				if (failure.code != Code::AcquisitionFailed)
				{
					throw spk::Exception("Unknown Collection failure");
				}
				return writer << failure.code << failure.message;
			}
			friend const spk::Message::Reader &operator>>(const spk::Message::Reader &reader, Failure &failure)
			{
				reader >> failure.code >> failure.message;
				if (failure.code != Code::AcquisitionFailed)
				{
					throw spk::Exception("Unknown Collection failure");
				}
				return reader;
			}
		};
		static void check(const spk::Message &message, MessageType type, bool correlated)
		{
			if (message.type() != static_cast<spk::Message::Type>(type))
			{
				throw spk::Exception("Wrong Collection Message type");
			}
			if ((message.requestID() != 0) != correlated)
			{
				throw spk::Exception("Invalid Collection RequestID");
			}
		}
		class Request : public spk::Message
		{
		public:
			explicit Request(spk::Message message) :
				spk::Message(std::move(message))
			{
				check(*this, Types::Request, true);
				(void)keys();
			}
			[[nodiscard]] std::vector<TKey> keys() const
			{
				std::vector<TKey> result;
				auto input = reader();
				while (input.readOffset() < size())
				{
					TKey key;
					input >> key;
					result.push_back(key);
					if (result.size() > TElement::MaximumElementsPerRequest)
					{
						throw spk::Exception("Collection Request exceeds domain limit");
					}
				}
				if (result.empty() == true)
				{
					throw spk::Exception("Empty Collection Request");
				}
				return result;
			}
			[[nodiscard]] static Request build(spk::Message::RequestID id, const std::vector<TKey> &keys)
			{
				if (id == 0 || keys.empty() == true || keys.size() > TElement::MaximumElementsPerRequest)
				{
					throw spk::Exception("Invalid Collection Request");
				}
				spk::Message::Writer writer(static_cast<spk::Message::Type>(Types::Request));
				writer.setRequestID(id);
				for (const auto &key : keys)
				{
					writer << key;
				}
				return Request(std::move(writer).build());
			}
		};
		struct Success
		{
			TKey key;
			TElement element;
		};
		struct Failed
		{
			TKey key;
			Failure failure;
		};
		struct Section
		{
			std::vector<Success> success;
			std::vector<Failed> failure;
			std::vector<TKey> removed;
		};
		// Shared framing for Response Success/Failure and Update Set/Remove.
		class Sections : public spk::Message
		{
		protected:
			explicit Sections(spk::Message message) :
				spk::Message(std::move(message))
			{
				_validateTable();
			}
			[[nodiscard]] static spk::Message encode(MessageType type, spk::Message::RequestID id, std::vector<Success> success, std::vector<Failed> failed, std::vector<TKey> removed)
			{
				auto byKey = [](const auto &left, const auto &right) {
					return left.key < right.key;
				};
				std::sort(success.begin(), success.end(), byKey);
				std::sort(failed.begin(), failed.end(), byKey);
				std::sort(removed.begin(), removed.end());
				const auto sectionCount = [](std::size_t count) {
					return (count + TElement::ElementsPerResponseSection - 1) / TElement::ElementsPerResponseSection;
				};
				const std::size_t first = sectionCount(success.size());
				const std::size_t total = first + sectionCount(failed.size() + removed.size());
				spk::Message::Writer writer(static_cast<spk::Message::Type>(type));
				writer.setRequestID(id);
				writer.resize((total + 3) * sizeof(std::uint32_t));
				writer.edit(0, static_cast<std::uint32_t>(total));
				writer.edit(4, static_cast<std::uint32_t>(first));
				std::size_t index = 0;
				appendEntries(writer, index, success, [](auto &out, const Success &value) {
					out << value.key << value.element;
				});
				appendEntries(writer, index, failed, [](auto &out, const Failed &value) {
					out << value.key << value.failure;
				});
				appendEntries(writer, index, removed, [](auto &out, const TKey &value) {
					out << value;
				});
				writeOffset(writer, index);
				return std::move(writer).build();
			}

		private:
			static void writeOffset(spk::Message::Writer &writer, std::size_t index)
			{
				if (writer.size() > std::numeric_limits<std::uint32_t>::max())
				{
					throw spk::Exception("Collection Message too large");
				}
				writer.edit(8 + index * 4, static_cast<std::uint32_t>(writer.size()));
			}
			template <typename TEntry, typename TWrite>
			static void appendEntries(spk::Message::Writer &writer, std::size_t &section, const std::vector<TEntry> &entries, TWrite write)
			{
				for (std::size_t index = 0; index < entries.size(); ++index)
				{
					if (index % TElement::ElementsPerResponseSection == 0)
					{
						writeOffset(writer, section++);
					}
					write(writer, entries[index]);
				}
			}
			void _validateTable() const
			{
				if (size() < 12)
				{
					throw spk::Exception("Truncated Collection offset table");
				}
				const auto count = sectionCount();
				if (count > (size() - 12) / 4 || firstSectionCount() > count)
				{
					throw spk::Exception("Invalid Collection section count");
				}
				if (offset(0) != (count + 3ull) * 4 || offset(count) != size())
				{
					throw spk::Exception("Invalid Collection offset bounds");
				}
				for (std::size_t index = 0; index < count; ++index)
				{
					if (offset(index) >= offset(index + 1) || offset(index + 1) > size())
					{
						throw spk::Exception("Invalid Collection section offsets");
					}
				}
			}

		public:
			[[nodiscard]] std::uint32_t sectionCount() const
			{
				return reader().template readAt<std::uint32_t>(0);
			}
			[[nodiscard]] std::uint32_t firstSectionCount() const
			{
				return reader().template readAt<std::uint32_t>(4);
			}
			[[nodiscard]] std::uint32_t offset(std::size_t index) const
			{
				if (index > sectionCount())
				{
					throw spk::Exception("Collection offset index out of range");
				}
				return reader().template readAt<std::uint32_t>(8 + index * 4);
			}
			[[nodiscard]] std::uint32_t failureOffset() const
			{
				return offset(firstSectionCount());
			}
			[[nodiscard]] Section section(std::size_t index, bool update = false) const
			{
				if (index >= sectionCount())
				{
					throw spk::Exception("Collection section index out of range");
				}
				auto input = reader(offset(index));
				const auto end = offset(index + 1);
				Section result;
				std::size_t count = 0;
				while (input.readOffset() < end)
				{
					TKey key;
					input >> key;
					if (index < firstSectionCount())
					{
						TElement value;
						input >> value;
						result.success.push_back({key, std::move(value)});
					}
					else if (update == true)
					{
						result.removed.push_back(key);
					}
					else
					{
						Failure value;
						input >> value;
						result.failure.push_back({key, std::move(value)});
					}
					if (++count > TElement::ElementsPerResponseSection || input.readOffset() > end)
					{
						throw spk::Exception("Invalid Collection section boundary");
					}
				}
				return result;
			}
			void validateEntries(bool update = false) const
			{
				std::set<TKey> seen;
				std::optional<TKey> previous;
				for (std::size_t index = 0; index < sectionCount(); ++index)
				{
					if (index == firstSectionCount())
					{
						previous.reset();
					}
					const auto data = section(index, update);
					std::vector<TKey> keys;
					for (const auto &value : data.success)
					{
						keys.push_back(value.key);
					}
					for (const auto &value : data.failure)
					{
						keys.push_back(value.key);
					}
					for (const auto &key : data.removed)
					{
						keys.push_back(key);
					}
					for (const auto &key : keys)
					{
						if (seen.insert(key).second == false || (previous.has_value() == true && (*previous < key) == false))
						{
							throw spk::Exception("Duplicate or unordered Collection entries");
						}
						previous = key;
					}
				}
			}
		};
		class Response : public Sections
		{
		public:
			explicit Response(spk::Message message) :
				Sections(std::move(message))
			{
				check(*this, Types::Response, true);
			}
			[[nodiscard]] static Response build(spk::Message::RequestID id, std::vector<Success> success, std::vector<Failed> failed = {})
			{
				auto response = Response(Sections::encode(Types::Response, id, std::move(success), std::move(failed), {}));
				response.validateEntries();
				return response;
			}
		};
		class Update : public Sections
		{
		public:
			explicit Update(spk::Message message) :
				Sections(std::move(message))
			{
				check(*this, Types::Update, false);
				this->validateEntries(true);
			}
			[[nodiscard]] static Update build(std::vector<Success> set, std::vector<TKey> remove = {})
			{
				return Update(Sections::encode(Types::Update, 0, std::move(set), {}, std::move(remove)));
			}
		};
		class Error : public spk::Message
		{
		public:
			explicit Error(spk::Message message) :
				spk::Message(std::move(message))
			{
				if (type() != static_cast<spk::Message::Type>(Types::Error))
				{
					throw spk::Exception("Wrong Collection Error type");
				}
				(void)keys();
			}
			[[nodiscard]] Diagnostic diagnostic() const
			{
				Diagnostic value;
				reader() >> value;
				return value;
			}
			[[nodiscard]] std::vector<TKey> keys() const
			{
				auto input = reader();
				Diagnostic value;
				input >> value;
				const auto count = input.template get<std::uint32_t>();
				if (count > size() - input.readOffset())
				{
					throw spk::Exception("Invalid Collection Error key count");
				}
				std::vector<TKey> result;
				for (std::uint32_t index = 0; index < count; ++index)
				{
					TKey key;
					input >> key;
					result.push_back(key);
				}
				if (input.readOffset() != size())
				{
					throw spk::Exception("Trailing Collection Error data");
				}
				return result;
			}
			[[nodiscard]] static Error build(spk::Message::RequestID id, Diagnostic diagnostic, const std::vector<TKey> &keys = {})
			{
				if (keys.size() > std::numeric_limits<std::uint32_t>::max())
				{
					throw spk::Exception("Too many Collection Error keys");
				}
				spk::Message::Writer writer(static_cast<spk::Message::Type>(Types::Error));
				writer.setRequestID(id);
				writer << diagnostic << static_cast<std::uint32_t>(keys.size());
				for (const auto &key : keys)
				{
					writer << key;
				}
				return Error(std::move(writer).build());
			}
		};
	};
}
