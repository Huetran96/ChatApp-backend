using chat_server.data;
using chat_server.DTOs;
using chat_server.Helpers;
using chat_server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace chat_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;

        public UserController(AppDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _config = config;
        }

        [HttpPost]
        [Route("init-admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] RegisterDto request)
        {
            var admin = new IdentityRole(Role.ADMIN);
            var user = new IdentityRole(Role.USER);

            if (!await _roleManager.RoleExistsAsync(Role.ADMIN))
            {
                await _roleManager.CreateAsync(admin);
            }
            if (!await _roleManager.RoleExistsAsync(Role.USER))
            {
                await _roleManager.CreateAsync(user);
            }
            var admins = await _userManager.GetUsersInRoleAsync(Role.ADMIN);
            if (admins.Count > 0)
            {
                return BadRequest("Admin is existed.");
            }
            var newAdmin = new User()
            {
                UserName = request.UserName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
            };
            var result = await _userManager.CreateAsync(newAdmin, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest("Something error");
            }
            await _userManager.AddToRoleAsync(newAdmin, Role.ADMIN);
            var profile = new Profile() { UserId = newAdmin.Id };
            await _context.Profiles.AddAsync(profile);
            await _context.SaveChangesAsync();
            return Ok("Create Admin successed");
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            /*Regex regexEmail = new Regex(Pattern.EMAIL_REGEX);
            Regex regexPhone = new Regex(Pattern.PHONE_REGEX);
            if (!(regexEmail.IsMatch(request.Email) && regexPhone.IsMatch(request.PhoneNumber))){
                return BadRequest("Số điện thoại hoặc Email chưa đúng định dạng, bạn hãy kiểm tra lại.");
            }*/
            var isEmail = await _userManager.FindByEmailAsync(request.Email);
            var isPhoneNumber = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
            if (isEmail != null)
            {
                return BadRequest("Email nãy đã được sử dụng, bạn hãy kiểm tra lại");
            }
            if (isPhoneNumber != null)
            {
                return BadRequest("Số điện thoại này đã được sử dụng,bạn hãy kiểm tra lại");
            }
            var newUser = new User()
            {
                UserName = request.UserName,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email

            };
            var result = await _userManager.CreateAsync(newUser, request.Password);


            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.ToList());
            }
            await _userManager.AddToRoleAsync(newUser, Role.USER);

            var profile = new Profile() { UserId = newUser.Id };
            await _context.Profiles.AddAsync(profile);
            await _context.SaveChangesAsync();
            return Ok("Register successed");

        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest("User not exist");
            }
            var result = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!result)
            {
                return BadRequest("Password's not correct. Try again!");
            }
            var token = await GenerateToken(user);

            return Ok(token);
        }

        [Authorize]
        [HttpGet]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            string token = "";
            string authoriztion = Request.Headers.Authorization;
            if(authoriztion != null) {
                if (authoriztion.StartsWith("Bearer "))
                {
                    token = authoriztion.Substring(7).Trim();
                }
            }
            
            await _signInManager.SignOutAsync();

            TokenLogout tokenLogout = new TokenLogout();

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
            {
                return BadRequest(" Token khong hop le");
            }
            var jwtToken = handler.ReadJwtToken(token);
            tokenLogout.Id = jwtToken.Claims.FirstOrDefault(c => c.Type == "JWTID").Value;
            var expClaim = jwtToken.Claims.FirstOrDefault(e => e.Type == "exp");
            if (expClaim == null)
            {
                return BadRequest(" Co loi");
            }
            long exp = long.Parse(expClaim.Value);
            DateTime expDate = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
            tokenLogout.expireDate = expDate;

            await _context.TokenLogouts.AddAsync(tokenLogout);
            await _context.SaveChangesAsync();

            return Ok(tokenLogout);
        }

        [Authorize(Roles = Role.ADMIN)]
        [HttpGet]
        [Route("user-list")]
        public async Task<IActionResult> GetAllUser([FromQuery] QueryObject request)
        {
            
            ListUserDto listUserDto = new ListUserDto();

            var allUser = await  _userManager.Users.ToListAsync();
            if (request.Keyword != null)
            {
                allUser = allUser.Where(u => u.UserName.Contains(request.Keyword) || u.Email.Contains(request.Keyword) || u.PhoneNumber.Contains(request.Keyword)).ToList();

            }
            int mod = allUser.Count % request.PageSize;
            if (mod == 0)
            {
                listUserDto.totalPage = allUser.ToList().Count / request.PageSize;
            }
            else
            {
                listUserDto.totalPage = (allUser.ToList().Count / request.PageSize) + 1;
            }
            if(request.Page > listUserDto.totalPage)
            {
                request.Page = listUserDto.totalPage;
            }

            int skip = (request.Page - 1) * request.PageSize;
            listUserDto.users = allUser.Skip(skip).Take(request.PageSize).ToList();

            return Ok(listUserDto);
        }


        [Authorize]
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeleteUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByIdAsync(userId);
            await _userManager.DeleteAsync(user);
            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("my-profile")]
        public async Task<IActionResult> MyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
            return Ok(profile);

        }
        [Authorize]
        [HttpGet]
        [Route("friend-profile/{id}")]
        public async Task<IActionResult> GetProfile([FromRoute] string id)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => (p.UserId) == id);
            if (profile == null)
            {
                return BadRequest("Không tìm thấy");
            }
            return Ok(profile);

        }
        [Authorize]
        [HttpPut]
        [Route("my-profile/update")]
        public async Task<IActionResult> UpdateProp([FromBody] UpdateDto request)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                return BadRequest("không tìm thấy thông tin.");
            }

            PropertyInfo[] props = profile.GetType().GetProperties();
            var isProp = props.FirstOrDefault(p => p.Name == request.prop);
            if (isProp == null)
            {
                return BadRequest("Dữ liệu không hợp lệ, Bạn hãy thử lại.");
            }

            foreach (var item in props)
            {
                if (request.prop == item.Name)
                {
                    if (item.Name == "Id" || item.Name == "UserId")
                    {
                        return BadRequest("Không được phép sửa dữ liệu này.");
                    }
                    else
                    {
                        item.SetValue(profile, request.value);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return Ok(profile);
        }
        [Authorize]
        [HttpGet]
        [Route("account")]
        public async Task<IActionResult> GetAccount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByIdAsync(userId);
            return Ok(user);
        }

        [Authorize]
        [HttpPut]
        [Route("account/update")]
        public async Task<IActionResult> UpdateAccount([FromBody] UpdateUserDto request)
        {
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByIdAsync(userId);
            var isPassCorrect = await _userManager.CheckPasswordAsync(user, request.password);
            if (!isPassCorrect)
            {
                return BadRequest("Mật khẩu sai");
            }

            if(request.prop == "Email")
            {
                Regex regex = new Regex(Pattern.EMAIL_REGEX);
                if (!regex.IsMatch(request.value))
                {
                    return BadRequest("Email chưa đúng định dạng.");
                }
                if(await _userManager.FindByEmailAsync(request.value)  != null)
                {
                    return BadRequest("Email đã được sử dụng ");
                }
                user.Email = request.value;
                await _userManager.UpdateAsync(user);
                return Ok(user);
                

            }
            if (request.prop == "PhoneNumber")
            {
                Regex regex = new Regex(Pattern.PHONE_REGEX);
                if (!regex.IsMatch(request.value))
                {
                    return BadRequest("Số điện thoại chưa đúng định dạng.");
                }
                var isExist = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.value);
                if (isExist != null)
                {
                    return BadRequest("Số điện thoại đã được sử dụng");
                }
                user.PhoneNumber = request.value;
                await _userManager.UpdateAsync(user);
                return Ok(user);
            }
            return BadRequest("Dữ liệu không hợp lệ, vui lòng kiểm tra lại.");
        }

        [HttpGet]
        [Route("verified-token")]
        public async Task<IActionResult> VerifiedToken()
        {
            string token = "";

            string authoriztion = Request.Headers.Authorization;
            if (authoriztion.StartsWith("Bearer "))
            {
                token = authoriztion.Substring(7).Trim();
            }
            TokenReponseDto tokenReponseDto = new TokenReponseDto();

            /*var tokenValidateParam = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SigningKey"]))
            };
            //check 1: AccessToken valid format
            var tokenInVerification = handler.ValidateToken(token, tokenValidateParam, out var validatedToken);*/

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
            {
                tokenReponseDto.isValid = false;
                tokenReponseDto.Message = "Invalid token";
                return BadRequest(tokenReponseDto);
            }
            var jwtToken = handler.ReadJwtToken(token);
            tokenReponseDto.Id = jwtToken.Claims.FirstOrDefault(c => c.Type == "JWTID").Value;
            var isTokenLogout = await _context.TokenLogouts.FirstOrDefaultAsync(t => t.Id == tokenReponseDto.Id);
            if (isTokenLogout != null)
            {
                tokenReponseDto.isValid = false;
                tokenReponseDto.Message = "Token logout.";

                return BadRequest(tokenReponseDto);


            }
            tokenReponseDto.Token = token;
            tokenReponseDto.UserId = jwtToken.Claims.FirstOrDefault(e => e.Type == ClaimTypes.NameIdentifier).Value;
            tokenReponseDto.UserName = jwtToken.Claims.FirstOrDefault(u => u.Type == "Sub").Value;
            var roles = jwtToken.Claims.Where( r => r.Type == ClaimTypes.Role);
            tokenReponseDto.Roles = roles.Select(r => r.Value).ToArray();
            

            var expClaim = jwtToken.Claims.FirstOrDefault(e => e.Type == "exp");
            if (expClaim == null)
            {
                tokenReponseDto.Message = "Co loi";
            }
            long exp = long.Parse(expClaim.Value);
            DateTime expDate = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
            tokenReponseDto.exp = expDate;
            if (expDate > DateTime.UtcNow)
            {
                tokenReponseDto.isValid = true;
                tokenReponseDto.Message = "Valid token";
                return Ok(tokenReponseDto);

            }
            tokenReponseDto.Message = "het han";
            return Ok(tokenReponseDto);
        }

        [Authorize]
        [HttpGet]
        [Route("create-trial")]
        public async Task<IActionResult> CreateTrialAccount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByIdAsync(userId);

            var account = new AccountDto();
            account.Name = "TRIAL";
            account.expireDate = DateTime.Now.AddMinutes(5);

            var claims = await _userManager.GetClaimsAsync(user);
            var isTrial = claims.Any(c => c.Type == account.Name);
            if (isTrial)
            {
                return BadRequest("Bạn đã sử dụng bản TRIAL. Hãy nâng cấp tài khoản PRO để sử dụng dịch vụ");
            }       

            await _userManager.AddClaimAsync(user, new Claim(account.Name, account.expireDate.ToString()));
            var token = GenerateToken(user);
            return Ok(token.Result);

        }
        [Authorize]
        [HttpGet]
        [Route("create-pro")]
        public async Task<IActionResult> CreateProAccount()
        {
            var account = new AccountDto();
            account.Name = "PRO";
            account.expireDate = DateTime.UtcNow.AddDays(30);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByIdAsync(userId);
            var claims = await _userManager.GetClaimsAsync(user);
            var pro = claims.FirstOrDefault(c => c.Type == account.Name);
            if(pro != null)
            {
                await _userManager.RemoveClaimAsync(user, pro);
            }
            
            await _userManager.AddClaimAsync(user, new Claim(account.Name, account.expireDate.ToString()));
            var token = GenerateToken(user);
            return Ok(token.Result);

        }
        [Authorize(Roles = "ADMIN,TRIAL,PRO")]
        [HttpGet]
        [Route("game-center")]
        public async Task<IActionResult> GameCenter()
        {
            ResponeGameDto responeGameDto = new ResponeGameDto();
            responeGameDto.isValid = false;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByIdAsync(userId);

            var claims = await _userManager.GetClaimsAsync(user);
            var admin = claims.Any(c => c.Type == "ADMIN");
            if(admin )
            {
                responeGameDto.isValid = true;
                return Ok(responeGameDto);
            }
            var trial = claims.FirstOrDefault(c => c.Type == "TRIAL");            
            
            if(trial != null)
            {
                responeGameDto.exp = DateTime.Parse(trial.Value);
                var isTrialValid = responeGameDto.exp  > DateTime.Now;
                if(isTrialValid )
                {
                    responeGameDto.isValid = true;

                    return Ok(responeGameDto);
                }
            }
            var pro = claims.FirstOrDefault(c => c.Type == "PRO");
            if (pro != null)
            {
                responeGameDto.exp = DateTime.Parse(pro.Value);
                var isProValid = responeGameDto.exp  > DateTime.Now;
                if(isProValid )
                {
                    responeGameDto.isValid = true;
                    return Ok(responeGameDto);
                }
            }

            return Ok(responeGameDto);
        }
        private async Task<IActionResult> GenerateToken(User user)
        {

            var roles = await _userManager.GetRolesAsync(user);
            var claimType = await _userManager.GetClaimsAsync(user);
            var claims = new List<Claim>
            {
                new Claim (ClaimTypes.NameIdentifier, user.Id),
                new Claim("Sub", user.UserName),
                new Claim("JWTID", Guid.NewGuid().ToString())
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            if (claimType != null)
            {
                foreach (var claim in claimType)
                {
                    claims.Add(new Claim(ClaimTypes.Role, claim.Type));
                }
            }
            

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SigningKey"]));
            var tokenObject = new JwtSecurityToken(
                issuer: _config["JWT:ValidIssuer"],
                audience: _config["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(24),
                claims: claims,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );
            string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);

            return Ok(token);

        }
    }
}
