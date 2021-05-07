using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CSharpToPlantUML
{
    public class TypeHolder
    {
        public Type ObjectType { get; }

        public TypeHolder(Type objectType)
        {
            ObjectType = objectType;
        }

        public IEnumerable<string> Exclusions { get; set; } =
            new[] {"System.", "Windows.", "Microsoft."};

        public IEnumerable<TypeHolder> GetNestedTypes() =>
            ObjectType.GetNestedTypes().Select(nestedType => new TypeHolder(nestedType));

        public PumlClip Generate(Layers layers)
        {
            var typeFullName = NormalizeName(ObjectType.FullName);
            var result = new PumlClip();

            var version = result.Version;

            var layerMap = (
                (layers & Layers.Type),
                (layers & Layers.Inheritance),
                (layers & Layers.NonPublic),
                (layers & Layers.Public),
                (layers & Layers.Relationships),
                (layers & Layers.Notes)
            );

            if (layerMap is (Layers.Type, _, _, _, _, _))
            {
                var typeMap =
                    (ObjectType.IsClass, ObjectType.IsEnum, ObjectType.IsArray, ObjectType.IsInterface,
                        ObjectType.IsValueType);

                result.Segments.Add((layers, @$"{(typeMap switch
                {
                    (true, _, _, _, _) => "class",
                    (_, true, _, _, _) => "enumeration",
                    (_, _, true, _, _) => "array",
                    (_, _, _, true, _) => "interface",
                    (_, _, _, _, true) => "struct",
                    _ => "component"
                })} ""{ObjectType.Name}"" as ""{typeFullName}"" {{"));

                if (version != result.Version)
                {
                    result.Segments.Add((layers, "\n}"));
                }
            }

            if (layerMap is (_, Layers.Inheritance, _, _, _, _))
            {
                var baseName = NormalizeName(ObjectType.BaseType?.FullName);
                result.Segments.Add((layers, @$"{(ObjectType.BaseType?.Name switch
                {
                    nameof(Object) => string.Empty,
                    null => string.Empty,
                    _ => GetExtends()
                })}"));

                string GetExtends()
                {
                    var skip = Exclusions.Any(e => baseName.StartsWith(e));
                    return !skip ? $"\"{typeFullName}\" extends \"{baseName}\"" : string.Empty;
                }

                ObjectType.GetInterfaces().OrderBy(i => i.FullName).ToList().ForEach(intfType =>
                {
                    var interfaceName = NormalizeName(intfType.FullName);
                    if (!Exclusions.Any(e => interfaceName.StartsWith(e)))
                    {
                        result.Segments.Add((layers, @$"{(intfType.Name switch
                        {
                            nameof(INullable) => string.Empty,
                            _ => $"\"{typeFullName}\" implements \"{interfaceName}\""
                        })}"));
                    }
                });
            }

            if (layerMap is (_, _, Layers.NonPublic, _, _, _))
            {
                var memberStrings = BuildConstructors().ToList();

                var fields = ObjectType.GetRuntimeFields().Where(fi => fi.IsPrivate);
                var properties = ObjectType.GetRuntimeProperties().Where(fi => fi.GetMethod?.IsPrivate ?? true);
                var methods = ObjectType.GetRuntimeMethods().Where(fi => fi.IsPrivate);
                var events = ObjectType.GetRuntimeEvents().Where(fi => fi.AddMethod?.IsPrivate ?? true);

                var members = from o in new[] {ObjectType}
                    join fi in fields on o equals fi.DeclaringType into fis
                    from rfi in fis.DefaultIfEmpty()
                    join pi in properties on o equals pi.DeclaringType into pis
                    from rpi in pis.DefaultIfEmpty()
                    join mi in methods on o equals mi.DeclaringType into mis
                    from rmi in mis.DefaultIfEmpty()
                    join ei in events on o equals ei.DeclaringType into eis
                    from rei in eis.DefaultIfEmpty()
                    select (ObjectType, rfi, rpi, rmi, rei);

                foreach (var member in members)
                {
                    if (member is (_, not null, _, _, _))
                        memberStrings.AddRange(BuildField(member.rfi, BindingFlags.NonPublic));

                    if (member is (_, _, not null, _, _))
                        memberStrings.AddRange(BuildProperty(member.rpi, BindingFlags.NonPublic));

                    if (member is (_, _, _, not null, _))
                        memberStrings.AddRange(BuildMethod(member.rmi, BindingFlags.NonPublic));

                    if (member is (_, _, _, _, not null))
                        memberStrings.AddRange(BuildEvent(member.rei, BindingFlags.NonPublic));
                }

                result.Segments.Add((Layers.NonPublic, string.Join("\n", memberStrings)));
            }

            if (layerMap is (_, _, _, Layers.Public, _, _))
            {
            }

            if (layerMap is (_, _, _, _, Layers.Relationships, _))
            {
            }

            if (layerMap is (_, _, _, _, _, Layers.Notes))
            {
            }

            return result;
        }

        private IEnumerable<string> BuildConstructors()
        {
            var ctors = ObjectType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var ctor in ctors)
            {
                var parList = ctor.GetParameters();

                var parms = MakeParList(parList);

                yield return $"{GetAccessibility(ctor.IsPublic, ctor.IsPrivate, ctor.IsFamily)}ctor({parms})";
            }
        }

        private IEnumerable<string> BuildMethod(MethodInfo? member, BindingFlags bindingFlags)
        {
            if (member is null) yield break;
            
            var methods = ObjectType.GetRuntimeMethods().Where(e => e.Name == member?.Name);

            foreach (var method in methods)
            {
                var parList = method?.GetParameters();
                var parms = MakeParList(parList);

                yield return
                    $"{GetAccessibility(method.IsPublic, method.IsPrivate, method.IsFamily)}{NormalizeName(method.Name)}({parms}) " +
                    $": {NormalizeName(method?.ReturnType?.FullName ?? "void")}";
            }
        }

        private IEnumerable<string> BuildField(FieldInfo? field, BindingFlags bindingFlags)
        {
            if (field is null) yield break;

            var isPublic = field.IsPublic;
            var isPrivate = field.IsPrivate;
            var isFamily = field.IsFamily;

            yield return
                $"{GetAccessibility(isPublic, isPrivate, isFamily)}{NormalizeName(field.Name)} " +
                $": {NormalizeName(field.FieldType.FullName)}";
        }

        private IEnumerable<string> BuildProperty(PropertyInfo? member, BindingFlags bindingFlags)
        {
            if (member is null) yield break;
            
            var properties = ObjectType.GetRuntimeProperties().Where(e => e.Name == member?.Name);

            foreach (var property in properties)
            {
                var parList = property?.GetIndexParameters();
                var parms = MakeParList(parList);

                var indexerParameters = string.IsNullOrWhiteSpace(parms)
                    ? string.Empty
                    : $"[ {parms} ]";

                var accessors = property.GetAccessors();
                var acc = accessors
                    .Select(accessor =>
                        $"{GetAccessibility(accessor.IsPublic, accessor.IsPrivate, accessor.IsFamily)}{NormalizeName(accessor.Name)}{indexerParameters}")
                    .ToList();

                var isPublic = (bindingFlags | BindingFlags.Public) == BindingFlags.Public;
                var isPrivate = (bindingFlags | BindingFlags.NonPublic) == BindingFlags.NonPublic;
                var isFamily = (property.GetMethod?.IsFamily ?? true) && !(property.SetMethod?.IsPublic ?? false);

                yield return
                    $"{GetAccessibility(isPublic, isPrivate, isFamily)}{NormalizeName(property.Name)} " +
                    $"( {string.Join(", ", acc)} ) " +
                    $": {NormalizeName(property.PropertyType.FullName)} << property >>";
            }
        }

        private static string GetAccessibility(bool methodIsPublic, bool methodIsPrivate, bool methodIsFamily)
        {
            return methodIsPublic
                ? "+"
                : methodIsFamily
                    ? "🔑"
                    : methodIsPrivate
                        ? "-"
                        : string.Empty;
        }

        private IEnumerable<string> BuildEvent(EventInfo? member, BindingFlags bindingFlags)
        {
            if (member is null) yield break;
            
            var events = ObjectType.GetRuntimeEvents().Where(e => e.Name == member?.Name);

            foreach (var evt in events)
            {
                var delegateType = evt.EventHandlerType;
                var method = delegateType?.GetMethod("Invoke");
                var parList = method?.GetParameters();
                var parms = MakeParList(parList);

                yield return
                    $"{GetAccessibility(method?.IsPublic ?? false, method?.IsPrivate ?? false, method?.IsFamily ?? false)}" +
                    $"{NormalizeName(evt.Name)}({parms}) : {NormalizeName(method?.ReturnType?.FullName ?? "void")} << event >>";
            }
        }

        private static string NormalizeName(string name)
        {
            if (name is null) return "<<No Name>>";

            name = name.Replace("+", ".");

            Regex regex = new(@"`[1-9]\[\[([^,\s]*).*\]\]");

            if (!regex.IsMatch(name)) return name;

            var match = regex.Match(name);
            var result = $"<{match.Groups.Values.LastOrDefault()?.Value}>";
            name = regex.Replace(name, result ?? match.Value);

            return name;
        }

        private string MakeParList(ParameterInfo[] parameters)
        {
            var p = parameters
                .Select(par => $"{NormalizeName(par.Name)} " +
                               $": {NormalizeName(par.ParameterType.FullName)} " +
                               $"{(par.HasDefaultValue ? $" = {par.DefaultValue}" : "")}")
                .ToArray();

            return string.Join(", ", p);
        }
    }
}