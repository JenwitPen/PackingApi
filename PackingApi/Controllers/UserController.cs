using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PackingApi.Models.DB;
using PackingApi.Models.Requests;

namespace PackingApi.Controllers
{

    [Route("api/[controller]/[action]")]
    [EnableCors("CorsPolicy")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private PackingDBContext db;
        public UserController(PackingDBContext packingDBContext)
        {
            this.db = packingDBContext;
        }
        // GET api/values
        [HttpPost]
        public async Task<ActionResult> selectUser([FromBody] selectUserRequest selectUserRequest)
        {
            try
            {
                var response = await (from user in db.TbmUser
                                      where user.UserName== selectUserRequest.UserName.Trim() &&
                                      user.Password== selectUserRequest.Password.Trim() &&
                                      user.Active==true
                                      select new { user.UserId,user.UserName}).FirstOrDefaultAsync();

                if (response!=null)
                {
                    return Ok(response);
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {

                return StatusCode(500, ex);
            }

        }
    }
}
