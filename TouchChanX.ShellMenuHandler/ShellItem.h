#pragma once
#pragma warning(push)
#pragma warning(disable: 28182) // 取消对 NULL 指针的引用。"Temp_value_#7550" 与 "new(1*144, nothrow)" 一样包含相同的 NULL 值。。 resource.h
#include <wil/com.h>
#pragma warning(pop)

struct IShellItem;

class ShellItem
{
	wil::com_ptr<IShellItem> m_ptr;
public:
	ShellItem(IShellItem* ptr = nullptr) :m_ptr{ ptr } {}
	ShellItem(wchar_t const* path);

	wchar_t* GetDisplayName() const;
	ShellItem GetParent();

	IShellItem* Get() const { return m_ptr.get(); }
};