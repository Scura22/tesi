using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class SettingsController : Controller
    {

        private readonly RFFContext _context;

        public SettingsController(RFFContext context)
        {
            _context = context;
        }


        [HttpPost]
        [ActionName("GetNewHashCode")]
        public RetCode GetNewHashCode()
        {
            RetCode ret = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                ret.note = Globals.g_JsonConfiguration.GetSection("HashCode")["Hash"];

                return ret;
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex.Message);
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetTimeLogout")]
        public async Task<int> GetTimeLogout()
        {
            return Globals.g_TimeLogout;
        }

        [HttpPost]
        [ActionName("GetUserGroup")]
        public async Task<DizGruppiUtente[]> GetUserGroup([FromBody] bool isWeb)
        {
            try
            {
                 return await _context.DizGruppiUtente.Where(dg => dg.DgIsWeb == isWeb).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetUserGroup");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetAllSecurityItemByGroup")]
        public async Task<VSecurityGroupAll[]> GetAllSecurityItemByGroup([FromBody] VSecurityGroupAll param)
        {
            try
            {
                return await _context.VSecurityGroupAll.Where(x => x.VsgGroupName == param.VsgGroupName && x.VsgIsWeb == param.VsgIsWeb).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetAllSecurityItemByGroup");
                return null;
            }
        }


        [HttpPost]
        [ActionName("SaveSecurity")]
        public async Task<RetCode> SaveSecurity([FromBody] VSecurityGroupAll[] param)
        {
            RetCode ret = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                if (param[0].VsgIsWeb.Value)
                {
                    List<DizGruppiFunzionalita> authList = await _context.DizGruppiFunzionalita.Where(x => x.DgpIdGruppoUtente == param[0].VsgGroupId.Value).ToListAsync();
                    _context.DizGruppiFunzionalita.RemoveRange(authList);
                    _ = _context.SaveChanges();

                    authList.Clear();

                    param.ToList()
                        .Where(x => x.VsgEnabled.HasValue && x.VsgEnabled.Value).ToList()
                        .ForEach(x =>
                        {
                            authList.Add(
                            new DizGruppiFunzionalita()
                            {
                                DgpIdFunzionalita = Convert.ToByte(x.VsgObjId),
                                DgpIdGruppoUtente = x.VsgGroupId.Value
                            });
                        });

                    await _context.DizGruppiFunzionalita.AddRangeAsync(authList);
                    _ = await _context.SaveChangesAsync();
                }
                else
                {
                    List<DizGruppiComponentiSa> authList = await _context.DizGruppiComponentiSa.Where(x => x.DgcsIdGruppoUtente == param[0].VsgGroupId.Value).ToListAsync();
                    _context.DizGruppiComponentiSa.RemoveRange(authList);
                    _ = _context.SaveChanges();

                    authList.Clear();

                    param.ToList()
                        .Where(x => x.VsgEnabled.HasValue && x.VsgEnabled.Value).ToList()
                        .ForEach(x =>
                        {
                            authList.Add(
                            new DizGruppiComponentiSa()
                            {
                                DgcsIdComponenteSa = x.VsgObjId,
                                DgcsIdGruppoUtente = x.VsgGroupId.Value
                            });
                        });

                    await _context.DizGruppiComponentiSa.AddRangeAsync(authList);
                    _ = await _context.SaveChangesAsync();
                }



                return ret;
            }
            catch (Exception ex)
            {
                ret = new RetCode((int)Globals.RetCodes.NotOk, ex.Message);
                Globals.g_Logger.Error(ex, "SaveSecurity");
                return ret;
            }
        }

    }
}
