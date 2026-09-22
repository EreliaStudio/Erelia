#include "ui/chunk_debug_screen.hpp"

#include <array>
#include <utility>

#include "ui/profiler_gauge_widget.hpp"
#include "ui/profiler_time_widget.hpp"

namespace playground
{
	namespace
	{
		class ProfilerHeaderWidget final : public spk::ContainerWidget
		{
			spk::GridLayoutFixedColumns<3> _grid;
			std::array<std::unique_ptr<spk::TextLabel>, 3> _cells{};

			void _onGeometryChange() override
			{
				_grid.setGeometry(geometry());
			}

		public:
			explicit ProfilerHeaderWidget(spk::Widget *parent) :
				spk::ContainerWidget("Profiler header", parent)
			{
				const std::array<std::string, 3> names{"Average", "Min", "Max"};
				_grid.setElementPadding({8, 0});
				for (std::size_t column = 0; column < _cells.size(); ++column)
				{
					_cells[column] = std::make_unique<spk::TextLabel>("Header cell", this);
					_cells[column]->setText(names[column]);
					_cells[column]->setHorizontalAlignment(spk::Alignment::Horizontal::Left);
					_grid.setWidget(column, 0, _cells[column].get(), spk::Layout::SizeSettings{spk::Layout::SizePolicy::Minimum});
				}
			}
		};

		struct Metric
		{
			std::string name;
			std::string identifier;
			bool isTime;
		};
	}

	ChunkDebugScreen::ChunkDebugScreen(spk::Widget *parent) :
		spk::ContainerWidget("Chunk debug", parent),
		_header(std::make_unique<ProfilerHeaderWidget>(this)),
		_spacer(std::make_unique<spk::SpacerWidget>("Chunk debug spacer", this))
	{
		_grid.setElementPadding({8, 2});

		auto headerName = std::make_unique<spk::TextLabel>("Metric header", this);
		headerName->setText("Metric");
		headerName->setHorizontalAlignment(spk::Alignment::Horizontal::Left);
		_names.push_back(std::move(headerName));
		_grid.setWidget(0, 0, _names.back().get(), spk::Layout::SizeSettings{spk::Layout::SizePolicy::Minimum});
		_grid.setWidget(1, 0, _header.get(), spk::Layout::SizeSettings{spk::Layout::SizePolicy::Minimum});

		const std::array<Metric, 14> metrics{{
			{"Vertices", "Chunks/vertices", false},
			{"Indexes", "Chunks/indexes", false},
			{"Vertices / chunk", "Chunks/vertices per chunk", false},
			{"Indexes / chunk", "Chunks/indexes per chunk", false},
			{"Bake / chunk", "Chunks/bake chunk", true},
			{"Bake / group", "Chunks/bake group", true},
			{"Build / chunk", "Chunks/build chunk", true},
			{"Build / group", "Chunks/build group", true},
			{"Render / chunk", "Chunks/render", true},
			{"Build / frame", spk::Profiler::BuildRenderSnapshotMeasurement, true},
			{"Render / frame", spk::Profiler::RenderFrameMeasurement, true},
			{"Render / commands", spk::Profiler::RenderCommandCountMeasurement, false},
			{"Render / engine.Scene commands", std::string(spk::Profiler::RenderPassCommandCountMeasurementPrefix) + spk::Engine::SceneRenderPassKey.name, false},
			{"Render / sparkle.Overlay commands", std::string(spk::Profiler::RenderPassCommandCountMeasurementPrefix) + spk::Widget::OverlayKey.name, false}}};

		std::size_t row = 1;
		for (const auto &metric : metrics)
		{
			auto name = std::make_unique<spk::TextLabel>("Metric", this);
			name->setText(metric.name);
			name->setHorizontalAlignment(spk::Alignment::Horizontal::Left);
			_names.push_back(std::move(name));
			_grid.setWidget(0, row, _names.back().get(), spk::Layout::SizeSettings{spk::Layout::SizePolicy::Minimum});

			if (metric.isTime)
				_measurements.push_back(std::make_unique<ProfilerTimeWidget>("Time measurement", metric.identifier, this));
			else
				_measurements.push_back(std::make_unique<ProfilerGaugeWidget>("Gauge measurement", metric.identifier, this));
			_grid.setWidget(1, row++, _measurements.back().get(), spk::Layout::SizeSettings{spk::Layout::SizePolicy::Minimum});
		}
		_grid.setWidget(1, row, _spacer.get());
	}

	void ChunkDebugScreen::_onGeometryChange()
	{
		_grid.setGeometry(geometry());
	}
}
