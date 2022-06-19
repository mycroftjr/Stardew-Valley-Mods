﻿using Linguini.Bundle;
using Linguini.Bundle.Builder;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Shockah.ProjectFluent
{
	internal class FluentImpl: IFluent<string>
	{
		private IFluent<string> Fallback { get; set; }
		private FluentBundle Bundle { get; set; }

		public FluentImpl(IEnumerable<(string name, ContextfulFluentFunction function)> functions, IGameLocale locale, string content, IFluent<string>? fallback = null)
		{
			this.Fallback = fallback ?? new NoOpFluent();

			try
			{
				Bundle = LinguiniBuilder.Builder()
					.CultureInfo(new CultureInfo(locale.LanguageCode))
					.AddResources(content)
					.SetUseIsolating(false)
					.UncheckedBuild();

				foreach (var (functionName, function) in functions)
				{
					var identifier = new Identifier(new ReadOnlyMemory<char>(functionName.ToCharArray()));

					if (Bundle.TryGetFunction(identifier, out _))
						continue;
					Bundle.AddFunction(functionName, (fluentPositionalArguments, fluentNamedArguments) =>
					{
						var positionalArguments = fluentPositionalArguments.Select(a => new FluentFunctionValue(a)).ToList();
						var namedArguments = new Dictionary<string, IFluentApi.IFluentFunctionValue>();
						foreach (var (key, a) in fluentNamedArguments)
							namedArguments[key] = new FluentFunctionValue(a);

						var result = function(locale, positionalArguments, namedArguments);
						var fluentResult = result.AsFluentValue();

						if (fluentResult is IFluentType fluentResultValue)
							return fluentResultValue;
						else
							throw new ArgumentException($"Function `{functionName}` returned a value that is not a `{nameof(IFluentType)}`.");
					}, out _);
				}
			}
			catch (Exception ex)
			{
				throw new ArgumentException($"Errors parsing Fluent:\n{ex}");
			}
		}

		private static IDictionary<string, IFluentType> ExtractTokens(object? tokens)
		{
			// source: https://github.com/Pathoschild/SMAPI/blob/develop/src/SMAPI/Translation.cs

			Dictionary<string, IFluentType> results = new();
			if (tokens == null)
				return results;

			void AddResult(string key, object? value)
			{
				if (value is double or float or int or long)
					results[key] = FluentNumber.FromString($"{value}");
				else if (value is not null)
					results[key] = new FluentString($"{value}");
			}

			if (tokens is IDictionary dictionary)
			{
				foreach (DictionaryEntry entry in dictionary)
				{
					string? key = entry.Key?.ToString()?.Trim();
					if (key is not null)
						AddResult(key, entry.Value);
				}
			}
			else
			{
				Type type = tokens.GetType();
				foreach (FieldInfo field in type.GetFields())
					AddResult(field.Name, field.GetValue(tokens));
				foreach (PropertyInfo prop in type.GetProperties())
					AddResult(prop.Name, prop.GetValue(tokens));
			}

			return results;
		}

		public bool ContainsKey(string key)
		{
			return Bundle.TryGetAttrMsg(key, null, out _, out _);
		}

		public string Get(string key, object? tokens)
		{
			var extractedTokens = ExtractTokens(tokens);
			return Bundle.GetAttrMessage(key, extractedTokens) ?? Fallback.Get(key, tokens);
		}
	}

	internal record FluentFunctionValue: IFluentApi.IFluentFunctionValue
	{
		internal IFluentType Value { get; set; }

		public FluentFunctionValue(IFluentType value)
		{
			this.Value = value;
		}

		public object AsFluentValue()
			=> Value;

		public string AsString()
			=> Value.AsString();

		public int? AsIntOrNull()
			=> int.TryParse(AsString(), out var @int) ? @int : null;

		public long? AsLongOrNull()
			=> int.TryParse(AsString(), out var @int) ? @int : null;

		public float? AsFloatOrNull()
			=> float.TryParse(AsString(), out var @int) ? @int : null;

		public double? AsDoubleOrNull()
			=> double.TryParse(AsString(), out var @int) ? @int : null;
	}

	internal class FluentValueFactory: IFluentValueFactory
	{
		public IFluentApi.IFluentFunctionValue CreateStringValue(string value)
			=> new FluentFunctionValue(new FluentString(value));

		public IFluentApi.IFluentFunctionValue CreateIntValue(int value)
			=> new FluentFunctionValue(FluentNumber.FromString($"{value}"));

		public IFluentApi.IFluentFunctionValue CreateLongValue(long value)
			=> new FluentFunctionValue(FluentNumber.FromString($"{value}"));

		public IFluentApi.IFluentFunctionValue CreateFloatValue(float value)
			=> new FluentFunctionValue(FluentNumber.FromString($"{value}"));

		public IFluentApi.IFluentFunctionValue CreateDoubleValue(double value)
			=> new FluentFunctionValue(FluentNumber.FromString($"{value}"));
	}
}