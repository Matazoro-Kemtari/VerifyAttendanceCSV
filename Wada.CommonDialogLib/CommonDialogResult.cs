namespace Wada.CommonDialogLib
{
    public record class CommonDialogResult(ButtonResult Result) : ICommonDialogResult;

    public record class OpenFileDialogResult(ButtonResult Result, string FileName, IEnumerable<string>? FileNames = null) : CommonDialogResult(Result);

    public record class FolderBrowsDialogResult(ButtonResult Result, string SelectedFolderPath) : CommonDialogResult(Result);

    public record class SaveFileDialogResult(ButtonResult Result, string FileName) : CommonDialogResult(Result);
}
