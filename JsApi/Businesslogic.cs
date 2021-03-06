﻿using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Data;
using System.Reflection;

namespace JsApi
{
    public class Businesslogic
    {
        /// <summary>
        /// 得到户型图
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static string GetHxt(string userid)
        {

            return SqlHelper.ExecuteScalar("select Extension5 from DemandShowRooms where UserId='" + userid + "'").ToSafeString();
        }

        /// <summary>
        /// 抽奖
        /// </summary>
        /// <returns></returns>
        public static string Cj(string openid, string nickname)
        {




            int nn = Convert.ToInt32(SqlHelper.ExecuteScalar("  select COUNT(*) from WinningRecord where openid='" + openid + "'"));

            if (nn < 4)
            {
                #region MyRegion
                Random r = new Random(DateTime.Now.Second);

                int v = r.Next(1, 101);

                int rr = 1;



                string pricename = "500元家装代金券";

                if (v < 90)
                {

                    pricename = "500元家装代金券";
                    rr = 1;
                }

                else if (v >= 90 & v < 100)
                {
                    int n = GettodyprizeCount("2元现金红包");
                    if (n >= 10)
                    {
                        pricename = "500元家装代金券";
                        rr = 1;
                    }
                    else
                    {
                        pricename = "2元现金红包";
                        rr = 0;
                    }

                }
                else if (v == 100)
                {
                    int n = GettodyprizeCount("10元现金红包");
                    if (n >= 2)
                    {
                        pricename = "500元家装代金券";
                        rr = 1;
                    }
                    else
                    {
                        rr = 2;
                        pricename = "10元现金红包";
                    }

                }

                AddWinningRecord(openid, nickname, pricename);

                UpdateChanceCount(openid, -1);

                return rr.ToSafeString();
                #endregion
            }
            else
            {
                return "-1";
            }


        }

        /// <summary>
        /// 砍价
        /// </summary>
        /// <returns></returns>
        public static string KanJa(string openid, string nickname, string foropenid, string headimg)
        {
            if (headimg.IsEmpty())
            {
                headimg = "http://www.mj100.com/img/header/z17.png";
            }
            if (string.IsNullOrEmpty(openid)||string.IsNullOrEmpty(headimg))
            {
                  return "{\"errorcode\":5,\"num\":" + 0 + ",\"msg\":\"未知错误\"}";
            }
            const int con = 1500;//上限

            bool cg = Convert.ToInt32(SqlHelper.ExecuteScalar("select COUNT(*) from  Bargain where openid='" + openid + "' and foruserid='" + foropenid + "'")) == 0;

            //没抽过
            if (cg)
            {
                int totle = 0;
                object o = SqlHelper.ExecuteScalar("select sum(value) from Bargain  where  foruserid='" + foropenid + "' group by foruserid");



                if (o != null)
                {
                    totle = Convert.ToInt32(o);
                }

                int num = new Random().Next(1, 16);//随机的钱数

                string v = SqlHelper.ExecuteScalar(" select top 1  (id+4)%7+4 from Bargain where foruserid='" + foropenid + "';").ToSafeString();
                if (!v.IsEmpty())
                {
                    int k = int.Parse(v);
                    int n1 = k + 7; int n2 = k + 13; int n3 = k + 1;


                    int t=Convert.ToInt32( SqlHelper.ExecuteScalar( " select COUNT(*) from Bargain where foruserid='"+foropenid+"';"));
                    if (t==n1||t==n2 ||t==n3)
                    {
                        num = new Random().Next(45,51);
                    }


                }


                if (totle >= con)
                {
                    return "{\"errorcode\":4,\"num\":" + 0 + ",\"msg\":\"已达上限，无需再砍，谢谢你的参与\"}";
                }

                if ((num + totle) >= con)
                {
                    num = 10000 - totle;
                }

                #region 构造插入函数
                string sql = @"insert Bargain (  openid, nickname, headimg, foruserid, value, Bargainname)

values( @openid, @nickname, @headimg, @foruserid, @value, '京东家装节砍价');";


                SqlParameter[] arr = new SqlParameter[] {
            new SqlParameter("openid",openid),
            new SqlParameter("nickname",nickname),
            new SqlParameter("headimg",headimg),
             new SqlParameter("foruserid",foropenid),
                new SqlParameter("value",num),
            };
                #endregion

                SqlHelper.ExecuteNonQuery(sql, arr);

                if (openid == foropenid)
                {
                    return "{\"errorcode\":3,\"num\":" + num + ",\"msg\":\"你为自己砍了\"}";
                }

                return "{\"errorcode\":0,\"num\":" + num + ",\"msg\":\"帮砍成功\"}";



            }
            else
            {
                if (openid == foropenid)
                {
                    return "{\"errorcode\":2,\"num\":" + 0 + ",\"msg\":\"你自己已经砍过了\"}";
                }


                return "{\"errorcode\":1,\"num\":" + 0 + ",\"msg\":\"已帮砍过了哦\"}";
            }
        }

        /// <summary>
        /// 得到分享次数
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static int GetShareCount(string openid)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(" select COUNT(*) from ShareRecord where openid='" + openid + "'"));
        }

        /// <summary>
        /// 得到奖品当天发放数量
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GettodyprizeCount(string value)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar("  select COUNT(*) from WinningRecord where DATEDIFF(DD,drawtime,GETDATE())=0 and prizename='" + value + "'  "));
        }

        /// <summary>
        /// 添加分享记录
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="nickname"></param>
        /// <param name="shraedesc"></param>
        /// <returns></returns>
        public static bool AddShareRecord(string openid, string nickname, string shraedesc)
        {
            string sql = "insert into ShareRecord(openid,nickname,shraedesc) values(@openid,@nickname,@shraedesc);";

            SqlParameter[] arr = new SqlParameter[] { 
             new SqlParameter("@openid",openid),
             new SqlParameter("@nickname",nickname),
             new SqlParameter("@shraedesc",shraedesc)
            };

            return SqlHelper.ExecuteNonQuery(sql, arr) > 0;

        }

        /// <summary>
        /// 分享
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static int Share(string openid, string nickname)
        {
            object o = SqlHelper.ExecuteScalar(" select COUNT(*) as num from ShareRecord where openid='" + openid + "'");

            int v = Convert.ToInt32(o);

            if (v == 0)
            {
                UpdateChanceCount(openid, 2);
            }
            else if (v == 1)
            {

                UpdateChanceCount(openid, 1);
            }
            AddShareRecord(openid, nickname, "抽奖分享");


            return v;

        }

        /// <summary>
        /// 查看还有几次抽奖机会
        /// </summary>
        /// <returns></returns>
        public static int GetChanceCount(string openid)
        {
            object n = SqlHelper.ExecuteScalar("  select WebChartUser.chancecount from WebChartUser where openid='" + openid + "'");

            return Convert.ToInt32(n);

        }

        /// <summary>
        /// 修改机会次数
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static bool UpdateChanceCount(string openid, int n)
        {

            //update WebChartUser set chancecount='1' where openid='o8r91jjmQWUqO8zrq4rxL0QVTEYs'
            string sql = "";
            if (n > 0)
            {
                sql = "update WebChartUser set chancecount=chancecount+" + n.ToSafeString() + " where openid='" + openid + "'";
            }
            else
            {
                sql = "update WebChartUser set chancecount=chancecount - " + Math.Abs(n) + " where openid='" + openid + "'";
            }


            SqlHelper.ExecuteNonQuery(sql);

            return true;

        }

        /// <summary>
        /// 添加中奖记录
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="nickname"></param>
        /// <param name="prizename"></param>
        /// <returns></returns>
        public static bool AddWinningRecord(string openid, string nickname, string prizename)
        {
            string sql = " insert into WinningRecord(openid,nickname,prizename) values(@openid,@nickname,@prizename);";

            SqlParameter[] arr = new SqlParameter[] { 
             new SqlParameter("@openid",openid),
             new SqlParameter("@nickname",nickname),
             new SqlParameter("@prizename",prizename)
            };

            return SqlHelper.ExecuteNonQuery(sql, arr) > 0;

        }

        /// <summary>
        /// 添加微信用户
        /// </summary>
        /// <param name="openid"></param>
        public static void AddWeChartUser(string openid)
        {
            string sql = @"declare @num int set @num=0;
select @num=count(openid) from WebChartUser where openid=@openid;
if(@num<1)begin  
insert into WebChartUser (openid) values(@openid);
end";

            SqlHelper.ExecuteNonQuery(sql, new SqlParameter("@openid", openid));
        }


        /// <summary>
        /// 添加微信用户
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="nickname"></param>
        /// <param name="headimgurl"></param>
        public static void AddWechartUser2(string openid, string nickname, string headimgurl)
        {
            string sql = @"declare @num int set @num=0;
select @num=count(openid) from WebChartUser where openid=@openid;
if(@num<1)begin  
insert into WebChartUser (openid,nickname,headimgurl,chancecount) values(@openid,@nickname,@headimgurl,1);
end
";

            SqlHelper.ExecuteNonQuery(sql, new SqlParameter("@openid", openid), new SqlParameter("@nickname", nickname), new SqlParameter("@headimgurl", headimgurl));
        }


        public static void AddWechartUser3(string openid, string nickname, string headimgurl)
        {
            string sql = @"declare @num int set @num=0;
select @num=count(openid) from WebChartUser where openid=@openid;
if(@num<1)begin  
insert into WebChartUser (openid,nickname,headimgurl,chancecount) values(@openid,@nickname,@headimgurl,1);
end
else 
begin
update WebChartUser set nickname=@nickname, headimgurl=@headimgurl where openid=@openid
end
";

            SqlHelper.ExecuteNonQuery(sql, new SqlParameter("@openid", openid), new SqlParameter("@nickname", nickname), new SqlParameter("@headimgurl", headimgurl));
        }

        /// <summary>
        /// 得到微信用户
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static JsApi.WebChartUser GetWebChartUser(string openid)
        {
            DataTable dt = SqlHelper.ExecuteDataTable(" select * from WebChartUser where openid=@id", new SqlParameter("@id", openid));

            if (dt.Rows.Count > 0)
            {
                return ConvertToList<JsApi.WebChartUser>(dt)[0];
            }
            else
            {
                return new JsApi.WebChartUser();
            }
        }


        public static void AddDemand(string phone, string serverId, string address, string openid, string Description, string functionrooms, string area, string themes, string budget)
        {


            //先删除需求


            #region 插入需求表

            string sql = "insert into DemandShowRooms (UserId,Extension5,createtime,Extension,extension2,Description) values(@UserId,@hxt,@createtime,'未审核',@extension2,@extension1);";
            string userid = "";
            object o = SqlHelper.ExecuteScalar("select UserId from Users where LoginName=@phone or UserMPhone=@phone;", new SqlParameter("@phone", phone));
            if (o != null)
            {
                userid = o.ToString();
                #region 清空所有
                SqlHelper.ExecuteNonQuery(@"declare @userid nvarchar(50) set @userid='" + userid + @"'
declare @DemandShowroomId nvarchar(50)
select @DemandShowroomId=DemandShowroomId from DemandShowRooms where UserId=@userid
delete from [DemandShowRooms]  where DemandShowroomId=@DemandShowroomId   --删除需求
			delete from [DemandShowRooms]  where UserId=@UserId   --删除需求
			delete from UserRoom where demandId=@DemandShowroomId   --删除用户房间配置
			delete from UserRoom where userId=@UserId   --删除用户房间配置
			delete from DemandShowRoomProduct where DemandShowroomId=@DemandShowroomId   --删除用户房间对应的建材
			delete from DemandYppCenter where TypeId=@DemandShowroomId					--删除用户房间对应的施工
			delete from Daily where userid=@UserId  --删除中轴日志
			delete from Stage where EngineeringId in (select engineeringId from Engineering where demandId=@DemandShowroomId)  --删除施工对应工序
			delete from Engineering where demandId=@DemandShowroomId		--删除施工
			delete from OrderDetail where DemandId in (select ot.Id from OrderMessage om left join OrderType ot on om.Id=ot.ParentId left join OrderDetail as odl on ot.Id=odl.DemandId where om.DemandId=@DemandShowroomId)		--删除订单详情
			delete from OrderType where ParentId in (select Id from OrderMessage where DemandId=@DemandShowroomId)  --删除订单类型
		    delete from OrderMessage where DemandId=@DemandShowroomId  --删除订单
		    delete from OrderMessage where UserId=@UserId  --删除订单
		    delete from XState where extension1=@DemandShowroomId  --删除施工日志
		    delete from xStatePic where demandId=@DemandShowroomId  --删除对应图片
		    delete from xgrlrz where extension=@DemandShowroomId    --删除日志");//删除需求 
                #endregion
            }
            else
            {
                o = SqlHelper.ExecuteScalar("insert into Users(LoginName,UserMPhone)values(@phone,@phone) select @@IDENTITY;", new SqlParameter("@phone", phone));
                userid = o.ToString();
            }
            SqlParameter[] arr = new SqlParameter[] { 
            new SqlParameter("@UserId",userid),
            new SqlParameter("@hxt",Dowload(serverId,JsApi.JsToken.GetApptoken(),userid)),
            new SqlParameter("@createtime",DateTime.Now.ToString()),
            new SqlParameter("@extension2",address.Contains("定位中")?"":address),
            new SqlParameter("@extension1",Description)
        
            };
            int n = SqlHelper.ExecuteNonQuery(sql, arr);
            #endregion

            //            functionrooms nvarchar(1024) ,


            //area nvarchar(1024),


            //themes nvarchar(1024),

            //budget nvarchar(1024)

            #region 修改用户微信状态userid
            SqlHelper.ExecuteNonQuery("update WebChartUser set userid=@userid ,functionrooms=@functionrooms,area=@area,themes=@themes,budget=@budget where openid=@openid", new SqlParameter("@userid", userid), new SqlParameter("@openid", openid), new SqlParameter("@functionrooms", functionrooms), new SqlParameter("@area", area), new SqlParameter("@themes", themes), new SqlParameter("@budget", budget));
            #endregion


            RemindCC(phone);
        }


        /// <summary>
        /// 提交需求
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="phone"></param>
        /// <param name="time"></param>
        /// <param name="name"></param>
        public static void AddDemand(string phone, string serverId, string address, string openid, string Description)
        {


            //先删除需求


            #region 插入需求表

            string sql = "insert into DemandShowRooms (UserId,Extension5,createtime,Extension,extension2,Description) values(@UserId,@hxt,@createtime,'未审核',@extension2,@extension1);";
            string userid = "";
            object o = SqlHelper.ExecuteScalar("select UserId from Users where LoginName=@phone or UserMPhone=@phone;", new SqlParameter("@phone", phone));
            if (o != null)
            {
                userid = o.ToString();
                #region 清空所有
                SqlHelper.ExecuteNonQuery(@"declare @userid nvarchar(50) set @userid='" + userid + @"'
declare @DemandShowroomId nvarchar(50)
select @DemandShowroomId=DemandShowroomId from DemandShowRooms where UserId=@userid
delete from [DemandShowRooms]  where DemandShowroomId=@DemandShowroomId   --删除需求
			delete from [DemandShowRooms]  where UserId=@UserId   --删除需求
			delete from UserRoom where demandId=@DemandShowroomId   --删除用户房间配置
			delete from UserRoom where userId=@UserId   --删除用户房间配置
			delete from DemandShowRoomProduct where DemandShowroomId=@DemandShowroomId   --删除用户房间对应的建材
			delete from DemandYppCenter where TypeId=@DemandShowroomId					--删除用户房间对应的施工
			delete from Daily where userid=@UserId  --删除中轴日志
			delete from Stage where EngineeringId in (select engineeringId from Engineering where demandId=@DemandShowroomId)  --删除施工对应工序
			delete from Engineering where demandId=@DemandShowroomId		--删除施工
			delete from OrderDetail where DemandId in (select ot.Id from OrderMessage om left join OrderType ot on om.Id=ot.ParentId left join OrderDetail as odl on ot.Id=odl.DemandId where om.DemandId=@DemandShowroomId)		--删除订单详情
			delete from OrderType where ParentId in (select Id from OrderMessage where DemandId=@DemandShowroomId)  --删除订单类型
		    delete from OrderMessage where DemandId=@DemandShowroomId  --删除订单
		    delete from OrderMessage where UserId=@UserId  --删除订单
		    delete from XState where extension1=@DemandShowroomId  --删除施工日志
		    delete from xStatePic where demandId=@DemandShowroomId  --删除对应图片
		    delete from xgrlrz where extension=@DemandShowroomId    --删除日志");//删除需求 
                #endregion
            }
            else
            {
                o = SqlHelper.ExecuteScalar("insert into Users(LoginName,UserMPhone)values(@phone,@phone) select @@IDENTITY;", new SqlParameter("@phone", phone));
                userid = o.ToString();
            }
            SqlParameter[] arr = new SqlParameter[] { 
            new SqlParameter("@UserId",userid),
            new SqlParameter("@hxt",Dowload(serverId,JsApi.JsToken.GetApptoken(),userid)),
            new SqlParameter("@createtime",DateTime.Now.ToString()),
            new SqlParameter("@extension2",address.Contains("定位中")?"":address),
            new SqlParameter("@extension1",Description)
        
            };
            int n = SqlHelper.ExecuteNonQuery(sql, arr);
            #endregion



            #region 修改用户微信状态userid
            SqlHelper.ExecuteNonQuery("update WebChartUser set userid=@userid where openid=@openid", new SqlParameter("@userid", userid), new SqlParameter("@openid", openid));
            #endregion


            RemindCC(phone);
        }


        /// <summary>
        /// 得到用户id
        /// </summary>
        public static string GetUserid(string openid)
        {

            object userid = SqlHelper.ExecuteScalar(" select userid from WebChartUser where openid='" + openid + "';");

            return userid.ToSafeString();


            //得到四个阶段
        }

        /// <summary>
        /// 删除预约
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static string DeleteYY(string userid)
        {

            #region 清空所有
            int n = SqlHelper.ExecuteNonQuery(@"update WebChartUser set userid='' where userid='" + userid + @"'
delete from DemandShowRooms  where UserId='" + userid + @"'
delete from Tentent  where UserId='" + userid + @"'
");//删除需求 
            #endregion

            return n.ToString();
        }


        /// <summary>
        /// 删除预约
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static string UpdateTime(string userid, string time)
        {

            #region 清空所有
            int n = SqlHelper.ExecuteNonQuery("update Tentent set Extension3='" + time + "' where UserId='" + userid + "'");
            #endregion

            return n.ToString();
        }


        /// <summary>
        /// 得到用户手机号
        /// </summary>
        public static string GetUserPhone(string userid)
        {

            object phone = SqlHelper.ExecuteScalar("  select LoginName  from Users where UserId='" + userid + "';;");

            return phone.ToSafeString();


            //得到四个阶段
        }

        /// <summary>
        /// 预约量房
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="phone"></param>
        /// <param name="time"></param>
        /// <param name="name"></param>
        public static void MakeAnAppointment(string phone, string name)
        {
            string sql = "insert into Tentent(UserId,Extension1,Extension3,Extension4) values(@UserId,@phone,@time,@name);";
            string userid = "";
            object o = SqlHelper.ExecuteScalar("select UserId from Users where LoginName=@phone or UserMPhone=@phone;", new SqlParameter("@phone", phone));
            if (o != null)
            {
                userid = o.ToString();
            }
            else
            {
                o = SqlHelper.ExecuteScalar("insert into Users(LoginName,UserMPhone)values(@phone,@phone) select @@IDENTITY;", new SqlParameter("@phone", phone));
                userid = o.ToString();
            }
            SqlParameter[] arr = new SqlParameter[] { 
            new SqlParameter("@UserId",userid),
            new SqlParameter("@phone",phone),
            new SqlParameter("@time",DateTime.Now.ToString("yy-MM-dd")),
            new SqlParameter("@name",name)
            };
            SqlHelper.ExecuteNonQuery(sql, arr);

        }




        /// <summary>
        /// 微信绑定预约量房
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="phone"></param>
        /// <param name="time"></param>
        /// <param name="name"></param>
        public static void MakeAnAppointmentWx(string phone, string name, string openid)
        {
            #region MyRegion
            string sql = "insert into Tentent(UserId,Extension1,Extension3,Extension4,createtime) values(@UserId,@phone,@time,@name,'" + DateTime.Now.ToString() + "');";
            string userid = "";
            object o = SqlHelper.ExecuteScalar("select UserId from Users where LoginName=@phone or UserMPhone=@phone;", new SqlParameter("@phone", phone));
            if (o != null)
            {
                userid = o.ToString();
            }
            else
            {
                o = SqlHelper.ExecuteScalar("insert into Users(LoginName,UserMPhone)values(@phone,@phone) select @@IDENTITY;", new SqlParameter("@phone", phone));
                userid = o.ToString();
            }
            SqlParameter[] arr = new SqlParameter[] { 
            new SqlParameter("@UserId",userid),
            new SqlParameter("@phone",phone),
            new SqlParameter("@time",DateTime.Now.ToString("yy-MM-dd")),
            new SqlParameter("@name",name)
            };
            #endregion

            object isyy = SqlHelper.ExecuteScalar("select count(*) from Tentent where UserId='" + userid + "';");
            if (isyy.ToSafeString() == "0")
            {
                #region 预约
                SqlHelper.ExecuteNonQuery(sql, arr);
                #endregion
            }



            #region 更新用户id
            SqlHelper.ExecuteNonQuery("update WebChartUser set userid=@userid where openid=@openid", new SqlParameter("@userid", userid), new SqlParameter("@openid", openid));
            #endregion
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="accestoken"></param>
        /// <returns></returns>
        public static string Dowload(string serverId, string accestoken, string userid)
        {
            if (serverId.IsEmpty())
            {
                return "";
            }

            Directory.CreateDirectory(Path.GetDirectoryName(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\HXT\\" + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString() + "\\"));
            System.Net.WebClient clinet = new System.Net.WebClient();
            clinet.DownloadFile("http://file.api.weixin.qq.com/cgi-bin/media/get?access_token=" + accestoken + "&media_id=" + serverId, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\HXT\\" + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString() + "\\" + userid + ".jpg");

            return "http://mobile.mj100.com/HXT/" + DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + userid + ".jpg";


        }

        /// <summary>
        /// 发送提醒
        /// </summary>
        /// <returns></returns>
        public static string Remind(string userid, string openid)
        {
            #region 更新是否发送
            SqlHelper.ExecuteNonQuery("UPDATE WebChartUser set sendid='" + userid + "' where openid='" + openid + "'"); 
            #endregion

            //查询出设计师的信息  id  name    电话

            JsApi.DesignerGrade d = JsApi.Businesslogic.GetDesingerGradeext(userid);

            return Commen.HttpRequest.PostMa("https://api.weixin.qq.com/cgi-bin/message/template/send?access_token=" + JsApi.JsToken.GetApptoken(), new Template.Notice5(openid, "http://mobile.mj100.com/wechart/login5/" + userid, "#FF0000", "点击进入服务查询了解最新的服务进度>>", d.DID, d.MPhone, DateTime.Now.ToSafeString(), "我们已为您分配好您的专属设计师啦！设计师上门量尺前会与您电话联系，请保持电话畅通！").ToString(), Encoding.UTF8);

        }
        /// <summary>
        /// 提醒后台管理员
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public static string RemindCC(string phone)
        {
            Commen.HttpRequest.PostMa("https://api.weixin.qq.com/cgi-bin/message/template/send?access_token=" + JsApi.JsToken.GetApptoken(), new Template.Notice4("o8r91jv006I_SCONLoJw6eACZEVM", "http://img.mj100.com/weixin/ueditor/userlist.aspx", "#FF0000", "tony女士你好,有人预约设计师啦，赶紧去后台查看最新", phone, phone, "上门量房", "上门量房时间待定，请及时联系客户", "无", "点击查看详情").ToString(), Encoding.UTF8);
            return "";
        }

        /// <summary>
        /// 能提交返回true
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static bool NoExpire(string openid)
        {
            string sql = " select top 1 CreateTime from DemandShowRooms where UserId in (select userid from WebChartUser where openid='" + openid + "')";


            object o = SqlHelper.ExecuteScalar(sql);

            if (o == null)
            {
                return true;
            }

            DateTime dt = Convert.ToDateTime(o);


            TimeSpan ts = DateTime.Now.Subtract(dt);


            if (ts.TotalMinutes > 60)
            {
                return true;
            }


            return false;
        }

        /// <summary>
        /// 得到户型图路径
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static string GetHxtUrl(string openid)
        {

            return SqlHelper.ExecuteScalar("select Extension5 from DemandShowRooms where UserId in (select UserId from WebChartUser where openid='" + openid + "')").ToSafeString();
        }

        /// <summary>
        /// 得到设计师
        /// </summary>
        /// <returns></returns>
        public static JsApi.DesignerGrade GetDesingerGrade(string id)
        {


            DataTable dt = SqlHelper.ExecuteDataTable("select * from DesignerGrade where  ID=@id", new SqlParameter("@id", id));

            if (dt.Rows.Count > 0)
            {
                return ConvertToList<JsApi.DesignerGrade>(dt)[0];
            }
            else
            {
                return new JsApi.DesignerGrade();
            }


        }
        /// <summary>
        /// 得到设计师
        /// </summary>
        /// <returns></returns>
        public static JsApi.DesignerGrade GetDesingerGradeext(string userid)
        {


            DataTable dt = SqlHelper.ExecuteDataTable("select * from DesignerGrade where  ID  in (select Extension9 from DemandShowRooms where UserId=@id)", new SqlParameter("@id", userid));

            if (dt.Rows.Count > 0)
            {
                return ConvertToList<JsApi.DesignerGrade>(dt)[0];
            }
            else
            {
                return new JsApi.DesignerGrade();
            }


        }


        /// <summary>
        /// 得到需求
        /// </summary>
        /// <returns></returns>
        public static JsApi.DemandShowRooms GetDemandShowRooms(string userid)
        {


            DataTable dt = SqlHelper.ExecuteDataTable(" select * from DemandShowRooms where UserId=@id", new SqlParameter("@id", userid));

            if (dt.Rows.Count > 0)
            {
                return ConvertToList<JsApi.DemandShowRooms>(dt)[0];
            }
            else
            {
                return new JsApi.DemandShowRooms();
            }


        }


        /// <summary>
        /// 得到预约
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static JsApi.Tentent GetTentent(string userid)
        {


            DataTable dt = SqlHelper.ExecuteDataTable(" select * from Tentent where   UserId=@id", new SqlParameter("@id", userid));

            if (dt.Rows.Count > 0)
            {
                return ConvertToList<JsApi.Tentent>(dt)[0];
            }
            else
            {
                return new JsApi.Tentent();
            }


        }

        /// <summary>
        /// 得到需求
        /// </summary>
        /// <returns></returns>
        public static JsApi.DemandShowRooms GetDemandShowRoomsext(string openid)
        {


            DataTable dt = SqlHelper.ExecuteDataTable(" select * from DemandShowRooms where UserId in (select UserId  from WebChartUser where openid=@id) ", new SqlParameter("@id", openid));

            if (dt.Rows.Count > 0)
            {
                return ConvertToList<JsApi.DemandShowRooms>(dt)[0];
            }
            else
            {
                return new JsApi.DemandShowRooms() { DemandShowroomId = 0 };
            }


        }




        /// <summary>
        /// 设计师项目
        /// </summary>
        /// <returns></returns>
        public static IList<JsApi.DesignWorks> Getdesingerworks(string id)
        {
            DataTable dt = SqlHelper.ExecuteDataTable("select * from DesignWorks where sjsId=@id;", new SqlParameter("@id", id));

            if (dt.Rows.Count > 0)
            {
                return ConvertToList<JsApi.DesignWorks>(dt);
            }
            else
            {
                return new List<JsApi.DesignWorks>();
            }

        }

        /// <summary>
        /// 设计师项目
        /// </summary>
        /// <returns></returns>
        public static IList<JsApi.DesignWorks> Getdesingerworksext(string id)
        {
            DataTable dt = SqlHelper.ExecuteDataTable("select * from DesignWorks where sjsZpId=@id;", new SqlParameter("@id", id));

            if (dt.Rows.Count > 0)
            {
                return ConvertToList<JsApi.DesignWorks>(dt);
            }
            else
            {
                return new List<JsApi.DesignWorks>();
            }

        }


        /// <summary>
        /// 设计师项目详细
        /// </summary>
        /// <returns></returns>
        public static IList<JsApi.DesignerWorksRoom> GetDesignerWorksRoom(string id)
        {
            DataTable dt = SqlHelper.ExecuteDataTable("select * from DesignerWorksRoom  where sjsWId=@id;", new SqlParameter("@id", id));

            if (dt.Rows.Count > 0)
            {
                return ConvertToList<JsApi.DesignerWorksRoom>(dt);
            }
            else
            {
                return new List<JsApi.DesignerWorksRoom>();
            }

        }

        public static string SendMsg(string phone)
        {


            //object o = SqlHelper.ExecuteScalar("select COUNT(*) from DesignerGrade where Extension5='" + phone + "' or mPhone='" + phone + "';");
            //if (Convert.ToInt32(o) < 1)
            //{
            #region 发送随机短信
            Random r = new Random();

            string s = r.Next(100000, 999999).ToString();

            Commen.DataCache.SetCache("yy" + phone, s);

            Commen.SendMsg.FSong(phone, "你好，您正在预约量房，您的验证码是：" + s + "");
            #endregion

            return "{\"errorcode\":\"0\",\"msg\":\"验证码发送成功\"}";
            //}
            //else
            //{
            //    return "{\"errorcode\":\"1\",\"msg\":\"此手机号已注册无需再次注册\"}";
            //}









        }

        /// <summary>
        /// 得到预约人数
        /// </summary>
        /// <returns></returns>
        public static string GetNum()
        {
            return SqlHelper.ExecuteScalar("select  COUNT(*) from Users").ToSafeString();
        }

        /// <summary>
        /// 得到openid
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static string GetAppid(string userid)
        {
            return SqlHelper.ExecuteScalar("select openid from WebChartUser where userid='" + userid + "'").ToSafeString();
        }


        /// <summary>
        /// 得到预约时间
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public string GetYYtime(string userid)
        {

            return "";
        }

        /// <summary>
        /// 得到抽奖列表
        /// </summary>
        /// <returns></returns>
        public static IList<JsApi.WinningRecord> GetWinningRecords()
        {
            DataTable dt = SqlHelper.ExecuteDataTable("  select top 150 * from  WinningRecord order by drawtime desc");

            IList<JsApi.WinningRecord> lis = ConvertToList<JsApi.WinningRecord>(dt);

            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "小不懂oο﹏", Prizename = "10元现金红包" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "萌主﹫", Prizename = "10元现金红包" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "じ☆ve﹍﹎格子伞", Prizename = "iWatch" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "Low-key丶是一种态度", Prizename = "iWatch" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "稚气未脱∩_∩", Prizename = "iWatch" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "▼魔方 小南", Prizename = "iPhone6" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "猫某君", Prizename = "iPhone6" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "twilight 暮年", Prizename = "iWatch" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "LK250520", Prizename = "iWatch" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "鱼人儿i", Prizename = "100元京东卡" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "小蘑菇", Prizename = "100元京东卡" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "萌面大侠i", Prizename = "iPhone6" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "素锦流年°", Prizename = "100元京东卡" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "糖豆微咸", Prizename = "100元京东卡" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "别摸头，会长不高耶！", Prizename = "100元京东卡" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "最萌分数差", Prizename = "100元京东卡" });



            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "10086520", Prizename = "10元现金红包" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "↙也許、這只是壹個巧合", Prizename = "10元现金红包" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "?俺是翠花", Prizename = "iWatch" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "狗比人忠", Prizename = "10元现金红包" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "没心没肺没脸没皮", Prizename = "iWatch" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "勿忘心安、", Prizename = "iPhone6" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "你最珍贵", Prizename = "10元现金红包" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "记忆幻化成了雨", Prizename = "2元现金红包" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "那一声感叹", Prizename = "iWatch" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "无言的结局", Prizename = "100元京东卡" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "单身少年", Prizename = "100元京东卡" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "貳歪骚年……i", Prizename = "iPhone6" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "x1n丶惘然___", Prizename = "2元现金红包" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "雪花ミ飞舞", Prizename = "100元京东卡" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "健平John", Prizename = "2元现金红包" });
            lis.Insert(new Random(Guid.NewGuid().GetHashCode()).Next(0, lis.Count()), new WinningRecord() { Nickname = "momo", Prizename = "10元现金红包" });

            return lis;
        }
        /// <summary> 
        /// 单表查询结果转换成泛型集合 
        /// </summary> 
        /// <typeparam name="T">泛型集合类型</typeparam> 
        /// <param name="dt">查询结果DataTable</param> 
        /// <returns>以实体类为元素的泛型集合</returns> 
        public static IList<T> ConvertToList<T>(DataTable dt) where T : new()
        {
            // 定义集合 
            List<T> ts = new List<T>();

            // 获得此模型的类型 
            Type type = typeof(T);
            //定义一个临时变量 
            string tempName = string.Empty;
            //遍历DataTable中所有的数据行  
            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                // 获得此模型的公共属性 
                PropertyInfo[] propertys = t.GetType().GetProperties();
                //遍历该对象的所有属性 
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;//将属性名称赋值给临时变量   
                    //检查DataTable是否包含此列（列名==对象的属性名）     
                    if (dt.Columns.Contains(tempName))
                    {
                        // 判断此属性是否有Setter   
                        if (!pi.CanWrite) continue;//该属性不可写，直接跳出   
                        //取值   
                        object value = dr[tempName];
                        //如果非空，则赋给对象的属性   
                        if (value != DBNull.Value)
                        {
                            //pi.SetValue(t, value, null);

                            if (!pi.PropertyType.IsGenericType)
                            {
                                //非泛型
                                pi.SetValue(t, string.IsNullOrEmpty(value.ToString()) ? null : Convert.ChangeType(value, pi.PropertyType), null);
                            }
                            else
                            {
                                //泛型Nullable<>
                                Type genericTypeDefinition = pi.PropertyType.GetGenericTypeDefinition();
                                if (genericTypeDefinition == typeof(Nullable<>))
                                {
                                    pi.SetValue(t, string.IsNullOrEmpty(value.ToString()) ? null : Convert.ChangeType(value, Nullable.GetUnderlyingType(pi.PropertyType)), null);
                                }
                            }
                        }
                    }
                }
                //对象添加到泛型集合中 
                ts.Add(t);
            }

            return ts;
        }

        /// <summary>
        /// 得到砍价列表
        /// </summary>
        /// <param name="foropenid"></param>
        /// <returns></returns>
        public static IList<JsApi.Bargain> GetBargainList(string foropenid)
        {

            DataTable dt = SqlHelper.ExecuteDataTable("  select * from Bargain where foruserid='" + foropenid + "' order by createtime desc;");

            IList<JsApi.Bargain> lis = ConvertToList<JsApi.Bargain>(dt);

            return lis;
        }

        /// <summary>
        /// 得到砍价总数
        /// </summary>
        /// <param name="foropenid"></param>
        /// <returns></returns>
        public static int GetTotleKjNum(string foropenid)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar("select SUM(value) from Bargain where foruserid='" + foropenid + "'"));
        }

        /// <summary>
        /// 得到砍价top10
        /// </summary>
        /// <param name="foropenid"></param>
        /// <returns></returns>
        public static IList<JsApi.Bargain> GetTop10()
        {

            DataTable dt = SqlHelper.ExecuteDataTable("   select top 7   WebChartUser.nickname,WebChartUser.headimgurl headimg, value from ( select MAX(openid) openid,MAX(nickname) nickname,MAX(headimg) headimg,MAX(foruserid) foruserid,SUM(value) value   from Bargain group by  foruserid)a left join WebChartUser on a.foruserid=WebChartUser.openid order by value desc;");

            IList<JsApi.Bargain> lis = ConvertToList<JsApi.Bargain>(dt);

            return lis;
        }
        /// <summary>
        /// 得到昵称
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static string GetnickName(string openid)
        {
            return SqlHelper.ExecuteScalar("select nickname from WebChartUser where openid='" + openid + "'").ToSafeString();

        }
    }
}
