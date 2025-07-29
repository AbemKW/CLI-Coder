using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Microsoft.SemanticKernel;

namespace FirstChatBot;

public class FileAnalyzerPlugin
{
    private string _workSpace =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "FileAnalyzerWorkSpace"
        );

    public FileAnalyzerPlugin() { }

    private string ResolveAndValidatePath(string inputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            return "Path can not be empty";
        }
        string fullPath = Path.GetFullPath(inputPath, _workSpace);
        if (!fullPath.StartsWith(_workSpace, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException(
                $"Access denied: Path '{inputPath}' is outside the workspace '{_workSpace}'."
            );
        }

        // Check for invalid path characters
        if (inputPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            throw new ArgumentException($"Path '{inputPath}' contains invalid characters.");
        }

        return fullPath;
    }

    [KernelFunction("read_files")]
    [Description("Reads the content of the file at that path.")]
    public async Task<string> ReadFiles(
        [Description("Full path of the file to read.")] string filePath
    )
    {
        try
        {
            string fullPath = ResolveAndValidatePath(filePath);
            if (!File.Exists(fullPath))
            {
                return $"Error: Could not find the file {filePath} in the workspace";
            }
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
    [Description(
        "This function creates a new text file and writes text inside the file on the user's computer. It handles folder creation if it doesn't exist."
    )]
    public async Task<string> WriteFile(
        [Description("Text to write into the file.")] string text,
        [Description("Full path where the file should be created.")] string filePath
    )
    {
        try
        {
            string fullPath = ResolveAndValidatePath(filePath);
            string? directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var writer = new StreamWriter(fullPath, false, Encoding.UTF8);
            await writer.WriteAsync(text);
            return "Successfully Created";
        }
        catch (Exception ex)
        {
            return "Error: " + ex.Message;
        }
    }

    [KernelFunction("delete_file")]
    [Description("Deletes a file or a directory in a specific path.")]
    public async Task<string> DeleteFiles(
        [Description("The file path to be deleted")] string inputPath
    )
    {
        try
        {
            string fullPath = ResolveAndValidatePath(inputPath);
            string? directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(directory))
            {
                return $"There is no directory which has {inputPath}";
            }
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
                return $"Successfully deleted {fullPath}";
            }
            else if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return $"Successfully deleted {fullPath}";
            }
            else
            {
                return $"Path does not exist";
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [KernelFunction("update_files")]
    [Description("Updates the contents inside of the file, can also rename files or directories")]
    public async Task<string> UpdateFiles(
        [Description("The file path to update its content or rename")] string inputPath,
        [Description("The new name to change the file or directory to")] string newName
    )
    {
        try
        {
            string fullPath = ResolveAndValidatePath(inputPath);
            string? directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(directory))
            {
                return $"There is no directory which has {inputPath}";
            }
            if (Directory.Exists(fullPath))
            {
                string newPath = Path.Combine(directory, newName);
                Directory.Move(fullPath, newPath);
                return $"Successfully renamed {fullPath} to {newPath}";
            }
            else if (File.Exists(fullPath))
            {
                string newPath = Path.Combine(directory, newName);
                File.Move(fullPath, newPath);
                return $"Successfully renamed the file";
            }
            else
            {
                return $"Path does not exist";
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [KernelFunction("list_path_contents")]
    [Description("Lists the files and directories in the specified path.")]
    public string ListPathContents()
    {
        if (!Directory.Exists(_workSpace))
        {
            return $"Error: The directory '{_workSpace}' does not exist.";
        }

        var dirs = Directory.GetDirectories(_workSpace).Select(Path.GetFileName);
        var files = Directory.GetFiles(_workSpace).Select(Path.GetFileName);

        var sb = new StringBuilder();
        sb.AppendLine($"Contents of '{_workSpace}':");
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
    [Description(
        "Executes a terminal command in the current working directory and returns the output or error."
    )]
    public async Task<string> ExecuteTerminalCommand(
        [Description("The terminal command to execute.")] string command
    )
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
                CreateNoWindow = true,
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
