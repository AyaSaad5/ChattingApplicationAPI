﻿using AutoMapper;
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
        public void AddMessage(Message message)
        {
            _context.Message.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Message.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Message.FindAsync(id);
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
            var messages = await _context.Message.
                           Include(u => u.Sender).ThenInclude(p => p.Photos)
                           .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                           .Where(
                                  m => m.RecipientUsername == currentUserName &&
                                  m.RecipientDeleted == false &&
                                  m.SenderUsername == recipientUserName ||
                                  m.RecipientUsername == recipientUserName &&
                                  m.SenderDeleted == false &&
                                  m.SenderUsername == currentUserName)
                                  .OrderByDescending(m => m.MessageSent).ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == null &&
                                                m.RecipientUsername == currentUserName);

            if(unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }
            return this.mapper.Map<IEnumerable<MessageDTO>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
