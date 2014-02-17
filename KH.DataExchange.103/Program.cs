using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartSchool.Customization.Data;
using System.IO;
using FISCA.DSAUtil;
using FISCA.Data;
using System.Data;
using Aspose.Cells;
using FISCA.Presentation.Controls;
using System.Windows.Forms;
using K12.Data;

namespace KH.DataExchange._103
{
    public class Program
    {
        [FISCA.MainMethod]
        public static void Main()
        {
            var button = FISCA.Presentation.MotherForm.RibbonBarItems["教務作業", "批次作業/檢視"]["其他"]["103高雄區資料交換"];
            System.ComponentModel.BackgroundWorker bkw = new System.ComponentModel.BackgroundWorker();
            bkw.WorkerReportsProgress = true;
            bkw.ProgressChanged += delegate(object sender, System.ComponentModel.ProgressChangedEventArgs e)
            {
                string message = e.ProgressPercentage == 100 ? "計算完成":"計算中...";
                FISCA.Presentation.MotherForm.SetStatusBarMessage("103高雄區資料交換"+message, e.ProgressPercentage);
            };
            bkw.DoWork += delegate
            {
                bkw.ReportProgress(1);
                QueryHelper _Q = new QueryHelper();
                DataTable dt_source, student;
                student = _Q.Select("select student.id from student left outer join class on student.ref_class_id=class.id where student.status = 1 and class.grade_year = 3");
                List<string> sids = new List<string>();
                foreach (DataRow row in student.Rows)
                {
                    sids.Add("" + row[0]);
                }
                bkw.ReportProgress(20);
                dt_source = _Q.Select(SqlString.MultivariateScore);
                List<UpdateRecordRecord> urrl = K12.Data.UpdateRecord.SelectByStudentIDs(sids);
                Dictionary<string,SemesterHistoryRecord> shrl = K12.Data.SemesterHistory.SelectByStudentIDs(sids).ToDictionary(x=>x.RefStudentID,x=>x);
                Dictionary<string,int> dsHas1year = new Dictionary<string,int>();
                foreach (UpdateRecordRecord urr in urrl)
                {
                    if ( !dsHas1year.ContainsKey(urr.Student.IDNumber) )
                        dsHas1year.Add(urr.Student.IDNumber,1);//預設為1
                    if ( urr.UpdateCode == "3") //轉入
                    {
                        int urrGrade = 0;
                        foreach (SemesterHistoryItem item in shrl[urr.StudentID].SemesterHistoryItems)//match schoolYear
                        {
                            if (item.SchoolYear == urr.SchoolYear && item.Semester == urr.Semester )
                                urrGrade = item.GradeYear ;
                        }
                        if (urrGrade == 3 || urrGrade == 9)
                            dsHas1year[urr.Student.IDNumber] = 0;
                        else if (urrGrade == 0)
                            ;
                    }
                }
                bkw.ReportProgress(60);
                List<string> l = new List<string> { "藝術與人文", "健康與體育", "綜合活動", "服務學習", "大功", "小功", "嘉獎", "大過", "小過", "警告", "幹部任期次數", "坐姿體前彎", "立定跳遠", "仰臥起坐", "心肺適能" };
                int index = 0;
                List<DataRow> deletedRows = new List<DataRow>();
                DataTable dt_tmp = dt_source.Clone();
                foreach (DataRow row in dt_source.Rows)
                {
                    row[3] = dsHas1year.ContainsKey(""+row[2])?""+dsHas1year[""+row[2]]:"unknow";
                    if ( l[index] != ""+row[4] ) //只取最後一筆(最新) , sql的left join已保證至少有一筆
                    {
                        dt_tmp.Rows.RemoveAt(dt_tmp.Rows.Count - 1);
                        dt_tmp.ImportRow(row);
                        continue;
                    }
                    dt_tmp.ImportRow(row);
                    index++;
                    if ( index >= l.Count )
                        index = 0 ;
                }
                bkw.ReportProgress(80);
                CompletedXls("103高雄區多元成績交換資料格式", dt_tmp, new Workbook());
                dt_tmp = _Q.Select(SqlString.IncentiveRecord);
                CompletedXls("103高雄區多元成績-獎懲記錄交換資料格式", dt_tmp, new Workbook());
                bkw.ReportProgress(100);
            };
            button.Click += delegate
            {
                bkw.RunWorkerAsync();
            };
        }
        public static void CompletedXls(string inputReportName, DataTable dt, Workbook inputXls)
        {

            string reportName = inputReportName;


            string path = Path.Combine(Application.StartupPath, "Reports");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".xls");

            Workbook wb = inputXls;

            wb.Worksheets[0].Cells.ImportDataTable(dt, true, "A1");
            wb.Worksheets[0].Name = inputReportName;
            wb.Worksheets[0].AutoFitColumns();
            if (File.Exists(path))
            {
                int i = 1;
                while (true)
                {
                    string newPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + (i++) + Path.GetExtension(path);
                    if (!File.Exists(newPath))
                    {
                        path = newPath;
                        break;
                    }
                }
            }

            try
            {
                wb.Save(path, Aspose.Cells.FileFormatType.Excel2003);
                System.Diagnostics.Process.Start(path);
            }
            catch
            {
                SaveFileDialog sd = new SaveFileDialog();
                sd.Title = "另存新檔";
                sd.FileName = reportName + ".xls";
                sd.Filter = "XLS檔案 (*.xls)|*.xls|所有檔案 (*.*)|*.*";
                if (sd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        wb.Save(sd.FileName, Aspose.Cells.FileFormatType.Excel2003);

                    }
                    catch
                    {
                        MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
        }
    }
}
