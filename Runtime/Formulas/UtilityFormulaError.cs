namespace DTech.UtilityAI
{
	/// <summary>
	/// Why a formula failed to compile, and where. The default value means the formula is valid.
	/// </summary>
	public readonly struct UtilityFormulaError
	{
		/// <summary>
		/// Gets whether the formula compiled, which is the case when there is no message.
		/// </summary>
		public bool IsValid => string.IsNullOrEmpty(Message);

		/// <summary>
		/// Gets what went wrong, or null when the formula compiled.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// Gets the character index in the formula the message refers to.
		/// </summary>
		public int Position { get; }

		public UtilityFormulaError(string message, int position)
		{
			Message = message;
			Position = position;
		}

		/// <summary>
		/// Builds the message with its position, as the inspector shows it.
		/// </summary>
		/// <returns>Message followed by the position, or an empty string when the formula is valid.</returns>
		public override string ToString() => IsValid ? string.Empty : $"{Message} at {Position}";
	}
}
