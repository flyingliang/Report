using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExportExtsMore
{
    public static class Permissions
    {
        public static string 匯出自訂欄位含基本資料高中 { get { return "plugins.student.export.exts.sh.huangwc.v1"; } }

        public static bool 匯出自訂欄位含基本資料高中權限
        {
            get { return FISCA.Permission.UserAcl.Current[匯出自訂欄位含基本資料高中].Executable; }
        }

    }
}
