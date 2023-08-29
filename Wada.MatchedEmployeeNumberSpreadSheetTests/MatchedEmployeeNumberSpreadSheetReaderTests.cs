using ClosedXML.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.MatchedEmployeeNumberAggregation;

namespace Wada.MatchedEmployeeNumberSpreadSheet.Tests
{
    [TestClass()]
    public class MatchedEmployeeNumberSpreadSheetReaderTests
    {
        [TestMethod()]
        public async Task 正常系_社員番号対応表が読み込めること()
        {
            // given
            // テスト用ブックを作る
            using var stream = new MemoryStream();
            CreateTestXlsxBook(正常系テストデータ, stream);

            // when
            IMatchedEmployeeNumberListReader reader = new MatchedEmployeeNumberSpreadSheetReader();
            var actual = await reader.ReadAllAsync(stream);

            CollectionAssert.AreEqual(
                正常系テストデータ.Select(x => MatchedEmployeeNumber.Reconstruct((uint)x[0], (uint)x[1])).ToList(),
                actual.ToList());
        }

        [TestMethod]
        public async Task 異常系_空のファイルを渡すと例外を返すこと()
        {
            // given
            // テスト用ブックを作る
            using var stream = new MemoryStream();

            // when
            IMatchedEmployeeNumberListReader reader = new MatchedEmployeeNumberSpreadSheetReader();
            Task target() => _ = reader.ReadAllAsync(stream!);

            var ex = await Assert.ThrowsExceptionAsync<MatchedEmployeeNumberAggregationException>(target);
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
            IMatchedEmployeeNumberListReader reader = new MatchedEmployeeNumberSpreadSheetReader();
            Task target() => _ = reader.ReadAllAsync(stream!);

            var ex = await Assert.ThrowsExceptionAsync<MatchedEmployeeNumberAggregationException>(target);
            var message = "データ中に数字以外の文字があります 数字のみにしてください";
            Assert.AreEqual(message, ex.Message);
        }

        private static void CreateTestXlsxBook(IEnumerable<object[]> testData, MemoryStream stream)
        {
            using var xlBook = new XLWorkbook();
            var sheet = xlBook.AddWorksheet();
            // ヘッダ
            sheet.Cell("A1").InsertData(new[] { new[] { "受注管理", "勤怠" } });
            // データ
            sheet.Cell("A2").InsertData(
                testData.Select(
                    x => new[] { x[0], x[1] }));
            xlBook.SaveAs(stream);
        }

        #region テストデータ
        private static IEnumerable<object[]> 正常系テストデータ => new[]
        {
            new object[] {1797u, 127u},
            new object[] {1326u, 553u},
            new object[] {1104u, 921u},
            new object[] {1291u, 794u},
            new object[] {1364u, 514u},
            new object[] {1812u, 964u},
            new object[] {1376u, 25u},
            new object[] {1081u, 1247u},
            new object[] {1468u, 583u},
            new object[] {1427u, 405u},
            new object[] {1654u, 2u},
            new object[] {1069u, 326u},
            new object[] {1222u, 251u},
            new object[] {1477u, 2196u},
            new object[] {1489u, 642u},
            new object[] {1366u, 747u},
            new object[] {1462u, 234u},
            new object[] {1666u, 7668u},
            new object[] {1355u, 372u},
            new object[] {1762u, 435u},
            new object[] {1075u, 947u},
            new object[] {1966u, 6737u},
            new object[] {1578u, 36u},
            new object[] {1937u, 94u},
            new object[] {1204u, 1106u},
            new object[] {1054u, 4u},
            new object[] {1652u, 4719u},
            new object[] {1382u, 509u},
            new object[] {1117u, 797u},
            new object[] {1101u, 584u},
            new object[] {1308u, 836u},
            new object[] {1342u, 181u},
            new object[] {1097u, 609u},
            new object[] {1684u, 67u},
            new object[] {1209u, 1758u},
            new object[] {1422u, 2953u},
            new object[] {1448u, 6244u},
            new object[] {1072u, 620u},
            new object[] {1650u, 253u},
            new object[] {1068u, 146u},
            new object[] {1179u, 46u},
            new object[] {1680u, 224u},
            new object[] {1582u, 73u},
            new object[] {1845u, 2647u},
            new object[] {1443u, 13u},
            new object[] {1377u, 603u},
            new object[] {1775u, 1527u},
            new object[] {1313u, 2595u},
            new object[] {1765u, 682u},
            new object[] {1629u, 786u},
            new object[] {1538u, 984u},
            new object[] {1665u, 11u},
        };

        private static IEnumerable<object[]> 異常系テストデータ
            => 正常系テストデータ.Take(5)
                                 .Concat(new[] { new object[] { "文字", "文字" } })
                                 .Concat(正常系テストデータ.Skip(5))
                                 .ToArray();
        #endregion
    }
}