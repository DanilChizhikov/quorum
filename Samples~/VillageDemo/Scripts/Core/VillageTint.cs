using UnityEngine;

namespace DTech.UtilityAI.Samples.Village
{
	/// <summary>
	/// Colours the renderers below it through a MaterialPropertyBlock, so the sample needs no material assets.
	/// </summary>
	[ExecuteAlways]
	public sealed class VillageTint : MonoBehaviour
	{
		private static readonly int ColorId = Shader.PropertyToID("_Color");
		private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

		[SerializeField]
		private Color _color = Color.white;

		private MaterialPropertyBlock _block;

		public void SetColor(Color color)
		{
			if (_color == color)
			{
				return;
			}

			_color = color;
			Apply();
		}

		private void OnEnable()
		{
			Apply();
		}

		private void OnValidate()
		{
			Apply();
		}

		private void Apply()
		{
			_block ??= new MaterialPropertyBlock();
			_block.SetColor(ColorId, _color);
			_block.SetColor(BaseColorId, _color);
			Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].SetPropertyBlock(_block);
			}
		}
	}
}
