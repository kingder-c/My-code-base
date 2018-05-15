using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Server;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SOESupport;
using ContourRestSOE2.common;
using System.Collections.Specialized;

using System.Runtime.InteropServices;


namespace ContourRestSOE2
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
    //Company: 正元智慧城市建设公司智慧城管事业部
    //Designer: ***
    //Coder: ***
    //Reviewer: 
    //Tester: ***
    //Version: 1.0
    //History:
    //2017-11-16 张久君 [创建]
    //2017-11-16 张久君 [编码]
    //2016-11-xx  程序员姓名 [修改] xxxxx
    /// </summary>
    public class WaterDepthContourHandlerBag
    {

        public static JsonObject WaterDepthContourPlane(IServerObjectHelper serverObjectHelper, List<DefinePoint> WaterDelthMonitorStationPointList)
        {
            JsonObject pResultJsonObject = new JsonObject();
            try
            {
                ISpatialReference pSpatialReference = Common.GetSpatialReferenceFromMapServer(serverObjectHelper);
                IGeoDataset pDEM_GeoDataSet = Common.GetGeoDataSetFromMapServer(serverObjectHelper);

                List<JsonObject> ContourLineList = new List<JsonObject>();

                //循环执行上传的积水监测点及监测值。每循环一次则计算一个监测点的等值线提取
                for (int i = 0; i < WaterDelthMonitorStationPointList.Count; i++)
                {
                    //获取监测站空间位置处的DEM高程
                    double pDEM_Height = Common.GetHeight(WaterDelthMonitorStationPointList[i].StationPoint, pDEM_GeoDataSet);
                    //DEM高程 + 监测点位置的水深监测值得到监测站位置处的积水水面高程
                    double pWaterHeight = pDEM_Height + WaterDelthMonitorStationPointList[i].value;
                    pWaterHeight = Math.Round(pWaterHeight, 2);
                    //进行栅格筛选，根据给定的高程（DEM高程 + 监测的积水深度）进行栅格删选，得到DEM高程值小于监测点处积水水面高程值的所有DEM
                    IGeoDataset pRasterCalculatorGeoDataSet = Common.RasterCalculator(pDEM_GeoDataSet, @"[1] < " + pWaterHeight.ToString());//该处30为测试值
                    //将筛选出的栅格转换为polygon面
                    IFeatureClass pPolygonFeatureClass = Common.RasterToPolygonFeatureClass(pRasterCalculatorGeoDataSet, pSpatialReference);
                    //通过积水深度监测站所在位置的空间点选择积水面范围
                    IPolygon pPolygon = Common.PointSelectPolygon(WaterDelthMonitorStationPointList[i].StationPoint, pPolygonFeatureClass);

                    //获取积水范围的面积（单位：平方米）
                    IArea pArea = pPolygon as IArea;

                    //通过积水面范围截取地形DEM。截取后的DEM用来进行积水点等值线、面的提取、积水体积的计算等功能的基础数据
                    IGeoDataset pWaterPolygon_DEM = Common.PolygonExtractionRaster(pPolygon as IPolygon4, pDEM_GeoDataSet);

                    //计算积水面范围内积水量（单位：立方米）
                    double pWaterVolume = Common.ComputeDEM_Volume(pWaterPolygon_DEM, pWaterHeight);

                    IGeoDataset pWaterDepth_GeoDataSet = Common.RasterCalculator(pWaterPolygon_DEM, pWaterHeight.ToString() + @" - [1]");

                    Dictionary<int, ReclassifyValue> pRasterReclassifyDictionary = null;

                    //对积水范围内的DEM数据进行栅格重分类
                    IGeoDataset pReclassifyGeoDataSet = Common.RasterReclassify(pWaterDepth_GeoDataSet, out pRasterReclassifyDictionary);

                    //将重分类的GeoDataSet数据转换为Polygon数据
                    JsonObject pPolygonJsonObject = Common.RasterToPolygonJsonObject(pReclassifyGeoDataSet, pSpatialReference, pRasterReclassifyDictionary);

                    pPolygonJsonObject.AddDouble("WaterVolume", pWaterVolume);
                    pPolygonJsonObject.AddDouble("WaterArea", pArea.Area);

                    //将每个监测站的积水点深度的等值线数据添加进列表中
                    ContourLineList.Add(pPolygonJsonObject);

                }

                pResultJsonObject.AddString("state", "success");
                pResultJsonObject.AddArray("WaterDepthContourPlaneArray", ContourLineList.ToArray());
            }
            catch (Exception ex)
            {
                pResultJsonObject.AddString("state", "fail");
            }
            return pResultJsonObject;
        }

        /// <summary>
        /// 实现根据积水点空间位置和监测值绘制积水深度等值线
        /// </summary>
        /// <param name="serverObjectHelper"></param>
        /// <param name="WaterDelthMonitorStationPointList"></param>
        /// <returns></returns>
        public static JsonObject WaterDepthContourLine(ISpatialReference pSpatialReference,IGeoDataset pDEM_GeoDataSet, List<DefinePoint> WaterDelthMonitorStationPointList)
        {
            JsonObject pResultJsonObject = new JsonObject();
            try
            {
                
                //获取DEM数据
                //IGeoDataset pDEM_GeoDataSet = pGeoDataSetQueue.Dequeue();

                List<JsonObject> ContourLineList = new List<JsonObject>();

                //循环执行上传的积水监测点及监测值。每循环一次则计算一个监测点的等值线提取
                for (int i = 0; i < WaterDelthMonitorStationPointList.Count; i++)
                {
                    //获取监测站空间位置处的DEM高程
                    double pDEM_Height = Common.GetHeight(WaterDelthMonitorStationPointList[i].StationPoint, pDEM_GeoDataSet);
                    //DEM高程 + 监测点位置的水深监测值得到监测站位置处的积水水面高程
                    double pWaterHeight = pDEM_Height + WaterDelthMonitorStationPointList[i].value;
                    pWaterHeight = Math.Round(pWaterHeight,2);
                    //进行栅格筛选，根据给定的高程（DEM高程 + 监测的积水深度）进行栅格删选，得到DEM高程值小于监测点处积水水面高程值的所有DEM
                    IGeoDataset pRasterCalculatorGeoDataSet = Common.RasterCalculator(pDEM_GeoDataSet, @"[1] < " + pWaterHeight.ToString());//该处30为测试值
                    //将筛选出的栅格转换为polygon面
                    IFeatureClass pPolygonFeatureClass = Common.RasterToPolygonFeatureClass(pRasterCalculatorGeoDataSet, pSpatialReference);
                    if (pPolygonFeatureClass == null)
                    {
                        pResultJsonObject.AddString("state", "fail");
                        pResultJsonObject.AddString("message", "栅格转换为polygon失败");
                        return pResultJsonObject;
                    }


                    //通过积水深度监测站所在位置的空间点选择积水面范围
                    IPolygon pPolygon = Common.PointSelectPolygon(WaterDelthMonitorStationPointList[i].StationPoint, pPolygonFeatureClass);

                    //IPolygon pSmoothPolygon = Common.DefinePolygonSmooth(pPolygon as IPolygon4);

                    //pPolygon.Smooth(3);
                    //获取积水范围的面积（单位：平方米）
                    IArea pArea = pPolygon as IArea;

                    //通过积水面范围截取地形DEM。截取后的DEM用来进行积水点等值线、面的提取、积水体积的计算等功能的基础数据
                    IGeoDataset pWaterPolygon_DEM = Common.PolygonExtractionRaster(pPolygon as IPolygon4, pDEM_GeoDataSet);

                    //计算积水面范围内积水量（单位：立方米）
                    double pWaterVolume = Common.ComputeDEM_Volume(pWaterPolygon_DEM, pWaterHeight);

                    IGeoDataset pWaterDepth_GeoDataSet = Common.RasterCalculator(pWaterPolygon_DEM, pWaterHeight.ToString() + @" - [1]");
                    //积水深度等值线的提取
                    JsonObject WaterDepthContourLine = Common.ContourLine(pWaterDepth_GeoDataSet,0.5);

                    //每个积水监测点对应的积水范围
                    JsonObject pWaterRangePolygon = ESRI.ArcGIS.SOESupport.Conversion.ToJsonObject(pPolygon, true);

                    JsonObject pWaterRangeAndContourLine = new JsonObject();
                    pWaterRangeAndContourLine.AddJsonObject("WaterRange", pWaterRangePolygon);
                    pWaterRangeAndContourLine.AddDouble("WaterVolume", pWaterVolume);
                    pWaterRangeAndContourLine.AddDouble("WaterArea", pArea.Area);
                    pWaterRangeAndContourLine.AddJsonObject("WaterContourLine", WaterDepthContourLine);

                    //将每个监测站的积水点深度的等值线数据添加进列表中
                    ContourLineList.Add(pWaterRangeAndContourLine);

                }

                pResultJsonObject.AddString("state", "success");
                pResultJsonObject.AddArray("WaterDepthContourLineArray", ContourLineList.ToArray());
                //pGeoDataSetQueue.Enqueue(pDEM_GeoDataSet);
                return pResultJsonObject;
            }
            catch (Exception ex)
            {
                pResultJsonObject.AddString("state","fail");
                return pResultJsonObject;
            }
            
        }
    }
}
