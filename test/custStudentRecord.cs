using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Xml;
namespace ConsoleApplication1
{
    class custStudentRecord
    {
        //Student
        public static string[] requiredFields = new string[] { "id" };
        public string AccountType { get; private set; }
        public DateTime? Birthday { get; private set; }
        public string BirthPlace { get; private set; }
        public string Comment { get; private set; }
        public string EnglishName { get; private set; }
        public string EnrollmentCategory { get; private set; }
        public string Gender { get; private set; }
        public bool HomeSchooling { get; private set; }
        public string ID { get; private set; }
        public string IDNumber { get; private set; }
        public string Name { get; private set; }
        public string Nationality { get; private set; }
        public string RefClassID { get; private set; }
        public string SALoginName { get; private set; }
        public int? SeatNo { get; private set; }
        public string StudentNumber { get; private set; }

        public string ClassName { get; private set; }
        public string GradeYear { get; private set; }
        public string Dept { get; private set; } // in table dept ?

        //new atti below
        public string SMSPhone { get; private set; }
        public string MallingAddressZipCode { get; private set; }
        public string MallingAddressCounty { get; private set; }
        public string MallingAddressTown { get; private set; }
        public string MallingAddressDetailAddress { get; private set; }
        public string ContactPhone { get; private set; }
        public string OtherPhone1 { get; private set; }
        public string OtherPhone2 { get; private set; }
        public string OtherPhone3 { get; private set; }
        public string PermanentAddressZipCode { get; private set; }
        public string PermanentAddressCounty { get; private set; }
        public string PermanentAddressTown { get; private set; }
        public string PermanentAddressDetailAddress { get; private set; }
        public string PermanentPhone { get; private set; }
        public string DiplomaNumber { get; private set; }
        public string FatherName { get; private set; }
        public string FatherNationality { get; private set; }
        public string FatherIDNumber { get; private set; }
        public string FatherLiving { get; private set; }
        public string FatherJob { get; private set; }
        public string FatherEducationDegree { get; private set; }
        public string MotherName { get; private set; }
        public string MotherNationality { get; private set; }
        public string MotherIDNumber { get; private set; }
        public string MotherLiving { get; private set; }
        public string MotherJob { get; private set; }
        public string MotherEducationDegree { get; private set; }
        public string CustodianName { get; private set; }
        public string CustodianNationality { get; private set; }
        public string CustodianIDNumber { get; private set; }
        public string CustodianLiving { get; private set; }
        public string CustodianRelationship { get; private set; }
        public string CustodianJob { get; private set; }
        public string CustodianEducationDegree { get; private set; }
        public string BeforeEnrollmentSchool { get; private set; }
        public string BeforeEnrollmentSchoolLocation { get; private set; }
        public string BeforeEnrollmentClassName { get; private set; }
        public string BeforeEnrollmentSeatNo { get; private set; }
        public string BeforeEnrollmentMemo { get; private set; }
        public string BeforeEnrollmentGraduateSchoolYear { get; private set; }
        public enum StudentStatus
        {
            一般 = 0,
            延修 = 1,
            畢業或離校 = 2,
            休學 = 3,
            輟學 = 4,
            刪除 = 5,
            轉出 = 6,
            退學 = 7,
        }
        public custStudentRecord(DataRow row)
        {
            //foreach (string field in requiredFields)
            //{
            //    if (string.IsNullOrEmpty(row["id"].ToString()))
            //        throw new ArgumentNullException("student id not allow be null");
            //}

            #region setup a Student
            this.ID = row.Table.Columns.Contains("id") ? "" + row["id"] : "";
            this.Name = "" + row["name"];
            this.EnglishName = "" + row["english_name"];
            this.Birthday = DateTime.Parse("" + row["bithdate"]); //maybe have problem
            this.IDNumber = "" + row["id_number"];
            this.RefClassID = "" + row["ref_class_id"];
            this.BirthPlace = "" + row["birth_place"];
            this.StudentNumber = "" + row["student_number"];
            {
                int tmp_seatno;
                this.SeatNo = int.TryParse("" + row["seat_no"], out tmp_seatno) ? (int?)tmp_seatno : null;
            }
            this.SALoginName = "" + row["sa_login_name"];
            this.AccountType = "" + row["account_type"];
            this.Gender = mapGender("" + row["gender"]);
            this.Nationality = "" + row["nationality"];
            this.Comment = "" + row["comment"];

            this.SMSPhone = "" + row["sms_phone"];
            this.ContactPhone = "" + row["contact_phone"];
            this.PermanentPhone = "" + row["permanent_phone"];
            //<PhoneList><PhoneNumber>0937177460</PhoneNumber><PhoneNumber/><PhoneNumber/></PhoneList>
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("" + row["other_phones"]);
                string tmp = "";
                if (parseXml(xmlDoc, "PhoneList/PhoneNumber", out tmp, 0))
                    this.OtherPhone1 = tmp;
                if (parseXml(xmlDoc, "PhoneList/PhoneNumber", out tmp, 1))
                    this.OtherPhone2 = tmp;
                if (parseXml(xmlDoc, "PhoneList/PhoneNumber", out tmp, 2))
                    this.OtherPhone3 = tmp;

            }

            //<AddressList><Address><ZipCode>304</ZipCode><County>新竹縣</County><Town>新豐鄉</Town><DetailAddress>瑞興村3鄰46之2號</DetailAddress></Address></AddressList>
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("" + row["mailing_address"]);
                string tmp = "";
                if (parseXml(xmlDoc, "AddressList/Address/ZipCode", out tmp))
                    this.MallingAddressZipCode = tmp;
                if (parseXml(xmlDoc, "AddressList/Address/County", out tmp))
                    this.MallingAddressCounty = tmp;
                if (parseXml(xmlDoc, "AddressList/Address/Town", out tmp))
                    this.MallingAddressTown = tmp;
                if (parseXml(xmlDoc, "AddressList/Address/DetailAddress", out tmp))
                    this.MallingAddressDetailAddress = tmp;
            }
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("" + row["permanent_address"]);
                string tmp = "";
                if (parseXml(xmlDoc, "AddressList/Address/ZipCode", out tmp))
                    this.PermanentAddressZipCode = tmp;
                if (parseXml(xmlDoc, "AddressList/Address/County", out tmp))
                    this.PermanentAddressCounty = tmp;
                if (parseXml(xmlDoc, "AddressList/Address/Town", out tmp))
                    this.PermanentAddressTown = tmp;
                if (parseXml(xmlDoc, "AddressList/Address/DetailAddress", out tmp))
                    this.PermanentAddressDetailAddress = tmp;
            }
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("" + row["diploma_number"]);
                string tmp = "";
                if (parseXml(xmlDoc, "DiplomaNumber", out tmp))
                    this.DiplomaNumber = tmp;
            }
            #region Father
            this.FatherName = "" + row["father_name"];
            this.FatherNationality = "" + row["father_nationality"];
            this.FatherIDNumber = "" + row["father_id_number"];
            this.FatherLiving = mapParentLiving("" + row["father_living"]);
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("" + row["father_other_info"]);
                string tmp = "";
                if (parseXml(xmlDoc, "FatherOtherInfo/FatherJob", out tmp))
                    this.FatherJob = tmp;
                if (parseXml(xmlDoc, "FatherOtherInfo/FatherEducationDegree", out tmp))
                    this.FatherEducationDegree = tmp;
            }
            #endregion

            #region Mother
            this.MotherName = "" + row["mother_name"];
            this.MotherNationality = "" + row["mother_nationality"];
            this.MotherIDNumber = "" + row["mother_id_number"];
            this.MotherLiving = mapParentLiving("" + row["mother_living"]);
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("" + row["mother_other_info"]);
                string tmp = "";
                if (parseXml(xmlDoc, "MotherOtherInfo/MotherJob", out tmp))
                    this.FatherJob = tmp;
                if (parseXml(xmlDoc, "MotherOtherInfo/MotherEducationDegree", out tmp))
                    this.FatherEducationDegree = tmp;
            }
            #endregion

            #region Custodian
            this.CustodianName = "" + row["custodian_name"];
            this.CustodianNationality = "" + row["custodian_nationality"];
            this.CustodianIDNumber = "" + row["custodian_id_number"];
            this.CustodianLiving = mapParentLiving("" + row["custodian_living"]);
            this.CustodianRelationship = "" + row["custodian_relationship"];
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("" + row["custodian_other_info"]);
                string tmp = "";
                if (parseXml(xmlDoc, "CustodianOtherInfo/CustodianJob", out tmp))
                    this.FatherJob = tmp;
                if (parseXml(xmlDoc, "CustodianOtherInfo/CustodianEducationDegree", out tmp))
                    this.FatherEducationDegree = tmp;
            }
            #endregion

            #region BeforeEnrollment
            //<BeforeEnrollment><School>1</School><SchoolLocation>2</SchoolLocation><ClassName>3</ClassName><SeatNo>4</SeatNo><Memo>5</Memo><GraduateSchoolYear>6</GraduateSchoolYear></BeforeEnrollment>
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("" + row["before_enrollment"]);
                string tmp = "";
                if (parseXml(xmlDoc, "BeforeEnrollment/School", out tmp))
                    this.BeforeEnrollmentSchool = tmp;
                if (parseXml(xmlDoc, "BeforeEnrollment/SchoolLocation", out tmp))
                    this.BeforeEnrollmentSchoolLocation = tmp;
                if (parseXml(xmlDoc, "BeforeEnrollment/ClassName", out tmp))
                    this.BeforeEnrollmentClassName = tmp;
                if (parseXml(xmlDoc, "BeforeEnrollment/SeatNo", out tmp))
                    this.BeforeEnrollmentSeatNo = tmp;
                if (parseXml(xmlDoc, "BeforeEnrollment/Memo", out tmp))
                    this.BeforeEnrollmentMemo = tmp;
                if (parseXml(xmlDoc, "BeforeEnrollment/GraduateSchoolYear", out tmp))
                    this.BeforeEnrollmentGraduateSchoolYear = tmp;

            }
            #endregion

            #endregion
        }
        public static bool parseXml(XmlDocument xmlDoc, string xpath, out string output, int elementIndex = 0)
        {
            output = "";
            List<string> r = new List<string>();
            var elements = xmlDoc.SelectNodes(xpath);
            for (int i = 0; i < elements.Count; i++)
            {
                XmlNode element = elements[i];
                if (i == elementIndex)
                {
                    output = element.InnerText;
                    return true;
                }
            }
            return false;
        }
        public static string mapGender(string gender)
        {
            switch (gender)
            {
                case "1":
                    return "男";
                case "0":
                    return "女";
                default:
                    return "";
            }
        }
        public static string mapParentLiving(string living)
        {
            switch (living)
            {
                case "true":
                    return "存";
                case "false":
                    return "歿";
                default:
                    return "";
            }
        }
    }
}
