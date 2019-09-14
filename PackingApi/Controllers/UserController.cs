using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PackingApi.Models.DB;

namespace PackingApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private PackingDBContext db;
        public UserController(PackingDBContext packingDBContext)
        {
            this.db = packingDBContext;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<TbmUser>> Get()
        {
            var data = (from user in db.TbmUser
                        select user).ToList();
            return data;
        }

      
    }
}
