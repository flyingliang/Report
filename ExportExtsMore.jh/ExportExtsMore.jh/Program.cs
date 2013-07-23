using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FISCA;
using FISCA.Permission;
using FISCA.Presentation;
namespace ExportExtsMore.jh
{
    public class Program
    {
        [MainMethod]
        static public void Main()
        {
            MenuButton rbItemExport1 = K12.Presentation.NLDPanels.Student.RibbonBarItems["資料統計"]["報表"]["其它相關報表"];
            rbItemExport1["匯出自訂欄位(含基本資料,國中)"].Enable = Permissions.匯出自訂欄位含基本資料國中權限;
            rbItemExport1["匯出自訂欄位(含基本資料,國中)"].Click += delegate
            {
                if (K12.Presentation.NLDPanels.Student.SelectedSource.Count < 1)
                    System.Windows.Forms.MessageBox.Show("請選擇學生.");
                else
                {
                    SmartSchool.API.PlugIn.Export.Exporter exporter = new ExportExtsMore.ExtsMore(ExportExtsMore.ExtsMore.k12type.jh);
                    ExtsMoreUI wizard = new ExtsMoreUI(exporter.Text, exporter.Image);
                    exporter.InitializeExport(wizard);
                    wizard.ShowDialog();
                }
            };
            Catalog detail1 = RoleAclSource.Instance["學生"]["報表"];
            detail1.Add(new RibbonFeature(Permissions.匯出自訂欄位含基本資料國中, "匯出自訂欄位(含基本資料,國中)"));
        }
    }
}
