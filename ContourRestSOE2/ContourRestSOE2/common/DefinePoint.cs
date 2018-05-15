using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;

namespace ContourRestSOE2.common
{
    /// <summary>
    /// <see cref="IUserManager"/>接口定义了<see cref="User"/>实体类的增、删、改、
    /// （CRUD）方法，该接口的实现类将负责用户对象的简单管理。
    /// <remarks>增加使用例子
    /// <see cref="IUserManager"/>接口仅定义<see cref="User"/>实体对象的CRUD操作
    /// 并没有定义关联用户对象与群组、组织机构等的操作，这些业务逻辑操作的方法定义
    /// 专门的关联操作接口<see cref="IUserGroupAssociator"/>及<see cref="IUserOrgAssociator"/>中。
    /// </remarks>
    //Title: IUserManager
    //Copyright:  ***** Software LTD.co Copyright (c) 2006
    //Company: 正元智慧城市建设公司智慧市政事业部
    //Designer: ***
    //Coder: ***
    //Reviewer: 
    //Tester: ***
    //Version: 1.0
    //History:
    //2017-11-09  张久君 [创建]
    //2016-11-xx  程序员姓名 [编码]
    //2016-11-xx  程序员姓名 [修改] xxxxx
    /// </summary>
    public class DefinePoint
    {
        /// <summary>
        /// 监测站的空间坐标点
        /// </summary>
        public IPoint StationPoint
        {
            get;
            set;
        }

        /// <summary>
        /// 检测点的监测值
        /// </summary>
        public double value
        {
            get;
            set;
        }
    }
}
