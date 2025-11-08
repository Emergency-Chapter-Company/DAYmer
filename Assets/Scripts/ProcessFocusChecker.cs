using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ProcessFocusChecker : MonoBehaviour
{
    /* ====== 감시 대상 프로세스 ====== */
    private Process targetProcess; // 감시할 프로세스
    private bool lastFocusState = false;

    /* ====== WinAPI ====== */
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    /* ====== UI ====== */
    [Header("UI")]
    [SerializeField] private Image focusIndicator;

    /* ====== 주요 로직 ====== */
    public void SetTargetProcess(Process proc)
    {
        targetProcess = proc;

        if (proc == null)
        {
            UnityEngine.Debug.LogWarning("⚠️ 대상 프로세스가 null입니다. 감시 불가.");
            SetIndicatorColor(Color.gray);
        }
        else
        {
            UnityEngine.Debug.Log($"🎯 프로세스 감시 시작: {proc.ProcessName} (PID {proc.Id})");
            SetIndicatorColor(Color.gray);
        }
    }

    private void Update()
    {
        if (targetProcess == null || targetProcess.HasExited)
        {
            SetIndicatorColor(Color.gray);
            return;
        }

        bool isFocused = IsProcessFocused(targetProcess);
        if (isFocused != lastFocusState)
        {
            lastFocusState = isFocused;

            UnityEngine.Debug.Log(isFocused
                ? $"🟢 '{targetProcess.ProcessName}' 창이 포커스됨!"
                : $"⚪ '{targetProcess.ProcessName}' 창이 포커스 해제됨.");

            SetIndicatorColor(isFocused ? Color.green : Color.gray);
        }
    }

    private bool IsProcessFocused(Process proc)
    {
        IntPtr hwnd = GetForegroundWindow();
        GetWindowThreadProcessId(hwnd, out uint activePID);
        return proc.Id == activePID;
    }

    private void SetIndicatorColor(Color c)
    {
        if (focusIndicator != null)
            focusIndicator.color = c;
    }
}
