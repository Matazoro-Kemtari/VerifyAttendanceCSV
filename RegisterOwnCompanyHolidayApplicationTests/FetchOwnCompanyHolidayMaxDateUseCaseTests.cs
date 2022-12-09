﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using Wada.AttendanceTableService;

namespace RegisterOwnCompanyHolidayApplication.Tests
{
    [TestClass()]
    public class FetchOwnCompanyHolidayMaxDateUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ユーズケースを実行するとリポジトリが実行されること()
        {
            // given
            // ロガーモック
            Mock<ILogger> mock_logger = new();

            Mock<IOwnCompanyHolidayRepository> mock_holiday = new();
            mock_holiday.Setup(x => x.MaxDate()).Returns(DateTime.MaxValue);

            // when
            IFetchOwnCompanyHolidayMaxDateUseCase registerOwnCompanyHolidayApplication =
                new FetchOwnCompanyHolidayMaxDateUseCase(mock_logger.Object, mock_holiday.Object);
            DateTime maxDate = await registerOwnCompanyHolidayApplication.ExecuteAsyc();

            // then
            Assert.AreEqual(DateTime.MaxValue, maxDate);
            mock_holiday.Verify(x=>x.MaxDate(), Times.Once);
        }
    }
}