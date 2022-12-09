using Wada.CommonDialogLib.InnerServices;

namespace Wada.CommonDialogLib
{
	/// <summary>コモンダイアログ表示用サービスクラスを表します。</summary>
	public class CommonDialogSwitcher : ICommonDialogSwitcher
	{
		/// <summary>コモンダイアログを表示します。</summary>
		/// <param name="parameters">ダイアログと値を受け渡しするためのICommonDialogParameters。</param>
		/// <returns>trueが返るとOKボタン、falseが返るとキャンセルボタンが押されたことを表します。</returns>
		public void ShowDialog(ICommonDialogParameters parameters, Action<ICommonDialogResult> callback)
		{
			var service = this.CreateInnerService(parameters);
			if (service == null)
				return;

			service.ShowDialog(parameters, callback);
		}


        /// <summary>表示するコモンダイアログサービスを生成します。</summary>
        /// <param name="parameters">ダイアログと値を受け渡しするためのICommonDialogParameters。</param>
        /// <returns>表示するコモンダイアログサービスを表すICommonDialogService。</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0066:switch ステートメントを式に変換します", Justification = "<保留中>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:メンバーを static に設定します", Justification = "<保留中>")]
        private ICommonDialogSwitcher? CreateInnerService(ICommonDialogParameters parameters)
		{
			if (parameters == null)
				return null;

			switch (parameters)
			{
				case ApiPackFolderBrowsDialogParameters:
				case ApiPackOpenFileDialogParameters:
				case ApiPackSaveFileDialogParameters:
                    return new CommonFileDialogSwitcher();
				case OpenFileDialogParameters:
				case SaveFileDialogParameters:
                    return new FileDialogSwitcher();
				default:
					return null;
			}
		}
	}
}
