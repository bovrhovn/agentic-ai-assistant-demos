using System.ComponentModel;
using System.Diagnostics;
using Microsoft.SemanticKernel;

namespace AAI.MakerChecker;

sealed class ClipboardAccess
{
    [KernelFunction]
    [Description("Copies the provided content to the clipboard.")]
    public static void SetClipboard(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;

        using var clipProcess = Process.Start(
            new ProcessStartInfo
            {
                FileName = "clip",
                RedirectStandardInput = true,
                UseShellExecute = false,
            }) ?? throw new InvalidOperationException("Unable to start clip process.");

        clipProcess.StandardInput.Write(content);
        clipProcess.StandardInput.Close();
    }
}