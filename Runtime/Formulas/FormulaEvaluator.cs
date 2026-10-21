using System;
using System.Text;
using DTech.Blackboard;

namespace DTech.Quorum
{
	/// <summary>
	/// Runs one formula against one blackboard. Compiling and resolving the variables happen in
	/// TrySetExpression and Bind; Evaluate then only reads cached variable handles and walks the
	/// compiled program, so it allocates nothing and is safe to call every frame.
	/// An evaluator holds per-agent data: give every agent its own.
	/// </summary>
	public sealed class FormulaEvaluator
	{
		private const string UnresolvedSeparator = ", ";

		private FormulaProgram _program;
		private FormulaError _error;
		private FormulaBinding[] _bindings;
		private float[] _values;
		private float[] _stack;
		private RuntimeBlackboard _blackboard;
		private string _expression;
		private string _unresolvedVariables;
		private bool _hasUnresolvedVariables;
		private bool _isCompiled;
		private bool _isBound;

		/// <summary>
		/// Gets whether the current expression compiled.
		/// </summary>
		public bool IsCompiled => _program != null;

		/// <summary>
		/// Gets why the current expression failed to compile, or the default value when it compiled.
		/// </summary>
		public FormulaError Error => _error;

		/// <summary>
		/// Gets the names the bound blackboard had no variable for, separated by commas, or null when
		/// every name resolved or no blackboard was bound.
		/// </summary>
		public string UnresolvedVariables => _blackboard != null ? _unresolvedVariables : null;

		/// <summary>
		/// Compiles an expression unless it is the one already compiled. Rebinding is required after
		/// the expression changed, because the variable slots are part of the compiled program.
		/// </summary>
		/// <param name="expression">Formula text.</param>
		/// <returns>TRUE when the expression compiled, FALSE otherwise.</returns>
		public bool TrySetExpression(string expression)
		{
			if (_isCompiled && string.Equals(_expression, expression, StringComparison.Ordinal))
			{
				return _program != null;
			}

			_expression = expression;
			_isCompiled = true;
			_isBound = false;
			_blackboard = null;
			_unresolvedVariables = null;
			_hasUnresolvedVariables = false;
			if (!FormulaCompiler.TryCompile(expression, out _program, out _error))
			{
				_bindings = null;
				_values = null;
				_stack = null;
				return false;
			}

			EnsureBuffers();
			return true;
		}

		/// <summary>
		/// Checks whether the evaluator already compiled this expression and resolved its variables
		/// against this blackboard, which is what tells a caller it can evaluate without rebinding.
		/// </summary>
		/// <param name="expression">Formula text to compare against.</param>
		/// <param name="blackboard">Blackboard to compare against, which may be null.</param>
		/// <returns>TRUE when the evaluator is current for both, FALSE otherwise.</returns>
		public bool IsCurrent(string expression, RuntimeBlackboard blackboard) =>
			_isCompiled &&
			_isBound &&
			ReferenceEquals(_blackboard, blackboard) &&
			string.Equals(_expression, expression, StringComparison.Ordinal);

		/// <summary>
		/// Resolves every variable of the compiled formula to a blackboard variable and caches the
		/// handles. Names the blackboard has no variable for, in any of the supported value types, are
		/// collected into UnresolvedVariables and make Evaluate return its fallback.
		/// </summary>
		/// <param name="blackboard">Blackboard the formula reads from, which may be null.</param>
		public void Bind(RuntimeBlackboard blackboard)
		{
			_blackboard = blackboard;
			_isBound = true;
			_unresolvedVariables = null;
			_hasUnresolvedVariables = false;
			if (_program == null)
			{
				return;
			}

			EnsureBuffers();
			StringBuilder unresolved = null;
			for (int i = 0; i < _bindings.Length; i++)
			{
				string name = _program.VariableNames[i];
				FormulaBinding binding = Resolve(blackboard, name);
				_bindings[i] = binding;
				if (binding.IsResolved)
				{
					continue;
				}

				_hasUnresolvedVariables = true;
				unresolved ??= new StringBuilder();
				if (unresolved.Length > 0)
				{
					unresolved.Append(UnresolvedSeparator);
				}

				unresolved.Append(name);
			}

			_unresolvedVariables = unresolved?.ToString();
		}

		/// <summary>
		/// Evaluates the formula against the bound blackboard.
		/// </summary>
		/// <param name="fallback">Value returned when the expression did not compile, a variable did not resolve, or the result is not finite.</param>
		/// <returns>Result of the formula.</returns>
		public float Evaluate(float fallback)
		{
			if (_program == null || _hasUnresolvedVariables)
			{
				return fallback;
			}

			for (int i = 0; i < _bindings.Length; i++)
			{
				_values[i] = _bindings[i].ReadValue();
			}

			float result = _program.Evaluate(_values, _stack);
			return float.IsNaN(result) || float.IsInfinity(result) ? fallback : result;
		}

		private static FormulaBinding Resolve(RuntimeBlackboard blackboard, string name)
		{
			if (blackboard == null || string.IsNullOrEmpty(name))
			{
				return default;
			}

			if (blackboard.TryGetVariable(name, out BlackboardVariable<float> floatVariable))
			{
				return FormulaBinding.Float(floatVariable);
			}

			if (blackboard.TryGetVariable(name, out BlackboardVariable<int> intVariable))
			{
				return FormulaBinding.Int(intVariable);
			}

			return blackboard.TryGetVariable(name, out BlackboardVariable<bool> boolVariable)
				? FormulaBinding.Bool(boolVariable)
				: default;
		}

		private void EnsureBuffers()
		{
			int variableCount = _program.VariableCount;
			if (_bindings == null || _bindings.Length != variableCount)
			{
				_bindings = new FormulaBinding[variableCount];
				_values = new float[variableCount];
			}

			int stackSize = _program.StackSize;
			if (_stack == null || _stack.Length < stackSize)
			{
				_stack = new float[stackSize];
			}
		}
	}
}
