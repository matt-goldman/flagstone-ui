using CommunityToolkit.Maui.Storage;
using System.Text;

namespace FlagstoneUI.BootstrapConverter.UI.Services;

public interface IFileService
{
	Task<List<SourceFile>> GetFilePaths();
	Task<SourceFile> GetFilePath();
	Task<SaveResult> SaveFilesAsync(string defaultFileName, Dictionary<string, string> files);
	Task<byte[]?> DownloadFileAsync(string url, CancellationToken cancellationToken = default);
}

internal class FileService : IFileService
{
	private readonly HttpClient _httpClient = new HttpClient();

	public Task<SourceFile> GetFilePath()
	{
		throw new NotImplementedException();
	}

	public async Task<List<SourceFile>> GetFilePaths()
	{
		var result = await FilePicker.Default.PickMultipleAsync();

		if (result is null)
		{
			return [];
		}

		return [.. result
			.Where(static f => f?.FullPath is not null)
			.Select(static f => new SourceFile(f!.FullPath))];
	}

	public async Task<SaveResult> SaveFilesAsync(string defaultFileName, Dictionary<string, string> files)
	{
		try
		{
			var folderResult = await FolderPicker.Default.PickAsync(CancellationToken.None);
			
			if (!folderResult.IsSuccessful || folderResult.Folder == null)
			{
				return new SaveResult(false, "Folder selection cancelled");
			}

			var savedFiles = new List<string>();
			var errors = new List<string>();

			foreach (var (fileName, content) in files)
			{
				try
				{
					var filePath = Path.Combine(folderResult.Folder.Path, fileName);
					await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
					savedFiles.Add(fileName);
				}
				catch (Exception ex)
				{
					errors.Add($"{fileName}: {ex.Message}");
				}
			}

			if (errors.Count > 0)
			{
				return new SaveResult(false, $"Some files failed to save:\n{string.Join("\n", errors)}", savedFiles);
			}

			return new SaveResult(true, $"Successfully saved {savedFiles.Count} files to {folderResult.Folder.Path}", savedFiles);
		}
		catch (Exception ex)
		{
			return new SaveResult(false, $"Error: {ex.Message}");
		}
	}

	public async Task<byte[]?> DownloadFileAsync(string url, CancellationToken cancellationToken = default)
	{
		try
		{
			var response = await _httpClient.GetAsync(url, cancellationToken);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsByteArrayAsync(cancellationToken);
		}
		catch
		{
			return null;
		}
	}
}

public record SourceFile(string Path);

public record SaveResult(bool IsSuccessful, string Message, List<string>? SavedFiles = null);
