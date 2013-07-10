using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
namespace StudentDetailWithExts
{
    class extsRecord 
    {
        public string current_id { get; private set; }
        /// <summary>
        /// db fields:ref_student_id,field_name,value
        /// field_name => value
        /// </summary>
        public Dictionary<string, string> exts { get; private set; }
        public extsRecord(string current_id)
        {
            this.current_id = current_id;
            exts = new Dictionary<string, string>();
        }
        public void addExt(DataRow row)
        {
            string id = "" + row["ref_student_id"];
            string field = ""+row["field_name"];
            string value = ""+row["value"];
            
            if (current_id == id && !exts.ContainsKey(field))
            {
                exts.Add(field, value);
            }
        }
    }
}
