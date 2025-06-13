using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ENTITIES.ViewModels.Voucher
{
    public class VoucherFEModel
    {
        public int Id { get; set; }
        public string code { get; set; }
        public DateTime? cdate { get; set; }
        public DateTime? udate { get; set; }
        public DateTime? eDate { get; set; }
        public int limitUse { get; set; }
        public decimal? price_sales { get; set; }
        public string unit { get; set; }
        public string store_apply { get; set; }
        public int? rule_type { get; set; }
        public bool? IsPublic { get; set; }
        public string description { get; set; }
        public bool? is_limit_voucher { get; set; }
        public double? limit_total_discount { get; set; }
        public double? MinTotalAmount { get; set; }
        public int? campaign_id { get; set; }
        public short? project_type { get; set; }
        public int TotalRow { get; set; }
    }
}
