using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace DTech.UtilityAI.Editor
{
	internal static class BrainConfigValidator
	{
		private const string ActionsPropertyPath = "_actionEntries";
		private const string ConsiderationPropertyPath = "_consideration";
		private const string ScoreAggregatorPropertyPath = "_scoreAggregator";
		private const string ConsiderationFieldName = "_consideration";
		private const string CompositeChildrenFieldName = "_considerations";
		private const string CalculateUtilityMethodName = "CalculateUtility";

		private const BindingFlags MethodFlags = BindingFlags.Instance |
												BindingFlags.Public |
												BindingFlags.NonPublic |
												BindingFlags.DeclaredOnly;

		public static void Validate(BrainConfig config, List<BrainConfigIssue> issues)
		{
			if (config == null || issues == null)
			{
				return;
			}

			ValidateScoreAggregator(config, issues);
			ValidateActions(config, issues);
			ValidateManagedTypes(config, issues);
			ValidateGraph(config, issues);
		}

		private static void ValidateScoreAggregator(BrainConfig config, List<BrainConfigIssue> issues)
		{
			if (config.ScoreAggregator != null)
			{
				return;
			}

			issues.Add(new BrainConfigIssue(
				config,
				BrainConfigIssueSeverity.Error,
				BrainConfigIssueCode.MissingScoreAggregator,
				"Score aggregator is not set: the brain logs an error and never picks an action.",
				ScoreAggregatorPropertyPath));
		}

		private static void ValidateActions(BrainConfig config, List<BrainConfigIssue> issues)
		{
			IReadOnlyList<UtilityAction> actions = config.Actions;
			if (actions == null || actions.Count <= 0)
			{
				issues.Add(new BrainConfigIssue(
					config,
					BrainConfigIssueSeverity.Warning,
					BrainConfigIssueCode.EmptyActionList,
					"Action list is empty: the brain has nothing to pick.",
					ActionsPropertyPath));

				return;
			}

			var actionTypes = new HashSet<Type>();
			for (int i = 0; i < actions.Count; i++)
			{
				UtilityAction action = actions[i];
				if (action == null)
				{
					issues.Add(new BrainConfigIssue(
						config,
						BrainConfigIssueSeverity.Error,
						BrainConfigIssueCode.NullActionEntry,
						$"Action {i} is empty.",
						ActionsPropertyPath));

					continue;
				}

				ValidateScriptableReference(config, issues, action, i);
				ValidateScoreSource(config, issues, action, i);
				if (action is ScriptableUtilityActionReference || actionTypes.Add(action.GetType()))
				{
					continue;
				}

				issues.Add(new BrainConfigIssue(
					config,
					BrainConfigIssueSeverity.Warning,
					BrainConfigIssueCode.DuplicateActionType,
					$"Action {i} ({GetActionName(action)}) repeats a type already present in the list.",
					ActionsPropertyPath));
			}
		}

		private static void ValidateScriptableReference(BrainConfig config, List<BrainConfigIssue> issues, UtilityAction action, int index)
		{
			if (action is not ScriptableUtilityActionReference reference || reference.Action != null)
			{
				return;
			}

			issues.Add(new BrainConfigIssue(
				config,
				BrainConfigIssueSeverity.Error,
				BrainConfigIssueCode.EmptyScriptableActionReference,
				$"Action {index} references no ScriptableUtilityAction asset and always fails.",
				ActionsPropertyPath));
		}

		private static void ValidateScoreSource(BrainConfig config, List<BrainConfigIssue> issues, UtilityAction action, int index)
		{
			if (GetConsideration(action) != null || HasCustomUtility(action.GetType()))
			{
				return;
			}

			issues.Add(new BrainConfigIssue(
				config,
				BrainConfigIssueSeverity.Warning,
				BrainConfigIssueCode.ActionWithoutScoreSource,
				$"Action {index} ({GetActionName(action)}) has no consideration and no own CalculateUtility: its score is always 0.",
				ActionsPropertyPath));
		}

		private static void ValidateManagedTypes(BrainConfig config, List<BrainConfigIssue> issues)
		{
			using var serializedObject = new SerializedObject(config);
			SerializedProperty iterator = serializedObject.GetIterator();
			while (iterator.NextVisible(true))
			{
				if (iterator.propertyType != SerializedPropertyType.ManagedReference)
				{
					continue;
				}

				if (string.IsNullOrEmpty(iterator.managedReferenceFullTypename) || iterator.managedReferenceValue != null)
				{
					continue;
				}

				issues.Add(new BrainConfigIssue(
					config,
					BrainConfigIssueSeverity.Error,
					BrainConfigIssueCode.MissingManagedType,
					$"Missing type {iterator.managedReferenceFullTypename} at {iterator.propertyPath}.",
					iterator.propertyPath));
			}
		}

		private static void ValidateGraph(BrainConfig config, List<BrainConfigIssue> issues)
		{
			var visited = new HashSet<object>();
			IReadOnlyList<UtilityAction> actions = config.Actions;
			if (actions != null)
			{
				for (int i = 0; i < actions.Count; i++)
				{
					ValidateNode(config, issues, actions[i], visited);
				}
			}
			
			ValidateNode(config, issues, config.ScoreAggregator, visited);
		}

		private static void ValidateNode(BrainConfig config, List<BrainConfigIssue> issues, object node, HashSet<object> visited)
		{
			if (node == null || !visited.Add(node))
			{
				return;
			}

			ValidateComposite(config, issues, node);
			ValidateFormulas(config, issues, node);
			Type type = node.GetType();
			IReadOnlyList<FieldInfo> fields = UtilitySerializableFieldCache.GetFields(type);
			for (int i = 0; i < fields.Count; i++)
			{
				FieldInfo field = fields[i];
				if (!UtilitySerializableFieldCache.IsManagedReference(field))
				{
					continue;
				}

				object value = field.GetValue(node);
				if (value == null)
				{
					continue;
				}

				if (!UtilitySerializableFieldCache.IsCollection(field.FieldType))
				{
					ValidateNode(config, issues, value, visited);
					continue;
				}

				if (value is not IList list)
				{
					continue;
				}

				for (int elementIndex = 0; elementIndex < list.Count; elementIndex++)
				{
					object element = list[elementIndex];
					if (element == null)
					{
						issues.Add(new BrainConfigIssue(
							config,
							BrainConfigIssueSeverity.Warning,
							BrainConfigIssueCode.NullManagedReferenceElement,
							$"{UtilityTypeRegistry.GetDisplayName(type)} has an empty entry {elementIndex} in {field.Name}."));

						continue;
					}

					ValidateNode(config, issues, element, visited);
				}
			}
		}

		private static void ValidateFormulas(BrainConfig config, List<BrainConfigIssue> issues, object node)
		{
			Type type = node.GetType();
			IReadOnlyList<FieldInfo> fields = UtilitySerializableFieldCache.GetFields(type);
			for (int i = 0; i < fields.Count; i++)
			{
				FieldInfo field = fields[i];
				if (field.FieldType != typeof(string) || !Attribute.IsDefined(field, typeof(UtilityFormulaAttribute)))
				{
					continue;
				}

				if (!UtilityFormulaCompiler.TryCompile(field.GetValue(node) as string, out UtilityFormulaProgram program, out UtilityFormulaError error))
				{
					issues.Add(new BrainConfigIssue(
						config,
						BrainConfigIssueSeverity.Error,
						BrainConfigIssueCode.InvalidFormula,
						$"{UtilityTypeRegistry.GetDisplayName(type)} has an invalid formula in {field.Name}: {error}."));

					continue;
				}

				string unknown = UtilityFormulaSchema.GetUnknownNames(config.BlackboardSchema, program.VariableNames);
				if (string.IsNullOrEmpty(unknown))
				{
					continue;
				}

				issues.Add(new BrainConfigIssue(
					config,
					BrainConfigIssueSeverity.Warning,
					BrainConfigIssueCode.UnknownFormulaVariable,
					$"{UtilityTypeRegistry.GetDisplayName(type)} reads names in {field.Name} that the blackboard schema does not have: {unknown}."));
			}
		}

		private static void ValidateComposite(BrainConfig config, List<BrainConfigIssue> issues, object node)
		{
			if (node is not CompositeUtilityConsideration)
			{
				return;
			}

			FieldInfo field = UtilitySerializableFieldCache.GetField(node.GetType(), CompositeChildrenFieldName);
			int count = field?.GetValue(node) is IList list ? list.Count : 0;
			if (count <= 0)
			{
				issues.Add(new BrainConfigIssue(
					config,
					BrainConfigIssueSeverity.Warning,
					BrainConfigIssueCode.CompositeWithoutChildren,
					"Composite consideration has no children and evaluates to 0.",
					ConsiderationPropertyPath));

				return;
			}

			if (count > 1)
			{
				return;
			}

			issues.Add(new BrainConfigIssue(
				config,
				BrainConfigIssueSeverity.Info,
				BrainConfigIssueCode.CompositeWithSingleChild,
				"Composite consideration wraps a single child and can be replaced by it.",
				ConsiderationPropertyPath));
		}

		private static UtilityConsideration GetConsideration(UtilityAction action)
		{
			FieldInfo field = UtilitySerializableFieldCache.GetField(action.GetType(), ConsiderationFieldName);
			return field?.GetValue(action) as UtilityConsideration;
		}

		private static bool HasCustomUtility(Type type)
		{
			for (Type current = type; current != null; current = current.BaseType)
			{
				if (IsFrameworkActionType(current))
				{
					return false;
				}

				MethodInfo[] methods = current.GetMethods(MethodFlags);
				for (int i = 0; i < methods.Length; i++)
				{
					if (!string.Equals(methods[i].Name, CalculateUtilityMethodName, StringComparison.Ordinal))
					{
						continue;
					}

					return true;
				}
			}

			return false;
		}

		private static bool IsFrameworkActionType(Type type)
		{
			if (type == typeof(UtilityAction))
			{
				return true;
			}

			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(UtilityAction<,>);
		}

		private static string GetActionName(UtilityAction action) =>
			UtilityTypeRegistry.GetDisplayName(action.GetType());
	}
}
