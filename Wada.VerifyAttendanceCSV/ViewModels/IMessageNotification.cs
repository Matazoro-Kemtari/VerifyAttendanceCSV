namespace Wada.VerifyAttendanceCSV.ViewModels
{
    public interface IMessageNotification
    {
        /// <summary>注意メッセージボックスを表示します</summary>
        /// <param name="message">メッセージボックスに表示する内容を表す文字列</param>
        /// <param name="title">メッセージボックスのタイトルを表す文字列</param>
        void ShowExclamationMessage(string message, string title);

        /// <summary>
        /// エラーメッセージボックスを表示します
        /// </summary>
        /// <param name="message">メッセージボックスに表示する内容を表す文字列</param>
        /// <param name="title">メッセージボックスのタイトルを表す文字列</param>
        void ShowErrorMessage(string message, string title);

        /// <summary>
        /// 確認メッセージボックスを表示します
        /// </summary>
        /// <param name="message">メッセージボックスに表示する内容を表す文字列</param>
        /// <param name="title">メッセージボックスのタイトルを表す文字列</param>
        MessageNotificationResult ShowQuestionMessage(string message, string title);
        
        /// <summary>
        /// 情報メッセージボックスを表示します
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        void ShowInformationMessage(string message, string title);
    }

    public enum MessageNotificationResult
    {
        //
        // 概要:
        //     The message box returns no result.
        None = 0,
        //
        // 概要:
        //     The result value of the message box is OK.
        OK = 1,
        //
        // 概要:
        //     The result value of the message box is Cancel.
        Cancel = 2,
        //
        // 概要:
        //     The result value of the message box is Yes.
        Yes = 6,
        //
        // 概要:
        //     The result value of the message box is No.
        No = 7
    }
}
