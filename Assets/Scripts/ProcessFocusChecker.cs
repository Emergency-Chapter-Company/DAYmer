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

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    /* ====== 입력 감시 ====== */
    private Vector2 lastMousePos;
    private float lastInputTime;
    [SerializeField, Range(0.1f, 5f)] private float inputActiveDuration = 1.0f; // 입력 유지시간 (초)

    /* ====== UI ====== */
    [Header("UI 표시")]
    [SerializeField] private Image focusIndicator; // 포커스 상태 불빛
    [SerializeField] private Image inputIndicator; // 입력 감지 불빛

    /* ====== 주요 로직 ====== */
    public void SetTargetProcess(Process proc)
    {
        targetProcess = proc;

        if (proc == null)
        {
            UnityEngine.Debug.LogWarning("⚠️ 대상 프로세스가 null입니다. 감시 불가.");
            SetIndicatorColor(focusIndicator, Color.gray);
            SetIndicatorColor(inputIndicator, Color.gray);
        }
        else
        {
            UnityEngine.Debug.Log($"🎯 프로세스 감시 시작: {proc.ProcessName} (PID {proc.Id})");
            SetIndicatorColor(focusIndicator, Color.gray);
            SetIndicatorColor(inputIndicator, Color.gray);
        }
    }

    private void Start()
    {
        GetCursorPos(out POINT p);
        lastMousePos = new Vector2(p.X, p.Y);
    }

    private void Update()
    {
        if (targetProcess == null || targetProcess.HasExited)
        {
            SetIndicatorColor(focusIndicator, Color.gray);
            SetIndicatorColor(inputIndicator, Color.gray);
            return;
        }

        // ✅ 포커스 상태 확인
        bool isFocused = IsProcessFocused(targetProcess);
        if (isFocused != lastFocusState)
        {
            lastFocusState = isFocused;

            UnityEngine.Debug.Log(isFocused
                ? $"🟢 '{targetProcess.ProcessName}' 창이 포커스됨!"
                : $"⚪ '{targetProcess.ProcessName}' 창이 포커스 해제됨.");

            SetIndicatorColor(focusIndicator, isFocused ? Color.green : Color.gray);
        }

        // ✅ 입력 감지 (키보드 + 마우스)
        if (DetectInput())
            lastInputTime = Time.time;

        bool inputActive = Time.time - lastInputTime < inputActiveDuration;
        SetIndicatorColor(inputIndicator, inputActive ? Color.yellow : Color.gray);
    }

    private bool IsProcessFocused(Process proc)
    {
        IntPtr hwnd = GetForegroundWindow();
        GetWindowThreadProcessId(hwnd, out uint activePID);
        return proc.Id == activePID;
    }

    private bool DetectInput()
    {
        // 키보드 입력 감지
        for (int i = 0; i < 255; i++)
        {
            if ((GetAsyncKeyState(i) & 0x8000) != 0)
                return true;
        }

        // 마우스 이동 감지
        GetCursorPos(out POINT pos);
        Vector2 currentMouse = new Vector2(pos.X, pos.Y);
        if (currentMouse != lastMousePos)
        {
            lastMousePos = currentMouse;
            return true;
        }

        return false;
    }

    private void SetIndicatorColor(Image img, Color color)
    {
        if (img != null)
            img.color = color;
    }
}
