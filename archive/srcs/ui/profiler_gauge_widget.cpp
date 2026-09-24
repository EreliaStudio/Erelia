#include "ui/profiler_gauge_widget.hpp"

#include <utility>

namespace playground
{
	ProfilerGaugeWidget::ProfilerGaugeWidget(std::string name, std::string measurementIdentifier, spk::Widget *parent) :
		spk::ContainerWidget(std::move(name), parent),
		_measurementIdentifier(std::move(measurementIdentifier))
	{
		_grid.setElementPadding({8, 0});
		for (std::size_t column = 0; column < _cells.size(); ++column)
		{
			_cells[column] = std::make_unique<spk::TextLabel>("Gauge value", this);
			_cells[column]->setHorizontalAlignment(spk::Alignment::Horizontal::Left);
			_grid.setWidget(column, 0, _cells[column].get(), spk::Layout::SizeSettings{spk::Layout::SizePolicy::Minimum});
		}
	}

	void ProfilerGaugeWidget::_onGeometryChange()
	{
		_grid.setGeometry(geometry());
	}

	void ProfilerGaugeWidget::_updateState(spk::UpdateContext &context)
	{
		const auto &stats = context.profiler.gaugeMeasurement<std::size_t>(_measurementIdentifier).stats();
		_cells[0]->setText(std::to_string(stats.average()));
		_cells[1]->setText(std::to_string(stats.min()));
		_cells[2]->setText(std::to_string(stats.max()));
	}
}
