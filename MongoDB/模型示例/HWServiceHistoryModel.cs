
using System;
using MongoRepository;
using MongoDB.Bson.Serialization.Attributes;



namespace IOTGateway.Data.Mongo
{
    /// <summary>
    /// 监测数据历史表
    /// </summary>
    [CollectionName("ROBIN_HWSERVICE_HISTORY")]
    public class HWServiceHistoryModel : Entity
    {

        /// <summary>
        /// 检测项编号
        /// </summary>

        public string TAG_KEY { get; set; }
        /// <summary>
        /// 检测项值
        /// </summary>

        public double TAG_VALUE { get; set; }
        /// <summary>
        /// 保存时间
        /// </summary>

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime SAVE_DATE { get; set; }
        /// <summary>
        /// 监测点编号
        /// </summary>

        public string STATION_KEY { get; set; }
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

    }
}

