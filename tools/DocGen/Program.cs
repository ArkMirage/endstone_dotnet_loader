using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Endstone.DocGen;

/// <summary>
/// Code generator: turns the public API of Endstone.Loader (src/csharp) into
/// MkDocs reference pages under docs/reference/csharp/.
///
/// Pipeline:
///   1. dotnet build with GenerateDocumentationFile=true -> Endstone.Loader.xml
///   2. Reflect over Endstone.Loader.dll for the public API surface
///   3. Merge XML doc comments (summary/param/returns) with the reflected members
///   4. Render one markdown page per type, plus events, enums and index pages
///
/// Every page is emitted twice (mkdocs-static-i18n suffix structure):
///   Foo.en.md  - English, from the source doc comments
///   Foo.zh.md  - Chinese, merged from tools/translations.json with English fallback
/// </summary>
internal static class Program
{
    /// <summary>mkdocs-static-i18n suffix structure: one file per locale.</summary>
    private const string EnSuffix = ".en";
    private const string ZhSuffix = ".zh";

    private static string _dllPath = "";
    private static string _xmlPath = "";
    private static string _outDir = "";
    private static string _translationsPath = "";
    private static XDocument? _docs;
    private static Dictionary<string, object> _translations = new();
    private static Assembly _assembly = null!;

    private static int Main(string[] args)
    {
        var root = FindRepoRoot();
        var build = false;
        foreach (var arg in args)
        {
            switch (arg)
            {
                case "--build": build = true; break;
                case "--help": case "-h":
                    Console.WriteLine("""
                        Endstone.DocGen - generates Markdown API reference pages from src/csharp.

                        Usage:
                          dotnet run --project tools/DocGen -- [--build]

                        Options:
                          --build   Rebuild src/csharp with GenerateDocumentationFile first.
                          --help    Show this help.
                        """);
                    return 0;
            }
        }

        const string rid = "net10.0";
        var binDir = Path.Combine(root, "src", "csharp", "bin", "Release", rid);
        _dllPath = Path.Combine(binDir, "Endstone.Loader.dll");
        _xmlPath = Path.Combine(binDir, "Endstone.Loader.xml");
        _outDir = Path.Combine(root, "docs", "reference", "csharp");
        _translationsPath = Path.Combine(root, "tools", "translations.json");

        if (build || !File.Exists(_dllPath) || !File.Exists(_xmlPath))
        {
            Console.WriteLine("[DocGen] Building Endstone.Loader with XML documentation...");
            Run(root, "dotnet",
                "build src/csharp/Endstone.Loader.csproj -c Release -p:GenerateDocumentationFile=true --nologo -v q");
        }

        if (!File.Exists(_dllPath) || !File.Exists(_xmlPath))
        {
            Console.Error.WriteLine("[DocGen] Endstone.Loader.dll / .xml not found after build.");
            return 1;
        }

        Console.WriteLine($"[DocGen] Loading {_dllPath}");
        _assembly = Assembly.LoadFrom(_dllPath);
        _docs = XDocument.Load(_xmlPath);
        _translations = LoadJsonObject(_translationsPath);

        Directory.CreateDirectory(_outDir);
        foreach (var stale in Directory.EnumerateFiles(_outDir, "*.md"))
        {
            File.Delete(stale);
        }
        GenerateAll();

        Console.WriteLine($"[DocGen] Reference pages written to {_outDir}");
        Console.WriteLine("[DocGen] Done. Run 'mkdocs serve' to preview the site.");
        return 0;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "src", "csharp", "Endstone.Loader.csproj")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Cannot locate the repository root (src/csharp/Endstone.Loader.csproj).");
    }

    private static void Run(string workDir, string fileName, string args)
    {
        var psi = new System.Diagnostics.ProcessStartInfo(fileName, args)
        {
            WorkingDirectory = workDir,
            UseShellExecute = false,
        };
        var p = System.Diagnostics.Process.Start(psi)!;
        p.WaitForExit();
        if (p.ExitCode != 0)
        {
            throw new InvalidOperationException($"{fileName} exited with code {p.ExitCode}.");
        }
    }

    // ------------------------------------------------------------------ //
    // Metadata model
    // ------------------------------------------------------------------ //

    private sealed record ApiType(
        TypeInfo Type, string Kind, string Summary, string[] Ctors, List<ApiProp> Props,
        List<ApiMethod> Methods, List<ApiField> Fields);

    private sealed record ApiProp(string Name, string Type, bool IsStatic, bool Get, bool Set, string Summary);

    private sealed record ApiMethod(
        string Name, string ReturnType, string ParamsDisplay, string[] ParamDocs, string Summary, string Returns,
        string SignatureKey);

    private sealed record ApiField(string Name, string Type, bool IsStatic, string Summary);

    // ------------------------------------------------------------------ //
    // Generation
    // ------------------------------------------------------------------ //

    private static void GenerateAll()
    {
        var publicTypes = _assembly.GetExportedTypes()
            .Where(t => !t.IsNested)
            .Select(t => t.GetTypeInfo())
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();

        var enums = publicTypes.Where(t => t.IsEnum).ToList();
        var events = publicTypes.Where(IsEventType).ToList();
        var classes = publicTypes
            .Where(t => !t.IsEnum && !IsEventType(t))
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .ToList();

        foreach (var type in classes)
        {
            var (en, zh) = RenderTypePage(type);
            var fileName = SafeFileName(type.Name);
            File.WriteAllText(Path.Combine(_outDir, fileName + EnSuffix + ".md"), en, Encoding.UTF8);
            File.WriteAllText(Path.Combine(_outDir, fileName + ZhSuffix + ".md"), zh, Encoding.UTF8);
        }

        var (eventsEn, eventsZh) = RenderEventPage(events);
        File.WriteAllText(Path.Combine(_outDir, "events" + EnSuffix + ".md"), eventsEn, Encoding.UTF8);
        File.WriteAllText(Path.Combine(_outDir, "events" + ZhSuffix + ".md"), eventsZh, Encoding.UTF8);

        var (enumsEn, enumsZh) = RenderEnumPage(enums);
        File.WriteAllText(Path.Combine(_outDir, "enums" + EnSuffix + ".md"), enumsEn, Encoding.UTF8);
        File.WriteAllText(Path.Combine(_outDir, "enums" + ZhSuffix + ".md"), enumsZh, Encoding.UTF8);

        var (indexEn, indexZh) = RenderIndex(classes, events, enums);
        File.WriteAllText(Path.Combine(_outDir, "index" + EnSuffix + ".md"), indexEn, Encoding.UTF8);
        File.WriteAllText(Path.Combine(_outDir, "index" + ZhSuffix + ".md"), indexZh, Encoding.UTF8);
    }

    private static bool IsEventType(TypeInfo t)
    {
        var b = t.BaseType;
        while (b != null)
        {
            if (b.FullName == "Endstone.Loader.Event") return true;
            b = b.BaseType;
        }
        return false;
    }

    private static ApiType BuildType(TypeInfo t)
    {
        var summary = Doc("T:" + t.FullName, "summary") ?? "";
        var props = new List<ApiProp>();
        var methods = new List<ApiMethod>();
        var fields = new List<ApiField>();
        var ctors = new List<string>();

        foreach (var p in t.DeclaredProperties)
        {
            if (!IsPublic(p.GetMethod) && !IsPublic(p.SetMethod)) continue;
            var sig = XmlMethodId("P", t, p.Name, Array.Empty<Type>());
            var isStatic = (p.GetMethod ?? p.SetMethod)!.IsStatic;
            props.Add(new ApiProp(p.Name, SimplifyType(p.PropertyType), isStatic,
                p.GetMethod != null && IsPublic(p.GetMethod),
                p.SetMethod != null && IsPublic(p.SetMethod),
                Doc(sig, "summary") ?? ""));
        }

        foreach (var m in t.DeclaredMethods)
        {
            if (m.IsPrivate || m.IsAssembly || m.IsFamilyAndAssembly)
            {
                continue;
            }
            if (m.IsSpecialName && (m.Name.StartsWith("get_") || m.Name.StartsWith("set_") ||
                                    m.Name.StartsWith("add_") || m.Name.StartsWith("remove_")))
            {
                continue;
            }
            if (IsBoilerplate(m)) continue;
            var sig = XmlMethodId("M", t, m.Name, m.GetParameters().Select(p => p.ParameterType).ToArray());

            var paramNames = m.GetParameters().Select(p => p.Name).ToArray();
            var paramDocs = new string[paramNames.Length];
            for (var i = 0; i < paramNames.Length; i++)
            {
                paramDocs[i] = Doc(sig, $"param/[@name='{paramNames[i]}']") ?? "";
            }
            methods.Add(new ApiMethod(m.Name, SimplifyType(m.ReturnType),
                string.Join(", ", m.GetParameters().Select(ParameterDisplay)), paramDocs,
                Doc(sig, "summary") ?? "", Doc(sig, "returns") ?? "",
                m.Name + "(" +
                string.Join(",", m.GetParameters().Select(p => SimplifyType(p.ParameterType))) + ")"));
        }

        foreach (var f in t.DeclaredFields)
        {
            if (t.IsEnum) continue;
            if (!f.IsPublic || f.IsSpecialName) continue;
            var sig = XmlMethodId("F", t, f.Name, Array.Empty<Type>());
            fields.Add(new ApiField(f.Name, SimplifyType(f.FieldType), f.IsStatic, Doc(sig, "summary") ?? ""));
        }

        foreach (var c in t.DeclaredConstructors)
        {
            if (!IsPublic(c)) continue;
            var sig = XmlMethodId("M", t, "#ctor", c.GetParameters().Select(p => p.ParameterType).ToArray());
            var ctorSummary = Doc(sig, "summary") ?? "";
            var display = string.Join(", ", c.GetParameters().Select(ParameterDisplay));
            ctors.Add((string.IsNullOrWhiteSpace(ctorSummary) ? "" : $" {ctorSummary}")
                .Insert(0, $"`{t.Name}({display})`"));
        }

        var kind = t.IsAbstract && t.IsSealed ? "static class"
            : t.IsAbstract ? "abstract class"
            : t.IsSealed ? "sealed class"
            : "class";
        if (t.IsEnum) kind = "enum";
        else if (KindOf(t) == "struct") kind = "struct";

        props.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        methods.Sort((a, b) => string.Compare(a.SignatureKey, b.SignatureKey, StringComparison.Ordinal));
        fields.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        return new ApiType(t, kind, summary, ctors.ToArray(), props, methods, fields);
    }

    private static string KindOf(TypeInfo t)
    {
        if (t.IsValueType && !t.IsEnum) return "struct";
        return "class";
    }

    private static bool IsPublic(MethodInfo? m) => m != null && m.IsPublic;

    private static bool IsPublic(ConstructorInfo c) => c.IsPublic;

    /// <summary>Filter compiler/object noise that adds no API value.</summary>
    private static bool IsBoilerplate(MethodInfo m)
    {
        return m.Name is "ToString" or "GetHashCode" or "Equals" or "Deconstruct" ||
               m.Name.StartsWith("op_", StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ //
    // XML doc id helpers
    // ------------------------------------------------------------------ //

    private static string XmlMethodId(string prefix, Type t, string name, Type[] paramTypes)
    {
        var typeName = (t.FullName ?? t.Name).Replace('+', '.');
        var sb = new StringBuilder($"{prefix}:{typeName}.{name}(");
        sb.Append(string.Join(",", paramTypes.Select(XmlTypeName)));
        sb.Append(')');
        return sb.ToString();
    }

    private static string XmlTypeName(Type t)
    {
        if (t.IsByRef) return XmlTypeName(t.GetElementType()!) + "@";
        if (t.IsPointer) return XmlTypeName(t.GetElementType()!) + "*";
        if (t.IsArray) return XmlTypeName(t.GetElementType()!) + "[]";
        if (t.IsGenericType)
        {
            var def = t.GetGenericTypeDefinition().FullName ?? t.Name;
            var tick = def.IndexOf('`');
            if (tick >= 0) def = def[..tick];
            return def + "{" + string.Join(",", t.GetGenericArguments().Select(XmlTypeName)) + "}";
        }
        return t.FullName ?? t.Name;
    }

    private static string? Doc(string id, string path)
    {
        if (_docs == null) return null;
        var node = _docs.Descendants("member")
            .FirstOrDefault(m => m.Attribute("name")?.Value == id);
        if (node == null) return null;

        string? value = null;
        if (path == "summary")
        {
            value = node.Element("summary")?.Value;
        }
        else if (path == "returns")
        {
            value = node.Element("returns")?.Value;
        }
        else if (path.StartsWith("param/"))
        {
            var name = path["param/".Length..].TrimStart();
            value = node.Elements("param").FirstOrDefault(p => p.Attribute("name")?.Value == name)?.Value;
        }
        else if (path == "remarks")
        {
            value = node.Element("remarks")?.Value;
        }
        else if (path == "values")
        {
            value = node.Element("value")?.Value;
        }
        var text = DocToMd(value ?? "");
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static string DocToMd(string value)
    {
        value = value
            .Replace("<para>", "\n\n")
            .Replace("</para>", "\n\n")
            .Replace("<c>", "`")
            .Replace("</c>", "`");
        value = Regex.Replace(value, "<see cref=\"([^\"]+)\"[^>]*?/>",
            m => "`" + SimplifyRef(m.Groups[1].Value) + "`");
        value = Regex.Replace(value, "<paramref name=\"([^\"]+)\"[^>]*?/>", "`$1`");
        value = Regex.Replace(value, "<see langword=\"([^\"]+)\"[^>]*?/>", "`$1`");
        value = Regex.Replace(value, "<[^>]+>", "");
        value = Regex.Replace(value, "(?<!\n)\n(?!\n)[ \\t]*", " ");
        value = Regex.Replace(value, "[ \\t]{2,}", " ");
        value = Regex.Replace(value, "\\n{3,}", "\n\n").Trim();
        return value;
    }

    private static string SimplifyRef(string cref)
    {
        var cut = cref.IndexOf('(');
        if (cut >= 0) cref = cref[..cut];
        var seg = cref.Split('.').LastOrDefault() ?? cref;
        seg = seg.Split('{')[0].TrimEnd('`', '1', '2', '3', '4', '5', '6', '7', '8', '9');
        return seg;
    }

    // ------------------------------------------------------------------ //
    // Type display helpers
    // ------------------------------------------------------------------ //

    private static readonly Dictionary<string, string> Alias = new()
    {
        ["System.String"] = "string",
        ["System.Int32"] = "int",
        ["System.Int64"] = "long",
        ["System.UInt32"] = "uint",
        ["System.UInt64"] = "ulong",
        ["System.Int16"] = "short",
        ["System.UInt16"] = "ushort",
        ["System.Byte"] = "byte",
        ["System.SByte"] = "sbyte",
        ["System.Single"] = "float",
        ["System.Double"] = "double",
        ["System.Boolean"] = "bool",
        ["System.Char"] = "char",
        ["System.Object"] = "object",
        ["System.Void"] = "void",
        ["System.Decimal"] = "decimal",
    };

    private static string SimplifyType(Type t)
    {
        if (t.IsByRef) return SimplifyType(t.GetElementType()!);
        if (t.IsPointer) return SimplifyType(t.GetElementType()!) + "*";
        if (t.IsArray) return SimplifyType(t.GetElementType()!) + "[]";
        var full = t.FullName ?? t.Name;
        if (t.IsGenericType)
        {
            var def = t.GetGenericTypeDefinition().FullName!;
            var tick = def.IndexOf('`');
            if (tick >= 0) def = def[..tick];
            if (def == "System.Nullable")
            {
                return SimplifyType(t.GetGenericArguments()[0]) + "?";
            }
            return Beautify(def) + "<" + string.Join(", ", t.GetGenericArguments().Select(SimplifyType)) + ">";
        }
        if (t.IsGenericParameter) return t.Name;
        return Beautify(full);
    }

    private static string Beautify(string full)
    {
        if (Alias.TryGetValue(full, out var alias)) return alias;
        return full.StartsWith("Endstone.Loader.") ? full["Endstone.Loader.".Length..] : full;
    }

    private static string ParameterDisplay(ParameterInfo p)
    {
        var prefix = p.IsOut ? "out " : p.ParameterType.IsByRef ? "ref " : "";
        var type = SimplifyType(p.ParameterType);
        if (p.IsDefined(typeof(ParamArrayAttribute))) type = "params " + type;
        var name = p.Name ?? "_";
        return $"{prefix}{type} {name}";
    }

    // ------------------------------------------------------------------ //
    // Rendering
    // ------------------------------------------------------------------ //

    private static (string En, string Zh) RenderTypePage(TypeInfo t)
    {
        var api = BuildType(t);
        var title = t.Name;
        if (t.IsGenericTypeDefinition && t.Name.Contains('`'))
        {
            title = t.Name.Split('`')[0] + "<" +
                    string.Join(", ", t.GetGenericArguments().Select(a => a.Name)) + ">";
        }

        var derived = DerivedClasses(t);
        var chain = !t.IsValueType ? BaseChain(t) : "";

        // ---- English page ----
        var lines = new List<string>
        {
            $"# {title}",
            "",
            $"`{api.Kind}`",
            "",
        };
        if (!string.IsNullOrWhiteSpace(api.Summary)) lines.Add(api.Summary);
        lines.Add("");
        lines.Add("**Namespace** `Endstone.Loader`");
        if (chain.Length > 0)
        {
            lines.Add("");
            lines.Add("**Inheritance** " + chain);
        }
        if (derived.Count > 0)
        {
            lines.Add("");
            lines.Add("**Derived classes** " + string.Join(", ", derived));
        }
        lines.Add("");
        RenderSections(lines, api, "en");

        // ---- Chinese page ----
        var zh = new List<string>
        {
            $"# {title}",
            "",
            $"`{api.Kind}`",
            "",
        };
        zh.Add(Translate("type", t.FullName ?? t.Name, api.Summary));
        zh.Add("");
        zh.Add("**命名空间** `Endstone.Loader`");
        if (chain.Length > 0)
        {
            zh.Add("");
            zh.Add("**继承** " + chain);
        }
        if (derived.Count > 0)
        {
            zh.Add("");
            zh.Add("**派生类** " + string.Join(", ", derived));
        }
        zh.Add("");
        RenderSections(zh, api, "zh");

        return (string.Join("\n", lines) + "\n", string.Join("\n", zh) + "\n");
    }

    private static void RenderSections(List<string> lines, ApiType api, string lang)
    {
        var useZh = lang == "zh";

        if (api.Ctors.Length > 0)
        {
            lines.Add($"## {SectionTitle("Constructors", useZh)}");
            lines.Add("");
            foreach (var ctor in api.Ctors)
            {
                lines.Add("- " + ctor);
            }
            lines.Add("");
        }
        if (api.Props.Count > 0)
        {
            lines.Add($"## {SectionTitle("Properties", useZh)}");
            lines.Add("");
            foreach (var p in api.Props)
            {
                lines.Add($"### `{p.Name}` : `{p.Type}`");
                lines.Add("");
                var accessors = (p.Get ? "get;" : "") + (p.Set ? "set;" : "");
                lines.Add((p.IsStatic ? "`static` " : "") + "`{ " + accessors.Trim() + " }`");
                lines.Add("");
                var summary = TranslateMember(api.Type, null, p.Name, p.Summary, useZh);
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    lines.Add(summary);
                    lines.Add("");
                }
            }
        }
        if (api.Methods.Count > 0)
        {
            lines.Add($"## {SectionTitle("Methods", useZh)}");
            lines.Add("");
            foreach (var m in api.Methods)
            {
                lines.Add($"### `{m.ReturnType} {m.Name}({m.ParamsDisplay})`");
                lines.Add("");
                var summary = TranslateMember(api.Type, m.SignatureKey, m.Name, m.Summary, useZh);
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    lines.Add(summary);
                    lines.Add("");
                }
                if (m.ParamDocs.Any(d => !string.IsNullOrWhiteSpace(d)))
                {
                    lines.Add($"**{SectionTitle("Parameters", useZh)}**");
                    lines.Add("");
                    var idx = 0;
                    foreach (var (name, _) in SplitParams(m.ParamsDisplay))
                    {
                        var docText = useZh
                            ? TranslateMember(api.Type, null, m.Name, m.ParamDocs[idx], zh: true)
                            : m.ParamDocs[idx];
                        lines.Add($"- `{name}` {docText}".TrimEnd());
                        idx++;
                    }
                    lines.Add("");
                }
                if (!string.IsNullOrWhiteSpace(m.Returns))
                {
                    lines.Add($"**{SectionTitle("Returns", useZh)}** {m.Returns}");
                    lines.Add("");
                }
            }
        }
        if (api.Fields.Count > 0)
        {
            lines.Add($"## {SectionTitle("Fields", useZh)}");
            lines.Add("");
            foreach (var f in api.Fields)
            {
                lines.Add($"### `{f.Name}` : `{f.Type}`");
                lines.Add("");
                var summary = TranslateMember(api.Type, null, f.Name, f.Summary, useZh);
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    lines.Add(summary);
                    lines.Add("");
                }
            }
        }
    }

    private static List<(string Name, string Type)> SplitParams(string display)
    {
        var result = new List<(string, string)>();
        if (string.IsNullOrWhiteSpace(display)) return result;
        foreach (var part in display.Split(','))
        {
            var trimmed = part.Trim();
            var name = trimmed.Split(' ').Last();
            result.Add((name, trimmed[..^name.Length].Trim()));
        }
        return result;
    }

    private static string SectionTitle(string en, bool zh)
    {
        if (!zh) return en;
        return en switch
        {
            "Constructors" => "构造函数",
            "Properties" => "属性",
            "Methods" => "方法",
            "Fields" => "字段",
            "Parameters" => "参数",
            "Returns" => "返回值",
            "Values" => "枚举值",
            "Members" => "成员",
            "Events" => "事件",
            _ => en,
        };
    }

    private static string? TranslateMember(TypeInfo type, string? sigKey, string? plainName, string? enFallback,
                                           bool zh)
    {
        if (!zh)
        {
            return string.IsNullOrWhiteSpace(enFallback) ? null : enFallback;
        }
        var entry = GetTypeEntry(type.FullName ?? type.Name);
        if (entry != null && entry.TryGetValue("members", out var m) && m is Dictionary<string, object> d)
        {
            var key = sigKey ?? plainName ?? "";
            if (d.TryGetValue(key, out var translated))
            {
                return translated.ToString();
            }
        }
        return string.IsNullOrWhiteSpace(enFallback) ? null : enFallback;
    }

    private static string Translate(string kind, string fullName, string? enFallback)
    {
        var entry = GetTypeEntry(fullName);
        if (entry != null && entry.TryGetValue("summary", out var zh))
        {
            return zh.ToString();
        }
        return string.IsNullOrWhiteSpace(enFallback) ? "" : enFallback;
    }

    private static Dictionary<string, object>? GetTypeEntry(string fullName)
    {
        if (_translations.TryGetValue("types", out var typesObj) &&
            typesObj is Dictionary<string, object> types)
        {
            var candidates = new[] { fullName, StripArity(fullName) };
            foreach (var candidate in candidates)
            {
                if (candidate != null && types.TryGetValue(candidate, out var entry) &&
                    entry is Dictionary<string, object> dict)
                {
                    return dict;
                }
            }
        }
        return null;
    }

    private static string? StripArity(string fullName)
    {
        var backtick = fullName.IndexOf('`');
        return backtick < 0 ? null : fullName[..backtick];
    }

    private static string BaseChain(TypeInfo t)
    {
        var parts = new List<string>();
        var b = t.BaseType;
        while (b != null && b != typeof(object))
        {
            parts.Add("`" + SimplifyType(b) + "`");
            b = b.BaseType;
        }
        parts.Add("`object`");
        return string.Join(" › ", parts);
    }

    private static List<string> DerivedClasses(TypeInfo t)
    {
        return _assembly.GetExportedTypes()
            .Where(x => x.BaseType?.FullName == t.FullName)
            .Select(x => "`" + x.Name + "`")
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();
    }

    // ------------------------------------------------------------------ //
    // events page - all events on one page
    // ------------------------------------------------------------------ //

    private static (string En, string Zh) RenderEventPage(List<TypeInfo> eventTypes)
    {
        var en = new StringBuilder();
        en.AppendLine("# Events");
        en.AppendLine();
        en.AppendLine("All event classes derive from `Event`. Register handlers with " +
                      "`PluginBase.RegisterEvent<T>()`. Events are raised synchronously on the " +
                      "server thread; most expose a `Player` and cancellation via `IsCancelled`.");
        en.AppendLine();
        en.AppendLine("| Event | Summary |");
        en.AppendLine("| --- | --- |");
        foreach (var t in eventTypes)
        {
            var summary = OneLine(Doc("T:" + t.FullName, "summary")) ?? "";
            en.AppendLine($"| [`{t.Name}`](#{t.Name.ToLowerInvariant()}) | {summary} |");
        }
        en.AppendLine();
        foreach (var t in eventTypes)
        {
            var api = BuildType(t);
            en.AppendLine($"## `{t.Name}`");
            en.AppendLine();
            if (!string.IsNullOrWhiteSpace(api.Summary))
            {
                en.AppendLine(api.Summary);
                en.AppendLine();
            }
            var body = new List<string>();
            RenderSections(body, api, "en");
            if (body.Count > 0)
            {
                en.Append(string.Join("\n", body));
                en.AppendLine();
            }
            en.AppendLine("---");
            en.AppendLine();
        }

        var zh = new StringBuilder();
        zh.AppendLine("# 事件");
        zh.AppendLine();
        zh.AppendLine("所有事件类均派生自 `Event`,通过 `PluginBase.RegisterEvent<T>()` 注册。");
        zh.AppendLine("事件在服务器主线程上同步触发;多数事件暴露 `Player`,并可通过 `IsCancelled` 取消。");
        zh.AppendLine();
        zh.AppendLine("| 事件 | 说明 |");
        zh.AppendLine("| --- | --- |");
        foreach (var t in eventTypes)
        {
            var english = Doc("T:" + t.FullName, "summary") ?? "";
            var translated = Translate("type", t.FullName ?? t.Name, english);
            zh.AppendLine($"| `{t.Name}` | {translated} |");
        }
        zh.AppendLine();
        foreach (var t in eventTypes)
        {
            var api = BuildType(t);
            zh.AppendLine($"## `{t.Name}`");
            zh.AppendLine();
            if (!string.IsNullOrWhiteSpace(api.Summary))
            {
                zh.AppendLine(Translate("type", t.FullName ?? t.Name, api.Summary));
                zh.AppendLine();
            }
            var body = new List<string>();
            RenderSections(body, api, "zh");
            if (body.Count > 0)
            {
                zh.Append(string.Join("\n", body));
                zh.AppendLine();
            }
            zh.AppendLine("---");
            zh.AppendLine();
        }
        return (en.ToString(), zh.ToString());
    }

    private static string? OneLine(string? text)
    {
        if (text == null) return null;
        var single = Regex.Replace(text, "\\s+", " ");
        return single.Length > 120 ? single[..117] + "..." : single;
    }

    // ------------------------------------------------------------------ //
    // enums page - all enums on one page
    // ------------------------------------------------------------------ //

    private static (string En, string Zh) RenderEnumPage(List<TypeInfo> enums)
    {
        var en = new StringBuilder();
        en.AppendLine("# Enums");
        en.AppendLine();
        en.AppendLine("Public enums used by the Endstone.Loader API.");
        en.AppendLine();
        en.AppendLine("| Enum | Purpose |");
        en.AppendLine("| --- | --- |");
        foreach (var t in enums)
        {
            en.AppendLine(
                $"| [`{t.Name}`](#{t.Name.ToLowerInvariant()}) | {OneLine(Doc("T:" + t.FullName, "summary")) ?? ""} |");
        }
        en.AppendLine();
        foreach (var t in enums)
        {
            en.AppendLine($"## `{t.Name}`");
            en.AppendLine();
            var summary = Doc("T:" + t.FullName, "summary");
            if (!string.IsNullOrWhiteSpace(summary))
            {
                en.AppendLine(summary);
                en.AppendLine();
            }
            en.AppendLine("| Value | Name |");
            en.AppendLine("| --- | --- |");
            foreach (var f in t.DeclaredFields.Where(f => f.IsLiteral))
            {
                en.AppendLine($"| `{f.GetRawConstantValue()}` | `{f.Name}` |");
            }
            en.AppendLine();
            en.AppendLine("---");
            en.AppendLine();
        }

        var zh = new StringBuilder();
        zh.AppendLine("# 枚举");
        zh.AppendLine();
        zh.AppendLine("Endstone.Loader API 使用的公开枚举。");
        zh.AppendLine();
        zh.AppendLine("| 枚举 | 用途 |");
        zh.AppendLine("| --- | --- |");
        foreach (var t in enums)
        {
            var translated = Translate("type", t.FullName ?? t.Name, Doc("T:" + t.FullName, "summary") ?? "");
            zh.AppendLine($"| `{t.Name}` | {translated} |");
        }
        zh.AppendLine();
        return (en.ToString(), zh.ToString());
    }

    // ------------------------------------------------------------------ //
    // index page
    // ------------------------------------------------------------------ //

    private static (string En, string Zh) RenderIndex(List<TypeInfo> classes, List<TypeInfo> events,
                                                      List<TypeInfo> enums)
    {
        var categories = new (string Title, string[] Names)[]
        {
            ("Core", new[] { "Server", "PluginBase", "PluginAttribute", "Logger", "Scheduler", "ScheduledTask",
                             "Service", "ServiceManager", "CommandSender", "CommandBuilder" }),
            ("Entities", new[] { "Actor", "Player", "Mob", "DamageSource", "Enchantment", "ItemEnchantment" }),
            ("World", new[] { "Level", "Dimension", "Chunk", "Block", "BlockState", "Location" }),
            ("Items & Inventory", new[] { "ItemStack", "Inventory", "PlayerInventory" }),
            ("UI", new[] { "FormBase`1", "MessageForm", "ActionForm", "ModalForm", "BossBar",
                           "MapView", "MapCanvas", "MapCursor", "MapColor", "MapRenderer" }),
            ("Native Interop", new[] { "Bootstrap" }),
        };

        var en = new StringBuilder();
        en.AppendLine("# Endstone.DotNet Loader API Reference");
        en.AppendLine();
        en.AppendLine("Every page below is **generated from source**: the code generator " +
                      "(`tools/DocGen`) reflects over `Endstone.Loader.dll`, merges the XML doc " +
                      "comments from `src/csharp`, and renders these Markdown pages. Never edit " +
                      "them by hand - run the generator instead.");
        en.AppendLine();

        foreach (var cat in categories)
        {
            en.AppendLine($"## {cat.Title}");
            en.AppendLine();
            en.AppendLine("| Type | Summary |");
            en.AppendLine("| --- | --- |");
            foreach (var name in cat.Names)
            {
                var t = classes.FirstOrDefault(x => x.FullName == "Endstone.Loader." + name);
                if (t == null) continue;
                var fileName = SafeFileName(t.Name) + ".md";
                var summary = OneLine(Doc("T:" + t.FullName, "summary")) ?? "";
                en.AppendLine($"| [`{t.Name}`]({fileName}) | {summary} |");
            }
            en.AppendLine();
        }

        en.AppendLine("## Events & Enums");
        en.AppendLine();
        en.AppendLine("| Page | Content |");
        en.AppendLine("| --- | --- |");
        en.AppendLine($"| [Events](events.md) | {events.Count} event classes deriving from `Event` |");
        en.AppendLine($"| [Enums](enums.md) | {enums.Count} public enums |");
        en.AppendLine();

        var zh = new StringBuilder();
        zh.AppendLine("# Endstone.DotNet Loader API 参考");
        zh.AppendLine();
        zh.AppendLine("以下每个页面都是**从源码生成**的:代码生成器(`tools/DocGen`)对 " +
                      "`Endstone.Loader.dll` 做反射,合并 `src/csharp` 中的 XML 文档注释,然后渲染成 " +
                      "这些 Markdown 页面。请勿手工编辑,运行生成器即可。");
        zh.AppendLine();
        foreach (var cat in categories)
        {
            var zhTitle = cat.Title switch
            {
                "Core" => "核心",
                "Entities" => "实体",
                "World" => "世界",
                "Items & Inventory" => "物品与背包",
                "UI" => "界面",
                "Native Interop" => "原生互操作",
                _ => cat.Title,
            };
            zh.AppendLine($"## {zhTitle}");
            zh.AppendLine();
            zh.AppendLine("| 类型 | 说明 |");
            zh.AppendLine("| --- | --- |");
            foreach (var name in cat.Names)
            {
                var t = classes.FirstOrDefault(x => x.FullName == "Endstone.Loader." + name);
                if (t == null) continue;
                var fileName = SafeFileName(t.Name) + ".md";
                var translated = Translate("type", t.FullName ?? t.Name,
                    Doc("T:" + t.FullName, "summary") ?? "");
                zh.AppendLine($"| [`{t.Name}`]({fileName}) | {translated} |");
            }
            zh.AppendLine();
        }
        zh.AppendLine("## 事件与枚举");
        zh.AppendLine();
        zh.AppendLine("| 页面 | 内容 |");
        zh.AppendLine("| --- | --- |");
        zh.AppendLine($"| [事件](events.md) | 派生自 `Event` 的 {events.Count} 个事件类 |");
        zh.AppendLine($"| [枚举](enums.md) | {enums.Count} 个公开枚举 |");
        zh.AppendLine();

        return (en.ToString(), zh.ToString());
    }

    private static string SafeFileName(string name) => name.Split('`')[0];

    // ------------------------------------------------------------------ //
    // translations.json loading
    // ------------------------------------------------------------------ //

    private static Dictionary<string, object> LoadJsonObject(string path)
    {
        if (!File.Exists(path)) return new Dictionary<string, object>();
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        return JsonToObject(doc.RootElement);
    }

    private static Dictionary<string, object> JsonToObject(JsonElement element)
    {
        var result = new Dictionary<string, object>();
        foreach (var prop in element.EnumerateObject())
        {
            result[prop.Name] = prop.Value.ValueKind switch
            {
                JsonValueKind.Object => JsonToObject(prop.Value),
                JsonValueKind.Array => prop.Value.EnumerateArray()
                    .Select<JsonElement, object>(e => e.ValueKind == JsonValueKind.Object
                        ? JsonToObject(e)
                        : e.ToString())
                    .ToList(),
                _ => prop.Value.ToString(),
            };
        }
        return result;
    }
}