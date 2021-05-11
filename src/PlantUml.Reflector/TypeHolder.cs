using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using Microsoft.VisualBasic.CompilerServices;
using Microsoft.VisualBasic.FileIO;
// ReSharper disable UnusedMember.Global

#nullable enable
namespace PlantUml.Reflector
{
    public class TypeHolder
    {
        public Type ObjectType { get; }

        public TypeHolder(Type objectType)
        {
            ObjectType = objectType;
        }

        public IEnumerable<string> Exclusions { get; set; } =
            new[] { "System.", "Windows.", "Microsoft." };

        public IEnumerable<TypeHolder> GetNestedTypes() =>
            ObjectType.GetNestedTypes().Select(nestedType => new TypeHolder(nestedType));

        public PumlClip Generate(Layers layers, bool showAttributes = false)
        {
            var typeFullName = NormalizeName(ObjectType);
            var result = new PumlClip(typeFullName, ObjectType.Namespace, ObjectType.Assembly);

            var version = result.Version;

            var layerMap = (
                (layers & Layers.Type),
                (layers & Layers.Inheritance),
                (layers & Layers.NonPublic),
                (layers & Layers.Public),
                (layers & Layers.Relationships),
                (layers & Layers.Notes)
            );

            GetObject(layers, layerMap, result, typeFullName, version, showAttributes);

            GetInheritance(layerMap, result, typeFullName);

            GetRelationships(layerMap, result);

            GetNotes(layerMap, result);

            return result;
        }

        private void GetInheritance(
            (Layers, Layers, Layers, Layers, Layers, Layers) layerMap,
            PumlClip result,
            string typeFullName)
        {
            if (layerMap is not (_, Layers.Inheritance, _, _, _, _)) return;

            var baseName = NormalizeName(ObjectType.BaseType ?? typeof(object));
            result.Segments.Add((Layers.Inheritance, @$"{ObjectType.BaseType?.Name switch
            {
                "Object" => string.Empty,
                null => string.Empty,
                _ => GetExtends(ObjectType.BaseType.FullName ?? $"{ObjectType.BaseType.Namespace}.{baseName}")
            }}"));

            string GetExtends(string baseName)
            {
                var skip = Exclusions.Any(baseName.StartsWith);
                return !skip ? $"{typeFullName.AsSlug()} -u-|> {baseName.AsSlug()} : extends" : string.Empty;
            }

            ObjectType.GetInterfaces().OrderBy(i => i.FullName).ToList().ForEach(intfType =>
            {
                var baseName = NormalizeName(intfType);
                var interfaceName = NormalizeNameString(intfType.FullName ?? $"{intfType.Namespace}.{baseName}");
                
                if (!Exclusions.Any(e => interfaceName.StartsWith(e)))
                {
                    result.Segments.Add((Layers.Inheritance, @$"{(intfType.Name switch
                    {
                        "INullable" => string.Empty,
                        _ => $"{typeFullName.AsSlug()} --() {interfaceName.AsSlug()} : implements"
                    })}"));
                }
            });
        }

        private void GetObject(Layers layers,
            (Layers, Layers, Layers, Layers, Layers, Layers) layerMap,
            PumlClip result,
            string typeFullName,
            int version, bool showAttributes)
        {
            if (layerMap is (Layers.Type, _, _, _, _, _))
            {
                string typeName = NormalizeNameString(ObjectType.Name);
                string genericTypes = string.Empty;

                var genericType = ObjectType.IsGenericTypeDefinition
                    ? ObjectType
                    : ObjectType.IsGenericType
                        ? ObjectType.GetGenericTypeDefinition()
                        : null;

                if (genericType is not null)
                {
                    var args = genericType.GetGenericArguments();

                    genericTypes = string.Join(", ",
                        args.Select(NormalizeName));
                    genericTypes = $"<{genericTypes}>";
                }

                var typeMap =
                    (ObjectType.IsClass && !ObjectType.IsAbstract,
                        ObjectType.IsClass && ObjectType.IsAbstract, 
                        ObjectType.IsEnum, ObjectType.IsArray, ObjectType.IsInterface,
                        ObjectType.IsValueType);

                var objectType = typeMap switch
                {
                    (true, _, _, _, _, _) => "class",
                    (_, true, _, _, _, _) => "abstract class",
                    (_, _, true, _, _, _) => "enum",
                    (_, _, _, _, true, _) => "interface",
                    _ => "entity"
                };

                var attributes = GetAttributes(ObjectType.GetCustomAttributesData(), showAttributes);

                var staticObject = ObjectType.IsAbstract && ObjectType.IsSealed
                    ? "<< static >> " : string.Empty;

                result.Segments.Add((Layers.Type, @$"
{objectType} ""{typeName}{genericTypes}"" as {typeFullName.AsSlug()} 
{objectType} {typeFullName.AsSlug()} {staticObject}{{" + 
(showAttributes ? $"\n\t--- attributes ---\n{attributes}" : string.Empty)));
            }

            GetMembers(layerMap, result, showAttributes);

            if (layerMap is not (Layers.Type, _, _, _, _, _)) return;

            result.Segments.Add((Layers.TypeEnd, "}\n"));

            foreach (var nestedType in GetNestedTypes())
            {
                result.Segments.Add((Layers.InnerObjects, nestedType.Generate(layers).ToString(layers)));
            }
        }

        private string GetAttributes(IList<CustomAttributeData> attributes, bool showAttributes)
            => showAttributes && attributes.Count > 0
                ? "\t[" + string.Join(", ",
                attributes
                    .Where(a => a.AttributeType != typeof(TypeForwardedFromAttribute))
                    .Select(a =>
                        a.AttributeType.Name.Replace("Attribute", string.Empty) +
                        (a.ConstructorArguments.Count > 0
                            ? "(" +
                              string.Join(", ",
                                  a.ConstructorArguments.Select(b => b.ToString())) +
                              ")"
                            : string.Empty))) + "]\n"
                : string.Empty;

        private void GetMembers((Layers, Layers, Layers, Layers, Layers, Layers) layerMap, PumlClip result,
            bool showAttributes)
        {
            List<string> memberStrings = new();

            var privateFields = ObjectType.GetRuntimeFields().Where(fi => fi.IsPrivate)!;
            var fields = ObjectType.GetRuntimeFields().Where(fi => fi.IsPublic)!;

            if ((layerMap is (_, _, Layers.NonPublic, _, _, _) && privateFields.Any()) ||
                (layerMap is (_, _, _, Layers.Public, _, _) && fields.Any()))
            {

                memberStrings.Add("\t... fields ...");
            }

            if (layerMap is (_, _, Layers.NonPublic, _, _, _))
            {
                foreach (var member in privateFields)
                {
                    memberStrings.AddRange(BuildField(member, showAttributes));
                }
            }

            if (layerMap is (_, _, _, Layers.Public, _, _))
            {
                foreach (var member in fields)
                {
                    memberStrings.AddRange(BuildField(member, showAttributes));
                }
            }

            var privateCtors = BuildConstructors(Layers.NonPublic, showAttributes).ToList()!;
            var ctors = BuildConstructors(Layers.Public, showAttributes).ToList()!;

            if (layerMap is (_, _, Layers.NonPublic, _, _, _) && privateCtors.Any() ||
                layerMap is (_, _, _, Layers.Public, _, _) && ctors.Any())
            {
                memberStrings.Add("\t... constructors ...");
            }

            if (layerMap is (_, _, Layers.NonPublic, _, _, _))
            {
                memberStrings.AddRange(privateCtors);
            }

            if (layerMap is (_, _, _, Layers.Public, _, _))
            {
                memberStrings.AddRange(ctors);
            }

            var privateProperties = ObjectType.GetRuntimeProperties().Where(fi => fi.GetMethod?.IsPrivate ?? true);
            var properties = ObjectType.GetRuntimeProperties().Where(fi => fi.GetMethod?.IsPublic ?? false);

            if (layerMap is (_, _, Layers.NonPublic, _, _, _) && privateProperties.Any() ||
                layerMap is (_, _, _, Layers.Public, _, _) && properties.Any())
            {
                memberStrings.Add("\t... properties ...");
            }

            if (layerMap is (_, _, Layers.NonPublic, _, _, _))
            {
                foreach (var member in privateProperties)
                {
                    memberStrings.AddRange(BuildProperty(member, BindingFlags.NonPublic, showAttributes));
                }
            }

            if (layerMap is (_, _, _, Layers.Public, _, _))
            {
                foreach (var member in properties)
                {
                    memberStrings.AddRange(BuildProperty(member, BindingFlags.Public, showAttributes));
                }
            }

            var privateMethods = ObjectType.GetRuntimeMethods().Where(fi =>
                fi.IsPrivate && !fi.Name.StartsWith("get_", true, CultureInfo.CurrentCulture) &&
                !fi.Name.StartsWith("set_", true, CultureInfo.CurrentCulture));
            var methods = ObjectType.GetRuntimeMethods().Where(fi =>
                fi.IsPublic && !fi.Name.StartsWith("get_", true, CultureInfo.CurrentCulture) &&
                !fi.Name.StartsWith("set_", true, CultureInfo.CurrentCulture));

            if (layerMap is (_, _, Layers.NonPublic, _, _, _) && privateMethods.Any() ||
                layerMap is (_, _, _, Layers.Public, _, _) && methods.Any())
            {
                memberStrings.Add("\t... methods ...");
            }

            if (layerMap is (_, _, Layers.NonPublic, _, _, _))
            {
                foreach (var member in privateMethods)
                {
                    memberStrings.AddRange(BuildMethod(member, showAttributes));
                }
            }

            if (layerMap is (_, _, _, Layers.Public, _, _))
            {
                foreach (var member in methods)
                {
                    memberStrings.AddRange(BuildMethod(member, showAttributes));
                }
            }

            var privateEvents = ObjectType.GetRuntimeEvents().Where(fi => fi.AddMethod?.IsPrivate ?? true);
            var events = ObjectType.GetRuntimeEvents().Where(fi => fi.AddMethod?.IsPublic ?? false);

            if (layerMap is (_, _, Layers.NonPublic, _, _, _) && privateEvents.Any() ||
                layerMap is (_, _, _, Layers.Public, _, _) && events.Any())
            {
                memberStrings.Add("\t... events ...");
            }

            if (layerMap is (_, _, Layers.NonPublic, _, _, _))
            {
                foreach (var member in privateEvents)
                {
                    memberStrings.AddRange(BuildEvent(member, showAttributes));
                }
            }

            if (layerMap is (_, _, _, Layers.Public, _, _))
            {
                foreach (var member in events)
                {
                    memberStrings.AddRange(BuildEvent(member, showAttributes));
                }
            }

            result.Segments.Add((Layers.Members, string.Join("\n", memberStrings)));
        }

        // ReSharper disable once UnusedParameter.Local
        private static void GetNotes((Layers, Layers, Layers, Layers, Layers, Layers) layerMap, PumlClip result)
        {
            if (layerMap is (_, _, _, _, _, Layers.Notes))
            {
            }
        }

        private void GetRelationships((Layers, Layers, Layers, Layers, Layers, Layers) layerMap, PumlClip result)
        {
            if (layerMap is not (_, _, _, _, Layers.Relationships, _)) return;
            if (ObjectType.IsEnum) return;

            var mapped = new List<string>();

            foreach (var field in ObjectType.GetRuntimeFields())
            {
                var IsSpecialName = (field.Attributes & FieldAttributes.SpecialName) == FieldAttributes.SpecialName;
                if (IsSpecialName) continue;
                if (field.Name.EndsWith("_BackingField")) continue;
                if (NormalizeName(field.DeclaringType) != NormalizeName(ObjectType)) continue;
                if (field.FieldType.Name.EndsWith(nameof(EventHandler))) continue;
                if (field.FieldType.Name != NormalizeType(field.FieldType.Name)) continue;

                var objectName = NormalizeName(ObjectType)!;

                var fieldTypeName =
                    field.FieldType.IsGenericType
                        ? NormalizeName(field.FieldType.GetGenericArguments().First())
                        : NormalizeName(field.FieldType);

                var genericCollectionType = field.FieldType.Name.StartsWith("IEnumerable")
                    ? field.FieldType.GetGenericArguments().First()
                    : typeof(object);

                var genericCollectionTypeName = NormalizeName(genericCollectionType);

                var arrayType = GetArrayType(field.FieldType);
                var arrayElementType = NormalizeName(arrayType);

                if (arrayElementType is null)
                {
                    var key = genericCollectionType.Namespace + "." + genericCollectionType.Name;
                    if (mapped.Any(m => m == key)) continue;

                    mapped.Add(key);
                }
                else
                {
                    var key = arrayType.Namespace + "." + arrayType.Name;
                    if (mapped.Any(m => m == key)) continue;

                    mapped.Add(key);
                }

                var relationship = (Layers.Relationships,
                    field.FieldType.IsArray
                        ? $"{objectName.AsSlug()} o- {arrayElementType?.AsSlug()} : {field.Name} << aggregation[] >> "
                        : genericCollectionType != typeof(object)
                            ? $"{objectName.AsSlug()} o- {genericCollectionTypeName?.AsSlug()} : {field.Name} << aggregation >>"
                            : $"{objectName.AsSlug()} -> {fieldTypeName.AsSlug()} : {field.Name} << use >>");

                result.Segments.Add(relationship);
            }

            foreach (var property in ObjectType.GetRuntimeProperties())
            {
                if (NormalizeName(property.DeclaringType) != NormalizeName(ObjectType)) continue;
                if (property.PropertyType.Name.EndsWith(nameof(EventHandler))) continue;
                if (property.PropertyType.Name != NormalizeType(property.PropertyType.Name)) continue;

                var objectName = NormalizeName(ObjectType)!;

                var propertyTypeName = 
                    property.PropertyType.IsGenericType 
                        ? NormalizeName(property.PropertyType.GetGenericArguments().First()) 
                        : NormalizeName(property.PropertyType);

                var genericCollectionType = property.PropertyType.Name.StartsWith("IEnumerable")
                    ? property.PropertyType.GetGenericArguments().First()
                    : typeof(object);

                var genericCollectionTypeName = NormalizeName(genericCollectionType);

                var arrayType = GetArrayType(property.PropertyType);
                var arrayElementType = NormalizeName(arrayType);

                if (arrayElementType is null)
                {
                    var key = genericCollectionType.Namespace + "." + genericCollectionType.Name;
                    if (mapped.Any(m => m == key)) continue;

                    mapped.Add(key);
                }
                else
                {
                    var key = arrayType.Namespace + "." + arrayType.Name;
                    if (mapped.Any(m => m == key)) continue;

                    mapped.Add(key);
                }

                var relationship = (Layers.Relationships,
                    property.PropertyType.IsArray
                        ? $"{objectName.AsSlug()} o- {arrayElementType?.AsSlug()} : {property.Name} << aggregation[] >>"
                        : genericCollectionType != typeof(object)
                            ? $"{objectName.AsSlug()} o- {genericCollectionTypeName?.AsSlug()} : {property.Name} << aggregation >>"
                            : $"{objectName.AsSlug()} -> {propertyTypeName.AsSlug()} : {property.Name} << use >>");

                result.Segments.Add(relationship);
            }
        }

        private Type? GetArrayType(Type arrayType)
        {
            if (!arrayType.IsArray) return null;

            var trimmedName = arrayType.Name.TrimEnd("[]".ToCharArray());
            var types = GetNestedTypes().Where(nt => nt.ObjectType.Name == trimmedName);

            return types.FirstOrDefault()?.ObjectType ?? GetFromAssembly();

            Type? GetFromAssembly()
                => arrayType.Assembly
                    .GetTypes()
                    .FirstOrDefault(t => t.Name == trimmedName) 
                   ?? arrayType;
        }

        private static string NormalizeType(string typeName)
            => typeName.Replace("System.", string.Empty) switch
            {
                "ValueType" => "struct",
                "Void" => "void",
                "Object" => "object",
                "String" => "string",
                "Int16" => "short",
                "UInt16" => "ushort",
                "Int32" => "int",
                "UInt32" => "uint",
                "Int64" => "long",
                "UInt64" => "ulong",
                "Single" => "float",
                "Double" => "double",
                "Byte" => "byte",
                "SByte" => "sbyte",
                "Decimal" => "decimal",
                "Boolean" => "bool",
                "Object[]" => "object[]",
                "String[]" => "string[]",
                "Int16[]" => "short[]",
                "UInt16[]" => "ushort[]",
                "Int32[]" => "int[]",
                "UInt32[]" => "uint[]",
                "Int64[]" => "long[]",
                "UInt64[]" => "ulong[]",
                "Single[]" => "float[]",
                "Double[]" => "double[]",
                "Byte[]" => "byte[]",
                "SByte[]" => "sbyte[]",
                "Decimal[]" => "decimal[]",
                "Boolean[]" => "bool[]",
                _ => typeName.Replace("System.", string.Empty)
            };

        private IEnumerable<string> BuildConstructors(Layers layers, bool showAttributes)
        {
            var bindingFlags = BindingFlags.Instance | layers switch
            {
                Layers.All => BindingFlags.Public | BindingFlags.NonPublic,
                Layers.Members => BindingFlags.Public | BindingFlags.NonPublic,
                Layers.NonPublic => BindingFlags.NonPublic,
                Layers.Public => BindingFlags.Public,
                _ => BindingFlags.Default
            };

            var ctors = ObjectType.GetConstructors(bindingFlags);

            if (!ctors.Any()) yield break;

            foreach (var ctor in ctors)
            {
                if (ctor.DeclaringType != ObjectType) continue;

                var parList = ctor.GetParameters();

                var parms = MakeParList(parList);

                yield return $"{GetAttributes(ctor.GetCustomAttributesData(), showAttributes)}" +
                             $"\t{GetAccessibility(ctor.IsStatic, ctor.IsAbstract, ctor.IsVirtual, ctor.IsPublic, ctor.IsPrivate, ctor.IsFamily, ctor.IsAssembly)}" +
                             $"ctor({parms})\n";
            }
        }

        private IEnumerable<string> BuildMethod(MethodInfo? member, bool showAttributes)
        {
            if (member is null || member.Name.StartsWith("add_") || member.Name.StartsWith("remove_")) yield break;

            IEnumerable<MethodInfo> methods = ObjectType.GetRuntimeMethods().Where(e => e.Name == member.Name);

            if (!methods.Any()) yield break;

            foreach (MethodInfo method in methods)
            {
                if (method.DeclaringType != ObjectType) continue;

                var methodName = method.Name;

                MethodInfo? genericMethod = null;

                if (method.IsGenericMethod)
                {
                    genericMethod = method.GetGenericMethodDefinition();
                }
                else if (method.IsGenericMethodDefinition)
                {
                    genericMethod = method;
                }

                if (genericMethod is not null)
                {
                    var genericTypes = string.Join(", ",
                        genericMethod.GetGenericArguments().Select(NormalizeName));
                    genericTypes = $"<{genericTypes}>";

                    methodName += genericTypes;
                }

                var parList = method.GetParameters();
                var parms = MakeParList(parList);

                yield return
                    $"{GetAttributes(method.GetCustomAttributesData(), showAttributes)}" +
                    $"\t{GetAccessibility(method.IsStatic, method.IsAbstract, method.IsVirtual, method.IsPublic, method.IsPrivate, method.IsFamily, method.IsAssembly)}" +
                    $"{NormalizeNameString(methodName)}({parms})" +
                    $": {NormalizeName(method.ReturnType)}\n";
            }
        }

        private IEnumerable<string> BuildField(FieldInfo? field, bool showAttributes)
        {
            if (field is null) yield break;

            if (field.FieldType.Name.EndsWith(nameof(EventHandler))) yield break;

            if (field.DeclaringType != ObjectType) yield break;

            if (field.Name.EndsWith("_BackingField")) yield break;

            var IsSpecialName = (field.Attributes & FieldAttributes.SpecialName) == FieldAttributes.SpecialName;
            if (IsSpecialName) yield break;

            var isPublic = field.IsPublic;
            var isPrivate = field.IsPrivate;
            var isFamily = field.IsFamily;

            var attributes = GetAttributes(field.FieldType.GetCustomAttributesData(), showAttributes);

            yield return
                $"{attributes}"+
                $"\t{GetAccessibility(field.IsStatic, false, false, isPublic, isPrivate, isFamily, field.IsAssembly)}" +
                $"{NormalizeNameString(field.Name)}" +
                $": {NormalizeName(GetArrayType(field.FieldType) ?? field.FieldType)}\n";
        }

        public string? NormalizeName(Type? type)
        {
            if (type is null) return null;

            var typeName = GetGenericName(type);

            return NormalizeNameString(typeName);
        }

        private string GetGenericName(Type genericType)
        {
            var typeName = genericType.FullName ?? genericType.Name;

            if (!typeName.Contains('`')) return NormalizeNameString(typeName);

            if (genericType.IsArray)
            {

            }

            if (!genericType.IsGenericType && !genericType.IsGenericTypeDefinition)
                return NormalizeNameString(typeName);

            string genericTypes = string.Join(", ", GetGenericsArguments());

            genericTypes = $"<{genericTypes}>";

            if (genericType.IsNested)
            {
                var parentName = NormalizeName(genericType.ReflectedType!);
                typeName = $"{parentName}.{genericType.Name}";
            }

            var result = $"{typeName[..typeName.IndexOf("`", StringComparison.Ordinal)]}{genericTypes}";

            return result;

            IEnumerable<string> GetGenericsArguments()
            {
                var reflectedTypes = genericType.ReflectedType?.IsGenericType ?? false
                    ? genericType.ReflectedType.GetGenericArguments()
                    : Array.Empty<Type>();

                var types = new List<Type>(reflectedTypes);

                types.AddRange(genericType.GetGenericArguments().Skip(types.Count));

                return types.Select(NormalizeName);
            }
        }

        private IEnumerable<string> BuildProperty(PropertyInfo? member, BindingFlags bindingFlags, bool showAttributes)
        {
            if (member is null) yield break;

            IEnumerable<PropertyInfo> properties = ObjectType.GetRuntimeProperties()
                .Where(e => e.Name == member.Name);

            if (!properties.Any()) yield break;

            foreach (var property in properties)
            {
                if (property.DeclaringType != ObjectType) continue;

                var parList = property.GetIndexParameters();
                var parms = MakeParList(parList);

                var indexerParameters = string.IsNullOrWhiteSpace(parms)
                    ? string.Empty
                    : $"[{parms}]";

                var accessors = property.GetAccessors(true);
                var acc = accessors
                    .Select(accessor =>
                        $"{GetAccessibility(accessor.IsStatic, accessor.IsAbstract, accessor.IsVirtual, accessor.IsPublic, accessor.IsPrivate, accessor.IsFamily, false)}{NormalizeNameString(accessor.Name)}{indexerParameters}")
                    .ToList();

                var isPublic = (bindingFlags | BindingFlags.Public) == BindingFlags.Public;
                var isPrivate = (bindingFlags | BindingFlags.NonPublic) == BindingFlags.NonPublic;
                var isFamily = (property.GetMethod?.IsFamily ?? true) && !(property.SetMethod?.IsPublic ?? false);

                var attributes = GetAttributes(property.PropertyType.GetCustomAttributesData(), showAttributes);

                yield return
                    $"{attributes}" +
                    $"\t{GetAccessibility(property.GetMethod.IsStatic, property.GetMethod.IsAbstract, property.GetMethod.IsVirtual, isPublic, isPrivate, isFamily, property.GetMethod?.IsAssembly ?? false)}" +
                    $"{NormalizeNameString(property.Name)} " +
                    $"({string.Join(" ", acc)}) " +
                    $": {NormalizeName(GetArrayType(property.PropertyType) ?? property.PropertyType)} << property >>\n";
            }
        }

        private string GetAccessibility(bool isStatic, bool isAbstract, bool isVirtual, bool isPublic, bool isPrivate, bool isFamily, bool isAssembly)
        {
            var accessibility = GetAccessibility(isPublic, isPrivate, isFamily, isAssembly);
            var modifiers = isStatic ? "{static} " : "";
            modifiers += isAbstract ? "{abstract} " : "";
            //modifiers += isVirtual ? "{virtual} " : "";
            return $"{modifiers}{accessibility}";
        }

        private static string GetAccessibility(bool methodIsPublic, bool methodIsPrivate, bool methodIsFamily, bool isAssembly)
        {
            return methodIsPublic
                ? "+"
                : methodIsFamily
                    ? "#"
                    : methodIsPrivate
                        ? "-"
                        : isAssembly
                            ? "~"
                            : "";
        }

        private IEnumerable<string> BuildEvent(EventInfo? member, bool showAttributes)
        {
            if (member is null) yield break;

            var events = ObjectType.GetRuntimeEvents().Where(e => e.Name == member.Name);

            if (!events.Any()) yield break;

            foreach (var evt in events)
            {
                if (evt.DeclaringType != ObjectType) continue;

                var delegateType = evt.EventHandlerType!;
                var method = delegateType.GetMethod("Invoke")!;
                var parList = method.GetParameters();
                var parms = MakeParList(parList);

                yield return
                    $"{GetAttributes(evt.GetCustomAttributesData(), showAttributes)}" +
                    $"\t{GetAccessibility(method.IsStatic, method.IsAbstract, method.IsVirtual, method.IsPublic, method.IsPrivate, method.IsFamily, method.IsAssembly)}" +
                    $"{NormalizeNameString(evt.Name)}({parms}) : {NormalizeName(method.ReturnType)} << event >>\n";
            }
        }

        private static string NormalizeNameString(string? name)
        {
            if (name is null) return "<<No Name>>";

            name = NormalizeType(name);

            if (name.StartsWith("get_") || name.StartsWith("set_") || name.StartsWith("init_"))
            {
                name = name[..name.IndexOf("_", StringComparison.Ordinal)] + ";";
            }

            Regex regex = new(@"`[1-9]\[\[([^,\s]*).*\]\]");

            if (regex.IsMatch(name))
            {
                var match = regex.Match(name);
                var result = $"<{match.Groups.Values.LastOrDefault()?.Value}>";
                name = regex.Replace(name, result);
            }
            else
            {
                regex = new(@"`[1-9]");
                if (regex.IsMatch(name))
                {
                    var match = regex.Match(name);
                    name = regex.Replace(name, string.Empty);
                }
            }

            return name;
        }

        private string MakeParList(ParameterInfo[] parameters)
        {
            var p = parameters
                .Select(par => ($"{NormalizeNameString(par.Name!)}" +
                               $": {NormalizeName(par.ParameterType)} " +
                               $"{(par.HasDefaultValue ? $" = {par.DefaultValue}" : "")}").Trim())
                .ToArray();

            return string.Join(", ", p).Trim();
        }
    }

    public static class PumlProcessingExtensions
    {
        private static readonly Regex Regex = new Regex(@"[^._0-9a-zA-Z]");

        public static string AsSlug(this string text) => Regex.Replace(text, "_");
    }
}
