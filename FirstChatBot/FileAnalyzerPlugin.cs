using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace FirstChatBot;

public class FileAnalyzerPlugin
{
    [KernelFunction("read_files")]
    [Description("Reads the content of the file at that path.")]
    public async Task<string> ReadFiles(
        [Description("Full path of the file to read.")] string filePath)
    {
        try
        {
            string content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            return content.Length > 100
                ? $"I read {filePath}. Here's the first 100 characters: {content[..100]}..."
                : $"I read {filePath}: {content}";
        }
        catch (UnauthorizedAccessException)
        {
            return $"Permission denied: Cannot access '{filePath}'.";
        }
        catch (IOException ex)
        {
            return $"File error: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Unexpected error reading '{filePath}': {ex.Message}";
        }
    }

    [KernelFunction("write_files")]
    [Description("This function creates a new text file and writes text inside the file on the user's computer. It handles folder creation if it doesn't exist.")]
    public async Task<string> WriteFile(
        [Description("Text to write into the file.")] string text,
        [Description("Full path where the file should be created.")] string filePath)
    {
        try
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            await writer.WriteAsync(text);
            return "Successfully Created";
        }
        catch (Exception ex)
        {
            return "Error: " + ex.Message;
        }
    }

    [KernelFunction("list_path_contents")]
    [Description("Lists the files and directories in the specified path.")]
    public string ListPathContents(
        [Description("The directory path to list contents from.")] string directory)
    {
        if (!Directory.Exists(directory))
        {
            return $"Error: The directory '{directory}' does not exist.";
        }

        var dirs = Directory.GetDirectories(directory).Select(Path.GetFileName);
        var files = Directory.GetFiles(directory).Select(Path.GetFileName);

        var sb = new StringBuilder();
        sb.AppendLine($"Contents of '{directory}':");
        sb.AppendLine("Directories:");
        foreach (var dir in dirs)
        {
            sb.AppendLine($"  [Folder] {dir}");
        }
        sb.AppendLine("Files:");
        foreach (var file in files)
        {
            sb.AppendLine($"  [File] {file}");
        }

        return sb.ToString();
    }

    [KernelFunction("execute_terminal_command")]
    [Description("Executes a terminal command in the current working directory and returns the output or error.")]
    public async Task<string> ExecuteTerminalCommand(
        [Description("The terminal command to execute.")] string command)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/bash",
                Arguments = OperatingSystem.IsWindows() ? $"/c {command}" : $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(error))
            {
                return $"Error executing command: {error.Trim()}";
            }

            return string.IsNullOrWhiteSpace(output)
                ? "Command executed successfully, but no output was returned."
                : output.Trim();
        }
        catch (Exception ex)
        {
            return $"Exception while executing command: {ex.Message}";
        }
    }
}