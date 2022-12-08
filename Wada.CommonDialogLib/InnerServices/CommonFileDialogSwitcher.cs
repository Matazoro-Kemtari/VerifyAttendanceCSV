using Microsoft.WindowsAPICodePack.Dialogs;

namespace Wada.CommonDialogLib.InnerServices
{
    /// <summary>Windows API Code PackのCommonFileDialogを表示します。</summary>
    class CommonFileDialogSwitcher : ICommonDialogSwitcher
    {
        /// <summary>コモンダイアログを表示します。</summary>
        /// <param name="parameters">設定情報を表すIDialogParameters。</param>
        /// <returns>trueが返ると選択したファイル名、ユーザがキャンセルするとfalseが返ります。</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0066:switch ステートメントを式に変換します", Justification = "<保留中>")]
        public void ShowDialog(ICommonDialogParameters parameters, Action<ICommonDialogResult> callback)
        {
            var dialog = CreateDialog(parameters);
            if (dialog == null)
            {
                CommonDialogResult result = new(
                    Result: ButtonResult.None);
                callback(result);
                return;
            }

            var ret = dialog.ShowDialog();

            ICommonDialogResult dialogResult;
            switch (ret)
            {
                case CommonFileDialogResult.None:
                case CommonFileDialogResult.Cancel:
                    dialogResult = new CommonDialogResult(
                        Result: ButtonResult.Cancel);
                    break;
                case CommonFileDialogResult.Ok:
                    dialogResult = SetReturnValues(dialog, parameters);
                    break;
                default:
                    dialogResult = new CommonDialogResult(
                        Result: ButtonResult.None);
                    break;
            }

            callback(dialogResult);
        }


        /// <summary>表示するコモンダイアログを生成します。</summary>
        /// <param name="parameters">設定情報を表すIDialogParameters。</param>
        /// <returns>生成したコモンダイアログを表すCommonFileDialog。</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:メンバーを static に設定します", Justification = "<保留中>")]
        private CommonFileDialog? CreateDialog(ICommonDialogParameters parameters)
        {
            if (parameters == null)
                return null;

            CommonFileDialog dialog;

            switch (parameters)
            {
                case ApiPackFolderBrowsDialogParameters f:
                    dialog = new CommonOpenFileDialog() { IsFolderPicker = true };
                    break;
                case ApiPackOpenFileDialogParameters o:
                    dialog = new CommonOpenFileDialog()
                    {
                        Multiselect = ((ApiPackOpenFileDialogParameters)parameters).Multiselect,
                    };
                    break;
                case ApiPackSaveFileDialogParameters s:
                    dialog = new CommonSaveFileDialog();
                    break;
                default:
                    return null;
            }

            dialog.InitialDirectory = parameters.InitialDirectory;
            dialog.Title = parameters.Title;

            var filters = new List<CommonFileDialogFilter>();

            switch (parameters)
            {
                case ApiPackOpenFileDialogParameters f:
                    filters = ApiPackDialogFilterCreator.Create(f.Filter);
                    break;
                case ApiPackSaveFileDialogParameters s:
                    filters = ApiPackDialogFilterCreator.Create(s.Filter);
                    break;
                default:
                    return dialog;
            }

            filters.ForEach(f => dialog.Filters.Add(f));

            return dialog;
        }


        /// <summary>戻り値を設定します。</summary>
        /// <param name="dialog">表示したダイアログを表すFileDialog。</param>
        /// <param name="parameters">設定情報を表すIDialogParameters。</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:メンバーを static に設定します", Justification = "<保留中>")]
        private ICommonDialogResult SetReturnValues(CommonFileDialog dialog, ICommonDialogParameters parameters)
        {
            ICommonDialogResult result;

            switch (parameters)
            {
                case ApiPackOpenFileDialogParameters:
                    OpenFileDialogResult openFile = new(
                        Result: ButtonResult.OK,
                        FileName: ((CommonOpenFileDialog)dialog).Multiselect ? ((CommonOpenFileDialog)dialog).FileNames.First() : dialog.FileName,
                        FileNames: ((CommonOpenFileDialog)dialog).Multiselect ? ((CommonOpenFileDialog)dialog).FileNames : default);
                    var common = (dialog as CommonOpenFileDialog);
                    if (common != null)
                        openFile = openFile with { FileNames = common.FileNames.ToList() };

                    result = openFile;
                    break;
                case ApiPackSaveFileDialogParameters:
                    result = new SaveFileDialogResult(
                        Result: ButtonResult.OK,
                        FileName: dialog.FileName);
                    break;
                case ApiPackFolderBrowsDialogParameters:
                    result = new FolderBrowsDialogResult(
                        Result: ButtonResult.OK,
                        SelectedFolderPath: dialog.FileName);
                    break;
                default:
                    result = new CommonDialogResult(ButtonResult.None);
                    break;
            }

            return result;
        }
    }
}
