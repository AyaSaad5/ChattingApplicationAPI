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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MessagesController(IUnitOfWork unitOfWork,
                                  IMapper mapper)
        {

            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        [HttpPost]
        public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
        {
            var username = User.GetUserName();
            if (username == createMessageDTO.RecipientUsername.ToLower())
                return BadRequest("you cant sent messages to ur self");

            var sender =await  _unitOfWork.UserRepository.GetUserByUserNameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUserNameAsync(createMessageDTO.RecipientUsername);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDTO.Content
            };
            _unitOfWork.MessageRepository.AddMessage(message);

            if(await _unitOfWork.Complete())
                return Ok(_mapper.Map<MessageDTO>(message));
            return BadRequest("failed to send message");
        }
        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.UserName = User.GetUserName();

            var messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(messages.currentPage,
                                         messages.pageSize, messages.totalCount,
                                         messages.totalPages));

            return Ok(messages);
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(string username)
        {
            var currentUserName = User.GetUserName();

            return Ok(await _unitOfWork.MessageRepository.GetMessageThread(currentUserName, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserName();

            var message = await _unitOfWork.MessageRepository.GetMessage(id);

            if(message.SenderUsername !=  username && message.RecipientUsername != username)
                return Unauthorized();

            if(message.RecipientUsername == username) message.RecipientDeleted = true;
            if(message.SenderUsername == username) message.SenderDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
                _unitOfWork.MessageRepository.DeleteMessage(message);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Can't delete message");
        }
     }
}
