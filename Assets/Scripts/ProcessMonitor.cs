using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProcessMonitor : MonoBehaviour
{
    private List<Process> allProcesses; // 모든 프로세스 목록
    private List<string> displayNames; // 드롭다운에 표시할 이름 목록
    private Dictionary<string, string> nameToProcName; // 표시 이름 -> 프로세스 이름 매핑

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(System.IntPtr hWnd); // 창이 보이는지 확인하는 WinAPI 함수

    [Header("UI")]
    [SerializeField]
    private TMP_Dropdown dropdown;

    void Start()
    {
        RefreshProcessList();
        dropdown.onValueChanged.AddListener(OnSelectProcess);
    }

    // TMP_Dropdown 클릭 시 자동 호출됨
    public void OnPointerClick(PointerEventData eventData)
    {
        RefreshProcessList();
    }

    void RefreshProcessList()
    {
        UnityEngine.Debug.Log("🔄 Process list refreshed!");

        allProcesses = Process.GetProcesses().ToList();

        displayNames = new List<string>();
        nameToProcName = new Dictionary<string, string>();

        var windowProcs = allProcesses
            .Where(p => p.MainWindowHandle != System.IntPtr.Zero && IsWindowVisible(p.MainWindowHandle))
            .ToList();

        foreach (var p in windowProcs)
        {
            string procName = p.ProcessName;
            string displayName = GetDisplayName(p);

            if (!displayNames.Contains(displayName))
            {
                displayNames.Add(displayName);
                nameToProcName[displayName] = procName;
            }
        }

        displayNames = displayNames.OrderBy(n => n).ToList();

        dropdown.ClearOptions();
        dropdown.AddOptions(displayNames);
    }

    private string GetDisplayName(Process p)
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

    private void OnSelectProcess(int index)
    {
        if (index < 0 || index >= displayNames.Count)
            return;

        string displayName = displayNames[index];
        string procName = nameToProcName[displayName];

        var validProcs = allProcesses
            .Where(p => p.ProcessName == procName &&
                        p.MainWindowHandle != System.IntPtr.Zero &&
                        IsWindowVisible(p.MainWindowHandle))
            .ToList();

        UnityEngine.Debug.Log($"✅ 선택된 프로그램 표시명: {displayName} / 내부 프로세스명: {procName}");
        UnityEngine.Debug.Log($"✅ 창이 있는 PID 개수: {validProcs.Count}");

        foreach (var p in validProcs)
        {
            UnityEngine.Debug.Log($" - PID {p.Id}, Handle: {p.MainWindowHandle}");
        }
    }
}
