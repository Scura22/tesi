using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class OutputColataController : Controller
    {
        private readonly RFFContext _context;

        public OutputColataController(RFFContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ActionName("AddOutputBase")]
        public async Task<RetCode> AddOutputBase([FromBody] OutputColataBase ocb)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                await _context.OutputColataBase.AddAsync(ocb);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("UpdateQtyBase")]
        public async Task<RetCode> UpdateQtyBase([FromBody] VOutputColataBase vocb)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                OutputColataBase oc;

                oc = await _context.OutputColataBase.Where(ocb => ocb.OcbId == vocb.VocId).FirstOrDefaultAsync();
                oc.OcbQuantitaOutput = vocb.VocQuantitaOutput;
                oc.OcbScorifica = vocb.VocScorifica;
                _context.OutputColataBase.Update(oc);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("MoveOutputBase")]
        public async Task<RetCode> MoveOutputBase([FromBody] VOutputColataBase vocb)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                OutputColataBase oc;

                oc = await _context.OutputColataBase.Where(ocb => ocb.OcbId == vocb.VocId).FirstOrDefaultAsync();
                oc.OcbIdScheda = vocb.VocIdScheda;
                _context.OutputColataBase.Update(oc);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }


        [HttpPost]
        [ActionName("AddOutputSpecifica")]
        public async Task<RetCode> AddOutputSpecifica([FromBody] OutputColataSpecifica ocs)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                await _context.OutputColataSpecifica.AddAsync(ocs);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("UpdateQtySpecifica")]
        public async Task<RetCode> UpdateQtySpecifica([FromBody] VOutputColataSpecifica vocs)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                OutputColataSpecifica oc = await _context.OutputColataSpecifica.Where(ocs => ocs.OcsId == vocs.VocId).FirstOrDefaultAsync();
                oc.OcsQuantitaOutput = vocs.VocQuantitaOutput;
                oc.OcsScorifica = vocs.VocScorifica;
                _context.OutputColataSpecifica.Update(oc);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("MoveOutputSpecifica")]
        public async Task<RetCode> MoveOutputSpecifica([FromBody] VOutputColataSpecifica vocs)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                OutputColataSpecifica oc = await _context.OutputColataSpecifica.Where(ocs => ocs.OcsId == vocs.VocId).FirstOrDefaultAsync();
                oc.OcsIdScheda = vocs.VocIdScheda;
                _context.OutputColataSpecifica.Update(oc);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("GetOutputColataBaseByScheda")]
        public async Task<VOutputColataBase[]> GetOutputColataBaseByScheda([FromBody] VSchedeOutput so)
        {

            List<VOutputColataBase> outputColataBase = new List<VOutputColataBase>();
            outputColataBase = await _context.VOutputColataBase.Where(ocb => ocb.VocIdScheda == so.VsoId).ToListAsync();
            return outputColataBase.ToArray();
        }

        [HttpPost]
        [ActionName("GetOutputColataSpecificaByScheda")]
        public async Task<VOutputColataSpecifica[]> GetOutputColataSpecificaByScheda([FromBody] VSchedeOutput so)
        {

            List<VOutputColataSpecifica> outputColataSpecifica = new List<VOutputColataSpecifica>();
            outputColataSpecifica = await _context.VOutputColataSpecifica.Where(ocs => ocs.VocIdScheda == so.VsoId).ToListAsync();
            return outputColataSpecifica.ToArray();
        }

    }
}
