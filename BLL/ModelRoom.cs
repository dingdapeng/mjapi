﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Data;
using System.Data.SqlClient;
namespace BLL
{
    public class ModelRoom
    {
        /// <summary>
        /// 得到模版房间列表
        /// </summary>
        /// <returns></returns>
        public string GetModelRoomS(string roomtype,string price)
        {
            #region 参数判断
            string desc = "asc";
            if (!string.IsNullOrEmpty(price))
            {
                desc = "desc";
            }

            string whereand = "";
            if (!string.IsNullOrEmpty(roomtype))
            {
                switch (roomtype.ToLower())
                {
                    case "kct":
                        whereand = "and roomid=34";
                        break;
                    case "cf":
                        whereand = "and roomid=30";
                        break;
                    case "wsj":
                        whereand = "and roomid=35";
                        break;
                    case "znf":
                        whereand = "and roomid=14";
                        break;
                    case "ws":
                        whereand = "and roomid=75";
                        break;
                    case "sf":
                        whereand = "and roomid=5";
                        break;
                    default:
                        break;
                }
            } 
            #endregion


            #region 查询
            string sql = @"select  roomId, did,Extension5 as roomtype, roomName,frontCover,Extension13,Extension14,
 round((CAST(Extension1 as float)+CAST(Extension2 as float))/CAST(unit as float),0) as price,unit as mj from Room where Extension4='已审核'  
 and Extension6 is null  " + whereand + " order by price  " + desc + ";";


            SqlParameter[] parr = new SqlParameter[] { 
            
            };
            DataTable dt = SqlHelper.ExecuteDataTable(sql);

            List<object> lis = new List<object>();
            for (int d = 0; d < dt.Rows.Count; d++)
            {
                DataRow row = dt.Rows[d];

                string htp = "http://www.mj100.com/UploadFile/610/";
                string arrstr = row["Extension14"].ToSafeString() + row["Extension13"].ToSafeString();

                List<string> lispic = new List<string>();
                string[] arr = arrstr.Split(new char[] { ',', '’' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < arr.Length; i++)
                {
                    lispic.Add(htp + arr[i]);
                }

                var room = new { did = row["did"].ToSafeString(), roomtype = row["roomtype"].ToSafeString(), roomName = row["roomName"].ToSafeString(), price = row["price"].ToSafeString(),mj=row["mj"].ToSafeString(), frontCover = htp + row["frontCover"].ToSafeString(), pics = lispic };
                lis.Add(room);
            }
            
            #endregion


            return JsonConvert.SerializeObject(lis);
        }


        /// <summary>
        /// 得到模版房间列表
        /// </summary>
        /// <returns></returns>
        public string GetModelRoomS2(string roomtype, string price,string userroomid)
        {

            //select roomId from UserRoom where id=@userroomid;



           


            #region 参数判断
            string desc = "asc";
            if (!string.IsNullOrEmpty(price))
            {
                desc = "desc";
            }

            string whereand = "";
            if (!string.IsNullOrEmpty(roomtype))
            {
                switch (roomtype.ToLower())
                {
                    case "kct":
                        whereand = "and roomid=34";
                        break;
                    case "cf":
                        whereand = "and roomid=30";
                        break;
                    case "wsj":
                        whereand = "and roomid=35";
                        break;
                    case "znf":
                        whereand = "and roomid=14";
                        break;
                    case "ws":
                        whereand = "and roomid=75";
                        break;
                    case "sf":
                        whereand = "and roomid=5";
                        break;
                    default:
                        break;
                }
            }
            #endregion


            #region 查询
            string sql = @"select  roomId, did,Extension5 as roomtype, roomName,frontCover,Extension13,Extension14,
 round((CAST(Extension1 as float)+CAST(Extension2 as float))/CAST(unit as float),0) as price,unit as mj from Room where budget=0 and Extension4='已审核'  
 and Extension6 is null  " + whereand + " order by price  " + desc + ";";


            SqlParameter[] parr = new SqlParameter[] { 
            
            };
            DataTable dt = SqlHelper.ExecuteDataTable(sql);

            List<object> lis = new List<object>();
            string flagroomid = "0";
            #region 使用过的房间用这个
            if (!userroomid.IsEmpty())
            {
                object oroomid = SqlHelper.ExecuteScalar("select roomId from UserRoom where id=@userroomid;", new SqlParameter("@userroomid", userroomid));

                flagroomid = oroomid.ToSafeString();

                #region MyRegion
                for (int d = 0; d < dt.Rows.Count; d++)
                {
                    DataRow row = dt.Rows[d];

                    string htp = "http://www.mj100.com/UploadFile/610/";
                    string arrstr = row["Extension14"].ToSafeString() + row["Extension13"].ToSafeString();

                    List<string> lispic = new List<string>();
                    string[] arr = arrstr.Split(new char[] { ',', '’' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < arr.Length; i++)
                    {
                        lispic.Add(htp + arr[i]);
                    }
                    var room = new { did = row["did"].ToSafeString(), roomtype = row["roomtype"].ToSafeString(), roomName = row["roomName"].ToSafeString(), price = S(row["price"].ToSafeString()), mj = row["mj"].ToSafeString(), frontCover = htp + row["frontCover"].ToSafeString(), pics = lispic, beused = "1" };

                    if (room.did == flagroomid)
                    {
                        lis.Add(room);
                        break;
                    }

                }
                #endregion
            } 
            #endregion
           

            #region MyRegion
            for (int d = 0; d < dt.Rows.Count; d++)
            {
                DataRow row = dt.Rows[d];

                string htp = "http://www.mj100.com/UploadFile/610/";
                string arrstr = row["Extension14"].ToSafeString() + row["Extension13"].ToSafeString();

                List<string> lispic = new List<string>();
                string[] arr = arrstr.Split(new char[] { ',', '’' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < arr.Length; i++)
                {
                    lispic.Add(htp + arr[i]);
                }
                var room = new { did = row["did"].ToSafeString(), roomtype = row["roomtype"].ToSafeString(), roomName = row["roomName"].ToSafeString(), price = S(row["price"].ToSafeString()), mj = row["mj"].ToSafeString(), frontCover = htp + row["frontCover"].ToSafeString(), pics = lispic, beused = "0" };

                if (room.did != flagroomid)
                {
                    lis.Add(room);

                }

            }
            #endregion

            #endregion


            return JsonConvert.SerializeObject(lis);
        }
        /// <summary>
        /// 某一个模版间的详情
        /// </summary>
        /// <param name="did"></param>
        /// <returns></returns>
        public string GetModelRoomSExt(string did)
        {
          


            #region 查询
            string sql = @"select Extension2, Extension1, unit, roomId, did,Extension5 as roomtype, roomName,frontCover,Extension13,Extension14,
 round((CAST(Extension1 as float)+CAST(Extension2 as float))/CAST(unit as float),0) 
 as price from Room where did=@did";


            SqlParameter[] parr = new SqlParameter[] { 
            new SqlParameter("@did",did)
            };
            DataTable dt = SqlHelper.ExecuteDataTable(sql,parr);

           
                DataRow row = dt.Rows[0];

                string htp = "http://www.mj100.com/UploadFile/610/";
                string arrstr = row["Extension14"].ToSafeString() + row["Extension13"].ToSafeString();

                List<string> lispic = new List<string>();
                string[] arr = arrstr.Split(new char[] { ',', '’' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < arr.Length; i++)
                {
                    lispic.Add(htp + arr[i]);
                }

                var room = new { did = row["did"].ToSafeString(), roomtype = row["roomtype"].ToSafeString(), roomName = row["roomName"].ToSafeString(), price = S(row["price"].ToSafeString()), frontCover = htp + row["frontCover"].ToSafeString(), pics = lispic, mj = row["unit"].ToSafeString(), gyzj = row["Extension1"].ToSafeString(), jczj = row["Extension2"].ToSafeString() };
                
            

            #endregion


                return JsonConvert.SerializeObject(room);
        }


        public  string  S(string aa)
        {
            int a = int.Parse(aa);
            int y = a % 10;

            if (y <= 5)
            {
                a = a - y;
            }
            else
            {
                a = a + (10 - y);
            }

            return a.ToSafeString();
        }
    }
}
