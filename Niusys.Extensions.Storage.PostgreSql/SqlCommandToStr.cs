using System.Collections;
using System.Text;

namespace Niusys.Extensions.Storage.PostgreSql
{

    static internal class SqlCommandToStr
    {
        //public static String ParameterValueForSQL(this SqlParameter sp)
        //{
        //    if (sp.Value == null)
        //    {
        //        //it should throw an error if an parameter had not set value
        //        //but here just make it through
        //        return "Nothing";
        //    }
        //    if (object.ReferenceEquals(sp.Value, DBNull.Value))
        //    {
        //        return "NULL";
        //    }
        //    String retval = "";

        //    switch (sp.SqlDbType)
        //    {
        //        case SqlDbType.Char:
        //        case SqlDbType.NChar:
        //        case SqlDbType.NText:
        //        case SqlDbType.NVarChar:
        //        case SqlDbType.Text:
        //        case SqlDbType.Time:
        //        case SqlDbType.VarChar:
        //        case SqlDbType.Xml:
        //        case SqlDbType.Date:
        //        case SqlDbType.DateTime:
        //        case SqlDbType.DateTime2:
        //        case SqlDbType.DateTimeOffset:
        //            retval = "'" + sp.Value.ToString().Replace("'", "''") + "'";
        //            break;

        //        case SqlDbType.Bit:
        //            if (sp.Value is bool)
        //            {
        //                if ((bool)sp.Value)
        //                {
        //                    retval = "1";
        //                }
        //                else
        //                {
        //                    retval = "0";
        //                }
        //            }
        //            else
        //                return sp.Value.ToString();
        //            break;
        //        case SqlDbType.Timestamp:
        //            retval = "0x" + BitConverter.ToString(sp.Value as byte[]).Replace("-", "");
        //            break;
        //        default:
        //            retval = sp.Value.ToString().Replace("'", "''");
        //            break;
        //    }

        //    return retval;
        //}

        public static string CommandAsSql(string sqlString, object parms)
        {
            StringBuilder sql = new StringBuilder();
            if (parms != null)
            {
                if (parms is IEnumerable)
                {
                    foreach (var item in (IEnumerable)parms)
                    {
                        sql.AppendLine(CommandAsSqlForSingleObject(sqlString, item));
                    }
                }
                else
                {
                    sql.AppendLine(CommandAsSqlForSingleObject(sqlString, parms));
                }
            }
            return sql.ToString();
        }

        public static string CommandAsSqlForSingleObject(string sqlString, object parms)
        {
            if (parms != null)
            {
                var sqlScript = new string(sqlString.ToCharArray());
                foreach (var parm in parms.GetType().GetProperties())
                {
                    sqlScript.Replace("@" + parm.Name, GetFormatedSqlValue(parm.GetValue(parms)));
                }
                return sqlScript;
            }
            return string.Empty;
        }


        private static string GetFormatedSqlValue(object objValue)
        {
            if (objValue == null)
                return string.Empty;

            switch (objValue.GetType().FullName)
            {
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                    return objValue.ToString();
                case "System.String":
                case "System.Guid":
                default:
                    return string.Format("'{0}'", objValue.ToString());
            }
        }
    }
}
