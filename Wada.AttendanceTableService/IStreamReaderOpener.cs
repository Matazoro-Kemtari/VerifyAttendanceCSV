namespace Wada.AttendanceTableService
{
    public interface IStreamReaderOpener
    {
        /// <summary>
        /// ストリームリーダーを開く
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        StreamReader Open(string path);
    }
}
