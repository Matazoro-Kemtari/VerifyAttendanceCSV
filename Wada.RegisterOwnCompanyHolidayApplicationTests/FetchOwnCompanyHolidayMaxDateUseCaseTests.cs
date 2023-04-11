using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.Data.DesignDepartmentDataBase.Models;

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
            holidayMock.Setup(x => x.FindByAfterYear(It.IsAny<string>(), It.IsAny<int>()));

            Mock<IEnvironment> envMock = new();
            var year = 2023;
            envMock.Setup(x => x.ObtainCurrentDate()).Returns(new DateTime(year, 1, 1));

            // when
            IFetchOwnCompanyHolidayMaxDateUseCase useCase =
                new FetchOwnCompanyHolidayMaxDateUseCase(configMock.Object, holidayMock.Object);

            // 実行日付を偽装する
            useCase.MimicEnvironment(envMock.Object);

            await useCase.ExecuteAsyc();

            // then
            configMock.Verify(x => x[It.IsAny<string>()], Times.Exactly(2));
            envMock.Verify(x => x.ObtainCurrentDate(), Times.Once);
            holidayMock.Verify(x => x.FindByAfterYear(headOfficeCalendarGroupId, year), Times.Once);
            holidayMock.Verify(x => x.FindByAfterYear(matsuzakaOfficeCalendarGroupId, year), Times.Once);
        }
    }
}