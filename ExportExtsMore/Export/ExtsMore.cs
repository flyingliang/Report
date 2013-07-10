using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHSchool.Data;
using SmartSchool.API.PlugIn;
using FISCA.DSAUtil;
using System.Xml;
using System.Data;
namespace ExportExtsMore
{

    class ExportExtsMore : SmartSchool.API.PlugIn.Export.Exporter
    {
        public ExportExtsMore()
        {
            this.Image = null;
            this.Text = "匯出自訂欄位(多)";
        }
        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            //加入欲匯出之欄位資料
            wizard.ExportableFields.AddRange("學年度", "學期");

            wizard.ExportPackage += delegate(object sender, SmartSchool.API.PlugIn.Export.ExportPackageEventArgs e)
            {
                //取得學生清單
                if (e.List.Count < 1)
                    System.Windows.Forms.MessageBox.Show("no student seleted");

                DataTable dt = tool._Q.Select("SELECT * FROM student JOIN class ON student.id IN (" + string.Join(",", e.List) + ") AND student.ref_class_id = class.id");
                List<custStudentRecord> csrl = new List<custStudentRecord>();
                foreach (DataRow row in dt.Rows)
                {
                    csrl.Add(new custStudentRecord(row));
                }
                //整理填入資料
                foreach (custStudentRecord csr in csrl) //每一位學生
                {
                    RowData row = new RowData();
                    row.ID = csr.ID;
                    //對於每一個要匯出的欄位
                    foreach (string field in e.ExportFields)
                    {
                        if (wizard.ExportableFields.Contains(field))
                        {
                            string value = "";
                            switch (field)
                            {
                                case "學年度":
                                    
                                    break;
                                default:
                                    
                                    break;
                            }
                            row.Add(field, value);
                        }
                    }
                    e.Items.Add(row);
                }
            };
        }
        private int SortStudent(custStudentRecord x, custStudentRecord y)
        {

            string xx1 = x.Class != null ? x.Class.Name : "";
            string xx2 = x.SeatNo.HasValue ? x.SeatNo.Value.ToString().PadLeft(3, '0') : "000";
            string xx3 = xx1 + xx2;

            string yy1 = y.Class != null ? y.Class.Name : "";
            string yy2 = y.SeatNo.HasValue ? y.SeatNo.Value.ToString().PadLeft(3, '0') : "000";
            string yy3 = yy1 + yy2;

            return xx3.CompareTo(yy3);
        }
    }
}
