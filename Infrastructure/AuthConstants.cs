namespace LoanApp.Infrastructure
{
    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string Employee = "Employee";
    }

    public static class SessionKeys
    {
        public const string EmployeeId = "CurrentEmployeeId";
        public const string EmployeeName = "CurrentEmployeeName";
        public const string EmployeeRole = "CurrentEmployeeRole";
    }
}