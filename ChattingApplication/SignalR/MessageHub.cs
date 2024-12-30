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
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IHubContext<PrecenseHub> _precenseHub;

        public MessageHub(IMessageRepository messageRepository,
                 IUserRepository userRepository,
                 IMapper mapper,
                 IHubContext<PrecenseHub> precenseHub)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
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

            var message = await _messageRepository.GetMessageThread(Context.User.GetUserName(), otherUsers);
              
            await Clients.Group(groupName).SendAsync("RecieveMessageThread", message);
        }

        public async Task SendMessage(CreateMessageDTO createMessageDTO)
        {
            var username = Context.User.GetUserName();
            if (username == createMessageDTO.RecipientUsername.ToLower())
                throw new HubException("you cant sent messages to ur self");

            var sender = await _userRepository.GetUserByUserNameAsync(username);
            var recipient = await _userRepository.GetUserByUserNameAsync(createMessageDTO.RecipientUsername);

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

            var group = await _messageRepository.GetMessagegroup(groupName);

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
            _messageRepository.AddMessage(message);

            if (await _messageRepository.SaveAllAsync())
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
            var group = await _messageRepository.GetMessagegroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());

            if(group == null)
            {
                group = new Group(groupName);
                _messageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);

            return await _messageRepository.SaveAllAsync();
        }

        private async Task RemoveFromMessageGroup()
        {
            var connection = await _messageRepository.GetConnection(Context.ConnectionId);
            _messageRepository.RemoveConnection(connection);

            await _messageRepository.SaveAllAsync();
        }
    }
}
