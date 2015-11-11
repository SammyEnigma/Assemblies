using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;

namespace RegExExtention
{
    public class RegExExtentionClass
    {
        public class MatchContent
        {
            public int MatchNumber;
            public string GroupName;
            public int CaptureNumber;
            public string Value;
        }

        [SqlFunction(DataAccess = DataAccessKind.Read, FillRowMethodName = "GetRegExDynamicFill")]
        public static IEnumerable Match(SqlString pattern, SqlString subject)
        {
            var list = new List<MatchContent>();
            var regex = new Regex(pattern.Value);
            var match = regex.Match(subject.Value);
            var groupNames = regex.GetGroupNames();
            var num = 0;
            while (match.Success)
            {
                num++;
                var array = groupNames;
                foreach (var text in array)
                {
                    list.AddRange(match.Groups[text].Captures
                        .Cast<Capture>()
                        .Select(capture =>
                            new MatchContent
                            {
                                MatchNumber = num,
                                GroupName = text,
                                CaptureNumber = capture.Index,
                                Value = capture.Value
                            }));
                }
                match = match.NextMatch();
            }
            return list;
        }

        public static void GetRegExDynamicFill(object o, out SqlInt32 matchNumber, 
            out SqlString groupName, out SqlInt32 captureNumber, out SqlString value)
        {
            var matchContent = (MatchContent) o;
            matchNumber = matchContent.MatchNumber;
            groupName = matchContent.GroupName;
            captureNumber = matchContent.CaptureNumber;
            value = matchContent.Value;
        }

        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBoolean IsMatch(SqlString pattern, SqlString subject)
        {
            var match = Regex.Match(subject.Value, pattern.Value);
            return new SqlBoolean(match.Success);
        }

        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString Replace(SqlString pattern, SqlString subject, SqlString replace)
        {
            var data = Regex.Replace(subject.Value, pattern.Value, replace.Value);
            return new SqlString(data);
        }
    }
}
