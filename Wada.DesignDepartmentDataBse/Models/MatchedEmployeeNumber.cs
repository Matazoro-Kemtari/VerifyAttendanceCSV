using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wada.DesignDepartmentDataBse.Models
{
    [Table("MatchedEmployeeNumbers")]
    internal class MatchedEmployeeNumber
    {
        internal MatchedEmployeeNumber(int employeeNumbers, int attendancePersonalCode)
        {
            EmployeeNumbers = employeeNumbers;
            AttendancePersonalCode = attendancePersonalCode;
        }

        [Key, Required]
        public int EmployeeNumbers { get; set; }
        
        [Required]
        public int AttendancePersonalCode { get; set; }
    }
}
