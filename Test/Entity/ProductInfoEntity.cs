using SqlSugar;
using System.Security.Principal;

namespace Test.Entity
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("ProductInfo")]
    public class ProductInfoEntity
    {
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }


        /// <summary>
        /// Desc:大厅
        /// Default:
        /// Nullable:False
        /// </summary>

        public int Lobby { get; set; }


        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>

        public string ProductName { get; set; } = string.Empty;


        /// <summary>
        /// Desc:支付类型: 0.华为 1.谷歌 2.苹果 3.Tron 4.YooPay
        /// Default:
        /// Nullable:True
        /// </summary>

        public string PayCategory { get; set; } = string.Empty;


        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>

        public string ProductId { get; set; } = string.Empty;


        /// <summary>
        /// Desc:价格
        /// Default:
        /// Nullable:False
        /// </summary>

        public decimal Price { get; set; }


        /// <summary>
        /// Desc:币种
        /// Default:
        /// Nullable:False
        /// </summary>

        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// 支付的地区 
        /// IN（印度）、ID（印度尼西亚）、BR（巴西）等
        /// </summary>
        public string Country { get; set; } = string.Empty;


        /// <summary>
        /// Desc:其他平台Id
        /// Default:
        /// Nullable:True
        /// </summary>

        public string OtherId { get; set; } = string.Empty;


        /// <summary>
        /// Desc:创建时间
        /// Default:
        /// Nullable:True
        /// </summary>

        public DateTime? CreateTime { get; set; }


        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>

        public string Ext { get; set; } = string.Empty;

        /// <summary>
        /// 状态
        /// </summary>
        public bool Staus { get; set; }

        /// <summary>
        /// 限购
        /// </summary>
        public int Limit { get; set; }
    }
}
