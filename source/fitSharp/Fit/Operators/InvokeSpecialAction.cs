// Copyright © 2016 Syterra Software Inc. All rights reserved.
// The use and distribution terms for this software are covered by the Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file license.txt at the root of this distribution. By using this software in any fashion, you are agreeing
// to be bound by the terms of this license. You must not remove this notice, or any other, from this software.

using System;
using fitSharp.Fit.Engine;
using fitSharp.Fit.Fixtures;
using fitSharp.Machine.Engine;
using fitSharp.Machine.Exception;
using fitSharp.Machine.Model;

namespace fitSharp.Fit.Operators {
    public class InvokeSpecialAction: CellOperator, InvokeSpecialOperator {

        public bool CanInvokeSpecial(TypedValue instance, MemberName memberName, Tree<Cell> parameters) {
            return true;
        }

        public TypedValue InvokeSpecial(TypedValue instance, MemberName memberName, Tree<Cell> parameters) {
            var cell = parameters.Branches[0];
            Type type;
            try {
                type = Processor.ApplicationUnderTest.FindType("fit.Parse").Type;
            }
            catch (TypeMissingException) {
                return TypedValue.MakeInvalid(new MemberMissingException(instance.Type, memberName.Name, 1));
            }

            // lookup Fixture
            foreach (var member in FindMember(instance.Value, memberName, type).Value)
            {
                cell.Value.SetAttribute(CellAttribute.Syntax, CellAttributeValue.SyntaxKeyword);
                return member.Invoke(new object[] { cell.Value });
            }

            // lookup FlowKeywords
            var runtimeType = Processor.ApplicationUnderTest.FindType("fit.Fixtures.FlowKeywords");
            var runtimeMember = runtimeType.GetConstructor(2);
            var flowKeywords = runtimeMember.Invoke(new[] {instance.Value, Processor});

            foreach (var member in FindMember(flowKeywords.Value, memberName, type).Value) {
                cell.Value.SetAttribute(CellAttribute.Syntax, CellAttributeValue.SyntaxKeyword);
                return member.Invoke(new object[] {cell.Value});
            }

            return TypedValue.MakeInvalid(new MemberMissingException(instance.Type, memberName.Name, 1));
        }

        static Maybe<RuntimeMember> FindMember(object instance, MemberName memberName, Type type) {
            return MemberQuery.FindDirectInstance(instance,
                    new MemberSpecification(memberName).WithParameterTypes(new[] {type}));
        }
    }
}
