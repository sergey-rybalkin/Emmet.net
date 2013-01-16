#pragma once

class CPromptDlg
{
public:
    CPromptDlg(HWND hwndOwner);
    ~CPromptDlg();

    PSTR Prompt();
private:
    HWND m_ownerWindow;
    CAutoPtr<char> m_input;
};

