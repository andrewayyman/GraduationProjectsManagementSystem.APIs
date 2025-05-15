namespace Graduation_Project_Management.DTOs.ProjectIdeasDTOs
{
    public class HandleIdeaRequestDto
    {
        public int RequestId { get; set; }
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }

    }
}