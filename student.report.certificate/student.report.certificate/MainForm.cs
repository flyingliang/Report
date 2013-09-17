using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using Aspose.Words;
using Aspose.Words.Drawing;
using K12.Data;
using Campus.Report;
namespace plugins.student.report.certificate
{
    public partial class MainForm : FISCA.Presentation.Controls.BaseForm
    {
        private BackgroundWorker _bgw = new BackgroundWorker();

        private const string config = "plugins.student.report.certificate.huangwc.config";
        private Dictionary<string, ReportConfiguration> custConfigs = new Dictionary<string, ReportConfiguration>();
        ReportConfiguration conf = new Campus.Report.ReportConfiguration(config);
        public string current = "";

        public MainForm()
        {
            InitializeComponent();

            if (conf.GetBoolean("firstTime", true))//第一次使用時加入
            {
                //addCustConfig("高中畢業證明書");
                //ReportConfiguration custConf;
                //custConf = new Campus.Report.ReportConfiguration(configNameRule("高中畢業證明書"));
                //custConf.Template = new Campus.Report.ReportTemplate(Properties.Resources.高中畢業證明書, Campus.Report.TemplateType.Word);
                //custConf.Save();
                ////addCustConfig("高中畢業證明書補發");
                //custConf = new Campus.Report.ReportConfiguration(configNameRule("高中畢業證明書補發"));
                //custConf.Template = new Campus.Report.ReportTemplate(Properties.Resources.高中畢業證明書補發, Campus.Report.TemplateType.Word);
                //custConf.Save();
                //conf.SetBoolean("firstTime", false);
                //conf.Save();
            }
            #region 設定comboBox選單
            foreach (string item in getCustConfig())
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    custConfigs.Add(item, new ReportConfiguration(configNameRule(item)));
                    comboBoxEx1.Items.Add(item);
                }
            }
            comboBoxEx1.Items.Add("新增");
            comboBoxEx1.SelectedIndex = 0;
            #endregion
            _bgw.DoWork += new DoWorkEventHandler(_bgw_DoWork);
            _bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bgw_RunWorkerCompleted);
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string value = (string)comboBoxEx1.SelectedItem;
            if (value == "新增") return;
            //畫面內容(範本內容,預設樣式
            Campus.Report.TemplateSettingForm TemplateForm;
            if (custConfigs[current].Template == null)
            {
                custConfigs[current].Template = new Campus.Report.ReportTemplate(Properties.Resources.證明書範本, Campus.Report.TemplateType.Word);
            }
            TemplateForm = new Campus.Report.TemplateSettingForm(custConfigs[current].Template, new Campus.Report.ReportTemplate(Properties.Resources.證明書範本, Campus.Report.TemplateType.Word));
            //預設名稱
            TemplateForm.DefaultFileName = current + "樣板";
            if (TemplateForm.ShowDialog() == DialogResult.OK)
            {
                custConfigs[current].Template = TemplateForm.Template;
                custConfigs[current].Save();
            }
        }
        private void btnPrint_Click(object sender, EventArgs e)
        {
            string value = (string)comboBoxEx1.SelectedItem;
            if (value == "新增") return;
            if (K12.Presentation.NLDPanels.Student.SelectedSource.Count < 1)
            {
                FISCA.Presentation.Controls.MsgBox.Show("請先選擇學生");
                return;
            }
            btnPrint.Enabled = false;
            _bgw.RunWorkerAsync();
        }
        void _bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            Document document = new Document();
            Document template = (custConfigs[current].Template != null) //單頁範本
                 ? custConfigs[current].Template.ToDocument()
                 : new Campus.Report.ReportTemplate(Properties.Resources.證明書範本, Campus.Report.TemplateType.Word).ToDocument();
            List<string> student_ids = K12.Presentation.NLDPanels.Student.SelectedSource;
            List<StudentRecord> srl = Student.SelectByIDs(student_ids);
            List<UpdateRecordRecord> urrl = K12.Data.UpdateRecord.SelectByStudentIDs(student_ids);
            //離校資訊
            Dictionary<string, SHSchool.Data.SHUpdateRecordRecord> dic_st_urr = new Dictionary<string, SHSchool.Data.SHUpdateRecordRecord>();
            List<SHSchool.Data.SHUpdateRecordRecord> shurrl = SHSchool.Data.SHUpdateRecord.SelectByStudentIDs(student_ids);
            foreach( SHSchool.Data.SHUpdateRecordRecord shurr in shurrl)
            {
                if (shurr.UpdateCode == "501")
                {
                    if (dic_st_urr.ContainsKey(shurr.StudentID))
                    {
                        if (dic_st_urr[shurr.StudentID].UpdateDate.CompareTo(shurr.UpdateDate) == 1 )
                        {
                            dic_st_urr[shurr.StudentID] = shurr;
                        }
                    }
                    else dic_st_urr.Add(shurr.StudentID, shurr);
                }
            }
            //入學照片
            Dictionary<string, string> dic_photo_p = K12.Data.Photo.SelectFreshmanPhoto(K12.Presentation.NLDPanels.Student.SelectedSource);
            Dictionary<string, string> dic_photo_g = K12.Data.Photo.SelectGraduatePhoto(K12.Presentation.NLDPanels.Student.SelectedSource);
            //科別中英文對照表
            Dictionary<string, string> dic_dept_ch_en = new Dictionary<string, string>();
            XmlElement Data = SmartSchool.Customization.Data.SystemInformation.Configuration["科別中英文對照表"];
            foreach (XmlElement var in Data)
            {
                if (!dic_dept_ch_en.ContainsKey(var.GetAttribute("Chinese")))
                {
                    dic_dept_ch_en.Add(var.GetAttribute("Chinese"), var.GetAttribute("English"));
                }
            }
            Dictionary<string, object> mailmerge = new Dictionary<string, object>();

            string 校內字號 = textBoxX1.Text;
            string 校內字號英文 = textBoxX2.Text;
            foreach (StudentRecord sr in srl)
            {
                mailmerge.Clear();
                #region MailMerge

                #region 學生資料
                mailmerge.Add("學生姓名", sr.Name);
                mailmerge.Add("學生英文姓名", sr.EnglishName);
                mailmerge.Add("學生身分證號", sr.IDNumber);
                if (sr.Birthday.HasValue)
                {
                    mailmerge.Add("學生生日民國年", sr.Birthday.Value.Year - 1911);
                    mailmerge.Add("學生生日英文年", sr.Birthday.Value.Year);
                    mailmerge.Add("學生生日月", sr.Birthday.Value.Month);
                    mailmerge.Add("學生生日英文月", sr.Birthday.Value.ToString("MMMM", new System.Globalization.CultureInfo("en-US")));
                    mailmerge.Add("學生生日英文月3", sr.Birthday.Value.ToString("MMM", new System.Globalization.CultureInfo("en-US")));
                    mailmerge.Add("學生生日上標", daySuffix(sr.Birthday.Value.Day.ToString()));
                    mailmerge.Add("學生生日日", sr.Birthday.Value.Day);
                }
                if (dic_photo_p.ContainsKey(sr.ID))
                {
                    mailmerge.Add("入學照片1吋", dic_photo_p[sr.ID]);
                    mailmerge.Add("入學照片2吋", dic_photo_p[sr.ID]);
                }
                if (dic_photo_p.ContainsKey(sr.ID))
                {
                    mailmerge.Add("畢業照片1吋", dic_photo_g[sr.ID]);
                    mailmerge.Add("畢業照片2吋", dic_photo_g[sr.ID]);
                }
                if (dic_st_urr.ContainsKey(sr.ID))//畢業異動?
                {
                    mailmerge["離校學年度"] = dic_st_urr[sr.ID].ExpectGraduateSchoolYear;
                    mailmerge["畢業證書字號"] = dic_st_urr[sr.ID].GraduateCertificateNumber;
                    mailmerge["離校科別中文"] = dic_st_urr[sr.ID].Department;
                    string tmp_dept = dic_st_urr[sr.ID].Department;
                    mailmerge["離校科別英文"] = (tmp_dept != null && dic_dept_ch_en.ContainsKey(dic_st_urr[sr.ID].Department)) ? dic_dept_ch_en[dic_st_urr[sr.ID].Department] : "";
                }
                #endregion

                #region 學校資料
                mailmerge.Add("學校全銜", School.ChineseName);
                mailmerge.Add("學校英文全銜", School.EnglishName);
                mailmerge.Add("校長姓名", "");
                if (K12.Data.School.Configuration["學校資訊"] != null && K12.Data.School.Configuration["學校資訊"].PreviousData.SelectSingleNode("ChancellorChineseName") != null)
                    mailmerge["校長姓名"] = K12.Data.School.Configuration["學校資訊"].PreviousData.SelectSingleNode("ChancellorChineseName").InnerText;
                mailmerge.Add("校長姓名英文", "");
                if (K12.Data.School.Configuration["學校資訊"] != null && K12.Data.School.Configuration["學校資訊"].PreviousData.SelectSingleNode("ChancellorChineseName") != null)
                    mailmerge["校長姓名英文"] = K12.Data.School.Configuration["學校資訊"].PreviousData.SelectSingleNode("ChancellorEnglishName").InnerText;
                #endregion
                mailmerge.Add("校內字號", 校內字號);
                mailmerge.Add("校內字號英文", 校內字號英文);
                mailmerge.Add("民國年", DateTime.Today.Year - 1911);
                mailmerge.Add("英文年", DateTime.Today.Year);
                mailmerge.Add("月", DateTime.Today.Month);
                mailmerge.Add("英文月", DateTime.Today.ToString("MMMM", new System.Globalization.CultureInfo("en-US")));
                mailmerge.Add("英文月3", DateTime.Today.ToString("MMM", new System.Globalization.CultureInfo("en-US")));
                mailmerge.Add("日上標", daySuffix(DateTime.Today.Day.ToString()));
                mailmerge.Add("日", DateTime.Today.Day);
                #endregion
                Document each = (Document)template.Clone(true);
                each.MailMerge.MergeField += new Aspose.Words.Reporting.MergeFieldEventHandler(merge);
                each.MailMerge.Execute(mailmerge.Keys.ToArray(), mailmerge.Values.ToArray());

                document.Sections.Add(document.ImportNode(each.FirstSection, true));
            }
            e.Result = document;
        }
        void merge(object sender, Aspose.Words.Reporting.MergeFieldEventArgs e)
        {
            if (e.FieldName == "入學照片1吋" || e.FieldName == "入學照片2吋")
            {
                int tmp_width;
                int tmp_height;
                if (e.FieldName == "入學照片1吋")
                {
                    tmp_width = 25;
                    tmp_height = 35;
                }
                else
                {
                    tmp_width = 35;
                    tmp_height = 45;
                }
                #region 入學照片
                if (!string.IsNullOrEmpty(e.FieldValue.ToString()))
                {
                    byte[] photo = Convert.FromBase64String(e.FieldValue.ToString()); //e.FieldValue as byte[];

                    if (photo != null && photo.Length > 0)
                    {
                        DocumentBuilder photoBuilder = new DocumentBuilder(e.Document);
                        photoBuilder.MoveToField(e.Field, true);
                        e.Field.Remove();
                        //Paragraph paragraph = photoBuilder.InsertParagraph();// new Paragraph(e.Document);
                        Shape photoShape = new Shape(e.Document, ShapeType.Image);
                        photoShape.ImageData.SetImage(photo);
                        photoShape.WrapType = WrapType.Inline;
                        //Cell cell = photoBuilder.CurrentParagraph.ParentNode as Cell;
                        //cell.CellFormat.LeftPadding = 0;
                        //cell.CellFormat.RightPadding = 0;

                        photoShape.Width = ConvertUtil.MillimeterToPoint(tmp_width);
                        photoShape.Height = ConvertUtil.MillimeterToPoint(tmp_height);
                        //paragraph.AppendChild(photoShape);
                        photoBuilder.InsertNode(photoShape);
                    }
                }
                #endregion
            }
            else if (e.FieldName == "畢業照片1吋" || e.FieldName == "畢業照片2吋")
            {
                int tmp_width;
                int tmp_height;
                if (e.FieldName == "畢業照片1吋")
                {
                    tmp_width = 25;
                    tmp_height = 35;
                }
                else
                {
                    tmp_width = 35;
                    tmp_height = 45;
                }
                #region 畢業照片
                if (!string.IsNullOrEmpty(e.FieldValue.ToString()))
                {
                    byte[] photo = Convert.FromBase64String(e.FieldValue.ToString()); //e.FieldValue as byte[];

                    if (photo != null && photo.Length > 0)
                    {
                        DocumentBuilder photoBuilder = new DocumentBuilder(e.Document);
                        photoBuilder.MoveToField(e.Field, true);
                        e.Field.Remove();
                        //Paragraph paragraph = photoBuilder.InsertParagraph();// new Paragraph(e.Document);
                        Shape photoShape = new Shape(e.Document, ShapeType.Image);
                        photoShape.ImageData.SetImage(photo);
                        photoShape.WrapType = WrapType.Inline;
                        //Cell cell = photoBuilder.CurrentParagraph.ParentNode as Cell;
                        //cell.CellFormat.LeftPadding = 0;
                        //cell.CellFormat.RightPadding = 0;
                        photoShape.Width = ConvertUtil.MillimeterToPoint(tmp_width);
                        photoShape.Height = ConvertUtil.MillimeterToPoint(tmp_height);
                        //paragraph.AppendChild(photoShape);
                        photoBuilder.InsertNode(photoShape);
                    }
                }
                #endregion
            }
        }
        void _bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Document inResult = (Document)e.Result;
            btnPrint.Enabled = true;
            try
            {
                SaveFileDialog SaveFileDialog1 = new SaveFileDialog();

                SaveFileDialog1.Filter = "Word (*.doc)|*.doc|所有檔案 (*.*)|*.*";
                SaveFileDialog1.FileName = current;

                if (SaveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    inResult.Save(SaveFileDialog1.FileName);
                    Process.Start(SaveFileDialog1.FileName);
                    FISCA.Presentation.MotherForm.SetStatusBarMessage(SaveFileDialog1.FileName + ",列印完成!!");
                }
                else
                {
                    FISCA.Presentation.Controls.MsgBox.Show("檔案未儲存");
                    return;
                }
            }
            catch
            {
                string msg = "檔案儲存錯誤,請檢查檔案是否開啟中!!";
                FISCA.Presentation.Controls.MsgBox.Show(msg);
                FISCA.Presentation.MotherForm.SetStatusBarMessage(msg);
            }
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void comboBoxEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var value = (string)comboBoxEx1.SelectedItem;
            switch (value)
            {
                case "新增":
                    AddNew input = new AddNew();
                    if (input.ShowDialog() == DialogResult.OK)
                    {
                        input.name = System.Text.RegularExpressions.Regex.Replace(input.name, @"[\W_]+", "");
                        if (string.IsNullOrWhiteSpace(input.name))
                            FISCA.Presentation.Controls.MsgBox.Show("請輸入樣板名稱(中文或英文字母)");
                        else if (custConfigs.ContainsKey(input.name))
                            FISCA.Presentation.Controls.MsgBox.Show("樣板名稱已存在");
                        else
                        {
                            ReportConfiguration tmp_conf = new ReportConfiguration(configNameRule(input.name));
                            if (input.Template != null)
                                tmp_conf.Template = new ReportTemplate(input.Template);
                            tmp_conf.Save();
                            custConfigs.Add(input.name, tmp_conf);
                            addCustConfig(input.name);
                            comboBoxEx1.Items.Insert(0, input.name);
                            comboBoxEx1.SelectedIndex = 0;
                        }
                    }
                    break;
                default:
                    current = value;
                    break;
            }
        }
        private void delete_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string value = (string)comboBoxEx1.SelectedItem;
            switch (value)
            {
                case "新增":
                    break;
                default:
                    if (custConfigs.ContainsKey(value))
                    {
                        custConfigs[value].Template = null;
                        custConfigs[value].Save();
                        custConfigs.Remove(value);
                        comboBoxEx1.Items.Remove(value);
                        delCustConfig(value);
                    }
                    break;
            }
            comboBoxEx1.SelectedIndex = 0;
            current = (string)comboBoxEx1.SelectedItem;
        }
        private void addCustConfig(string custConfig)
        {
            List<string> tmp = conf.GetString("customs", "").Split(';').ToList<string>();
            tmp.Add(System.Text.RegularExpressions.Regex.Replace(custConfig, @"[\W_]+", ""));
            conf.SetString("customs", string.Join(";", tmp));
            conf.Save();
        }
        private void delCustConfig(string custConfig)
        {
            List<string> tmp = conf.GetString("customs", "").Split(';').ToList<string>();
            tmp.Remove(custConfig);
            conf.SetString("customs", string.Join(";", tmp));
            conf.Save();
        }
        private string[] getCustConfig()
        {
            return conf.GetString("customs", "").Split(';');
        }
        private static string configNameRule(string custConfigName)
        {
            return config + "." + custConfigName;
        }
        public static string daySuffix(string date)
        {
            switch (int.Parse(date) % 10)
            {
                case 1: return "st";
                case 2: return "nd";
                case 3: return "rd";
                default: return "th";
            }
        }

    }
}
