using System.Collections.Generic;
using DTech.Blackboard;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	[CustomPropertyDrawer(typeof(UtilityFormulaAttribute))]
	internal sealed class UtilityFormulaDrawer : PropertyDrawer
	{
		private const int CacheLimit = 256;
		private const float GlowMargin = 4f;
		private const float RingThickness = 1f;
		private const float PickerWidth = 20f;
		private const float PickerSpacing = 2f;
		private const string OperatorSymbols = "+-*/%^<>!&|=";
		private const string UnknownPrefix = "Not on the blackboard: ";

		private static readonly float[] _ringAlphas = { 1f, 0.5f, 0.25f, 0.1f };
		private static readonly Dictionary<string, FormulaInfo> _cache = new();
		private static readonly GUIContent _pickerContent = new("▾", "Insert a blackboard variable name");

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.String)
			{
				return EditorGUI.GetPropertyHeight(property, label, true);
			}

			FormulaInfo info = GetInfo(property.stringValue);
			float height = GlowMargin + EditorGUIUtility.singleLineHeight + GlowMargin;
			if (info.HasSpans)
			{
				height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
			}

			if (GetMessage(info, GetSchema(property)) != null)
			{
				height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
			}

			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.String)
			{
				EditorGUI.PropertyField(position, property, label, true);
				return;
			}

			var labelRect = new Rect(position.x, position.y + GlowMargin, position.width, EditorGUIUtility.singleLineHeight);
			Rect contentRect = EditorGUI.PrefixLabel(labelRect, label);
			int previousIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			try
			{
				Draw(contentRect, property);
			}
			finally
			{
				EditorGUI.indentLevel = previousIndent;
			}
		}

		private static FormulaInfo GetInfo(string expression)
		{
			string key = expression ?? string.Empty;
			if (_cache.TryGetValue(key, out FormulaInfo cached))
			{
				return cached;
			}

			if (_cache.Count >= CacheLimit)
			{
				_cache.Clear();
			}

			var tokens = new List<UtilityFormulaToken>();
			UtilityFormulaTokenizer.TryTokenize(key, tokens, out _);
			UtilityFormulaCompiler.TryCompile(key, out _, out UtilityFormulaError error);
			var info = new FormulaInfo(error, UtilityFormulaHighlighter.Build(key, tokens));
			_cache[key] = info;
			return info;
		}

		private static BlackboardAsset GetSchema(SerializedProperty property) =>
			property.serializedObject.targetObject is BrainConfig config ? config.BlackboardSchema : null;

		private static string GetMessage(in FormulaInfo info, BlackboardAsset schema)
		{
			if (!info.Error.IsValid)
			{
				return info.Error.ToString();
			}

			string unknown = UtilityFormulaSchema.GetUnknownNames(schema, info.Spans);
			return string.IsNullOrEmpty(unknown) ? null : UnknownPrefix + unknown;
		}

		private static void DrawGlow(Rect field, Color color)
		{
			for (int i = 0; i < _ringAlphas.Length; i++)
			{
				var inner = new Rect(field.x - i, field.y - i, field.width + i * 2f, field.height + i * 2f);
				DrawRing(inner, new Color(color.r, color.g, color.b, _ringAlphas[i]));
			}
		}

		private static void DrawRing(Rect inner, Color color)
		{
			float span = inner.width + RingThickness * 2f;
			EditorGUI.DrawRect(new Rect(inner.x - RingThickness, inner.y - RingThickness, span, RingThickness), color);
			EditorGUI.DrawRect(new Rect(inner.x - RingThickness, inner.yMax, span, RingThickness), color);
			EditorGUI.DrawRect(new Rect(inner.x - RingThickness, inner.y, RingThickness, inner.height), color);
			EditorGUI.DrawRect(new Rect(inner.xMax, inner.y, RingThickness, inner.height), color);
		}

		private static void OpenNamePicker(Rect buttonRect, SerializedProperty property, BlackboardAsset schema)
		{
			Object targetObject = property.serializedObject.targetObject;
			string propertyPath = property.propertyPath;
			UtilityFormulaNameSearchProvider provider = UtilityFormulaNameSearchProvider.Create(
				schema,
				name => AppendName(targetObject, propertyPath, name));

			var searchContext = new SearchWindowContext(
				GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.x, buttonRect.yMax)));

			SearchWindow.Open(searchContext, provider);
		}

		private static void AppendName(Object targetObject, string propertyPath, string name)
		{
			if (targetObject == null || string.IsNullOrEmpty(name))
			{
				return;
			}

			using var serializedObject = new SerializedObject(targetObject);
			SerializedProperty property = serializedObject.FindProperty(propertyPath);
			if (property == null)
			{
				return;
			}

			property.stringValue = Combine(property.stringValue, GetToken(name));
			serializedObject.ApplyModifiedProperties();
		}

		private static string GetToken(string name) =>
			IsPlainIdentifier(name) && !UtilityFormulaFunctions.TryGetConstant(name, out _)
				? name
				: string.Concat("{", name, "}");

		private static bool IsPlainIdentifier(string name)
		{
			if (string.IsNullOrEmpty(name) || (name[0] != '_' && !char.IsLetter(name[0])))
			{
				return false;
			}

			for (int i = 1; i < name.Length; i++)
			{
				if (name[i] != '_' && !char.IsLetterOrDigit(name[i]))
				{
					return false;
				}
			}

			return true;
		}

		private static string Combine(string expression, string token)
		{
			string current = expression ?? string.Empty;
			if (current.Length <= 0)
			{
				return token;
			}

			char last = current[current.Length - 1];
			bool joined = char.IsWhiteSpace(last) || last == '(' || last == ',' || OperatorSymbols.IndexOf(last) >= 0;
			return joined ? string.Concat(current, token) : string.Concat(current, " ", token);
		}

		private static void Draw(Rect contentRect, SerializedProperty property)
		{
			BlackboardAsset schema = GetSchema(property);
			bool hasPicker = UtilityFormulaSchema.HasNames(schema);
			float pickerSpan = hasPicker ? PickerWidth + PickerSpacing : 0f;
			var fieldRect = new Rect(
				contentRect.x + GlowMargin,
				contentRect.y,
				Mathf.Max(0f, contentRect.width - GlowMargin * 2f - pickerSpan),
				contentRect.height);

			EditorGUI.BeginChangeCheck();
			string expression = EditorGUI.TextField(fieldRect, property.stringValue, UtilityFormulaStyles.FieldStyle);
			if (EditorGUI.EndChangeCheck())
			{
				property.stringValue = expression;
			}

			FormulaInfo info = GetInfo(expression);
			DrawGlow(fieldRect, info.Error.IsValid ? UtilityFormulaStyles.Valid : UtilityFormulaStyles.Invalid);
			if (hasPicker)
			{
				var pickerRect = new Rect(contentRect.xMax - PickerWidth, contentRect.y, PickerWidth, contentRect.height);
				if (GUI.Button(pickerRect, _pickerContent, EditorStyles.miniButton))
				{
					OpenNamePicker(pickerRect, property, schema);
				}
			}

			float y = fieldRect.yMax + GlowMargin;
			if (info.HasSpans)
			{
				y += EditorGUIUtility.standardVerticalSpacing;
				UtilityFormulaHighlighter.Draw(new UtilityFormulaHighlightRequest(
					new Rect(fieldRect.x, y, fieldRect.width, EditorGUIUtility.singleLineHeight),
					expression,
					info.Spans,
					schema));

				y += EditorGUIUtility.singleLineHeight;
			}

			string message = GetMessage(info, schema);
			if (message == null)
			{
				return;
			}

			y += EditorGUIUtility.standardVerticalSpacing;
			var messageRect = new Rect(fieldRect.x, y, fieldRect.width, EditorGUIUtility.singleLineHeight);
			Color previousColor = GUI.color;
			GUI.color = info.Error.IsValid ? UtilityFormulaStyles.Warning : UtilityFormulaStyles.Invalid;
			EditorGUI.LabelField(messageRect, message, EditorStyles.miniLabel);
			GUI.color = previousColor;
		}

		private readonly struct FormulaInfo
		{
			public UtilityFormulaError Error { get; }
			public UtilityFormulaHighlightSpan[] Spans { get; }
			public bool HasSpans => Spans != null && Spans.Length > 0;

			public FormulaInfo(UtilityFormulaError error, UtilityFormulaHighlightSpan[] spans)
			{
				Error = error;
				Spans = spans;
			}
		}
	}
}
