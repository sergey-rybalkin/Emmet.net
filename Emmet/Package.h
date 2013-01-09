// Package.h

#pragma once

#include <atlstr.h>
#include <VSLCommandTarget.h>

#include "resource.h"       // main symbols
#include "Guids.h"
#include "..\EmmetUI\Resource.h"

#include "..\EmmetUI\CommandIds.h"

#include "EmmetEngine.h"

using namespace VSL;

char szAbbreviation[256]; // receives name of item to delete.
UINT nchAbbreviation = 0;
 
BOOL CALLBACK PromptDlgProc(HWND hwndDlg, 
                            UINT message, 
                            WPARAM wParam, 
                            LPARAM lParam) 
{ 
    switch (message) 
    { 
        case WM_COMMAND: 
            switch (LOWORD(wParam)) 
            { 
                case IDOK: 
                    nchAbbreviation = GetDlgItemTextA(hwndDlg, IDC_ABBREVIATION, szAbbreviation, 256);
                    if (0 == nchAbbreviation)
                        *szAbbreviation = 0;
 
                    // Fall through. 
 
                case IDCANCEL: 
                    EndDialog(hwndDlg, wParam); 
                    return TRUE; 
            } 
    } 
    return FALSE; 
} 


class ATL_NO_VTABLE CEmmetPackage : 
	// CComObjectRootEx and CComCoClass are used to implement a non-thread safe COM object, and 
	// a partial implementation for IUnknown (the COM map below provides the rest).
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CEmmetPackage, &CLSID_Emmet>,
	// Provides the implementation for IVsPackage to make this COM object into a VS Package.
	public IVsPackageImpl<CEmmetPackage, &CLSID_Emmet>,
	public IOleCommandTargetImpl<CEmmetPackage>,
	// Provides consumers of this object with the ability to determine which interfaces support
	// extended error information.
	public ATL::ISupportErrorInfoImpl<&__uuidof(IVsPackage)>
{
public:

// Provides a portion of the implementation of IUnknown, in particular the list of interfaces
// the CEmmetPackage object will support via QueryInterface
BEGIN_COM_MAP(CEmmetPackage)
	COM_INTERFACE_ENTRY(IVsPackage)
	COM_INTERFACE_ENTRY(IOleCommandTarget)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

// COM objects typically should not be cloned, and this prevents cloning by declaring the 
// copy constructor and assignment operator private (NOTE:  this macro includes the declaration of
// a private section, so everything following this macro and preceding a public or protected 
// section will be private).
VSL_DECLARE_NOT_COPYABLE(CEmmetPackage)

public:
	CEmmetPackage()
	{
        m_pEngine = NULL;
	}
	
	~CEmmetPackage()
	{
        if (NULL != m_pEngine)
            delete m_pEngine;
	}

    // This method will be called after IVsPackage::SetSite is called with a valid site
	void PostSited(IVsPackageEnums::SetSiteResult /*result*/)
	{
		// Initialize the output window utility class
		m_OutputWindow.SetSite(GetVsSiteCache());
	}
    
	// This function provides the error information if it is not possible to load
	// the UI dll. It is for this reason that the resource IDS_E_BADINSTALL must
	// be defined inside this dll's resources.
	static const LoadUILibrary::ExtendedErrorInfo& GetLoadUILibraryErrorInfo()
	{
		static LoadUILibrary::ExtendedErrorInfo errorInfo(IDS_E_BADINSTALL);
		return errorInfo;
	}

	// DLL is registered with VS via a pkgdef file. Don't do anything if asked to
	// self-register.
	static HRESULT WINAPI UpdateRegistry(BOOL bRegister)
	{
		return S_OK;
	}

// NOTE - the arguments passed to these macros can not have names longer then 30 characters

// Definition of the commands handled by this package
VSL_BEGIN_COMMAND_MAP()

    VSL_COMMAND_MAP_ENTRY(CLSID_EmmetCmdSet,
                          cmdidExpandAbbreviation,
                          NULL,
                          CommandHandler::ExecHandler(&OnExpandAbbreviationCommand))

    VSL_COMMAND_MAP_ENTRY(CLSID_EmmetCmdSet,
                          cmdidExpandAbbreviationInternal,
                          NULL,
                          CommandHandler::ExecHandler(&OnExpandAbbreviationCommand))

    VSL_COMMAND_MAP_ENTRY(CLSID_EmmetCmdSet,
                          cmdidWrapWithAbbreviation,
                          NULL,
                          CommandHandler::ExecHandler(&OnWrapWithAbbreviationCommand))

    VSL_COMMAND_MAP_ENTRY(CLSID_EmmetCmdSet,
                          cmdidWrapWithAbbreviationInternal,
                          NULL,
                          CommandHandler::ExecHandler(&OnWrapWithAbbreviationCommand))

    VSL_COMMAND_MAP_ENTRY(CLSID_EmmetCmdSet,
                          cmdidToggleComment,
                          NULL,
                          CommandHandler::ExecHandler(&OnToggleCommentCommand))

    VSL_COMMAND_MAP_ENTRY(CLSID_EmmetCmdSet,
                          cmdidRemoveTag,
                          NULL,
                          CommandHandler::ExecHandler(&OnRemoveTagCommand))

    VSL_COMMAND_MAP_ENTRY(CLSID_EmmetCmdSet,
                          cmdidMergeLines,
                          NULL,
                          CommandHandler::ExecHandler(&OnMergeLinesCommand))

    VSL_COMMAND_MAP_ENTRY(CLSID_EmmetCmdSet,
                          cmdidUpdateImageSize,
                          NULL,
                          CommandHandler::ExecHandler(&OnUpdateImageSizeCommand))
VSL_END_VSCOMMAND_MAP()

void ShowDiagnosticMessage(PWSTR szMessage, OLEMSGICON icon = OLEMSGICON_INFO)
{
    // Get the string for the title of the message box from the resource dll.
	CComBSTR bstrTitle;
	VSL_CHECKBOOL_GLE(bstrTitle.LoadStringW(_AtlBaseModule.GetResourceInstance(), IDS_PROJNAME));
	// Get a pointer to the UI Shell service to show the message box.
    CComPtr<IVsUIShell> spUiShell = this->GetVsSiteCache().GetCachedService<IVsUIShell, SID_SVsUIShell>();
	LONG lResult;
	HRESULT hr = spUiShell->ShowMessageBox(
	                             0,
	                             CLSID_NULL,
	                             bstrTitle,
	                             W2OLE(szMessage),
	                             NULL,
	                             0,
	                             OLEMSGBUTTON_OK,
	                             OLEMSGDEFBUTTON_FIRST,
	                             icon,
	                             0,
	                             &lResult);
	VSL_CHECKHRESULT(hr);
}

void WriteToOutputWindow(PWSTR szMessage)
{
    m_OutputWindow.OutputMessage(szMessage);
    m_OutputWindow.OutputMessage(L"\n");
}

void OnExpandAbbreviationCommand(CommandHandler* /*pSender*/, DWORD /*flags*/, VARIANT* /*pIn*/, VARIANT* /*pOut*/)
{
    if (NULL == m_pEngine)
        InitializeEngine();

    if (EmmetResult_OK != m_pEngine->ExpandAbbreviation())
        ShowDiagnosticMessage(L"Expand abbreviation failed", OLEMSGICON_WARNING);
}

void OnWrapWithAbbreviationCommand(CommandHandler* /*pSender*/, DWORD /*flags*/, VARIANT* /*pIn*/, VARIANT* /*pOut*/)
{
    if (NULL == m_pEngine)
        InitializeEngine();

    CComPtr<IVsUIShell> spUiShell = this->GetVsSiteCache().GetCachedService<IVsUIShell, SID_SVsUIShell>();
    HWND hwndOwner;
    spUiShell->GetDialogOwnerHwnd(&hwndOwner);
    spUiShell->EnableModeless(FALSE);
    INT_PTR dlgResult = DialogBox(GetModuleHandle(L"emmet.dll"),
                                  MAKEINTRESOURCE(IDD_PROMPT_ABBREVIATION),
                                  hwndOwner,
                                  PromptDlgProc);
    if (dlgResult && nchAbbreviation > 0)
    {
        EmmetResult engineResult = m_pEngine->WrapWithAbbreviation(szAbbreviation, nchAbbreviation);
        if (EmmetResult_OK != engineResult)
            ShowDiagnosticMessage(L"Wrap with abbreviation failed", OLEMSGICON_WARNING);
    }

    spUiShell->EnableModeless(TRUE);

    *szAbbreviation = 0;
    nchAbbreviation = 0;
}

void OnToggleCommentCommand(CommandHandler* /*pSender*/, DWORD /*flags*/, VARIANT* /*pIn*/, VARIANT* /*pOut*/)
{
    if (NULL == m_pEngine)
        InitializeEngine();

    if (EmmetResult_OK != m_pEngine->ToggleComment())
        ShowDiagnosticMessage(L"Toggle comment failed", OLEMSGICON_WARNING);
}

void OnRemoveTagCommand(CommandHandler* /*pSender*/, DWORD /*flags*/, VARIANT* /*pIn*/, VARIANT* /*pOut*/)
{
    if (NULL == m_pEngine)
        InitializeEngine();

    if (EmmetResult_OK != m_pEngine->RemoveTag())
        ShowDiagnosticMessage(L"Remove tag failed", OLEMSGICON_WARNING);
}

void OnMergeLinesCommand(CommandHandler* /*pSender*/, DWORD /*flags*/, VARIANT* /*pIn*/, VARIANT* /*pOut*/)
{
    if (NULL == m_pEngine)
        InitializeEngine();

    if (EmmetResult_OK != m_pEngine->MergeLines())
        ShowDiagnosticMessage(L"Merge lines failed", OLEMSGICON_WARNING);
}

void OnUpdateImageSizeCommand(CommandHandler* /*pSender*/, DWORD /*flags*/, VARIANT* /*pIn*/, VARIANT* /*pOut*/)
{
    if (NULL == m_pEngine)
        InitializeEngine();

    if (EmmetResult_OK != m_pEngine->UpdateImageSize())
        ShowDiagnosticMessage(L"Update image size failed", OLEMSGICON_WARNING);
}

private:
    VOID InitializeEngine()
    {
        // Get path to the engine.js file. It should be placed in the same folder with extension DLL.
        HMODULE hEmmetDll = GetModuleHandle(L"Emmet.dll");
        WCHAR szFilePath[MAX_PATH] = {0};
        DWORD dwPathLen = GetModuleFileName(hEmmetDll, szFilePath, MAX_PATH);
        PWSTR szLastSlash = szFilePath + dwPathLen;

        while (*(--szLastSlash) != L'\\');
        StringCchCopy(szLastSlash + 1, MAX_PATH - dwPathLen, L"engine.js");

        // Get DTE interface for the engine
        _DTE* pDTE;
        if (FAILED(GetVsSiteCache().QueryService(SID_SDTE, &pDTE)) || !pDTE)
            return;

        m_pEngine = new CEmmetEngine();

        m_pEngine->Initialize(pDTE, szFilePath);
    }

private:
    CEmmetEngine* m_pEngine;
    VsOutputWindowUtilities<> m_OutputWindow;
};

// This exposes CEmmetPackage for instantiation via DllGetClassObject; however, an instance
// can not be created by CoCreateInstance, as CEmmetPackage is specifically registered with
// VS, not the the system in general.
OBJECT_ENTRY_AUTO(CLSID_Emmet, CEmmetPackage)