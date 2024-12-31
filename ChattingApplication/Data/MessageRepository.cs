using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChattingApplication.DTOs;
using ChattingApplication.Entities;
using ChattingApplication.Helpers;
using ChattingApplication.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChattingApplication.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            this.mapper = mapper;
        }

        public async void AddGroup(Group group)
        {
             _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Message.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Message.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
           return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Message.FindAsync(id);
        }

        public async Task<Group> GetMessagegroup(string groupName)
        {
            return await _context.Groups.Include(c => c.Connections)
                .FirstOrDefaultAsync(n => n.Name == groupName);
        }

        public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Message.OrderByDescending(x => x.MessageSent).AsQueryable();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.UserName
                                       && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.UserName
                                       && u.SenderDeleted == false),
                _ => query.Where(u => u.RecipientUsername == messageParams.UserName &&
                                 u.DateRead == null && u.RecipientDeleted == false),
            };

            var messages = query.ProjectTo<MessageDTO>(this.mapper.ConfigurationProvider);

            return await PagedList<MessageDTO>
                         .CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);

        }

        public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            var query =  _context.Message
                           .Where(
                                  m => m.RecipientUsername == currentUserName &&
                                  m.RecipientDeleted == false &&
                                  m.SenderUsername == recipientUserName ||
                                  m.RecipientUsername == recipientUserName &&
                                  m.SenderDeleted == false &&
                                  m.SenderUsername == currentUserName)
                                  .OrderByDescending(m => m.MessageSent).AsQueryable();

            var unreadMessages = query.Where(m => m.DateRead == null &&
                                                m.RecipientUsername == currentUserName);

            if(unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }
            return await query.ProjectTo<MessageDTO>(mapper.ConfigurationProvider).ToListAsync();
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

   
    }
}
