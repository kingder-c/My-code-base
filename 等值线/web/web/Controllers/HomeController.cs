using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using wContour;
using Newtonsoft.Json;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;

namespace web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// 计算等值线
        /// minLon 最小经度
        /// maxLon 最大经度
        /// minLat 最小纬度
        /// maxLat 最大维度
        /// sizeX 网格X方向大小（数量）
        /// sizeY 网格Y方向大小（数量）
        /// contour 等高线的值
        /// station 雨量站信息（二维数组  每个站点的经纬度和值）
        /// clip 裁剪多段线点坐标
        /// </summary>
        /// <returns>string json格式字符串 是一个对象数组 每个元素包含其是否闭合Type Close/Border 值Value 和点的坐标数组data</returns>
        public string getContourLine(double minLon, double maxLon, double minLat, double maxLat, int sizeX, int sizeY, string contour, string station, string clip)
        {
            double undefData = -9999.0;
            double[] X = createGrid(sizeX);
            double[] Y = createGrid(sizeY);
            double[] contourArray = strToArray(contour);
            double[,] stationArray = strToDArray(station);
            int nc = contourArray.Length;
            // 将地理坐标转换为网格坐标
            double[,] discreteData = convertToGrid(minLon, maxLon, minLat, maxLat, sizeX, sizeY, stationArray);
            double[,] dataArray = new double[sizeX, sizeY];
            // 插值
            dataArray = Interpolate.Interpolation_IDW_Neighbor(discreteData, X, Y, 8, undefData);

            int[,] S1 = new int[1, 1];
            // 求边界
            List<Border> borders = new List<Border>();
            borders = wContour.Contour.TracingBorders(dataArray, X, Y, ref S1, undefData);
            // 求等值线
            List<PolyLine> polyline = new List<PolyLine>();
            polyline = wContour.Contour.TracingContourLines(dataArray, X, Y, nc, contourArray, undefData, borders, S1);
            polyline = SmoothLines(polyline);
            List<List<PointD>> clipLines = GetEcllipseClipping(sizeX);
            //List<PolyLine> clipContourLines = ClipLines(clipLines, polyline);
            List<Polygon> polygon = new List<Polygon>();
            polygon = TracingPolygons(dataArray, polyline, borders, contourArray);
            List<Polygon> poly = ClipPolygons(clipLines, polygon);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(polygon.GetType());
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, polygon);
            byte[] dataBytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(dataBytes, 0, (int)stream.Length);
            return Encoding.UTF8.GetString(dataBytes);
        }
        /// <summary>
        /// 将地理坐标系转换为网格坐标
        /// </summary>
        /// <param name="minLon"></param>
        /// <param name="maxLon"></param>
        /// <param name="minLat"></param>
        /// <param name="maxlat"></param>
        /// <param name="sizeX"></param>
        /// <param name="sizeY"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private double[,] convertToGrid(double minLon, double maxLon, double minLat, double maxlat, int sizeX, int sizeY, double[,] data)
        {
            double unitLon = (maxLon - minLon) / sizeX;
            double unitLat = (maxlat - minLat) / sizeY;
            int dataLength = data.Length / 3;
            double[,] result = new double[3, dataLength];

            for (int i = 0; i < dataLength; i++)
            {
                int x = (int)((data[i, 0] - minLon) / unitLon);
                int y = (int)((data[i, 1] - minLat) / unitLat);
                result[0, i] = x;
                result[1, i] = y;
                result[2, i] = data[i, 2];
            }

            return result;
        }

        /// <summary>
        /// 将网格坐标转换为地理坐标
        /// </summary>
        /// <param name="minLon"></param>
        /// <param name="maxLon"></param>
        /// <param name="minLat"></param>
        /// <param name="maxlat"></param>
        /// <param name="sizeX"></param>
        /// <param name="sizeY"></param>
        /// <param name="data"></param>
        /// <returns></returns>

        private List<PointD> convertToLonlat(double minLon, double maxLon, double minLat, double maxlat, int sizeX, int sizeY, List<PointD> data)
        {
            double unitLon = (maxLon - minLon) / sizeX;
            double unitLat = (maxlat - minLat) / sizeY;
            int dataLength = data.Count;

            for (int i = 0; i < dataLength; i++)
            {
                data[i].X = minLon + unitLon * data[i].X;
                data[i].Y = minLat + unitLat * data[i].Y;
            }

            return data;
        }

        /// <summary>
        /// 构建网格
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private double[] createGrid(int size)
        {
            double[] grid = new double[size];
            for (int i = 0; i < size; i++)
            {
                grid[i] = i;
            }

            return grid;
        }

        /// <summary>
        /// 将数组格式的字符砖转换为数组
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private double[] strToArray(string str)
        {

            string[] temp = str.Split(',');
            double[] result = new double[temp.Length];
            for (int i = 0; i < temp.Length; i++)
            {
                result[i] = double.Parse(temp[i]);
            }

            return result;
        }
        /// <summary>
        /// 将二维数组格式字符串转换为二维数组
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private double[,] strToDArray(string str)
        {
            string[] temp = str.Split(',');
            double[,] result = new double[temp.Length / 3, 3];
            int length = temp.Length;
            for (int i = 0; i < length / 3; i++)
            {
                result[i, 0] = double.Parse(temp[i * 3]);
                result[i, 1] = double.Parse(temp[i * 3 + 1]);
                result[i, 2] = double.Parse(temp[i * 3 + 2]);
            }

            return result;
        }
        /// <summary>
        /// 将结果转换为json格式的字符串
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        private string getString(List<PolyLine> polyline)
        {
            string str = "[";
            for (int count = 0; count < polyline.Count; count++)
            {

                PolyLine polyl = polyline[count];
                str += "{" + "\'Type\':\'" + polyl.Type.ToString() + "\'," + " \'Value\':\'" + polyl.Value.ToString() + "\'," + " \'data\':[";
                List<PointD> pointList = polyline[count].PointList;
                for (int pointCount = 0; pointCount < pointList.Count; pointCount++)
                {
                    str += "[" + pointList[pointCount].X + "," + pointList[pointCount].Y + "]";
                    if (pointCount != pointList.Count - 1)
                    {
                        str += ",";
                    }
                }
                str += "]}";
                if (count != polyline.Count - 1)
                {
                    str += ",";
                }
            }
            str += "]";

            return str;
        }

        /// <summary>
        /// 字符串转坐标点
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private List<PointD> toPoints(string str)
        {
            double[] _str = strToArray(str);
            List<PointD> result = new List<PointD>();
            for (var i = 0; i < _str.Length; i += 2)
            {
                result.Add(new PointD(_str[i], _str[i + 1]));
            }

            return result;
        }

        private List<PolyLine> SmoothLines(List<PolyLine> lines)
        {
            return wContour.Contour.SmoothLines(lines);
        }

        public List<List<PointD>> GetEcllipseClipping(int size)
        {
            List<List<PointD>> _clipLines = new List<List<PointD>>();

            //---- Generate border with ellipse
            double x0 = 0;
            double y0 = 0;
            double a = 0;
            double b = 0;
            double c = 0;
            bool ifX = false;
            x0 = size / 2;
            y0 = size / 2;
            double dist = 0;
            dist = 100;
            a = x0 - dist;
            b = y0 - dist / 2;
            if (a > b)
            {
                ifX = true;
            }
            else
            {
                ifX = false;
                c = a;
                a = b;
                b = c;
            }

            int i = 0;
            int n = 0;
            n = 100;
            double nx = 0;
            double x1 = 0;
            double y1 = 0;
            double ytemp = 0;
            List<PointD> pList = new List<PointD>();
            List<PointD> pList1 = new List<PointD>();
            wContour.PointD aPoint;
            nx = (x0 * 2 - dist * 2) / n;
            for (i = 1; i <= n; i++)
            {
                x1 = dist + nx / 2 + (i - 1) * nx;
                if (ifX)
                {
                    ytemp = Math.Sqrt((1 - Math.Pow((x1 - x0), 2) / Math.Pow(a, 2)) * Math.Pow(b, 2));
                    y1 = y0 + ytemp;
                    aPoint = new PointD();
                    aPoint.X = x1;
                    aPoint.Y = y1;
                    pList.Add(aPoint);
                    aPoint = new PointD();
                    aPoint.X = x1;
                    y1 = y0 - ytemp;
                    aPoint.Y = y1;
                    pList1.Add(aPoint);
                }
                else
                {
                    ytemp = Math.Sqrt((1 - Math.Pow((x1 - x0), 2) / Math.Pow(b, 2)) * Math.Pow(a, 2));
                    y1 = y0 + ytemp;
                    aPoint = new PointD();
                    aPoint.X = x1;
                    aPoint.Y = y1;
                    pList1.Add(aPoint);
                    aPoint = new PointD();
                    aPoint.X = x1;
                    y1 = y0 - ytemp;
                    aPoint.Y = y1;
                    pList1.Add(aPoint);
                }
            }

            aPoint = new PointD();
            if (ifX)
            {
                aPoint.X = x0 - a;
            }
            else
            {
                aPoint.X = x0 - b;
            }
            aPoint.Y = y0;
            List<PointD> cLine = new List<PointD>();
            cLine.Add(aPoint);
            for (i = 0; i <= pList.Count - 1; i++)
            {
                cLine.Add(pList[i]);
            }
            aPoint = new PointD();
            aPoint.Y = y0;
            if (ifX)
            {
                aPoint.X = x0 + a;
            }
            else
            {
                aPoint.X = x0 + b;
            }
            cLine.Add(aPoint);
            for (i = pList1.Count - 1; i >= 0; i += -1)
            {
                cLine.Add(pList1[i]);
            }
            cLine.Add(cLine[0]);
            _clipLines.Add(cLine);

            return _clipLines;
        }

        public List<PolyLine> ClipLines(List<List<PointD>>_clipLines, List<PolyLine> _contourLines)
        {
            List<PolyLine> _clipContourLines = new List<PolyLine>();
            foreach (List<PointD> cLine in _clipLines)
                _clipContourLines.AddRange(Contour.ClipPolylines(_contourLines, cLine));

            return _clipContourLines;
        }

        public List<Polygon> TracingPolygons(double[,]_gridData, List<PolyLine> _contourLines, List<Border> _borders, double[] _CValues)
        {
            List<Polygon> _contourPolygons = Contour.TracingPolygons(_gridData, _contourLines, _borders, _CValues);

            return _contourPolygons;
        }

        public List<Polygon> ClipPolygons(List<List<PointD>> _clipLines, List<Polygon> _contourPolygons)
        {
            List<Polygon> _clipContourPolygons = new List<Polygon>();
            //for (int i = 0; i < _clipLines.Count; i++)
            //    _clipContourPolygons.AddRange(Contour.ClipPolygons(_contourPolygons, _clipLines[i]));

            //_clipContourPolygons.AddRange(Contour.ClipPolygons(_contourPolygons, _clipLines[20]));

            foreach (List<PointD> cLine in _clipLines)
                _clipContourPolygons.AddRange(Contour.ClipPolygons(_contourPolygons, cLine));

            return _clipContourPolygons;
        }
    }
}