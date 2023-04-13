using ClosedXML.Excel;
using Wada.AOP.Logging;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.MatchedEmployeeNumberAggregation;

namespace Wada.MatchedEmployeeNumberSpreadSheet;

public class MatchedEmployeeNumberSpreadSheetReader : IMatchedEmployeeNumberListReader
{
    [Logging]
    public async Task<IEnumerable<MatchedEmployeeNumber>> ReadAllAsync(Stream stream)
    {
        using var xlBook = new XLWorkbook(stream);
        var sheet = xlBook.Worksheet(1);

        var range = sheet.RangeUsed();

        // Rangeをジャグ配列にする
        var table = await Task.WhenAll(
            range.Rows()
                 .Select(async r => await Task.WhenAll(
                     r.Cells()
                      .Select(async c => await Task.Run(
                          () => c.IsEmpty() ? null : c.Value.ToString())))));

        if (table.Select(x => x.Length).First() < 2)
        {
            // 列数が2未満の場合はデータ異常
            throw new MatchedEmployeeNumberException(
                "取込可能なデータ形式ではありません\n" +
                "フォーマットを確認してください");
        }

        try
        {
            return table.Skip(1)
                        .Where(x => x[0] != null)
                        .Where(x => x[1] != null)
                        .Cast<string[]>()
                        .Select(x => new MatchedEmployeeNumber(
                            uint.Parse(x[0]),
                            uint.Parse(x[1])))
                        .ToList();
        }
        catch (FormatException ex)
        {
            throw new MatchedEmployeeNumberException(
                "データ中に数字以外の文字があります 数字のみにしてください", ex);
        }
    }
}