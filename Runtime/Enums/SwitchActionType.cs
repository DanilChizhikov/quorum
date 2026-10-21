namespace DTech.Quorum
{
	public enum SwitchActionType : byte
	{
		/// <summary>
		/// The brain picks again as soon as the current action reports Completed or Failed.
		/// </summary>
		OnActionFinished = 0,

		/// <summary>
		/// The brain only picks when RequestSwitchAction is called.
		/// </summary>
		OnlyRequest = 1,
	}
}
