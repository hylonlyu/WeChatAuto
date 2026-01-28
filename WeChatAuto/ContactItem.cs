using System;

namespace WeChatAuto
{
    /// <summary>
    /// 联系人项目类，包含姓名和金额信息
    /// </summary>
    public class ContactItem
    {
        /// <summary>
        /// 联系人姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">姓名</param>
        /// <param name="amount">金额，默认为10</param>
        public ContactItem(string name, decimal amount = 10)
        {
            Name = name ?? string.Empty;
            Amount = amount > 0 ? amount : 10; // 确保金额大于0
        }

        /// <summary>
        /// 重写ToString方法，用于显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name} - {Amount}";
        }
    }
}