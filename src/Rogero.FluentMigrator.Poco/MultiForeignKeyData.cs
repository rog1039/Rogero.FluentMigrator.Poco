using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Rogero.FluentMigrator.Poco
{
    public class MultiForeignKeyData
    {
        public string        GroupName  { get; set; }
        public string        ForeignSchemaName  { get; set; }
        public string        ForeignTableName   { get; set; }
        public IList<string> ForeignColumnNames { get; set; }
        
        public string        PrimarySchemaName  { get; set; }
        public string        PrimaryTableName   { get; set; }
        public IList<string> PrimaryColumnNames { get; set; }

        public Rule CascadeRule { get; set; }

        public string GetForeignKeyName()
        {
            return NameHelper.GetForeignKeyName(
                ForeignTableName,
                ForeignColumnNames,
                PrimaryTableName,
                PrimaryColumnNames);
        }

        public static List<MultiForeignKeyData> CreateFromForeignKeyDatas(
            IEnumerable<ColumnDataForeignKey> foreignKeyInformations)
        {
            var groupedFks = foreignKeyInformations
                .GroupBy(z => z.GroupId)
                .ToList();

            var results = new List<MultiForeignKeyData>();
            
            foreach (var fkGroup in groupedFks)
            {
                ValidateFkGroup(fkGroup);
                var fk             = fkGroup.First();
                var foreignColumns = fkGroup.Select(z => z.ForeignColumnNames).ToList();
                var primaryColumns = fkGroup.Select(z => z.PrimaryColumnNames).ToList();

                var multiForeignKey = new MultiForeignKeyData()
                {
                    GroupName = fk.GroupId,
                    ForeignSchemaName  = fk.ForeignSchemaName,
                    ForeignTableName   = fk.ForeignTableName,
                    ForeignColumnNames = foreignColumns,
                    PrimarySchemaName  = fk.PrimarySchemaName,
                    PrimaryTableName   = fk.PrimaryTableName,
                    PrimaryColumnNames = primaryColumns,
                    CascadeRule        = fk.CascadeDeleteRule
                };
                results.Add(multiForeignKey);
            }

            return results;
        }
        
        private static void ValidateFkGroup(IGrouping<string,ColumnDataForeignKey> fkGroup)
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
    }
}