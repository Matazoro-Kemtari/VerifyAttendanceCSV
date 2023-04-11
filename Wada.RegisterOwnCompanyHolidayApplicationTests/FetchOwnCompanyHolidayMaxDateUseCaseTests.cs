using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections;
using Wada.Data.DesignDepartmentDataBase.Models;
using Wada.Data.DesignDepartmentDataBase.Models.OwnCompanyCalendarAggregation;

namespace Wada.RegisterOwnCompanyHolidayApplication.Tests
{
    [TestClass()]
    public class FetchOwnCompanyHolidayMaxDateUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ユースケースを実行するとリポジトリが実行されること()
        {
            // given
            Mock<IConfiguration> configMock = new();
            var headOfficeCalendarGroupId = "A";
            configMock.Setup(x => x["applicationConfiguration:HeadOfficeCalendarGroupId"])
                .Returns(headOfficeCalendarGroupId);
            var matsuzakaOfficeCalendarGroupId = "B";
            configMock.Setup(x => x["applicationConfiguration:MatsuzakaOfficeCalendarGroupId"])
                .Returns(matsuzakaOfficeCalendarGroupId);

            Mock<IOwnCompanyHolidayRepository> holidayMock = new();
            var year = 2023;
            holidayMock.Setup(x => x.FindByAfterYearAsync(headOfficeCalendarGroupId, It.IsAny<int>()))
                .ReturnsAsync(new List<OwnCompanyHoliday> 
                { TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(year, 3, 1)) });
            var maxDate = new DateTime(year, 3, 25);
            holidayMock.Setup(x => x.FindByAfterYearAsync(matsuzakaOfficeCalendarGroupId, It.IsAny<int>()))
                .ReturnsAsync(new List<OwnCompanyHoliday>
                { TestOwnCompanyHolidayFactory.Create(holidayDate: maxDate) });

            Mock<IEnvironment> envMock = new();
            envMock.Setup(x => x.ObtainCurrentDate()).Returns(new DateTime(year, 1, 1));

            // when
            IFetchOwnCompanyHolidayMaxDateUseCase useCase =
                new FetchOwnCompanyHolidayMaxDateUseCase(configMock.Object, holidayMock.Object);

            // 実行日付を偽装する
            useCase.MimicEnvironment(envMock.Object);

            var actual = await useCase.ExecuteAsyc();

            // then
            configMock.Verify(x => x[It.IsAny<string>()], Times.Exactly(2));
            envMock.Verify(x => x.ObtainCurrentDate(), Times.Once);
            holidayMock.Verify(x => x.FindByAfterYearAsync(headOfficeCalendarGroupId, year), Times.Once);
            holidayMock.Verify(x => x.FindByAfterYearAsync(matsuzakaOfficeCalendarGroupId, year), Times.Once);
            Assert.AreEqual(maxDate, actual);
        }
    }
}