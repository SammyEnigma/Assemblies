using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.SqlServer.Server;
using Nini.Config;

namespace JalousieLinked
{
    public static class JalousieLinkedFile
    {
        public class LamelItem
        {
            public int LayerNumber;
            public int LamelNumber;
            public double LengthLamel;

        }

        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes GetLinkedFile(SqlInt32 OrderId, SqlInt32 ProgramId)
        {
            
            if (OrderId.IsNull || ProgramId.IsNull)
                return SqlBytes.Null;
            string FileExtention = null;
            string FilePath = null;

            using (var db = new SqlConnection("context connection=true"))
            {

                using (var q = new SqlCommand() { Connection = db, CommandType = CommandType.Text })
                {
                    q.CommandText =
                        @"SELECT P.[Расширение]
                            FROM [Жалюзи_заказы] AS O
	                            INNER JOIN [Жалюзи_бланки_связанные_программы] AS L
		                            ON O.[Код бланка]=L.[Код бланка]
	                            INNER JOIN [Жалюзи_связанные_программы] AS P
		                            ON P.[Код]=L.[Код программы]
                            WHERE 
	                            O.[Код]=@OrderId AND
                                P.[Код]=@ProgramId";

                    q.Parameters.Add("@OrderId", SqlDbType.Int).Direction = ParameterDirection.Input;
                    q.Parameters.Add("@ProgramId", SqlDbType.Int).Direction = ParameterDirection.Input;

                    q.Parameters["@OrderId"].Value = OrderId.Value;
                    q.Parameters["@ProgramId"].Value = ProgramId.Value;

                    db.Open();
                    FileExtention = (string) q.ExecuteScalar();
                    if (FileExtention == null)
                        return SqlBytes.Null;
                }

                using (var q = new SqlCommand() { Connection = db, CommandType = CommandType.Text })
                {
                    q.CommandText =
                        @"SELECT [Текстовое значение]
                        FROM [_Константы]
                        WHERE [Код]=67";
                    FilePath = (string)q.ExecuteScalar();
                    if (FilePath == null || !Directory.Exists(FilePath))
                        return SqlBytes.Null;
                }

                
            }

            var FullPath = Path.Combine(FilePath, 
                String.Format("{0}-{1}.{2}",OrderId.Value,ProgramId.Value,FileExtention));
            if (!File.Exists(FullPath))
                return SqlBytes.Null;

            var fs = File.OpenRead(FullPath);
            var ms = new MemoryStream();
            ms.SetLength(fs.Length);
            fs.Read(ms.GetBuffer(), 0, (int)fs.Length);
            ms.Flush();
            fs.Close();
            return new SqlBytes(ms.GetBuffer());

        }

        [SqlProcedure()]
        public static void SetLinkedFile(SqlInt32 OrderId, SqlInt32 ProgramId, SqlBytes Value,
            out SqlBoolean Error, out SqlString Message)
        {
            if (OrderId.IsNull)
            {
                Error = true;
                Message = "Заказ с таким номером не были найдены.";
                return;
            }
            if (ProgramId.IsNull)
            {
                Error = true;
                Message = "Необходимую программу не удалось обнаружить.";
                return;
            }
            string FileExtention = null;
            string FilePath = null;

            using (var db = new SqlConnection("context connection=true"))
            {

                using (var q = new SqlCommand() { Connection = db, CommandType = CommandType.Text })
                {
                    q.CommandText =
                        @"SELECT P.[Расширение]
                        FROM [Жалюзи_заказы] AS O
	                        INNER JOIN [Жалюзи_бланки_связанные_программы] AS L
		                        ON O.[Код бланка]=L.[Код бланка]
	                        INNER JOIN [Жалюзи_связанные_программы] AS P
		                        ON P.[Код]=L.[Код программы]
                        WHERE 
	                        O.[Код]=@OrderId AND
                            P.[Код]=@ProgramId AND
                            O.[Статус]=0";

                    q.Parameters.Add("@OrderId", SqlDbType.Int).Direction = ParameterDirection.Input;
                    q.Parameters.Add("@ProgramId", SqlDbType.Int).Direction = ParameterDirection.Input;

                    q.Parameters["@OrderId"].Value = OrderId.Value;
                    q.Parameters["@ProgramId"].Value = ProgramId.Value;

                    db.Open();
                    FileExtention = (string)q.ExecuteScalar();
                    if (FileExtention == null)
                    {
                        Error = true;
                        Message = "Данный уже заказ нельзя редактировать.";
                        return;
                    }
                }

                using (var q = new SqlCommand() { Connection = db, CommandType = CommandType.Text })
                {
                    q.CommandText =
                        @"SELECT [Текстовое значение]
                        FROM [_Константы]
                        WHERE [Код]=67";
                    FilePath = (string)q.ExecuteScalar();
                    if (FilePath == null || !Directory.Exists(FilePath))
                    {
                        Error = true;
                        Message = "Катало хранения файлов не найден.";
                        return;
                    }
                }

            }

            var FullPath = Path.Combine(FilePath,
                String.Format("{0}-{1}.{2}", OrderId.Value, ProgramId.Value, FileExtention));
            if (File.Exists(FullPath))
                File.Delete(FullPath);

            if (!Value.IsNull)
            {
                var ms = new MemoryStream(Value.Value);
                var fs = File.OpenWrite(FullPath);
                ms.WriteTo(fs);
                fs.Flush();
                fs.Close();
            }
            Error = false;
            Message = "Все замечательно.";
        }

        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes GetTemplateLinkedFile(SqlInt32 OrderId, SqlInt32 ProgramId)
        {

            if (OrderId.IsNull || ProgramId.IsNull)
                return SqlBytes.Null;
            string FileExtention = null;
            string FilePath = null;
            int BlankId;

            using (var db = new SqlConnection("context connection=true"))
            {

                using (var q = new SqlCommand() { Connection = db, CommandType = CommandType.Text })
                {
                    q.CommandText =
                        @"SELECT 
                            P.[Расширение], 
                            O.[Код бланка]
                        FROM [Жалюзи_заказы] AS O
	                        INNER JOIN [Жалюзи_бланки_связанные_программы] AS L
		                        ON O.[Код бланка]=L.[Код бланка]
	                        INNER JOIN [Жалюзи_связанные_программы] AS P
		                        ON P.[Код]=L.[Код программы]
                        WHERE 
	                        O.[Код]=@OrderId AND
                            P.[Код]=@ProgramId";

                    q.Parameters.Add("@OrderId", SqlDbType.Int).Direction = ParameterDirection.Input;
                    q.Parameters.Add("@ProgramId", SqlDbType.Int).Direction = ParameterDirection.Input;

                    q.Parameters["@OrderId"].Value = OrderId.Value;
                    q.Parameters["@ProgramId"].Value = ProgramId.Value;

                    db.Open();
                    var result = q.ExecuteReader();
                    if (!result.Read())
                        return SqlBytes.Null;
                    FileExtention = (string)result["Расширение"];
                    BlankId = (int)result["Код бланка"];
                    result.Close();
                    if (FileExtention == null)
                        return SqlBytes.Null;
                }

                using (var q = new SqlCommand() { Connection = db, CommandType = CommandType.Text })
                {
                    q.CommandText =
                        @"SELECT [Текстовое значение]
                        FROM [_Константы]
                        WHERE [Код]=69";
                    FilePath = (string)q.ExecuteScalar();
                    if (FilePath == null || !Directory.Exists(FilePath))
                        return SqlBytes.Null;
                }

            }

            var FullPath = Path.Combine(FilePath,
                String.Format("{0}-{1}.{2}", BlankId, ProgramId.Value, FileExtention));
            if (!File.Exists(FullPath))
                return SqlBytes.Null;
            
            var fs = File.OpenRead(FullPath);
            var ms = new MemoryStream();
            ms.SetLength(fs.Length);
            fs.Read(ms.GetBuffer(), 0, (int)fs.Length);
            ms.Flush();
            fs.Close();
            return new SqlBytes(ms.GetBuffer());

        }

        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlXml GetContentLinkedFiles(SqlInt32 OrderId)
        {
            if (OrderId.IsNull)
                return SqlXml.Null;
            var ProgramList = new List<int>();
            using (var db = new SqlConnection("context connection=true"))
            {

                using (var q = new SqlCommand() {Connection = db, CommandType = CommandType.Text})
                {
                    q.CommandText =
                        @"SELECT L.[Код программы]
                        FROM [Жалюзи_заказы] AS O
	                        INNER JOIN [Жалюзи_бланки_связанные_программы] AS L
		                        ON L.[Код бланка]=O.[Код бланка]
                        WHERE 
	                        O.[Код]=@OrderId";

                    q.Parameters.Add("@OrderId", SqlDbType.Int).Direction = ParameterDirection.Input;
                    q.Parameters["@OrderId"].Value = OrderId.Value;

                    db.Open();
                    var result = q.ExecuteReader();
                    while(result.Read())
                        ProgramList.Add((int)result["Код программы"]);
                    result.Close();
                }
            }

            var doc = new XDocument(
                new XElement("root",
                    new XElement("Код_x0020_заказа", OrderId.Value)));

            foreach (var ProgramId in ProgramList)
            {
                var FileContent = GetLinkedFile(OrderId, ProgramId);
                if (FileContent.IsNull)
                    continue;

                var ms = new MemoryStream(FileContent.Value);
                List<XElement> content = null;
                switch (ProgramId)
                {
                    case 1:
                        content = GetMultiTexture(ms);
                        break;
                }
                if (content==null && content.Count<=0)
                    continue;
                doc.Root.Add(
                    new XElement("Содержимое", 
                        new XElement("Код_x0020_программы", ProgramId), 
                        content));

            }

            return new SqlXml(doc.CreateReader());
        }

        private static List<XElement> GetMultiTexture(MemoryStream ms)
        {
            var source = new IniConfigSource(ms);
            var rx = new Regex(@"\bSl_(\d*)_Lam_(\d*)\b");
            var lamels = source.Configs["LAMEL"];
            var param = source.Configs["PARAM"];

            var LamelList = lamels.GetKeys()
                .Select(z => new LamelItem()
                                {
                                    LayerNumber = int.Parse(rx.Replace(z, "$1")) + 1,
                                    LamelNumber = int.Parse(rx.Replace(z, "$2")),
                                    LengthLamel = 0.001*lamels.GetInt(z)
                                })
                .ToList();

            var Layers = LamelList.GroupBy(x => x.LayerNumber)
                .Select(x => new XElement(
                                 "Слой",
                                 new XElement("Позиция", x.Key),
                                 new XElement("Ламели", 
                                 x.Select(y => new XElement(
                                                   "Ламель",
                                                   new XAttribute("Номер", y.LamelNumber),
                                                   new XAttribute("Длина", y.LengthLamel))))))
                .ToList();
            return new List<XElement>()
                       {
                           new XElement("Параметры",
                                        new XElement("Количество_x0020_слоев", param.GetInt("KOLVOSLOY") + 1),
                                        new XElement("Ширина", 0.001*param.GetInt("SHIRINA")),
                                        new XElement("Высота", 0.001*param.GetInt("VYSOTA"))),
                           new XElement("Слои", Layers)
                       };
        }

    }
}
