#include "player.hpp"

#include "ubo_factory.hpp"

Player::TopDownCamera::TopDownCamera(const std::wstring &p_name) :
	spk::Component(p_name),
	_cameraHolder(p_name + L"/CameraHolder", nullptr),
	_cameraUBO(UBOFactory::cameraUBO())
{
	_cameraHolder.transform().place({0, 0, 10});
	_cameraHolder.transform().lookAt({0, 0, 0});
}

void Player::TopDownCamera::start()
{
	owner()->addChild(&_cameraHolder);

	_onEditionCallback = owner()->transform().addOnEditionCallback([&](){
		_cameraUBO[L"viewMatrix"] = viewMatrix();
		_cameraUBO[L"projectionMatrix"] = projectionMatrix();
		_cameraUBO.validate();
	});
}

const spk::Matrix4x4& Player::TopDownCamera::projectionMatrix() const
{
	return (_camera.projectionMatrix());
}

const spk::Matrix4x4& Player::TopDownCamera::viewMatrix() const
{
	return (_cameraHolder.transform().inverseModel());
}