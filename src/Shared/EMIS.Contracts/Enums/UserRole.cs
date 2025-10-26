namespace EMIS.Contracts.Enums;

/// <summary>
/// User roles in the EMIS system
/// </summary>
public enum UserRole
{
    /// <summary>
    /// System administrator - full access
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Teacher - can manage classes, grades, assignments
    /// </summary>
    Teacher = 2,

    /// <summary>
    /// Parent - can view student information
    /// </summary>
    Parent = 3,

    /// <summary>
    /// Student - can view own information
    /// </summary>
    Student = 4,

    /// <summary>
    /// School administrator
    /// </summary>
    SchoolAdmin = 5,

    /// <summary>
    /// Academic staff
    /// </summary>
    AcademicStaff = 6
}
