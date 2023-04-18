using ClosedXML.Excel;
using Wada.AOP.Logging;
using Wada.AttendanceTableService;
using Wada.Data.DesignDepartmentDataBase.Models.OwnCompanyCalendarAggregation;
using Wada.Data.DesignDepartmentDataBase.Models.ValueObjects;

namespace Wada.OwnCompanyHolidaySpreadSheet;

public class OwnCompanyHolidayListReader : IOwnCompanyHolidayListReader
{
    [Logging]
    public async Task<IEnumerable<OwnCompanyHoliday>> ReadAllAsync(Stream stream, string calendarGroupId)
    {
        string?[][] table;
        try
        {
            using var xlBook = new XLWorkbook(stream);
            var sheet = xlBook.Worksheet(1);

            var range = sheet.RangeUsed();

            // Rangeをジャグ配列にする
            table = await Task.WhenAll(
                range.Rows()
                     .Select(async r => await Task.WhenAll(
                         r.Cells()
                          .Select(async c => await Task.Run(
                              () => c.IsEmpty() ? null : c.Value.ToString())))));
        }
        catch (FormatException ex)
        {
            throw new OwnCompanyCalendarAggregationException(
                "取込可能なファイル形式ではありません\nファイルが壊れている可能性があります", ex);
        }

        if (table.Select(x => x.Length).First() < 2)
        {
            // 列数が2未満の場合はデータ異常
            throw new OwnCompanyCalendarAggregationException(
                "取込可能なデータ形式ではありません\n" +
                "フォーマットを確認してください");
        }

        try
        {
            return table.Skip(1)
                        .Where(x => x[0] != null)
                        .Cast<string[]>()
                        .Select(x => OwnCompanyHoliday.Create(
                            calendarGroupId,
                            DateTime.Parse(x[0]),
                            x[1] switch
                            {
                                "1" => HolidayClassification.LegalHoliday,
                                "0" or null => HolidayClassification.RegularHoliday,
                                _ => throw new FormatException(),
                            }))
                        .ToList();
        }
        catch (FormatException ex)
        {
            throw new OwnCompanyCalendarAggregationException(
                "データ中に日付または数字以外の文字があります 日付と数字にしてください", ex);
        }
    }
}