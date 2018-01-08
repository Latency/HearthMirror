//  *****************************************************************************
//  File:       BuilderData.cs
//  Solution:   Hearthstone.Deck.Tracker
//  Project:    HearthMirror
//  Date:       01/01/2018
//  Author:     Latency McLaughlin
//  *****************************************************************************

using System.Reflection;
using System.Reflection.Emit;

// ReSharper disable InconsistentNaming

namespace HearthMirror.TypeBuilder {
  internal class BuilderData {
    public BuilderData() {
      _methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;
    }

    public FieldBuilder _fieldBuilder { get; set; }
    public MethodAttributes _methodAttributes { get; set; }
    public PropertyBuilder _propertyBuilder { get; set; }
    public System.Reflection.Emit.TypeBuilder _typeBuilder { get; set; }
  }
}
