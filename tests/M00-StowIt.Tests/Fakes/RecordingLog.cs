using System.Collections.Generic;
using M00StowIt.Core;

namespace M00StowIt.Tests.Fakes;

internal sealed class RecordingLog : ISorterLog
{
	public List<string> Infos { get; } = new();

	public List<string> Warnings { get; } = new();

	public void Info(string message) => Infos.Add(message);

	public void Warning(string message) => Warnings.Add(message);
}
