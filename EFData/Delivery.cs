//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace EFData
{
    using System;
    using System.Collections.Generic;
    
    public partial class Delivery
    {
        public int id { get; set; }
        public Nullable<int> SahreId { get; set; }
        public string Openid { get; set; }
        public string Nickname { get; set; }
        public string ZipCode { get; set; }
        public string Name { get; set; }
        public string Deliveryaddress { get; set; }
        public string Mobile { get; set; }
        public string Phone { get; set; }
        public string Mark { get; set; }
        public Nullable<System.DateTime> Createtime { get; set; }
        public Nullable<int> IsSend { get; set; }
        public Nullable<System.DateTime> SendTime { get; set; }
    }
}
