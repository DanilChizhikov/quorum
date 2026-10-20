using System;

namespace DTech.UtilityAI
{
	/// <summary>
	/// Declares a type name this type was serialized under before a rename or a move.
	/// The editor uses it to migrate SerializeReference data instead of dropping it.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class UtilityFormerlySerializedTypeAttribute : Attribute
	{
		/// <summary>
		/// Former type name in the "Namespace.TypeName, AssemblyName" form.
		/// </summary>
		public string TypeName { get; }

		/// <summary>
		/// Creates the attribute for one former type name.
		/// </summary>
		/// <param name="typeName">Former type name in the "Namespace.TypeName, AssemblyName" form.</param>
		public UtilityFormerlySerializedTypeAttribute(string typeName)
		{
			TypeName = typeName;
		}
	}
}
