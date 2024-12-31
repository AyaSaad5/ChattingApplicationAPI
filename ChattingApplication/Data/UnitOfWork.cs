using AutoMapper;
using ChattingApplication.Interfaces;

namespace ChattingApplication.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext dataContext;
        private readonly IMapper mapper;

        public UnitOfWork(DataContext dataContext, IMapper mapper)
        {
            this.dataContext = dataContext;
            this.mapper = mapper;
        }
        public IUserRepository UserRepository =>  new UserRepository(this.dataContext, this.mapper);

        public IMessageRepository MessageRepository =>  new MessageRepository(this.dataContext, this.mapper);

        public ILikeRepository LikeRepository =>  new LikeRepository(this.dataContext);

        public async Task<bool> Complete()
        {
            return await this.dataContext.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return this.dataContext.ChangeTracker.HasChanges();
        }
    }
}
