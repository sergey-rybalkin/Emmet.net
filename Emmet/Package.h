#pragma once

#include <atlstr.h>
#include <VSLCommandTarget.h>

#include "resource.h"       // main symbols
#include "Guids.h"
#include "..\EmmetUI\Resource.h"

#include "..\EmmetUI\CommandIds.h"

#include "EmmetEngine.h"
#include "PromptDlg.h"

using namespace VSL;

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
        m_initialized = FALSE;
	}
	
	~CEmmetPackage()
	{
	}

    // This method will be called after IVsPackage::SetSite is called with a valid site
	void PostSited(IVsPackageEnums::SetSiteResult /*result*/)
	{
		// Initialize the output window utility class
		m_outputWindow.SetSite(GetVsSiteCache());
        InitializeEngine();
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

void OnExpandAbbreviationCommand(CommandHandler* /*pSender*/, DWORD /*flags*/, VARIANT* /*pIn*/, VARIANT* /*pOut*/)
{
    EmmetResult result = m_engine->ExpandAbbreviation();
    if (EmmetResult_OK != result)
        ShowDiagnosticMessage(L"Expand abbreviation failed", result);
}

void OnWrapWithAbbreviationCommand(CommandHandler* /*pSender*/, DWORD /*flags*/, VARIANT* /*pIn*/, VARIANT* /*pOut*/)
{
    CComPtr<IVsUIShell> spUiShell = this->GetVsSiteCache().GetCachedService<IVsUIShell, SID_SVsUIShell>();
    HWND hwndOwner;
    spUiShell->GetDialogOwnerHwnd(&hwndOwner);
    spUiShell->EnableModeless(FALSE);
    CPromptDlg dlg(hwndOwner);
    char* szAbbreviation = dlg.Prompt();
    spUiShell->EnableModeless(TRUE);
    
    if (szAbbreviation > 0)
    {
        EmmetResult result = m_engine->WrapWithAbbreviation(szAbbreviation, strlen(szAbbreviation));
        if (EmmetResult_OK != result)
            ShowDiagnosticMessage(L"Wrap with abbreviation failed", result);
    }
}

void OnToggleCommentCommand(CommandHandler* /*pSender*/, DWORD /*flags*/, VARIANT* /*pIn*/, VARIANT* /*pOut*/)
{
    EmmetResult result = m_engine->ToggleComment();
    if (EmmetResult_OK != result)
        ShowDiagnosticMessage(L"Toggle comment failed", result);
}

void OnRemoveTagCommand(CommandHandler* /*pSender*/, DWORD /*flags*/, VARIANT* /*pIn*/, VARIANT* /*pOut*/)
{
    EmmetResult result = m_engine->RemoveTag();
    if (EmmetResult_OK != result)
        ShowDiagnosticMessage(L"Remove tag failed", result);
}

void OnMergeLinesCommand(CommandHandler* /*pSender*/, DWORD /*flags*/, VARIANT* /*pIn*/, VARIANT* /*pOut*/)
{
    EmmetResult result = m_engine->MergeLines();
    if (EmmetResult_OK != result)
        ShowDiagnosticMessage(L"Merge lines failed", result);
}

void OnUpdateImageSizeCommand(CommandHandler* /*pSender*/, DWORD /*flags*/, VARIANT* /*pIn*/, VARIANT* /*pOut*/)
{
    EmmetResult result = m_engine->UpdateImageSize();
    if (EmmetResult_OK != result)
        ShowDiagnosticMessage(L"Update image size failed", result);
}

private:
    VOID InitializeEngine()
    {
        if (m_initialized)
            return;

        // Get path to the engine.js file. It should be placed in the same folder with extension DLL.
        HMODULE hEmmetDll = GetModuleHandle(L"Emmet.dll");
        WCHAR szFilePath[MAX_PATH] = {0};
        DWORD dwPathLen = GetModuleFileName(hEmmetDll, szFilePath, MAX_PATH);
        PWSTR szLastSlash = szFilePath + dwPathLen;

        while (*(--szLastSlash) != L'\\');
#ifdef DEBUG
        StringCchCopy(szLastSlash + 1, MAX_PATH - dwPathLen, L"engine.js");
#else
        StringCchCopy(szLastSlash + 1, MAX_PATH - dwPathLen, L"engine.min.js");
#endif

        // Get DTE interface for the engine
        _DTE* pDTE;
        if (FAILED(GetVsSiteCache().QueryService(SID_SDTE, &pDTE)) || !pDTE)
            return;

        m_engine.Attach(new CEmmetEngine());

        EmmetResult result = m_engine->Initialize(pDTE, szFilePath);
        if (EmmetResult_OK != result)
        {
            ShowDiagnosticMessage(L"Engine initialization failure", result);
        }
        else
            m_initialized = TRUE;
    }

    void ShowDiagnosticMessage(PWSTR szMessageTitle, EmmetResult result)
    {
        // Get the string for the title of the message box from the resource dll.
	    CComBSTR bstrTitle;
        CComBSTR bstrMessage;
	    VSL_CHECKBOOL_GLE(bstrTitle.LoadStringW(_AtlBaseModule.GetResourceInstance(), IDS_PROJNAME));
        bstrTitle.Append(L" - ");
        bstrTitle.Append(szMessageTitle);

        if (EmmetResult_DocumentFormatNotSupported == result)
            bstrMessage = L"Document format is not supported.";
        else if (EmmetResult_NoActiveDocument == result)
            bstrMessage = L"No active document found.";
        else if (EmmetResult_UnexpectedError == result)
            bstrMessage = m_engine->GetLastError();
        else
            bstrMessage = L"No error information available.";

	    // Get a pointer to the UI Shell service to show the message box.
        CComPtr<IVsUIShell> spUiShell = this->GetVsSiteCache().GetCachedService<IVsUIShell, SID_SVsUIShell>();
	    LONG lResult;
	    HRESULT hr = spUiShell->ShowMessageBox(
	                                 0,
	                                 CLSID_NULL,
	                                 bstrTitle,
	                                 bstrMessage,
	                                 NULL,
	                                 0,
	                                 OLEMSGBUTTON_OK,
	                                 OLEMSGDEFBUTTON_FIRST,
	                                 OLEMSGICON_WARNING,
	                                 0,
	                                 &lResult);
	    VSL_CHECKHRESULT(hr);
    }

    void WriteToOutputWindow(PWSTR szMessage)
    {
        m_outputWindow.OutputMessage(szMessage);
        m_outputWindow.OutputMessage(L"\n");
    }

private:
    CAutoPtr<CEmmetEngine> m_engine;
    BOOL m_initialized;
    VsOutputWindowUtilities<> m_outputWindow;
};

// This exposes CEmmetPackage for instantiation via DllGetClassObject; however, an instance
// can not be created by CoCreateInstance, as CEmmetPackage is specifically registered with
// VS, not the the system in general.
OBJECT_ENTRY_AUTO(CLSID_Emmet, CEmmetPackage)