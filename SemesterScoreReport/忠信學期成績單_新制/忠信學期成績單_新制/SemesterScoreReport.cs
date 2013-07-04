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

            string reportName = "�Ǵ����Z��(�s��)";
            string path = "���H����";

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
        private Dictionary<string, decimal> _degreeList = null; //����List

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
            #region �Ĥ@�Ӧr�@�˪��ɭ�
            if (a1 == b1)
            {
                if (a.Length > 1 && b.Length > 1)
                    return SortBySubjectName(a.Substring(1), b.Substring(1));
                else
                    return a.Length.CompareTo(b.Length);
            }
            #endregion
            #region �Ĥ@�Ӧr���P�A���O���o�b�]�w���Ǥ����Ʀr�A�p�G�����b�]�w���Ǥ��N�γ�¦r����
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
            list.AddRange(new string[] { "��", "�^", "��", "��", "��", "��", "��", "��", "�a", "��", "��", "¦", "�@" });

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
            list.AddRange(new string[] { "�Ƿ~", "�Ƿ~���Z�W��", "��߬��", "��|", "�꨾�q��", "���d�P�@�z", "�w��", "�Ǧ~�w�榨�Z" });

            int x = list.IndexOf(a1);
            if (x < 0)
                return list.Count;
            else
                return x;
        }

        //��دŧO�ഫ
        private string GetNumber(int p)
        {
            List<string> list = new List<string>(new string[] { "", "��", "��", "��", "��", "��", "��", "��", "��", "��", "��" });

            if (p < list.Count)
                return list[p];
            else
                return "" + p;
        }

        //�w�榨�Z -> ����
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

        //�����ͧ�����A�x�s�åB�}��
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
                sd.Title = "�t�s�s��";
                sd.FileName = reportName + ".doc";
                sd.Filter = "Word�ɮ� (*.doc)|*.doc|�Ҧ��ɮ� (*.*)|*.*";
                if (sd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        doc.Save(sd.FileName, Aspose.Words.SaveFormat.Doc);

                    }
                    catch
                    {
                        MsgBox.Show("���w���|�L�k�s���C", "�إ��ɮץ���", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
        }

        //��J�ǥ͸��
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
            helper.StudentHelper.FillField("SemesterEntryClassRating", students); //�Ǵ������Z�ƦW�C
            helper.StudentHelper.FillField("�ɦҼз�", students);
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
            button.SetBarMessage("�Ǵ����Z�沣�ͧ���");
            Completed("�Ǵ����Z��", (Document)e.Result);
        }

        private void _BGWSemesterScoreReport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            button.SetBarMessage("�Ǵ����Z�沣�ͤ�...", e.ProgressPercentage);
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

            #region ���o���

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

            #region ���ͳ���ö�J���

            Document doc = new Document();
            doc.Sections.Clear();

            foreach (StudentRecord eachStudent in allStudent)
            {
                Document eachStudentDoc = new Document(template, "", LoadFormat.Doc, "");

                Dictionary<string, object> mergeKeyValue = new Dictionary<string, object>();

                #region �Ǯհ򥻸��
                mergeKeyValue.Add("�ǮզW��", SmartSchool.Customization.Data.SystemInformation.SchoolChineseName);
                mergeKeyValue.Add("�Ǯզa�}", SmartSchool.Customization.Data.SystemInformation.Address);
                mergeKeyValue.Add("�Ǯչq��", SmartSchool.Customization.Data.SystemInformation.Telephone);
                #endregion

                #region ����H�m�W�P�a�}
                if (receiver == 0)
                    mergeKeyValue.Add("����H", eachStudent.ParentInfo.CustodianName);
                else if (receiver == 1)
                    mergeKeyValue.Add("����H", eachStudent.ParentInfo.FatherName);
                else if (receiver == 2)
                    mergeKeyValue.Add("����H", eachStudent.ParentInfo.MotherName);
                else if (receiver == 3)
                    mergeKeyValue.Add("����H", eachStudent.StudentName);

                if (address == 0)
                {
                    mergeKeyValue.Add("����H�a�}", eachStudent.ContactInfo.PermanentAddress.FullAddress);
                }
                else if (address == 1)
                {
                    mergeKeyValue.Add("����H�a�}", eachStudent.ContactInfo.MailingAddress.FullAddress);
                }
                #endregion

                #region �ǥͰ򥻸��
                mergeKeyValue.Add("�Ǧ~��", schoolyear.ToString());
                mergeKeyValue.Add("�Ǵ�", semester.ToString());
                mergeKeyValue.Add("�Z�Ŭ�O�W��", (eachStudent.RefClass != null) ? eachStudent.RefClass.Department : "");
                mergeKeyValue.Add("�Z��", (eachStudent.RefClass != null) ? eachStudent.RefClass.ClassName : "");
                mergeKeyValue.Add("�Ǹ�", eachStudent.StudentNumber);
                mergeKeyValue.Add("�m�W", eachStudent.StudentName);
                mergeKeyValue.Add("�y��", eachStudent.SeatNo);
                #endregion

                #region �ɮv�P���y
                if (eachStudent.RefClass != null && eachStudent.RefClass.RefTeacher != null)
                {
                    mergeKeyValue.Add("�Z�ɮv", eachStudent.RefClass.RefTeacher.TeacherName);
                }

                mergeKeyValue.Add("�ɮv���y", "");
                if (eachStudent.SemesterMoralScoreList.Count > 0)
                {
                    foreach (SemesterMoralScoreInfo info in eachStudent.SemesterMoralScoreList)
                    {
                        if (info.SchoolYear == schoolyear && info.Semester == semester)
                            mergeKeyValue["�ɮv���y"] = info.SupervisedByComment;
                    }
                }
                #endregion

                #region ���g����
                int awardA = 0;
                int awardB = 0;
                int awardC = 0;
                int faultA = 0;
                int faultB = 0;
                int faultC = 0;
                bool ua = false; //�d�չ��
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
                if (awardA > 0) disciplineBuilder.Append("�j�\ ").Append(awardA).Append(",");
                if (awardB > 0) disciplineBuilder.Append("�p�\ ").Append(awardB).Append(",");
                if (awardC > 0) disciplineBuilder.Append("�ż� ").Append(awardC).Append(",");
                if (faultA > 0) disciplineBuilder.Append("�j�L ").Append(faultA).Append(",");
                if (faultB > 0) disciplineBuilder.Append("�p�L ").Append(faultB).Append(",");
                if (faultC > 0) disciplineBuilder.Append("ĵ�i ").Append(faultC).Append(",");
                if (ua) disciplineBuilder.Append("�d�չ��");
                string disciplineString = disciplineBuilder.ToString();
                if (disciplineString.EndsWith(",")) disciplineString = disciplineString.Substring(0, disciplineString.Length - 1);
                if (string.IsNullOrEmpty(disciplineString)) disciplineString = "�L���g����";
                mergeKeyValue.Add("���g����", disciplineString);

                #endregion

                #region ��ئ��Z

                Dictionary<SemesterSubjectScoreInfo, Dictionary<string, string>> subjectScore = new Dictionary<SemesterSubjectScoreInfo, Dictionary<string, string>>();
                int thisSemesterTotalCredit = 0;
                int thisSchoolYearTotalCredit = 0;
                int beforeSemesterTotalCredit = 0;

                Dictionary<int, decimal> resitStandard = eachStudent.Fields["�ɦҼз�"] as Dictionary<int, decimal>;

                foreach (SemesterSubjectScoreInfo info in eachStudent.SemesterSubjectScoreList)
                {
                    if (info.SchoolYear == schoolyear && info.Semester == semester)
                    {
                        if (!subjectScore.ContainsKey(info))
                            subjectScore.Add(info, new Dictionary<string, string>());

                        subjectScore[info].Add("���", info.Subject);
                        subjectScore[info].Add("�ŧO", (string.IsNullOrEmpty(info.Level) ? "" : GetNumber(int.Parse(info.Level))));
                        subjectScore[info].Add("�Ǥ�", info.Credit.ToString());
                        subjectScore[info].Add("����", info.Score.ToString());
                        subjectScore[info].Add("����", ((info.Require) ? "��" : "��"));

                        //�P�_�ɦҩέ��� 
                        if (!info.Pass)
                        {
                            if (info.Score >= resitStandard[info.GradeYear])
                                subjectScore[info].Add("�ɦ�", "�O");
                            else
                                subjectScore[info].Add("�ɦ�", "�_");
                        }
                    }

                    if (info.Pass && info.Detail.GetAttribute("���p�Ǥ�") != "�O")
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


                mergeKeyValue.Add("���", new object[] { subjectScore, resitSign, repeatSign });

                #endregion

                #region �������Z

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

                        if (entry == "�w��")
                        {
                            semesterMoralScore = score;
                        }
                        else
                        {
                            if (!entryScore.ContainsKey(entry))
                                entryScore.Add(entry, new Dictionary<string, string>());
                            entryScore[entry].Add("����", score.ToString());
                        }
                    }
                }

                //�p�G�O�U�Ǵ��A�N�h�C�L�Ǧ~�w�榨�Z�C
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

                            if (entry == "�w��")
                            {
                                schoolyearMoralScore = score;
                                //entryScore.Add("�Ǧ~�w�榨�Z", new Dictionary<string, string>());
                                //entryScore["�Ǧ~�w�榨�Z"].Add("����", info.Score.ToString());
                            }
                        }
                    }
                }

                SemesterEntryRatingNew rating = new SemesterEntryRatingNew(eachStudent);

                Dictionary<string, string> totalCredit = new Dictionary<string, string>();

                //modify by huangwc , 2013/6/27
                //�Ƿ~���Z�W���p�󵥩�20��ܦW�� , ��L���"N/A"
                int tmp_rating;
                bool tag_rating = int.TryParse(rating.GetPlace(schoolyear, semester),out tmp_rating);
                totalCredit.Add("�Ƿ~���Z�W��", tag_rating?(tmp_rating <= 20 ? tmp_rating.ToString() : "N/A"):"N/A");
                //totalCredit.Add("�Ƿ~���Z�W��", rating.GetPlace(schoolyear, semester));

                totalCredit.Add("�Ǵ����o�Ǥ���", thisSemesterTotalCredit.ToString());
                totalCredit.Add("�֭p���o�Ǥ���", beforeSemesterTotalCredit.ToString());

                mergeKeyValue.Add("����", new object[] { entryScore, totalCredit});
                //mergeKeyValue.Add("�w�榨�Z", semesterMoralScore + " / " + ParseLevel(semesterMoralScore));
                //mergeKeyValue.Add("�Ǧ~�w�榨�Z", schoolyearMoralScore + " / " + ParseLevel(schoolyearMoralScore));

                #endregion

                #region �w���r���q
                XmlElement textScoreXml = null;
                foreach (SemesterMoralScoreInfo info in eachStudent.SemesterMoralScoreList)
                {
                    if (info.SchoolYear == schoolyear && info.Semester == semester)
                    {
                        textScoreXml = info.Detail;
                    }
                }

                foreach (string var in new string[] { "�۹�a����ۤv", "�۹�a��ݥL�H", "�ۧڴL��", "�L���L�H", "�ӿ�", "�d��", "�a�A", "�P��" })
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

                #region �X�ʮu����

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
                if (string.IsNullOrEmpty(absenceString)) absenceString = "����";
                mergeKeyValue.Add("�X�ʮu����", absenceString);

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

                //�^���i��
                _BGWSemesterScoreReport.ReportProgress((int)(currentStudent++ * 100.0 / totalStudent));
            }

            #endregion

            e.Result = doc;
        }

        private void MailMerge_MergeField(object sender, Aspose.Words.Reporting.MergeFieldEventArgs e)
        {
            #region ��ئ��Z

            if (e.FieldName == "���")
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
                        throw new ArgumentException("��ئ��Z��椣���e�U�Ҧ���ئ��Z�C");
                    }

                    Runs runs = SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex].Paragraphs[0].Runs;
                    runs.Add(new Run(e.Document));
                    runs[runs.Count - 1].Text = subjectScore[info]["���"] + ((string.IsNullOrEmpty(subjectScore[info]["�ŧO"])) ? "" : (" (" + subjectScore[info]["�ŧO"] + ")"));
                    runs[runs.Count - 1].Font.Size = 10;
                    runs[runs.Count - 1].Font.Name = "�s�ө���";

                    SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 1].Paragraphs[0].Runs.Add(new Run(e.Document, subjectScore[info]["����"] + subjectScore[info]["�Ǥ�"]));
                    SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 1].Paragraphs[0].Runs[0].Font.Size = 10;
                    SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 2].Paragraphs[0].Runs.Add(new Run(e.Document, subjectScore[info]["����"]));
                    SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 2].Paragraphs[0].Runs[0].Font.Size = 10;

                    int colshift = 0;
                    string re = "";
                    if (subjectScore[info].ContainsKey("�ɦ�"))
                    {
                        if (subjectScore[info]["�ɦ�"] == "�O")
                            re = resitSign;
                        else if (subjectScore[info]["�ɦ�"] == "�_")
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

            #region �������Z

            if (e.FieldName == "����")
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
                    if (entry == "�w��" || entry == "�Ǧ~�w�榨�Z")
                        continue;

                    Runs runs = ESTable.Rows[ESTableRowIndex].Cells[ESTableColIndex].Paragraphs[0].Runs;

                    runs.Add(new Run(e.Document, ToDisplayName(entry)));
                    runs[runs.Count - 1].Font.Size = 10;
                    ESTable.Rows[ESTableRowIndex].Cells[ESTableColIndex + 1].Paragraphs[0].Runs.Add(new Run(e.Document, entryScore[entry]["����"] + semesterDegree));
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
                case "�Ƿ~":
                    return "�Ǵ��Ƿ~���Z";
                case "�w��":
                    return "�Ǵ��w�榨�Z";
                default:
                    return entry;
            }
        }
    }

    /// <summary>
    /// �u�B�z�Ƿ~���Z���ƦW�C
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

            string path = string.Format("SemesterEntryScore[SchoolYear='{0}' and Semester='{1}']/ClassRating/Rating/Item[@����='�Ƿ~']/@�ƦW", schoolYear, semester);
            XmlNode result = _sems_ratings.SelectSingleNode(path);

            if (result != null)
                return result.InnerText;
            else
                return string.Empty;
        }
    }
}
