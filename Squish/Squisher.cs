using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squish
{
    public class Squisher
    {
        public static string Emit(string sourceFrom, string sourceTo)
        {
            var sourceMappables = ToMappables(sourceFrom);
           
            var targetMappables = ToMappables(sourceTo);
            var targetMappableByName = targetMappables.ToDictionary(
                                            (m) => { return m.Name.Last(); },
                                            (m) => { return m; });

            var namespaceName = string.Join(@".", sourceMappables[0].Name.Take(sourceMappables[0].Name.Count - 1));
            var sourceTypeName = string.Join(@".", sourceMappables[0].Name.Take(sourceMappables[0].Name.Count - 1));
            var targetTypeName = string.Join(@".", targetMappables[0].Name.Take(targetMappables[0].Name.Count - 1));

            var output = new StringBuilder();

            AppendNamespaceStart(namespaceName, output);
            AppendClassStart(output);
            AppendMethodStart(sourceTypeName, targetTypeName, output);

            foreach (var sourceMappable in sourceMappables)
            {
                var name = sourceMappable.Name.Last();
                Mappable targetMappable;
                if (!targetMappableByName.TryGetValue(name, out targetMappable))
                {
                    continue;
                }

                AppendAssignment(name, output);
            }

            AppendMethodEnd(output);
            AppendClassEnd(output);
            AppendNamespaceEnd(output);

            return output.ToString();
        }

        private static void AppendAssignment(string name, StringBuilder output)
        {
            output.Append(@"target.");
            output.Append(name);
            output.Append(@" = source.");
            output.Append(name);
            output.AppendLine(@";");
        }

        private static void AppendMethodStart(string sourceTypeName, string targetTypeName, StringBuilder output)
        {
            output.Append(@"public static void Populate(this ");
            output.Append(sourceTypeName);
            output.Append(@" source, ");
            output.Append(targetTypeName);
            output.AppendLine(@" target)");
            output.AppendLine("{");
        }

        private static void AppendMethodEnd(StringBuilder output)
        {
            output.AppendLine(@"}");
        }

        private static void AppendClassStart(StringBuilder output)
        {
            output.AppendLine(@"public static class Extensions");
            output.AppendLine(@"{");
        }

        private static void AppendClassEnd(StringBuilder output)
        {
            output.AppendLine(@"}");
        }

        private static void AppendNamespaceStart(string namespaceName, StringBuilder output)
        {
            output.Append(@"namespace ");
            output.Append(namespaceName);
            output.AppendLine(@".Generated");
            output.AppendLine(@"{");
        }

        private static void AppendNamespaceEnd(StringBuilder output)
        {
            output.AppendLine(@"}");
        }

        private static List<Mappable> ToMappables(string source)
        {
            var words = ToWords(source);
            var node = ToNestedNodes(words);
            var mappables = ToMappables(node);
            return mappables;
        }

        private static List<Mappable> ToMappables(Node node)
        {
            var nameStack = new Stack<string>();
            var mappables = new List<Mappable>();
            foreach (var child in node.Children)
            {
                FindMappables(child, nameStack, mappables);
            }
            return mappables;
        }

        private static void FindMappables(Node node, Stack<string> nameStack, List<Mappable> mappables)
        {
            if (node.Words.Count == 0 &&
                node.Children.Count == 0)
            {
                return;
            }

            if (TryParseUsing(node))
            {
                return;
            }

            if (TryParseNamespace(node, nameStack, mappables))
            {
                return;
            }

            if (TryParseClass(node, nameStack, mappables))
            {
                return;
            }

            if (TryParseMethod(node))
            {
                return;
            }

            if (TryParseField(node, nameStack, mappables))
            {
                return;
            }

            if (TryParseProperty(node, nameStack, mappables))
            {
                return;
            }
        }

        private static bool TryParseProperty(Node node, Stack<string> nameStack, List<Mappable> mappables)
        {
            if (node.Children.Count == 0)
            {
                return false;
            }

            if (node.Words.Contains("("))
            {
                return false;
            }

            if (node.Children[0].Words.Count == 0 ||
                (node.Children[0].Words[0] != "get" && node.Children[0].Words[0] != "set"))
            {
                return false;
            }

            if (node.Words.Contains("private"))
            {
                return true;
            }

            var typeName = node.Words[node.Words.Count - 2];
            var propertyName = node.Words[node.Words.Count - 1];
            var mappable = new Mappable();
            mappables.Add(mappable);
            mappable.Name.AddRange(nameStack.Reverse());
            mappable.Name.Add(propertyName);
            mappable.TypeName = typeName;
            return true;
        }

        private static bool TryParseField(Node node, Stack<string> nameStack, List<Mappable> mappables)
        {
            if (node.Children.Count != 0)
            {
                return false;
            }

            if (node.Words.Contains("private"))
            {
                return true;
            }
            
            var lastWordIndex = node.Words.Count;
            var equalsIndex = node.Words.IndexOf("=");
            if (equalsIndex >= 0)
            {
                lastWordIndex = equalsIndex;
            }
            var typeName = node.Words[lastWordIndex - 2];
            var fieldName = node.Words[lastWordIndex - 1];

            var mappable = new Mappable();
            mappables.Add(mappable);
            mappable.Name.AddRange(nameStack.Reverse());
            mappable.Name.Add(fieldName);
            mappable.TypeName = typeName;
            return true;
        }

        private static bool TryParseMethod(Node node)
        {
            if (node.Words.Contains("="))
            {
                return false;
            }

            if (!node.Words.Contains("("))
            {
                return false;
            }

            return true;
        }

        private static bool TryParseClass(Node node, Stack<string> nameStack, List<Mappable> mappables)
        {
            if (!node.Words.Contains("class"))
            {
                return false;
            }
            var earlyExit = false;
            string returnTypeName = null;
            string className = null;
            foreach (var word in node.Words)
            {
                switch (word)
                {
                    case ":":
                        earlyExit = true;
                        break;
                    case "private":
                    case "protected":
                    case "public":
                    case "internal":
                    case "static":
                    case "readonly":
                    case "abstract":
                    case "virtual":
                        break;
                    default:
                        if (returnTypeName == null)
                        {
                            returnTypeName = word;
                        }
                        else
                        {
                            className = word;
                            earlyExit = true;
                        }
                        break;
                }

                if (earlyExit)
                {
                    break;
                }
            }

            nameStack.Push(className);
            foreach (var child in node.Children)
            {
                FindMappables(child, nameStack, mappables);
            }
            nameStack.Pop();
            return true;
        }

        private static bool TryParseNamespace(Node node, Stack<string> nameStack, List<Mappable> mappables)
        {
            if (node.Words[0] != "namespace")
            {
                return false;
            }
            var nameDepth = 0;
            foreach (var name in node.Words.Skip(1))
            {
                if (name == ".")
                {
                    continue;
                }
                nameStack.Push(name);
                nameDepth++;
            }
            foreach (var child in node.Children)
            {
                FindMappables(child, nameStack, mappables);
            }
            for (var popCount = 0; popCount < nameDepth; popCount++)
            {
                nameStack.Pop();
            }
            return true;
        }

        private static bool TryParseUsing(Node node)
        {
            return node.Words[0] == "using";
        }

        private static Node ToNestedNodes(List<string> words)
        {
            var root = new Node();
            var node = root;
            node = node.CreateChild();
            var nameStack = new Stack<string>();
            foreach (var word in words)
            {
                switch (word)
                {
                    case "{":
                        node = node.CreateChild();
                        break;
                    case "}":
                        node = node.Parent;
                        node = node.Parent.CreateChild();
                        break;
                    case ";":
                        node = node.Parent.CreateChild();
                        break;
                    default:
                        if (word.StartsWith("//") || word.StartsWith("/*"))
                        {
                            continue;
                        }

                        node.Words.Add(word);
                        break;
                }
            }
            return root;
        }

        private static List<string> ToWords(string source)
        {
            var words = new List<string>();
            var word = new StringBuilder();

            var mode = TextParseMode.Text;

            foreach (var c in source)
            {
                switch (mode)
                {
                    case TextParseMode.Text:
                        switch (c)
                        {
                            case ' ':
                            case '\t':
                            case '\r':
                            case '\n':
                                Add(word, words);
                                break;
                            case '{':
                            case '}':
                            case ':':
                            case ';':
                            case ',':
                            case '(':
                            case ')':
                            case '.':
                            case '=':
                                Add(word, words);
                                Add(c, word);
                                Add(word, words);
                                break;
                            case '"':
                                Add(word, words);
                                Add(c, word);
                                mode = TextParseMode.DoubleQuoteText;
                                break;
                            case '\'':
                                Add(word, words);
                                Add(c, word);
                                mode = TextParseMode.SingleQuoteText;
                                break;
                            case '/':
                                if (word.Length > 0 &&
                                    word[word.Length - 1] == '/')
                                {
                                    Add(c, word);
                                    mode = TextParseMode.SingleLineComment;
                                }
                                else
                                {
                                    goto default;
                                }
                                break;
                            case '*':
                                if (word.Length > 0 &&
                                    word[word.Length - 1] == '/')
                                {
                                    Add(c, word);
                                    mode = TextParseMode.MultiLineComment;
                                }
                                else
                                {
                                    goto default;
                                }
                                break;
                            default:
                                Add(c, word);
                                break;
                        }
                        break;
                    case TextParseMode.SingleQuoteText:
                        switch (c)
                        {
                            case '\'':
                                if (word.Length < 0 ||
                                    word[word.Length - 1] != '\\')
                                {
                                    Add(c, word);
                                    Add(word, words);
                                    mode = TextParseMode.Text;
                                }
                                else
                                {
                                    goto default;
                                }
                                break;
                            default:
                                Add(c, word);
                                break;
                        }
                        break;
                    case TextParseMode.DoubleQuoteText:
                        switch (c)
                        {
                            case '"':
                                if (word.Length < 0 ||
                                    word[word.Length - 1] != '\\')
                                {
                                    Add(c, word);
                                    Add(word, words);
                                    mode = TextParseMode.Text;

                                }
                                else
                                {
                                    goto default;
                                }
                                break;
                            default:
                                Add(c, word);
                                break;
                        }
                        break;
                    case TextParseMode.SingleLineComment:
                        switch (c)
                        {
                            case '\r':
                            case '\n':
                                mode = TextParseMode.Text;
                                break;
                            default:
                                Add(c, word);
                                break;
                        }
                        break;
                    case TextParseMode.MultiLineComment:
                        switch (c)
                        {
                            case '/':
                                if (word.Length > 0 &&
                                    word[word.Length - 1] == '*')
                                {
                                    Add(c, word);
                                    Add(word, words);
                                    mode = TextParseMode.Text;
                                }
                                else
                                {
                                    goto default;
                                }
                                break;
                            default:
                                Add(c, word);
                                break;
                        }
                        break;
                }
            }
            Add(word, words);
            return words;
        }

        private static void Add(char c, StringBuilder word)
        {
            word.Append(c);
        }

        private static void Add(StringBuilder word, List<string> words)
        {
            if (word.Length == 0)
            {
                return;
            }
            words.Add(word.ToString());
            word.Clear();
        }
    }
}
