using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.AttendanceTableService;
using Wada.Data.DesignDepartmentDataBase.Models;
using Wada.Data.DesignDepartmentDataBase.Models.MatchedEmployeeNumberAggregation;

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
            var allEmployeeNumbers = new List<MatchedEmployeeNumber>
            {
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 1990),
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 1991),
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 1992),
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 1993),
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 1994),
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 2000),
            };
            var additionalEmployeeNumbers = new List<MatchedEmployeeNumber>
            {
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 2000),
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 2001),
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 2002),
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 2003),
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 2004),
            };
            var deletableEmployeeNumbers = new List<MatchedEmployeeNumber>
            {
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 2000),
            };

            Mock<IFileStreamOpener> streamMock = new();
            streamMock.Setup(x => x.OpenAsync(filePath)).ReturnsAsync(stream);

            Mock<IMatchedEmployeeNumberListReader> readerMock = new();
            readerMock.Setup(x => x.ReadAllAsync(stream)).ReturnsAsync(additionalEmployeeNumbers);

            Mock<IMatchedEmployeeNumberRepository> repositoryMock = new();
            repositoryMock.Setup(x => x.FindAllAsync()).ReturnsAsync(allEmployeeNumbers);

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
                x => x.FindAllAsync(), Times.Once);
            repositoryMock.Verify(
                x => x.RemoveRangeAsync(deletableEmployeeNumbers), Times.Once);
            repositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.IsAny<IEnumerable<MatchedEmployeeNumber>>()),
                    Times.Once);
        }

        [TestMethod]
        public async Task 正常系_削除対象がない場合削除せずに追加が実行されること()
        {
            // given
            var filePath = "test.xlsx";
            var stream = new MemoryStream();
            var allEmployeeNumbers = new List<MatchedEmployeeNumber>
            {
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 1990),
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 1991),
            };
            var additionalEmployeeNumbers = new List<MatchedEmployeeNumber>
            {
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 2000),
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 2001),
            };

            Mock<IFileStreamOpener> streamMock = new();
            streamMock.Setup(x => x.OpenAsync(filePath)).ReturnsAsync(stream);

            Mock<IMatchedEmployeeNumberListReader> readerMock = new();
            readerMock.Setup(x => x.ReadAllAsync(stream)).ReturnsAsync(additionalEmployeeNumbers);

            Mock<IMatchedEmployeeNumberRepository> repositoryMock = new();
            repositoryMock.Setup(x => x.FindAllAsync()).ReturnsAsync(allEmployeeNumbers);

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
                x => x.FindAllAsync(), Times.Once);
            repositoryMock.Verify(
                x => x.RemoveRangeAsync(It.IsAny<IEnumerable<MatchedEmployeeNumber>>()), Times.Never);
            repositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.IsAny<IEnumerable<MatchedEmployeeNumber>>()),
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
                x => x.RemoveRangeAsync(
                    It.IsAny<IEnumerable<MatchedEmployeeNumber>>()),
                Times.Never);
            repositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.IsAny<IEnumerable<MatchedEmployeeNumber>>()),
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
                x => x.RemoveRangeAsync(
                    It.IsAny<IEnumerable<MatchedEmployeeNumber>>()),
                Times.Never);
            repositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.IsAny<IEnumerable<MatchedEmployeeNumber>>()),
                    Times.Never);
        }

        [TestMethod]
        public async Task 異常系_データベース削除に失敗した場合例外を返すこと()
        {
            // given
            var filePath = "test.xlsx";
            var stream = new MemoryStream();
            var allEmployeeNumbers = new List<MatchedEmployeeNumber>
            {
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 1990),
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 2000),
            };
            var additionalEmployeeNumbers = new List<MatchedEmployeeNumber>
            {
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 2000),
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 2001),
            };

            Mock<IFileStreamOpener> streamMock = new();
            streamMock.Setup(x => x.OpenAsync(filePath)).ReturnsAsync(stream);

            Mock<IMatchedEmployeeNumberListReader> readerMock = new();
            readerMock.Setup(x => x.ReadAllAsync(It.IsAny<Stream>())).ReturnsAsync(additionalEmployeeNumbers);

            Mock<IMatchedEmployeeNumberRepository> repositoryMock = new();
            repositoryMock.Setup(x => x.FindAllAsync()).ReturnsAsync(allEmployeeNumbers);
            repositoryMock.Setup(x => x.RemoveRangeAsync(It.IsAny<IEnumerable<MatchedEmployeeNumber>>()))
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
            repositoryMock.Verify(x => x.FindAllAsync(), Times.Once);
            repositoryMock.Verify(
                x => x.RemoveRangeAsync(
                    It.IsAny<IEnumerable<MatchedEmployeeNumber>>()),
                Times.Once);
            repositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.IsAny<IEnumerable<MatchedEmployeeNumber>>()),
                    Times.Never);
        }

        [TestMethod]
        public async Task 異常系_データベース登録に失敗した場合例外を返すこと()
        {
            // given
            var filePath = "test.xlsx";
            var stream = new MemoryStream();
            var allEmployeeNumbers = new List<MatchedEmployeeNumber>
            {
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 1990),
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 2000),
            };
            var additionalEmployeeNumbers = new List<MatchedEmployeeNumber>
            {
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 2000),
                TestMatchedEmployeeNumberFactory.Create(employeeNumber: 2001),
            };

            Mock<IFileStreamOpener> streamMock = new();
            streamMock.Setup(x => x.OpenAsync(filePath)).ReturnsAsync(stream);

            Mock<IMatchedEmployeeNumberListReader> readerMock = new();
            readerMock.Setup(x => x.ReadAllAsync(It.IsAny<Stream>())).ReturnsAsync(additionalEmployeeNumbers);

            Mock<IMatchedEmployeeNumberRepository> repositoryMock = new();
            repositoryMock.Setup(x => x.FindAllAsync()).ReturnsAsync(allEmployeeNumbers);
            repositoryMock.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<MatchedEmployeeNumber>>()))
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
            repositoryMock.Verify(x => x.FindAllAsync(), Times.Once);
            repositoryMock.Verify(
                x => x.RemoveRangeAsync(
                    It.IsAny<IEnumerable<MatchedEmployeeNumber>>()),
                Times.Once);
            repositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.IsAny<IEnumerable<MatchedEmployeeNumber>>()),
                    Times.Once);
        }
    }
}