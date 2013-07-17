using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExportTag
{
    /// <summary>
    /// 學生類別資料
    /// </summary>
    public class TagRecord
    {
        public Dictionary<string, List<string>> tags = new Dictionary<string, List<string>>();
        public void setTag(string tagName, string tagPrefix)
        {
            string keyName = getKeyName(tagName, tagPrefix);
            if (tags.ContainsKey(keyName))
                tags[keyName].Add(tagName);
            else
                tags.Add(keyName, new List<string>(new string[] { tagName }));
        }
        // 類別處理原則：群組當欄位名稱，如果沒有群組，欄位名稱用[類別名稱]，內容值是類別名稱，如果有2個以上用,隔開。
        public static string getKeyName( string tagName,string tagPrefix)
        {
            if (string.IsNullOrEmpty(tagPrefix))
                return "[" + tagName + "]";
            else
                return tagPrefix;
        }
        // 類別處理原則：群組當欄位名稱，如果沒有群組，欄位名稱用[類別名稱]，內容值是類別名稱，如果有2個以上用,隔開。
        public string getTagValues(string keyName)
        {
            if (tags.ContainsKey(keyName))
                return string.Join(",", tags[keyName]);
            else return "";
        }
    }
}
