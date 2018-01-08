// **************************************************************************
// File:      RunTimeCodeCompiler.cs
// Solution:  Hearthstone Deck Tracker
// Date:      12/23/2017
// Author:    Latency McLaughlin
// ***************************************************************************

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace HearthMirror.TypeBuilder {
  /// <summary>
  ///  RuntimeCodeCompiler
  /// </summary>
  internal static class RuntimeCodeCompiler {
    private static volatile Dictionary<string, Assembly> _cache = new Dictionary<string, Assembly>();
    private static readonly object SyncRoot = new object();
    private static readonly Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();

    static RuntimeCodeCompiler() {
      AppDomain.CurrentDomain.AssemblyLoad += (sender, e) => { Assemblies[e.LoadedAssembly.FullName] = e.LoadedAssembly; };
      AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => {
        Assemblies.TryGetValue(e.Name, out var assembly);
        return assembly;
      };
    }


	/// <summary>
	///  CompileCode
	/// </summary>
	/// <param name="code"></param>
	/// <returns></returns>
	public static Assembly CompileCode(string code) {
      var provider = new CSharpCodeProvider();
      var compilerparams = new CompilerParameters { GenerateExecutable = false, GenerateInMemory = true, IncludeDebugInformation = true };

	  foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
	    if (assembly.FullName.StartsWith("Microsoft.GeneratedCode") || assembly.FullName.StartsWith("Anonymously Hosted"))
		  continue;
	    try {
	      var location = assembly.Location;
	      if (!string.IsNullOrEmpty(location))
	        compilerparams.ReferencedAssemblies.Add(location);
	    } catch (NotSupportedException) {
	      // this happens for dynamic assemblies, so just ignore it.
	      return null;
	    }
	  }

	  var results = provider.CompileAssemblyFromSource(compilerparams, code);
      if (results.Errors.HasErrors) {
        var errors = new StringBuilder("Compiler Errors :\r\n");
        foreach (CompilerError error in results.Errors)
          errors.AppendFormat("Line {0},{1}\t: {2}\n", error.Line, error.Column, error.ErrorText);
        throw new Exception(errors.ToString());
      }

      AppDomain.CurrentDomain.Load(results.CompiledAssembly.GetName());
      return results.CompiledAssembly;
    }


	/// <summary>
	///  CompileCodeOrGetFromCache
	/// </summary>
	/// <param name="code"></param>
	/// <param name="key"></param>
	/// <returns></returns>
	public static Assembly CompileCodeOrGetFromCache(string code, string key) {
      var exists = _cache.ContainsKey(key);

      if (!exists)
        lock (SyncRoot) {
          exists = _cache.ContainsKey(key);

          if (!exists)
            _cache.Add(key, CompileCode(code));
        }

      return _cache[key];
    }
  }
}
