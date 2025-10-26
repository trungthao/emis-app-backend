using Teacher.Domain.Common;

namespace Teacher.Domain.Entities
{
    public class Teacher : BaseEntity
    {
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string FullName => $"{LastName} {FirstName}";
        public DateTime? DateOfBirth { get; private set; }
        public string? Phone { get; private set; }
        public string? Email { get; private set; }
        public string? Address { get; private set; }
        public string? AvatarUrl { get; private set; }
        public TeacherStatus Status { get; private set; }

        private Teacher() { }

        public Teacher(string firstName, string lastName, string? phone = null)
        {
            FirstName = firstName;
            LastName = lastName;
            Phone = phone ?? string.Empty;
            Status = TeacherStatus.Active;
        }

        public void UpdateBasicInfo(string firstName, string lastName, DateTime? dateOfBirth, string? phone, string? email, string? address)
        {
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            Phone = phone;
            Email = email;
            Address = address;
        }

        public void Deactivate() => Status = TeacherStatus.Inactive;
        public void Activate() => Status = TeacherStatus.Active;
    }

    public enum TeacherStatus
    {
        Active = 1,
        Inactive = 2
    }
}
