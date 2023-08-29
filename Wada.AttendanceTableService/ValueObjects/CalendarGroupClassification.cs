using Wada.Extensions;

namespace Wada.AttendanceTableService.ValueObjects;

/// <summary>
/// 会社カレンダー グループ
/// </summary>
public enum CalendarGroupClassification
{
    /// <summary>
    /// 本社
    /// </summary>
    [EnumDisplayName("本社")]
    HeadOffice,

    /// <summary>
    /// 松阪
    /// </summary>
    [EnumDisplayName("松阪")]
    MatsuzakaOffice,
}