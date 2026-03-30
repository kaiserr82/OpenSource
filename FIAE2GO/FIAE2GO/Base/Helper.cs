using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Base;

public class Helper()
{
    private static readonly Lazy<Helper> lazy =
    new Lazy<Helper>(() => new Helper());

    public static Helper Instance { get { return lazy.Value; } }

    /// <summary>
    /// Shows a message box and returns the result.
    /// </summary>
    /// <param name="title">The title of the message box.</param>
    /// <param name="message">The message to display.</param>
    /// <param name="buttons">The buttons to show in the message box.</param>
    /// <param name="icon">The icon to show in the message box.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the button that was pressed.</returns>
    public async Task<ButtonResult> ShowMessageBoxAsync(string title, string message, ButtonEnum buttons = ButtonEnum.Ok, Icon icon = Icon.None)
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandard(title, message, buttons, icon);

        TopLevel? topLevel = GetTopLevel();
        if (topLevel is not null)
        {
            return await messageBox.ShowAsPopupAsync(topLevel);
        }
        
        // Fallback or log error if no TopLevel found
        return ButtonResult.None;
    }

    private static TopLevel? GetTopLevel()
    {
        return Application.Current?.ApplicationLifetime switch {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            ISingleViewApplicationLifetime singleView => TopLevel.GetTopLevel(singleView.MainView),
            _ => null };
    }
}