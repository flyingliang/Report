using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Customization.PlugIn;
using SmartSchool.Customization.PlugIn.Report;
using System.ComponentModel;
using Aspose.Words;
using System.IO;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using System.Windows.Forms;
using System.Xml;
using SmartSchool.Common;

namespace SemesterScoreReport
{
    class SemesterScoreReport
    {
        public static void RegistryFeature()
        {
            SemesterScoreReport semsScoreReport = new SemesterScoreReport();

            string reportName = "學期成績單(新制)";
            string path = "忠信高中";

            semsScoreReport.button = new ButtonAdapter();
            semsScoreReport.button.Text = reportName;
            semsScoreReport.button.Path = path;
            semsScoreReport.button.OnClick += new EventHandler(semsScoreReport.button_OnClick);
            StudentReport.AddReport(semsScoreReport.button);

            semsScoreReport.button2 = new ButtonAdapter();
            semsScoreReport.button2.Text = reportName;
            semsScoreReport.button2.Path = path;
            semsScoreReport.button2.OnClick += new EventHandler(semsScoreReport.button2_OnClick);
            ClassReport.AddReport(semsScoreReport.button2);
        }

        private ButtonAdapter button, button2;
        private BackgroundWorker _BGWSemesterScoreReport;
        private Dictionary<string, decimal> _degreeList = null; //等第List

        private enum Entity { Student, Class }

        public SemesterScoreReport()
        {
            SmartSchool.Customization.Data.SystemInformation.getField("Degree");
            SmartSchool.Customization.Data.SystemInformation.getField("Period");
            SmartSchool.Customization.Data.SystemInformation.getField("Absence");
        }

        #region Common Function

        public int SortBySemesterSubjectScoreInfo(SemesterSubjectScoreInfo a, SemesterSubjectScoreInfo b)
        {
            return SortBySubjectName(a.Subject, b.Subject);
        }

        private int SortBySubjectName(string a, string b)
        {
            string a1 = a.Length > 0 ? a.Substring(0, 1) : "";
            string b1 = b.Length > 0 ? b.Substring(0, 1) : "";
            #region 第一個字一樣的時候
            if (a1 == b1)
            {
                if (a.Length > 1 && b.Length > 1)
                    return SortBySubjectName(a.Substring(1), b.Substring(1));
                else
                    return a.Length.CompareTo(b.Length);
            }
            #endregion
            #region 第一個字不同，分別取得在設定順序中的數字，如果都不在設定順序中就用單純字串比較
            int ai = getIntForSubject(a1), bi = getIntForSubject(b1);
            if (ai > 0 || bi > 0)
                return ai.CompareTo(bi);
            else
                return a1.CompareTo(b1);
            #endregion
        }

        private int getIntForSubject(string a1)
        {
            List<string> list = new List<string>();
            list.AddRange(new string[] { "國", "英", "數", "物", "化", "生", "基", "歷", "地", "公", "文", "礎", "世" });

            int x = list.IndexOf(a1);
            if (x < 0)
                return list.Count;
            else
                return x;
        }

        private int SortByEntryName(string a, string b)
        {
            int ai = getIntForEntry(a), bi = getIntForEntry(b);
            if (ai > 0 || bi > 0)
                return ai.CompareTo(bi);
            else
                return a.CompareTo(b);
        }

        private int getIntForEntry(string a1)
        {
            List<string> list = new List<string>();
            list.AddRange(new string[] { "學業", "學業成績名次", "實習科目", "體育", "國防通識", "健康與護理", "德行", "學年德行成績" });

            int x = list.IndexOf(a1);
            if (x < 0)
                return list.Count;
            else
                return x;
        }

        //科目級別轉換
        private string GetNumber(int p)
        {
            List<string> list = new List<string>(new string[] { "", "Ⅰ", "Ⅱ", "Ⅲ", "Ⅳ", "Ⅴ", "Ⅵ", "Ⅶ", "Ⅷ", "Ⅸ", "Ⅹ" });

            if (p < list.Count)
                return list[p];
            else
                return "" + p;
        }

        //德行成績 -> 等第
        private string ParseLevel(decimal score)
        {
            if (_degreeList == null)
            {
                _degreeList = SmartSchool.Customization.Data.SystemInformation.Fields["Degree"] as Dictionary<string, decimal>;
            }

            foreach (string var in _degreeList.Keys)
            {
                if (_degreeList[var] <= score)
                    return var;
            }
            return "";
        }

        //報表產生完成後，儲存並且開啟
        private void Completed(string inputReportName, Document inputDoc)
        {
            string reportName = inputReportName;

            string path = Path.Combine(Application.StartupPath, "Reports");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".doc");

            Aspose.Words.Document doc = inputDoc;

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
                doc.Save(path, Aspose.Words.SaveFormat.Doc);
                System.Diagnostics.Process.Start(path);
            }
            catch
            {
                SaveFileDialog sd = new SaveFileDialog();
                sd.Title = "另存新檔";
                sd.FileName = reportName + ".doc";
                sd.Filter = "Word檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
                if (sd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        doc.Save(sd.FileName, Aspose.Words.SaveFormat.Doc);

                    }
                    catch
                    {
                        MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
        }

        //填入學生資料
        private void FillStudentData(AccessHelper helper, List<StudentRecord> students)
        {
            helper.StudentHelper.FillAttendance(students);
            helper.StudentHelper.FillReward(students);
            helper.StudentHelper.FillParentInfo(students);
            helper.StudentHelper.FillContactInfo(students);
            //helper.StudentHelper.FillSchoolYearEntryScore(true, students);
            //helper.StudentHelper.FillSchoolYearSubjectScore(true, students);
            helper.StudentHelper.FillSemesterSubjectScore(true, students);
            helper.StudentHelper.FillSemesterEntryScore(true, students);
            helper.StudentHelper.FillField("SemesterEntryClassRating", students); //學期分項班排名。
            helper.StudentHelper.FillField("補考標準", students);
            helper.StudentHelper.FillSchoolYearEntryScore(true, students);
            helper.StudentHelper.FillSemesterMoralScore(true, students);
        }

        #endregion

        private void button_OnClick(object sender, EventArgs e)
        {
            SemesterScoreReportFormNew form = new SemesterScoreReportFormNew();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _BGWSemesterScoreReport = new BackgroundWorker();
                _BGWSemesterScoreReport.WorkerReportsProgress = true;
                _BGWSemesterScoreReport.DoWork += new DoWorkEventHandler(_BGWSemesterScoreReport_DoWork);
                _BGWSemesterScoreReport.ProgressChanged += new ProgressChangedEventHandler(_BGWSemesterScoreReport_ProgressChanged);
                _BGWSemesterScoreReport.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BGWSemesterScoreReport_RunWorkerCompleted);
                _BGWSemesterScoreReport.RunWorkerAsync(new object[] { 
                    form.SchoolYear,
                    form.Semester,
                    form.UserDefinedType,
                    form.Template,
                    form.Receiver,
                    form.Address,
                    form.ResitSign,
                    form.RepeatSign,
                    Entity.Student});
            }
        }

        private void button2_OnClick(object sender, EventArgs e)
        {
            SemesterScoreReportFormNew form = new SemesterScoreReportFormNew();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _BGWSemesterScoreReport = new BackgroundWorker();
                _BGWSemesterScoreReport.WorkerReportsProgress = true;
                _BGWSemesterScoreReport.DoWork += new DoWorkEventHandler(_BGWSemesterScoreReport_DoWork);
                _BGWSemesterScoreReport.ProgressChanged += new ProgressChangedEventHandler(_BGWSemesterScoreReport_ProgressChanged);
                _BGWSemesterScoreReport.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BGWSemesterScoreReport_RunWorkerCompleted);
                _BGWSemesterScoreReport.RunWorkerAsync(new object[] {
                    form.SchoolYear,
                    form.Semester,
                    form.UserDefinedType,
                    form.Template,
                    form.Receiver,
                    form.Address,
                    form.ResitSign,
                    form.RepeatSign,
                    Entity.Class});
            }
        }

        private void _BGWSemesterScoreReport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button.SetBarMessage("學期成績單產生完成");
            Completed("學期成績單", (Document)e.Result);
        }

        private void _BGWSemesterScoreReport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            button.SetBarMessage("學期成績單產生中...", e.ProgressPercentage);
        }

        private void _BGWSemesterScoreReport_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] objectValue = (object[])e.Argument;

            int schoolyear = (int)objectValue[0];
            int semester = (int)objectValue[1];
            Dictionary<string, List<string>> userType = (Dictionary<string, List<string>>)objectValue[2];
            MemoryStream template = (MemoryStream)objectValue[3];
            int receiver = (int)objectValue[4];
            int address = (int)objectValue[5];
            string resitSign = (string)objectValue[6];
            string repeatSign = (string)objectValue[7];
            Entity entity = (Entity)objectValue[8];

            _BGWSemesterScoreReport.ReportProgress(0);

            #region 取得資料

            AccessHelper helper = new AccessHelper();

            List<StudentRecord> allStudent = new List<StudentRecord>();

            if (entity == Entity.Student)
            {
                allStudent = helper.StudentHelper.GetSelectedStudent();
                FillStudentData(helper, allStudent);
            }
            else if (entity == Entity.Class)
            {
                foreach (ClassRecord aClass in helper.ClassHelper.GetSelectedClass())
                {
                    FillStudentData(helper, aClass.Students);
                    allStudent.AddRange(aClass.Students);
                }
            }

            int currentStudent = 1;
            int totalStudent = allStudent.Count;

            #endregion

            #region 產生報表並填入資料

            Document doc = new Document();
            doc.Sections.Clear();

            foreach (StudentRecord eachStudent in allStudent)
            {
                Document eachStudentDoc = new Document(template, "", LoadFormat.Doc, "");

                Dictionary<string, object> mergeKeyValue = new Dictionary<string, object>();

                #region 學校基本資料
                mergeKeyValue.Add("學校名稱", SmartSchool.Customization.Data.SystemInformation.SchoolChineseName);
                mergeKeyValue.Add("學校地址", SmartSchool.Customization.Data.SystemInformation.Address);
                mergeKeyValue.Add("學校電話", SmartSchool.Customization.Data.SystemInformation.Telephone);
                #endregion

                #region 收件人姓名與地址
                if (receiver == 0)
                    mergeKeyValue.Add("收件人", eachStudent.ParentInfo.CustodianName);
                else if (receiver == 1)
                    mergeKeyValue.Add("收件人", eachStudent.ParentInfo.FatherName);
                else if (receiver == 2)
                    mergeKeyValue.Add("收件人", eachStudent.ParentInfo.MotherName);
                else if (receiver == 3)
                    mergeKeyValue.Add("收件人", eachStudent.StudentName);

                if (address == 0)
                {
                    mergeKeyValue.Add("收件人地址", eachStudent.ContactInfo.PermanentAddress.FullAddress);
                }
                else if (address == 1)
                {
                    mergeKeyValue.Add("收件人地址", eachStudent.ContactInfo.MailingAddress.FullAddress);
                }
                #endregion

                #region 學生基本資料
                mergeKeyValue.Add("學年度", schoolyear.ToString());
                mergeKeyValue.Add("學期", semester.ToString());
                mergeKeyValue.Add("班級科別名稱", (eachStudent.RefClass != null) ? eachStudent.RefClass.Department : "");
                mergeKeyValue.Add("班級", (eachStudent.RefClass != null) ? eachStudent.RefClass.ClassName : "");
                mergeKeyValue.Add("學號", eachStudent.StudentNumber);
                mergeKeyValue.Add("姓名", eachStudent.StudentName);
                mergeKeyValue.Add("座號", eachStudent.SeatNo);
                #endregion

                #region 導師與評語
                if (eachStudent.RefClass != null && eachStudent.RefClass.RefTeacher != null)
                {
                    mergeKeyValue.Add("班導師", eachStudent.RefClass.RefTeacher.TeacherName);
                }

                mergeKeyValue.Add("導師評語", "");
                if (eachStudent.SemesterMoralScoreList.Count > 0)
                {
                    foreach (SemesterMoralScoreInfo info in eachStudent.SemesterMoralScoreList)
                    {
                        if (info.SchoolYear == schoolyear && info.Semester == semester)
                            mergeKeyValue["導師評語"] = info.SupervisedByComment;
                    }
                }
                #endregion

                #region 獎懲紀錄
                int awardA = 0;
                int awardB = 0;
                int awardC = 0;
                int faultA = 0;
                int faultB = 0;
                int faultC = 0;
                bool ua = false; //留校察看
                foreach (RewardInfo info in eachStudent.RewardList)
                {
                    if (info.SchoolYear == schoolyear && info.Semester == semester)
                    {
                        awardA += info.AwardA;
                        awardB += info.AwardB;
                        awardC += info.AwardC;

                        if (!info.Cleared)
                        {
                            faultA += info.FaultA;
                            faultB += info.FaultB;
                            faultC += info.FaultC;
                        }

                        if (info.UltimateAdmonition)
                            ua = true;
                    }
                }

                StringBuilder disciplineBuilder = new StringBuilder("");
                if (awardA > 0) disciplineBuilder.Append("大功 ").Append(awardA).Append(",");
                if (awardB > 0) disciplineBuilder.Append("小功 ").Append(awardB).Append(",");
                if (awardC > 0) disciplineBuilder.Append("嘉獎 ").Append(awardC).Append(",");
                if (faultA > 0) disciplineBuilder.Append("大過 ").Append(faultA).Append(",");
                if (faultB > 0) disciplineBuilder.Append("小過 ").Append(faultB).Append(",");
                if (faultC > 0) disciplineBuilder.Append("警告 ").Append(faultC).Append(",");
                if (ua) disciplineBuilder.Append("留校察看");
                string disciplineString = disciplineBuilder.ToString();
                if (disciplineString.EndsWith(",")) disciplineString = disciplineString.Substring(0, disciplineString.Length - 1);
                if (string.IsNullOrEmpty(disciplineString)) disciplineString = "無獎懲紀錄";
                mergeKeyValue.Add("獎懲紀錄", disciplineString);

                #endregion

                #region 科目成績

                Dictionary<SemesterSubjectScoreInfo, Dictionary<string, string>> subjectScore = new Dictionary<SemesterSubjectScoreInfo, Dictionary<string, string>>();
                int thisSemesterTotalCredit = 0;
                int thisSchoolYearTotalCredit = 0;
                int beforeSemesterTotalCredit = 0;

                Dictionary<int, decimal> resitStandard = eachStudent.Fields["補考標準"] as Dictionary<int, decimal>;

                foreach (SemesterSubjectScoreInfo info in eachStudent.SemesterSubjectScoreList)
                {
                    if (info.SchoolYear == schoolyear && info.Semester == semester)
                    {
                        if (!subjectScore.ContainsKey(info))
                            subjectScore.Add(info, new Dictionary<string, string>());

                        subjectScore[info].Add("科目", info.Subject);
                        subjectScore[info].Add("級別", (string.IsNullOrEmpty(info.Level) ? "" : GetNumber(int.Parse(info.Level))));
                        subjectScore[info].Add("學分", info.Credit.ToString());
                        subjectScore[info].Add("分數", info.Score.ToString());
                        subjectScore[info].Add("必修", ((info.Require) ? "必" : "選"));

                        //判斷補考或重修 
                        if (!info.Pass)
                        {
                            if (info.Score >= resitStandard[info.GradeYear])
                                subjectScore[info].Add("補考", "是");
                            else
                                subjectScore[info].Add("補考", "否");
                        }
                    }

                    if (info.Pass && info.Detail.GetAttribute("不計學分") != "是")
                    {
                        if (info.SchoolYear == schoolyear && info.Semester == semester)
                            thisSemesterTotalCredit += info.Credit;

                        if (info.SchoolYear < schoolyear)
                            beforeSemesterTotalCredit += info.Credit;
                        else if (info.SchoolYear == schoolyear && info.Semester <= semester)
                            beforeSemesterTotalCredit += info.Credit;

                        if (info.SchoolYear == schoolyear)
                            thisSchoolYearTotalCredit += info.Credit;
                    }
                }


                mergeKeyValue.Add("科目", new object[] { subjectScore, resitSign, repeatSign });

                #endregion

                #region 分項成績

                Dictionary<string, Dictionary<string, string>> entryScore = new Dictionary<string, Dictionary<string, string>>();
                decimal semesterMoralScore = 0;
                decimal schoolyearMoralScore = 0;

                foreach (SemesterEntryScoreInfo info in eachStudent.SemesterEntryScoreList)
                {
                    if (info.SchoolYear == schoolyear && info.Semester == semester)
                    {
                        string entry = info.Entry;
                        decimal score = info.Score;
                        if (decimal.Truncate(info.Score) == info.Score)
                            score = decimal.Truncate(info.Score);

                        if (entry == "德行")
                        {
                            semesterMoralScore = score;
                        }
                        else
                        {
                            if (!entryScore.ContainsKey(entry))
                                entryScore.Add(entry, new Dictionary<string, string>());
                            entryScore[entry].Add("分數", score.ToString());
                        }
                    }
                }

                //如果是下學期，就多列印學年德行成績。
                if (semester == 2)
                {
                    foreach (SchoolYearEntryScoreInfo info in eachStudent.SchoolYearEntryScoreList)
                    {
                        if (info.SchoolYear == schoolyear)
                        {
                            string entry = info.Entry;
                            decimal score = info.Score;
                            if (decimal.Truncate(info.Score) == info.Score)
                                score = decimal.Truncate(info.Score);

                            if (entry == "德行")
                            {
                                schoolyearMoralScore = score;
                                //entryScore.Add("學年德行成績", new Dictionary<string, string>());
                                //entryScore["學年德行成績"].Add("分數", info.Score.ToString());
                            }
                        }
                    }
                }

                SemesterEntryRatingNew rating = new SemesterEntryRatingNew(eachStudent);

                Dictionary<string, string> totalCredit = new Dictionary<string, string>();

                //modify by huangwc , 2013/6/27
                //學業成績名次小於等於20顯示名次 , 其他顯示"N/A"
                int tmp_rating;
                bool tag_rating = int.TryParse(rating.GetPlace(schoolyear, semester),out tmp_rating);
                totalCredit.Add("學業成績名次", tag_rating?(tmp_rating <= 20 ? tmp_rating.ToString() : "N/A"):"N/A");
                //totalCredit.Add("學業成績名次", rating.GetPlace(schoolyear, semester));

                totalCredit.Add("學期取得學分數", thisSemesterTotalCredit.ToString());
                totalCredit.Add("累計取得學分數", beforeSemesterTotalCredit.ToString());

                mergeKeyValue.Add("分項", new object[] { entryScore, totalCredit});
                //mergeKeyValue.Add("德行成績", semesterMoralScore + " / " + ParseLevel(semesterMoralScore));
                //mergeKeyValue.Add("學年德行成績", schoolyearMoralScore + " / " + ParseLevel(schoolyearMoralScore));

                #endregion

                #region 德行文字評量
                XmlElement textScoreXml = null;
                foreach (SemesterMoralScoreInfo info in eachStudent.SemesterMoralScoreList)
                {
                    if (info.SchoolYear == schoolyear && info.Semester == semester)
                    {
                        textScoreXml = info.Detail;
                    }
                }

                foreach (string var in new string[] { "誠實地面對自己", "誠實地對待他人", "自我尊重", "尊重他人", "承諾", "責任", "榮譽", "感謝" })
                {
                    mergeKeyValue.Add(var, "");
                    if (textScoreXml != null)
                    {
                        XmlElement face = (XmlElement)textScoreXml.SelectSingleNode("TextScore/Morality[@Face='" + var + "']");
                        if (face != null)
                            mergeKeyValue[var] = face.InnerText;
                    }
                }

                #endregion

                #region 出缺席紀錄

                Dictionary<string, int> absenceInfo = new Dictionary<string, int>();

                foreach (string periodType in userType.Keys)
                {
                    foreach (string absence in userType[periodType])
                    {
                        if (!absenceInfo.ContainsKey(absence))
                            absenceInfo.Add(absence, 0);
                    }
                }

                foreach (AttendanceInfo info in eachStudent.AttendanceList)
                {
                    if (info.SchoolYear == schoolyear && info.Semester == semester)
                    {
                        if (absenceInfo.ContainsKey(info.Absence))
                            absenceInfo[info.Absence]++;
                    }
                }

                StringBuilder absenceBuilder = new StringBuilder("");
                foreach (string var in absenceInfo.Keys)
                {
                    if (absenceInfo[var] > 0)
                        absenceBuilder.Append(var).Append(" ").Append(absenceInfo[var]).Append(",");
                }
                string absenceString = absenceBuilder.ToString();
                if (absenceString.EndsWith(",")) absenceString = absenceString.Substring(0, absenceString.Length - 1);
                if (string.IsNullOrEmpty(absenceString)) absenceString = "全勤";
                mergeKeyValue.Add("出缺席紀錄", absenceString);

                #endregion

                eachStudentDoc.MailMerge.MergeField += new Aspose.Words.Reporting.MergeFieldEventHandler(MailMerge_MergeField);
                eachStudentDoc.MailMerge.RemoveEmptyParagraphs = true;

                List<string> keys = new List<string>();
                List<object> values = new List<object>();

                foreach (string key in mergeKeyValue.Keys)
                {
                    keys.Add(key);
                    values.Add(mergeKeyValue[key]);
                }
                eachStudentDoc.MailMerge.Execute(keys.ToArray(), values.ToArray());

                doc.Sections.Add(doc.ImportNode(eachStudentDoc.Sections[0], true));

                //回報進度
                _BGWSemesterScoreReport.ReportProgress((int)(currentStudent++ * 100.0 / totalStudent));
            }

            #endregion

            e.Result = doc;
        }

        private void MailMerge_MergeField(object sender, Aspose.Words.Reporting.MergeFieldEventArgs e)
        {
            #region 科目成績

            if (e.FieldName == "科目")
            {
                object[] objectValue = (object[])e.FieldValue;
                Dictionary<SemesterSubjectScoreInfo, Dictionary<string, string>> subjectScore = (Dictionary<SemesterSubjectScoreInfo, Dictionary<string, string>>)objectValue[0];
                string resitSign = (string)objectValue[1];
                string repeatSign = (string)objectValue[2];

                DocumentBuilder builder = new DocumentBuilder(e.Document);
                builder.MoveToField(e.Field, false);

                Table SSTable = ((Row)((Cell)builder.CurrentParagraph.ParentNode).ParentRow).ParentTable;

                int SSRowNumber = SSTable.Rows.Count - 1;
                int SSTableRowIndex = 1;
                int SSTableColIndex = 0;

                List<SemesterSubjectScoreInfo> sortList = new List<SemesterSubjectScoreInfo>();
                sortList.AddRange(subjectScore.Keys);
                sortList.Sort(SortBySemesterSubjectScoreInfo);

                foreach (SemesterSubjectScoreInfo info in sortList)
                {
                    if (SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex] == null)
                    {
                        throw new ArgumentException("科目成績表格不足容下所有科目成績。");
                    }

                    Runs runs = SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex].Paragraphs[0].Runs;
                    runs.Add(new Run(e.Document));
                    runs[runs.Count - 1].Text = subjectScore[info]["科目"] + ((string.IsNullOrEmpty(subjectScore[info]["級別"])) ? "" : (" (" + subjectScore[info]["級別"] + ")"));
                    runs[runs.Count - 1].Font.Size = 10;
                    runs[runs.Count - 1].Font.Name = "新細明體";

                    SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 1].Paragraphs[0].Runs.Add(new Run(e.Document, subjectScore[info]["必修"] + subjectScore[info]["學分"]));
                    SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 1].Paragraphs[0].Runs[0].Font.Size = 10;
                    SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 2].Paragraphs[0].Runs.Add(new Run(e.Document, subjectScore[info]["分數"]));
                    SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 2].Paragraphs[0].Runs[0].Font.Size = 10;

                    int colshift = 0;
                    string re = "";
                    if (subjectScore[info].ContainsKey("補考"))
                    {
                        if (subjectScore[info]["補考"] == "是")
                            re = resitSign;
                        else if (subjectScore[info]["補考"] == "否")
                            re = repeatSign;
                    }

                    SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 3 + colshift].Paragraphs[0].Runs.Add(new Run(e.Document, re));
                    SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 3 + colshift].Paragraphs[0].Runs[0].Font.Size = 10;

                    SSTableRowIndex++;
                    if (SSTableRowIndex > SSRowNumber)
                    {
                        SSTableRowIndex = 1;
                        SSTableColIndex += 4;
                    }
                }

                e.Text = string.Empty;
            }

            #endregion

            #region 分項成績

            if (e.FieldName == "分項")
            {
                object[] objectValue = (object[])e.FieldValue;
                Dictionary<string, Dictionary<string, string>> entryScore = (Dictionary<string, Dictionary<string, string>>)objectValue[0];
                Dictionary<string, string> totalCredit = (Dictionary<string, string>)objectValue[1];

                DocumentBuilder builder = new DocumentBuilder(e.Document);
                builder.MoveToField(e.Field, false);

                Table ESTable = ((Row)((Cell)builder.CurrentParagraph.ParentNode).ParentRow).ParentTable;

                int ESRowNumber = ESTable.Rows.Count - 1;
                int ESTableRowIndex = 1;
                int ESTableColIndex = 0;

                List<string> sortList = new List<string>();
                sortList.AddRange(entryScore.Keys);
                sortList.Sort(SortByEntryName);

                foreach (string entry in sortList)
                {
                    string semesterDegree = "";
                    if (entry == "德行" || entry == "學年德行成績")
                        continue;

                    Runs runs = ESTable.Rows[ESTableRowIndex].Cells[ESTableColIndex].Paragraphs[0].Runs;

                    runs.Add(new Run(e.Document, ToDisplayName(entry)));
                    runs[runs.Count - 1].Font.Size = 10;
                    ESTable.Rows[ESTableRowIndex].Cells[ESTableColIndex + 1].Paragraphs[0].Runs.Add(new Run(e.Document, entryScore[entry]["分數"] + semesterDegree));
                    ESTable.Rows[ESTableRowIndex].Cells[ESTableColIndex + 1].Paragraphs[0].Runs[0].Font.Size = 10;

                    ESTableRowIndex++;
                    if (ESTableRowIndex > ESRowNumber)
                    {
                        ESTableRowIndex = 1;
                        ESTableColIndex += 2;
                    }
                }

                foreach (string key in totalCredit.Keys)
                {
                    Runs runs = ESTable.Rows[ESTableRowIndex].Cells[ESTableColIndex].Paragraphs[0].Runs;

                    runs.Add(new Run(e.Document, key));
                    runs[runs.Count - 1].Font.Size = 10;
                    ESTable.Rows[ESTableRowIndex].Cells[ESTableColIndex + 1].Paragraphs[0].Runs.Add(new Run(e.Document, totalCredit[key]));
                    ESTable.Rows[ESTableRowIndex].Cells[ESTableColIndex + 1].Paragraphs[0].Runs[0].Font.Size = 10;

                    ESTableRowIndex++;
                    if (ESTableRowIndex > ESRowNumber)
                    {
                        ESTableRowIndex = 1;
                        ESTableColIndex += 2;
                    }
                }

                e.Text = string.Empty;
            }

            #endregion
        }

        private static string ToDisplayName(string entry)
        {
            switch (entry)
            {
                case "學業":
                    return "學期學業成績";
                case "德行":
                    return "學期德行成績";
                default:
                    return entry;
            }
        }
    }

    /// <summary>
    /// 只處理學業成績的排名。
    /// </summary>
    class SemesterEntryRatingNew
    {
        private XmlElement _sems_ratings = null;

        public SemesterEntryRatingNew(StudentRecord student)
        {
            if (student.Fields.ContainsKey("SemesterEntryClassRating"))
                _sems_ratings = student.Fields["SemesterEntryClassRating"] as XmlElement;
        }

        public string GetPlace(int schoolYear, int semester)
        {
            if (_sems_ratings == null) return string.Empty;

            string path = string.Format("SemesterEntryScore[SchoolYear='{0}' and Semester='{1}']/ClassRating/Rating/Item[@分項='學業']/@排名", schoolYear, semester);
            XmlNode result = _sems_ratings.SelectSingleNode(path);

            if (result != null)
                return result.InnerText;
            else
                return string.Empty;
        }
    }
}
