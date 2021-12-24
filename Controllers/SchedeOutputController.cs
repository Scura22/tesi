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
    public class SchedeOutputController : Controller
    {
        private readonly RFFContext _context;

        public SchedeOutputController(RFFContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ActionName("AddNewScheda")]
        public async Task<RetCode> AddNewScheda([FromBody] SchedeOutput so)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                _context.SchedeOutput.Add(so);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("CloseScheda")]
        public async Task<RetCode> CloseScheda([FromBody] VSchedeOutput vso)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            SchedeOutput s;

            try
            {
                s = _context.SchedeOutput.Where(so => so.SoId == vso.VsoId).First();
                s.SoSchedaPronta = DateTime.UtcNow;
                s.SoStato = (int)Global.Globals.StatoOutput.ATTESA_SPEDIZIONE;
                s.SoOperatoreSchedaPronta = vso.VsoOperatoreSchedaPronta;
                _context.SchedeOutput.Update(s);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("AddNCamion")]
        public async Task<RetCode> AddNCamion([FromBody] VSchedeOutput vso)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            SchedeOutput s;

            try
            {
                s = _context.SchedeOutput.Where(so => so.SoId == vso.VsoId).First();
                s.SoSchedaSpedita = DateTime.UtcNow;
                s.SoStato = (int)Global.Globals.StatoOutput.SPEDITO;
                s.SoNumeroCamion = vso.VsoNumeroCamion;
                s.SoOperatoreSchedaSpedita = vso.VsoOperatoreSchedaSpedita;
                _context.SchedeOutput.Update(s);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }
         
        [HttpPost]
        [ActionName("AddNDdt")]
        public async Task<RetCode> AddNDdt([FromBody] VSchedeOutput vso)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            SchedeOutput s;

            try
            {
                s = _context.SchedeOutput.Where(so => so.SoId == vso.VsoId).First();
                s.SoNumeroDdt = vso.VsoNumeroDdt;
                _context.SchedeOutput.Update(s);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("GetNumeroSchedeByCassone")]
        public async Task<int> GetNumeroSchedeByCassone([FromBody] int cassone)
        {

            int numeroSchede;
            numeroSchede = await _context.SchedeOutput.CountAsync(so => so.SoCassone == cassone && so.SoStato == 0);
            return numeroSchede;
        }

        [HttpPost]
        [ActionName("GetSchedaAttivaByCassone")]
        public async Task<SchedeOutput> GetSchedaAttivaByCassone([FromBody] int cassone)
        {
            SchedeOutput scheda;
            scheda = await _context.SchedeOutput.Where(so => so.SoCassone == cassone && so.SoStato == 0).FirstOrDefaultAsync();
            return scheda;
        }

        [HttpPost]
        [ActionName("GetSchedeBaseChiuse")]
        public async Task<VSchedeOutput[]> GetSchedeBaseChiuse()
        {

            List<VSchedeOutput> schedeOutput = new List<VSchedeOutput>();
            schedeOutput = await _context.VSchedeOutput.Where(so => so.VsoTipoScheda == (int)Global.Globals.SchedaType.BASE && so.VsoChiusa).ToListAsync();
            return schedeOutput.ToArray();
        }

        [HttpPost]
        [ActionName("GetSchedeBaseChiuseByDate")]
        public async Task<VSchedeOutput[]> GetSchedeBaseChiuseByDate([FromBody] Filter f)
        {
            List<VSchedeOutput> schedeOutput = new List<VSchedeOutput>();
            schedeOutput = await _context.VSchedeOutput.Where(so => so.VsoTipoScheda == (int)Global.Globals.SchedaType.BASE && so.VsoChiusa && so.VsoAperturaScheda >= f.StartDate && so.VsoAperturaScheda <= f.EndDate).ToListAsync();
            return schedeOutput.ToArray();
        }

        [HttpPost]
        [ActionName("GetSchedeBaseAperte")]
        public async Task<VSchedeOutput[]> GetSchedeBaseAperte()
        {

            List<VSchedeOutput> schedeOutput = new List<VSchedeOutput>();
            schedeOutput = await _context.VSchedeOutput.Where(so => so.VsoTipoScheda == (int)Global.Globals.SchedaType.BASE && !so.VsoChiusa).ToListAsync();
            return schedeOutput.ToArray();
        }

        [HttpPost]
        [ActionName("GetSchedeSpecificheChiuse")]
        public async Task<VSchedeOutput[]> GetSchedeSpecificheChiuse()
        {

            List<VSchedeOutput> schedeOutput = new List<VSchedeOutput>();
            schedeOutput = await _context.VSchedeOutput.Where(so => so.VsoTipoScheda == (int)Global.Globals.SchedaType.SPECIFICA && so.VsoChiusa).ToListAsync();
            return schedeOutput.ToArray();
        }

        [HttpPost]
        [ActionName("GetSchedeSpecificheChiuseByDate")]
        public async Task<VSchedeOutput[]> GetSchedeSpecificheChiuseByDate([FromBody] Filter f)
        {
            List<VSchedeOutput> schedeOutput = new List<VSchedeOutput>();
            schedeOutput = await _context.VSchedeOutput.Where(so => so.VsoTipoScheda == (int)Global.Globals.SchedaType.SPECIFICA && so.VsoChiusa && so.VsoAperturaScheda >= f.StartDate && so.VsoAperturaScheda <= f.EndDate).ToListAsync();
            return schedeOutput.ToArray();
        }

        [HttpPost]
        [ActionName("GetSchedeSpecificheAperte")]
        public async Task<VSchedeOutput[]> GetSchedeSpecificheAperte()
        {

            List<VSchedeOutput> schedeOutput = new List<VSchedeOutput>();
            schedeOutput = await _context.VSchedeOutput.Where(so => so.VsoTipoScheda == (int)Global.Globals.SchedaType.SPECIFICA && !so.VsoChiusa).ToListAsync();
            return schedeOutput.ToArray();
        }

        [HttpPost]
        [ActionName("GetSchedaById")]
        public async Task<VSchedeOutput[]> GetSchedaById([FromBody] int nScheda)
        {

            List<VSchedeOutput> schedeOutput = new List<VSchedeOutput>();
            schedeOutput = await _context.VSchedeOutput.Where(so => so.VsoChiusa && so.VsoId == nScheda).ToListAsync();
            return schedeOutput.ToArray();
        }

        [HttpPost]
        [ActionName("GetSchedeEditByTipoScheda")]
        public async Task<VSchedeOutput[]> GetSchedeEditByTipoScheda([FromBody] VSchedeOutput vso)
        {

            List<VSchedeOutput> schedeOutput = new List<VSchedeOutput>();
            schedeOutput = await _context.VSchedeOutput.Where(so => so.VsoTipoScheda == vso.VsoTipoScheda && (so.VsoIdStato == 0 || so.VsoIdStato == 2) && so.VsoId != vso.VsoId).ToListAsync();
            return schedeOutput.ToArray();
        }

    }
}
