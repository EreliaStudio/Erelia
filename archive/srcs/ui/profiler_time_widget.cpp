#include "ui/profiler_time_widget.hpp"

#include <chrono>
#include <iomanip>
#include <sstream>
#include <utility>

namespace playground
{
	ProfilerTimeWidget::ProfilerTimeWidget(std::string name, std::string measurementIdentifier, spk::Widget *parent) :
		spk::ContainerWidget(std::move(name), parent),
		_measurementIdentifier(std::move(measurementIdentifier))
	{
		_grid.setElementPadding({8, 0});
		for (std::size_t column = 0; column < _cells.size(); ++column)
		{
			_cells[column] = std::make_unique<spk::TextLabel>("Time value", this);
			_cells[column]->setHorizontalAlignment(spk::Alignment::Horizontal::Left);
			_grid.setWidget(column, 0, _cells[column].get(), spk::Layout::SizeSettings{spk::Layout::SizePolicy::Minimum});
		}
	}

	void ProfilerTimeWidget::_onGeometryChange()
	{
		_grid.setGeometry(geometry());
	}

	void ProfilerTimeWidget::_updateState(spk::UpdateContext &context)
	{
		const auto &stats = context.profiler.timeMeasurement(_measurementIdentifier).stats();
		const auto format = [](spk::Profiler::TimeMeasurement::Duration value) {
			std::ostringstream output;
			output << std::fixed << std::setprecision(3)
				   << std::chrono::duration<double, std::milli>(value).count() << " ms";
			return output.str();
		};
		_cells[0]->setText(format(stats.average()));
		_cells[1]->setText(format(stats.min()));
		_cells[2]->setText(format(stats.max()));
	}
}
