using Livet.Messaging;
using Livet.Messaging.IO;
using System.Windows;
using Wada.AOP.Logging;

namespace Wada.VerifyAttendanceCSV.ViewModels;

public class MessageNotificationViaLivet
{
    const string _messageTitle = "勤務表検証システム";

    [Logging]
    public static InformationMessage MakeErrorMessage(string message, string title = _messageTitle) => new(
        message, title, MessageBoxImage.Error, "Info");

    [Logging]
    public static InformationMessage MakeExclamationMessage(string message, string title = _messageTitle) => new(
        message, title, MessageBoxImage.Exclamation, "Info");

    [Logging]
    public static InformationMessage MakeInformationMessage(string message, string title = _messageTitle) => new(
        message, title, MessageBoxImage.Information, "Info");

    [Logging]
    public static ConfirmationMessage MakeQuestionMessage(string message, string title = _messageTitle) => new(
        message, title, MessageBoxImage.Question, "Confirm")
    { Button = MessageBoxButton.YesNo };

    [Logging]
    public static SavingFileSelectionMessage MakeSaveFileDialog() => new("SaveFiling");
}
