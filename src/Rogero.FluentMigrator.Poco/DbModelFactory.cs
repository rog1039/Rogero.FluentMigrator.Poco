using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;

namespace Rogero.FluentMigrator.Poco
{
    public class DbModel
    {
        public List<Type>      InputTypes       { get; set; }
        public List<TableData> OutputTableDatas { get; set; }

        public void Apply(Migration migration)
        {
            OutputTableDatas
                .ForEach(tableData => migration.Apply(tableData));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            OutputTableDatas.ForEach(td => sb.AppendLine(td.ToString()));
            return sb.ToString();
        }
    }

    public static class DbModelFactory
    {
        public static DbModel GenerateModel(IEnumerable<Type> typesParameter)
        {
            var types = typesParameter.ToList();

            var tableDatas = types.Select(TableDataFactory.CreateTableDataFromType).ToList();

            //Now that we have generated the basic table datas, let's go over it again in at attempt to infer foreign keys.
            InferForeignKeys(tableDatas);

            //And let's sort them topologically so they are sent to SQL server in a correct order.
            tableDatas = SortTableDatasTopologically(tableDatas);

            var model = new DbModel()
            {
                InputTypes       = types,
                OutputTableDatas = tableDatas
            };
            return model;
        }

        private static List<TableData> SortTableDatasTopologically(List<TableData> tableDatas)
        {
            var stack = new Stack<TableData>();
            var seen  = new HashSet<TableData>();

            void Visit(TableData tableData)
            {
                seen.Add(tableData);
                var nextNodes = GetNextNodes(tableData);
                foreach (var nextNode in nextNodes)
                {
                    if (stack.Contains(nextNode)) continue;
                    if (seen.Contains(nextNode)) continue;
                    Visit(nextNode);
                }

                if (!stack.Contains(tableData)) stack.Push(tableData);
            }

            IList<TableData> GetNextNodes(TableData tableData)
            {
                return tableData
                    .ColumnCreationData
                    .Select(z => z.ForeignKeyInformation)
                    .Where(z => z is not null)
                    .Select(fk =>
                    {
                        var foreignKeyTable = new SchemaTableNames(fk.PrimarySchemaName, fk.PrimaryTableName);
                        var matchingTable   = tableDatas.SingleOrDefault(z => z.TableName == foreignKeyTable);
                        return matchingTable;
                    })
                    .Where(z => z is not null)
                    .ToList()!;
            }

            foreach (var table in tableDatas)
            {
                Visit(table);
            }

            var results = stack.Reverse().ToList();
            return results;
        }

        private static void InferForeignKeys(List<TableData> tableDatas)
        {
            TableData? GetPrimaryTable(string columnName)
            {
                if (!columnName.EndsWith("Id")) return null;
                var typeName = columnName.Substring(0, columnName.Length - 2);

                //First let's find a match solely on CLR type name.
                var matchingTableByClrMatch = tableDatas.SingleOrDefault(z => z.SourceType.Name == typeName);
                if (matchingTableByClrMatch != null) return matchingTableByClrMatch;

                //Second, let's try matching on table name.
                var matchingTableByTableName = tableDatas.SingleOrDefault(z => z.TableName.Table == typeName);
                return matchingTableByTableName;
            }

            foreach (var tableData in tableDatas)
            {
                foreach (var columnData in tableData.ColumnCreationData)
                {
                    /*
                     * We'd like to do a few things:
                     * 1. Handle columns attribute with simply cascade rule.
                     * 2. Fix up FK attributes with no primary key specified.
                     * */
                    if (columnData.CascadeRuleInformation is not null)
                    {
                        var columnName = columnData.ColumnDataName.Name;
                        var endsWithId = columnName.EndsWith("Id", StringComparison.InvariantCultureIgnoreCase);
                        if (!endsWithId) continue;

                        var isForeignKeyAlready = columnData.ForeignKeyInformation != null;
                        if (isForeignKeyAlready) continue;

                        var matchingPrimaryTable = GetPrimaryTable(columnData.ColumnDataName.Name);
                        if (matchingPrimaryTable == null) continue;

                        var foreignKey = new ColumnDataForeignKey(
                            tableData.TableName.Schema,
                            tableData.TableName.Table,
                            columnName,
                            matchingPrimaryTable.TableName.Schema,
                            matchingPrimaryTable.TableName.Table,
                            "Id",
                            columnData.CascadeRuleInformation.CascadeRule);
                        columnData.ForeignKeyInformation = foreignKey;
                    }
                }
            }
        }

    }
}