using System;

namespace DTech.UtilityAI.Editor
{
	internal readonly struct UtilityTypeMigration
	{
		public string AssetPath { get; }
		public string FormerTypeName { get; }
		public Type TargetType { get; }
		public int Count { get; }

		public UtilityTypeMigration(string assetPath, string formerTypeName, Type targetType, int count)
		{
			AssetPath = assetPath;
			FormerTypeName = formerTypeName;
			TargetType = targetType;
			Count = count;
		}
	}
}
