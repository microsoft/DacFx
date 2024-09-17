//------------------------------------------------------------------------------
// <copyright>
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dac.Extensibility;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;

namespace CodeAnalyzerSample
{

    /// <summary>
    /// Simple test class - note it doesn't use resources since these aren't handled by the test harness
    /// that builds dll files
    /// </summary>
    [ExportCodeAnalysisRule("CodeAnalyzerSample.TableNameRule001",
        "SampleRule",
        Description = "Table names should not end in 'View'",
        Category = "Naming",
        PlatformCompatibility = TSqlPlatformCompatibility.OnPremises)]
    class TableNameEndingInViewRule : SqlCodeAnalysisRule
    {
        private static readonly ModelTypeClass[] _supportedElementTypes = new[] { ModelSchema.Table };

        public TableNameEndingInViewRule()
        {
            SupportedElementTypes = new[] { Table.TypeClass };
        }
        
        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            List<SqlRuleProblem> problems = new List<SqlRuleProblem>();

            TSqlObject table = ruleExecutionContext.ModelElement;
            if (table != null)
            {
                if (NameEndsInView(table.Name))
                {
                    string problemDescription = string.Format("Table name {0} ends in View. This can cause confusion and should be avoided",
                                                              GetQualifiedTableName(table.Name));
                    SqlRuleProblem problem = new SqlRuleProblem(problemDescription, table);
                    problems.Add(problem);
                }
            }

            return problems;
        }

        private bool NameEndsInView(ObjectIdentifier id)
        {
            return id.HasName && id.Parts.Last().EndsWith("View", StringComparison.OrdinalIgnoreCase);
        }

        private string GetQualifiedTableName(ObjectIdentifier id)
        {
            StringBuilder buf = new StringBuilder();
            foreach (string part in id.Parts)
            {
                if (buf.Length > 0)
                {
                    buf.Append('.');
                }
                buf.Append('[').Append(part).Append(']');
            }
            return buf.ToString();
        }
    }
}
