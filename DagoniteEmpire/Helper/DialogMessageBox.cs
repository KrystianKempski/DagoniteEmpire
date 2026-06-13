using System.Net;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace DagoniteEmpire.Helper;

public static class DialogMessageBox
{
    public static readonly DialogOptions CompactOptions = new()
    {
        MaxWidth = MaxWidth.ExtraSmall,
        FullWidth = false,
    };

    public static MarkupString CompactMessage(string line1, string? line2 = null)
    {
        line1 = WebUtility.HtmlEncode(line1);
        var secondLine = string.IsNullOrWhiteSpace(line2)
            ? string.Empty
            : $"<span>{WebUtility.HtmlEncode(line2)}</span>";

        return (MarkupString)$"<div class=\"compact-message-box-text\"><span>{line1}</span>{secondLine}</div>";
    }

    public static Task<bool?> ShowCompactAsync(
        IDialogService dialogService,
        string title,
        string line1,
        string? line2,
        string yesText,
        string cancelText = "Cancel")
    {
        return dialogService.ShowMessageBoxAsync(
            title,
            CompactMessage(line1, line2),
            yesText: yesText,
            cancelText: cancelText,
            options: CompactOptions);
    }
}
