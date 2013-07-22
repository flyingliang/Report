using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExportTag
{
    public static class Permissions
    {
        //need

        public static string 匯出學生類別含基本資料 { get { return "plugins.student.export.tag2.huangwc.v1"; } }

        public static bool 匯出學生類別含基本資料權限
        {
            get { return FISCA.Permission.UserAcl.Current[匯出學生類別含基本資料].Executable; }
        }

    }
}
