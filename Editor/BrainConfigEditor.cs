using UnityEditor;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	[CustomEditor(typeof(BrainConfig), true)]
	public class BrainConfigEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			using (new EditorGUI.DisabledScope(Application.isPlaying))
			{
				DrawDefaultInspector();
			}
		}
	}
}