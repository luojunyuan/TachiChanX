#include "pch.h"
#include "TouchChanRootCommand.h"
#include <Shlwapi.h>

STDMETHODIMP TouchChanRootCommand::SetSite(IUnknown* pUnkSite)
{
	m_site = pUnkSite;
	return S_OK;
}

STDMETHODIMP TouchChanRootCommand::GetSite(REFIID riid, void** ppvSite)
{
	return m_site.CopyTo(riid, ppvSite);
}

STDMETHODIMP TouchChanRootCommand::GetTitle(IShellItemArray*, PWSTR* name)
{
	return SHStrDup(L"TouchChan", name);
}

STDMETHODIMP TouchChanRootCommand::GetIcon(IShellItemArray*, PWSTR* icon)
{
	return E_NOTIMPL;
}

STDMETHODIMP TouchChanRootCommand::GetToolTip(IShellItemArray*, PWSTR* infoTip)
{
	*infoTip = {};
	return E_NOTIMPL;
}

STDMETHODIMP TouchChanRootCommand::GetCanonicalName(GUID* guidCommandName)
{
	*guidCommandName = GUID_NULL;
	return E_NOTIMPL;
}

STDMETHODIMP TouchChanRootCommand::GetState(IShellItemArray* selection, BOOL okToBeSlow, EXPCMDSTATE* cmdState)
{
	*cmdState = ECS_ENABLED;
	return S_OK;
}

STDMETHODIMP TouchChanRootCommand::GetFlags(EXPCMDFLAGS* flags)
{
	*flags = ECF_HASSUBCOMMANDS;
	return S_OK;
}

STDMETHODIMP TouchChanRootCommand::Invoke(IShellItemArray* selection, IBindCtx*)
{
	return E_NOTIMPL;
}

STDMETHODIMP TouchChanRootCommand::EnumSubCommands(IEnumExplorerCommand** enumCommands)
{
	return QueryInterface(IID_PPV_ARGS(enumCommands));
}

//
//HRESULT TouchChanRootCommand::Next(ULONG numItemsToFetch, IExplorerCommand** pUICommand, ULONG* numItemsFetched)
//{
//	ULONG fetched = 0;
//
//	for (
//		ULONG i = 0;
//		(i < numItemsToFetch) &&
//		(this->m_subCommandIter != this->m_subCommands.cend());
//		++i)
//	{
//		m_subCommandIter->CopyTo(&pUICommand[i]);
//		++m_subCommandIter;
//		++fetched;
//	}
//
//	if (numItemsFetched)
//	{
//		*numItemsFetched = fetched;
//	}
//
//	return (fetched == numItemsToFetch) ? S_OK : S_FALSE;
//}
//
//HRESULT TouchChanRootCommand::Skip(ULONG)
//{
//	return E_NOTIMPL;
//}
//
//HRESULT TouchChanRootCommand::Reset()
//{
//	m_subCommandIter = m_subCommands.begin();
//	return S_OK;
//}
//
//HRESULT TouchChanRootCommand::Clone(IEnumExplorerCommand** ppenum)
//{
//	*ppenum = nullptr;
//	return E_NOTIMPL;
//}
