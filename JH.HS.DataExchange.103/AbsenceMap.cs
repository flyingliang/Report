using FISCA.Presentation.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JH.HS.DataExchange._103
{
    public partial class AbsenceMap : BaseForm
    {
        private FISCA.UDT.AccessHelper _AccessHelper = new FISCA.UDT.AccessHelper();
        List<K12.Data.AbsenceMappingInfo> infoList;
        List<K12.Data.PeriodMappingInfo> infoList2 ;
        private List<AbsenceMapRecord> _MapRecords = new List<AbsenceMapRecord>();
        public AbsenceMap()
        {
            InitializeComponent();
            infoList= K12.Data.AbsenceMapping.SelectAll();
            infoList2 = K12.Data.PeriodMapping.SelectAll();
            _MapRecords = _AccessHelper.Select<AbsenceMapRecord>();
            DataGridViewColumn dgvc ;
            foreach (K12.Data.AbsenceMappingInfo each in infoList)
            {
                dgvc  = new DataGridViewCheckBoxColumn();
                dgvc.Width = 40;
                dgvc.Name = each.Name;
                dataGridView1.Columns.Add(dgvc); 
            }
            DataGridViewRow row;
            foreach (K12.Data.PeriodMappingInfo each in infoList2)
            {
                row = new DataGridViewRow();
                row.CreateCells(dataGridView1);
                row.Cells[0].Value = each.Name;
                for (int i = 0; i < infoList.Count; i++)
                {
                    row.Cells[i + 1].Value = false;   //??
                    foreach (AbsenceMapRecord item in _MapRecords)
                    {
                        if ( item.absence == infoList[i].Name && item.period == each.Name)
                            row.Cells[i + 1].Value = true;
                    }
                }
                dataGridView1.Rows.Add(row);
            }
        }
        private void buttonX1_Click(object sender, EventArgs e)
        {
            _AccessHelper.DeletedValues(_MapRecords);
            _MapRecords.Clear();
            AbsenceMapRecord amr;
            for (int i = 0; i < infoList2.Count; i++)
            {
                DataGridViewRow row = dataGridView1.Rows[i];
                amr = new AbsenceMapRecord();
                amr.period = infoList2[i].Name;
                for (int j = 0;j < infoList.Count; j++)
                {
                    if ((bool)row.Cells[j + 1].Value == true)
                    {
                        amr.absence = infoList[j].Name;
                        _MapRecords.Add(amr);
                    }
                }
            }
            _MapRecords.SaveAll();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
