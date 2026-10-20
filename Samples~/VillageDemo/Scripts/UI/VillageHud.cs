using System.Collections.Generic;
using UnityEngine;

namespace DTech.UtilityAI.Samples.Village
{
	/// <summary>
	/// IMGUI overlay: current action above every agent, needs bars, spawn buttons and time scale.
	/// IMGUI works with the Input System package, so the sample needs no input setup.
	/// </summary>
	public sealed class VillageHud : MonoBehaviour
	{
		private const float PanelWidth = 270f;
		private const float BarWidth = 60f;
		private const float BarHeight = 5f;

		private static readonly Color HungerColor = new(0.95f, 0.45f, 0.2f);
		private static readonly Color EnergyColor = new(0.3f, 0.6f, 1f);
		private static readonly Color StaminaColor = new(0.9f, 0.85f, 0.3f);
		private static readonly Color BarBackground = new(0f, 0f, 0f, 0.6f);

		[SerializeField]
		private VillageWorld _world = default;

		[SerializeField]
		private Camera _camera = default;

		[SerializeField]
		private float _labelHeight = 2.1f;

		private GUIStyle _labelStyle;
		private GUIStyle _titleStyle;

		private void OnGUI()
		{
			if (_world == null)
			{
				return;
			}

			if (_camera == null)
			{
				_camera = Camera.main;
			}

			_labelStyle ??= new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				fontStyle = FontStyle.Bold,
				normal = { textColor = Color.white },
			};

			_titleStyle ??= new GUIStyle(GUI.skin.label) { richText = true };

			if (_camera != null)
			{
				DrawVillagers(_world.Villagers);
				DrawWolves(_world.Wolves);
			}

			DrawPanel();
		}

		private void DrawVillagers(IReadOnlyList<VillagerAgent> villagers)
		{
			for (int i = 0; i < villagers.Count; i++)
			{
				VillagerAgent villager = villagers[i];
				if (villager == null || !TryGetScreenPoint(villager.transform.position, out Vector2 point))
				{
					continue;
				}

				string label = villager.CarriedWood > 0 ? $"{villager.CurrentActionLabel} (+{villager.CarriedWood})" : villager.CurrentActionLabel;
				DrawLabel(point, label);
				DrawBar(point + new Vector2(0f, 12f), villager.Hunger, HungerColor);
				DrawBar(point + new Vector2(0f, 19f), villager.Energy, EnergyColor);
			}
		}

		private void DrawWolves(IReadOnlyList<WolfAgent> wolves)
		{
			for (int i = 0; i < wolves.Count; i++)
			{
				WolfAgent wolf = wolves[i];
				if (wolf == null || !TryGetScreenPoint(wolf.transform.position, out Vector2 point))
				{
					continue;
				}

				DrawLabel(point, $"Wolf: {wolf.CurrentActionLabel}");
				DrawBar(point + new Vector2(0f, 12f), wolf.Stamina, StaminaColor);
			}
		}

		private void DrawPanel()
		{
			GUILayout.BeginArea(new Rect(10f, 10f, PanelWidth, Screen.height - 20f));
			GUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label("<b>Utility AI - Village Demo</b>", _titleStyle);
			GUILayout.Label($"Wood: {_world.Wood}   Food: {_world.FoodCount}");
			GUILayout.Label($"Villagers: {_world.Villagers.Count}   Wolves: {_world.Wolves.Count}");

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("+ Villager"))
			{
				_world.SpawnVillager();
			}

			if (GUILayout.Button("+ Food"))
			{
				_world.SpawnFood();
			}

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("+ Wolf"))
			{
				_world.SpawnWolf();
			}

			if (GUILayout.Button("- Wolf"))
			{
				_world.RemoveWolf();
			}

			GUILayout.EndHorizontal();
			GUILayout.Label($"Time scale: {Time.timeScale:0.00}");
			Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0.25f, 4f);
			GUILayout.Space(6f);
			GUILayout.Label("Bars: hunger (orange), energy (blue), stamina (yellow).");
			GUILayout.Label("Window/DTech/Utility AI/Debugger - live scores.\nWindow/DTech/Utility AI/Brain Map - config graph.");
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		private bool TryGetScreenPoint(Vector3 worldPosition, out Vector2 point)
		{
			Vector3 screen = _camera.WorldToScreenPoint(worldPosition + Vector3.up * _labelHeight);
			point = new Vector2(screen.x, Screen.height - screen.y);
			return screen.z > 0f;
		}

		private void DrawLabel(Vector2 point, string text)
		{
			var rect = new Rect(point.x - 70f, point.y - 10f, 140f, 20f);
			Color previous = GUI.color;
			GUI.color = Color.black;
			GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), text, _labelStyle);
			GUI.color = previous;
			GUI.Label(rect, text, _labelStyle);
		}

		private static void DrawBar(Vector2 point, float value, Color color)
		{
			var background = new Rect(point.x - BarWidth * 0.5f, point.y, BarWidth, BarHeight);
			Color previous = GUI.color;
			GUI.color = BarBackground;
			GUI.DrawTexture(background, Texture2D.whiteTexture);
			GUI.color = color;
			GUI.DrawTexture(new Rect(background.x, background.y, BarWidth * Mathf.Clamp01(value), BarHeight), Texture2D.whiteTexture);
			GUI.color = previous;
		}
	}
}
