using System;

/// 타이머 시간 기록을 저장
[Serializable]
public class TimeRecord
{
    public string recordTime;      // "00:00:00" 형식의 기록 시간
    public DateTime recordDate;    // 기록된 날짜 및 시간
    public float totalSeconds;     // 총 초 단위 시간

    public TimeRecord(float seconds)
    {
        totalSeconds = seconds;
        recordTime = FormatTime(seconds);
        recordDate = DateTime.Now;
    }

    /// 초를 00:00:00 형식으로 변환
    private string FormatTime(float seconds)
    {
        int hours = (int)(seconds / 3600);
        int minutes = (int)((seconds % 3600) / 60);
        int secs = (int)(seconds % 60);

        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, secs);
    }

    /// 기록 정보를 문자열로 반환
    public override string ToString()
    {
        return $"{recordTime} - {recordDate:yyyy/MM/dd HH:mm:ss}";
    }
}