#include "stdafx.h"
#include "resource.h"  
#include "PromptDlg.h"

char* szInput = 0; // receives user input.
 
BOOL CALLBACK PromptDlgProc(HWND hwndDlg, 
                            UINT message, 
                            WPARAM wParam, 
                            LPARAM lParam) 
{
    HWND hAbbreviation;
    int nChars;

    switch (message) 
    { 
        case WM_COMMAND: 
            switch (LOWORD(wParam)) 
            { 
                case IDOK:
                    hAbbreviation = GetDlgItem(hwndDlg, IDC_ABBREVIATION);
                    nChars = GetWindowTextLength(hAbbreviation);
                    if (nChars > 0)
                    {
                        szInput = new char[nChars + 1];
                        if (!GetDlgItemTextA(hwndDlg, IDC_ABBREVIATION, szInput, nChars + 1))
                        {
                            delete szInput;
                            szInput = 0;
                        }
                    }

                    // Fall through. 
 
                case IDCANCEL: 
                    EndDialog(hwndDlg, wParam); 
                    return TRUE; 
            } 
    } 
    return FALSE; 
} 

CPromptDlg::CPromptDlg(HWND hwndOwner)
{
    m_ownerWindow = hwndOwner;
}

CPromptDlg::~CPromptDlg()
{
}

PSTR CPromptDlg::Prompt()
{
    INT_PTR dlgResult = DialogBox(GetModuleHandle(L"emmet.dll"),
                                  MAKEINTRESOURCE(IDD_PROMPT_ABBREVIATION),
                                  m_ownerWindow,
                                  PromptDlgProc);

    if (dlgResult && szInput)
    {
        m_input.Attach(szInput);
        return m_input;
    }

    return NULL;
}