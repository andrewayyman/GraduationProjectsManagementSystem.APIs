﻿using Graduation_Project_Management.DTOs.ProjectIdeasDTOs;
using Graduation_Project_Management.DTOs.TeamsDTOs;

namespace Graduation_Project_Management.DTOs.TeamsDtos
{
    public class GetTeamByStdIdDto
    {
        public string? StudentName { get; set; }
        public int TeamId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TeamDepartment { get; set; }
        public int MembersCount { get; set; }
        public List<string>? TechStack { get; set; }
        public List<TeamDtoWithRole>? TeamMembers { get; set; }
        public List<ProjectIdeaDto> ProjectIdeas { get; set; }

    }
}
