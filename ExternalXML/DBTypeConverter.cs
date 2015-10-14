using System;
using System.Data.SqlClient;
using System.Xml.Linq;
using System.Data;
using System.Xml;
using System.Data.SqlTypes;

namespace ExternalXML
{
    class DBTypeConverter
    {
        public static void XElenemtToParam(SqlParameter param, XElement el)
        {
            switch (param.SqlDbType)
            {
                case SqlDbType.BigInt:
                    param.Value = (long)el;
                    break;
                case SqlDbType.Bit:
                    param.Value = (bool)el;
                    break;
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.Udt:
                case SqlDbType.VarChar:
                case SqlDbType.Variant:
                    param.Value = el.Value;
                    break;
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                case SqlDbType.SmallDateTime:
                case SqlDbType.Time:
                case SqlDbType.Timestamp:
                    param.Value = (DateTime)el;
                    break;
                case SqlDbType.DateTimeOffset:
                    param.Value = (DateTimeOffset)el;
                    break;
                case SqlDbType.Money:
                case SqlDbType.Decimal:
                case SqlDbType.SmallMoney:
                    param.Value = (decimal)el;
                    break;
                case SqlDbType.Float:
                case SqlDbType.Real:
                    param.Value = (double)el;
                    break;
                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.VarBinary:
                    param.Value = Convert.FromBase64String(el.Value);
                    break;
                case SqlDbType.Int:
                case SqlDbType.SmallInt:
                case SqlDbType.TinyInt:
                    param.Value = (int)el;
                    break;
                case SqlDbType.Structured:
                    throw new Exception("Type <Structured> isn't implement.");
                case SqlDbType.UniqueIdentifier:
                    param.Value = (Guid)el;
                    break;
                case SqlDbType.Xml:
                    var context = new XmlParserContext
                        (null, null, null, XmlSpace.None);
                    param.Value = new SqlXml(new XmlTextReader(
                        el.FirstNode.ToString(), XmlNodeType.Element, context));
                    break;
            }
        }
        public static void ParamToXElenemt(SqlParameter param, XElement el)
        {
            switch (param.SqlDbType)
            {
                case SqlDbType.BigInt:
                case SqlDbType.Bit:
                case SqlDbType.Char:
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                case SqlDbType.DateTimeOffset:
                case SqlDbType.Decimal:
                case SqlDbType.Float:
                case SqlDbType.Int:
                case SqlDbType.Money:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Real:
                case SqlDbType.SmallDateTime:
                case SqlDbType.SmallInt:
                case SqlDbType.SmallMoney:
                case SqlDbType.Structured:
                case SqlDbType.Text:
                case SqlDbType.Time:
                case SqlDbType.Timestamp:
                case SqlDbType.TinyInt:
                case SqlDbType.Udt:
                case SqlDbType.UniqueIdentifier:
                case SqlDbType.VarChar:
                case SqlDbType.Variant:
                    el.SetValue(param.Value);
                    break;
                case SqlDbType.Image:
                case SqlDbType.Binary:
                case SqlDbType.VarBinary:
                    el.Value = Convert.ToBase64String((byte[])param.Value);
                    break;
                case SqlDbType.Xml:
                    var value = (SqlXml) param.Value;
                    el.Add(XElement.Load(value.CreateReader()));
                    break;
            }
        }
    }
}
