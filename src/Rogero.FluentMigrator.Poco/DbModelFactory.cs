using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rogero.FluentMigrator.Poco
{
    public class DbModel
    {
        public List<Type>      InputTypes       { get; set; }
        public List<TableData> OutputTableDatas { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            OutputTableDatas.ForEach(td => sb.AppendLine(td.ToString()));
            return sb.ToString();
        }
    }
    public class DbModelFactory
    {
        public static DbModel GenerateModel(IEnumerable<Type> types)
        {
            var tableDatas = types.Select(TableDataFactory.CreateTableDataFromType).ToList();
            
            //Now that we have generated the basic table datas, let's go over it again in at attempt to infer foreign keys.
            InferForeignKeys(tableDatas);

            var model = new DbModel()
            {
                InputTypes       = types.ToList(),
                OutputTableDatas = tableDatas
            };
            return model;
        }

        private static void InferForeignKeys(List<TableData> tableDatas)
        {
            TableData? GetPrimaryTable(string columnName)
            {
                if (!columnName.EndsWith("Id")) return null;
                var typeName = columnName.Substring(0, columnName.Length - 2);
                return tableDatas.SingleOrDefault(z => z.SourceType.Name == typeName);
            }
            
            foreach (var tableData in tableDatas)
            {
                foreach (var columnData in tableData.ColumnCreationData)
                {
                    if (columnData.CascadeRuleInformation == null) continue;
                    
                    var columnName = columnData.ColumnDataName.Name;
                    var endsWithId = columnName.EndsWith("Id", StringComparison.InvariantCultureIgnoreCase);
                    if(!endsWithId) continue;

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