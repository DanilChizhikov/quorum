using System;
using UnityEngine;

namespace DTech.Quorum
{
	/// <summary>
	/// Marks a serialized string field as a utility formula. The inspector draws it with live
	/// validation - green while the formula compiles, red while it does not - and the Quorum
	/// validator reports every field carrying this attribute that holds a formula it cannot compile.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class FormulaAttribute : PropertyAttribute { }
}
