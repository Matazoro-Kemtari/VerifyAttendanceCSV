namespace Wada.CommonDialogLib
{
    public class SaveFileDialogParameters : DialogParametersBase
    {
        public string Filter { get; set; } = string.Empty;

        public int FilterIndex { get; set; } = 0;
    }
}
