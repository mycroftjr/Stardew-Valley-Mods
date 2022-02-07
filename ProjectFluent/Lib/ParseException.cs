using System;

namespace Shockah.ProjectFluent.Lib
{
	internal class ParseException: Exception
	{
		internal class Generic: ParseException { }

		internal class ExpectedEntryStart: ParseException { }

		internal class ExpectedToken: ParseException
		{
			public readonly string Token;

			public ExpectedToken(string token)
			{
				this.Token = token;
			}
		}

		internal class ExpectedCharacterFromRange: ParseException
		{
			public readonly string Range;

			public ExpectedCharacterFromRange(string range)
			{
				this.Range = range;
			}
		}

		internal class ExpectedMessageToHaveValueOrAttributes: ParseException
		{
			public new readonly string Message;

			public ExpectedMessageToHaveValueOrAttributes(string message)
			{
				this.Message = message;
			}
		}

		internal class ExpectedTermToHaveValue: ParseException
		{
			public readonly string Term;

			public ExpectedTermToHaveValue(string term)
			{
				this.Term = term;
			}
		}

		internal class KeywordCannotEndWithWhitespace: ParseException { }

		internal class CalleeHasToBeUppercaseIdentifierOrTerm: ParseException { }

		internal class ArgumentHasToBeSimpleIdentifier: ParseException { }

		internal class ExpectedMarkedDefaultVariant: ParseException { }

		internal class ExpectedAtLeastOneVariant: ParseException { }

		internal class ExpectedValue: ParseException { }

		internal class ExpectedVariantKey: ParseException { }

		internal class ExpectedLiteral: ParseException { }

		internal class OnlyOneVariantCanBeMarkedDefault: ParseException { }

		internal class MessageReferencesCannotBeSelectors: ParseException { }

		internal class TermsCannotBeSelectors: ParseException { }

		internal class MessageAttributesCannotBeSelectors: ParseException { }

		internal class TermAttributesCannotBeSelectors: ParseException { }

		internal class UndeterminedStringExpression: ParseException { }

		internal class PositionalArgumentsMustNotFollowNamed: ParseException { }

		internal class NamedArgumentsMustBeUnique: ParseException { }

		internal class CannotAccessMessageVariants: ParseException { }

		internal class UnknownEscapeSequence: ParseException
		{
			public readonly string Sequence;

			public UnknownEscapeSequence(string sequence)
			{
				this.Sequence = sequence;
			}
		}

		internal class InvalidUnicodeEscapeSequence: ParseException
		{
			public readonly string Sequence;

			public InvalidUnicodeEscapeSequence(string sequence)
			{
				this.Sequence = sequence;
			}
		}

		internal class UnbalancedClosingBraceInTextElement: ParseException { }

		internal class ExpectedInlineExpression: ParseException { }
	}
}