using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace DataDefineLanLib
{
    /// <summary>
    /// 資料定義物件
    /// </summary>
    public class DataDefineObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataDefineObject"/> class.
        /// </summary>
        public DataDefineObject() { }

        /// <summary>
        /// 初始化 物件.
        /// </summary>
        /// <param name="objectName">Name of the object.</param>
        /// <param name="desCol">The DES col.</param>
        public DataDefineObject(string objectName, StringCollection desCol)
        {
            this.ObjectName = objectName;
            foreach (var des in desCol)
            {
                this.Objectddl.Append(des).AppendLine();
            }
        }

        /// <summary>
        /// ObjectName 名稱
        /// </summary>
        public string ObjectName = String.Empty;

        /// <summary>
        /// ObjectDDL DDL定義值.
        /// </summary>
        public StringBuilder Objectddl = new StringBuilder();
    }

    /// <summary>
    /// DbObjectTypeList 列表
    /// </summary>
    public enum DbObjectTypeList
    {
        /// <summary>
        /// The none
        /// </summary>
        None,
        /// <summary>
        /// The stored procedure
        /// </summary>
        StoredProcedure,
        /// <summary>
        /// The function
        /// </summary>
        Function,
        /// <summary>
        /// The view
        /// </summary>
        View,
        /// <summary>
        /// The table
        /// </summary>
        Table,
        /// <summary>
        /// The mark down
        /// </summary>
        MarkDown
    }
}
