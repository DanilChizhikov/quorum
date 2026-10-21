using System;
using UnityEditor;

namespace DTech.Quorum.Editor
{
	[CustomPropertyDrawer(typeof(QuorumAction), true)]
	internal sealed class ActionDrawer : ReferenceDrawer
	{
		protected override Type BaseType => typeof(QuorumAction);
		protected override string Title => "Actions";
	}
}
