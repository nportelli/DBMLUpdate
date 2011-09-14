using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Collections;

namespace DBMLUpdate
{
    /// <summary>
    /// The details of columns within each table
    /// </summary>
    class DBMLColumn : IComparable
    {
        /// <summary>
        /// Creates a DBML column based on the information held in the Xml Element
        /// </summary>
        /// <param name="e">The Xml element holding the details of a column</param>
        public DBMLColumn(XmlElement e)
        {
            _elem = e;
            _name = e.GetAttribute("Name");
            if(string.IsNullOrEmpty(_name))
                throw new ArgumentException("Column has no name");
            _found = false;
        }

        /// <summary>
        /// Create a DBML column based of the information held in the datarow
        /// </summary>
        /// <param name="dr">The datarow holding the details of a column</param>
        public DBMLColumn(DataRow dr)
        {
            RowRead rr = new RowRead(dr);
            _name = rr.ToStr("COLUMN_NAME");
            _dbType = rr.ToStr("DATA_TYPE");
            _maxLen = rr.ToInt("CHARACTER_MAXIMU_LENGTH");
            _precision = rr.ToInt("NUMERIC_PRECISION");
            _scale = rr.ToInt("NUMERIC_SCALE");
            _isIdentity = rr.ToBool("IsIdentity");
            _canBeNull = rr.ToBool("AllowsNull");
            _isPrimaryKey = rr.ToBool("PrimaryKey");
            _dbGenerated = rr.ToBool("HasDefault");
            _position = rr.ToInt("OrdinalPosition");
            _found = true;
        }

        /// <summary>
        /// Returns true if the column is found in the database
        /// </summary>
        public bool Found { get { return _found; } }

        /// <summary>
        /// Updates the details of a column to match those in the database
        /// </summary>
        /// <param name="col"></param>
        public void Update(DBMLColumn col)
        {
            _dbType = col._dbType;
            _maxLen = col._maxLen;
            _precision = col._precision;
            _scale = col._scale;
            _isIdentity = col._isIdentity;
            _canBeNull = col._canBeNull;
            _isPrimaryKey = col._isPrimaryKey;
            _dbGenerated = col._dbGenerated;
            _position = col._position;
            _found = true;
        }

        /// <summary>
        /// Creates an Xml Element with the details of the column for a new DBML file
        /// </summary>
        /// <param name="doc">The parent document</param>
        /// <returns>An Xml Element cotaining the details of the column</returns>
        public XmlElement CreateDBML(DBMLDoc doc)
        {
            if(!_found)
                return null;

            XmlElement col = doc.AddElement("Column");
            doc.AddAttribute(col, "Name", _name);
            doc.AddAttribute(col, "Type", clrType);
            doc.AddAttribute(col, "DbType", dbType);
            if(_isPrimaryKey)
                doc.AddAttribute(col, "IsPrimaryKey", "true");
            if(_isIdentity || _dbGenerated)
                doc.AddAttribute(col, "IsDbGenerated", "true");
            doc.AddAttribute(col, "CanBeNull", _canBeNull ? "true" : "false");

            if(_elem != null)
                doc.AddAttributes(col, _elem.Attributes, COL_KNOWNATTR);

            return col;
        }

        /// <summary>
        /// Does a quick name match between columns
        /// </summary>
        /// <param name="c">The column to match with</param>
        /// <returns>True if the names match</returns>
        public bool Match(DBMLColumn c)
        {
            if(_name == c._name || _name == ("[" + c._name + "]"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Compare interface for sorting
        /// </summary>
        /// <param name="obj">The obj to compare to</param>
        public int CompareTo(object obj)
        {
            if(obj is DBMLColumn)
            {
                DBMLColumn col = obj as DBMLColumn;
                return Comparer.Default.Compare(_position, col._position);
            }
            else
                throw new ArgumentException("obj must be of type DBMLColumn for comparison");
        }

        private string clrType
        {
            get
            {
                string netType = string.Empty;
                switch(_dbType.ToLower())
                {
                    case "bit": return "System.Boolean";
                    case "tinyint": return "System.Byte";
                    case "smallint": return "System.Int16";
                    case "int": return "System.Int32";
                    case "bigint": return "System.Int64";

                    case "money":
                    case "smallmoney":
                    case "numeric":
                    case "decimal": return "System.Decimal";

                    case "float": return "System.Double";
                    case "real": return "System.Single";

                    case "date":
                    case "datetime":
                    case "smalldatetime":
                    case "datetime2": return "System.DateTime";

                    case "time": return "System.TimeSpan";

                    case "datetimeoffset": return "System.DateTimeOffset";

                    case "nvarchar":
                    case "varchar":
                    case "ntext":
                    case "text": return "System.String";

                    case "nchar":
                    case "char":
                        if(_maxLen == 1)
                            return "System.Char";
                        else
                            return "System.String";

                    case "binary":
                    case "varbinary":
                    case "image": return "System.Data.Linq.Binary";

                    case "timestamp": return "System.Data.Linq.Binary";
                    case "hierarchid": return "System.String";
                    case "uniqueidentifier": return "System.Guid";
                    case "sql_variant": return "System.Object";
                    case "xml": return "System.Xml.Linq.XElement";
                }

                return "System.String";
            }
        }

        private string dbType
        {
            get
            {
                string rst = string.Empty;
                switch(_dbType.ToLower())
                {
                    case "sql_variant":
                        rst = "Variant";
                        break;

                    case "timestamp":
                        rst = "rowversion";
                        break;
                    case "decimal":
                    case "numeric":
                        rst = "decimal" + ((_precision > 0 && _scale > 0) ? "(" + _precision + "," + _scale + ")" : "");
                        break;
                    case "varbinary":
                    case "varchar":
                    case "char":
                        rst = _dbType + (_maxLen > 0 ? ("(" + _maxLen.ToString() + ")") : (_maxLen == -1 ? "(max)" : ""));
                        break;
                    case "nvarchar":
                    case "nchar":
                        rst = _dbType + (_maxLen > 0 ? ("(" + (_maxLen / 2).ToString() + ")") : (_maxLen == -1 ? "(max)" : ""));
                        break;
                    case "hierarchyid":
                        rst = "nvarchar";
                        break;
                    default:
                        rst = _dbType;
                        break;
                }
                return (rst + (!_canBeNull ? " NOT NULL" : "") + (_isIdentity ? " IDENTITY" : ""));

            }
        }

        private string _name;
        private XmlElement _elem;
        private string _dbType;
        private int _maxLen;
        private int _precision;
        private int _scale;
        private bool _canBeNull;
        private bool _isPrimaryKey;
        private bool _isIdentity;
        private bool _dbGenerated;
        private int _position;
        private bool _found;

        private static string[] COL_KNOWNATTR = new string[] { "Name", "Type", "DbType", "IsPrimaryKey", "IsDbGenerated", "CanBeNull" };
    }

    class DBMLColumnList : List<DBMLColumn>
    {
        public DBMLColumn MatchCol(DBMLColumn column)
        {
            foreach(DBMLColumn c in this)
            {
                if(c.Match(column))
                    return c;
            }

            return null;
        }
    }
}
