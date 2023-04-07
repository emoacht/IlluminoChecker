using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Encoding = Windows.Storage.Streams.UnicodeEncoding;

namespace Checker.Core.Models;

internal static class LogAccess
{
	private static StorageFolder GetFolder() => ApplicationData.Current.LocalFolder;

	private const string FileName = $"illumino{FileExtension}";
	private const string FileExtension = ".csv";
	private static string _fileName;

	private const string Header = @"Date,Elapsed,Illuminance,Brightness";

	private static readonly SemaphoreSlim _semaphore = new(1, 1);

	public static async Task CleanAsync(TimeSpan thresholdDuration)
	{
		if (thresholdDuration <= TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(thresholdDuration));

		try
		{
			await _semaphore.WaitAsync();

			var thresholdDate = DateTimeOffset.Now - thresholdDuration;

			foreach (var file in (await GetFolder().GetFilesAsync())
				.Where(x => x.Name.EndsWith(FileExtension)))
			{
				if ((await file.GetBasicPropertiesAsync()).DateModified < thresholdDate)
				{
					await file.DeleteAsync();
				}
			}
		}
		finally
		{
			_semaphore.Release();
		}
	}

	public static async Task RecordAsync(DateTimeOffset date, double elapsed, double illuminance, int brightness)
	{
		var line = $"\"{date:yyyy/MM/dd HH:mm:ss fff}\",{elapsed:f2},{illuminance:f2},{brightness}";

		try
		{
			await _semaphore.WaitAsync();

			_fileName ??= $"{DateTimeOffset.Now:yyyyMMdd}{FileExtension}";

			var file = await GetFolder().TryGetItemAsync(_fileName) as IStorageFile;
			file ??= await GetFolder().CreateFileAsync(_fileName, CreationCollisionOption.ReplaceExisting);

			await FileIO.AppendLinesAsync(file, new[] { line }, Encoding.Utf8);
		}
		finally
		{
			_semaphore.Release();
		}
	}

	public static async Task ExportAsync()
	{
		// https://learn.microsoft.com/ja-jp/windows/uwp/files/quickstart-using-file-and-folder-pickers

		var picker = new FolderPicker();
		picker.SuggestedStartLocation = PickerLocationId.Desktop;
		picker.FileTypeFilter.Add("*");

		var destinationFolder = await picker.PickSingleFolderAsync();
		if (destinationFolder is null)
			return;

		string content = null;
		try
		{
			await _semaphore.WaitAsync();

			var buffer = new StringBuilder().AppendLine(Header);

			foreach (var sourceFile in (await GetFolder().GetFilesAsync())
				.Where(x => x.Name.EndsWith(FileExtension))
				.OrderBy(x => x.DateCreated))
			{
				buffer.Append(await FileIO.ReadTextAsync(sourceFile));
			}

			content = buffer.ToString();
		}
		finally
		{
			_semaphore.Release();
		}

		// Application now has read/write access to all contents in the picked folder
		// (including other sub-folder contents).
		Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", destinationFolder);

		var destinationFile = await destinationFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
		await FileIO.WriteTextAsync(destinationFile, content, Encoding.Utf8);
	}
}
