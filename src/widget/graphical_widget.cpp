#include "widget/graphical_widget.hpp"

spk::Vector2 GraphicalWidget::convertScreenToWorldPosition(const spk::Vector2Int& p_screenPosition)
{
	const spk::SafePointer<Player> player = Context::instance()->player;

	const spk::Vector2 screenOffset = p_screenPosition - (viewport().geometry().size / 2);

	const spk::Vector2 convertedOffset = screenOffset / spk::Vector2(_nodeSize) * spk::Vector2(1, -1);

	return convertedOffset + (player == nullptr ? 0 : player->position());
}

spk::Vector2Int GraphicalWidget::convertWorldToScreenPosition(const spk::Vector2& p_worldPosition)
{
	const spk::SafePointer<Player> player = Context::instance()->player;

	const spk::Vector2 relativePos = (player == nullptr) ? p_worldPosition : (p_worldPosition - player->position());

	return relativePos * _nodeSize + viewport().geometry().size / 2 + viewport().geometry().anchor;
}

spk::Vector2Int GraphicalWidget::convertWorldToScreenPosition(const spk::Vector3& p_worldPosition)
{
	return (convertWorldToScreenPosition(p_worldPosition.xy()));
}

GraphicalWidget::GraphicalWidget(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::Widget(p_name, p_parent)
{
	_onNodeSizeEditionContract = _onNodeSizeEditionContractProvider.subscribe([&](){requireGeometryUpdate();});
}

void GraphicalWidget::setNodeSize(const spk::Vector2Int& p_nodeSize)
{
	_nodeSize = p_nodeSize;
}

const spk::Vector2Int& GraphicalWidget::nodeSize() const
{
	return (_nodeSize);
}