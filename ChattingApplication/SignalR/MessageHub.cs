using AutoMapper;
using ChattingApplication.Data;
using ChattingApplication.DTOs;
using ChattingApplication.Entities;
using ChattingApplication.Extensions;
using ChattingApplication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChattingApplication.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<PrecenseHub> _precenseHub;

        public MessageHub(IUnitOfWork unitOfWork,
                  
                 IMapper mapper,
                 IHubContext<PrecenseHub> precenseHub)
        {
             
             
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _precenseHub = precenseHub;
        }
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Connection established: {Context.ConnectionId}");

            var httpContext = Context.GetHttpContext();
            var otherUsers = httpContext.Request.Query["user"];

            var groupName = GetGroupName(Context.User.GetUserName(), otherUsers);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await AddToGroup(groupName);

            var message = await _unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUserName(), otherUsers);
              
            await Clients.Group(groupName).SendAsync("RecieveMessageThread", message);
        }

        public async Task SendMessage(CreateMessageDTO createMessageDTO)
        {
            var username = Context.User.GetUserName();
            if (username == createMessageDTO.RecipientUsername.ToLower())
                throw new HubException("you cant sent messages to ur self");

            var sender = await _unitOfWork.UserRepository.GetUserByUserNameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUserNameAsync(createMessageDTO.RecipientUsername);

            if (recipient == null) throw new HubException("user not found");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDTO.Content
            };
            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            var group = await _unitOfWork.MessageRepository.GetMessagegroup(groupName);

            if(group.Connections.Any(v => v.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PrecenseTracker.GetConnectionsForUser(recipient.UserName);
                if(connections != null)
                {
                    await _precenseHub.Clients.Clients(connections).SendAsync(
                        "NewMessageRecieved", new
                        {
                            username = recipient.UserName,
                            knownAs = sender.KnownAs

                        });
                }
            }
            _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("newMessage", _mapper.Map<MessageDTO>(message));
            }
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await RemoveFromMessageGroup();
            await base.OnDisconnectedAsync(exception);
        }
        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
        private async Task<bool> AddToGroup(string groupName)
        {
            var group = await _unitOfWork.MessageRepository.GetMessagegroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());

            if(group == null)
            {
                group = new Group(groupName);
                _unitOfWork.MessageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);

            return await _unitOfWork.Complete();
        }

        private async Task RemoveFromMessageGroup()
        {
            var connection = await _unitOfWork.MessageRepository.GetConnection(Context.ConnectionId);
            _unitOfWork.MessageRepository.RemoveConnection(connection);

            await _unitOfWork.Complete();
        }
    }
}
