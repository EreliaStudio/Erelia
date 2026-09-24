#pragma once

#include <array>
#include <string>

#include <sparkle>

namespace playground
{
	class ProfilerGaugeWidget final : public spk::ContainerWidget
	{
		spk::GridLayoutFixedColumns<3> _grid;
		std::array<std::unique_ptr<spk::TextLabel>, 3> _cells{};
		std::string _measurementIdentifier;

		void _onGeometryChange() override;
		void _updateState(spk::UpdateContext &context) override;

	public:
		ProfilerGaugeWidget(std::string name, std::string measurementIdentifier, spk::Widget *parent = nullptr);
	};
}
