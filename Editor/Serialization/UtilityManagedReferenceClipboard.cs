using System;
using System.Collections.Generic;
using UnityEditor;

namespace DTech.UtilityAI.Editor
{
	internal static class UtilityManagedReferenceClipboard
	{
		private const string Prefix = "UtilityAI.ManagedRef:";

		public static void Copy(object value)
		{
			if (value == null)
			{
				return;
			}

			Write(UtilityManagedReferenceSerializer.Serialize(value));
		}

		public static void CopyMany(IReadOnlyList<object> values)
		{
			Write(UtilityManagedReferenceSerializer.SerializeMany(values));
		}

		public static bool CanPaste(Type baseType)
		{
			if (!TryReadPayload(out string payload))
			{
				return false;
			}

			Type rootType = UtilityManagedReferenceSerializer.PeekRootType(payload);
			return rootType != null && (baseType == null || baseType.IsAssignableFrom(rootType));
		}

		public static bool TryPaste(Type baseType, out object value)
		{
			value = null;
			if (!TryReadPayload(out string payload))
			{
				return false;
			}

			value = UtilityManagedReferenceSerializer.Deserialize(payload, baseType);
			return value != null;
		}

		public static bool TryPasteMany(Type baseType, out List<object> values)
		{
			values = null;
			if (!TryReadPayload(out string payload))
			{
				return false;
			}

			values = UtilityManagedReferenceSerializer.DeserializeAll(payload, baseType);
			return values.Count > 0;
		}

		private static void Write(string payload)
		{
			if (string.IsNullOrEmpty(payload))
			{
				return;
			}

			EditorGUIUtility.systemCopyBuffer = string.Concat(Prefix, payload);
		}

		private static bool TryReadPayload(out string payload)
		{
			payload = null;
			string buffer = EditorGUIUtility.systemCopyBuffer;
			if (string.IsNullOrEmpty(buffer) || !buffer.StartsWith(Prefix, StringComparison.Ordinal))
			{
				return false;
			}

			payload = buffer.Substring(Prefix.Length);
			return !string.IsNullOrEmpty(payload);
		}
	}
}
