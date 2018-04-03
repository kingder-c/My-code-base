using System;
using MongoRepository;
using System.Runtime.Serialization;

namespace IOTGateway.Data.Mongo
{
    /// <summary>
    /// 监测项配置表
    /// </summary>
    [CollectionName("ROBIN_HWSERVICE_CONFIG")]

    public class HWServiceConfigModel : Entity
    {
        /// <summary>
        /// 监测点编号
        /// </summary>

        public string STATION_KEY { get; set; }
        /// <summary>
        /// 检测项编码
        /// </summary>

        public string TAG_CODE { get; set; }
        /// <summary>
        /// 检测项描述
        /// </summary>

        public string TAG_DESC { get; set; }
        /// <summary>
        /// 监测项类型编号
        /// </summary>

        public string TAG_KEY { get; set; }
        /// <summary>
        /// 监测项类型名称
        /// </summary>

        public string TAG_NAME { get; set; }

        /// <summary>
        ///消息模板 用户短信发送时的模板
        /// </summary>

        public string Template { get; set; }

        /// <summary>
        /// 变量名称
        /// </summary>

        public string VARIABLE_NAME { get; set; }
        /// <summary>
        /// 页面相对位置X
        /// </summary>

        public decimal PAGE_X { get; set; }
        /// <summary>
        /// 页面相对位置Y
        /// </summary>

        public double PAGE_Y { get; set; }
        /// <summary>
        /// 最小正常值
        /// </summary>

        public double MIN_VALUE { get; set; }
        /// <summary>
        /// 最大正常值
        /// </summary>

        public double MAX_VALUE { get; set; }
        /// <summary>
        /// 量程最小值
        /// </summary>
        
        public double MIN_MIN_VALUE { get; set; }
        /// <summary>
        /// 量程最大值
        /// </summary>

        public double MAX_MAX_VALUE { get; set; }
        /// <summary>
        /// 单位
        /// </summary>

        public string UNITS { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>

        public int? ORDER_NUM { get; set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>

        public DateTime? SAVE_DATE { get; set; }
        /// <summary>
        /// 最后更新值
        /// </summary>

        public double? TAG_VALUE { get; set; }

        /// <summary>
        /// 监测值是否是正常值 1 正常 0不正常
        /// </summary>

        public string IsNormal { get; set; }

        /// <summary>
        /// 报警级别
        /// </summary>

        public string ALERTLEVEL { get; set; }

        /// <summary>
        /// 报警间隔 毫秒
        /// </summary>

        public int AlertInterval { get; set; }
        /// <summary>
        /// 监测点类型编号
        /// </summary>

        public String JCDCODE { get; set; }
        /// <summary>
        /// 监测点类型名称
        /// </summary>
        public string JCDNAME { get; set; }
        /// <summary>
        /// 相关联的的附属设备，对应的传感器编号
        /// </summary>

        public string JCSB { get; set; }
        /// <summary>
        /// 内部设施如泵站的水泵
        /// </summary>
         
        public string NBSS { get; set; }
        /// <summary>
        /// 彩色值
        /// </summary>
        public string COLOR_VALUE { get; set; }

        /// <summary>
        /// 显示精度
        /// </summary>

        public int? PRECISION { get; set; }

        /// <summary>
        /// 一级预警范围起始 最高
        /// </summary>
         
        public double L1_START { get; set; }
        /// <summary>
        /// 一级预警范围终止
        /// </summary>
         
        public double L1_END { get; set; }
        /// <summary>
        /// 一级预警返回值
        /// </summary>
         
        public double L1_ReturnValue { get; set; }
        /// <summary>
        /// 一级预警彩色值
        /// </summary>
         
        public string L1_COLOR_VALUE { get; set; }
        /// <summary>
        /// 二级预警范围起始
        /// </summary>
         
        public double L2_START { get; set; }
        /// <summary>
        /// 二级预警范围终止
        /// </summary>
         
        public double L2_END { get; set; }
        /// <summary>
        /// 二级预警返回值
        /// </summary>
         
        public double L2_ReturnValue { get; set; }
        /// <summary>
        /// 二级预警彩色值
        /// </summary>
         
        public string L2_COLOR_VALUE { get; set; }
        /// <summary>
        /// 三级预警范围起始
        /// </summary>
         
        public double L3_START { get; set; }
        /// <summary>
        /// 三级预警范围终止
        /// </summary>
         
        public double L3_END { get; set; }
        /// <summary>
        /// 三级预警返回值
        /// </summary>
         
        public double L3_ReturnValue { get; set; }
        /// <summary>
        /// 三级预警彩色值
        /// </summary>
         
        public string L3_COLOR_VALUE { get; set; }
        /// <summary>
        /// REMARK
        /// </summary>
         
        public string REMARK { get; set; }
        /// <summary>
        /// EXTENDCODE
        /// </summary>
         
        public string EXTENDCODE { get; set; }
        /// <summary>
        /// EXTENDCODE2
        /// </summary>
         
        public string EXTENDCODE2 { get; set; }
        /// <summary>
        /// EXTENDCODE3
        /// </summary>
         
        public string EXTENDCODE3 { get; set; }
        /// <summary>
        /// EXTENDCODE4
        /// </summary>
         
        public string EXTENDCODE4 { get; set; }
        /// <summary>
        /// EXTENDCODE5
        /// </summary>
         
        public string EXTENDCODE5 { get; set; }
        /// <summary>
        /// 是否启用报警 1开启 0不开启
        /// </summary>
         
        public string ENABLE { get; set; }

        ///// <summary>
        ///// 添加时间
        ///// </summary>
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        //public DateTime? ADDTIME { get; set; }

        /// <summary>
        /// 最小理想值
        /// </summary>
         
        public double MIN_IDEALVALUE { get; set; }

        /// <summary>
        /// 最大理想值
        /// </summary>
         
        public double MAX_IDEALVALUE { get; set; }


        /// <summary>
        /// 是否启用下行预警范围 1启用 0停用
        /// </summary>
         
        public string ENABLEDOWN { get; set; }


        /// <summary>
        /// 下行一级预警范围起始 最高
        /// </summary>
         
        public double L1_STARTDOWN { get; set; }
        /// <summary>
        /// 下行一级预警范围终止
        /// </summary>
         
        public double L1_ENDDOWN { get; set; }
        /// <summary>
        /// 下行一级预警返回值
        /// </summary>
         
        public double L1_DOWNReturnValue { get; set; }
        /// <summary>
        /// 下行一级预警彩色值
        /// </summary>
         
        public string L1_COLOR_VALUEDOWN { get; set; }
        /// <summary>
        /// 下行二级预警范围起始
        /// </summary>
         
        public double L2_STARTDOWN { get; set; }
        /// <summary>
        /// 下行二级预警范围终止
        /// </summary>
         
        public double L2_ENDDOWN { get; set; }
        /// <summary>
        /// 下行二级预警返回值
        /// </summary>
         
        public double L2_DOWNReturnValue { get; set; }
        /// <summary>
        /// 下行二级预警彩色值
        /// </summary>
         
        public string L2_COLOR_VALUEDOWN { get; set; }
        /// <summary>
        /// 下行三级预警范围起始
        /// </summary>
         
        public double L3_STARTDOWN { get; set; }
        /// <summary>
        /// 下行三级预警范围终止
        /// </summary>
         
        public double L3_ENDDOWN { get; set; }
        /// <summary>
        /// 下行三级预警返回值
        /// </summary>
         
        public double L3_DOWNReturnValue { get; set; }
        /// <summary>
        /// 下行三级预警彩色值
        /// </summary>
         
        public string L3_COLOR_VALUEDOWN { get; set; }


    }
}

