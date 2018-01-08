// **************************************************************************
// File:      DynamicClass.cs
// Solution:  Hearthstone Deck Tracker
// Date:      12/23/2017
// Author:    Latency McLaughlin
// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using PropertyAttributes = System.Reflection.PropertyAttributes;
using PropertyType = System.Action<System.Reflection.MethodInfo, HearthMirror.TypeBuilder.BuilderData>;


namespace HearthMirror.TypeBuilder {
  /// <summary>
  ///   DynamicObject
  /// </summary>
  internal static class DynamicClass {
    /// <summary>
    ///   All of the types generated for each interface.  This dictionary is indexed by the interface's type object
    /// </summary>
    private static readonly Dictionary<Type, Type> TypeCollection = new Dictionary<Type, Type>();

    /// <summary>
    ///   The module builder used for all types constructed
    /// </summary>
    internal static readonly ModuleBuilder ModuleBuilder;


    /// <summary>
    ///   DynamicObject
    /// </summary>
    static DynamicClass() {
      // Singletons
      var assemblyName = new AssemblyName {
        // ReSharper disable once PossibleNullReferenceException
        Name = MethodBase.GetCurrentMethod().DeclaringType.Name
      };
      var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
      ModuleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name + "-Module");
    }


    /// <summary>
    ///   Returns an object that implement an interface, without the need to manually create a type that implements the
    ///   interface.
    /// </summary>
    /// <typeparam name="T">T must be an interface that is public.</typeparam>
    /// <param name="kvp"></param>
    /// <param name="collection"></param>
    /// <returns>An object that implements the T interface</returns>
    /// <exception cref="TargetException">This member was invoked with a late-binding mechanism.</exception>
    internal static T New<T>(KeyValuePair<string, string> kvp, Dictionary<string, Type> collection = null) where T : class {
      var type = typeof(T);
      if (!type.IsInterface)
        throw new NotSupportedException("Template parameter must be an interface.");

      // If the type that implements the isn't created, create it
      if (!TypeCollection.ContainsKey(type))
        TypeCollection[type] = CreateType(type, collection);

      // Now that the type exists to implement the interface, use the Activator to create an object
      return (T) Activator.CreateInstance(TypeCollection[type], kvp.Key, kvp.Value, collection);
    }


    /// <summary>
    ///   New
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="kvp"></param>
    /// <param name="collection"></param>
    /// <returns></returns>
    internal static T New<T>(KeyValuePair<string, string> kvp, IEnumerable<MyPropertyInfo> collection) where T : class {
      return New<T>(kvp, collection?.ToDictionary(f => f.Name, f => f.PropertyType));
    }


    /// <summary>
    ///   CreateType
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <param name="properties"></param>
    /// <param name="accessor"></param>
    /// <param name="hasSubTypes"></param>
    /// <returns>The newly created type of the object.</returns>
    internal static Type CreateType(this Mirror obj, string name, IEnumerable<string> properties, string accessor = "", bool hasSubTypes = false) {
      Type subTypeRef = null;

      // Tested Regex @ http://regex101.com
      const string subTypes = @"(?:<|(?:\$))([a-zA-Z_]+[0-9`]*)(?:>([a-zA-Z_]+[0-9`]*))";
      var match = Regex.Match(name, subTypes);

      if (match.Success) {
        var refType = match.Groups[1].Value; // Class reference type.
        if (match.Groups[2].Success && !string.IsNullOrEmpty(match.Groups[2].Value))
          accessor = match.Groups[2].Value; // Class accessor.

        // ReSharper disable once TailRecursiveCall
        var enumerable = properties as IList<string> ?? properties.ToList();
        subTypeRef = CreateType(obj, refType, enumerable, accessor, true);

        // Tokenize this for the actual derived class name.
        name = name.Substring(0, name.IndexOf('+'));
      }

      // Check if formating of the class name matches traditional valid syntax.
      // Assume at least 3 levels deep.
      var toks = name.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
      Type type = null;

      foreach (var tok in toks.Reverse()) {
        var o = obj.RefTypes.FirstOrDefault(t => t.Value.Name == tok);
        if (!o.Equals(default(KeyValuePair<string, Type>)))
          continue;

        // Not exists.
        var sb = new StringBuilder();
        sb.Append(@"using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;

namespace HearthMirror.TypeBuilder {
  [CompilerGenerated]
  public class ").Append(tok.Replace('.', '_')).AppendLine(@" {");

        if (subTypeRef != null)
          sb.AppendLine($"	public {subTypeRef.Name} {accessor}").AppendLine(" { get; set; }");

        sb.Append("  }\n}");

        var asm = RuntimeCodeCompiler.CompileCode(sb.ToString());
        if (asm == null)
          return type;

        type = asm.GetType($"{MethodBase.GetCurrentMethod().ReflectedType?.Namespace}.{tok}"); // => generated

        // Register our type for reference.   This container will handle collisions and throw if encountered.
        obj.RefTypes.Add(tok, type);
      }

      return type;
    }


    /// <summary>
    ///   AddConstructor
    /// </summary>
    /// <param name="typeBuilder"></param>
    /// <param name="backingFields"></param>
    private static void CreateConstructor(System.Reflection.Emit.TypeBuilder typeBuilder, IReadOnlyList<dynamic> backingFields) {
      #region Constructor

      //--------------------------------------------------------------------

      // Constructor:  (Default)
      // <code>
      //  .custom instance void [Newtonsoft.Json]Newtonsoft.Json.JsonConverterAttribute::.ctor(class [mscorlib]System.Type) = { type(<NameSpace>.PayloadComplexTypeConverter) }
      //   .method public hidebysig specialname rtspecialname instance void .ctor() cil managed
      //  {
      //  }
      // </code>

      // Attribute:
      // [JsonConverter(typeof(PayloadComplexTypeConverter))]
      // Ctor attributes

      /* Uncomment if needing similar functionality for nested duck types */
      // Attribute:
      // [JsonConverterAttribute()]
      //var jsonConverterAttr = typeof(JsonConverterAttribute).GetConstructor(new [] { typeof(Type) });
      //if (jsonConverterAttr != null) {
      //  // ReSharper disable once InconsistentNaming
      //  var jsonConverterCABuilder = new CustomAttributeBuilder(
      //    jsonConverterAttr,
      //    new object[] { typeof(PayloadComplexTypeConverter) }
      //  );
      //  typeBuilder.SetCustomAttribute(jsonConverterCABuilder);
      //}

      var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new[] { typeof(string), typeof(string), typeof(Dictionary<string, Type>) });
      var ctorIl = constructorBuilder.GetILGenerator();

      // Now, we'll load the current instance ref in arg 0, along
      // with the value of parameter "x" stored in arg X, into stfld.

      for (var x = 0; x < backingFields.Count; x++) {
        ctorIl.Emit(OpCodes.Ldarg_0);
        ctorIl.Emit(OpCodes.Ldarg_S, x + 1);
        ctorIl.Emit(OpCodes.Stfld, backingFields[x]);
      }

      // Our work complete, we return.

      ctorIl.Emit(OpCodes.Ret);

      //--------------------------------------------------------------------

      #endregion Constructor
    }


    /// <summary>
    ///   Creates a method that will generate an object that implements the interface for the given type.
    /// </summary>
    /// <remarks>
    ///   Creates a custom POCO entity type with auto-properties and serialized attributes.
    /// </remarks>
    /// <param name="type"></param>
    /// <param name="collection"></param>
    private static Type CreateType(Type type, Dictionary<string, Type> collection = null) {
      var typeBuilder = ModuleBuilder.DefineType(type.Name.Substring(1), TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Serializable | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout, typeof(object) // IL: hasSubTypes
      );

      // Make sure that the type is an interface    
      if (type.IsInterface)
        typeBuilder.AddInterfaceImplementation(type);

	  // Get a list of all methods, including methods in inherited interfaces
	  // The methods that aren't accessors and will need default implementations...  However,
	  // a property's accessors are also methods!
	  var methods = new List<MethodInfo>();
      AddMethodsToList(methods, type);

      if (collection != null)
        foreach (var pi in collection.Select(item => new MyPropertyInfo(item.Key, item.Value)))
          pi.CreateProperty(methods, typeBuilder, false);

      // Get a list of all of the properties, including properties in inherited interfaces
      var properties = new List<PropertyInfo>();
      AddPropertiesToList(properties, type);

      // Create accessors for each property.
      // Return the field methods so that we can bind them in the constructor as 'read-only' backing fields.
      var backingFields = properties.Select(pi => pi.CreateProperty(methods, typeBuilder, true)).Cast<dynamic>().ToList();

      // Create default methods.  These methods will essentially be no-ops; if there is a 
      // return value, they will either return a default value or null
      foreach (var methodInfo in methods)
        methodInfo.CreateMethod(typeBuilder);

      // Create Constructor
      CreateConstructor(typeBuilder, backingFields);

      // Finally, after all the fields and methods are generated, create the type for use at run-time
      return typeBuilder.CreateTypeInfo().AsType();
    }


    /// <summary>
    ///   Helper method to get all MethodInfo objects from an interface.  This recurses to all sub-interfaces
    /// </summary>
    /// <param name="methods"></param>
    /// <param name="type"></param>
    private static void AddMethodsToList(List<MethodInfo> methods, Type type) {
      methods.AddRange(type.GetMethods());
      foreach (var subInterface in type.GetInterfaces())
        AddMethodsToList(methods, subInterface);
    }


    /// <summary>
    ///   Helper method to get all PropertyInfo objects from an interface.  This recurses to all
    ///   sub-interfaces
    /// </summary>
    /// <param name="properties"></param>
    /// <param name="type"></param>
    private static void AddPropertiesToList(List<PropertyInfo> properties, Type type) {
      properties.AddRange(type.GetProperties());
      foreach (var subInterface in type.GetInterfaces())
        AddPropertiesToList(properties, subInterface);
    }


    /// <summary>
    ///   CreateProperty
    /// </summary>
    /// <param name="pi"></param>
    /// <param name="methods"></param>
    /// <param name="typeBuilder"></param>
    /// <param name="privateSetters"></param>
    private static FieldBuilder CreateProperty(this PropertyInfo pi, ICollection<MethodInfo> methods, System.Reflection.Emit.TypeBuilder typeBuilder, bool privateSetters) {
      // Create underlying field; all properties have a field of the same type
      var backingField = typeBuilder.DefineField($"<{pi.Name}>k_BackingField", pi.PropertyType, FieldAttributes.Private);

      // Attribute:
      // [CompilerGenerated]
      var compilerGeneratedAttr = typeof(CompilerGeneratedAttribute).GetConstructor(Type.EmptyTypes);
      if (compilerGeneratedAttr != null) {
        // ReSharper disable once InconsistentNaming
        var fieldCABuilder = new CustomAttributeBuilder(compilerGeneratedAttr, new object[] { });
        backingField.SetCustomAttribute(fieldCABuilder);
      }

      // The last argument of DefineProperty is null, because the 
      // property has no parameters. (If you don't specify null, you must 
      // specify an array of Type objects. For a parameterless property, 
      // use an array with no elements: new Type[] {})
      var propBldr = typeBuilder.DefineProperty(pi.Name, PropertyAttributes.HasDefault, CallingConventions.HasThis, pi.PropertyType, Type.EmptyTypes);

      var container = new BuilderData { _typeBuilder = typeBuilder, _fieldBuilder = backingField, _propertyBuilder = propBldr };

      // Disregard exceptions so that the custom defined properties can be used with the same algorithms even though their backing methods have not yet been defined.
      MethodInfo getter = null, setter = null;
      try {
        getter = pi.GetGetMethod();
        setter = pi.GetSetMethod();
      } catch (Exception) {
        // ignored
      }

      var jmpTable = new Dictionary<Instr, KeyValuePair<MethodInfo, PropertyType>> { { Instr.Getter, new KeyValuePair<MethodInfo, PropertyType>(getter, GetProperty) }, { Instr.Setter, new KeyValuePair<MethodInfo, PropertyType>(setter, SetProperty) } };

      foreach (var z in jmpTable) {
        if (z.Key == Instr.Setter && privateSetters)
          container._methodAttributes = MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;

        // If there is a getter in the interface, create a getter in the new type
        if (pi.ReflectedType != null)
          if (z.Value.Key != null)
            methods.Remove(z.Value.Key);
        z.Value.Value(z.Value.Key, container);
      }

      return backingField;
    }


    /// <summary>
    ///   GetProperty
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <param name="builderData"></param>
    private static void GetProperty(this MethodInfo methodInfo, BuilderData builderData) {
      // Define the "get" accessor method for CustomerName.
      var methodBuilder = builderData._typeBuilder.DefineMethod("get_" + builderData._propertyBuilder.Name, builderData._methodAttributes, builderData._propertyBuilder.PropertyType, Type.EmptyTypes);

      // ReSharper disable once InconsistentNaming
      var IL = methodBuilder.GetILGenerator();

      IL.Emit(OpCodes.Ldarg_0);
      IL.Emit(OpCodes.Ldfld, builderData._fieldBuilder);
      IL.Emit(OpCodes.Ret);

      // Map to the PropertyBuilder. 
      builderData._propertyBuilder.SetGetMethod(methodBuilder);

      // We need to associate our new type's method with the setter method in the interface
      if (methodInfo != null)
        builderData._typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
    }


    /// <summary>
    ///   SetProperty
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <param name="builderData"></param>
    private static void SetProperty(this MethodInfo methodInfo, BuilderData builderData) {
      // Define the "set" accessor method for CustomerName.
      var methodBuilder = builderData._typeBuilder.DefineMethod("set_" + builderData._propertyBuilder.Name, builderData._methodAttributes, typeof(void), new[] { builderData._propertyBuilder.PropertyType });

      // ReSharper disable once InconsistentNaming
      var IL = methodBuilder.GetILGenerator();

      IL.Emit(OpCodes.Ldarg_0);
      IL.Emit(OpCodes.Ldarg_1);
      IL.Emit(OpCodes.Stfld, builderData._fieldBuilder);
      IL.Emit(OpCodes.Ret);

      // Map to the PropertyBuilder. 
      builderData._propertyBuilder.SetSetMethod(methodBuilder);

      // We need to associate our new type's method with the setter method in the interface
      if (methodInfo != null)
        builderData._typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
    }


    /// <summary>
    ///   CreateMethod
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <param name="typeBuilder"></param>
    private static void CreateMethod(this MethodInfo methodInfo, System.Reflection.Emit.TypeBuilder typeBuilder) {
      // Define the method
      var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, methodInfo.ReturnType, methodInfo.GetParameters().Select(parameterInfo => parameterInfo.ParameterType).ToArray());

      // The ILGenerator class is used to put op-codes (similar to assembly) into the method
      // ReSharper disable once InconsistentNaming
      var IL = methodBuilder.GetILGenerator();

      // If there's a return type, create a default value or null to return
      if (methodInfo.ReturnType != typeof(void)) {
        var localBuilder = IL.DeclareLocal(methodInfo.ReturnType); // this declares the local object, 
        // int, long, float, ect
        IL.Emit(OpCodes.Ldloc, localBuilder); // load the value on the stack to return
      }

      IL.Emit(OpCodes.Ret); // return

      // We need to associate our new type's method with the setter method in the interface
      typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
    }

    private enum Instr {
      Getter,
      Setter
    }


    #region MyPropetyInfo

    // --------------------------------------------------------------------

    public class MyPropertyInfo : PropertyInfo {
      private readonly Type _declaringType;
      private readonly Type _propertyType;

      private readonly Type _reflectedType;

      // Essentially this would be a KVP, but the code would require converstion methods and conditionals
      // to distinguish the different use scenarios.  This basically creates a fake PropertyInfo type which can
      // then be instanciated and and filled with the necessary information needed without using reflection to
      // set properties on protected properties within an abstract class that is unable to be instanciated.
      public MyPropertyInfo(string name, Type type) {
        Name = name;
        _propertyType = type;
        _declaringType = GetType();
        _reflectedType = null; // RESERVED
      }

      public override string Name { get; }

      public override object[] GetCustomAttributes(bool inherit) {
        throw new NotImplementedException();
      }

      public override bool IsDefined(Type attributeType, bool inherit) {
        throw new NotImplementedException();
      }

      public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
        throw new NotImplementedException();
      }

      public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
        throw new NotImplementedException();
      }

      public override MethodInfo[] GetAccessors(bool nonPublic) {
        throw new NotImplementedException();
      }

      public override MethodInfo GetGetMethod(bool nonPublic) {
        throw new NotImplementedException();
      }

      public override MethodInfo GetSetMethod(bool nonPublic) {
        throw new NotImplementedException();
      }

      public override ParameterInfo[] GetIndexParameters() {
        throw new NotImplementedException();
      }

      public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
        throw new NotImplementedException();
      }

      public override string ToString() {
        return '"' + $"{Name}\":\"{PropertyType}" + '"';
      }
#pragma warning disable 649
      private PropertyAttributes _attributes;
      private bool _canRead;
      private bool _canWrite;
#pragma warning restore 649
      // ReSharper disable ConvertToAutoProperty
      public override Type DeclaringType => _declaringType;
      public override Type ReflectedType => _reflectedType;
      public override Type PropertyType => _propertyType;
      public override PropertyAttributes Attributes => _attributes;
      public override bool CanRead => _canRead;

      public override bool CanWrite => _canWrite;
      // ReSharper restore ConvertToAutoProperty
    }

    // --------------------------------------------------------------------

    #endregion MyPropertyInfo
  }
}
