using ChattingApplication.DTOs;
using ChattingApplication.Entities;
using ChattingApplication.Helpers;

namespace ChattingApplication.Interfaces
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessage(int id);
        Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams);
        Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName);

        void AddGroup(Group group);
        void RemoveConnection(Connection connection);

        Task<Connection> GetConnection(string connectionId);
        Task<Group> GetMessagegroup(string groupName);
        Task<bool> SaveAllAsync();
    }
}
