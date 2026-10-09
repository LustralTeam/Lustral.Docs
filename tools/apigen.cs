// Generates the API reference (docs/api) from Lustral.Api. Public types are read by reflection, and their descriptions
// from the compiler-generated XML documentation. Run it from the docs repository after the API changes and commit the
// result:
//   dotnet run tools/apigen.cs -- <path to the Lustral repository>
// Lustral.Api is built first (Release, net10.0). The assembly is only inspected; none of its code runs.
#:property PublishAot=false
#:property Nullable=enable

using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: dotnet run tools/apigen.cs -- <path to the Lustral repository>");
    return 2;
}

var repository = Path.GetFullPath(args[0]);
var project = Path.Combine(repository, "src", "Lustral.Api", "Lustral.Api.csproj");
var docsRoot = AppContext.GetData("EntryPointFileDirectoryPath") is string scriptDirectory
    ? Path.GetFullPath(Path.Combine(scriptDirectory, "..", "docs"))
    : Path.GetFullPath("docs");
var output = Path.Combine(docsRoot, "api");

Console.WriteLine($"Building {project}");
var build = Process.Start(new ProcessStartInfo("dotnet", ["build", project, "-c", "Release", "-f", "net10.0", "-v", "q", "-nologo"]))!;
build.WaitForExit();
if (build.ExitCode != 0)
    return 1;
var assemblyPath = Path.Combine(repository, "src", "Lustral.Api", "bin", "Release", "net10.0", "Lustral.Api.dll");

// HarmonyX (the type of Mod.Harmony) is resolved from the NuGet cache. No other assemblies outside the framework are needed.
var nugetRoot = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    var folder = Path.Combine(nugetRoot, name.Name == "0Harmony" ? "harmonyx" : name.Name!.ToLowerInvariant());
    if (!Directory.Exists(folder))
        return null;
    var file = Directory.EnumerateFiles(folder, name.Name + ".dll", SearchOption.AllDirectories)
        .OrderByDescending(f => f.Contains("netstandard2.0")).ThenByDescending(f => f).FirstOrDefault();
    return file is null ? null : AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
};
var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
var docs = XDocument.Load(Path.ChangeExtension(assemblyPath, ".xml")).Descendants("member")
    .ToDictionary(m => (string)m.Attribute("name")!, m => m);

var types = assembly.GetExportedTypes().OrderBy(t => t.FullName, StringComparer.Ordinal).ToList();
var pages = new ApiPages(types, docs);

if (Directory.Exists(output))
    Directory.Delete(output, recursive: true);
Directory.CreateDirectory(output);
foreach (var type in types)
    File.WriteAllText(Path.Combine(output, ApiPages.Slug(type) + ".md"), pages.TypePage(type));
File.WriteAllText(Path.Combine(output, "index.md"), pages.IndexPage());
File.WriteAllText(Path.Combine(docsRoot, ".vitepress", "api-sidebar.json"), pages.Sidebar());
Console.WriteLine($"Wrote {types.Count} types to {output}");
return 0;

/// <summary>Generates VitePress Markdown: one page per type, an index page and the sidebar.</summary>
sealed class ApiPages(List<Type> types, Dictionary<string, XElement> docs)
{
    // Groups of types in the reference. Types not listed appear under "Other".
    private static readonly (string Group, string[] Types)[] Groups =
    [
        ("The mod", ["Mod", "ModInfo", "ILog"]),
        ("The game", ["GameInfo", "ScriptingBackend", "GameEvents", "SceneInfo"]),
        ("Coroutines", ["Coroutines", "CoroutineHandle", "Wait"]),
        ("Settings", ["ModConfig"]),
        ("Assets", ["ModAssets", "ModAssetBundle"]),
        ("Other mods", ["LoadedMods"]),
    ];

    private readonly HashSet<Type> _types = [.. types];
    private readonly NullabilityInfoContext _nullability = new();
    private Type? _current;

    public static string Slug(Type type) => Kebab(TypeName(type, withArguments: false));

    private static string Kebab(string name)
    {
        var text = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0 && (char.IsLower(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1]) && char.IsUpper(name[i - 1]))))
                text.Append('-');
            text.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
        }
        return text.ToString().Trim('-');
    }

    private IEnumerable<(string Group, List<Type> Types)> Grouped()
    {
        var listed = new HashSet<Type>();
        foreach (var (group, names) in Groups)
        {
            var members = names.Select(n => types.FirstOrDefault(t => t.Name == n)).OfType<Type>().ToList();
            listed.UnionWith(members);
            if (members.Count > 0)
                yield return (group, members);
        }
        var others = types.Where(t => !listed.Contains(t)).ToList();
        if (others.Count > 0)
            yield return ("Other", others);
    }

    public string Sidebar()
    {
        var sidebar = new List<object> { new { text = "Overview", link = "/api/" } };
        foreach (var (group, members) in Grouped())
            sidebar.Add(new { text = group, items = members.Select(t => new { text = TypeName(t, withArguments: false), link = "/api/" + Slug(t) }).ToList() });
        return JsonSerializer.Serialize(sidebar, new JsonSerializerOptions { WriteIndented = true }) + "\n";
    }

    public string IndexPage()
    {
        _current = null;
        var md = new StringBuilder();
        md.AppendLine("<!-- Generated by tools/apigen.cs from Lustral.Api. Do not edit; run `npm run api` to regenerate. -->");
        md.AppendLine();
        md.AppendLine("# API reference");
        md.AppendLine();
        md.AppendLine("Everything a mod uses from Lustral is in the `Lustral` namespace of **Lustral.Api**, which the SDK references for you. " +
                      "A mod reaches most of it through its [`Mod`](/api/mod) class: `Log`, `Harmony`, `Config`, `Events`, `Coroutines`, `Assets`, `Game` and `Mods`.");
        foreach (var (group, members) in Grouped())
        {
            md.AppendLine();
            md.AppendLine($"## {group}");
            md.AppendLine();
            md.AppendLine("| Type | Description |");
            md.AppendLine("| --- | --- |");
            foreach (var type in members)
                md.AppendLine($"| [`{TypeName(type, withArguments: true)}`](/api/{Slug(type)}) | {Cell(FirstSentence(Summary(Doc(type))))} |");
        }
        return md.ToString();
    }

    public string TypePage(Type type)
    {
        _current = type;
        var doc = Doc(type);
        var md = new StringBuilder();
        md.AppendLine("---");
        md.AppendLine("outline: [2, 3]");
        md.AppendLine("---");
        md.AppendLine();
        md.AppendLine("<!-- Generated by tools/apigen.cs from Lustral.Api. Do not edit; run `npm run api` to regenerate. -->");
        md.AppendLine();
        md.AppendLine($"# {Escape(TypeName(type, withArguments: true))} <Badge type=\"info\" text=\"{Kind(type)}\" />");
        md.AppendLine();
        md.AppendLine($"<p class=\"api-meta\">Namespace <code>{type.Namespace}</code> · Assembly <code>{type.Assembly.GetName().Name}</code></p>");
        md.AppendLine();
        AppendText(md, Summary(doc));
        md.AppendLine("```csharp");
        md.AppendLine(TypeDeclaration(type));
        md.AppendLine("```");
        md.AppendLine();
        AppendTypeParameters(md, doc);
        AppendSection(md, doc, "remarks", "Remarks", level: 2);
        AppendSection(md, doc, "example", "Example", level: 2);

        if (type.IsEnum)
        {
            md.AppendLine("## Values");
            md.AppendLine();
            md.AppendLine("| Name | Value | Description |");
            md.AppendLine("| --- | --- | --- |");
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                md.AppendLine($"| `{field.Name}` | {Convert.ToInt64(field.GetRawConstantValue())} | {Cell(Summary(Doc(field)))} |");
            md.AppendLine();
            return md.ToString();
        }

        const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        var members = type.GetMembers(declared).Where(Visible).ToList();
        var constructors = members.OfType<ConstructorInfo>().Where(c => !c.IsStatic).ToList();
        var properties = members.OfType<PropertyInfo>().OrderBy(p => p.Name, StringComparer.Ordinal).ToList();
        var fields = members.OfType<FieldInfo>().Where(f => !f.IsSpecialName).OrderBy(f => f.Name, StringComparer.Ordinal).ToList();
        var events = members.OfType<EventInfo>().OrderBy(e => e.Name, StringComparer.Ordinal).ToList();
        var methods = members.OfType<MethodInfo>().Where(m => !m.IsSpecialName).OrderBy(m => m.Name, StringComparer.Ordinal).ThenBy(m => m.GetParameters().Length).ToList();
        var operators = members.OfType<MethodInfo>().Where(m => m.IsSpecialName && m.Name.StartsWith("op_", StringComparison.Ordinal)).ToList();

        AppendMembers(md, "Constructors", constructors);
        AppendMembers(md, "Properties", properties);
        AppendMembers(md, "Fields", fields);
        AppendMembers(md, "Events", events);
        AppendMembers(md, "Methods", methods);
        AppendMembers(md, "Operators", operators);
        return md.ToString();
    }

    private static bool Visible(MemberInfo member) => member switch
    {
        MethodBase method => method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly,
        PropertyInfo property => property.GetAccessors(nonPublic: true).Any(a => a.IsPublic || a.IsFamily || a.IsFamilyOrAssembly),
        FieldInfo field => field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly,
        EventInfo e => e.AddMethod is { } add && (add.IsPublic || add.IsFamily),
        _ => false,
    };

    private static string Kind(Type type) =>
        type.IsEnum ? "enum" : type.IsInterface ? "interface" : type.IsValueType ? "struct" : "class";

    private void AppendMembers(StringBuilder md, string title, IEnumerable<MemberInfo> members)
    {
        var list = members.ToList();
        if (list.Count == 0)
            return;
        md.AppendLine($"## {title}");
        md.AppendLine();
        foreach (var member in list)
        {
            var doc = Doc(member);
            md.AppendLine($"### {Escape(MemberTitle(member))} {{#{Anchor(member)}}}");
            md.AppendLine();
            md.AppendLine("```csharp");
            md.AppendLine(Declaration(member));
            md.AppendLine("```");
            md.AppendLine();
            AppendText(md, Summary(doc));
            AppendTypeParameters(md, doc);
            var parameters = doc?.Elements("param").ToList() ?? [];
            if (parameters.Count > 0)
            {
                md.AppendLine("**Parameters**");
                md.AppendLine();
                foreach (var parameter in parameters)
                    md.AppendLine($"- `{(string)parameter.Attribute("name")!}`: {Inline(parameter)}");
                md.AppendLine();
            }
            if (doc?.Element("returns") is { } returns)
            {
                md.AppendLine($"**Returns** {Inline(returns)}");
                md.AppendLine();
            }
            var exceptions = doc?.Elements("exception").ToList() ?? [];
            if (exceptions.Count > 0)
            {
                md.AppendLine("**Exceptions**");
                md.AppendLine();
                foreach (var exception in exceptions)
                    md.AppendLine($"- {CrefLink((string)exception.Attribute("cref")!, null)}: {Inline(exception)}");
                md.AppendLine();
            }
            AppendSection(md, doc, "remarks", "Remarks", level: 0);
            AppendSection(md, doc, "example", "Example", level: 0);
        }
    }

    private void AppendTypeParameters(StringBuilder md, XElement? doc)
    {
        var typeParameters = doc?.Elements("typeparam").ToList() ?? [];
        if (typeParameters.Count == 0)
            return;
        md.AppendLine("**Type parameters**");
        md.AppendLine();
        foreach (var parameter in typeParameters)
            md.AppendLine($"- `{(string)parameter.Attribute("name")!}`: {Inline(parameter)}");
        md.AppendLine();
    }

    private void AppendSection(StringBuilder md, XElement? doc, string element, string title, int level)
    {
        if (doc?.Element(element) is not { } section)
            return;
        md.AppendLine(level == 0 ? $"**{title}**" : $"{new string('#', level)} {title}");
        md.AppendLine();
        AppendText(md, section);
    }

    private void AppendText(StringBuilder md, XElement? element)
    {
        if (element is null)
            return;
        var text = Blocks(element).Trim();
        if (text.Length == 0)
            return;
        md.AppendLine(text);
        md.AppendLine();
    }

    // ---- Documentation text -------------------------------------------------------------------------------------

    private XElement? Doc(MemberInfo member)
    {
        if (!docs.TryGetValue(DocId(member), out var doc))
            return null;
        // <inheritdoc/> on overrides of System.Object methods uses the framework's descriptions.
        if (doc.Element("inheritdoc") is not null && doc.Element("summary") is null && member is MethodInfo inherited)
        {
            var summary = inherited.Name switch
            {
                "ToString" => "A description for logs.",
                "Equals" => "Whether the other value is equal to this one.",
                "GetHashCode" => "A hash code for this value.",
                "Dispose" => "Releases what this holds.",
                "GetEnumerator" => "Enumerates the items in order.",
                _ => null,
            };
            return summary is null ? doc : new XElement("member", new XElement("summary", summary));
        }
        return doc;
    }

    private static XElement? Summary(XElement? doc) => doc?.Element("summary");

    // The first sentence of the summary, used in the index, excluding code, lists and further paragraphs.
    private string FirstSentence(XElement? summary)
    {
        if (summary is null)
            return "";
        var clone = new XElement(summary);
        foreach (var block in clone.Elements().Where(e => e.Name == "code" || e.Name == "list" || e.Name == "para").ToList())
            block.Remove();
        var text = Inline(clone).Trim();
        var end = text.IndexOf(". ", StringComparison.Ordinal);
        if (end > 0)
            text = text[..(end + 1)];
        return text.EndsWith(':') ? text[..^1] + "." : text;
    }

    // Block-level text: paragraphs, code blocks and lists, separated by blank lines.
    private string Blocks(XElement element)
    {
        var md = new StringBuilder();
        var line = new StringBuilder();
        void Flush()
        {
            var text = line.ToString().Trim();
            if (text.Length > 0)
                md.Append(text).Append("\n\n");
            line.Clear();
        }
        foreach (var node in element.Nodes())
        {
            switch (node)
            {
                case XElement { Name.LocalName: "para" } para:
                    Flush();
                    md.Append(Blocks(para));
                    break;
                case XElement { Name.LocalName: "code" } code:
                    Flush();
                    md.Append("```csharp\n").Append(Dedent(code.Value)).Append("\n```\n\n");
                    break;
                case XElement { Name.LocalName: "list" } list:
                    Flush();
                    foreach (var item in list.Elements("item"))
                    {
                        var term = item.Element("term");
                        var description = item.Element("description");
                        var text = term is not null && description is not null ? $"**{Inline(term)}**: {Inline(description)}"
                            : description is not null ? Inline(description) : Inline(item);
                        md.Append(list.Attribute("type")?.Value == "number" ? "1. " : "- ").Append(text).Append('\n');
                    }
                    md.Append('\n');
                    break;
                default:
                    line.Append(InlineNode(node));
                    break;
            }
        }
        Flush();
        return md.ToString();
    }

    private string Inline(XElement element) => string.Concat(element.Nodes().Select(InlineNode)).Trim();

    private string InlineNode(XNode node) => node switch
    {
        XText text => Escape(CollapseSpaces(text.Value)),
        XElement { Name.LocalName: "c" } c => Code(c.Value),
        XElement { Name.LocalName: "see" or "seealso" } see when see.Attribute("cref") is { } cref => CrefLink(cref.Value, see.Value.Length > 0 ? see.Value : null),
        XElement { Name.LocalName: "see" } see when see.Attribute("href") is { } href => $"[{(see.Value.Length > 0 ? Escape(see.Value) : href.Value)}]({href.Value})",
        XElement { Name.LocalName: "see" } see when see.Attribute("langword") is { } word => Code(word.Value),
        XElement { Name.LocalName: "paramref" or "typeparamref" } reference => Code((string)reference.Attribute("name")!),
        XElement { Name.LocalName: "br" } => "<br>",
        XElement { Name.LocalName: "para" } para => " " + Inline(para) + " ",
        XElement { Name.LocalName: "list" } list => " " + string.Join("; ", list.Elements("item").Select(Inline)) + " ",
        XElement other => Inline(other),
        _ => "",
    };

    private static string CollapseSpaces(string text)
    {
        var builder = new StringBuilder(text.Length);
        var space = false;
        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                if (!space)
                    builder.Append(' ');
                space = true;
            }
            else
            {
                builder.Append(c);
                space = false;
            }
        }
        return builder.ToString();
    }

    private static string Dedent(string code)
    {
        var lines = code.Replace("\r\n", "\n").Split('\n').ToList();
        while (lines.Count > 0 && lines[0].Trim().Length == 0)
            lines.RemoveAt(0);
        while (lines.Count > 0 && lines[^1].Trim().Length == 0)
            lines.RemoveAt(lines.Count - 1);
        var indent = lines.Where(l => l.Trim().Length > 0).Select(l => l.Length - l.TrimStart().Length).DefaultIfEmpty(0).Min();
        return string.Join("\n", lines.Select(l => l.Length >= indent ? l[indent..] : l.TrimStart()));
    }

    // Escapes angle brackets and braces, which Markdown and Vue would otherwise parse as HTML or template syntax.
    private static string Escape(string text) =>
        text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("{", "&#123;").Replace("}", "&#125;").Replace("*", "\\*").Replace("_", "\\_");

    private static string Code(string text) => text.Contains('`') ? $"`` {text} ``" : $"`{text}`";

    private static string Cell(string markdown) => markdown.Replace("\n", " ").Replace("|", "\\|").Trim();

    private string Cell(XElement? summary) => summary is null ? "" : Cell(Inline(summary));

    // ---- Cross-references ---------------------------------------------------------------------------------------

    private string CrefLink(string cref, string? text)
    {
        var (url, display) = Resolve(cref);
        display = text is not null ? Escape(text) : Code(display);
        return url is null ? display : $"[{display}]({url})";
    }

    private (string? Url, string Display) Resolve(string cref)
    {
        var kind = cref[0];
        var name = cref[2..];
        var parenthesis = name.IndexOf('(');
        var path = parenthesis < 0 ? name : name[..parenthesis];
        if (kind == 'T')
        {
            var type = types.FirstOrDefault(t => DocId(t) == cref);
            var shortName = ShortName(path);
            if (type is not null)
                return ($"/api/{Slug(type)}", TypeName(type, withArguments: false));
            return (ExternalUrl(path), shortName);
        }
        var dot = path.LastIndexOf('.');
        var typeId = "T:" + path[..dot];
        var memberName = path[(dot + 1)..].Replace("``1", "").Replace("#ctor", ShortName(path[..dot]));
        var owner = types.FirstOrDefault(t => DocId(t) == typeId);
        if (owner is not null)
        {
            var member = owner.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .FirstOrDefault(m => DocId(m) == cref);
            var display = owner == _current ? memberName : $"{TypeName(owner, withArguments: false)}.{memberName}";
            return (member is null ? $"/api/{Slug(owner)}" : $"/api/{Slug(owner)}#{Anchor(member)}", display);
        }
        return (ExternalUrl(path[..dot]), $"{ShortName(path[..dot])}.{memberName}");
    }

    private static string ShortName(string fullName)
    {
        var name = fullName[(fullName.LastIndexOf('.') + 1)..];
        var tick = name.IndexOf('`');
        return tick < 0 ? name : name[..tick];
    }

    private static string? ExternalUrl(string typeFullName) =>
        typeFullName.StartsWith("System.", StringComparison.Ordinal)
            ? $"https://learn.microsoft.com/dotnet/api/{typeFullName.Replace('`', '-').ToLowerInvariant()}"
            : typeFullName.StartsWith("HarmonyLib.", StringComparison.Ordinal)
                ? $"https://harmony.pardeike.net/api/{typeFullName}.html"
                : null;

    // ---- Names, anchors and declarations ------------------------------------------------------------------------

    private string MemberTitle(MemberInfo member) => member switch
    {
        ConstructorInfo c => $"{TypeName(c.DeclaringType!, withArguments: false)}({string.Join(", ", c.GetParameters().Select(p => TypeName(p.ParameterType, true)))})",
        MethodInfo m when m.Name.StartsWith("op_", StringComparison.Ordinal) => $"operator {Operator(m.Name)}",
        MethodInfo m => $"{m.Name}{GenericParameters(m)}({string.Join(", ", m.GetParameters().Select(p => TypeName(p.ParameterType, true)))})",
        PropertyInfo p when p.GetIndexParameters().Length > 0 => $"this[{string.Join(", ", p.GetIndexParameters().Select(i => TypeName(i.ParameterType, true)))}]",
        _ => member.Name,
    };

    private static string GenericParameters(MethodInfo method) =>
        method.IsGenericMethodDefinition ? "<" + string.Join(", ", method.GetGenericArguments().Select(a => a.Name)) + ">" : "";

    private string Anchor(MemberInfo member)
    {
        var overloaded = member is MethodBase method && member.DeclaringType!
            .GetMember(member.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Count(Visible) > 1;
        var name = member switch
        {
            ConstructorInfo => "constructor",
            MethodInfo m when m.Name.StartsWith("op_", StringComparison.Ordinal) => "operator-" + m.Name[3..].ToLowerInvariant(),
            PropertyInfo p when p.GetIndexParameters().Length > 0 => "indexer",
            _ => member.Name.ToLowerInvariant(),
        };
        if (overloaded && member is MethodBase overload)
            name += "-" + string.Join("-", overload.GetParameters().Select(p => Kebab(TypeName(p.ParameterType, false).Replace("[]", "Array"))));
        return name.TrimEnd('-');
    }

    private static string Operator(string name) => name switch
    {
        "op_Equality" => "==",
        "op_Inequality" => "!=",
        "op_Implicit" => "implicit",
        "op_Explicit" => "explicit",
        _ => name[3..],
    };

    private string TypeDeclaration(Type type)
    {
        var text = new StringBuilder("public ");
        if (type.IsEnum)
            return text.Append("enum ").Append(type.Name).ToString();
        if (type.IsInterface)
            text.Append("interface ");
        else if (type.IsValueType)
            text.Append(type.GetCustomAttributesData().Any(a => a.AttributeType.Name == "IsReadOnlyAttribute") ? "readonly struct " : "struct ");
        else if (type.IsAbstract && type.IsSealed)
            text.Append("static class ");
        else
            text.Append(type.IsAbstract ? "abstract class " : type.IsSealed ? "sealed class " : "class ");
        text.Append(TypeName(type, withArguments: true));
        var bases = new List<string>();
        if (type.BaseType is { } baseType && baseType != typeof(object) && baseType != typeof(ValueType))
            bases.Add(TypeName(baseType, true));
        var interfaces = type.GetInterfaces();
        var inherited = interfaces.SelectMany(i => i.GetInterfaces()).Concat(type.BaseType?.GetInterfaces() ?? []).ToHashSet();
        bases.AddRange(interfaces.Where(i => !inherited.Contains(i)).Select(i => TypeName(i, true)));
        if (bases.Count > 0)
            text.Append(" : ").Append(string.Join(", ", bases));
        return text.ToString();
    }

    private string Declaration(MemberInfo member)
    {
        switch (member)
        {
            case ConstructorInfo constructor:
                return $"{Access(constructor)} {TypeName(constructor.DeclaringType!, false)}({Parameters(constructor)})";
            case MethodInfo method when method.Name.StartsWith("op_", StringComparison.Ordinal):
                return $"public static {Returned(method)} operator {Operator(method.Name)}({Parameters(method)})";
            case MethodInfo method:
                return $"{Modifiers(method)}{Returned(method)} {method.Name}{GenericParameters(method)}({Parameters(method)}){Constraints(method)};";
            case PropertyInfo property:
            {
                var accessor = property.GetMethod ?? property.SetMethod!;
                var name = property.GetIndexParameters().Length > 0
                    ? $"this[{string.Join(", ", property.GetIndexParameters().Select(Parameter))}]"
                    : property.Name;
                var accessors = (property.GetMethod is { } get && Visible(get) ? "get; " : "") +
                                (property.SetMethod is { } set && (set.IsPublic || set.IsFamily) ? "set; " : "");
                return $"{Modifiers(accessor)}{TypeName(property.PropertyType, true, _nullability.Create(property))} {name} {{ {accessors}}}";
            }
            case EventInfo e:
                return $"{Modifiers(e.AddMethod!)}event {TypeName(e.EventHandlerType!, true, _nullability.Create(e))} {e.Name};";
            case FieldInfo field:
                return $"public {(field.IsLiteral ? "const " : field.IsStatic ? "static " : "")}{(field.IsInitOnly ? "readonly " : "")}{TypeName(field.FieldType, true, _nullability.Create(field))} {field.Name};";
            default:
                return member.Name;
        }
    }

    private static bool Visible(MethodInfo method) => method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly;

    private static string Access(MethodBase method) => method.IsPublic ? "public" : "protected";

    private static string Modifiers(MethodInfo method)
    {
        if (method.DeclaringType!.IsInterface)
            return "";
        var text = Access(method) + " ";
        if (method.IsStatic)
            text += "static ";
        else if (method.IsAbstract)
            text += "abstract ";
        else if (method.IsVirtual && !method.IsFinal)
            text += method.GetBaseDefinition() != method ? "override " : "virtual ";
        else if (method.IsVirtual && method.GetBaseDefinition() != method)
            text += "override ";
        return text;
    }

    private string Returned(MethodInfo method) => TypeName(method.ReturnType, true, _nullability.Create(method.ReturnParameter));

    private string Parameters(MethodBase method) => string.Join(", ", method.GetParameters().Select(Parameter));

    private string Parameter(ParameterInfo parameter)
    {
        var text = $"{TypeName(parameter.ParameterType, true, _nullability.Create(parameter))} {parameter.Name}";
        if (parameter.HasDefaultValue)
            text += " = " + parameter.DefaultValue switch
            {
                null => parameter.ParameterType.IsValueType && Nullable.GetUnderlyingType(parameter.ParameterType) is null ? "default" : "null",
                bool b => b ? "true" : "false",
                string s => $"\"{s}\"",
                var other => Convert.ToString(other, System.Globalization.CultureInfo.InvariantCulture),
            };
        return text;
    }

    private static string Constraints(MethodInfo method)
    {
        if (!method.IsGenericMethodDefinition)
            return "";
        var text = new StringBuilder();
        foreach (var argument in method.GetGenericArguments())
        {
            var constraints = new List<string>();
            var attributes = argument.GenericParameterAttributes;
            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
                constraints.Add("class");
            if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                constraints.Add("struct");
            constraints.AddRange(argument.GetGenericParameterConstraints().Where(c => c != typeof(ValueType)).Select(c => TypeName(c, true)));
            if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) && !attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                constraints.Add("new()");
            if (constraints.Count > 0)
                text.Append($" where {argument.Name} : {string.Join(", ", constraints)}");
        }
        return text.ToString();
    }

    private static readonly Dictionary<Type, string> Keywords = new()
    {
        [typeof(void)] = "void", [typeof(object)] = "object", [typeof(string)] = "string", [typeof(bool)] = "bool",
        [typeof(byte)] = "byte", [typeof(int)] = "int", [typeof(long)] = "long", [typeof(float)] = "float",
        [typeof(double)] = "double", [typeof(char)] = "char", [typeof(uint)] = "uint", [typeof(short)] = "short",
    };

    public static string TypeName(Type type, bool withArguments, NullabilityInfo? nullability = null)
    {
        if (type.IsByRef)
            return "ref " + TypeName(type.GetElementType()!, withArguments);
        if (type.IsArray)
            return TypeName(type.GetElementType()!, withArguments, nullability?.ElementType) + "[]" + (nullability?.ReadState == NullabilityState.Nullable ? "?" : "");
        if (Nullable.GetUnderlyingType(type) is { } underlying)
            return TypeName(underlying, withArguments) + "?";
        if (Keywords.TryGetValue(type, out var keyword))
            return keyword + (nullability?.ReadState == NullabilityState.Nullable && !type.IsValueType ? "?" : "");
        var name = type.Name;
        var tick = name.IndexOf('`');
        if (tick >= 0)
            name = name[..tick];
        if (type.IsNested && !type.IsGenericParameter)
            name = TypeName(type.DeclaringType!, false) + "." + name;
        if (withArguments && type.IsGenericType)
        {
            var arguments = type.GetGenericArguments();
            var argumentNullability = nullability?.GenericTypeArguments;
            name += "<" + string.Join(", ", arguments.Select((a, i) => TypeName(a, true, argumentNullability is { Length: > 0 } n && i < n.Length ? n[i] : null))) + ">";
        }
        if (nullability?.ReadState == NullabilityState.Nullable && !type.IsValueType)
            name += "?";
        return name;
    }

    // ---- XML documentation ids ------------------------------------------------------------------------------------

    public static string DocId(MemberInfo member) => member switch
    {
        Type type => "T:" + TypeId(type),
        ConstructorInfo c => $"M:{TypeId(c.DeclaringType!)}.#ctor{ParameterIds(c)}",
        MethodInfo m => $"M:{TypeId(m.DeclaringType!)}.{m.Name}{(m.IsGenericMethodDefinition ? "``" + m.GetGenericArguments().Length : "")}{ParameterIds(m)}" +
                        (m.Name is "op_Implicit" or "op_Explicit" ? "~" + ParameterTypeId(m.ReturnType) : ""),
        PropertyInfo p => $"P:{TypeId(p.DeclaringType!)}.{p.Name}{(p.GetIndexParameters().Length > 0 ? "(" + string.Join(",", p.GetIndexParameters().Select(i => ParameterTypeId(i.ParameterType))) + ")" : "")}",
        FieldInfo f => $"F:{TypeId(f.DeclaringType!)}.{f.Name}",
        EventInfo e => $"E:{TypeId(e.DeclaringType!)}.{e.Name}",
        _ => "",
    };

    private static string TypeId(Type type) =>
        type.IsNested ? TypeId(type.DeclaringType!) + "." + type.Name : $"{type.Namespace}.{type.Name}";

    private static string ParameterIds(MethodBase method)
    {
        var parameters = method.GetParameters();
        return parameters.Length == 0 ? "" : "(" + string.Join(",", parameters.Select(p => ParameterTypeId(p.ParameterType))) + ")";
    }

    private static string ParameterTypeId(Type type)
    {
        if (type.IsByRef)
            return ParameterTypeId(type.GetElementType()!) + "@";
        if (type.IsArray)
            return ParameterTypeId(type.GetElementType()!) + "[]";
        if (type.IsGenericMethodParameter)
            return "``" + type.GenericParameterPosition;
        if (type.IsGenericParameter)
            return "`" + type.GenericParameterPosition;
        if (type.IsGenericType)
        {
            var definition = type.GetGenericTypeDefinition();
            var name = TypeId(definition);
            name = name[..name.IndexOf('`')];
            return $"{name}{{{string.Join(",", type.GetGenericArguments().Select(ParameterTypeId))}}}";
        }
        return TypeId(type);
    }
}
