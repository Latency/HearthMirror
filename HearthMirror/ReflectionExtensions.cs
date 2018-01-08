// **************************************************************************
// File:      ReflectionExtensions.cs
// Solution:  Hearthstone Deck Tracker
// Date:      12/22/2017
// Author:    Latency McLaughlin
// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace HearthMirror {
  /// <summary>
  ///   Extensions methos for using reflection to get / set member values
  /// </summary>
  internal static class ReflectionExtensions {
    /// <summary>
    ///   Gets the public or private member using reflection.
    /// </summary>
    /// <param name="obj">The source target.</param>
    /// <param name="memberName">Name of the field or property.</param>
    /// <returns>the value of member</returns>
    public static object GetMemberValue(this object obj, string memberName) {
      var memInf = GetMemberInfo(obj, memberName);

      switch (memInf) {
        case null:
          throw new NullReferenceException($"'{memberName}' does not exist or can not be found.");
        case PropertyInfo _:
          return memInf.As<PropertyInfo>().GetValue(obj, null);
        case FieldInfo _:
          return memInf.As<FieldInfo>().GetValue(obj);
      }
      throw new Exception();
    }


    /// <summary>
    ///   Gets the public or private member using reflection.
    /// </summary>
    /// <param name="obj">The target object.</param>
    /// <param name="memberName">Name of the field or property.</param>
    /// <param name="newValue"></param>
    /// <returns>Old Value</returns>
    public static object SetMemberValue(this object obj, string memberName, object newValue) {
      var memInf = GetMemberInfo(obj, memberName);


      if (memInf == null)
        throw new Exception("memberName");

      var oldValue = obj.GetMemberValue(memberName);

      switch (memInf) {
        case PropertyInfo _:
          memInf.As<PropertyInfo>().SetValue(obj, newValue, null);
          break;
        case FieldInfo _:
          memInf.As<FieldInfo>().SetValue(obj, newValue);
          break;
        default:
          throw new Exception();
      }

      return oldValue;
    }


	/// <summary>
	///   Gets the member info
	/// </summary>
	/// <param name="obj">source object</param>
	/// <param name="memberName">name of member</param>
	/// <returns>instanse of MemberInfo corresponsing to member</returns>
	private static MemberInfo GetMemberInfo(object obj, string memberName) {
      var prps = new List<PropertyInfo> {
        obj.GetType().GetProperty(memberName,
          BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
          BindingFlags.FlattenHierarchy)
      };

      prps = prps.Where(i => !ReferenceEquals(i, null)).ToList();
      if (prps.Count != 0)
        return prps[0];

      var flds = new List<FieldInfo> {
        obj.GetType().GetField(memberName,
          BindingFlags.NonPublic | BindingFlags.Instance |
          BindingFlags.FlattenHierarchy)
      };


      //to add more types of properties

      flds = flds.Where(i => !ReferenceEquals(i, null)).ToList();

      return flds.Count != 0 ? flds[0] : null;
    }

    [DebuggerHidden]
    private static T As<T>(this object obj) {
      return (T) obj;
    }
  }
}
