#pragma once
#include <wrl/implements.h>
#include <ShObjIdl_core.h>

class TouchChanRootCommand;

class __declspec(uuid("C7F36224-0EBF-4CE7-A07B-71BD79CFEFC7")) TouchChanRootCommand :
    public Microsoft::WRL::RuntimeClass
    <
    Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::ClassicCom>,
    IExplorerCommand,
    //IEnumExplorerCommand,
    IObjectWithSite
    >
{
    Microsoft::WRL::ComPtr<IUnknown> m_site;

public:

    TouchChanRootCommand() = default;

    // IObjectWithSite
    IFACEMETHODIMP SetSite(IUnknown* pUnkSite) override;
    IFACEMETHODIMP GetSite(REFIID riid, void** ppvSite) override;

#pragma region IExplorerCommand
    IFACEMETHODIMP GetTitle(IShellItemArray* items, PWSTR* name) override;
    IFACEMETHODIMP GetIcon(IShellItemArray* items, PWSTR* icon) override;
    IFACEMETHODIMP GetToolTip(IShellItemArray* items, PWSTR* infoTip) override;
    IFACEMETHODIMP GetCanonicalName(GUID* guidCommandName) override;
    IFACEMETHODIMP GetState(IShellItemArray* selection, BOOL okToBeSlow, EXPCMDSTATE* cmdState) override;
    IFACEMETHODIMP GetFlags(EXPCMDFLAGS* flags) override;
    IFACEMETHODIMP Invoke(IShellItemArray* selection, IBindCtx*) override;
    IFACEMETHODIMP EnumSubCommands(IEnumExplorerCommand** enumCommands) override;
#pragma endregion

#pragma region IEnumExplorerCommand
    //HRESULT Next(ULONG celt, IExplorerCommand** pUICommand, ULONG* pceltFetched) override;
    //HRESULT Skip(ULONG) override;
    //HRESULT Reset() override;
    //HRESULT Clone(IEnumExplorerCommand** ppenum) override;
#pragma endregion
};