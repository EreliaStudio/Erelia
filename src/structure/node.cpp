#include "structure/node.hpp"

Node::Type toNodeType(const std::wstring& p_string)
{
	if (p_string == L"Autotiled")
	{
		return (Node::Type::Autotiled);
	}
	else if (p_string == L"Monotiled")
	{
		return (Node::Type::Monotiled);
	}
	GENERATE_ERROR("Invalid Node::Type type [" + spk::StringUtils::wstringToString(p_string) + "]");
}

Node::Flag toNodeFlag(const std::wstring& p_string)
{
	if (p_string == L"None")
	{
		return (Node::Flag::None);
	}
	else if (p_string == L"EastBlocked")
	{
		return (Node::Flag::EastBlocked);
	}
	else if (p_string == L"WestBlocked")
	{
		return (Node::Flag::WestBlocked);
	}
	else if (p_string == L"NorthBlocked")
	{
		return (Node::Flag::NorthBlocked);
	}
	else if (p_string == L"SouthBlocked")
	{
		return (Node::Flag::SouthBlocked);
	}
	else if (p_string == L"Obstacle")
	{
		return (Node::Flag::Obstacle);
	}
	GENERATE_ERROR("Invalid Node::Flag type [" + spk::StringUtils::wstringToString(p_string) + "]");
}