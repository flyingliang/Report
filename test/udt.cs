using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UDT範例專案
{
    public partial class Form2 : Form
    {
        FISCA.UDT.AccessHelper _A = new FISCA.UDT.AccessHelper();
        List<testUDTClass> UDTList = new List<testUDTClass>();
        Dictionary<string, int> UDTDic = new Dictionary<string, int>();
        BackgroundWorker bgw = new BackgroundWorker();
        public Form2()
        {
            InitializeComponent();
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            //建立資料
            //testUDTClass obj = new testUDTClass();
            //obj.ref_student_id = "652335"; //學生系統編號
            //obj.Note = "測試UDT資料";

            //新增資料於系統
            //List<testUDTClass> UDTList = new List<testUDTClass>();
            //UDTList.Add(obj);
            //_A.InsertValues(UDTList);
            //取回資料
            bgw.DoWork += delegate { save();get();};
            bgw.RunWorkerCompleted += delegate { show(); };
            get();
            show();
            //把資料建立到畫面上
        }
        //button save
        private void button1_Click(object sender, EventArgs e)
        {
            bgw.RunWorkerAsync() ; 
        }
        private void get()
        {
            UDTList = _A.Select<testUDTClass>();
            UDTDic.Clear();
            for (int i = 0; i < UDTList.Count; i++)
            {
                UDTDic.Add(UDTList[i].UID, i);
            }
        }
        private void show()
        {
            dataGridView1.Rows.Clear();
            foreach (testUDTClass sr in UDTList)
            {
                DataGridViewRow row = new DataGridViewRow(); //建立一個DataGridViewRow
                row.CreateCells(dataGridView1); //此row 參考dataGridViewX1的樣式
                row.Cells[1].Value = sr.ref_student_id;
                row.Cells[2].Value = sr.Note;
                row.Cells[0].Value = sr.UID;
                dataGridView1.Rows.Add(row); //加回dataGridViewX1
            }
        }
        private void save()
        {
            List<testUDTClass> UDTList_insert = new List<testUDTClass>();
            List<testUDTClass> UDTList_delete = new List<testUDTClass>();
            bool tag_something_updated = false;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                // 0 => uuid
                // 1 => id 
                // 2 => note
                if (row.Cells[0].Value == null)
                {
                    if (row.Cells[1].Value == null && row.Cells[2].Value == null)
                        continue;
                    testUDTClass obj = new testUDTClass();
                    obj.ref_student_id = (row.Cells[id.Index].Value == null) ? "" : row.Cells[id.Index].Value.ToString();
                    obj.Note = (row.Cells[note.Index].Value == null) ? "" : row.Cells[note.Index].Value.ToString();
                    UDTList_insert.Add(obj);
                }
                else
                {
                    int idx = UDTDic[row.Cells[0].Value.ToString()];
                    if (row.Cells[1].Value == null && row.Cells[2].Value == null)
                    {
                        UDTList_delete.Add(UDTList[idx]);
                    }
                    else
                    {
                        forecah(row.Cells.Count)
                        string tmp_ref_student_id = (row.Cells[id.Index].Value == null) ? "" : row.Cells[id.Index].Value.ToString(); 
                        string tmp_Note= (row.Cells[note.Index].Value == null) ? "" : row.Cells[note.Index].Value.ToString();
                        if (tmp_ref_student_id != UDTList[idx].ref_student_id || tmp_Note!=UDTList[idx].Note)
                        {
                            UDTList[idx].ref_student_id = tmp_ref_student_id;
                            UDTList[idx].Note = tmp_Note;
                            tag_something_updated = true;
                        }
                    }
                }
            }
            if (tag_something_updated )
                _A.UpdateValues(UDTList);
            if (UDTList_insert.Count > 0)
                _A.InsertValues(UDTList_insert);
            if (UDTList_delete.Count > 0)
                _A.DeletedValues(UDTList_delete);
        }
    }
}
