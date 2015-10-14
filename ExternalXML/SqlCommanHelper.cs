using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace ExternalXML
{
    public static class SqlCommanHelper
    {
        public static void InitOptions(this SqlCommand q, string xml)
        {
            var options = XDocument.Parse(xml);
            foreach (var xParam in options.Root.Element("parameters").Elements("parameter"))
            {
                var param = new SqlParameter
                {
                    ParameterName = "@" + xParam.Attribute("name").Value,
                    SqlDbType = (SqlDbType)
                        Enum.Parse(typeof (SqlDbType), xParam.Attribute("type").Value, true),
                    Direction = (ParameterDirection)
                        Enum.Parse(typeof (ParameterDirection), xParam.Attribute("direction").Value, true)
                };
                if (xParam.Attribute("size") != null)
                    param.Size = (int)xParam.Attribute("size");
                q.Parameters.Add(param);
            }
        }

        public static void SetInput(this SqlParameterCollection parameters, string xml)
        {
            var input = XDocument.Parse(xml);
            foreach (var XInputValue in input.Root.Elements("parameter"))
            {
                DBTypeConverter.XElenemtToParam(
                    parameters["@" + XInputValue.Attribute("name").Value],
                    XInputValue);
            }
        }

        public static XElement GetOutput(this SqlParameterCollection parameters)
        {
            var output = new XElement("content");
            foreach (SqlParameter p in parameters)
            {
                if (
                    p.Direction == ParameterDirection.Input ||
                    p.Value == null || p.Value == DBNull.Value)
                    continue;

                var xp = new XElement("parameter");
                output.Add(xp);
                xp.Add(new XAttribute("name", p.ParameterName.Replace("@", "")));
                DBTypeConverter.ParamToXElenemt(p, xp);
            }
            return output;
        }

        public static XElement ExceptionSerialize(this Exception e)
        {
            return new XElement("exception",
                new XElement("type", e.GetType()),
                new XElement("message", e.Message),
                new XElement("source", e.Source),
                new XElement("stacktrace", e.StackTrace));
        }


    }
}
