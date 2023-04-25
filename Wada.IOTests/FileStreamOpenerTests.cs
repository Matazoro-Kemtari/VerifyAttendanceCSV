using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using System.Text;
using Wada.AttendanceTableService;

namespace Wada.IO.Tests;

[TestClass()]
public class FileStreamOpenerTests
{
    [TestMethod()]
    public async Task 正常系_既存のファイルを開くこと()
    {
        // given
        const string testDataName = "Wada.IOTests.Resources.RandomData.txt";
        if (File.Exists(testDataName))
            File.Delete(testDataName);

        try
        {
            // when
            var expected = string.Empty;
            // アセンブリに埋め込まれているリソース"RandomData.txt"のStreamを取得する
            var assembly = Assembly.GetExecutingAssembly();
            using (var resurceStream = assembly.GetManifestResourceStream(testDataName))
            {
                using var resurceReader = new StreamReader(resurceStream!);
                expected = resurceReader.ReadToEnd();
                using var write = File.Create(testDataName);

                byte[] bytes = new UTF8Encoding(true).GetBytes(expected);
                write.Write(bytes, 0, bytes.Length);
            }
            IFileStreamOpener streamOpener = new FileStreamOpener();
            var actual = string.Empty;
            using (var stream = await streamOpener.OpenAsync(testDataName))
            using (var reader = new StreamReader(stream!))
                actual = reader.ReadToEnd();

            // then
            Assert.AreEqual(expected, actual);
        }
        finally
        {
            File.Delete(testDataName);
        }
    }

    [TestMethod()]
    public async Task 異常系_存在しないファイルを開こうとすると例外を返すこと()
    {
        // given
        const string testDataName = "Wada.IOTests.Resources.RandomData.txt";
        if (File.Exists(testDataName))
            File.Delete(testDataName);

        // when
        IFileStreamOpener streamOpener = new FileStreamOpener();
        Task<Stream> target() => streamOpener.OpenAsync(testDataName);

        // then
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(target);
        var message = $"ファイルが見つかりません ファイルパス: {testDataName}";
        Assert.AreEqual(message, ex.Message);
    }

    [TestMethod()]
    public async Task 異常系_他のプロセスがファイルをロックしているファイルを開こうとすると例外を返すこと()
    {
        // given
        const string testDataName = "Wada.IOTests.Resources.RandomData.txt";
        using var fileStream = File.Open(testDataName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);

        try
        {
            // when
            IFileStreamOpener streamOpener = new FileStreamOpener();
            Task<Stream> target() => streamOpener.OpenAsync(testDataName);

            // then
            var ex = await Assert.ThrowsExceptionAsync<DomainException>(target);
            var message = $"ファイルが使用中です ファイルパス: {testDataName}";
            Assert.AreEqual(message, ex.Message);
        }
        finally
        {
            fileStream.Dispose();
            File.Delete(testDataName);
        }
    }
}