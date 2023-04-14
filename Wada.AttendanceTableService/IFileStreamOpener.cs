namespace Wada.AttendanceTableService
{
    public interface IFileStreamOpener
    {
        /// <summary>
        /// ストリームを開く
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<Stream> OpenAsync(string path);
    }
}
