namespace DTech.Quorum.Editor.Update
{
	internal enum UpdateStatus : byte
	{
		Unknown = 0,
		Unsupported = 1,
		Checking = 2,
		UpToDate = 3,
		Available = 4,
		Installing = 5,
		Failed = 6,
	}
}
