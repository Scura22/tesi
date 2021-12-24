using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RapportiLavoro.Global;
using RapportiLavoro.Models;
using RapportiLavoro.Models.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RapportiLavoro.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly RFFContext _context;

        public UserController(RFFContext context, ILoggerFactory logFactory, IConfiguration iConfiguration)
        {
            _logger = logFactory.CreateLogger<UserController>();
            _configuration = iConfiguration;
            _context = context;
        }

        //// GET: api/User/GetAll
        [HttpGet]
        [ActionName("GetAll")]
        public User[] GetAll()
        {
            User[] users = new User[0];


            return users;
        }

        //// GET: api/User/GetCurrentUser
        [HttpGet]
        [ActionName("GetCurrentUser")]
        public User GetCurrentUser()
        {
            return Globals.g_CurrentUser;
        }

        //// GET: api/User/GetUserById/iUserId
        [HttpGet("{id}")]
        [ActionName("GetUserById")]
        public User[] GetUserById(int iUserId)
        {
            User[] users = new User[0];


            return users;
        }

        [HttpPost]
        [ActionName("AuthenticateSupportUser")]
        public async Task<RetCode> AuthenticateSupportUser([FromBody] User param)
        {
            RetCode r = new RetCode((int)Globals.RetCodes.Ok, "");

            if (!Utilities.IsInitilized)
                Utilities.InitSecurity(_logger, _configuration);

            User supportUser = new User();
            supportUser.username = param.username;
            supportUser.password = param.password;
            supportUser.bypass = false;
            supportUser.domainDisplayName = "";
            supportUser.domainDescription = "";

            Globals.g_Logger.Info("Ricevuta richiesta autenticazione <" + param.username + ">");

            Globals.RetCodes ret = Globals.g_Security.CheckCredentials(supportUser, false);

            if (ret != Globals.RetCodes.Ok && ret != Globals.RetCodes.Ok_But_Expired)
            {
                supportUser = null;
                Globals.g_Logger.Info("Controllo credenziali fallito");
                r.retCode = (int)Globals.RetCodes.NotOk;
                r.note = "Controllo credenziali fallito";
            }

            return r;

        }







        //// GET: api/User/AuthenticateUser
        [HttpPost]
        [ActionName("AuthenticateUser")]
        public async Task<User> AuthenticateUser([FromBody] User uUser)
        {
            if (!Utilities.IsInitilized)
                Utilities.InitSecurity(_logger, _configuration);


            Globals.g_CurrentUser = new User();
            Globals.g_CurrentUser.username = uUser.username;
            Globals.g_CurrentUser.password = uUser.password;
            Globals.g_CurrentUser.bypass = false;
            Globals.g_CurrentUser.domainDisplayName = "";
            Globals.g_CurrentUser.domainDescription = "";
            //Globals.g_CurrentUser.pswExpired = false;

            Globals.g_Logger.Info("Ricevuta richiesta autenticazione <" + uUser.username + ">");

            Globals.RetCodes ret = Globals.g_Security.CheckCredentials(Globals.g_CurrentUser, false);

            if (ret != Globals.RetCodes.Ok && ret != Globals.RetCodes.Ok_But_Expired)
            {
                Globals.g_CurrentUser = null;
                Globals.g_Logger.Info("Controllo credenziali fallito");
            }
            else
            {
                Globals.g_Logger.Info("Login effettuato, recupero parametri utente");

                if (_configuration.GetSection("SecurityInfo")["SecurityManager"] == "MACHINE")
                {
                    Globals.g_Logger.Info("#### Controllo se presente in DB per Emplyee Shift ####");

                    Globals.g_CurrentUser.username = uUser.username;

                }
                
            }

            return Globals.g_CurrentUser;
        }

        //// GET: api/User/AuthenticateUser
        [HttpPost]
        [ActionName("AuthenticateUserBypass")]
        public async Task<User> AuthenticateUserBypass([FromBody] User uUser)
        {
            _logger.LogInformation("AUTHENTICATEUSERBYPASS --> Utente bypass: <" + uUser.username + ">");

            if (!Utilities.IsInitilized)
                Utilities.InitSecurity(_logger, _configuration);

            Globals.g_CurrentUser = new User();

            /*if (_configuration.GetSection("SecurityInfo")["SecurityManager"] == "DOMAIN")
                Globals.g_CurrentUser.id = await _context.NtbEmployee.Where(x => x.EGlobalId == uUser.username).Select(x => x.EId).FirstOrDefaultAsync();
            else
                Globals.g_CurrentUser.id = 1;*/

            Globals.g_CurrentUser.username = uUser.username;
            Globals.g_CurrentUser.password = "";
            Globals.g_CurrentUser.bypass = uUser.bypass;
            Globals.g_CurrentUser.domainDisplayName = "";
            Globals.g_CurrentUser.domainDescription = "";

            if (Globals.g_Security.CheckCredentials(Globals.g_CurrentUser, uUser.bypass) != Globals.RetCodes.Ok)
            {
                Globals.g_CurrentUser = null;
            }

            return Globals.g_CurrentUser;
        }


        //// GET: api/User/AuthenticateUser
        [HttpPost]
        [ActionName("GetLogoutTime")]
        public async Task<int> GetLogoutTime([FromBody] string domainGroup)
        {
            int ret = 10;
            //ret = await _context.NtbDicDomainGroup.Where(g => g.DdgName.Trim() == domainGroup.Trim()).Select(g => g.DdgTimeLogout).FirstOrDefaultAsync();

            return ret;
        }

        [HttpPost]
        [ActionName("GetMenuByUserGroup")]
        public async Task<DizFunzionalita[]> GetMenuByUserGroup([FromBody] string[] eGroups)
        {
            try
            {
                eGroups.ToList().ForEach(x => { x = x.ToUpper(); });

                DizFunzionalita[] allLinks = await _context.DizFunzionalita.Join(_context.DizGruppiFunzionalita,
                            funzionalita => funzionalita.DfuId,
                            gruppiFunzionalita => gruppiFunzionalita.DgpIdFunzionalita,
                            (funzionalita, gruppiFunzionalita) => new { DizFunzionalitum = funzionalita, DizGruppiFunzionalita = gruppiFunzionalita }).Join(_context.DizGruppiUtente,
                            gf => gf.DizGruppiFunzionalita.DgpIdGruppoUtente,
                            gruppoUtente => gruppoUtente.DgId,
                            (gf, gruppoUtente) => new { gf = gf, DizGruppiUtente = gruppoUtente })
                            .Where(m => eGroups.ToList().Contains(m.DizGruppiUtente.DgNome.ToUpper()) && m.gf.DizFunzionalitum.DfuLink != null)
                            .OrderBy(menu => menu.gf.DizFunzionalitum.DfuIndice)
                            .Select(menu => menu.gf.DizFunzionalitum)
                            .ToArrayAsync();

                List<DizFunzionalita> functLinks = new List<DizFunzionalita>();

                allLinks.ToList().ForEach(link =>
                {
                    if (functLinks.Find(k => k.DfuId == link.DfuId) == null)
                        functLinks.Add(link);
                });

                return functLinks.ToArray();
            }
            catch (Exception e)
            {
                List<DizFunzionalita> functLinks = new List<DizFunzionalita>();
                return functLinks.ToArray();
            }
        }

        [HttpPost]
        [ActionName("LoadAllFunctByUserGroup")]
        public async Task<DizFunzionalita[]> LoadAllFunctByUserGroup([FromBody] string group)
        {
            group = group.Trim();
            DizFunzionalita[] dm = await _context.DizFunzionalita.Join(_context.DizGruppiFunzionalita,
                                funzionalita => funzionalita.DfuId,
                                gruppiFunzionalita => gruppiFunzionalita.DgpIdFunzionalita,
                                (funzionalita, gruppiFunzionalita) => new { DizFunzionalitum = funzionalita, DizGruppiFunzionalita = gruppiFunzionalita }).Join(_context.DizGruppiUtente,
                                gf => gf.DizGruppiFunzionalita.DgpIdGruppoUtente,
                                gruppoUtente => gruppoUtente.DgId,
                                (gf, gruppoUtente) => new { gf = gf, DizGruppiUtente = gruppoUtente })
                                .Where(m => group == m.DizGruppiUtente.DgNome.ToUpper() && m.gf.DizFunzionalitum.DfuLink == null)
                                .Select(menu => menu.gf.DizFunzionalitum)
                                .ToArrayAsync();

            return dm;
        }

        [HttpPost]
        [ActionName("GetIsMachine")]
        public async Task<bool> GetIsMachine()
        {
            return Globals.g_isMachine;
        }
    }

}