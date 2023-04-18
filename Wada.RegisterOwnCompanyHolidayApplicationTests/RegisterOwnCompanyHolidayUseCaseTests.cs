using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.AttendanceTableService;
using Wada.Data.DesignDepartmentDataBase.Models;
using Wada.Data.DesignDepartmentDataBase.Models.OwnCompanyCalendarAggregation;
using Wada.Data.DesignDepartmentDataBase.Models.ValueObjects;

namespace Wada.RegisterOwnCompanyHolidayApplication.Tests
{
    [TestClass()]
    public class RegisterOwnCompanyHolidayUseCaseTests
    {
        [TestMethod]
        public async Task 正常系_DIしたオブジェクトが実行されること()
        {
            // given
            var filePath = "test.xlsx";
            var stream = new MemoryStream();
            var allHolidays = new List<OwnCompanyHoliday>
            {
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 1, 1), holidayClassification: HolidayClassification.LegalHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 1, 2), holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 2, 1), holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 2, 2), holidayClassification: HolidayClassification.LegalHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 3, 1), holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 3, 2), holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 4, 1), holidayClassification: HolidayClassification.LegalHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 4, 2), holidayClassification: HolidayClassification.LegalHoliday ),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 5, 1), holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 5, 2), holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 6, 1), holidayClassification: HolidayClassification.LegalHoliday ),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 6, 2), holidayClassification: HolidayClassification.RegularHoliday),
            };
            var additionalHolidays = new List<OwnCompanyHoliday>
            {
                TestOwnCompanyHolidayFactory.Create(holidayDate: DateTime.Parse("2023/1/8"),holidayClassification: HolidayClassification.LegalHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: DateTime.Parse("2023/2/9"),holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: DateTime.Parse("2023/3/14"),holidayClassification: HolidayClassification.RegularHoliday),
            };
            var deletableHolidays = new List<OwnCompanyHoliday>
            {
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 1, 1), holidayClassification: HolidayClassification.LegalHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 1, 2), holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 2, 1), holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 2, 2), holidayClassification: HolidayClassification.LegalHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 3, 1), holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 3, 2), holidayClassification: HolidayClassification.RegularHoliday),
            };

            Mock<IFileStreamOpener> streamMock = new();
            streamMock.Setup(x => x.OpenAsync(filePath)).ReturnsAsync(stream);

            Mock<IOwnCompanyHolidayListReader> readerMock = new();
            readerMock.Setup(x => x.ReadAllAsync(stream, additionalHolidays[0].CalendarGroupId)).ReturnsAsync(additionalHolidays);

            Mock<IOwnCompanyHolidayRepository> repositoryMock = new();
            repositoryMock.Setup(x => x.FindCalendarGroupIdAsync(It.IsAny<CalendarGroupClassification>())).ReturnsAsync(additionalHolidays[0].CalendarGroupId);
            repositoryMock.Setup(x => x.FindByAfterDateAsync(allHolidays[0].CalendarGroupId, DateTime.Parse("2023/1/1"))).ReturnsAsync(allHolidays);

            // when
            var useCase = new RegisterOwnCompanyHolidayUseCase(
                streamMock.Object,
                readerMock.Object,
                repositoryMock.Object);
            await useCase.ExecuteAsync(filePath, CalendarGroupAttempt.HeadOffice);

            // then
            repositoryMock.Verify(x => x.FindCalendarGroupIdAsync(It.IsAny<CalendarGroupClassification>()), Times.Once);
            streamMock.Verify(x => x.OpenAsync(filePath), Times.Once);
            readerMock.Verify(x => x.ReadAllAsync(stream, allHolidays[0].CalendarGroupId), Times.Once);
            repositoryMock.Verify(
                x => x.FindByAfterDateAsync(allHolidays[0].CalendarGroupId, DateTime.Parse("2023/1/1")), Times.Once);
            repositoryMock.Verify(
                x => x.RemoveRangeAsync(deletableHolidays), Times.Once);
            repositoryMock.Verify(
                x => x.AddRangeAsync(additionalHolidays),
                Times.Once);
        }

        [TestMethod]
        public async Task 正常系_削除対象がない場合削除せずに追加が実行されること()
        {
            // given
            var filePath = "test.xlsx";
            var stream = new MemoryStream();
            var allHolidays = new List<OwnCompanyHoliday>
            {
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 1, 1), holidayClassification: HolidayClassification.LegalHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 1, 2), holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 2, 1), holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 2, 2), holidayClassification: HolidayClassification.LegalHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 3, 1), holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 3, 2), holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 4, 1), holidayClassification: HolidayClassification.LegalHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 4, 2), holidayClassification: HolidayClassification.LegalHoliday ),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 5, 1), holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 5, 2), holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 6, 1), holidayClassification: HolidayClassification.LegalHoliday ),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 6, 2), holidayClassification: HolidayClassification.RegularHoliday),
            };
            var additionalHolidays = new List<OwnCompanyHoliday>
            {
                TestOwnCompanyHolidayFactory.Create(holidayDate: DateTime.Parse("2023/1/8"),holidayClassification: HolidayClassification.LegalHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: DateTime.Parse("2023/2/9"),holidayClassification: HolidayClassification.RegularHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: DateTime.Parse("2023/3/14"),holidayClassification: HolidayClassification.RegularHoliday),
            };

            Mock<IFileStreamOpener> streamMock = new();
            streamMock.Setup(x => x.OpenAsync(filePath)).ReturnsAsync(stream);

            Mock<IOwnCompanyHolidayListReader> readerMock = new();
            readerMock.Setup(x => x.ReadAllAsync(stream, additionalHolidays[0].CalendarGroupId)).ReturnsAsync(additionalHolidays);

            Mock<IOwnCompanyHolidayRepository> repositoryMock = new();
            repositoryMock.Setup(x => x.FindCalendarGroupIdAsync(It.IsAny<CalendarGroupClassification>())).ReturnsAsync(additionalHolidays[0].CalendarGroupId);
            repositoryMock.Setup(x => x.FindByAfterDateAsync(allHolidays[0].CalendarGroupId, DateTime.Parse("2023/1/1")))
                .ReturnsAsync(Array.Empty<OwnCompanyHoliday>());

            // when
            var useCase = new RegisterOwnCompanyHolidayUseCase(
                streamMock.Object,
                readerMock.Object,
                repositoryMock.Object);
            await useCase.ExecuteAsync(filePath, CalendarGroupAttempt.HeadOffice);

            // then
            repositoryMock.Verify(x => x.FindCalendarGroupIdAsync(It.IsAny<CalendarGroupClassification>()), Times.Once);
            streamMock.Verify(x => x.OpenAsync(filePath), Times.Once);
            readerMock.Verify(x => x.ReadAllAsync(stream, additionalHolidays[0].CalendarGroupId), Times.Once);
            repositoryMock.Verify(
                x => x.FindByAfterDateAsync(allHolidays[0].CalendarGroupId, DateTime.Parse("2023/1/1")), Times.Once);
            repositoryMock.Verify(
                x => x.RemoveRangeAsync(It.IsAny<IEnumerable<OwnCompanyHoliday>>()), Times.Never);
            repositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.IsAny<IEnumerable<OwnCompanyHoliday>>()),
                    Times.Once);
        }

        [TestMethod]
        public async Task 異常系_ファイルが存在しない場合例外を返すこと()
        {
            // given
            var filePath = "test.xlsx";
            var stream = new MemoryStream();
            var ownCompanyHolidays = Array.Empty<OwnCompanyHoliday>();

            Mock<IFileStreamOpener> streamMock = new();
            streamMock.Setup(x => x.OpenAsync(filePath)).ThrowsAsync(new DomainException());

            Mock<IOwnCompanyHolidayListReader> readerMock = new();
            Mock<IOwnCompanyHolidayRepository> repositoryMock = new();

            // when
            var useCase = new RegisterOwnCompanyHolidayUseCase(
                streamMock.Object,
                readerMock.Object,
                repositoryMock.Object);
            Task target() => useCase.ExecuteAsync(filePath, CalendarGroupAttempt.HeadOffice);

            // then
            _ = await Assert.ThrowsExceptionAsync<UseCaseException>(target);
            repositoryMock.Verify(x => x.FindCalendarGroupIdAsync(It.IsAny<CalendarGroupClassification>()), Times.Once);
            streamMock.Verify(x => x.OpenAsync(filePath), Times.Once);
            readerMock.Verify(x => x.ReadAllAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
            repositoryMock.Verify(
                x => x.RemoveRangeAsync(
                    It.IsAny<IEnumerable<OwnCompanyHoliday>>()),
                Times.Never);
            repositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.IsAny<IEnumerable<OwnCompanyHoliday>>()),
                    Times.Never);
        }

        [TestMethod]
        public async Task 異常系_不正なファイル形式の場合例外を返すこと()
        {
            // given
            var filePath = "test.xlsx";
            var stream = new MemoryStream();
            var ownCompanyHolidays = Array.Empty<OwnCompanyHoliday>();

            Mock<IFileStreamOpener> streamMock = new();
            streamMock.Setup(x => x.OpenAsync(filePath)).ReturnsAsync(stream);

            Mock<IOwnCompanyHolidayListReader> readerMock = new();
            readerMock.Setup(x => x.ReadAllAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ThrowsAsync(new DomainException());

            Mock<IOwnCompanyHolidayRepository> repositoryMock = new();

            // when
            var useCase = new RegisterOwnCompanyHolidayUseCase(
                streamMock.Object,
                readerMock.Object,
                repositoryMock.Object);
            Task target() => useCase.ExecuteAsync(filePath, CalendarGroupAttempt.HeadOffice);

            // then
            _ = await Assert.ThrowsExceptionAsync<UseCaseException>(target);
            repositoryMock.Verify(x => x.FindCalendarGroupIdAsync(It.IsAny<CalendarGroupClassification>()), Times.Once);
            streamMock.Verify(x => x.OpenAsync(filePath), Times.Once);
            readerMock.Verify(x => x.ReadAllAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
            repositoryMock.Verify(
                x => x.RemoveRangeAsync(
                    It.IsAny<IEnumerable<OwnCompanyHoliday>>()),
                Times.Never);
            repositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.IsAny<IEnumerable<OwnCompanyHoliday>>()),
                    Times.Never);
        }

        [TestMethod]
        public async Task 異常系_データベース削除に失敗した場合例外を返すこと()
        {
            // given
            var filePath = "test.xlsx";
            var stream = new MemoryStream();
            var allHolidays = new List<OwnCompanyHoliday>
            {
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 1, 1), holidayClassification: HolidayClassification.LegalHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 1, 2), holidayClassification: HolidayClassification.RegularHoliday),
            };
            var additionalHolidays = new List<OwnCompanyHoliday>
            {
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 1, 1), holidayClassification: HolidayClassification.LegalHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 1, 2), holidayClassification: HolidayClassification.RegularHoliday),
            };

            Mock<IFileStreamOpener> streamMock = new();
            streamMock.Setup(x => x.OpenAsync(filePath)).ReturnsAsync(stream);

            Mock<IOwnCompanyHolidayListReader> readerMock = new();
            readerMock.Setup(x => x.ReadAllAsync(It.IsAny<Stream>(), It.IsAny<string>())).ReturnsAsync(additionalHolidays);

            Mock<IOwnCompanyHolidayRepository> repositoryMock = new();
            repositoryMock.Setup(x => x.FindByAfterDateAsync(It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(allHolidays);
            repositoryMock.Setup(x => x.RemoveRangeAsync(It.IsAny<IEnumerable<OwnCompanyHoliday>>()))
                .ThrowsAsync(new DomainException());

            // when
            var useCase = new RegisterOwnCompanyHolidayUseCase(
                streamMock.Object,
                readerMock.Object,
                repositoryMock.Object);
            Task target() => useCase.ExecuteAsync(filePath, CalendarGroupAttempt.HeadOffice);

            // then
            _ = await Assert.ThrowsExceptionAsync<UseCaseException>(target);
            repositoryMock.Verify(x => x.FindCalendarGroupIdAsync(It.IsAny<CalendarGroupClassification>()), Times.Once);
            streamMock.Verify(x => x.OpenAsync(filePath), Times.Once);
            readerMock.Verify(x => x.ReadAllAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
            repositoryMock.Verify(x => x.FindByAfterDateAsync(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
            repositoryMock.Verify(
                x => x.RemoveRangeAsync(
                    It.IsAny<IEnumerable<OwnCompanyHoliday>>()),
                Times.Once);
            repositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.IsAny<IEnumerable<OwnCompanyHoliday>>()),
                    Times.Never);
        }

        [TestMethod]
        public async Task 異常系_データベース登録に失敗した場合例外を返すこと()
        {
            // given
            var filePath = "test.xlsx";
            var stream = new MemoryStream();
            var allEmployeeNumbers = new List<OwnCompanyHoliday>
            {
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 1, 1), holidayClassification: HolidayClassification.LegalHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 1, 2), holidayClassification: HolidayClassification.RegularHoliday),
            };
            var additionalEmployeeNumbers = new List<OwnCompanyHoliday>
            {
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 1, 1), holidayClassification: HolidayClassification.LegalHoliday),
                TestOwnCompanyHolidayFactory.Create(holidayDate: new DateTime(2023, 1, 2), holidayClassification: HolidayClassification.RegularHoliday),
            };

            Mock<IFileStreamOpener> streamMock = new();
            streamMock.Setup(x => x.OpenAsync(filePath)).ReturnsAsync(stream);

            Mock<IOwnCompanyHolidayListReader> readerMock = new();
            readerMock.Setup(x => x.ReadAllAsync(It.IsAny<Stream>(), It.IsAny<string>())).ReturnsAsync(additionalEmployeeNumbers);

            Mock<IOwnCompanyHolidayRepository> repositoryMock = new();
            repositoryMock.Setup(x => x.FindByAfterDateAsync(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(allEmployeeNumbers);
            repositoryMock.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<OwnCompanyHoliday>>()))
                .ThrowsAsync(new DomainException());

            // when
            var useCase = new RegisterOwnCompanyHolidayUseCase(
                streamMock.Object,
                readerMock.Object,
                repositoryMock.Object);
            Task target() => useCase.ExecuteAsync(filePath, CalendarGroupAttempt.HeadOffice);

            // then
            _ = await Assert.ThrowsExceptionAsync<UseCaseException>(target);
            repositoryMock.Verify(x => x.FindCalendarGroupIdAsync(It.IsAny<CalendarGroupClassification>()), Times.Once);
            streamMock.Verify(x => x.OpenAsync(filePath), Times.Once);
            readerMock.Verify(x => x.ReadAllAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
            repositoryMock.Verify(x => x.FindByAfterDateAsync(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
            repositoryMock.Verify(
                x => x.RemoveRangeAsync(
                    It.IsAny<IEnumerable<OwnCompanyHoliday>>()),
                Times.Once);
            repositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.IsAny<IEnumerable<OwnCompanyHoliday>>()),
                    Times.Once);
        }
    }
}