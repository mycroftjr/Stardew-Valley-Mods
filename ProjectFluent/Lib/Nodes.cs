using System;
using System.Collections.Generic;
using System.Linq;

namespace Shockah.ProjectFluent.Lib
{
	internal interface IHasSpan
	{
		BaseNode.Span Span { get; set; }
	}

	internal static class HasSpanExtensions
	{
		public static void AddSpan(this IHasSpan self, int start, int end)
		{
			self.Span = new(start, end);
		}
	}

	internal abstract class BaseNode
	{
		internal abstract class SyntaxNode: BaseNode
		{
			internal sealed class Resource: SyntaxNode, IEquatable<Resource>
			{
				public readonly IReadOnlyList<TopLevel> Body;

				public Resource(IReadOnlyList<TopLevel> body)
				{
					this.Body = body;
				}

				public bool Equals(Resource other)
				{
					return Body.SequenceEqual(other.Body);
				}

				public override bool Equals(object obj)
				{
					return Equals(obj as Resource);
				}

				public override int GetHashCode()
				{
					return Body.GetHashCode();
				}
			}

			internal sealed class Attribute: SyntaxNode, IEquatable<Attribute>
			{
				public readonly Identifier ID;
				public readonly TopLevel.Pattern Value;

				public Attribute(Identifier id, TopLevel.Pattern value)
				{
					this.ID = id;
					this.Value = value;
				}

				public bool Equals(Attribute other)
				{
					return ID.Equals(other.ID) && Value.Equals(other.Value);
				}

				public override bool Equals(object obj)
				{
					return Equals(obj as Attribute);
				}

				public override int GetHashCode()
				{
					return ID.GetHashCode() * 17 ^ Value.GetHashCode();
				}
			}

			internal sealed class Variant: SyntaxNode, IEquatable<Variant>
			{
				public readonly TopLevel.IVariantKey Key;
				public readonly TopLevel.Pattern Value;
				public readonly bool Default;

				public Variant(TopLevel.IVariantKey key, TopLevel.Pattern value, bool @default)
				{
					this.Key = key;
					this.Value = value;
					this.Default = @default;
				}

				public bool Equals(Variant other)
				{
					return Key.Equals(other.Key) && Value.Equals(other.Value) && Default == other.Default;
				}

				public override bool Equals(object obj)
				{
					return Equals(obj as Variant);
				}

				public override int GetHashCode()
				{
					return Key.GetHashCode() * 17 ^ Value.GetHashCode() * 17 ^ Default.GetHashCode();
				}
			}

			internal sealed class NamedArgument: SyntaxNode, TopLevel.ICallArgument, IEquatable<NamedArgument>
			{
				public readonly Identifier Name;
				public readonly TopLevel.Expression.Literal Value;

				public NamedArgument(Identifier name, TopLevel.Expression.Literal value)
				{
					this.Name = name;
					this.Value = value;
				}

				public bool Equals(NamedArgument other)
				{
					return Name.Equals(other.Name) && Value.Equals(other.Value);
				}

				public override bool Equals(object obj)
				{
					return Equals(obj as NamedArgument);
				}

				public override int GetHashCode()
				{
					return Name.GetHashCode() * 17 ^ Value.GetHashCode();
				}
			}

			internal sealed class Identifier: SyntaxNode, TopLevel.IVariantKey, IEquatable<Identifier>
			{
				public readonly string Name;

				public Identifier(string name)
				{
					this.Name = name;
				}

				public bool Equals(Identifier other)
				{
					return Name == other.Name;
				}

				public override bool Equals(object obj)
				{
					return Equals(obj as Identifier);
				}

				public override int GetHashCode()
				{
					return Name.GetHashCode();
				}
			}

			internal abstract class PatternElement: SyntaxNode
			{
				internal sealed class TextElement: PatternElement, IHasSpan, IEquatable<TextElement>
				{
					public readonly string Value;
					Span IHasSpan.Span { get; set; }

					public TextElement(string value)
					{
						this.Value = value;
					}

					public bool Equals(TextElement other)
					{
						return Value == other.Value && ((IHasSpan)this).Span == ((IHasSpan)other).Span;
					}

					public override bool Equals(object obj)
					{
						return Equals(obj as TextElement);
					}

					public override int GetHashCode()
					{
						return Value.GetHashCode() * 17 ^ ((IHasSpan)this).Span.GetHashCode();
					}
				}

				internal sealed class Placeable: PatternElement, TopLevel.IInsidePlaceable, IEquatable<Placeable>
				{
					public readonly TopLevel.IInsidePlaceable Expression;

					public Placeable(TopLevel.IInsidePlaceable expression)
					{
						this.Expression = expression;
					}

					public bool Equals(Placeable other)
					{
						return Expression == other.Expression;
					}

					public override bool Equals(object obj)
					{
						return Equals(obj as Placeable);
					}

					public override int GetHashCode()
					{
						return Expression.GetHashCode();
					}
				}

				internal sealed class Indent: PatternElement, IHasSpan, IEquatable<Indent>
				{
					public readonly string Value;
					Span IHasSpan.Span { get; set; }

					public Indent(String value)
					{
						Value = value;
					}

					public Indent(String value, int start, int end)
					{
						Value = value;
						((IHasSpan)this).AddSpan(start, end);
					}

					public bool Equals(Indent other)
					{
						return Value == other.Value && ((IHasSpan)this).Span == ((IHasSpan)other).Span;
					}

					public override bool Equals(object obj)
					{
						return Equals(obj as Indent);
					}

					public override int GetHashCode()
					{
						return Value.GetHashCode() * 17 ^ ((IHasSpan)this).Span.GetHashCode();
					}
				}
			}

			internal sealed class CallArguments: SyntaxNode, IEquatable<CallArguments>
			{
				public readonly IReadOnlyList<TopLevel.Expression> Positional;
				public readonly IReadOnlyList<NamedArgument> Named;

				public CallArguments(IReadOnlyList<TopLevel.Expression> positional, IReadOnlyList<NamedArgument> named)
				{
					this.Positional = positional;
					this.Named = named;
				}

				public bool Equals(CallArguments other)
				{
					return Positional.SequenceEqual(other.Positional) && Named.SequenceEqual(other.Named);
				}

				public override bool Equals(object obj)
				{
					return Equals(obj as CallArguments);
				}

				public override int GetHashCode()
				{
					return Positional.GetHashCode() * 17 ^ Named.GetHashCode();
				}
			}

			internal sealed class Annotation: SyntaxNode, IHasSpan, IEquatable<Annotation>
			{
				public readonly string Code;
				public readonly string Message;
				public readonly IReadOnlyList<object> Arguments;
				Span IHasSpan.Span { get; set; }

				public Annotation(string code, string message, IReadOnlyList<object> arguments)
				{
					this.Code = code;
					this.Message = message;
					this.Arguments = arguments;
				}

				public bool Equals(Annotation other)
				{
					return Code == other.Code && Message == other.Message && Arguments.SequenceEqual(other.Arguments) && ((IHasSpan)this).Span == ((IHasSpan)other).Span;
				}

				public override bool Equals(object obj)
				{
					return Equals(obj as Annotation);
				}

				public override int GetHashCode()
				{
					return Code.GetHashCode() * 17 ^ Message.GetHashCode() * 17 ^ Arguments.GetHashCode() * 17 ^ ((IHasSpan)this).Span.GetHashCode();
				}
			}

			internal abstract class TopLevel: SyntaxNode
			{
				internal abstract class Entry: TopLevel
				{
					internal sealed class Message: Entry, IEquatable<Message>
					{
						public readonly Identifier ID;
						public readonly Pattern Value;
						public readonly IReadOnlyList<Attribute> Attributes;
						public readonly BaseComment.Comment Comment;

						public Message(Identifier id, Pattern value, IReadOnlyList<Attribute> attributes, BaseComment.Comment comment = null)
						{
							this.ID = id;
							this.Value = value;
							this.Attributes = attributes;
							this.Comment = comment;
						}

						public bool Equals(Message other)
						{
							return ID.Equals(other.ID) && Value.Equals(other.Value) && Attributes.SequenceEqual(other.Attributes) && Comment.Equals(other.Comment);
						}

						public override bool Equals(object obj)
						{
							return Equals(obj as Message);
						}

						public override int GetHashCode()
						{
							return ID.GetHashCode() * 17 ^ Value.GetHashCode() * 17 ^ Attributes.GetHashCode() * 17 ^ Comment.GetHashCode();
						}
					}

					internal sealed class Term: Entry, IEquatable<Term>
					{
						public readonly Identifier ID;
						public readonly Pattern Value;
						public readonly IReadOnlyList<Attribute> Attributes;
						public readonly BaseComment.Comment Comment;

						public Term(Identifier id, Pattern value, IReadOnlyList<Attribute> attributes, BaseComment.Comment comment = null)
						{
							this.ID = id;
							this.Value = value;
							this.Attributes = attributes;
							this.Comment = comment;
						}

						public bool Equals(Term other)
						{
							return ID.Equals(other.ID) && Value.Equals(other.Value) && Attributes.SequenceEqual(other.Attributes) && Comment.Equals(other.Comment);
						}

						public override bool Equals(object obj)
						{
							return Equals(obj as Term);
						}

						public override int GetHashCode()
						{
							return ID.GetHashCode() * 17 ^ Value.GetHashCode() * 17 ^ Attributes.GetHashCode() * 17 ^ Comment.GetHashCode();
						}
					}

					internal abstract class BaseComment: Entry
					{
						public readonly string Content;

						public BaseComment(string content)
						{
							this.Content = content;
						}

						internal sealed class Comment: BaseComment, IEquatable<Comment>
						{
							public Comment(string content) : base(content)
							{
							}

							public bool Equals(Comment other)
							{
								return Content == other.Content;
							}

							public override bool Equals(object obj)
							{
								return Equals(obj as Comment);
							}

							public override int GetHashCode()
							{
								return Content.GetHashCode();
							}
						}

						internal sealed class GroupComment: BaseComment, IEquatable<GroupComment>
						{
							public GroupComment(string content) : base(content)
							{
							}

							public bool Equals(GroupComment other)
							{
								return Content == other.Content;
							}

							public override bool Equals(object obj)
							{
								return Equals(obj as GroupComment);
							}

							public override int GetHashCode()
							{
								return Content.GetHashCode();
							}
						}

						internal sealed class ResourceComment: BaseComment, IEquatable<ResourceComment>
						{
							public ResourceComment(string content) : base(content)
							{
							}

							public bool Equals(ResourceComment other)
							{
								return Content == other.Content;
							}

							public override bool Equals(object obj)
							{
								return Equals(obj as ResourceComment);
							}

							public override int GetHashCode()
							{
								return Content.GetHashCode();
							}
						}
					}
				}

				internal sealed class Pattern: SyntaxNode, IEquatable<Pattern>
				{
					public readonly IReadOnlyList<PatternElement> Elements;

					public Pattern() : this(new List<PatternElement>().AsReadOnly())
					{
					}

					public Pattern(IReadOnlyList<PatternElement> elements)
					{
						this.Elements = elements;
					}

					public bool Equals(Pattern other)
					{
						return Elements.SequenceEqual(other.Elements);
					}

					public override bool Equals(object obj)
					{
						return Equals(obj as Pattern);
					}

					public override int GetHashCode()
					{
						return Elements.GetHashCode();
					}
				}

				internal interface IInsidePlaceable
				{
				}

				internal abstract class Expression: SyntaxNode, ICallArgument, IInsidePlaceable
				{
					internal abstract class Literal: Expression
					{
						public abstract string Value { get; }

						internal sealed class StringLiteral: Literal, IEquatable<StringLiteral>
						{
							private readonly string _value;
							public override string Value => _value;

							public StringLiteral(string value)
							{
								_value = value;
							}

							public bool Equals(StringLiteral other)
							{
								return Value == other.Value;
							}

							public override bool Equals(object obj)
							{
								return Equals(obj as StringLiteral);
							}

							public override int GetHashCode()
							{
								return Value.GetHashCode();
							}
						}

						internal sealed class NumberLiteral: Literal, IVariantKey, IEquatable<NumberLiteral>
						{
							private readonly string _value;
							public override string Value => _value;

							public NumberLiteral(string value)
							{
								_value = value;
							}

							public bool Equals(NumberLiteral other)
							{
								return Value == other.Value;
							}

							public override bool Equals(object obj)
							{
								return Equals(obj as NumberLiteral);
							}

							public override int GetHashCode()
							{
								return Value.GetHashCode();
							}
						}
					}

					internal sealed class MessageReference: Expression, IEquatable<MessageReference>
					{
						public readonly Identifier ID;
						public new readonly Identifier Attribute;

						public MessageReference(Identifier id, Identifier attribute = null)
						{
							this.ID = id;
							this.Attribute = attribute;
						}

						public bool Equals(MessageReference other)
						{
							return ID.Equals(other.ID) && Attribute.Equals(other.Attribute);
						}

						public override bool Equals(object obj)
						{
							return Equals(obj as MessageReference);
						}

						public override int GetHashCode()
						{
							return ID.GetHashCode() * 17 ^ Attribute.GetHashCode();
						}
					}

					internal sealed class TermReference: Expression, IEquatable<TermReference>
					{
						public readonly Identifier ID;
						public new readonly Identifier Attribute;
						public readonly CallArguments Arguments;

						public TermReference(Identifier id, Identifier attribute = null, CallArguments arguments = null)
						{
							this.ID = id;
							this.Attribute = attribute;
							this.Arguments = arguments;
						}

						public bool Equals(TermReference other)
						{
							return ID.Equals(other.ID) && Attribute.Equals(other.Attribute) && Arguments.Equals(other.Arguments);
						}

						public override bool Equals(object obj)
						{
							return Equals(obj as TermReference);
						}

						public override int GetHashCode()
						{
							return ID.GetHashCode() * 17 ^ Attribute.GetHashCode() * 17 ^ Arguments.GetHashCode();
						}
					}

					internal sealed class VariableReference: Expression, IEquatable<VariableReference>
					{
						public readonly Identifier ID;

						public VariableReference(Identifier id)
						{
							this.ID = id;
						}

						public bool Equals(VariableReference other)
						{
							return ID.Equals(other.ID);
						}

						public override bool Equals(object obj)
						{
							return Equals(obj as VariableReference);
						}

						public override int GetHashCode()
						{
							return ID.GetHashCode();
						}
					}

					internal sealed class FunctionReferrence: Expression, IEquatable<FunctionReferrence>
					{
						public readonly Identifier ID;
						public readonly CallArguments Arguments;

						public FunctionReferrence(Identifier id, CallArguments arguments)
						{
							this.ID = id;
							this.Arguments = arguments;
						}

						public bool Equals(FunctionReferrence other)
						{
							return ID.Equals(other.ID) && Arguments.Equals(other.Arguments);
						}

						public override bool Equals(object obj)
						{
							return Equals(obj as FunctionReferrence);
						}

						public override int GetHashCode()
						{
							return ID.GetHashCode() * 17 ^ Arguments.GetHashCode();
						}
					}

					internal sealed class SelectExpression: Expression, IEquatable<SelectExpression>
					{
						public readonly Expression Selector;
						public readonly IReadOnlyList<Variant> Variants;

						public SelectExpression(Expression selector, IReadOnlyList<Variant> variants)
						{
							this.Selector = selector;
							this.Variants = variants;
						}

						public bool Equals(SelectExpression other)
						{
							return Selector.Equals(other.Selector) && Variants.SequenceEqual(other.Variants);
						}

						public override bool Equals(object obj)
						{
							return Equals(obj as SelectExpression);
						}

						public override int GetHashCode()
						{
							return Selector.GetHashCode() * 17 ^ Variants.GetHashCode();
						}
					}
				}

				internal interface ICallArgument
				{
				}

				internal interface IVariantKey
				{
				}

				internal sealed class Junk: TopLevel, IEquatable<Junk>
				{
					public readonly string Content;
					public readonly IReadOnlyList<Annotation> Annotations;

					public Junk(string content, IReadOnlyList<Annotation> annotations)
					{
						this.Content = content;
						this.Annotations = annotations;
					}

					public bool Equals(Junk other)
					{
						return Content == other.Content && Annotations.SequenceEqual(other.Annotations);
					}

					public override bool Equals(object obj)
					{
						return Equals(obj as Junk);
					}

					public override int GetHashCode()
					{
						return Content.GetHashCode() * 17 ^ Annotations.GetHashCode();
					}
				}

				internal sealed class Whitespace: TopLevel, IEquatable<Whitespace>
				{
					public readonly string Content;

					public Whitespace(string content)
					{
						this.Content = content;
					}

					public bool Equals(Whitespace other)
					{
						return Content == other.Content;
					}

					public override bool Equals(object obj)
					{
						return Equals(obj as Whitespace);
					}

					public override int GetHashCode()
					{
						return Content.GetHashCode();
					}
				}
			}
		}

		internal sealed class Span: BaseNode, IEquatable<Span>
		{
			public readonly int Start;
			public readonly int End;

			public Span(int start, int end)
			{
				this.Start = start;
				this.End = end;
			}

			public bool Equals(Span other)
			{
				return Start == other.Start && End == other.End;
			}

			public override bool Equals(object obj)
			{
				return Equals(obj as Span);
			}

			public override int GetHashCode()
			{
				return Start * 17 ^ End;
			}
		}
	}
}