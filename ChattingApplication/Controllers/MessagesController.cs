using AutoMapper;
using ChattingApplication.DTOs;
using ChattingApplication.Entities;
using ChattingApplication.Extensions;
using ChattingApplication.Helpers;
using ChattingApplication.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ChattingApplication.Controllers
{
   
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessagesController(IUserRepository userRepository,
                                  IMessageRepository messageRepository,
                                  IMapper mapper)
        {
            _userRepository = userRepository;
           _messageRepository = messageRepository;
            _mapper = mapper;
        }
        [HttpPost]
        public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
        {
            var username = User.GetUserName();
            if (username == createMessageDTO.RecipientUsername.ToLower())
                return BadRequest("you cant sent messages to ur self");

            var sender =await  _userRepository.GetUserByUserNameAsync(username);
            var recipient = await _userRepository.GetUserByUserNameAsync(createMessageDTO.RecipientUsername);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDTO.Content
            };
            _messageRepository.AddMessage(message);

            if(await _messageRepository.SaveAllAsync())
                return Ok(_mapper.Map<MessageDTO>(message));
            return BadRequest("failed to send message");
        }
        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.UserName = User.GetUserName();

            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(messages.currentPage,
                                         messages.pageSize, messages.totalCount,
                                         messages.totalPages));

            return Ok(messages);
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(string username)
        {
            var currentUserName = User.GetUserName();

            return Ok(await _messageRepository.GetMessageThread(currentUserName, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserName();

            var message = await _messageRepository.GetMessage(id);

            if(message.SenderUsername !=  username && message.RecipientUsername != username)
                return Unauthorized();

            if(message.RecipientUsername == username) message.RecipientDeleted = true;
            if(message.SenderUsername == username) message.SenderDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
                _messageRepository.DeleteMessage(message);

            if (await _messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Can't delete message");
        }
     }
}
