using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeniusAutoInput
{
    class DataToXml
    {
  

        public void DataGridViewToXml(DataGridView dataGridView, String filePath)
        {
            DataGridViewToXml(new DataGridViewExportOptions(dataGridView), filePath);
        }

        public void DataGridViewToXml(DataGridViewExportOptions dataGridViewExportOption, String filePath)
        {
            DataGridViewToXml(new List<DataGridViewExportOptions>(new DataGridViewExportOptions[] { dataGridViewExportOption }), filePath);
        }

        public void DataGridViewToXml(List<DataGridViewExportOptions> dataGridViewExportOptions, String filePath)
        {
            if (dataGridViewExportOptions == null || dataGridViewExportOptions.Count == 0) return;

            DataSet dataSet = new DataSet();

            int sheetIndex = 1;
            List<String> sheetNames = new List<String>();
            try
            {
                foreach (DataGridViewExportOptions option in dataGridViewExportOptions)
                {  
                    if (sheetNames.Contains(option.WorkSheetName))
                    {
                        int i = 1;
                        while (true)
                        {
                            string newSheetName = option.WorkSheetName + i.ToString();
                            if (!sheetNames.Contains(newSheetName))
                            {
                                sheetNames.Add(newSheetName);
                                option.WorkSheetName = newSheetName;
                                break;
                            }
                            i++;
                        }
                    }
                    else
                    {
                        sheetNames.Add(option.WorkSheetName);
                    }
                    DataGridViewFillToDataSet(dataSet, option);
                    sheetIndex++;
                }
                ExportToXml(dataSet, filePath);
            }
            finally
            {
                dataSet.Dispose();
                GC.Collect();
            }
        }

        // 处理 DataGridView 中的数据以填充到指定的 DataSet 中      
        private void DataGridViewFillToDataSet(DataSet dataSet, DataGridViewExportOptions Option)
        {
            DataTable Table = new DataTable();
            Table.TableName = Option.WorkSheetName;

            if (Option.DataGridView == null) return;
 

            foreach (DataColumnExportOptions option in Option.VisibleColumnOptions)
            {
                if (!option.Visible) continue;
                Table.Columns.Add(new DataColumn(option.ColumnName));
            }



            foreach (DataGridViewRow dataRow in Option.DataGridView.Rows)
            {
                //if (dataRow.IsNewRow) continue;  

                DataRow Row = Table.NewRow();
                foreach (DataColumnExportOptions option in Option.VisibleColumnOptions)
                {
                    if (dataRow.Cells[option.ColumnName].Value == null)
                    {
                        Row[option.ColumnName] = "";
                    }
                    else
                    {
                        Row[option.ColumnName] = dataRow.Cells[option.ColumnName].Value.ToString();
                    }
                }
                Table.Rows.Add(Row);
            }

            dataSet.Tables.Add(Table);
        }


        /// <summary>      
        /// 保存 DataSet 数据到 Xml 文件      
        /// </summary>      
        /// <param name="dataSet">DataSet数据对象</param>      
        /// <param name="filePath">Xml 文件地址</param>  
        private void ExportToXml(DataSet dataSet, String filePath)
        {
 
            try
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            catch
            {
                return;
            }


            dataSet.WriteXml(filePath);
        }

        public void Xml2DataGridView(DataGridView dataGridView, String filePath)
        {
            System.Data.DataSet dataSet1 = new System.Data.DataSet(); ;
            dataGridView.Rows.Clear();
            dataSet1.ReadXml(filePath, XmlReadMode.Auto);

            DataTable dt = dataSet1.Tables[0];
            dataGridView.Rows.Add(dt.Rows.Count);
            for (int i = 0; i < dt.Rows.Count-1; i++)//防止读取最后一条空值
            {
                object[] data = dt.Rows[i].ItemArray;
                for (int j = 0; j < data.Length; j++)
                {
                    dataGridView[j, i].Value = data[j];
                }
            }
        }


    }

 
    /// <summary>      
    /// 导出数据字段属性选项类      
    /// </summary>  
    class DataColumnExportOptions
    {
        private String _ColumnName;
        private String _Caption;
        private Boolean _Visible;
        /// <summary>      
        /// 字段名称      
        /// </summary>  
        public String ColumnName
        {
            get { return _ColumnName; }
            set { _ColumnName = value; }
        }
        /// <summary>      
        /// 字段标题      
        /// </summary>  
        public String Caption
        {
            get { return _Caption; }
            set { _Caption = value; }
        }
        /// <summary>      
        /// 是否显示（导出）      
        /// </summary>  
        public Boolean Visible
        {
            get { return _Visible; }
            set { _Visible = value; }
        }
        /// <summary>      
        /// 构造函数      
        /// </summary>      
        /// <param name="fColumnName">字段名称</param>  
        public DataColumnExportOptions(String columnName)
            : this(columnName, columnName)
        {

        }
        /// <summary>      
        /// 构造函数      
        /// </summary>      
        /// <param name="fColumnName">字段名称</param>      
        /// <param name="fCaption">字段标题</param>  
        public DataColumnExportOptions(String columnName, String caption)
            : this(columnName, caption, true)
        {

        }
        /// <summary>      
        /// 构造函数      
        /// </summary>      
        /// <param name="fColumnName">字段名称</param>      
        /// <param name="fCaption">字段标题</param>      
        /// <param name="fVisible">是否显示（导出）</param>  
        public DataColumnExportOptions(String columnName, String caption, Boolean visible)
        {
            this._ColumnName = columnName;
            this._Caption = caption;
            this._Visible = visible;
        }
    }
 

    class DataGridViewExportOptions
    {
        private DataGridView _DataGridView;
        private List<DataColumnExportOptions> _ColumnOptions;
        private List<DataColumnExportOptions> _VisibleColumnOptions;
        private String _WorkSheetName;

        /// <summary>      
        /// 要导出到DataGridView对象      
        /// </summary>  
        public DataGridView DataGridView
        {
            get { return _DataGridView; }
            set { _DataGridView = value; }
        }
        /// <summary>      
        /// 导出的字段属性列表      
        /// </summary>  
        public List<DataColumnExportOptions> ColumnOptions
        {
            get { return _ColumnOptions; }
            set { _ColumnOptions = value; }
        }
        /// <summary>      
        /// 要导出的字段列表（只读）      
        /// </summary>  
        public List<DataColumnExportOptions> VisibleColumnOptions
        {
            get { return _VisibleColumnOptions; }
        }
        /// <summary>      
        /// 导出的工作表名称      
        /// </summary>  
        public String WorkSheetName
        {
            get { return _WorkSheetName; }
            set { _WorkSheetName = value; }
        }
        /// <summary>      
        /// 构造函数      
        /// </summary>      
        /// <param name="dataGridView">要导出到DataGridView对象</param>  
        public DataGridViewExportOptions(DataGridView dataGridView)
            : this(dataGridView, null)
        { }
        /// <summary>      
        /// 构造函数      
        /// </summary>      
        /// <param name="dataGridView">要导出到DataGridView对象</param>      
        /// <param name="columnOptions">导出的字段属性列表</param>  
        public DataGridViewExportOptions(DataGridView dataGridView, List<DataColumnExportOptions> columnOptions)
            : this(dataGridView, columnOptions, null) { }
        /// <summary>      
        /// 构造函数      
        /// </summary>      
        /// <param name="dataGridView">要导出到DataGridView对象</param>      
        /// <param name="columnOptions">导出的字段属性列表</param>      
        /// <param name="workSheetName">导出生成的工作表名称</param>  
        public DataGridViewExportOptions(DataGridView dataGridView, List<DataColumnExportOptions> columnOptions, String workSheetName)
        {
            if (dataGridView == null) return;

            this._DataGridView = dataGridView;
            if (columnOptions == null)
            {
                this._ColumnOptions = new List<DataColumnExportOptions>();
                foreach (DataGridViewColumn dataColumn in dataGridView.Columns)
                    this._ColumnOptions.Add(new DataColumnExportOptions(dataColumn.Name, dataColumn.HeaderText, dataColumn.Visible));
            }
            else
            {
                this._ColumnOptions = columnOptions;
            }

            if (String.IsNullOrEmpty(workSheetName))
                this._WorkSheetName = dataGridView.Name;
            else
                this._WorkSheetName = workSheetName;

            this._VisibleColumnOptions = new List<DataColumnExportOptions>();
            foreach (DataColumnExportOptions option in this._ColumnOptions)
            {
                if (option.Visible)
                    this._VisibleColumnOptions.Add(option);
            }
        }

    }
}
