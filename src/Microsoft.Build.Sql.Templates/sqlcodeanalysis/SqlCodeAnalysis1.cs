using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Sample.SqlCodeAnalysis1 {
    /// <summary>
    /// This is a rule that returns a warning message
    /// whenever there is a WAITFOR DELAY statement appears inside a subroutine body.
    /// This rule only applies to stored procedures, functions and triggers.
    /// </summary>
    [ExportCodeAnalysisRule(
        id: RuleId,
        displayName: RuleName,
        Description = ProblemDescription,
        Category = RuleCategory,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidWaitForDelayRule : SqlCodeAnalysisRule
    {
        /// <summary>
        /// The Rule ID should resemble a fully-qualified class name. In the Visual Studio UI
        /// rules are grouped by "Namespace + Category", and each rule is shown using "Short ID: DisplayName".
        /// For this rule, that means the grouping will be "Public.Dac.Samples.Performance", with the rule
        /// shown as "SR1004: Avoid using WaitFor Delay statements in stored procedures, functions and triggers."
        /// </summary>
        public const string RuleId = "Sample.SqlCodeAnalysis1.SSCA1004";
        public const string RuleName = "Avoid using WaitFor Delay statements in stored procedures, functions and triggers.";
        public const string ProblemDescription = "Avoid using WAITFOR DELAY in {0}";
        public const string RuleCategory = "Performance";

        public AvoidWaitForDelayRule()
        {
            // This rule supports Procedures, Functions and Triggers. Only those objects will be passed to the Analyze method
            SupportedElementTypes = new[]
            {
                // Note: can use the ModelSchema definitions, or access the TypeClass for any of these types
                ModelSchema.ExtendedProcedure,
                ModelSchema.Procedure,
                ModelSchema.TableValuedFunction,
                ModelSchema.ScalarFunction,
                ModelSchema.DatabaseDdlTrigger,
                ModelSchema.DmlTrigger,
                ModelSchema.ServerDdlTrigger,
            };
        }

        /// <summary>
        /// For element-scoped rules the Analyze method is executed once for every matching
        /// object in the model.
        /// </summary>
        /// <param name="ruleExecutionContext">The context object contains the TSqlObject being
        /// analyzed, a TSqlFragment
        /// that's the AST representation of the object, the current rule's descriptor, and a
        /// reference to the model being
        /// analyzed.
        /// </param>
        /// <returns>A list of problems should be returned. These will be displayed in the Visual
        /// Studio error list</returns>
        public override IList<SqlRuleProblem> Analyze(
            SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var modelElement = ruleExecutionContext.ModelElement;

            // this rule does not apply to inline table-valued function
            // we simply do not return any problem in that case.
            if (IsInlineTableValuedFunction(modelElement))
            {
                return problems;
            }

            var elementName = GetElementName(ruleExecutionContext, modelElement);

            // The rule execution context has all the objects we'll need, including the
            // fragment representing the object,
            // and a descriptor that lets us access rule metadata
            var fragment = ruleExecutionContext.ScriptFragment;
            var ruleDescriptor = ruleExecutionContext.RuleDescriptor;

            // To process the fragment and identify WAITFOR DELAY statements we will use a
            // visitor
            var visitor = new WaitForDelayVisitor();
            fragment.Accept(visitor);
            var waitforDelayStatements = visitor.WaitForDelayStatements;

            // Create problems for each WAITFOR DELAY statement found
            // When creating a rule problem, always include the TSqlObject being analyzed. This
            // is used to determine
            // the name of the source this problem was found in and a best guess as to the
            // line/column the problem was found at.
            //
            // In addition if you have a specific TSqlFragment that is related to the problem
            // also include this
            // since the most accurate source position information (start line and column) will
            // be read from the fragment
            foreach (WaitForStatement waitForStatement in waitforDelayStatements)
            {
                var problem = new SqlRuleProblem(
                    string.Format(CultureInfo.InvariantCulture, ruleDescriptor.DisplayDescription, elementName),
                    modelElement,
                    waitForStatement);
                problems.Add(problem);
            }

            return problems;
        }

        private static string GetElementName(
            SqlRuleExecutionContext ruleExecutionContext,
            TSqlObject modelElement)
        {
            // Get the element name using the built in DisplayServices. This provides a number of
            // useful formatting options to
            // make a name user-readable
            var displayServices = ruleExecutionContext.SchemaModel.DisplayServices;
            var elementName = displayServices.GetElementName(
                modelElement, ElementNameStyle.EscapedFullyQualifiedName);
            return elementName;
        }

        private static bool IsInlineTableValuedFunction(TSqlObject modelElement)
        {
            return TableValuedFunction.TypeClass.Equals(modelElement.ObjectType)
                && modelElement.GetMetadata<FunctionType>(TableValuedFunction.FunctionType)
                == FunctionType.InlineTableValuedFunction;
        }
    }

    internal class WaitForDelayVisitor : TSqlConcreteFragmentVisitor
    {
        public IList<WaitForStatement> WaitForDelayStatements { get; private set; }

        // Define the class constructor
        public WaitForDelayVisitor()
        {
            WaitForDelayStatements = new List<WaitForStatement>();
        }

        public override void ExplicitVisit(WaitForStatement node)
        {
            // We are only interested in WAITFOR DELAY occurrences
            if (node.WaitForOption == WaitForOption.Delay)
            {
                WaitForDelayStatements.Add(node);
            }
        }
    }
}