using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data.SqlClient;
using System.Data;

namespace DBMLUpdate
{
    /// <summary>
    /// Handles the details for each table in the dbml file
    /// </summary>
    class DBMLTable
    {
        /// <summary>
        /// Creates a DBMLTable for a document, with the details held in xml element
        /// </summary>
        /// <param name="doc">The DBMLDocument this table belongs to</param>
        /// <param name="e">The XmlElement containing the details of the table</param>
        public DBMLTable(DBMLDoc doc, XmlElement e)
        {
            _elem = e;
            _tableName = _elem.GetAttribute("Name");
            if(string.IsNullOrEmpty(_tableName))
                throw new ArgumentException("Cannot get table name");

            XmlNodeList nl = _elem.GetElementsByTagName("Type");
            if(nl.Count != 1 || nl[0].NodeType != XmlNodeType.Element)
                throw new ArgumentException("Cannot Identity type for table " + _tableName);
            _typeElem = nl[0] as XmlElement;

            _cols = new DBMLColumnList();
            try
            {
                foreach(XmlNode n in _typeElem.ChildNodes)
                {
                    XmlElement ce = n as XmlElement;
                    if(ce != null && ce.Name == "Column")
                        _cols.Add(new DBMLColumn(ce));
                }
            }
            catch(Exception ex)
            {
                throw new ArgumentException(string.Format("Problem parsing table {0} {1}", _tableName, ex.Message));
            }
        }

        /// <summary>
        /// The name of the table
        /// </summary>
        public string Name { get { return _tableName; } }

        /// <summary>
        /// Updates the table with information from the sql connection
        /// </summary>
        /// <param name="conn">The sql Connection</param>
        public void DBUpdate(SqlConnection conn)
        {
            string schemaName, tableName;
            UnqualifyName(out schemaName,out tableName);
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter(string.Format(SQL_COLSELECT, schemaName, tableName), conn);
            da.Fill(ds);
            foreach(DataRow dr in ds.Tables[0].Rows)
            {
                DBMLColumn col = new DBMLColumn(dr);
                DBMLColumn match = _cols.MatchCol(col);
                if(match != null)
                    match.Update(col);
                else
                    _cols.Add(col);
            }
        }

        /// <summary>
        /// Creates an Xml element with the details of the table
        /// </summary>
        /// <param name="doc">The parent doc</param>
        /// <returns>XmlElement for the new document</returns>
        public XmlElement CreateDBML(DBMLDoc doc)
        {
            XmlElement table = doc.AddElement("Table");
            doc.AddAttributes(table, _elem.Attributes, string.Empty);

            XmlElement type = doc.AddElement("Type");
            doc.AddAttributes(type, _typeElem.Attributes, string.Empty);

            _cols.Sort();
            foreach(DBMLColumn c in _cols)
            {
                if(c.Found)
                    type.AppendChild(c.CreateDBML(doc));
            }

            doc.AddNodes(type, _typeElem.ChildNodes, "Column");
            table.AppendChild(type);

            doc.AddNodes(table, _elem.ChildNodes, "Type");
            return table;
        }

        private void UnqualifyName(out string schemaName, out string tableName)
        {
            schemaName = "dbo";
            tableName = _tableName;
            if(tableName.Contains("."))
            {
                schemaName = tableName.Substring(0, tableName.IndexOf("."));
                tableName = tableName.Substring(tableName.IndexOf(".") + 1);
            }

             if(schemaName.StartsWith("[") && schemaName.EndsWith("]"))
                schemaName = schemaName.Substring(1, schemaName.Length - 2);

            if(tableName.StartsWith("[") && tableName.EndsWith("]"))
                tableName = tableName.Substring(1, tableName.Length - 2);
        }

        private XmlElement _elem;
        private XmlElement _typeElem;
        private string _tableName;
        private DBMLColumnList _cols;

        private const string SQL_COLSELECT = @"SELECT c.COLUMN_NAME, c.DATA_TYPE, c.CHARACTER_MAXIMUM_LENGTH, c.NUMERIC_PRECISION, c.NUMERIC_SCALE, c.ORDINAL_POSITION,
        CAST(COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME),c.COLUMN_NAME,'IsIdentity') AS BIT) AS IsIdentity,
        CAST(COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME),c.COLUMN_NAME,'AllowsNull') AS BIT) AS AllowsNull,
        CAST(CASE WHEN tc.CONSTRAINT_TYPE = 'PRIMARY KEY' THEN 1 ELSE 0 END AS BIT) AS PrimaryKey,
        CAST(CASE WHEN c.COLUMN_DEFAULT IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS HasDefault
    FROM INFORMATION_SCHEMA.COLUMNS c
        LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu ON kcu.TABLE_SCHEMA = c.TABLE_SCHEMA AND kcu.TABLE_NAME = c.TABLE_NAME AND kcu.COLUMN_NAME = c.COLUMN_NAME
        LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc ON tc.CONSTRAINT_SCHEMA = kcu.CONSTRAINT_SCHEMA AND tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME AND CONSTRAINT_TYPE = 'PRIMARY KEY'
    WHERE c.TABLE_SCHEMA = '{0}' AND c.TABLE_NAME = '{1}'
    ORDER BY c.ORDINAL_POSITION";
    }
}
