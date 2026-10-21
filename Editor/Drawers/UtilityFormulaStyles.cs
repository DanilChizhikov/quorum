using UnityEditor;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	internal static class UtilityFormulaStyles
	{
		private const string MonospaceFontPath = "Fonts/RobotoMono/RobotoMono-Regular.ttf";
		private const int MonospaceFallbackSize = 12;
		private const int BracketColorCount = 4;

		private static readonly string[] _monospaceFallbacks = { "Menlo", "Consolas", "Courier New" };

		public static Color Valid => IsDark ? new Color32(0x4D, 0xB3, 0x6B, 0xFF) : new Color32(0x1A, 0x7F, 0x37, 0xFF);
		public static Color Invalid => IsDark ? new Color32(0xDE, 0x43, 0x43, 0xFF) : new Color32(0xCF, 0x22, 0x2E, 0xFF);
		public static Color Warning => IsDark ? new Color32(0xE8, 0x87, 0x3C, 0xFF) : new Color32(0xB3, 0x47, 0x00, 0xFF);
		public static Color Function => IsDark ? new Color32(0xE5, 0xC0, 0x7B, 0xFF) : new Color32(0x9A, 0x67, 0x00, 0xFF);
		public static Color Variable => IsDark ? new Color32(0x61, 0xAF, 0xEF, 0xFF) : new Color32(0x05, 0x50, 0xAE, 0xFF);
		public static Color Number => IsDark ? new Color32(0x98, 0xC3, 0x79, 0xFF) : new Color32(0x11, 0x63, 0x29, 0xFF);
		public static Color Constant => IsDark ? new Color32(0xC6, 0x78, 0xDD, 0xFF) : new Color32(0x82, 0x50, 0xDF, 0xFF);
		public static Color Operator => IsDark ? new Color32(0x9B, 0x9B, 0x9B, 0xFF) : new Color32(0x66, 0x66, 0x66, 0xFF);

		public static GUIStyle FieldStyle
		{
			get
			{
				if (_fieldStyle == null || _fieldStyle.font == null)
				{
					_fieldStyle = new GUIStyle(EditorStyles.textField) { font = Monospace };
				}

				return _fieldStyle;
			}
		}

		public static GUIStyle TextStyle
		{
			get
			{
				if (_textStyle == null || _textStyle.font == null)
				{
					_textStyle = new GUIStyle(EditorStyles.label)
					{
						font = Monospace,
						padding = new RectOffset(0, 0, 0, 0),
						wordWrap = false,
					};

					_textStyle.normal.textColor = Color.white;
				}

				return _textStyle;
			}
		}

		private static Font _monospace;
		private static GUIStyle _fieldStyle;
		private static GUIStyle _textStyle;

		private static bool IsDark => EditorGUIUtility.isProSkin;

		private static Font Monospace
		{
			get
			{
				if (_monospace == null)
				{
					_monospace = EditorGUIUtility.Load(MonospaceFontPath) as Font;
					if (_monospace == null)
					{
						_monospace = Font.CreateDynamicFontFromOSFont(_monospaceFallbacks, MonospaceFallbackSize);
					}
				}

				return _monospace;
			}
		}

		public static Color GetBracketColor(int depth)
		{
			int index = depth < 0 ? 0 : depth % BracketColorCount;
			switch (index)
			{
				case 0:
				{
					return IsDark ? new Color32(0xD1, 0x9A, 0x66, 0xFF) : new Color32(0xBC, 0x4C, 0x00, 0xFF);
				}
				case 1:
				{
					return IsDark ? new Color32(0x56, 0xB6, 0xC2, 0xFF) : new Color32(0x09, 0x69, 0xDA, 0xFF);
				}
				case 2:
				{
					return IsDark ? new Color32(0xE0, 0x6C, 0x9F, 0xFF) : new Color32(0xA4, 0x0E, 0x26, 0xFF);
				}
				default:
				{
					return IsDark ? new Color32(0xB5, 0xCE, 0xA8, 0xFF) : new Color32(0x4C, 0x7A, 0x2E, 0xFF);
				}
			}
		}
	}
}
