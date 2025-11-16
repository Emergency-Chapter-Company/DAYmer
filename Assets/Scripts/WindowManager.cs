using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WindowManager : MonoBehaviour
{
    /* ====== 싱글톤 변수 ====== */
    public static WindowManager instance = null; // 싱글톤 변수

    /* ====== WinAPI 상수 및 함수 ====== */
    const int HWND_TOPMOST = -1;        // 항상 위에
    const int HWND_NOTOPMOST = -2;      // 항상 위에 아님
    const uint SWP_NOMOVE = 0x0002;     // 위치 유지
    const uint SWP_NOSIZE = 0x0001;     // 크기 유지
    const uint SWP_SHOWWINDOW = 0x0040; // 창 보이기

    /* ====== WinAPI 함수 ====== */
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr GetActiveWindow(); // 현재 활성 창 핸들 가져오기
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    /* ====== 화면 설정 변수 ====== */
#pragma warning disable CS0414 // 사용 안하는 변수 경고 무시
    [Header("화면 설정")]
    [SerializeField, Range(0, 10000)]
    private int timerScreenX = 320;
    [SerializeField, Range(0, 10000)]
    private int timerScreenY = 100;
    [SerializeField, Range(0, 10000)]
    private int roomScreenX = 500;
    [SerializeField, Range(0, 10000)]
    private int roomScreenY = 300;
    [SerializeField]
    private bool bTopMost = true;
#pragma warning restore CS0414

    [Header("씬 이름 설정")]
    [SerializeField]
    private string timerSceneName = "WindowTestScene";
    [SerializeField]
    private string roomSceneName = "WindowChangeTestScene";

    void Awake()
    {
        // 싱글톤 설정
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(WindowManager.instance.gameObject);

            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        Application.runInBackground = true; // 백그라운드 실행 허용

        WindowSetting(GetCurrentSceneScreenX(), GetCurrentSceneScreenY());
    }

    private void OnEnable()
    {
        // 씬 로드 후 항상 위 적용
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 바뀐 후 항상 위 적용
        WindowSetting(GetCurrentSceneScreenX(), GetCurrentSceneScreenY());

        GameManager.instance.LoadGame(); // 씬 전환 시 게임 데이터 로드
    }

    private void WindowSetting(int width, int height)
    {
#if UNITY_EDITOR
        EditorUtility.DisplayDialog("WindowManager 비활성화",
        "WindowManager는 Unity Editor에서는 작동하지 않습니다.\n빌드 후 실행해주세요.",
        "확인");
        return; // 에디터에서는 실행 안함

#elif UNITY_STANDALONE_WIN
        // 화면 크기 적용
        Screen.SetResolution(width, height, false);

        // 항상 위 적용
        IntPtr handle = GetActiveWindow();
        if (bTopMost)
        {
            SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }
        else
        {
            SetWindowPos(handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }
#endif
    }

    private int GetCurrentSceneScreenX()
    {
        return SceneManager.GetActiveScene().name == timerSceneName ? timerScreenX : roomScreenX;
    }

    private int GetCurrentSceneScreenY()
    {
        return SceneManager.GetActiveScene().name == timerSceneName ? timerScreenY : roomScreenY;
    }

    public void ChangeScene()
    {
        if (SceneManager.GetActiveScene().name == timerSceneName) // 현재 타이머 씬이면
        {
            SceneManager.LoadScene(roomSceneName);
            Screen.SetResolution(roomScreenX, roomScreenY, false);
        }
        else // 현재 방 씬이면
        {
            SceneManager.LoadScene(timerSceneName);
            Screen.SetResolution(timerScreenX, timerScreenY, false);
        }
    }
}
