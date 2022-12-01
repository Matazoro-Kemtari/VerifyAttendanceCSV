namespace Wada.AttendanceTableService
{
    public interface IStreamReaderOpener
    {
        StreamReader Open(string path);
    }
}
