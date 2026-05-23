using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    public class BaseController : Controller
    {
        protected readonly AppDbContext _context;
        protected readonly IMapper _mapper;
        protected readonly IUserService _user;
        public BaseController(
            AppDbContext context,
            IMapper mapper,
            IUserService user
            )
        {
            _context = context;
            _mapper = mapper;
            _user = user;
        }
    }
}
