using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace DTech.Quorum.Editor
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
			ValidateSharedAssetFormulas(config, issues);
			ValidateContextTypes(config, issues);
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
			IReadOnlyList<QuorumAction> actions = config.Actions;
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
				QuorumAction action = actions[i];
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
				if (action is ScriptableQuorumActionReference || actionTypes.Add(action.GetType()))
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

		private static void ValidateScriptableReference(BrainConfig config, List<BrainConfigIssue> issues, QuorumAction action, int index)
		{
			if (action is not ScriptableQuorumActionReference reference || reference.Action != null)
			{
				return;
			}

			issues.Add(new BrainConfigIssue(
				config,
				BrainConfigIssueSeverity.Error,
				BrainConfigIssueCode.EmptyScriptableActionReference,
				$"Action {index} references no ScriptableQuorumAction asset and always fails.",
				ActionsPropertyPath));
		}

		private static void ValidateScoreSource(BrainConfig config, List<BrainConfigIssue> issues, QuorumAction action, int index)
		{
			object scoreSource = action is ScriptableQuorumActionReference reference ? reference.Action : action;
			if (scoreSource == null || GetConsideration(scoreSource) != null || HasCustomUtility(scoreSource.GetType()))
			{
				return;
			}

			issues.Add(new BrainConfigIssue(
				config,
				BrainConfigIssueSeverity.Warning,
				BrainConfigIssueCode.ActionWithoutScoreSource,
				$"Action {index} ({GetScoreSourceName(scoreSource)}) has no consideration and no own CalculateUtility: its score is always 0.",
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
			IReadOnlyList<QuorumAction> actions = config.Actions;
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
			IReadOnlyList<FieldInfo> fields = SerializableFieldCache.GetFields(type);
			for (int i = 0; i < fields.Count; i++)
			{
				FieldInfo field = fields[i];
				if (!SerializableFieldCache.IsManagedReference(field))
				{
					continue;
				}

				object value = field.GetValue(node);
				if (value == null)
				{
					continue;
				}

				if (!SerializableFieldCache.IsCollection(field.FieldType))
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
							$"{TypeRegistry.GetDisplayName(type)} has an empty entry {elementIndex} in {field.Name}."));

						continue;
					}

					ValidateNode(config, issues, element, visited);
				}
			}
		}

		private static void ValidateFormulas(BrainConfig config, List<BrainConfigIssue> issues, object node)
		{
			Type type = node.GetType();
			IReadOnlyList<FieldInfo> fields = SerializableFieldCache.GetFields(type);
			for (int i = 0; i < fields.Count; i++)
			{
				FieldInfo field = fields[i];
				if (field.FieldType != typeof(string) || !Attribute.IsDefined(field, typeof(FormulaAttribute)))
				{
					continue;
				}

				if (!FormulaCompiler.TryCompile(field.GetValue(node) as string, out FormulaProgram program, out FormulaError error))
				{
					issues.Add(new BrainConfigIssue(
						config,
						BrainConfigIssueSeverity.Error,
						BrainConfigIssueCode.InvalidFormula,
						$"{TypeRegistry.GetDisplayName(type)} has an invalid formula in {field.Name}: {error}."));

					continue;
				}

				string unknown = FormulaSchema.GetUnknownNames(config.BlackboardSchema, program.VariableNames);
				if (string.IsNullOrEmpty(unknown))
				{
					continue;
				}

				issues.Add(new BrainConfigIssue(
					config,
					BrainConfigIssueSeverity.Warning,
					BrainConfigIssueCode.UnknownFormulaVariable,
					$"{TypeRegistry.GetDisplayName(type)} reads names in {field.Name} that the blackboard schema does not have: {unknown}."));
			}
		}

		private static void ValidateComposite(BrainConfig config, List<BrainConfigIssue> issues, object node)
		{
			if (node is not CompositeConsideration)
			{
				return;
			}

			FieldInfo field = SerializableFieldCache.GetField(node.GetType(), CompositeChildrenFieldName);
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

		private static void ValidateSharedAssetFormulas(BrainConfig config, List<BrainConfigIssue> issues)
		{
			IReadOnlyList<QuorumAction> actions = config.Actions;
			if (actions == null)
			{
				return;
			}

			for (int i = 0; i < actions.Count; i++)
			{
				if (actions[i] is not ScriptableQuorumActionReference reference || reference.Action == null)
				{
					continue;
				}

				if (!ContainsFormula(reference.Action, new HashSet<object>()))
				{
					continue;
				}

				issues.Add(new BrainConfigIssue(
					config,
					BrainConfigIssueSeverity.Error,
					BrainConfigIssueCode.FormulaOnSharedAsset,
					$"Action {i} ({reference.Action.name}) keeps a formula on a shared asset: " +
					"nothing on the asset gets Initialize, so the formula never binds and always scores 0. " +
					"Move the formula to a per-brain action.",
					ActionsPropertyPath));
			}
		}

		private static bool ContainsFormula(object node, HashSet<object> visited)
		{
			if (node == null || !visited.Add(node))
			{
				return false;
			}

			if (node is FormulaConsideration)
			{
				return true;
			}

			IReadOnlyList<FieldInfo> fields = SerializableFieldCache.GetFields(node.GetType());
			for (int i = 0; i < fields.Count; i++)
			{
				FieldInfo field = fields[i];
				if (!SerializableFieldCache.IsManagedReference(field))
				{
					continue;
				}

				object value = field.GetValue(node);
				if (value == null)
				{
					continue;
				}

				if (!SerializableFieldCache.IsCollection(field.FieldType))
				{
					if (ContainsFormula(value, visited))
					{
						return true;
					}

					continue;
				}

				if (value is not IList list)
				{
					continue;
				}

				for (int elementIndex = 0; elementIndex < list.Count; elementIndex++)
				{
					if (ContainsFormula(list[elementIndex], visited))
					{
						return true;
					}
				}
			}

			return false;
		}

		private static void ValidateContextTypes(BrainConfig config, List<BrainConfigIssue> issues)
		{
			IReadOnlyList<QuorumAction> actions = config.Actions;
			if (actions == null || actions.Count <= 1)
			{
				return;
			}

			var contextTypes = new Dictionary<Type, int>();
			for (int i = 0; i < actions.Count; i++)
			{
				QuorumAction action = actions[i];
				Type contextType = action != null ? GetContextType(action) : null;
				if (contextType == null || contextTypes.ContainsKey(contextType))
				{
					continue;
				}

				contextTypes.Add(contextType, i);
			}

			if (contextTypes.Count <= 1 || HasCommonContext(contextTypes.Keys))
			{
				return;
			}

			var names = new List<string>(contextTypes.Count);
			foreach (KeyValuePair<Type, int> pair in contextTypes)
			{
				names.Add($"{pair.Key.Name} (action {pair.Value})");
			}

			issues.Add(new BrainConfigIssue(
				config,
				BrainConfigIssueSeverity.Error,
				BrainConfigIssueCode.ContextTypeMismatch,
				$"Actions expect different contexts: {string.Join(", ", names)}. " +
				"Only the actions matching the brain context run; the rest fail and score 0.",
				ActionsPropertyPath));
		}

		private static bool HasCommonContext(Dictionary<Type, int>.KeyCollection contextTypes)
		{
			foreach (Type candidate in contextTypes)
			{
				bool isCommon = true;
				foreach (Type contextType in contextTypes)
				{
					if (contextType.IsAssignableFrom(candidate))
					{
						continue;
					}

					isCommon = false;
					break;
				}

				if (isCommon)
				{
					return true;
				}
			}

			return false;
		}

		private static Type GetContextType(QuorumAction action)
		{
			if (action is not ScriptableQuorumActionReference reference)
			{
				return GetContextArgument(action.GetType(), typeof(QuorumAction<,>));
			}

			return reference.Action != null
				? GetContextArgument(reference.Action.GetType(), typeof(ScriptableQuorumAction<,>))
				: null;
		}

		private static Type GetContextArgument(Type type, Type definition)
		{
			for (Type current = type; current != null; current = current.BaseType)
			{
				if (current.IsGenericType && current.GetGenericTypeDefinition() == definition)
				{
					return current.GetGenericArguments()[1];
				}
			}

			return null;
		}

		private static Consideration GetConsideration(object scoreSource)
		{
			FieldInfo field = SerializableFieldCache.GetField(scoreSource.GetType(), ConsiderationFieldName);
			return field?.GetValue(scoreSource) as Consideration;
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
			if (type == typeof(QuorumAction) || type == typeof(ScriptableQuorumAction))
			{
				return true;
			}

			if (!type.IsGenericType)
			{
				return false;
			}

			Type definition = type.GetGenericTypeDefinition();
			return definition == typeof(QuorumAction<,>) || definition == typeof(ScriptableQuorumAction<,>);
		}

		private static string GetActionName(QuorumAction action) =>
			TypeRegistry.GetDisplayName(action.GetType());

		private static string GetScoreSourceName(object scoreSource) =>
			scoreSource is ScriptableQuorumAction asset
				? asset.name
				: TypeRegistry.GetDisplayName(scoreSource.GetType());
	}
}
