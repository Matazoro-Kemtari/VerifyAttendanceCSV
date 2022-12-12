namespace Wada.CommonDialogLib
{
    public interface ICommonDialogResult
    {
        ButtonResult Result { get; init; }
    }

    public enum ButtonResult
    {
        None,
        OK,
        Cancel,
    }
}
