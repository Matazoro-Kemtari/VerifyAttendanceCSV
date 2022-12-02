using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Wada.OrderDataBase.Models;

namespace Wada.OrderDataBase
{
    internal class OrderDbContext : DbContext
    {
        private readonly OrderDbConfig orderDbConfig;

        public OrderDbContext(OrderDbConfig orderDbConfig)
        {
            this.orderDbConfig = orderDbConfig ?? throw new ArgumentNullException(nameof(orderDbConfig));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 接続文字列を作成する
            // NOTE: コンフィグに持つやり方も参考になる
            // https://csharp.sql55.com/database/how-to-use-transaction-scope.php
            var connectionString = new SqlConnectionStringBuilder
            {
                DataSource = orderDbConfig.Server,
                InitialCatalog = orderDbConfig.DataBase,
                UserID = orderDbConfig.User,
                Password = orderDbConfig.Password,

                // NOTE: SQLサーバーへのアクセスで証明書のエラーが出る場合の対処法
                // https://tech.tinybetter.com/Article/7b5d05c8-de00-2985-ebb7-3a00e1e23073/View
                TrustServerCertificate = true,
            }.ToString();
            optionsBuilder.UseSqlServer(connectionString);
        }

        internal DbSet<OwnCompanyHoliday>? OwnCompanyHolidays { get; set; }
    }

    /// <summary>
    /// データベース接続情報
    /// </summary>
    /// <param name="Server"></param>
    /// <param name="DataBase"></param>
    /// <param name="User"></param>
    /// <param name="Password"></param>
    internal record class OrderDbConfig
    {
        public OrderDbConfig(IConfiguration configuration)
        {
            Server = configuration.GetValue("DB_SERVER", string.Empty)!;
            DataBase = configuration.GetValue("DB_NAME", string.Empty)!;
            User = configuration.GetValue("DB_USER", string.Empty)!;
            Password = configuration.GetValue("DB_PASS", string.Empty)!;
        }

        public string Server { get; init; }
        public string DataBase { get; init; }
        public string User { get; init; }
        public string Password { get; init; }
    }
}