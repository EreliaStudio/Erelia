#pragma once

#include <sparkle.hpp>

#include "structure/context.hpp"

class GraphicalWidget : public spk::Widget
{
private:
	Context::Instanciator _contextInstanciator;

	static inline spk::Vector2Int _nodeSize = {32, 32};
	static inline spk::ContractProvider _onNodeSizeEditionContractProvider;

	spk::ContractProvider::Contract _onNodeSizeEditionContract;

protected:
	spk::Vector2 convertScreenToWorldPosition(const spk::Vector2Int& p_screenPosition);
	spk::Vector2Int convertWorldToScreenPosition(const spk::Vector2& p_worldPosition);
	spk::Vector2Int convertWorldToScreenPosition(const spk::Vector3& p_worldPosition);

public:
	GraphicalWidget(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

	static void setNodeSize(const spk::Vector2Int& p_nodeSize);
	const spk::Vector2Int& nodeSize() const;
};