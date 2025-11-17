using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ProcessFocusChecker : MonoBehaviour
{
    /* ====== 감시 대상 프로세스 ====== */
    private Process targetProcess = null;   // 감시할 프로세스
    private bool lastFocusState = false;    // 마지막 포커스 상태
    private bool isProcessSelected = false; // 현재 포커스 상태

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
    private float lastActiveTime;
    [SerializeField, Range(0.1f, 5f)]
    private float inputActiveDuration = 1.0f;
    [SerializeField, Range(10f, 120f)]
    private float graceDuration = 10f; // 10초 유예 시간

    /* ====== UI ====== */
    [Header("UI 표시")]
    [SerializeField]
    private Image focusIndicator; // 포커스 상태 불빛
    [SerializeField]
    private Image inputIndicator; // 입력 감지 불빛
    [SerializeField]
    private Image workIndicator;  // 작업 상태 불빛 (추가)

    /* ====== 컴포넌트 ====== */
    [Header("컴포넌트")]
    [SerializeField]
    private TimerManager timerManager;

    /* ====== 상태 정의 ====== */
    private enum WorkState { Working, IdleGrace, NotWorking }
    private WorkState currentState = WorkState.NotWorking;

    /* ====== 주요 로직 ====== */
    public void SetTargetProcess(Process proc)
    {
        targetProcess = proc;

        if (proc == null)
        {
            UnityEngine.Debug.LogWarning("[ProcessFocusChecker] ⚠️ 대상 프로세스가 null입니다. 감시 불가.");
            SetIndicatorColor(focusIndicator, Color.gray);
            SetIndicatorColor(inputIndicator, Color.gray);
            SetIndicatorColor(workIndicator, Color.gray);
            isProcessSelected = false;
        }
        else
        {
            UnityEngine.Debug.Log($"[ProcessFocusChecker] 🎯 프로세스 감시 시작: {proc.ProcessName} (PID {proc.Id})");
            SetIndicatorColor(focusIndicator, Color.gray);
            SetIndicatorColor(inputIndicator, Color.gray);
            SetIndicatorColor(workIndicator, Color.gray);
            isProcessSelected = true;
        }

        // 타이머 버튼 상태 즉시 갱신
        if (timerManager != null)
            timerManager.ForceRefreshButtons();
    }

    private void Start()
    {
        GetCursorPos(out POINT p);
        lastMousePos = new Vector2(p.X, p.Y);
        lastInputTime = Time.time;
        lastActiveTime = Time.time;
    }

    private void Update()
    {
        if (targetProcess == null || targetProcess.HasExited)
        {
            SetAllGray();
            return;
        }

        // ✅ 포커스 상태 확인
        bool isFocused = IsProcessFocused(targetProcess);
        if (isFocused != lastFocusState)
        {
            lastFocusState = isFocused;
            UnityEngine.Debug.Log(isFocused
                ? $"[ProcessFocusChecker] 🟢 '{targetProcess.ProcessName}' 창이 포커스됨!"
                : $"[ProcessFocusChecker] ⚪ '{targetProcess.ProcessName}' 창이 포커스 해제됨.");

            SetIndicatorColor(focusIndicator, isFocused ? Color.cyan : Color.gray);
        }

        // ✅ 입력 감지
        bool inputDetected = DetectInput();
        if (inputDetected)
            lastInputTime = Time.time;

        bool inputActive = Time.time - lastInputTime < inputActiveDuration;
        SetIndicatorColor(inputIndicator, inputActive ? Color.cyan : Color.gray);

        // ✅ 활동 상태 판정
        UpdateWorkState(isFocused, inputActive);
    }

    /* ====== 상태 판정 ====== */
    private void UpdateWorkState(bool isFocused, bool inputActive)
    {
        float now = Time.time;

        if (isFocused && inputActive)
        {
            // 실제 작업 중
            lastActiveTime = now;
            SetWorkState(WorkState.Working);

            if (timerManager != null)
                timerManager.AutoResume();  // 자동 재개
            else
                UnityEngine.Debug.LogWarning("[ProcessFocusChecker] ⚠️ TimerManager 컴포넌트가 할당되지 않음.");
        }
        else if (now - lastActiveTime < graceDuration)
        {
            // 1분 유예 상태
            SetWorkState(WorkState.IdleGrace);
        }
        else
        {
            // 완전히 비활동
            SetWorkState(WorkState.NotWorking);

            if (timerManager != null)
                timerManager.AutoPause();   // 자동 정지
            else
                UnityEngine.Debug.LogWarning("[ProcessFocusChecker] ⚠️ TimerManager 컴포넌트가 할당되지 않음.");
        }
    }

    private void SetWorkState(WorkState newState)
    {
        if (newState == currentState) return;

        currentState = newState;

        switch (newState)
        {
            case WorkState.Working:
                SetIndicatorColor(workIndicator, Color.green); // 작업 중 → 파랑
                UnityEngine.Debug.Log("[ProcessFocusChecker] 💼 작업 중");
                break;
            case WorkState.IdleGrace:
                SetIndicatorColor(workIndicator, Color.yellow); // 유예 상태 → 노랑
                UnityEngine.Debug.Log($"[ProcessFocusChecker] ⏳ 유예 상태 ({graceDuration}초 이내 비활동)");
                break;
            case WorkState.NotWorking:
                SetIndicatorColor(workIndicator, Color.red); // 비활동 → 빨강
                UnityEngine.Debug.Log($"[ProcessFocusChecker] 🟥 작업 안 함 ({graceDuration}초 이상 비활동)");
                break;
        }
    }

    /* ====== WinAPI 유틸 ====== */
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

    /* ====== UI 유틸 ====== */
    private void SetIndicatorColor(Image img, Color color)
    {
        if (img != null)
            img.color = color;
    }

    private void SetAllGray()
    {
        SetIndicatorColor(focusIndicator, Color.gray);
        SetIndicatorColor(inputIndicator, Color.gray);
        SetIndicatorColor(workIndicator, Color.gray);
    }

    /* ====== 상태 조회 ====== */
    public bool GetIsProcessSelected()
    {
        return isProcessSelected;
    }
}
