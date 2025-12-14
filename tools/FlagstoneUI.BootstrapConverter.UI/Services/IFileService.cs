namespace FlagstoneUI.BootstrapConverter.UI.Services;

public interface IFileService
{
	Task<List<SourceFile>> GetFilePaths();
	Task<SourceFile> GetFilePath();
}

internal class FileService : IFileService
{
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
}

public record SourceFile(string Path);
