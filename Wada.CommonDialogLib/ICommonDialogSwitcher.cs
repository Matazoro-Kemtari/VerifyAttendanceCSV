namespace Wada.CommonDialogLib
{
    /// <summary>コモンダイアログ表示用インタフェース</summary>
    /// <remarks>
    /// 出典元 https://elf-mission.net/programming/wpf/episode17/#Prism
    /// Mock化しにくいので変更
    /// </remarks>
    public interface ICommonDialogSwitcher
    {
        /// <summary>コモンダイアログを表示します。</summary>
        /// <param name="parameters">ダイアログと値を受け渡しするためのICommonDialogSettings。</param>
        /// <returns>trueが返るとOKボタン、falseが返るとキャンセルボタンが押されたことを表します。</returns>
        void ShowDialog(ICommonDialogParameters parameters, Action<ICommonDialogResult> callback);
    }
}
