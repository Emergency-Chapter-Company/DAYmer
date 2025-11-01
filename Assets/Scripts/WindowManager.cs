using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    const int HWND_TOPMOST = -1;        // 항상 위에
    const int HWND_NOTOPMOST = -2;      // 항상 위에 아님
    const uint SWP_NOMOVE = 0x0002;     // 위치 유지
    const uint SWP_NOSIZE = 0x0001;     // 크기 유지
    const uint SWP_SHOWWINDOW = 0x0040; // 창 보이기

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr GetActiveWindow(); // 현재 활성 창 핸들 가져오기
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

#pragma warning disable CS0414 // 사용 안하는 변수 경고 무시
    [Header("화면 설정")]
    [SerializeField, Range(0, 10000)]
    private int screenX = 320;
    [SerializeField, Range(0, 10000)]
    private int screenY = 100;
    [SerializeField]
    bool bTopMost = true;
#pragma warning restore CS0414

    void Start()
    {
#if UNITY_EDITOR
        EditorUtility.DisplayDialog("WindowManager 비활성화",
        "WindowManager는 Unity Editor에서는 작동하지 않습니다.\n빌드 후 실행해주세요.",
        "확인");
        return; // 에디터에서는 실행 안함

#elif UNITY_STANDALONE_WIN
        // 창 크기/모드 설정
        Screen.SetResolution(screenX, screenY, false); // screenX x screenY, 창모드

        IntPtr handle = GetActiveWindow();
        if (bTopMost)
        {
            // 항상 위에, 위치/크기 유지
            SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }
        else
        {
            // 항상 위에x, 위치/크기 유지
            SetWindowPos(handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }
#endif
    }

    void Update()
    {
        
    }
}
