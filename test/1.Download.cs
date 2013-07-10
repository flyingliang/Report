using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using K12.Data;
namespace StudentDetailWithExts
{
    public partial class Download : FISCA.Presentation.Controls.BaseForm
    {
        Dictionary<setting.StudentDetailFields, int> detailFields = new Dictionary<setting.StudentDetailFields, int>();
        Dictionary<string, int> extsFields = new Dictionary<string, int>();
        
        public Download()
        {
            InitializeComponent();
            #region 從setting與db table:student_exts繪製checkedListBoxs
            
            foreach (setting.StudentDetailFields _f in Enum.GetValues(typeof(setting.StudentDetailFields)))
            {
                checkedListBox2.Items.Add(_f.ToString());
            }
            DataTable dt = tool._Q.Select(string.Format("SELECT DISTINCT field_name as field FROM student_exts"));
            foreach (DataRow row in dt.Rows)
            {
                checkedListBox1.Items.Add(row["field"]);
            }
            #endregion
        }
        private void buttonX1_Click(object sender, EventArgs e)
        {
            buttonX1.Enabled = false;
            buttonX1.Text = "產生中...";
            clear_all();
            if (checkedListBox1.CheckedItems.Count + checkedListBox2.CheckedItems.Count < 1)
            {
                FISCA.Presentation.Controls.MsgBox.Show("請至少選擇一個欄位");
                return;
            }
            #region 繪製datagridview column與欄位位置map
            for (int i = 0; i < checkedListBox2.CheckedItems.Count; i++)
            {
                //add col
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.HeaderText = "" + checkedListBox2.CheckedItems[i];
                dataGridView1.Columns.Add(col);

                //sql>field
                detailFields.Add((setting.StudentDetailFields)Enum.Parse(typeof(setting.StudentDetailFields), "" + checkedListBox2.CheckedItems[i]), dataGridView1.ColumnCount - 1);
            }
            for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
            {
                //add col
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.HeaderText = "" + checkedListBox1.CheckedItems[i];
                dataGridView1.Columns.Add(col);

                //sql>field
                extsFields.Add("" + checkedListBox1.CheckedItems[i], dataGridView1.ColumnCount - 1);
            }
            #endregion

            #region 抓取資料並整理
            List<K12.Data.StudentRecord> srl = K12.Data.Student.SelectByIDs(K12.Presentation.NLDPanels.Student.SelectedSource);
            //List<K12.Data.BeforeEnrollmentRecord> berl = K12.Data.BeforeEnrollment.SelectByStudentIDs(K12.Presentation.NLDPanels.Student.SelectedSource);
            string _s = string.Join(",", K12.Presentation.NLDPanels.Student.SelectedSource);
            string _f = "'" + string.Join("','", extsFields.Keys) + "'";
            string _q = "SELECT ref_student_id,field_name,value FROM student_exts WHERE ref_student_id IN ( " + _s + " ) AND field_name IN ( " + _f + " ) order by ref_student_id";
            DataTable dt = tool._Q.Select(_q);

            Dictionary<string, extsRecord> Dic_st_exts = new Dictionary<string, extsRecord>();
            foreach (DataRow row in dt.Rows)
            {
                string id = "" + row["ref_student_id"];
                if (!Dic_st_exts.ContainsKey(id))
                {
                    Dic_st_exts.Add(id, new extsRecord(id));
                }
                Dic_st_exts[id].addExt(row);
            }
            #endregion
            
            show_all(srl, Dic_st_exts);

            buttonX1.Text = "再次產生";
            buttonX1.Enabled = true;
        }
        /// <summary>
        /// 清理datagridview及欄位位置map
        /// </summary>
        private void clear_all()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            detailFields.Clear();
            extsFields.Clear();
        }
        private void show_all(List<K12.Data.StudentRecord> srl, Dictionary<string, extsRecord> Dic_st_exts)
        {
            dataGridView1.Rows.Clear();
            foreach (StudentRecord sr in srl)
            {
                show_row(sr, (Dic_st_exts.ContainsKey(sr.ID))?Dic_st_exts[sr.ID]:new extsRecord(sr.ID));
            }
        }
        private void show_row(StudentRecord sr, extsRecord er)
        {
            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dataGridView1);
            //student detail
            foreach (KeyValuePair<setting.StudentDetailFields, int> item in detailFields)
                row.Cells[detailFields[item.Key]].Value = setting.getStudentDetailValue(item.Key,sr);
            //student exts
            foreach (KeyValuePair<string, string> item in er.exts)
                row.Cells[extsFields[item.Key]].Value = item.Value;

            dataGridView1.Rows.Add(row);
        }
        private void buttonX2_Click(object sender, EventArgs e)
        {
            #region 匯出
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = "學生基本資料(含自訂欄位)";
            saveFileDialog1.Filter = "Excel (*.xls)|*.xls";
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;

            DataGridViewExport export = new DataGridViewExport(dataGridView1);
            export.Save(saveFileDialog1.FileName);

            System.Diagnostics.Process.Start(saveFileDialog1.FileName);
            #endregion
        }
        #region 全選按鈕CheckedChanged
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            bool enable = cb.Checked;
            for (int j = 0; j < checkedListBox1.Items.Count; j++)
                checkedListBox1.SetItemChecked(j, enable);
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            bool enable = cb.Checked;
            for (int j = 0; j < checkedListBox2.Items.Count; j++)
                checkedListBox2.SetItemChecked(j, enable);
        }
        #endregion
        
    }
}
