#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wada.OrderDataBase.Models
{
    [Table("OwnCompanyHolidays")]
    internal class OwnCompanyHoliday
    {
        public OwnCompanyHoliday(DateTime holidayDate, bool legalHoliday)
        {
            HolidayDate = holidayDate;
            LegalHoliday = legalHoliday;
        }

        [Key, Required]
        public DateTime HolidayDate { get; set; }

        [Required]
        public bool LegalHoliday { get; set; }
    }
}
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
