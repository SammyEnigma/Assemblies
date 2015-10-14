using System;
using System.Data.SqlClient;
using System.Xml.Linq;
using System.Data;
using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;
using System.Xml;
using System.Xml.Xsl;
using System.IO;

namespace ExternalXML
{
    public static class ExternalXML
    {
        [SqlFunction(DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
        public static SqlXml ExecQuery(SqlString Sql, SqlXml Options, SqlXml Input)
        {
            var XOutput = new XDocument(new XElement("root"));

            try
            {
                using (var q = new SqlCommand())
                {
                    q.Connection = new SqlConnection("context connection=true");
                    q.CommandType = CommandType.Text;

                    q.CommandText = Sql.Value;
                    q.InitOptions(Options.Value);
                    q.Parameters.SetInput(Input.Value);

                    q.Connection.Open();
                    var Result = q.ExecuteXmlReader();
                    if (Result.Read())
                        XOutput.Root.Element("content").Add(
                            XElement.Load(Result, LoadOptions.None));
                    q.Connection.Close();
                }
            }
            catch (Exception ex)
            {
                XOutput.Root.Add(ex.ExceptionSerialize());
            }
            
            return new SqlXml(XOutput.CreateReader());
             
        }

        [SqlFunction(DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
        public static SqlXml ExecScript(SqlString Sql, SqlXml Options, SqlXml Input)
        {
            var XOutput = new XDocument(
                new XElement("root", 
                    new XElement("content")));

            try
            {
                using (var q = new SqlCommand())
                {
                    q.Connection = new SqlConnection("context connection=true");
                    q.CommandType = CommandType.Text;

                    q.CommandText = Sql.Value;
                    q.InitOptions(Options.Value);
                    q.Parameters.SetInput(Input.Value);

                    q.Connection.Open();
                    q.ExecuteNonQuery();
                    XOutput.Root.Add(q.Parameters.GetOutput());
                    q.Connection.Close();
                }
            }
            catch (Exception ex)
            {
                XOutput.Root.Add(ex.ExceptionSerialize());
            }

            return new SqlXml(XOutput.CreateReader());

        }

        [SqlFunction(DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
        public static SqlXml ExecXQuery(SqlXml Source, SqlString XQuery)
        {

            using (var q = new SqlCommand())
            {
                q.Connection = new SqlConnection("context connection=true");
                q.CommandType = CommandType.Text;


                q.CommandText =
                    String.Format("SELECT @X.query('{0}')",
                                  XQuery.Value.Replace("'", "''"));

                q.Parameters.Add("@X", SqlDbType.Xml).Direction = ParameterDirection.Input;
                q.Parameters["@X"].Value = Source.Value;

                q.Connection.Open();
                return new SqlXml(q.ExecuteXmlReader());
            }
        }

        [SqlFunction(DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
        public static SqlXml ExecXSLT(SqlXml Source, SqlXml XSLT)
        {
            var XslCompiledTransform = new XslCompiledTransform();
            XslCompiledTransform.Load(XSLT.CreateReader());

            var buffer = new MemoryStream();
            var stream = new StreamWriter(buffer);
            XslCompiledTransform.Transform(Source.CreateReader(), XmlWriter.Create(stream));
            stream.Close();
            return new SqlXml(buffer);
        }

        [SqlFunction(DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
        public static SqlBoolean ExecXSD(SqlXml Source, SqlXml XSD)
        {
            try
            {
                var settings = new XmlReaderSettings();
                settings.Schemas.Add(null, XSD.CreateReader());
                settings.ConformanceLevel = ConformanceLevel.Auto;
                settings.ValidationType = ValidationType.Schema;
                var reader = XmlReader.Create(Source.CreateReader(), settings);
                while (reader.Read()) { }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

