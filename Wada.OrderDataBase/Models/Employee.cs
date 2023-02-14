using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wada.OrderDataBase.Models
{
    [Table("S社員")]
    internal class Employee
    {
        public Employee(int employeeNumber, string name, int? departmentID)
        {
            EmployeeNumber = employeeNumber;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DepartmentID = departmentID;
        }

        [Key, Required, Column("社員NO")]
        public int EmployeeNumber { get; set; }

        [Required, Column("氏名")]
        public string Name { get; set; }

        [Column("部署ID")]
        public int? DepartmentID { get; set; }
    }
}
