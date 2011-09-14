using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using System.Xml;
using System.Data.SqlClient;

namespace DBMLUpdate
{
    // Handles to the DBML Doc and is the core class of the plugin
    class DBMLDoc
    {
        /// <summary>
        /// Initializes the document
        /// </summary>
        /// <param name="fileName">The file name of the dbml document</param>
        /// <param name="projectItem">The documents project item</param>
        public DBMLDoc(string fileName, ProjectItem projectItem)
        {
            _doc = new XmlDocument();
            _doc.Load(fileName);
            _pi = projectItem;
            _nsm = new XmlNamespaceManager(_doc.NameTable);
            _nsm.AddNamespace("dbml", "http://schemas.microsoft.com/linqtosql/dbml/2007");

            if(_doc.GetElementsByTagName("Connection").Count == 1)
                _connStr = _doc.GetElementsByTagName("Connection")[0].Attributes.GetNamedItem("ConnectionString").InnerText;

            if(string.IsNullOrEmpty(_connStr))
                throw new ArgumentException("No Connection string is specified for this linq-to-SQL dbml file");

            _tables = new List<DBMLTable>();
            foreach(XmlNode n in _doc.GetElementsByTagName("Table"))
            {
                XmlElement e = n as XmlElement;
                if(e != null)
                    _tables.Add(new DBMLTable(this, e));
            }
        }

        /// <summary>
        /// The number of tables in the file
        /// </summary>
        public int TableCount { get { return _tables.Count; } }

        /// <summary>
        /// Updates the DBML information
        /// </summary>
        /// <param name="output">The output window to report status</param>
        public void DBUpdate(OutputWindowPane output)
        {
            using(SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                foreach(DBMLTable t in _tables)
                {
                    output.OutputString(string.Format("Getting Table {0} information...\r\n", t.Name));
                    t.DBUpdate(conn);
                }
            }
        }

        /// <summary>
        /// Creates a new DBML file with the updates
        /// </summary>
        public void CreateDBML(string fileName, OutputWindowPane output)
        {
            _newDoc = new XmlDocument();
            XmlDeclaration dec = _newDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            _newDoc.AppendChild(dec);

            XmlNode den = _newDoc.ImportNode(_doc.DocumentElement, false);
            _newDoc.AppendChild(den);

            bool doneTables = false;
            foreach(XmlNode n in _doc.DocumentElement.ChildNodes)
            {
                XmlElement e = n as XmlElement;
                if(e != null && e.Name == "Table")
                {
                    if(!doneTables)
                    {
                        foreach(DBMLTable t in _tables)
                        {
                            output.OutputString(string.Format("Adding table {0}...\r\n", t.Name));
                            den.AppendChild(t.CreateDBML(this));
                        }
                        doneTables = true;
                    }
                    continue;
                }

                XmlNode un = _newDoc.ImportNode(n, true);
                den.AppendChild(un);
            }

            output.OutputString(string.Format("Writing file \"{0}\"...\r\n", fileName));
            _newDoc.Save(fileName);
        }

        /// <summary>
        /// Imports a node to the new document
        /// </summary>
        /// <param name="node">The node to import</param>
        /// <param name="deep">If we import child nodes as well</param>
        /// <returns>The imported node</returns>
        public XmlNode ImportNode(XmlNode node, bool deep)
        {
            return _newDoc.ImportNode(node, deep);
        }

        /// <summary>
        /// Adds an element to the new document
        /// </summary>
        /// <param name="name">The name of the element</param>
        /// <returns>The new node</returns>
        public XmlElement AddElement(string name)
        {
            return _newDoc.CreateElement(name, _newDoc.DocumentElement.NamespaceURI);
        }

        /// <summary>
        /// Adds nodes from the old document to the new document, with a parent element
        /// </summary>
        /// <param name="e">The parent element</param>
        /// <param name="nodes">The nodes to add</param>
        /// <param name="excludes">An array of the names of nodes to exclude</param>
        public void AddNodes(XmlElement e, XmlNodeList nodes, params string[] excludes)
        {
            IList<string> list = (IList<string>)excludes;
            foreach(XmlNode n in nodes)
            {
                if(!list.Contains(n.Name))
                    e.AppendChild(ImportNode(n, true));
            }
        }

        /// <summary>
        /// Creates an attribute on the new document, and adds it to the element
        /// </summary>
        /// <param name="e">The element to add the attribute to</param>
        /// <param name="name">The name of the attribute</param>
        /// <param name="value">The value of the attribute</param>
        public void AddAttribute(XmlElement e, string name, string value)
        {
            XmlAttribute atr = _newDoc.CreateAttribute(name);
            atr.Value = value;
            e.Attributes.Append(atr);
        }

        /// <summary>
        /// Adds a list of attributes on the new document to an element
        /// </summary>
        /// <param name="e">The element to add the attributes to</param>
        /// <param name="attrs">The list of attributes to add</param>
        /// <param name="excludes">The names of attributes to exclude from the list</param>
        public void AddAttributes(XmlElement e, XmlAttributeCollection attrs, params string[] excludes)
        {
            IList<string> list = (IList<string>)excludes;
            foreach(XmlAttribute a in attrs)
            {
                if(!list.Contains(a.Name))
                    AddAttribute(e, a.Name, a.Value);
            }
        }

        private XmlDocument _doc;
        private XmlDocument _newDoc;
        private ProjectItem _pi;
        private XmlNamespaceManager _nsm;
        private string _connStr;
        private List<DBMLTable> _tables;
    }
}
