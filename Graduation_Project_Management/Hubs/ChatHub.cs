using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;


using SystemTask = System.Threading.Tasks.Task;
using Domain.Entities;
using Domain.Repository;
using Repository.Identity;

namespace Graduation_Project_Management.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public ChatHub(ILogger<ChatHub> logger,
                      ApplicationDbContext context,
                      IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public override async SystemTask OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async SystemTask OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        public async SystemTask JoinTeamGroup(string userEmail)
        {
            try
            {
                var student = await _unitOfWork.GetRepository<Student>()
                    .GetAllAsync()
                    .FirstOrDefaultAsync(s => s.Email == userEmail);

                var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                    .GetAllAsync()
                    .FirstOrDefaultAsync(s => s.Email == userEmail);

                if (student != null)
                {
                    var team = await _unitOfWork.GetRepository<Team>()
                        .GetAllAsync()
                        .Include(t => t.TeamMembers)
                        .FirstOrDefaultAsync(t => t.TeamMembers.Any(m => m.Id == student.Id));

                    if (team != null)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"Team_{team.Id}");
                        await Clients.Group($"Team_{team.Id}")
                            .SendAsync("ReceiveSystemMessage", $"{student.FirstName} has joined the chat");
                        _logger.LogInformation($"{student.FirstName} joined Team_{team.Id}");
                    }
                }
                else if (supervisor != null)
                {
                    var team = await _unitOfWork.GetRepository<Team>()
                        .GetAllAsync()
                        .FirstOrDefaultAsync(t => t.SupervisorId == supervisor.Id);

                    if (team != null)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"Team_{team.Id}");
                        await Clients.Group($"Team_{team.Id}")
                            .SendAsync("ReceiveSystemMessage", $"{supervisor.FirstName} has joined the chat");
                        _logger.LogInformation($"{supervisor.FirstName} joined Team_{team.Id}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JoinTeamGroup");
                throw;
            }
        }

        public async SystemTask SendMessageToTeam(string userName, string userEmail, string message)
        {
            try
            {
                var student = await _unitOfWork.GetRepository<Student>()
                    .GetAllAsync()
                    .FirstOrDefaultAsync(s => s.Email == userEmail);

                var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                    .GetAllAsync()
                    .FirstOrDefaultAsync(s => s.Email == userEmail);

                int? teamId = null;

                if (student != null)
                {
                    var team = await _unitOfWork.GetRepository<Team>()
                        .GetAllAsync()
                        .Include(t => t.TeamMembers)
                        .FirstOrDefaultAsync(t => t.TeamMembers.Any(m => m.Id == student.Id));
                    teamId = team?.Id;
                }
                else if (supervisor != null)
                {
                    var team = await _unitOfWork.GetRepository<Team>()
                        .GetAllAsync()
                        .FirstOrDefaultAsync(t => t.SupervisorId == supervisor.Id);
                    teamId = team?.Id;
                }

                if (!teamId.HasValue)
                {
                    throw new HubException("Cannot find team for this user.");
                }

                await Clients.Group($"Team_{teamId}")
                    .SendAsync("ReceiveTeamMessage", userName, message, DateTime.Now.ToString("hh:mm tt"));

                var msg = new Message()
                {
                    SenderName = userName,
                    MessageText = message,
                    TeamId = teamId.Value,
                    SentAt = DateTime.UtcNow
                };

                await _context.Messages.AddAsync(msg);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessageToTeam");
                throw;
            }
        }
    }
}