using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.SqlServer.Server;

namespace CbrWebService
{
    public partial class CbrWebServiceClass
    {
        public class CursContent
        {
            public DateTime Дата;
            public double Курс;
        }

        [SqlFunction(DataAccess = DataAccessKind.Read, FillRowMethodName = "GetCursDynamicFill")]
        public static IEnumerable GetCursDynamic(SqlDateTime From, SqlDateTime To, SqlString EnumValute)
        {
            var xmlNode = (new DailyInfo()).GetCursDynamicXML(
                From.Value,To.Value,EnumValute.Value);

            var xList = XDocument
                .Load(new XmlNodeReader(xmlNode))
                .Element("ValuteData")
                .Elements("ValuteCursDynamic")
                .Select(x => new CursContent
                {
                    Дата = (DateTime)x.Element("CursDate"),
                    Курс = (double)x.Element("Vcurs") / (double)x.Element("Vnom")
                });
            /*
            var xList = new List<CursContent>();
            xList.Add(new CursContent(){Дата=DateTime.Now, Курс=0});
            */
            return xList;
        }

        public static void GetCursDynamicFill(object o, out SqlDateTime Дата, out SqlDouble Курс)
        {
            Дата = ((CursContent)o).Дата;
            Курс = ((CursContent)o).Курс;
        }

    }
}