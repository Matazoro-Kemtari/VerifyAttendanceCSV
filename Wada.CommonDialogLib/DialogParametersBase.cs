namespace Wada.CommonDialogLib
{
    public abstract class DialogParametersBase : ICommonDialogParameters
    {
        public string InitialDirectory { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
    }
}
