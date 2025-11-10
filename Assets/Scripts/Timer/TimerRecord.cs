using System;

/// 기록된 시간 정보를 저장하는 클래스
[Serializable]
public class TimeRecord
{
    private string recordTime;      // "00:00:00" 형식으로 표시되는 시간
    private DateTime recordDate;    // 기록된 날짜 및 시간
    private float totalSeconds;     // 총 경과 시간(초 단위)

    public TimeRecord(float seconds)
    {
        totalSeconds = seconds;
        recordTime = FormatTime(seconds);
        recordDate = DateTime.Now;
    }

    // 초를 00:00:00 형식으로 변환
    private string FormatTime(float seconds)
    {
        int hours = (int)(seconds / 3600);
        int minutes = (int)((seconds % 3600) / 60);
        int secs = (int)(seconds % 60);

        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, secs);
    }

    // 기록 정보를 문자열로 변환
    public override string ToString()
    {
        return $"{recordTime} - {recordDate:yyyy/MM/dd HH:mm:ss}";
    }

    // 기록된 시간 문자열 가져오기
    public string GetRecordTime()
    {
        return recordTime;
    }

    // 기록된 시간 초 가져오기
    public float GetTotalSeconds()
    {
        return totalSeconds;
    }
}
