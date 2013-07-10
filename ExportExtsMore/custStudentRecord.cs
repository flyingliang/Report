using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
namespace ExportExtsMore
{
    class custStudentRecord
    {
        public static string[] requiredFields = new string[] { "id" };
        public string AccountType { get; private set; }
        public DateTime? Birthday { get; private set; }
        public string BirthPlace { get; private set; }
        //public ClassRecord Class { get; }
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
        //public string SAPassword { get; private set; }
        public int? SeatNo { get; private set; }
        public string StatusStr { get; }
        public string StudentNumber { get; private set; }
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
            foreach (string field in requiredFields)
            {
                if (string.IsNullOrEmpty(row["id"].ToString()))
                    return; //false ; ?
            }
            #region private setup a Student
            this.ID = "" + row["id"];
            this.Name = (row["name"].Equals(null)) ? "" + row["name"] : "";
            //...
            
            #endregion

        }

    }
}
