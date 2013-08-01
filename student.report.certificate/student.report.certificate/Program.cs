using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA;
using FISCA.Presentation;
using FISCA.Permission;

namespace plugins.student.report.certificate
{
    public class Program
    {
        [MainMethod()]
        static public void Main()
        {
            RibbonBarItem item = FISCA.Presentation.MotherForm.RibbonBarItems["學生", "其它"];
            //Print["報表"]["社團相關報表"]["社團幹部證明單"].Enable = 社團幹部證明單權限;
            //Print["報表"]["社團相關報表"]["社團幹部證明單"].Click += delegate
            item["證明單測試"].Click += delegate
            {
                new MainForm().ShowDialog();
            };
            //Catalog detail1 = RoleAclSource.Instance["學生"]["報表"];
            //detail1.Add(new RibbonFeature(社團幹部證明單, "社團幹部證明單"));
        }

        //static public string 社團幹部證明單 { get { return "K12.Club.Universal.CadreProveReport.cs"; } }
        //static public bool 社團幹部證明單權限
        //{
        //    get
        //    {
        //        return FISCA.Permission.UserAcl.Current[社團幹部證明單].Executable;
        //    }
        //}
    }
}
