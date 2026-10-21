using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DTech.Quorum.Editor
{
	internal static class BrainMapLayout
	{
		public const float ActionColumnX = 520f;
		public const float ColumnWidth = 240f;
		public const float RowHeight = 90f;
		public const float ActionGap = 40f;

		public static void Place(Node node, float x, float y)
		{
			node.SetPosition(new Rect(x, y, 0f, 0f));
		}

		public static float GetConsiderationColumnX(int depth) =>
			ActionColumnX - (depth + 1) * ColumnWidth;
	}
}
