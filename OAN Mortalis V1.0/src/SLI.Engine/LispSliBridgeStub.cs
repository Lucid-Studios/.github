using System.Diagnostics;
using OAN.Core.Sli;

namespace SLI.Engine;

public sealed class LispSliBridgeStub : ISliBridge
{
    private const string DefaultExecutable = "sbcl";
    private const string DefaultScriptArgPrefix = "--script";

    public async Task<string> SendPacketAsync(string sliExpression, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sliExpression);

        var scriptPath = Path.Combine(AppContext.BaseDirectory, "sli_engine.lisp");
        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException("SLI engine script not found.", scriptPath);
        }

        var executable = GetExecutable();
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = $"{DefaultScriptArgPrefix} \"{scriptPath}\"",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Unable to launch Lisp runtime '{executable}'. Set OAN_SLI_LISP_EXECUTABLE if needed.",
                ex);
        }

        await process.StandardInput.WriteLineAsync(sliExpression.AsMemory(), cancellationToken).ConfigureAwait(false);
        process.StandardInput.Close();

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        var stdout = await stdoutTask.ConfigureAwait(false);
        var stderr = await stderrTask.ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            var detail = string.IsNullOrWhiteSpace(stderr) ? "Unknown Lisp runtime error." : stderr.Trim();
            throw new InvalidOperationException($"SLI Lisp engine failed: {detail}");
        }

        return string.IsNullOrWhiteSpace(stdout) ? "(:result :status rejected :reason empty-output)" : stdout.Trim();
    }

    private static string GetExecutable()
    {
        var configured = Environment.GetEnvironmentVariable("OAN_SLI_LISP_EXECUTABLE");
        return string.IsNullOrWhiteSpace(configured) ? DefaultExecutable : configured.Trim();
    }
}
