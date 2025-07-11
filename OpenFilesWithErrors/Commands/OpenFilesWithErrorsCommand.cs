using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableManager;

namespace OpenFilesWithErrors.Commands;

/// <summary>
/// The open files with errors command.
/// </summary>
[Command(PackageIds.OpenFilesWithErrorsCommand)]
internal sealed class OpenFilesWithErrorsCommand : BaseCommand<OpenFilesWithErrorsCommand>
{
	protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
		if (await Package.GetServiceAsync(typeof(SVsErrorList)) is not IErrorList errorList)
		{
			Trace.TraceWarning($"Got null attempting to get service{nameof(SVsErrorList)}");
			return;
		}

		var tableControl = errorList.TableControl;
		var fileDictionary = new Dictionary<string, string>();

		foreach (var entry in tableControl.Entries)
		{
			if (!entry.TryGetValue(StandardTableKeyNames.DocumentName, out string fileName))
			{
				break;
			}

			if (!fileDictionary.ContainsKey(fileName))
			{
				fileDictionary.Add(fileName, fileName);
			}
		}

#if TRACE
		var c = string.Join(Environment.NewLine, fileDictionary.Values);

		Trace.TraceInformation($"Attempting to open files:{Environment.NewLine}{c}");
#endif

		foreach (var file in fileDictionary.Values)
		{
			if (!await VS.Documents.IsOpenAsync(file))
			{
				await VS.Documents.OpenAsync(file);
			}
		}
	}
}
