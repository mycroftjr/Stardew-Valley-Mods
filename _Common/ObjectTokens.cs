﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Shockah.CommonModCode
{
	public static class ObjectTokens
	{
		public static IDictionary<string, string> Extract(object tokens)
		{
			// source: https://github.com/Pathoschild/SMAPI/blob/develop/src/SMAPI/Translation.cs

			var results = new Dictionary<string, string>();
			if (tokens is null)
				return results;

			if (tokens is IDictionary dictionary)
			{
				foreach (DictionaryEntry entry in dictionary)
				{
					string? key = entry.Key?.ToString()?.Trim();
					if (key is not null)
						results[key] = $"{entry.Value}";
				}
			}
			else
			{
				Type type = tokens.GetType();
				foreach (FieldInfo field in type.GetFields())
					results[field.Name] = $"{field.GetValue(tokens)}";
				foreach (PropertyInfo prop in type.GetProperties())
					results[prop.Name] = $"{prop.GetValue(tokens)}";
			}

			return results;
		}
	}
}