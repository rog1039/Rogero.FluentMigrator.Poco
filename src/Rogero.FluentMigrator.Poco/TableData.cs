using System;
using System.Collections.Generic;
using System.Text;

namespace Rogero.FluentMigrator.Poco
{
    public class TableData
    {
        public Type             SourceType { get; set; }
        public SchemaTableNames TableName  { get; set; }

        public List<ColumnData> ColumnCreationData { get; set; } = new();

        public List<ColumnDataForeignKey> ForeignKeyParts  { get; } = new();
        public List<MultiForeignKeyData>  MultiForeignKeys { get; } = new();

        public void Deconstruct(out SchemaTableNames table, out IList<ColumnData> columns)
        {
            table   = TableName;
            columns = ColumnCreationData;
        }

        public void AddForeignKeyPart(ColumnDataForeignKey fkInfo)
        {
            ForeignKeyParts.Add(fkInfo);
            BuildForeignKeyComplete();
        }

        private void BuildForeignKeyComplete()
        {
            MultiForeignKeys.Clear();
            var multiForeignKeys = MultiForeignKeyData.CreateFromForeignKeyDatas(ForeignKeyParts);
            MultiForeignKeys.AddRange(multiForeignKeys);
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