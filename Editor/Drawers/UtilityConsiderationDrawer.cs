using System;
using UnityEditor;

namespace DTech.UtilityAI.Editor
{
	[CustomPropertyDrawer(typeof(UtilityConsideration), true)]
	internal sealed class UtilityConsiderationDrawer : UtilityReferenceDrawer
	{
		protected override Type BaseType => typeof(UtilityConsideration);
		protected override string Title => "Considerations";
	}
}
