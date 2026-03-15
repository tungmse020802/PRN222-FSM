namespace StudentManagementSystem.Shared.Entities;

public class SubjectPrerequisite
{
    public int SubjectId { get; set; }

    public int PrerequisiteSubjectId { get; set; }

    public Subject? Subject { get; set; }

    public Subject? PrerequisiteSubject { get; set; }
}
