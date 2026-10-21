using System;

namespace DTech.Quorum.Editor
{
	internal readonly struct TypeMigration
	{
		public string AssetPath { get; }
		public string FormerTypeName { get; }
		public Type TargetType { get; }
		public int Count { get; }

		public TypeMigration(string assetPath, string formerTypeName, Type targetType, int count)
		{
			AssetPath = assetPath;
			FormerTypeName = formerTypeName;
			TargetType = targetType;
			Count = count;
		}
	}
}
