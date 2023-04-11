namespace Wada.AttendanceTableService.MatchedEmployeeNumberAggregation
{
    [Equals(DoNotAddEqualityOperators = true), ToString]
    public class MatchedEmployeeNumber
    {
        private MatchedEmployeeNumber(uint employeeNumber, uint attendancePersonalCode)
        {
            EmployeeNumber = employeeNumber;
            AttendancePersonalCode = attendancePersonalCode;
        }

        public static MatchedEmployeeNumber Reconstruct(uint employeeNumber, uint attendancePersonalCode) => new(employeeNumber, attendancePersonalCode);

        /// <summary>
        /// 社員番号
        /// </summary>
        public uint EmployeeNumber { get; init; }

        /// <summary>
        /// 勤怠個人コード
        /// </summary>
        public uint AttendancePersonalCode { get; init; }
    }
}
