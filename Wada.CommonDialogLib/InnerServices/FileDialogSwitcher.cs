using Microsoft.Win32;

namespace Wada.CommonDialogLib.InnerServices
{
    /// <summary>ファイルを開く、ファイルに名前を付けて保存ダイアログ用のサービスを表します。</summary>
    class FileDialogSwitcher : ICommonDialogSwitcher
    {
        /// <summary>コモンダイアログを表示します。</summary>
        /// <param name="parameters">設定情報を表すIDialogParameters。</param>
        /// <returns>trueが返ると選択したファイル名、ユーザがキャンセルするとfalseが返ります。</returns>
        public void ShowDialog(ICommonDialogParameters parameters, Action<ICommonDialogResult> callback)
        {
            var dialog = CreateDialogService(parameters);
            if (dialog == null)
            {
                CommonDialogResult result = new(
                    Result: ButtonResult.None);
                callback(result);
                return;
            }

            var ret = dialog.ShowDialog();

            if (ret.HasValue)
            {
                ICommonDialogResult result = SetReturnValues(dialog, parameters);
                callback(result); 
                return;
            }
            else
            {
                CommonDialogResult result = new(
                    Result: ButtonResult.Cancel);
                callback(result);
                return;
            }
        }

        /// <summary>表示するコモンダイアログを生成します。</summary>
        /// <param name="parameters">設定情報を表すIDialogParameters。</param>
        /// <returns>生成したコモンダイアログを表すFileDialog。</returns>
        private FileDialog? CreateDialogService(ICommonDialogParameters parameters)
        {
            if (parameters == null)
                return null;

            FileDialog dialog;

            if (parameters is OpenFileDialogParameters)
                dialog = new OpenFileDialog()
                {
                    Multiselect = ((OpenFileDialogParameters)parameters).Multiselect,
                };
            else if (parameters is SaveFileDialogParameters)
                dialog = new SaveFileDialog();
            else
                return null;

            var saveSettings = parameters as SaveFileDialogParameters;

            dialog.Filter = saveSettings?.Filter ?? string.Empty;
            dialog.FilterIndex = saveSettings?.FilterIndex ?? 1;
            dialog.InitialDirectory = saveSettings?.InitialDirectory ?? string.Empty;
            dialog.Title = saveSettings?.Title ?? string.Empty;

            return dialog;
        }

        /// <summary>戻り値を設定します。</summary>
        /// <param name="dialog">表示したダイアログを表すFileDialog。</param>
        /// <param name="parameters">設定情報を表すIDialogParameters。</param>
        private ICommonDialogResult SetReturnValues(FileDialog dialog, ICommonDialogParameters parameters)
        {
            ICommonDialogResult result;

            switch (parameters)
            {
                case OpenFileDialogParameters o:
                    var openDialog = dialog as OpenFileDialog;
                    if (openDialog == null)
                    {
                        result = new CommonDialogResult(ButtonResult.None);
                        break;
                    }

                    result = new OpenFileDialogResult(
                        Result: ButtonResult.OK,
                        FileName: openDialog.FileName,
                        FileNames: openDialog.FileNames.ToList());
                    break;
                case SaveFileDialogParameters s:
                    var saveDialog = dialog as SaveFileDialog;
                    if (saveDialog == null)
                    {
                        result = new CommonDialogResult(ButtonResult.None);
                        break;
                    }

                    result = new SaveFileDialogResult(
                        Result: ButtonResult.OK,
                        FileName: saveDialog.FileName);
                    break;
                default:
                    result = new CommonDialogResult(ButtonResult.None);
                    break;
            }

            return result;
        }
    }
}
