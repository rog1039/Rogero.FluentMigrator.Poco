using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public class TableData
    {
        public SchemaTableNames TableName { get; set; }

        public List<ColumnData> ColumnCreationData { get; set; } = new();

        public List<ForeignKeyInformation> ForeignKeyParts  { get; } = new();
        public List<MultiForeignKeyData>   MultiForeignKeys { get; } = new();

        public void Deconstruct(out SchemaTableNames table, out IList<ColumnData> columns)
        {
            table   = TableName;
            columns = ColumnCreationData;
        }

        public void AddForeignKeyPart(ForeignKeyInformation fkInfo)
        {
            ForeignKeyParts.Add(fkInfo);
            BuildForeignKeyComplete();
        }

        private void BuildForeignKeyComplete()
        {
            MultiForeignKeys.Clear();
            var groupedFks = ForeignKeyParts
                .GroupBy(z => z.GroupId)
                .ToList();
            foreach (var fkGroup in groupedFks)
            {
                VerifyFkGroup(fkGroup);
                var fk             = fkGroup.First();
                var foreignColumns = fkGroup.SelectMany(z => z.ForeignColumnNames).ToArray();
                var primaryColumns = fkGroup.SelectMany(z => z.PrimaryColumnNames).ToArray();

                var multiForeignKey = new MultiForeignKeyData()
                {
                    ForeignSchemaName  = fk.ForeignSchemaName,
                    ForeignTableName   = fk.ForeignTableName,
                    ForeignColumnNames = foreignColumns.ToList(),
                    PrimarySchemaName  = fk.PrimarySchemaName,
                    PrimaryTableName   = fk.PrimaryTableName,
                    PrimaryColumnNames = primaryColumns.ToList(),
                    CascadeRule        = fk.CascadeDeleteRule
                };
                MultiForeignKeys.Add(multiForeignKey);
            }
        }
        
        private static void VerifyFkGroup(IGrouping<string,ForeignKeyInformation> fkGroup)
        {
            var primarySchema = fkGroup.Select(z => z.PrimarySchemaName).Distinct().ToList();
            var primaryTable  = fkGroup.Select(z => z.PrimaryTableName).Distinct().ToList();
            var cascadeRule   = fkGroup.Select(z => z.CascadeDeleteRule).Distinct().ToList();

            var primarySchemaCount = primarySchema.Count;
            var primaryTableCount  = primaryTable.Count;
            var cascadeRuleCount   = cascadeRule.Count;

            if (primarySchemaCount > 1 || primaryTableCount > 1 || cascadeRuleCount > 1)
            {
                var problems = new List<string>();
                if(primarySchema.Count > 1) 
                    problems.Add($"Multiple primary schemas found: {primarySchema.StringJoin(",")}");
                if(primaryTable.Count > 1) 
                    problems.Add($"Multiple primary tables found: {primaryTable.StringJoin(",")}");
                if(cascadeRule.Count > 1) 
                    problems.Add($"Multiple cascade rules found: {cascadeRule.StringJoin(",")}");

                var problemSummary = problems.StringJoin(Environment.NewLine);

                throw new InvalidDataException("All foreign key links in a group must be congruent."
                                             + Environment.NewLine 
                                             + problemSummary);
            }
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Table: {TableName}");
            foreach (var column in ColumnCreationData)
            {
                sb.Append("   ");
                sb.AppendLine(column.ToString());
            }

            foreach (var multiForeignKey in MultiForeignKeys)
            {
                var foreignColumnNames = multiForeignKey.ForeignColumnNames.StringJoin(",");
                var primaryColumnNames = multiForeignKey.PrimaryColumnNames.StringJoin(",");
                sb.Append("   ");
                sb.AppendLine($"FK: ({foreignColumnNames}) -> " +
                              $"{multiForeignKey.PrimarySchemaName}.{multiForeignKey.PrimaryTableName}.({primaryColumnNames})");
            }
            return sb.ToString();
        }
    }
}