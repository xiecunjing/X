﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;
using NewLife.Collections;

namespace XCode.DataAccessLayer
{
    /// <summary>
    /// 表构架
    /// </summary>
    [DebuggerDisplay("ID={ID} Name={Name} Description={Description}")]
    [Serializable]
    [XmlRoot("Table")]
    public class XTable : IDataTable, ICloneable
    {
        #region 基本属性
        private Int32 _ID;
        /// <summary>
        /// 编号
        /// </summary>
        [XmlAttribute]
        [Description("编号")]
        public Int32 ID { get { return _ID; } set { _ID = value; } }

        private String _Name;
        /// <summary>
        /// 表名
        /// </summary>
        [XmlAttribute]
        [Description("表名")]
        public String Name { get { return _Name; } set { _Name = value; _Alias = null; } }

        private String _Alias;
        /// <summary>
        /// 别名
        /// </summary>
        [XmlAttribute]
        [Description("别名")]
        public String Alias { get { return _Alias ?? (_Alias = GetAlias(Name)); } set { _Alias = value; } }

        private String _Description;
        /// <summary>
        /// 表说明
        /// </summary>
        [XmlAttribute]
        [Description("表说明")]
        public String Description { get { return _Description; } set { _Description = value; } }

        private Boolean _IsView = false;
        /// <summary>
        /// 是否视图
        /// </summary>
        [XmlAttribute]
        [Description("是否视图")]
        public Boolean IsView { get { return _IsView; } set { _IsView = value; } }

        private String _Owner;
        /// <summary>所有者</summary>
        [XmlAttribute]
        [Description("所有者")]
        public String Owner
        {
            get { return _Owner; }
            set { _Owner = value; }
        }

        private DatabaseType _DbType;
        /// <summary>数据库类型</summary>
        [XmlAttribute]
        [Description("数据库类型")]
        public DatabaseType DbType
        {
            get { return _DbType; }
            set { _DbType = value; }
        }
        #endregion

        #region 扩展属性
        private IDataColumn[] _Columns;
        /// <summary>
        /// 字段集合。
        /// </summary>
        [XmlArray("Columns")]
        [Description("字段集合")]
        public IDataColumn[] Columns
        {
            get { return _Columns; }
            set
            {
                _Columns = value;
                if (value == null)
                    _Fields = null;
                else
                {
                    _Fields = new List<XField>();
                    foreach (IDataColumn item in value)
                    {
                        if (item is XField) _Fields.Add(item as XField);
                    }
                }
            }
        }

        private List<XField> _Fields;
        /// <summary>
        /// 字段集合。
        /// </summary>
        [XmlIgnore]
        [Obsolete("建议使用Columns")]
        public List<XField> Fields
        {
            get
            {
                return _Fields;
            }
            set
            {
                _Fields = value;
                if (value == null)
                    _Columns = null;
                else
                {
                    List<IDataColumn> list = new List<IDataColumn>();
                    foreach (XField item in value)
                    {
                        list.Add(item);
                    }
                    _Columns = list.ToArray();
                }
            }
        }

        private IDataRelation[] _ForeignKeys;
        /// <summary>
        /// 外键集合。
        /// </summary>
        [XmlArray]
        [Description("外键集合")]
        public IDataRelation[] Relations { get { return _ForeignKeys; } set { _ForeignKeys = value; } }

        private IDataIndex[] _Indexes;
        /// <summary>
        /// 字段集合。
        /// </summary>
        [XmlArray]
        [Description("索引集合")]
        public IDataIndex[] Indexes { get { return _Indexes; } set { _Indexes = value; } }
        #endregion

        #region 构造
        /// <summary>
        /// 初始化
        /// </summary>
        public XTable() { }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        public XTable(String name) { Name = name; }
        #endregion

        #region 方法
        /// <summary>
        /// 创建字段
        /// </summary>
        /// <returns></returns>
        public virtual IDataColumn CreateColumn()
        {
            return XField.Create(this);
        }

        /// <summary>
        /// 创建外键
        /// </summary>
        /// <returns></returns>
        public virtual IDataRelation CreateRelation()
        {
            XRelation fk = new XRelation();
            fk.Table = this;
            return fk;
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <returns></returns>
        public virtual IDataIndex CreateIndex()
        {
            XIndex idx = new XIndex();
            idx.Table = this;
            return idx;
        }

        /// <summary>
        /// 已重载。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!String.IsNullOrEmpty(Description))
                return String.Format("{0}({1})", Description, Name);
            else
                return Name;
        }
        #endregion

        #region 导入导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public String Export()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(XTable));
            using (StringWriter sw = new StringWriter())
            {
                serializer.Serialize(sw, this);
                return sw.ToString();
            }
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static XTable Import(String xml)
        {
            if (String.IsNullOrEmpty(xml)) return null;

            XmlSerializer serializer = new XmlSerializer(typeof(XTable));
            using (StringReader sr = new StringReader(xml))
            {
                return serializer.Deserialize(sr) as XTable;
            }
        }
        #endregion

        #region ICloneable 成员
        /// <summary>
        /// /// 克隆
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public XTable Clone()
        {
            XTable table = base.MemberwiseClone() as XTable;
            if (table != null && Columns != null)
            {
                List<IDataColumn> list = new List<IDataColumn>();
                foreach (XField item in Columns)
                {
                    list.Add(item.Clone(table));
                }
                table.Columns = list.ToArray();
            }
            return table;
        }
        #endregion

        #region 辅助
        /// <summary>
        /// 获取别名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static String GetAlias(String name)
        {
            //TODO 很多时候，这个别名就是表名
            return name;
        }
        #endregion
    }
}