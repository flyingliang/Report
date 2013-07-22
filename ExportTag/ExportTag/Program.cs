using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.Presentation;
using FISCA;
using FISCA.Permission;
namespace ExportTag
{
    public class Program
    {
        [MainMethod]
        static public void Main()
        {
            MenuButton rbItemExport1 = K12.Presentation.NLDPanels.Student.RibbonBarItems["資料統計"]["報表"]["學籍相關報表"];
            rbItemExport1["匯出學生類別(含基本資料)"].Enable = Permissions.匯出學生類別含基本資料權限;
            rbItemExport1["匯出學生類別(含基本資料)"].Click += delegate
            {
                if (K12.Presentation.NLDPanels.Student.SelectedSource.Count < 1)
                    System.Windows.Forms.MessageBox.Show("請選擇學生.");
                else
                {
                    SmartSchool.API.PlugIn.Export.Exporter exporter = new ExportTag();
                    TagUI wizard = new TagUI(exporter.Text, exporter.Image);
                    exporter.InitializeExport(wizard);
                    wizard.ShowDialog();
                }
            };
            Catalog detail1 = RoleAclSource.Instance["學生"]["報表"];
            detail1.Add(new RibbonFeature(Permissions.匯出學生類別含基本資料, "匯出學生類別(含基本資料)"));
        }
    }
}
