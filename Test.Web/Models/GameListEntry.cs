using QYQ.Base.Common.Xml;

namespace Test.Web.Models
{
    /// <summary>
    /// 游戏列表条目
    /// </summary>
    public class GameListEntry
    {
        /// <summary>
        /// 序号
        /// </summary>
        [XmlAttributeName("ID")] 
        public int Id { get; set; }

        /// <summary>
        /// 游戏名称
        /// </summary>
        [XmlAttributeName("游戏ID")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 游戏ID
        /// </summary>
        [XmlAttributeName("GameID")] 
        public int GameId { get; set; }

        /// <summary>
        /// 游戏类型
        /// </summary>
        [XmlAttributeName("GameType")] 
        public int GameType { get; set; }

        /// <summary>
        /// 所属地区
        /// </summary>
        [XmlAttributeName("DistrictID", Required = false)]
        public int? DistrictId { get; set; }
    }
}
