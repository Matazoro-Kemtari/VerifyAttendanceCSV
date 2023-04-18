using ClosedXML.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.Data.DesignDepartmentDataBase.Models.OwnCompanyCalendarAggregation;
using Wada.Data.DesignDepartmentDataBase.Models.ValueObjects;

namespace Wada.OwnCompanyHolidaySpreadSheet.Tests
{
    [TestClass()]
    public class OwnCompanyHolidayListReaderTests
    {
        private readonly string _calendarGroupName = "本社";
        private readonly string _calendarGroupID = "01GW8E3ENDPWX0FXW0788VR63J";

        [TestMethod()]
        public async Task 正常系_休日カレンダーが読み込めること()
        {
            // given
            // テスト用ブックを作る
            using var stream = new MemoryStream();
            CreateTestXlsxBook(正常系テストデータ, stream);

            // when
            var reader = new OwnCompanyHolidayListReader();
            var actual = await reader.ReadAllAsync(stream, _calendarGroupName);

            CollectionAssert.AreEqual(
                正常系テストデータ.Select(x => OwnCompanyHoliday.Reconstruct(
                    _calendarGroupID,
                    (DateTime)x[0],
                    x[1] switch
                    {
                        int and 1 => HolidayClassification.LegalHoliday,
                        int and 0 => HolidayClassification.RegularHoliday,
                        string and "" => HolidayClassification.RegularHoliday,
                        _ => throw new FormatException(),
                    }))
                .ToList(),
                actual.ToList());
        }

        [TestMethod]
        public async Task 異常系_空のファイルを渡すと例外を返すこと()
        {
            // given
            // テスト用ブックを作る
            using var stream = new MemoryStream();

            // when
            var reader = new OwnCompanyHolidayListReader();
            Task target() => _ = reader.ReadAllAsync(stream, _calendarGroupName);

            var ex = await Assert.ThrowsExceptionAsync<OwnCompanyCalendarAggregationException>(target);
            var message = "取込可能なファイル形式ではありません\nファイルが壊れている可能性があります";
            Assert.AreEqual(message, ex.Message);
        }

        [TestMethod()]
        public async Task 異常系_テストデータに文字列が混ざっているとき例外を返すこと()
        {
            // given
            // テスト用ブックを作る
            using var stream = new MemoryStream();
            CreateTestXlsxBook(異常系テストデータ, stream);

            // when
            var reader = new OwnCompanyHolidayListReader();
            Task target() => _ = reader.ReadAllAsync(stream, _calendarGroupName);

            var ex = await Assert.ThrowsExceptionAsync<OwnCompanyCalendarAggregationException>(target);
            var message = "データ中に日付または数字以外の文字があります 日付と数字にしてください";
            Assert.AreEqual(message, ex.Message);
        }

        private static void CreateTestXlsxBook(IEnumerable<object[]> testData, MemoryStream stream)
        {
            using (var xlBook = new XLWorkbook())
            {
                var sheet = xlBook.AddWorksheet();
                // ヘッダ
                sheet.Cell("A1").InsertData(new[] { new[] { "休日", "法定休日の場合は1" } });
                // データ
                sheet.Cell("A2").InsertData(
                    testData.Select(
                        x => new[] { x[0], x[1] }));
                xlBook.SaveAs(stream);
            }
        }

        #region テストデータ
        private static IEnumerable<object[]> 正常系テストデータ => new[]
        {
            new object[] { DateTime.Parse("2023-01-01"), 1 },
            new object[] { DateTime.Parse("2023-02-11"), "" },
            new object[] { DateTime.Parse("2023-04-17"), 0 },
            new object[] { DateTime.Parse("2023-04-18"), 1 },
            new object[] { DateTime.Parse("2023-04-19"), 0 },
            new object[] { DateTime.Parse("2023-04-29"), 1 },
            new object[] { DateTime.Parse("2023-05-03"), 0 },
            new object[] { DateTime.Parse("2023-06-20"), 1 },
            new object[] { DateTime.Parse("2023-08-11"), 0 },
            new object[] { DateTime.Parse("2023-12-31"), 1 },
        };

        private static IEnumerable<object[]> 異常系テストデータ
            => 正常系テストデータ.Take(5)
                                 .Concat(new[] { new object[] { "文字", "文字" } })
                                 .Concat(正常系テストデータ.Skip(5))
                                 .ToArray();
        #endregion
    }
}