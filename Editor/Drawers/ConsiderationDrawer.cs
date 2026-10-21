using System;
using UnityEditor;

namespace DTech.Quorum.Editor
{
	[CustomPropertyDrawer(typeof(Consideration), true)]
	internal sealed class ConsiderationDrawer : ReferenceDrawer
	{
		protected override Type BaseType => typeof(Consideration);
		protected override string Title => "Considerations";
	}
}
