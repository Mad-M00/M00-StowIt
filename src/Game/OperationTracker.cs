namespace M00StowIt.Game;

internal enum SortOperation
{
	None,
	Sort,
	Restock
}

// The single in-flight sort operation. Container locks are asynchronous, so
// patches consult this to suppress vanilla popups while an operation runs.
internal sealed class OperationTracker
{
	public SortOperation Current { get; set; } = SortOperation.None;

	public bool InProgress => Current != SortOperation.None;
}
