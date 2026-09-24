#pragma once

#include <memory>
#include <string>
#include <vector>

#include <sparkle>
#include "ui/widget/spacer_widget.hpp"

namespace playground
{
	class ChunkDebugScreen final : public spk::ContainerWidget
	{
		spk::GridLayoutFixedColumns<2> _grid;
		std::vector<std::unique_ptr<spk::TextLabel>> _names;
		std::vector<std::unique_ptr<spk::Widget>> _measurements;
		std::unique_ptr<spk::Widget> _header;
		std::unique_ptr<spk::SpacerWidget> _spacer;

		void _onGeometryChange() override;

	public:
		explicit ChunkDebugScreen(spk::Widget *parent = nullptr);
	};
}
