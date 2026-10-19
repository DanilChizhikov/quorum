using System;
using UnityEditor;

namespace DTech.UtilityAI.Editor
{
	[CustomPropertyDrawer(typeof(UtilityAction), true)]
	internal sealed class UtilityActionDrawer : UtilityReferenceDrawer
	{
		protected override Type BaseType => typeof(UtilityAction);
		protected override string Title => "Actions";
	}
}
