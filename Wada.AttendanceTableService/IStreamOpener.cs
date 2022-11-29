namespace Wada.AttendanceTableService
{
    public interface IStreamOpener
    {
        Stream Open(string path);
    }
}
