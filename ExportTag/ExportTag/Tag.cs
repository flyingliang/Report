using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHSchool.Data;
using SmartSchool.API.PlugIn;
using FISCA.DSAUtil;
using System.Xml;
using System.Data;
namespace ExportTag
{

    class ExportTag : SmartSchool.API.PlugIn.Export.Exporter
    {
        List<string> SelectableFieldsList;
        static List<string> ExportFields = new List<string>
        {
            //"學生系統編號","學號","班級","座號","姓名",
             "身份證號","國籍","出生地","生日","性別","英文姓名","科別","年級","登入帳號","帳號類型"
            ,"戶籍地址","戶籍地址:郵遞區號","戶籍地址:縣市","戶籍地址:鄉鎮市區","戶籍地址:村里街號","戶籍電話"
            ,"聯絡地址","聯絡地址:郵遞區號", "聯絡地址:縣市", "聯絡地址:鄉鎮市區", "聯絡地址:村里街號","聯絡電話"
            ,"行動電話",/*"其他電話",*/ "其他電話:1", "其他電話:2", "其他電話:3"
            ,"監護人姓名","監護人身份證號","監護人國籍","監護人稱謂",/*"監護人其他資訊",*/"監護人其他:學歷", "監護人其他:職業"
            ,"父親姓名","父親身份證號","父親國籍","父親存歿",/*"父親其他資訊",*/"父親其他:學歷", "父親其他:職業"
            ,"母親姓名","母親身份證號","母親國籍","母親存歿",/*"母親其他資訊",*/"母親其他:學歷", "母親其他:職業"
            ,/*"前級畢業資訊",*/"前級畢業:學校名稱", "前級畢業:學校所在地", "前級畢業:班級", "前級畢業:座號", "前級畢業:備註", "前級畢業:國中畢業年度"
            ,"畢結業證書字號"
        };
        public ExportTag()
        {
            this.Image = null;
            this.Text = "匯出學生類別(含基本資料)";

            SelectableFieldsList = ExportFields.FindAll(delegate(string f)
            {
                //刪除ExportFields中 有":"符號
                //if (f.IndexOf(":") == -1) return true;
                //else return false;
                return true;
            });
            //加上自訂欄位
            DataTable dt = tool._Q.Select("SELECT DISTINCT tag.prefix AS tag_prefix,tag.name AS tag_name " +
                    "FROM tag_student " +
                    "LEFT JOIN tag ON tag_student.ref_tag_id = tag.id " +
                    "WHERE tag_student.ref_student_id IN (" + string.Join(",", K12.Presentation.NLDPanels.Student.SelectedSource) + ") ");
            foreach (DataRow row in dt.Rows)
            {
                SelectableFieldsList.Add("類別:"+TagRecord.getKeyName("" + row["tag_name"], "" + row["tag_prefix"]));
            }
            SelectableFieldsList.Sort(SortField);
        }
        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            //加入欲匯出之欄位資料
            wizard.ExportableFields.AddRange(SelectableFieldsList);

            wizard.ExportPackage += delegate(object sender, SmartSchool.API.PlugIn.Export.ExportPackageEventArgs e)
            {
                //取得學生清單
                //if (e.List.Count < 1)
                //    System.Windows.Forms.MessageBox.Show("no student seleted");

                #region 整理欄位名稱 unused
                /*List<string> tmp_ExportFields;
                tmp_ExportFields = e.ExportFields.ToList<string>();
                foreach (string field in tmp_ExportFields)
                {
                    if (SelectableFieldsList.Contains(field))
                    {
                        switch (field)
                        {
                            case "戶籍地址":
                                e.ExportFields.Remove("戶籍地址");
                                e.ExportFields.AddRange(new string[] { "戶籍:郵遞區號", "戶籍:縣市", "戶籍:鄉鎮市區", "戶籍:村里街號" });
                                break;
                            case "聯絡地址":
                                e.ExportFields.Remove("聯絡地址");
                                e.ExportFields.AddRange(new string[] { "聯絡:郵遞區號", "聯絡:縣市", "聯絡:鄉鎮市區", "聯絡:村里街號" });
                                break;
                            case "其他電話":
                                e.ExportFields.Remove("其他電話");
                                e.ExportFields.AddRange(new string[] { "其他電話:1", "其他電話:2", "其他電話:3" });
                                break;
                            case "監護人其他資訊":
                                e.ExportFields.Remove("監護人其他資訊");
                                e.ExportFields.AddRange(new string[] { "監護人:學歷", "監護人:職業" });
                                break;
                            case "父親其他資訊":
                                e.ExportFields.Remove("父親其他資訊");
                                e.ExportFields.AddRange(new string[] { "父親:學歷", "父親:職業" });
                                break;
                            case "母親其他資訊":
                                e.ExportFields.Remove("母親其他資訊");
                                e.ExportFields.AddRange(new string[] { "母親:學歷", "母親:職業" });
                                break;
                            case "前級畢業資訊":
                                e.ExportFields.Remove("前級畢業資訊");
                                e.ExportFields.AddRange(new string[] { "前級:學校名稱", "前級:學校所在地", "前級:班級", "前級:座號", "前級:備註", "前級:國中畢業年度" });
                                break;
                        }
                    }
                    else e.ExportFields.Remove(field);
                }
                e.ExportFields.Sort(SortField);*/
                #endregion

                #region 取得及整理學生欄位資料
                DataTable dt = tool._Q.Select("SELECT student.*," +
                "class.class_name ,class.grade_year AS class_grade_year,class.ref_dept_id AS class_ref_dept_id," +
                "dept.name AS dept_name " +
                "FROM student " +
                "LEFT JOIN class ON student.ref_class_id = class.id " +
                "LEFT JOIN dept ON dept.id = class.ref_dept_id " +
                "WHERE student.id IN (" + string.Join(",", e.List) + ")");

                List<custStudentRecord> csrl = new List<custStudentRecord>();
                foreach (DataRow row in dt.Rows)
                {
                    csrl.Add(new custStudentRecord(row));
                }
                #endregion

                #region 取得及整理類別資料
                dt = tool._Q.Select("SELECT tag_student.ref_student_id," +
                    "tag.prefix AS tag_prefix,tag.name AS tag_name " +
                    "FROM tag_student " +
                    "LEFT JOIN tag ON tag_student.ref_tag_id = tag.id " +
                    "WHERE tag_student.ref_student_id IN (" + string.Join(",", e.List) + ") ");

                Dictionary<string, TagRecord> Dic_st_tag = new Dictionary<string, TagRecord>();
                foreach (DataRow row in dt.Rows)
                {
                    // ref_student_id,tag_prefix,tag_name
                    string id = "" + row["ref_student_id"];
                    if (!Dic_st_tag.ContainsKey(id))
                        Dic_st_tag.Add(id, new TagRecord());

                    string tagName = "" + row["tag_name"];
                    string tagPrefix = "" + row["tag_prefix"];
                    if (!string.IsNullOrEmpty(tagName))
                    {
                        Dic_st_tag[id].setTag(tagName, tagPrefix);
                    }
                }
                #endregion

                //整理填入資料
                csrl.Sort(SortStudent);
                foreach (custStudentRecord csr in csrl) //每一位學生
                {
                    RowData row = new RowData();
                    row.ID = csr.ID;
                    //對於每一個要匯出的欄位
                    foreach (string field in e.ExportFields)
                    {
                        string value = "";

                        switch (field)
                        {
                            #region 學生資料
                            case "出生地": value = "" + csr.BirthPlace; break;
                            case "生日":
                                if (csr.Birthday.HasValue)
                                    value = "" + csr.Birthday.Value.Date.ToShortDateString();
                                break;
                            case "身份證號": value = "" + csr.IDNumber; break;
                            case "國籍": value = "" + csr.Nationality; break;
                            case "性別": value = "" + csr.Gender; break;
                            case "英文姓名": value = "" + csr.EnglishName; break;
                            case "科別": value = "" + csr.DeptName; break;
                            case "年級": value = "" + csr.ClassGradeYear; break;
                            case "登入帳號": value = "" + csr.SALoginName; break;
                            case "帳號類型": value = "" + csr.AccountType; break;
                            case "戶籍地址": value = "" + csr.PermanentAddress; break;//new
                            case "戶籍地址:郵遞區號": value = "" + csr.PermanentAddressZipCode; break;
                            case "戶籍地址:縣市": value = "" + csr.PermanentAddressCounty; break;
                            case "戶籍地址:鄉鎮市區": value = "" + csr.PermanentAddressTown; break;
                            case "戶籍地址:村里街號": value = "" + csr.PermanentAddressDetailAddress; break;
                            case "聯絡地址": value = "" + csr.MallingAddress; break;
                            case "聯絡地址:郵遞區號": value = "" + csr.MallingAddressZipCode; break;
                            case "聯絡地址:縣市": value = "" + csr.MallingAddressCounty; break;
                            case "聯絡地址:鄉鎮市區": value = "" + csr.MallingAddressTown; break;
                            case "聯絡地址:村里街號": value = "" + csr.MallingAddressDetailAddress; break;
                            case "戶籍電話": value = "" + csr.PermanentPhone; break;
                            case "聯絡電話": value = "" + csr.ContactPhone; break;
                            case "行動電話": value = "" + csr.SMSPhone; break;
                            case "其他電話:1": value = "" + csr.OtherPhone1; break;
                            case "其他電話:2": value = "" + csr.OtherPhone2; break;
                            case "其他電話:3": value = "" + csr.OtherPhone3; break;
                            case "監護人姓名": value = "" + csr.CustodianName; break;
                            case "監護人身份證號": value = "" + csr.CustodianIDNumber; break;
                            case "監護人國籍": value = "" + csr.CustodianNationality; break;
                            case "監護人稱謂": value = "" + csr.CustodianRelationship; break;
                            case "監護人其他:學歷": value = "" + csr.CustodianEducationDegree; break;
                            case "監護人其他:職業": value = "" + csr.CustodianJob; break;
                            case "父親姓名": value = "" + csr.FatherName; break;
                            case "父親身份證號": value = "" + csr.FatherIDNumber; break;
                            case "父親國籍": value = "" + csr.FatherNationality; break;
                            case "父親存歿": value = "" + csr.FatherLiving; break;
                            case "父親其他:學歷": value = "" + csr.FatherEducationDegree; break;
                            case "父親其他:職業": value = "" + csr.FatherJob; break;
                            case "母親姓名": value = "" + csr.MotherName; break;
                            case "母親身份證號": value = "" + csr.MotherIDNumber; break;
                            case "母親國籍": value = "" + csr.MotherNationality; break;
                            case "母親存歿": value = "" + csr.MotherLiving; break;
                            case "母親其他:學歷": value = "" + csr.MotherEducationDegree; break;
                            case "母親其他:職業": value = "" + csr.MotherJob; break;
                            case "前級畢業:學校名稱": value = "" + csr.BeforeEnrollmentSchool; break;
                            case "前級畢業:學校所在地": value = "" + csr.BeforeEnrollmentSchoolLocation; break;
                            case "前級畢業:班級": value = "" + csr.BeforeEnrollmentClassName; break;
                            case "前級畢業:座號": value = "" + csr.BeforeEnrollmentSeatNo; break;
                            case "前級畢業:備註": value = "" + csr.BeforeEnrollmentMemo; break;
                            case "前級畢業:國中畢業年度": value = "" + csr.BeforeEnrollmentGraduateSchoolYear; break;
                            case "畢結業證書字號": value = "" + csr.DiplomaNumber; break;
                            #endregion

                            #region 類別資料
                            default:
                                string tmp_field = field.Replace("類別:","");
                                if (Dic_st_tag.ContainsKey(row.ID))
                                {
                                    if (Dic_st_tag[row.ID].tags.ContainsKey(tmp_field))
                                        value = Dic_st_tag[row.ID].getTagValues(tmp_field);
                                }
                                break;
                            #endregion
                        }

                        row.Add(field, value);
                    }
                    e.Items.Add(row);
                }
            };
        }
        public static int SortStudent(custStudentRecord x, custStudentRecord y)
        {

            string xx1 = "" + x.ClassName;
            string xx2 = x.SeatNo.HasValue ? x.SeatNo.Value.ToString().PadLeft(3, '0') : "000";
            string xx3 = xx1 + xx2;

            string yy1 = "" + y.ClassName;
            string yy2 = y.SeatNo.HasValue ? y.SeatNo.Value.ToString().PadLeft(3, '0') : "000";
            string yy3 = yy1 + yy2;

            return xx3.CompareTo(yy3);
        }
        public static int SortField(string x, string y)
        {
            int xi = ExportFields.IndexOf(x);
            int yi = ExportFields.IndexOf(y);

            if (xi == -1 && yi == -1) //如果都沒有在ExportFields中則依照xy字串排序
                return x.CompareTo(y);
            else if (xi == -1)//如果xi沒有在ExportFields中則將x排至後方
                return 1;
            else if (yi == -1)//如果yi沒有在ExportFields中則將x排至前方
                return -1;
            else
                return xi.CompareTo(yi);
        }
    }
}