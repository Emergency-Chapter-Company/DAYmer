using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ProcessMonitor : MonoBehaviour
{
    /* WinAPI */
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int GW_OWNER = 4;

    private delegate bool EnumWindowsProc(System.IntPtr hWnd, System.IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc cb, System.IntPtr lp);
    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(System.IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern System.IntPtr GetWindow(System.IntPtr hWnd, int cmd);
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(System.IntPtr hWnd, int index);
    [DllImport("user32.dll")]
    private static extern int GetWindowThreadProcessId(System.IntPtr hWnd, out int pid);

    /* 프로세스 관련 변수 */
    private List<Process> allProcesses;
    private List<Process> visibleProcesses;
    private List<string> displayNames;
    private Dictionary<string, string> displayToProcName;

    /* UI 요소 */
    [Header("UI")]
    [SerializeField]
    private TMP_Dropdown dropdown;
    [SerializeField]
    private Button refreshButton;

    /* ====== 필터 ====== */

    // 1) 프로세스명 정확 일치 블랙리스트(대소문자 무시)
    private readonly HashSet<string> nameBlacklist = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
    {
        // 시스템/헬퍼 성격
        "ctfmon", "dllhost", "svchost", "conhost", "fontdrvhost", "TextInputHost",
        "RuntimeBroker", "ApplicationFrameHost", "ShellExperienceHost", "SearchHost", "SystemSettings",
        // GPU/VR/백그라운드 벤더
        "NVDisplay.Container", "NVIDIA Share", "NVIDIA Web Helper", "NVIDIA ShadowPlay Helper",
        "OVRServer_x64", "OVRRedir", "OVRServiceLauncher", "OVRLibraryService",
        // 예시: MS 대시보드/백그라
        "CrossDeviceResume", "DashboardNotificationManager",
        // Unity 백그라운드
        "UnityShaderCompiler", "Unity.Licensing.Client", "Unity.ILPP.Runner", "UnityPackageManager"
    };

    // 2) 표시명/경로/회사명에 포함되면 제외할 토큰들 (느슨한 매칭)
    private readonly string[] containsBlacklistTokens =
    {
        // NVIDIA/Meta/Oculus/Quest/VR/헬퍼류
        "nvidia", "oculus", "meta", "quest", "vr", "openxr",
        "container", "service", "helper", "runtime", "driver",
        // VS ServiceHub/IntelliCode
        "servicehub", "intellicode", "modelservice", "code analysis service",
        // 브라우저/렌더러/헬퍼
        "gpu process", "renderer", "crashpad", "helper", "settings"
    };

    // 3) 화이트리스트(업무/협업/브라우저/IDE/오피스/디자인/런처) — 프로세스명 기준
    private readonly HashSet<string> uiWhitelist = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
    {
        // 협업/커뮤니케이션
        "Discord", "slack", "teams", "zoom", "Webex", "notion", "obsidian",
        // 브라우저
        "chrome", "msedge", "firefox", "brave", "opera",
        // IDE/에디터
        "Code", "code", "devenv", "rider64", "rider", "idea64", "webstorm64",
        "pycharm64", "clion64", "notepad++", "notepad", "sublime_text", "androidstudio",
        // 디자인/영상/문서
        "Blender", "Photoshop", "Illustrator", "AfterFX", "Premiere", "PremierePro",
        "Lightroom", "Figma", "Acrobat", "AcrobatSDIWindow", "WINWORD", "EXCEL", "POWERPNT",
        // 런처/도구
        "steam", "EpicGamesLauncher", "Battle.net",
        // Unity — 허용
        "Unity", "Unity Editor", "Unity Hub"
    };

    // 4) Unity는 Editor/Hub만 허용
    private readonly HashSet<string> allowedUnity = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
    {
        "Unity", "Unity Editor", "Unity Hub"
    };

    /* ====== 주요 로직 ====== */

    void Start()
    {
        RefreshProcessList();

        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshProcessList);

        dropdown.onValueChanged.AddListener(OnSelectProcess);
    }

    public void RefreshProcessList()
    {
        UnityEngine.Debug.Log("🔄 프로세스 목록 새로고침 (현재 열려있는 창 기준 + 화/블랙리스트)");

        allProcesses = Process.GetProcesses().ToList();
        visibleProcesses = new List<Process>();
        displayToProcName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /* 실제 화면에 보이는 ‘사용자 창’ 후보 수집 */
        EnumWindows((hWnd, lParam) =>
        {
            if (!IsWindowVisible(hWnd))
                return true;

            int ex = GetWindowLong(hWnd, GWL_EXSTYLE);
            if ((ex & WS_EX_TOOLWINDOW) != 0)
                return true;

            if (GetWindow(hWnd, GW_OWNER) != IntPtr.Zero)
                return true;

            GetWindowThreadProcessId(hWnd, out int pid);
            if (pid == 0)
                return true;

            try
            {
                var proc = Process.GetProcessById(pid);
                if (LooksUnity(proc) && !UnityAllowed(proc))
                    return true;

                string readable = GetReadableName(proc);
                if (IsBlacklisted(proc, readable))
                    return true;

                if (!visibleProcesses.Contains(proc))
                    visibleProcesses.Add(proc);
            }
            catch { }

            return true;
        }, IntPtr.Zero);

        /* 표시명 구성(FileDescription 우선) */
        displayNames = new List<string> { "Select Process..." }; // 첫 항목에 NULL 대용 추가

        foreach (var p in visibleProcesses)
        {
            string readableName = GetReadableName(p);
            if (!displayToProcName.ContainsKey(readableName))
                displayToProcName.Add(readableName, p.ProcessName);
        }

        displayNames.AddRange(displayToProcName.Keys.OrderBy(n => n));

        /* 드롭다운 업데이트 */
        dropdown.ClearOptions();
        dropdown.AddOptions(displayNames);
        dropdown.SetValueWithoutNotify(0); // 첫 항목("Select Process...") 선택
    }

    /* ====== 필터/도우미 ====== */

    private bool LooksUnity(Process p)
    {
        string pn = p.ProcessName;
        try
        {
            var info = p.MainModule.FileVersionInfo;
            string fd = info?.FileDescription ?? string.Empty;
            return pn.StartsWith("Unity", System.StringComparison.OrdinalIgnoreCase) ||
                   fd.StartsWith("Unity", System.StringComparison.OrdinalIgnoreCase);
        }
        catch { return pn.StartsWith("Unity", System.StringComparison.OrdinalIgnoreCase); }
    }

    private bool UnityAllowed(Process p)
    {
        // Unity Editor/Hub만 허용
        string pn = p.ProcessName;
        string fd = string.Empty;
        try { fd = p.MainModule.FileVersionInfo?.FileDescription ?? string.Empty; } catch { }
        return allowedUnity.Contains(pn) || allowedUnity.Contains(fd);
    }

    private bool IsBlacklisted(Process p, string readableName)
    {
        // (A) 정확 이름 컷
        if (nameBlacklist.Contains(p.ProcessName))
            return true;

        // (B) 표시명/경로/회사명 토큰 컷
        string disp = (readableName ?? string.Empty).ToLowerInvariant();
        string path = SafeLowerPath(p);
        string company = SafeCompany(p);

        foreach (var token in containsBlacklistTokens)
        {
            if (disp.Contains(token) || path.Contains(token) || company.Contains(token))
                return true;
        }

        // (C) 세부 규칙: Unity 백그라운드(상단에서 1차 컷했지만 안전망)
        if (LooksUnity(p) && !UnityAllowed(p))
            return true;

        return false;
    }

    private string GetReadableName(Process p)
    {
        try
        {
            var info = p.MainModule.FileVersionInfo;
            if (!string.IsNullOrEmpty(info.FileDescription))
                return info.FileDescription;
            return p.ProcessName;
        }
        catch
        {
            return p.ProcessName;
        }
    }

    private string SafeLowerPath(Process p)
    {
        try { return p.MainModule.FileName.Replace('/', '\\').ToLowerInvariant(); }
        catch { return string.Empty; }
    }

    private string SafeCompany(Process p)
    {
        try
        {
            var info = p.MainModule.FileVersionInfo;
            return (info?.CompanyName ?? string.Empty).ToLowerInvariant();
        }
        catch { return string.Empty; }
    }

    private void OnSelectProcess(int index)
    {
        if (index <= 0 || index >= displayNames.Count)
            return; // 0번째("Select Process...")는 무시

        string displayName = displayNames[index];
        string procName = displayToProcName[displayName];

        var procGroup = visibleProcesses
            .Where(p => p.ProcessName == procName)
            .ToList();

        UnityEngine.Debug.Log($"✅ 선택됨: {displayName} ({procName}), {procGroup.Count}개 인스턴스");

        foreach (var p in procGroup)
            UnityEngine.Debug.Log($" - PID: {p.Id}");
    }
}
