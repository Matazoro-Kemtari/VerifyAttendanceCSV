using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.MatchedEmployeeNumberAggregation;
using Wada.Data.DesignDepartmentDataBase.Models;

namespace Wada.RegisterEmployeeNumberTableApplication.Tests
{
    [TestClass()]
    public class RegisterEmployeeNumberTableUseCaseTests
    {
        [TestMethod]
        public async Task 正常系_DIしたオブジェクトが実行されること()
        {
            // given
            var filePath = "test.xlsx";
            var stream = new MemoryStream();
            var matchedEmployeeNumbers = Array.Empty<MatchedEmployeeNumber>();

            Mock<IFileStreamOpener> streamMock = new();
            streamMock.Setup(x => x.OpenAsync(filePath)).ReturnsAsync(stream);

            Mock<IMatchedEmployeeNumberListReader> readerMock = new();
            readerMock.Setup(x => x.ReadAllAsync(stream)).ReturnsAsync(matchedEmployeeNumbers);

            Mock<IMatchedEmployeeNumberRepository> repositoryMock = new();

            // when
            var useCase = new RegisterEmployeeNumberTableUseCase(
                streamMock.Object,
                readerMock.Object,
                repositoryMock.Object);
            await useCase.ExecuteAsync(filePath);

            // then
            streamMock.Verify(x => x.OpenAsync(filePath), Times.Once);
            readerMock.Verify(x => x.ReadAllAsync(stream), Times.Once);
            repositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.IsAny<IEnumerable<Data.DesignDepartmentDataBase.Models.MatchedEmployeeNumberAggregation.MatchedEmployeeNumber>>()),
                    Times.Once);
        }

        [TestMethod]
        public async Task 異常系_ファイルが存在しない場合例外を返すこと()
        {
            // given
            var filePath = "test.xlsx";
            var stream = new MemoryStream();
            var matchedEmployeeNumbers = Array.Empty<MatchedEmployeeNumber>();

            Mock<IFileStreamOpener> streamMock = new();
            streamMock.Setup(x => x.OpenAsync(filePath)).ThrowsAsync(new DomainException());

            Mock<IMatchedEmployeeNumberListReader> readerMock = new();
            Mock<IMatchedEmployeeNumberRepository> repositoryMock = new();

            // when
            var useCase = new RegisterEmployeeNumberTableUseCase(
                streamMock.Object,
                readerMock.Object,
                repositoryMock.Object);
            Task target() => useCase.ExecuteAsync(filePath);

            // then
            _ = await Assert.ThrowsExceptionAsync<UseCaseException>(target);
            streamMock.Verify(x => x.OpenAsync(filePath), Times.Once);
            readerMock.Verify(x => x.ReadAllAsync(It.IsAny<Stream>()), Times.Never);
            repositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.IsAny<IEnumerable<Data.DesignDepartmentDataBase.Models.MatchedEmployeeNumberAggregation.MatchedEmployeeNumber>>()),
                    Times.Never);
        }

        [TestMethod]
        public async Task 異常系_不正なファイル形式の場合例外を返すこと()
        {
            // given
            var filePath = "test.xlsx";
            var stream = new MemoryStream();
            var matchedEmployeeNumbers = Array.Empty<MatchedEmployeeNumber>();

            Mock<IFileStreamOpener> streamMock = new();
            streamMock.Setup(x => x.OpenAsync(filePath)).ReturnsAsync(stream);

            Mock<IMatchedEmployeeNumberListReader> readerMock = new();
            readerMock.Setup(x => x.ReadAllAsync(It.IsAny<Stream>())).ThrowsAsync(new DomainException());

            Mock<IMatchedEmployeeNumberRepository> repositoryMock = new();

            // when
            var useCase = new RegisterEmployeeNumberTableUseCase(
                streamMock.Object,
                readerMock.Object,
                repositoryMock.Object);
            Task target() => useCase.ExecuteAsync(filePath);

            // then
            _ = await Assert.ThrowsExceptionAsync<UseCaseException>(target);
            streamMock.Verify(x => x.OpenAsync(filePath), Times.Once);
            readerMock.Verify(x => x.ReadAllAsync(It.IsAny<Stream>()), Times.Once);
            repositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.IsAny<IEnumerable<Data.DesignDepartmentDataBase.Models.MatchedEmployeeNumberAggregation.MatchedEmployeeNumber>>()),
                    Times.Never);
        }

        [TestMethod]
        public async Task 異常系_データベース登録に失敗した場合例外を返すこと()
        {
            // given
            var filePath = "test.xlsx";
            var stream = new MemoryStream();
            var matchedEmployeeNumbers = Array.Empty<MatchedEmployeeNumber>();

            Mock<IFileStreamOpener> streamMock = new();
            streamMock.Setup(x => x.OpenAsync(filePath)).ReturnsAsync(stream);

            Mock<IMatchedEmployeeNumberListReader> readerMock = new();
            readerMock.Setup(x => x.ReadAllAsync(It.IsAny<Stream>())).ReturnsAsync(matchedEmployeeNumbers);

            Mock<IMatchedEmployeeNumberRepository> repositoryMock = new();
            repositoryMock.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<Data.DesignDepartmentDataBase.Models.MatchedEmployeeNumberAggregation.MatchedEmployeeNumber>>()))
                .ThrowsAsync(new DomainException());

            // when
            var useCase = new RegisterEmployeeNumberTableUseCase(
                streamMock.Object,
                readerMock.Object,
                repositoryMock.Object);
            Task target() => useCase.ExecuteAsync(filePath);

            // then
            _ = await Assert.ThrowsExceptionAsync<UseCaseException>(target);
            streamMock.Verify(x => x.OpenAsync(filePath), Times.Once);
            readerMock.Verify(x => x.ReadAllAsync(It.IsAny<Stream>()), Times.Once);
            repositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.IsAny<IEnumerable<Data.DesignDepartmentDataBase.Models.MatchedEmployeeNumberAggregation.MatchedEmployeeNumber>>()),
                    Times.Once);
        }
    }
}