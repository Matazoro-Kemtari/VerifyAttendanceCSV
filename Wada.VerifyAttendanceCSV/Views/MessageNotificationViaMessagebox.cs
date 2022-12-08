using System.Windows;
using Wada.VerifyAttendanceCSV.ViewModels;

namespace Wada.VerifyAttendanceCSV.Views
{
    public class MessageNotificationViaMessagebox : IMessageNotification
    {
        public void ShowErrorMessage(string message, string title)
        {
            _ = MessageBox.Show(message,
                                title,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
        }

        public void ShowExclamationMessage(string message, string title)
        {
            _ = MessageBox.Show(message,
                                title,
                                MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
        }

        public void ShowInformationMessage(string message, string title)
        {
            _ = MessageBox.Show(message,
                                title,
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
        }

        public MessageNotificationResult ShowQuestionMessage(string message, string title)
        {
            var result = MessageBox.Show(message,
                                         title,
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            return (MessageNotificationResult)result;
        }
    }
}
